using System;
using System.Collections.Generic;
using GemRacer.Core;

// L-05: "상호 강화 나선이라 성장이 가파를 수 있다. 체감 효과를 어디에 넣을지 봇 시뮬레이션으로
// 확인"(docs/design/core-loop.md "아직 안 정한 것"). 테스트가 아니라 리포트라서 평소
// `dotnet run`(인자 없음)에는 안 끼고 `dotnet run -- sim`으로만 돈다 — CLAUDE.md 2번 규칙
// ("dotnet run이 실패 0으로 끝나야 한다")과 부딪히지 않게 하려는 것. Program.cs와 같은 스타일로
// namespace 없이 둔다(이 프로젝트의 테스트 러너 파일들은 전역 네임스페이스에 있다).
//
// 전략: 표준 방치형 봇 — 원석이 쌓이는 대로 Tool/Cargo/Engine 중 지금 제일 싸게 살 수 있는
// 걸 산다(D05-N UpgradeCost). raceIntervalHours마다 로컬 레이스를 한 판 이겼다고 치고
// RigPartReward(무료 +1 레벨)를 하나 준다 — 코어 루프의 "레이싱이 채굴을 먹인다" 절반.
//
// 보고 싶은 것: 레벨이 오를수록 "다음 업그레이드까지 걸리는 시간"이 완만히 늘어나는가
// (원하는 그림 — 자연스러운 체감 효과), 아니면 그대로거나 줄어드는가(터짐 — 상한 없이
// 폭주해서 몇 시간 안에 다 살 수 있게 됨).
static class BalanceSim
{
    const float RaceIntervalHours = 0.5f; // 로컬 레이스 한 판(연출+대기 포함) 대략 30분 가정. 플레이스홀더.
    const float MaxHours = 300f;          // 12.5일. 이 안에 세 슬롯이 다 안 찬다면 그 자체가 발견.
    const float StepHours = 0.05f;        // 3분 단위 적분 — MiningRunState처럼 프레임 단위는 아니지만 충분히 촘촘하다.

    public static void Run()
    {
        var planet = DefaultData.Planets()[0]; // 쿼츠 — P1 프로토타입 범위와 맞춘다
        var raceRewards = DefaultData.QuartzLocalRaceRewards();
        var rng = new DeterministicRandom(2026);

        var rig = new MiningRig();
        var minerals = 0f;
        var hours = 0f;
        var lastUpgradeHour = 0f;
        var nextRaceHour = RaceIntervalHours;
        var totalUpgrades = 0;
        var totalRaceWins = 0;
        var log = new List<string>();

        while (hours < MaxHours && !AllMaxed(rig))
        {
            minerals += MiningSimulator.MineralsPerHour(rig, planet) * StepHours;
            hours += StepHours;

            if (hours >= nextRaceHour)
            {
                var reward = raceRewards[rng.NextInt(0, raceRewards.Count)];
                rig = RigPartApply.Apply(rig, reward);
                nextRaceHour += RaceIntervalHours;
                totalRaceWins++;
            }

            totalUpgrades += BuyCheapestUntilBroke(ref rig, ref minerals, hours, ref lastUpgradeHour, log);
        }

        Console.WriteLine($"=== L-05 밸런스 봇 시뮬레이션 (쿼츠, 레이스 {RaceIntervalHours}h마다 승리 가정) ===");
        Console.WriteLine($"종료: {hours:F1}시간 경과, 업그레이드 {totalUpgrades}회 (그중 레이스 무료 보상 {totalRaceWins}회는 별도)");
        Console.WriteLine($"최종 레벨 — Tool {rig.ToolLevel}/{UpgradeCost.ToolMaxLevel}, " +
            $"Cargo {rig.CargoLevel}/{UpgradeCost.CargoMaxLevel}, Engine {rig.EngineLevel}/{UpgradeCost.EngineMaxLevel}");
        if (!AllMaxed(rig)) Console.WriteLine($"** {MaxHours}시간 안에 다 못 채웠다 — 성장이 너무 느릴 수 있다 **");
        Console.WriteLine();
        Console.WriteLine("구매 로그 (업그레이드 시각·간격) — 간격이 뒤로 갈수록 완만히 늘어나야 건강하다:");
        foreach (var line in log) Console.WriteLine(line);

        Console.WriteLine();
        Console.WriteLine("(간격 추세는 위 로그를 앞/뒤로 눈으로 비교해서 판단할 것 — " +
            "뒤로 갈수록 간격이 점점 벌어지면 체감 효과가 자연스럽게 생긴 것이고, " +
            "그대로거나 짧아지면 비용 곡선을 더 가파르게 잡아야 한다.)");
    }

    static bool AllMaxed(MiningRig rig) =>
        UpgradeCost.AtMax(UpgradeSlot.Tool, rig) && UpgradeCost.AtMax(UpgradeSlot.Cargo, rig) && UpgradeCost.AtMax(UpgradeSlot.Engine, rig);

    /// <summary>한 스텝(StepHours) 안에서 살 수 있는 만큼 몰아 산다(초반엔 한 스텝에 여러 번
    /// 살 수도 있다). 돌려주는 값은 이번 호출에서 실제로 산 개수.</summary>
    static int BuyCheapestUntilBroke(ref MiningRig rig, ref float minerals, float hours, ref float lastUpgradeHour, List<string> log)
    {
        var bought = 0;
        while (true)
        {
            UpgradeSlot? cheapest = null;
            var cheapestCost = float.PositiveInfinity;
            foreach (var slot in new[] { UpgradeSlot.Tool, UpgradeSlot.Cargo, UpgradeSlot.Engine })
            {
                var cost = UpgradeCost.Cost(slot, rig);
                if (cost < cheapestCost) { cheapestCost = cost; cheapest = slot; }
            }
            if (cheapest == null || cheapestCost > minerals) return bought;

            minerals -= cheapestCost;
            var beforeLevel = UpgradeCost.CurrentLevel(cheapest.Value, rig);
            rig = UpgradeCost.Apply(cheapest.Value, rig);
            bought++;

            var gap = hours - lastUpgradeHour;
            log.Add($"  [{hours,6:F1}h] {cheapest,-6} Lv.{beforeLevel,2}→{beforeLevel + 1,2}  비용 {cheapestCost,7:F1}  간격 {gap,6:F2}h");
            lastUpgradeHour = hours;
        }
    }
}
