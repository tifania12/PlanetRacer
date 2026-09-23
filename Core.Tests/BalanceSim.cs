using System;
using System.Collections.Generic;
using GemRacer.Core;

// L-05: "상호 강화 나선이라 성장이 가파를 수 있다. 체감 효과를 어디에 넣을지 봇 시뮬레이션으로
// 확인"(docs/design/core-loop.md "아직 안 정한 것"). 테스트가 아니라 리포트라서 평소
// `dotnet run`(인자 없음)에는 안 끼고 `dotnet run -- sim`으로만 돈다 — CLAUDE.md 2번 규칙
// ("dotnet run이 실패 0으로 끝나야 한다")과 부딪히지 않게 하려는 것. Program.cs와 같은 스타일로
// namespace 없이 둔다(이 프로젝트의 테스트 러너 파일들은 전역 네임스페이스에 있다).
//
// 2026-09-18: M-02(제련소 분리) 이후로 쭉 미뤄져 있던 수정. tempo.md 4절이 "M-02 이전 것이라
// 원석으로 사던 시절 그대로다 — 쓰기 전에 고쳐야 한다"고 못 박아 둔 그 부채를 갚는다.
// 실제 게임(MiningController.Update)은 화폐가 둘이다 — 원석(RawMinerals, 화물칸 상한에 걸림)과
// 정제 광물(RefinedMinerals, 제련소가 원석을 정제한 것, 화물칸을 안 차지함). 제련소만 원석으로
// 사고(IsPaidWithRawMinerals) 나머지 셋(곡괭이/화물칸/엔진)은 정제 광물로 산다. 옛 시뮬레이션은
// 이 갈림길 자체가 없어서 제련소를 단 한 번도 안 사고 곧장 정제 광물처럼 원석을 나머지 셋에
// 부었다 — 지금 게임에서는 있을 수 없는 진행이다(제련소 0레벨이면 정제 광물이 영원히 0).
//
// 전략: 표준 방치형 봇. 원석이 쌓이면 먼저 제련소가 살 수 있는 한 산다(원석 화물칸 상한과
// 경쟁하는 자원이라 묵혀 둘 이유가 없다 — RigUpgrade.cs 주석의 "원석 → 제련소 → 정제 광물 →
// 나머지 업그레이드" 순서 그대로). 정제 광물이 쌓이면 Tool/Cargo/Engine 중 지금 제일 싼 것을
// 산다(D05-N UpgradeCost). raceIntervalHours마다 로컬 레이스를 한 판 이겼다고 치고
// RigPartReward(무료 +1 레벨, Tool/Cargo/Engine만 — 제련소는 레이스 보상에 없다)를 하나 준다.
//
// 보고 싶은 것: 레벨이 오를수록 "다음 업그레이드까지 걸리는 시간"이 완만히 늘어나는가
// (원하는 그림 — 자연스러운 체감 효과), 아니면 그대로거나 줄어드는가(터짐 — 상한 없이
// 폭주해서 몇 시간 안에 다 살 수 있게 됨). 제련소 0레벨 구간(정제 광물이 아예 안 나오는 동안)이
// 얼마나 긴지도 이번에 처음으로 실측한다 — 옛 시뮬레이션은 그 구간 자체를 몰랐다.
//
// 2026-09-23 야간 세션: 로컬 레이스 승리마다 확정 RigPartReward 말고도 녹슨 상자가 하나 같이
// 나온다(RaceBoxReward.ForTier(Local) = Rusty, MiningController.cs 864~870행)는 걸 이 시뮬레이션이
// 여태 안 반영하고 있었다 — 그래서 상자가 30% 확률로 주는 증폭기(Amplifier)를 이 시뮬레이션이
// 한 번도 안 뽑아 봤다. `docs/decisions.md`의 "증폭기 누적 상한을 어떻게 정할지" 판단 대기 항목이
// 바로 이 수치가 있어야 답할 수 있는 질문이라(상한 없이 두면 하루에 얼마나 쌓이는지), 상자
// 개봉을 끼워 넣고 네 슬롯이 다 찬 뒤로도(SimHorizonHours까지) 계속 돌려 누적치를 찍는다.
static class BalanceSim
{
    const float RaceIntervalHours = 0.5f; // 로컬 레이스 한 판(연출+대기 포함) 대략 30분 가정. 플레이스홀더.
    const float SimHorizonHours = 24f;    // 네 슬롯이 2~3시간이면 다 차므로, 증폭기 누적 추이를 보려면
                                           // 그보다 훨씬 뒤(하루치)까지 계속 돌려야 한다.
    const float StepHours = 0.05f;        // 3분 단위 적분 — MiningRunState처럼 프레임 단위는 아니지만 충분히 촘촘하다.
    static readonly float[] AmplifierCheckpointHours = { 0.5f, 1f, 2f, 4f, 8f, 12f, 24f };

    public static void Run()
    {
        var planet = DefaultData.Planets()[0]; // 쿼츠 — P1 프로토타입 범위와 맞춘다
        var raceRewards = DefaultData.QuartzLocalRaceRewards();
        var rng = new DeterministicRandom(2026);

        var rig = new MiningRig();
        var raw = 0f;
        var refined = 0f;
        var hours = 0f;
        var lastUpgradeHour = 0f;
        var nextRaceHour = RaceIntervalHours;
        var totalRefineryUpgrades = 0;
        var totalOtherUpgrades = 0;
        var totalRaceWins = 0;
        float? refineryMaxedAtHour = null;
        float? firstRefinedUpgradeHour = null;
        float? allMaxedAtHour = null;
        var log = new List<string>();

        // 상자 쪽 누적 — 등급 순서는 PartGrade(C/B/A/S)와 같다(Amplifier.cs 관례 그대로).
        var boxesOpened = 0;
        var boxRigPartCount = 0;
        var boxMineralTotal = 0f;
        var amplifierCounts = new int[4];
        var amplifierWorstCaseSum = 0f; // 한 칸에 전부 몰아 끼웠을 때(가장 빨리 상한에 닿는 경우)
        var checkpointIndex = 0;
        var checkpointLog = new List<string>();

        while (hours < SimHorizonHours)
        {
            // MiningController.Update와 같은 순서 — 캔 원석을 먼저 더하고, 그 값 기준으로 정제하고,
            // 남는 원석만 화물칸 상한으로 자른다(정제 광물은 상한 밖이라 안 잘린다).
            var minedThisTick = MiningSimulator.MineralsPerHour(rig, planet) * StepHours;
            var rawAfterMining = raw + minedThisTick;
            var refinedNow = MiningSimulator.Refine(rawAfterMining, rig, planet, StepHours * 3600f);
            var cap = MiningSimulator.CargoCapacityMinerals(rig, planet);
            raw = Math.Min(rawAfterMining - refinedNow, cap);
            refined += refinedNow;
            hours += StepHours;

            if (hours >= nextRaceHour)
            {
                var reward = raceRewards[rng.NextInt(0, raceRewards.Count)];
                rig = RigPartApply.Apply(rig, reward);
                nextRaceHour += RaceIntervalHours;
                totalRaceWins++;

                // GDD "로컬=녹슨" — 확정 슬롯 보상과 별개로 녹슨 상자 하나(RaceBoxReward.ForTier).
                // 네 시드 모두 같은 rng에서 순서대로 뽑는다 — 서로 다른 뽑기가 시드를 공유하면 안
                // 된다는 LootBoxOpener.OpenAny 주석 그대로, 매번 새 값을 네 번 뽑아 흘려보낸다.
                var box = LootBoxOpener.OpenAny(LootBoxType.Rusty,
                    gradeSeed: rng.NextInt(1, int.MaxValue), kindSeed: rng.NextInt(1, int.MaxValue),
                    slotSeed: rng.NextInt(1, int.MaxValue), mineralSeed: rng.NextInt(1, int.MaxValue),
                    openedSincePity: 0); // 녹슨 상자는 천장이 없어(LootTable.Rusty) 카운터가 의미 없다
                boxesOpened++;
                switch (box.Kind)
                {
                    case LootRewardKind.RigPart:
                        rig = RigPartApply.Apply(rig, box.Reward);
                        boxRigPartCount++;
                        break;
                    case LootRewardKind.Amplifier:
                        var a = box.AmplifierBonus!.Value;
                        amplifierCounts[(int)a.Grade]++;
                        amplifierWorstCaseSum += a.Bonus;
                        break;
                    case LootRewardKind.Minerals:
                        // MiningController.Update와 같은 화물칸 상한 취급 — 원석과 같은 자원이라 다음
                        // 틱에 raw가 다시 cap으로 잘릴 것이므로 여기서는 더하기만 한다.
                        raw += box.Minerals!.Value.Amount;
                        boxMineralTotal += box.Minerals.Value.Amount;
                        break;
                }
            }

            var (r, o) = BuyCheapestUntilBroke(ref rig, ref raw, ref refined, hours, ref lastUpgradeHour, log);
            totalRefineryUpgrades += r;
            totalOtherUpgrades += o;
            if (r > 0 && UpgradeCost.AtMax(UpgradeSlot.Refinery, rig) && refineryMaxedAtHour == null)
                refineryMaxedAtHour = hours;
            if (o > 0 && firstRefinedUpgradeHour == null)
                firstRefinedUpgradeHour = hours;
            if (allMaxedAtHour == null && AllMaxed(rig))
                allMaxedAtHour = hours;

            while (checkpointIndex < AmplifierCheckpointHours.Length && hours >= AmplifierCheckpointHours[checkpointIndex])
            {
                checkpointLog.Add($"  [{AmplifierCheckpointHours[checkpointIndex],5:F1}h] 누적 증폭률 합(한 칸 몰빵 가정) " +
                    $"{amplifierWorstCaseSum * 100f,6:F0}%  (증폭기 {amplifierCounts[0] + amplifierCounts[1] + amplifierCounts[2] + amplifierCounts[3]}개 — " +
                    $"C {amplifierCounts[0]} B {amplifierCounts[1]} A {amplifierCounts[2]} S {amplifierCounts[3]})");
                checkpointIndex++;
            }
        }

        Console.WriteLine($"=== L-05 밸런스 봇 시뮬레이션 (쿼츠, 레이스 {RaceIntervalHours}h마다 승리 가정) ===");
        Console.WriteLine($"네 슬롯 전부 최대 도달: " + (allMaxedAtHour == null
            ? $"없음({SimHorizonHours:F0}시간 안에 못 채웠다 — 성장이 너무 느릴 수 있다)"
            : $"{allMaxedAtHour:F2}시간, 제련소 업그레이드 {totalRefineryUpgrades}회 + 나머지 {totalOtherUpgrades}회" +
              $" (그중 레이스 무료 보상 {totalRaceWins}회는 별도, Tool/Cargo/Engine에만 붙음)"));
        Console.WriteLine($"최종 레벨 — Tool {rig.ToolLevel}/{UpgradeCost.ToolMaxLevel}, " +
            $"Cargo {rig.CargoLevel}/{UpgradeCost.CargoMaxLevel}, Engine {rig.EngineLevel}/{UpgradeCost.EngineMaxLevel}, " +
            $"Refinery {rig.RefineryLevel}/{UpgradeCost.RefineryMaxLevel}");
        Console.WriteLine($"첫 정제 광물 구매(=제련소가 처음으로 뭔가를 빨리 돌린 시점): " +
            (firstRefinedUpgradeHour == null ? "없음(끝까지 정제 광물로 아무것도 못 삼)" : $"{firstRefinedUpgradeHour:F2}시간"));
        Console.WriteLine($"제련소 5레벨(정제 100%) 도달: " +
            (refineryMaxedAtHour == null ? "없음" : $"{refineryMaxedAtHour:F2}시간"));
        Console.WriteLine();
        Console.WriteLine("구매 로그 (업그레이드 시각·간격) — 간격이 뒤로 갈수록 완만히 늘어나야 건강하다:");
        foreach (var line in log) Console.WriteLine(line);

        Console.WriteLine();
        Console.WriteLine("(간격 추세는 위 로그를 앞/뒤로 눈으로 비교해서 판단할 것 — " +
            "뒤로 갈수록 간격이 점점 벌어지면 체감 효과가 자연스럽게 생긴 것이고, " +
            "그대로거나 짧아지면 비용 곡선을 더 가파르게 잡아야 한다.)");

        Console.WriteLine();
        Console.WriteLine($"=== 녹슨 상자 {boxesOpened}개 개봉 결과 — 증폭기 누적 상한 판단용(decisions.md 판단 대기 목차) ===");
        Console.WriteLine($"부품 {boxRigPartCount}개 · 광물 합계 {boxMineralTotal:F0} · 증폭기 " +
            $"{amplifierCounts[0] + amplifierCounts[1] + amplifierCounts[2] + amplifierCounts[3]}개" +
            $"(C {amplifierCounts[0]} B {amplifierCounts[1]} A {amplifierCounts[2]} S {amplifierCounts[3]})");
        Console.WriteLine("체크포인트별 누적 — '한 칸 몰빵'은 상한이 없는 지금 규칙 그대로 뽑힌 증폭기를" +
            " 전부 한 슬롯에 넣었다고 가정한 값이다(제일 빨리 폭주하는 경우, 상한 논의의 상한선 역할):");
        foreach (var line in checkpointLog) Console.WriteLine(line);
    }

    static bool AllMaxed(MiningRig rig) =>
        UpgradeCost.AtMax(UpgradeSlot.Tool, rig) && UpgradeCost.AtMax(UpgradeSlot.Cargo, rig) &&
        UpgradeCost.AtMax(UpgradeSlot.Engine, rig) && UpgradeCost.AtMax(UpgradeSlot.Refinery, rig);

    /// <summary>한 스텝(StepHours) 안에서 살 수 있는 만큼 몰아 산다(초반엔 한 스텝에 여러 번
    /// 살 수도 있다). 제련소는 원석(raw)으로, 나머지 셋은 정제 광물(refined)로 산다 —
    /// UpgradeCost.IsPaidWithRawMinerals와 같은 갈림길. 돌려주는 값은 (제련소 구매 수, 나머지 구매 수).</summary>
    static (int refineryBought, int otherBought) BuyCheapestUntilBroke(
        ref MiningRig rig, ref float raw, ref float refined, float hours, ref float lastUpgradeHour, List<string> log)
    {
        var refineryBought = 0;
        var otherBought = 0;
        while (true)
        {
            var boughtSomething = false;

            // 제련소 먼저 — 원석은 화물칸 상한에 묶인 자원이라 놀리는 것보다 항상 태우는 게 이득이다.
            if (!UpgradeCost.AtMax(UpgradeSlot.Refinery, rig))
            {
                var cost = UpgradeCost.Cost(UpgradeSlot.Refinery, rig);
                if (cost <= raw)
                {
                    raw -= cost;
                    var beforeLevel = rig.RefineryLevel;
                    rig = UpgradeCost.Apply(UpgradeSlot.Refinery, rig);
                    refineryBought++;
                    boughtSomething = true;
                    LogPurchase(log, hours, ref lastUpgradeHour, "Refinery", beforeLevel, cost, "원석");
                }
            }

            // 나머지 셋 중 지금 제일 싼 것 — 정제 광물로 산다.
            UpgradeSlot? cheapest = null;
            var cheapestCost = float.PositiveInfinity;
            foreach (var slot in new[] { UpgradeSlot.Tool, UpgradeSlot.Cargo, UpgradeSlot.Engine })
            {
                var cost = UpgradeCost.Cost(slot, rig);
                if (cost < cheapestCost) { cheapestCost = cost; cheapest = slot; }
            }
            if (cheapest != null && cheapestCost <= refined)
            {
                refined -= cheapestCost;
                var beforeLevel = UpgradeCost.CurrentLevel(cheapest.Value, rig);
                rig = UpgradeCost.Apply(cheapest.Value, rig);
                otherBought++;
                boughtSomething = true;
                LogPurchase(log, hours, ref lastUpgradeHour, cheapest.Value.ToString(), beforeLevel, cheapestCost, "정제");
            }

            if (!boughtSomething) return (refineryBought, otherBought);
        }
    }

    static void LogPurchase(List<string> log, float hours, ref float lastUpgradeHour, string slotName,
        int beforeLevel, float cost, string currency)
    {
        var gap = hours - lastUpgradeHour;
        log.Add($"  [{hours,6:F2}h] {slotName,-8} Lv.{beforeLevel,2}→{beforeLevel + 1,2}  비용 {cost,7:F1}{currency}  간격 {gap,6:F2}h");
        lastUpgradeHour = hours;
    }
}
