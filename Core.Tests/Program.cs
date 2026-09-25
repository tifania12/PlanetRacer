using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GemRacer.Core;

// NuGet 없이 돌아가는 최소 테스트 러너. `dotnet run` 으로 실행. 실패가 하나라도 있으면 종료 코드 1.
static class Program
{
    static int _pass, _fail;

    static int Main(string[] args)
    {
        // L-05 봇 시뮬레이션은 테스트가 아니라 리포트라 별도 인자로만 돈다.
        // `dotnet run`(인자 없음)은 지금처럼 그대로 테스트만 돈다 — CLAUDE.md 2번 규칙 유지.
        if (args.Length > 0 && args[0] == "sim")
        {
            BalanceSim.Run();
            return 0;
        }

        // P-07 레이싱카 부품 등급(C/B/A/S) 밸런스 리포트 — 위 sim과 같은 이유로 별도 인자.
        if (args.Length > 0 && args[0] == "sim-parts")
        {
            BalanceSimParts.Run();
            return 0;
        }

        var quartz = DefaultData.Planets()[0];
        var lapis = DefaultData.Planets()[5];

        Test("채굴: 레벨이 오르면 시간당 산출이 오른다", () =>
        {
            var a = MiningSimulator.MineralsPerHour(new MiningRig { ToolLevel = 1 }, quartz);
            var b = MiningSimulator.MineralsPerHour(new MiningRig { ToolLevel = 5 }, quartz);
            var c = MiningSimulator.MineralsPerHour(new MiningRig { ToolLevel = 5, EngineLevel = 5 }, quartz);
            Assert(a > 0, "산출 > 0");
            Assert(b > a, $"도구 5레벨 {b:F1} > 1레벨 {a:F1}");
            Assert(c > b, $"엔진 5레벨 {c:F1} > 엔진 1레벨 {b:F1}");
        });

        Test("채굴: 첫 부품(15광물)까지 5분 안쪽 — P1 관문 기준", () =>
        {
            var perHour = MiningSimulator.MineralsPerHour(new MiningRig(), quartz);
            var minutesToFirstPart = DefaultData.PartCostC / perHour * 60f;
            Assert(minutesToFirstPart <= 5f, $"첫 부품까지 {minutesToFirstPart:F1}분 (시간당 {perHour:F0})");
        });

        Test("오프라인: 화물칸 상한에서 잘린다", () =>
        {
            var rig = new MiningRig { CargoLevel = 1 }; // 4시간
            var r = MiningSimulator.Offline(rig, quartz, 10 * 3600);
            Assert(Math.Abs(r.HoursCounted - 4f) < 0.001f, $"인정 {r.HoursCounted}h");
            Assert(Math.Abs(r.HoursWasted - 6f) < 0.001f, $"버림 {r.HoursWasted}h");
            var full = new MiningRig { CargoLevel = UpgradeCost.CargoMaxLevel };
            var expectedFull = 4f * MathF.Pow(1.12f, UpgradeCost.CargoMaxLevel - 1);
            AssertNear(expectedFull, MiningSimulator.CargoHours(full, quartz), "30레벨 = 쿼츠 기본 4h × 1.12^29");
        });

        Test("M-01: 화물칸 기본 상한이 행성마다 다르다(쿼츠·루비 4h / 사파이어·아쿠아마린 5h / 주사·라피스 6h)", () =>
        {
            var ruby = DefaultData.Planets()[1];
            var sapphire = DefaultData.Planets()[2];
            var aquamarine = DefaultData.Planets()[3];
            var cinnabar = DefaultData.Planets()[4];
            var rig = new MiningRig { CargoLevel = 1 }; // 배율 ×1이라 기본값이 그대로 나온다
            AssertNear(4f, MiningSimulator.CargoHours(rig, quartz), "쿼츠");
            AssertNear(4f, MiningSimulator.CargoHours(rig, ruby), "루비");
            AssertNear(5f, MiningSimulator.CargoHours(rig, sapphire), "사파이어");
            AssertNear(5f, MiningSimulator.CargoHours(rig, aquamarine), "아쿠아마린");
            AssertNear(6f, MiningSimulator.CargoHours(rig, cinnabar), "주사");
            AssertNear(6f, MiningSimulator.CargoHours(rig, lapis), "라피스 라줄리");
        });

        Test("M-01: 같은 행성에서 화물칸(CargoLevel)을 올리면 상한(원석)도 그만큼(배율 그대로) 늘어난다", () =>
        {
            // 2026-09-17 P-01부터 배율은 레벨당 ×1.12 지수식이다 — MineralsPerHour는 CargoLevel과
            // 무관하니 원석 상한도 정확히 1.12^(lvl-1)배가 나와야 한다.
            var level1 = MiningSimulator.CargoCapacityMinerals(new MiningRig { CargoLevel = 1 }, quartz);
            var level10 = MiningSimulator.CargoCapacityMinerals(new MiningRig { CargoLevel = 10 }, quartz);
            AssertNear(level1 * MathF.Pow(1.12f, 9), level10, "10레벨 상한 = 1레벨 상한 × 1.12^9");
        });

        Test("P-01: 화물칸 30레벨까지 올려도 구매 간격이 20레벨 뒤 3배를 안 넘는다", () =>
        {
            // idle-research.md 1절 — 중요한 건 비용 성장률 자체가 아니라 "비용 성장률 ÷ 생산 성장률".
            // 그 비율이 레벨마다 구매 간격이 몇 %씩 느는지를 정한다. 화물칸은 "생산"에 해당하는 게
            // 없어서(정제 광물 수입은 Tool/Engine/Refinery가 만든다) CargoHours 배율을 그 자리에 쓴다
            // — 레벨을 올려서 얻는 값이 커질수록 같은 돈을 써도 체감 간격이 짧아진다는 뜻이다.
            float RatioAt(int lvl) =>
                (UpgradeCost.Cost(UpgradeSlot.Cargo, new MiningRig { CargoLevel = lvl + 1 })
                    / UpgradeCost.Cost(UpgradeSlot.Cargo, new MiningRig { CargoLevel = lvl }))
                / (MiningSimulator.CargoHours(new MiningRig { CargoLevel = lvl + 1 }, quartz)
                    / MiningSimulator.CargoHours(new MiningRig { CargoLevel = lvl }, quartz));

            for (var lvl = 1; lvl <= UpgradeCost.CargoMaxLevel - 2; lvl++)
            {
                var r = RatioAt(lvl);
                Assert(r > 1.039f && r < 1.061f, $"{lvl}→{lvl + 1}레벨 비율 {r:F4} (목표 1.04~1.06)");
            }

            var cumulative = 1f;
            for (var lvl = 1; lvl <= 20; lvl++) cumulative *= RatioAt(lvl);
            Assert(cumulative < 3f, $"1→21레벨 누적 비율 {cumulative:F2}배 (기준 3배 미만, idle-research.md 목표 2.4배 근접)");
        });

        Test("M-01: 접속 중(온라인) 채굴도 화물칸 상한에서 멈춘다 — 상한 도달 후 더 캐도 원석이 안 늘어난다", () =>
        {
            var rig = new MiningRig { CargoLevel = 1 };
            var capacity = MiningSimulator.CargoCapacityMinerals(rig, quartz);

            // 상한에 못 미치면 그대로 더해진다.
            var below = MiningSimulator.ClampToCargoCapacity(capacity * 0.5f, rig, quartz);
            AssertNear(capacity * 0.5f, below, "상한 아래에서는 안 잘림");

            // 상한을 넘기면 그 이상은 안 는다(경계값 포함).
            var atCap = MiningSimulator.ClampToCargoCapacity(capacity, rig, quartz);
            AssertNear(capacity, atCap, "정확히 상한이면 그대로");
            var over = MiningSimulator.ClampToCargoCapacity(capacity + 999f, rig, quartz);
            AssertNear(capacity, over, "상한을 넘겨 캤어도 원석은 상한에서 잘린다");

            // 실제 실시간 루프처럼 여러 틱을 몰아서 흘려도(MiningRunState.Advance) 상한을 못 넘는다.
            var run = new MiningRunState(rig, quartz);
            var raw = 0f;
            for (var i = 0; i < 20; i++)
                raw = MiningSimulator.ClampToCargoCapacity(raw + run.Advance(rig, quartz, 3600f), rig, quartz);
            Assert(raw <= capacity + 0.001f, $"20시간을 몰아 캐도 원석 {raw:F1} <= 상한 {capacity:F1}");
            AssertNear(capacity, raw, "이 정도로 오래 돌리면 결국 상한에 딱 닿는다");
        });

        Test("M-02: 제련소 시간당 변환량은 0레벨 0, 5레벨(최대)에서 원석 산출과 같다, 범위 밖은 클램프", () =>
        {
            var rig0 = new MiningRig { RefineryLevel = 0 };
            var rig5 = new MiningRig { RefineryLevel = 5 };
            var rigOver = new MiningRig { RefineryLevel = 99 };   // 방어적 클램프 확인
            var rigNeg = new MiningRig { RefineryLevel = -3 };
            AssertNear(0f, MiningSimulator.RefinePerHour(rig0, quartz), "0레벨");
            AssertNear(MiningSimulator.MineralsPerHour(rig5, quartz), MiningSimulator.RefinePerHour(rig5, quartz), "5레벨 = 원석 산출과 동일");
            AssertNear(MiningSimulator.RefinePerHour(rig5, quartz), MiningSimulator.RefinePerHour(rigOver, quartz), "5 초과는 5로 클램프");
            AssertNear(0f, MiningSimulator.RefinePerHour(rigNeg, quartz), "음수는 0으로 클램프");

            var rig3 = new MiningRig { RefineryLevel = 3 };
            Assert(MiningSimulator.RefinePerHour(rig3, quartz) > MiningSimulator.RefinePerHour(rig0, quartz)
                && MiningSimulator.RefinePerHour(rig3, quartz) < MiningSimulator.RefinePerHour(rig5, quartz),
                "레벨이 오를수록 변환량도 단조 증가");
        });

        Test("M-02: Refine은 가진 원석보다 많이 못 넘기고, 델타 0/음수·원석 0이면 0을 돌려준다", () =>
        {
            var rig = new MiningRig { RefineryLevel = 5 };
            Assert(MiningSimulator.Refine(100f, rig, quartz, 0f) == 0f, "델타 0");
            Assert(MiningSimulator.Refine(100f, rig, quartz, -1f) == 0f, "델타 음수");
            Assert(MiningSimulator.Refine(0f, rig, quartz, 3600f) == 0f, "원석 0");

            // 1시간(3600초) 몰아 주면 시간당 변환량과 정확히 같아야 한다(원석이 충분할 때).
            var perHour = MiningSimulator.RefinePerHour(rig, quartz);
            AssertNear(perHour, MiningSimulator.Refine(perHour * 10f, rig, quartz, 3600f), "1시간분 정제 = RefinePerHour");

            // 원석이 모자라면 그만큼만 — 절대 원석 보유량을 넘길 수 없다.
            AssertNear(5f, MiningSimulator.Refine(5f, rig, quartz, 3600f), "가진 원석(5)이 시간당 변환량보다 적으면 5만");
        });

        Test("2026-09-19: Entitlements.AutoRefineryAlwaysOn 배선용 forceFullRefine 오버로드 — 기존 호출부는 그대로, true면 레벨 무관 5레벨과 동일", () =>
        {
            var rig0 = new MiningRig { RefineryLevel = 0 };
            var rig3 = new MiningRig { RefineryLevel = 3 };
            var rig5 = new MiningRig { RefineryLevel = 5 };

            // 기존 2/4인자 호출은 새 오버로드에 false를 넘기는 것과 완전히 같다(회귀 없음).
            AssertNear(MiningSimulator.RefinePerHour(rig3, quartz), MiningSimulator.RefinePerHour(rig3, quartz, false), "2인자==4인자(false) 회귀");
            AssertNear(MiningSimulator.Refine(100f, rig3, quartz, 60f), MiningSimulator.Refine(100f, rig3, quartz, 60f, false), "4인자==5인자(false) 회귀");

            // forceFullRefine=true면 제련소 레벨과 무관하게 원석 산출과 같다(=5레벨과 동일).
            AssertNear(MiningSimulator.MineralsPerHour(rig0, quartz), MiningSimulator.RefinePerHour(rig0, quartz, true), "0레벨도 강제 전체 정제면 원석 산출과 동일");
            AssertNear(MiningSimulator.RefinePerHour(rig5, quartz, false), MiningSimulator.RefinePerHour(rig0, quartz, true), "0레벨 강제 전체 정제 = 5레벨 평소 정제");
            AssertNear(MiningSimulator.RefinePerHour(rig0, quartz, true), MiningSimulator.RefinePerHour(rig3, quartz, true), "강제 전체 정제는 레벨 무관 동일");

            // Refine 5인자도 forceFullRefine을 그대로 전달한다.
            var perHourForced = MiningSimulator.RefinePerHour(rig0, quartz, true);
            AssertNear(perHourForced, MiningSimulator.Refine(perHourForced * 10f, rig0, quartz, 3600f, true), "1시간분 강제 전체 정제 = RefinePerHour(강제)");
            Assert(MiningSimulator.Refine(100f, rig0, quartz, 3600f, false) < MiningSimulator.Refine(100f, rig0, quartz, 3600f, true),
                "0레벨은 강제 전체 정제 쪽이 평소보다 많이 정제된다");
        });

        Test("2026-09-19: Offline도 forceFullRefine 오버로드 — 구독 중이면 오프라인에서도 화물칸이 절대 안 찬다", () =>
        {
            var rig0 = new MiningRig { RefineryLevel = 0 };
            var hours = 50.0; // 0레벨은 이 시간 안에 상한에 닿을 만큼 충분히 길다(위 테스트에서 확인됨)

            // 기존 3인자 호출은 4인자(false)와 완전히 같다(회귀 없음).
            var normal = MiningSimulator.Offline(rig0, quartz, hours * 3600.0);
            var explicitFalse = MiningSimulator.Offline(rig0, quartz, hours * 3600.0, false);
            AssertNear(normal.HoursCounted, explicitFalse.HoursCounted, "3인자==4인자(false) HoursCounted 회귀");
            AssertNear(normal.RefinedGained, explicitFalse.RefinedGained, "3인자==4인자(false) RefinedGained 회귀");

            // 0레벨(제련소 없음)은 평소엔 상한에 닿아 시간이 낭비된다.
            Assert(normal.HoursWasted > 0f, "0레벨은 평소 오프라인에서 화물칸이 차서 낭비 시간이 생긴다");

            // forceFullRefine=true면 0레벨이라도 5레벨과 똑같이 절대 상한에 안 닿는다(HoursWasted=0).
            var forced = MiningSimulator.Offline(rig0, quartz, hours * 3600.0, true);
            AssertNear(0f, forced.HoursWasted, "강제 전체 정제면 0레벨도 낭비 시간 0");
            AssertNear(0f, forced.Minerals, "강제 전체 정제면 원석이 안 쌓인다(전부 정제로 빠짐)");

            // 정제량 자체는 강제 쪽이 원석 산출 전체(rate)와 같아 5레벨 평소와 동일해야 한다.
            var rig5 = new MiningRig { RefineryLevel = 5 };
            var withMaxRefinery = MiningSimulator.Offline(rig5, quartz, hours * 3600.0);
            AssertNear(withMaxRefinery.RefinedGained, forced.RefinedGained, "0레벨 강제 전체 정제 = 5레벨 평소 정제량(오프라인)");

            // 초 단위 경과가 0/음수여도 안전하다(회귀 방지, 기존 3인자 테스트와 같은 방어선).
            var atZero = MiningSimulator.Offline(rig0, quartz, 0.0, true);
            var negative = MiningSimulator.Offline(rig0, quartz, -1.0, true);
            AssertNear(0f, atZero.RefinedGained, "경과 0초는 강제 전체 정제여도 정제량 0");
            AssertNear(0f, negative.RefinedGained, "경과 음수는 강제 전체 정제여도 정제량 0");
        });

        Test("M-02: 제련소 레벨이 오르면 오프라인에서 같은 시간에 원석 상한에 더 늦게(또는 안) 닿는다", () =>
        {
            var planet = quartz;
            var hours = 50.0; // 0~4레벨 전부 이 안에서 상한에 닿을 만큼 충분히 긴 시간으로 고른다
            float prevCounted = -1f;
            float prevRefined = -1f;
            for (var level = 0; level <= 5; level++)
            {
                var rig = new MiningRig { RefineryLevel = level };
                var r = MiningSimulator.Offline(rig, planet, hours * 3600.0);
                if (level > 0)
                {
                    Assert(r.HoursCounted >= prevCounted - 0.001f, $"레벨 {level} 인정 시간({r.HoursCounted:F2}h)은 레벨 {level - 1}({prevCounted:F2}h) 이상");
                    Assert(r.RefinedGained > prevRefined, $"레벨 {level} 정제량({r.RefinedGained:F1})은 레벨 {level - 1}({prevRefined:F1})보다 많다");
                }
                prevCounted = r.HoursCounted;
                prevRefined = r.RefinedGained;
            }

            // 5레벨(원석 산출과 변환량이 같음)은 아무리 오래 지나도 상한에 안 닿는다 — 낭비된 시간이 없다.
            var maxRig = new MiningRig { RefineryLevel = 5 };
            var centuries = MiningSimulator.Offline(maxRig, planet, 3600.0 * 24 * 365 * 300);
            AssertNear(0f, centuries.HoursWasted, "5레벨은 300년치를 몰아줘도 낭비 시간 0");
            Assert(!float.IsNaN(centuries.RefinedGained) && !float.IsInfinity(centuries.RefinedGained), "정제량이 정상 수");

            // 제련소가 없으면(0레벨) 예전처럼 정제량이 0이다 — RefinedGained가 새로 생겼다고
            // 아무 것도 없던 채굴차가 갑자기 정제를 하면 안 된다(회귀 방지).
            var noRefinery = new MiningRig { RefineryLevel = 0 };
            var r0 = MiningSimulator.Offline(noRefinery, planet, hours * 3600.0);
            AssertNear(0f, r0.RefinedGained, "제련소 0레벨은 정제량도 0");
        });

        Test("레이스: 부품 장착 전보다 후가 빠르다", () =>
        {
            var course = DefaultData.QuartzCourses()[0];
            var bare = new RacingCar();
            var t0 = RaceSimulator.LapTime(bare.TotalStats(), quartz, course);
            var car = new RacingCar();
            foreach (var p in DefaultData.QuartzStarterParts()) car.Slots[p.Slot] = p;
            var t1 = RaceSimulator.LapTime(car.TotalStats(), quartz, course);
            Assert(t1 < t0, $"장착 후 {t1:F1}s < 전 {t0:F1}s");
            Assert(t0 < 120f && t1 > 5f, $"시간이 상식 범위 ({t0:F1}, {t1:F1})");
        });

        Test("레이스: 강화하면 빨라진다", () =>
        {
            var course = DefaultData.QuartzCourses()[0];
            var car = new RacingCar();
            foreach (var p in DefaultData.QuartzStarterParts()) car.Slots[p.Slot] = p;
            var t1 = RaceSimulator.LapTime(car.TotalStats(), quartz, course);
            foreach (var p in car.Slots.Values) if (p != null) p.Enhance = 10;
            var t2 = RaceSimulator.LapTime(car.TotalStats(), quartz, course);
            Assert(t2 < t1, $"+10 {t2:F1}s < +0 {t1:F1}s");
        });

        Test("레이스: 같은 seed는 같은 결과, 다른 seed는 순위가 흔들릴 수 있다", () =>
        {
            var course = DefaultData.QuartzCourses()[1];
            var ai = RaceSimulator.MakeOpponents(5, 40f, 7);
            var entrants = new List<RaceSimulator.Entrant>(ai) { new RaceSimulator.Entrant { Id = "me", IsPlayer = true, Stats = new Stats { Power = 40, Grip = 40, Suspension = 32, Durability = 40, Boost = 20, Aero = 20 } } };
            var r1 = RaceSimulator.Run(entrants, quartz, course, 1234);
            var r2 = RaceSimulator.Run(entrants, quartz, course, 1234);
            Assert(r1.Select(r => r.Id).SequenceEqual(r2.Select(r => r.Id)), "seed 동일 → 순위 동일");
            Assert(r1.All(r => r.Rank >= 1 && r.Rank <= 6), "순위 1~6");
            Assert(Math.Abs(r1[0].Time - r2[0].Time) < 1e-5f, "시간 동일");
        });

        Test("레이스: 행성 환경이 맞지 않는 부품은 느려진다 (라피스에서 부스터 유리)", () =>
        {
            var course = new Course { Id = "t", PlanetId = "lapis", Length = 1000, FlatRatio = 0.3f, RoughRatio = 0.2f, BoostRatio = 0.5f };
            var boostBuild = new Stats { Power = 40, Grip = 40, Suspension = 40, Durability = 40, Boost = 80, Aero = 20 };
            var gripBuild = new Stats { Power = 40, Grip = 80, Suspension = 40, Durability = 40, Boost = 40, Aero = 20 };
            var tb = RaceSimulator.LapTime(boostBuild, lapis, course);
            var tg = RaceSimulator.LapTime(gripBuild, lapis, course);
            Assert(tb < tg, $"부스터 빌드 {tb:F1}s < 접지 빌드 {tg:F1}s");
        });

        Test("세트: 같은 행성 부품 개수를 센다", () =>
        {
            var car = new RacingCar();
            var parts = DefaultData.QuartzStarterParts();
            car.Slots[PartSlot.Engine] = parts[0];
            car.Slots[PartSlot.Tire] = parts[1];
            car.Slots[PartSlot.Body] = new Part { Id = "r", PlanetId = "ruby", Slot = PartSlot.Body };
            var n = car.SetCount(out var pid);
            Assert(n == 2 && pid == "quartz", $"세트 {n} ({pid})");
        });

        Test("세트: 부품이 하나도 없으면 세트 0, 행성 없음(빈 문자열)", () =>
        {
            var car = new RacingCar();
            var n = car.SetCount(out var pid);
            Assert(n == 0 && pid == "", $"빈 차 세트 {n} ({pid})");
        });

        Test("세트: 행성 없는 범용 부품(PlanetId 빈 문자열)은 아무리 많아도 세트로 안 잡힌다", () =>
        {
            var car = new RacingCar();
            foreach (PartSlot slot in Enum.GetValues(typeof(PartSlot)))
                car.Slots[slot] = new Part { Id = slot.ToString(), PlanetId = "", Slot = slot };
            var n = car.SetCount(out var pid);
            Assert(n == 0 && pid == "", $"범용 부품 6개인데도 세트 {n} ({pid})");
        });

        Test("세트: 두 행성이 동점이면 슬롯 순서상 먼저 나오는 쪽이 이긴다(Dictionary 순회 순서, Engine이 Tire보다 먼저)", () =>
        {
            var car = new RacingCar();
            car.Slots[PartSlot.Engine] = new Part { Id = "e", PlanetId = "ruby", Slot = PartSlot.Engine };
            car.Slots[PartSlot.Tire] = new Part { Id = "t", PlanetId = "quartz", Slot = PartSlot.Tire };
            var n = car.SetCount(out var pid);
            Assert(n == 1 && pid == "ruby", $"동점 1:1인데 {pid} 승 (세트 {n}) — Engine이 먼저 채워졌으니 ruby여야 한다");
        });

        Test("세트: 여섯 슬롯 전부 같은 행성이면 6개 다 센다(최대값)", () =>
        {
            var car = new RacingCar();
            foreach (PartSlot slot in Enum.GetValues(typeof(PartSlot)))
                car.Slots[slot] = new Part { Id = slot.ToString(), PlanetId = "quartz", Slot = slot };
            var n = car.SetCount(out var pid);
            Assert(n == 6 && pid == "quartz", $"풀세트 {n} ({pid})");
        });

        Test("레이스: 출전자가 없으면 빈 결과(예외 없음)", () =>
        {
            var course = DefaultData.QuartzCourses()[1];
            var results = RaceSimulator.Run(new List<RaceSimulator.Entrant>(), quartz, course, 1);
            Assert(results.Count == 0, $"출전자 0 → 결과 0 ({results.Count})");
        });

        Test("레이스: AI 상대 0명을 요청해도 빈 목록(예외 없음)", () =>
        {
            var ai = RaceSimulator.MakeOpponents(0, 40f, 1);
            Assert(ai.Count == 0, $"count 0 → 목록 0 ({ai.Count})");
        });

        Test("레이스: AI 상대 수가 음수여도(잘못된 코스 데이터 등) 예외 없이 빈 목록", () =>
        {
            // new List<T>(음수)는 원래 ArgumentOutOfRangeException을 던진다 — MakeOpponents가
            // count를 0으로 방어하는지 확인. 지금은 5명 고정 호출뿐이라 실제로는 안 벌어지지만,
            // 코스 데이터에서 상대 수를 읽어오게 될 때(W2)를 대비한 방어선.
            var ai = RaceSimulator.MakeOpponents(-3, 40f, 1);
            Assert(ai.Count == 0, $"count -3 → 목록 0 (예외 없이, {ai.Count})");
        });

        Test("레이스: 행성 스탯이 정상 범위(0~1)를 벗어나도(오염된 데이터) LapTime이 NaN·Infinity 없이 유한하다", () =>
        {
            var course = DefaultData.QuartzCourses()[0];
            var stats = new Stats { Power = 40, Grip = 40, Suspension = 32, Durability = 40, Boost = 20, Aero = 20 };
            var extreme = new Planet { Id = "x", Heat = 5f, Cold = -5f, Toxic = 10f, Liquid = -10f, Gravity = 20f, Atmosphere = -20f, Roughness = 5f, VeinYield = 1, Circumference = 100, VeinCount = 1, BaseCargoHours = 1 };
            var t = RaceSimulator.LapTime(stats, extreme, course);
            Assert(!float.IsNaN(t) && !float.IsInfinity(t), $"오염된 행성 값에도 LapTime이 유한하다 ({t})");
        });

        Test("레이스: 스탯이 전부 0이거나 음수여도(신규 채굴차·오염된 세이브) LapTime이 유한하고 완주는 한다", () =>
        {
            var course = DefaultData.QuartzCourses()[0];
            var zero = RaceSimulator.LapTime(new Stats(), quartz, course);
            Assert(!float.IsNaN(zero) && !float.IsInfinity(zero) && zero > 0, $"스탯 0 → 유한하고 양수 ({zero})");
            var negative = RaceSimulator.LapTime(new Stats { Power = -40, Grip = -40, Suspension = -40, Boost = -20 }, quartz, course);
            Assert(!float.IsNaN(negative) && !float.IsInfinity(negative) && negative > 0, $"스탯 음수 → 유한하고 양수 ({negative})");
        });

        Test("난수: xorshift는 플랫폼 무관하게 같은 수열", () =>
        {
            var a = new DeterministicRandom(42); var b = new DeterministicRandom(42);
            for (var i = 0; i < 100; i++) Assert(a.NextUInt() == b.NextUInt(), "수열 동일");
            var z = new DeterministicRandom(0);
            Assert(z.NextUInt() != 0, "seed 0도 동작");
        });

        Test("난수: 음수 시드도 예외 없이 동작하고 같은 값이면 같은 수열을 낸다", () =>
        {
            var a = new DeterministicRandom(-1); var b = new DeterministicRandom(-1);
            for (var i = 0; i < 50; i++) Assert(a.NextUInt() == b.NextUInt(), "음수 시드도 재현성 유지");
            var c = new DeterministicRandom(int.MinValue);
            Assert(c.NextUInt() != 0, "int.MinValue 시드도 동작(오버플로 없이)");
        });

        Test("난수: NextInt는 [min, max) 경계를 절대 안 벗어난다(음수 범위·단일값 범위 포함)", () =>
        {
            var rng = new DeterministicRandom(2026);
            for (var i = 0; i < 500; i++)
            {
                var single = rng.NextInt(7, 8);
                Assert(single == 7, $"[7,8) 범위는 항상 7이어야 한다 (실제 {single})");
            }
            var sawMin = false; var sawMax = false;
            for (var i = 0; i < 2000; i++)
            {
                var v = rng.NextInt(-5, 5);
                Assert(v >= -5 && v < 5, $"NextInt(-5,5)가 범위 밖({v})");
                if (v == -5) sawMin = true;
                if (v == 4) sawMax = true;
            }
            Assert(sawMin && sawMax, "충분히 돌리면 음수 범위의 양 끝(-5, 4)이 다 나와야 한다");
        });

        Test("난수: NextInt(x, x) 빈 범위는 예외 대신 x를 돌려준다", () =>
        {
            // 위 테스트가 "단일값 범위"로 커버한 NextInt(7,8)(폭 1)과 다르다 — 이건 폭 0이라
            // 고치기 전에는 `% (uint)(max-min)`이 `% 0`이 되어 DivideByZeroException을 던졌다.
            // PetSpeciesTable.PickInGrade(빈 등급을 잘못 넘기면 InGrade의 ids.Length가 0)처럼
            // 호출부가 실수로 빈 배열 Length를 그대로 넘길 수 있는 자리라 방어해 둔 것.
            var rng = new DeterministicRandom(7);
            Assert(rng.NextInt(3, 3) == 3, "NextInt(3,3)은 3을 돌려줘야 한다");
            Assert(rng.NextInt(0, 0) == 0, "NextInt(0,0)은 0을 돌려줘야 한다");
            Assert(rng.NextInt(-2, -2) == -2, "NextInt(-2,-2)는 -2를 돌려줘야 한다");
            // 폭이 음수(max < min, 더 잘못된 호출)여도 예외 없이 min을 돌려준다.
            Assert(rng.NextInt(5, 1) == 5, "NextInt(5,1)(역전된 범위)은 min인 5를 돌려줘야 한다");
        });

        // L-02: 보물 등급이 높을수록 요구 채굴 도구 레벨도 높아야 한다.
        Test("보물: 등급이 오를수록 요구 도구 레벨도 오른다", () =>
        {
            var defs = DefaultData.QuartzTreasureDefs();
            Assert(defs.Count == 4, $"등급 4종(C~S) {defs.Count}");
            for (var i = 1; i < defs.Count; i++)
                Assert(defs[i].RequiredToolLevel > defs[i - 1].RequiredToolLevel,
                    $"{defs[i].Grade} 요구 레벨 {defs[i].RequiredToolLevel} > {defs[i - 1].Grade} {defs[i - 1].RequiredToolLevel}");
        });

        // L-01: 탐험 — 경과 시간이 길수록(사이클이 늘수록) 발견 수가 줄지 않는다. seed가 같으면
        // 뒤에서 새로 도는 사이클만 늘어날 뿐 앞선 결과는 그대로다(순차 RNG라 접두어가 보존됨).
        Test("탐험: 경과 시간이 길수록 발견 수가 늘거나 같다(같은 seed)", () =>
        {
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var short1h = ExplorationSimulator.Discover(rig, quartz, 3600, defs, seed: 99);
            var long10h = ExplorationSimulator.Discover(rig, quartz, 36000, defs, seed: 99);
            Assert(long10h.Count >= short1h.Count, $"10시간 발견 {long10h.Count} >= 1시간 {short1h.Count}");
        });

        Test("탐험: 같은 seed는 같은 발견 목록(재현 가능)", () =>
        {
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var a = ExplorationSimulator.Discover(rig, quartz, 20000, defs, seed: 7);
            var b = ExplorationSimulator.Discover(rig, quartz, 20000, defs, seed: 7);
            Assert(a.Count == b.Count, $"발견 수 동일 {a.Count} == {b.Count}");
            for (var i = 0; i < a.Count; i++) Assert(a[i].DefId == b[i].DefId, $"[{i}] {a[i].DefId} == {b[i].DefId}");
        });

        Test("탐험: 도구 레벨이 낮으면 높은 등급 보물이 목록엔 남고 CanMineNow만 거짓", () =>
        {
            var weakRig = new MiningRig { ToolLevel = 1 };
            var strongRig = new MiningRig { ToolLevel = 26 };
            var defs = DefaultData.QuartzTreasureDefs();
            var sGrade = defs[defs.Count - 1]; // S등급, RequiredToolLevel 26
            Assert(!ExplorationSimulator.CanMine(sGrade, weakRig), "도구 1레벨로는 S등급 못 캠");
            Assert(ExplorationSimulator.CanMine(sGrade, strongRig), "도구 26레벨이면 S등급 캘 수 있음");
        });

        // L-03: 레이스 보상 = 채굴차 부품(슬롯 레벨 상승). 광물이 아니라 이게 나와야 나선이 돈다.
        Test("레이스 보상: 채굴차 부품을 적용하면 해당 슬롯 레벨만 오른다", () =>
        {
            var rig = new MiningRig();
            var reward = DefaultData.QuartzLocalRaceRewards()[0]; // Tool
            var after = RigPartApply.Apply(rig, reward);
            Assert(after.ToolLevel == rig.ToolLevel + 1, $"Tool +1 {after.ToolLevel}");
            Assert(after.CargoLevel == rig.CargoLevel && after.EngineLevel == rig.EngineLevel
                && after.DetectorLevel == rig.DetectorLevel && after.RefineryLevel == rig.RefineryLevel,
                "다른 슬롯은 그대로");
        });

        // L-05 봇 시뮬레이션에서 발견: 레이스 무료 보상이 슬롯 상한을 무시하고 계속 올라가고 있었다.
        Test("레이스 보상: 이미 최대 레벨이면 레이스 보상을 받아도 상한을 넘지 않는다", () =>
        {
            var maxedRig = new MiningRig { CargoLevel = UpgradeCost.CargoMaxLevel };
            var cargoReward = DefaultData.QuartzLocalRaceRewards()[1]; // Cargo
            var after = RigPartApply.Apply(maxedRig, cargoReward);
            Assert(after.CargoLevel == UpgradeCost.CargoMaxLevel, $"화물칸 {UpgradeCost.CargoMaxLevel}레벨에서 보상을 받아도 그대로 {after.CargoLevel}");
        });

        Test("레이스 보상: 쿼츠 로컬 레이스 3개가 서로 다른 슬롯을 준다", () =>
        {
            var rewards = DefaultData.QuartzLocalRaceRewards();
            Assert(rewards.Count == 3, $"보상 3개 {rewards.Count}");
            var slots = new HashSet<RigSlot>();
            foreach (var r in rewards) slots.Add(r.Slot);
            Assert(slots.Count == 3, $"슬롯 3종류 서로 다름 {slots.Count}");
        });

        // 위 최대 레벨 테스트는 Cargo(상한 30)만 확인했다. 나머지 네 슬롯도 각자 다른 상한(Tool 30,
        // Engine 30, Detector/Refinery 5)이라 슬롯마다 따로 막히는지 확인해야 한다.
        Test("레이스 보상: 다섯 슬롯 전부 각자의 최대 레벨에서 보상을 받아도 상한을 넘지 않는다", () =>
        {
            var maxed = new MiningRig
            {
                ToolLevel = UpgradeCost.ToolMaxLevel, CargoLevel = UpgradeCost.CargoMaxLevel,
                EngineLevel = UpgradeCost.EngineMaxLevel, DetectorLevel = 5, RefineryLevel = 5,
            };
            foreach (var slot in new[] { RigSlot.Tool, RigSlot.Cargo, RigSlot.Engine, RigSlot.Detector, RigSlot.Refinery })
            {
                var after = RigPartApply.Apply(maxed, new RigPartReward { Slot = slot, LevelBonus = 1 });
                Assert(after.ToolLevel == UpgradeCost.ToolMaxLevel && after.CargoLevel == UpgradeCost.CargoMaxLevel
                    && after.EngineLevel == UpgradeCost.EngineMaxLevel && after.DetectorLevel == 5 && after.RefineryLevel == 5,
                    $"{slot}: 이미 최대인데 넘지 않음");
            }
        });

        // D09-M: RigParts.cs 주석엔 "레이스 보상은 LevelBonus=1 고정"이라 적혀 있지만, 공구 상자
        // 쪽(LootReward.LevelBonusFor)은 이미 등급별로 다르다 — S등급이면 3이라 Detector·Refinery
        // (상한 5)는 겨우 두세 번만 열어도 이 경계에 닿는다. RigPartApply가 "이미 최대일 때"뿐
        // 아니라 "한 번에 최대를 막 넘길 때"도 Math.Min으로 그대로 버텨 주는지 실제 값으로 확인한다.
        Test("공구 상자 보상: S등급(LevelBonus 3)을 상한 근처 Refinery에 적용해도 딱 최대에서 멈춘다", () =>
        {
            Assert(LootReward.LevelBonusFor(PartGrade.S) == 3, "S등급 보너스는 3");
            var rig = new MiningRig { RefineryLevel = 4 };
            var after = RigPartApply.Apply(rig, new RigPartReward { Slot = RigSlot.Refinery, LevelBonus = LootReward.LevelBonusFor(PartGrade.S) });
            Assert(after.RefineryLevel == 5, $"4 + 3은 5(상한)를 넘지만 상한에서 멈춘다 {after.RefineryLevel}");
        });

        Test("레이스 보상: LevelBonus 0은 레벨을 그대로 둔다", () =>
        {
            var rig = new MiningRig { EngineLevel = 4 };
            var after = RigPartApply.Apply(rig, new RigPartReward { Slot = RigSlot.Engine, LevelBonus = 0 });
            Assert(after.EngineLevel == 4, $"보너스 0이면 그대로 {after.EngineLevel}");
        });

        // RigPartApply의 switch에는 default가 없다 — UpgradeCost(D05-N)와 달리 정의 밖 RigSlot을
        // 예외로 막지 않고 조용히 아무 슬롯도 안 바꾼 채 돌아온다. RigPartReward는 세이브에서
        // 역직렬화되는 값이 아니라 보상 테이블에서만 만들어져 지금은 실제로 이 경로를 못 타지만,
        // "던지지 않고 무시한다"가 의도된 동작인지 다음에 헷갈리지 않도록 현재 동작을 그대로 못 박아 둔다.
        Test("레이스 보상: 정의 밖 RigSlot은 예외 없이 조용히 무시된다(현재 동작, UpgradeCost와 다름)", () =>
        {
            var rig = new MiningRig { ToolLevel = 3, CargoLevel = 2, EngineLevel = 1, DetectorLevel = 0, RefineryLevel = 0 };
            var after = RigPartApply.Apply(rig, new RigPartReward { Slot = (RigSlot)99, LevelBonus = 1 });
            Assert(after.ToolLevel == rig.ToolLevel && after.CargoLevel == rig.CargoLevel
                && after.EngineLevel == rig.EngineLevel && after.DetectorLevel == rig.DetectorLevel
                && after.RefineryLevel == rig.RefineryLevel, "다섯 슬롯 전부 그대로");
        });

        // D09-M: 레이스 연료 회복 경계값. RaceFuel.Recover는 시간을 인자로만 받는 순수 함수라
        // (CLAUDE.md 1번) 실제 시각 없이도 경계 케이스를 그대로 재현할 수 있다.
        Test("연료: 시간이 하나도 안 지나면 그대로다", () =>
        {
            var (fuel, baseline) = RaceFuel.Recover(3, 1_000_000L, 1_000_000L);
            Assert(fuel == 3, $"연료 그대로 {fuel}");
            Assert(baseline == 1_000_000L, $"기준 시각 그대로 {baseline}");
        });

        Test("연료: 시계가 되감기면(음수 경과) 회복하지 않는다", () =>
        {
            var (fuel, baseline) = RaceFuel.Recover(3, 1_000_000L, 900_000L);
            Assert(fuel == 3, $"연료 그대로 {fuel}");
            Assert(baseline == 1_000_000L, $"기준 시각도 그대로(당기지 않음) {baseline}");
        });

        Test("연료: 정확히 회복 주기만큼 지나면 1개 늘고 기준 시각이 그만큼만 앞으로 간다", () =>
        {
            var (fuel, baseline) = RaceFuel.Recover(3, 0L, RaceFuel.RecoverySeconds);
            Assert(fuel == 4, $"연료 +1 {fuel}");
            Assert(baseline == RaceFuel.RecoverySeconds, $"기준 시각이 정확히 한 주기만큼 {baseline}");
        });

        Test("연료: 이미 최대치면 아무리 기다려도 그대로고, 기준 시각만 지금으로 당겨진다", () =>
        {
            var (fuel, baseline) = RaceFuel.Recover(RaceFuel.MaxFuel, 0L, 999_999_999L);
            Assert(fuel == RaceFuel.MaxFuel, $"최대 {fuel}");
            Assert(baseline == 999_999_999L, "기준 시각이 지금으로 당겨짐 — 나중에 연료를 쓰기 시작할 때부터 다시 잰다");
        });

        Test("연료: 아주 오래(300년치) 지나도 최대치에서 멈추고 오버플로 없이 지금으로 당겨진다", () =>
        {
            var hugeSeconds = 300L * 365 * 24 * 3600;
            var (fuel, baseline) = RaceFuel.Recover(0, 0L, hugeSeconds);
            Assert(fuel == RaceFuel.MaxFuel, $"최대에서 멈춤 {fuel}");
            Assert(baseline == hugeSeconds, $"기준 시각 지금으로 당겨짐 {baseline}");
        });

        Test("연료: 음수로 들어와도 방어적으로 0으로 취급한다", () =>
        {
            var (fuel, _) = RaceFuel.Recover(-5, 0L, 0L);
            Assert(fuel == 0, $"음수 연료는 0으로 취급 {fuel}");
        });

        Test("연료: 잘게 나눠 불러도 한 번에 몰아 불러도 결과가 같다 (오프라인 캐치업과 같은 성질)", () =>
        {
            var totalSeconds = RaceFuel.RecoverySeconds * 3 + 120; // 3개 회복 + 남는 시간
            var (bigFuel, bigBaseline) = RaceFuel.Recover(2, 0L, totalSeconds);

            // 같은 델타를 세 번에 나눠서 순서대로 적용.
            // 조각 크기는 RecoverySeconds에서 끌어온다 — 전에는 500/700처럼 박아 둬서
            // 회복 주기를 600초에서 240초로 바꾸자 마지막 조각이 음수가 되어 테스트가 깨졌다
            // (2026-09-17). 상수를 조정해도 이 테스트가 같이 따라와야 한다.
            var fuel = 2; long baseline = 0L; long now = 0L;
            var a = RaceFuel.RecoverySeconds * 5 / 6;      // 한 주기보다 조금 모자란 조각
            var b = RaceFuel.RecoverySeconds * 7 / 6;      // 한 주기를 조금 넘는 조각
            var steps = new long[] { a, b, totalSeconds - a - b };
            foreach (var step in steps)
            {
                now += step;
                (fuel, baseline) = RaceFuel.Recover(fuel, baseline, now);
            }
            Assert(fuel == bigFuel, $"잘게 나눈 결과 {fuel} == 한 번에 {bigFuel}");
            Assert(baseline == bigBaseline, $"기준 시각도 같다 {baseline} == {bigBaseline}");
        });

        Test("연료: 세이브가 깨져 MaxFuel보다 큰 값이 들어와도 방어적으로 클램프된다", () =>
        {
            var (fuel, baseline) = RaceFuel.Recover(999, 0L, 500L);
            Assert(fuel == RaceFuel.MaxFuel, $"MaxFuel로 클램프 {fuel}");
            Assert(baseline == 500L, "이미 꽉 찬 것으로 취급해 기준 시각이 지금으로 당겨짐");
        });

        Test("연료: 남은 칸을 정확히 채우는 경계(recovered == missing)에서도 최대치로 차고 기준 시각이 지금으로 당겨진다", () =>
        {
            // currentFuel=9, missing=1, 정확히 한 주기가 지나 recovered(1) == missing(1)인 경계.
            var (fuel, baseline) = RaceFuel.Recover(RaceFuel.MaxFuel - 1, 0L, RaceFuel.RecoverySeconds);
            Assert(fuel == RaceFuel.MaxFuel, $"경계에서도 최대치 {fuel}");
            Assert(baseline == RaceFuel.RecoverySeconds, "기준 시각이 지금(경계 시각)으로 당겨짐 — baseline+recovered*주기가 아니라 now");
        });

        // 2026-09-19: BonusFuelCapacity(구독 +2) 배선용 4인자 오버로드. 기존 3인자 호출은
        // 전부 이 오버로드에 MaxFuel을 그대로 넘기는 것과 같아야 한다 — MiningController.cs가
        // 안 바뀌었는데도 동작이 달라지면 안 되기 때문이다.
        Test("연료: 4인자 오버로드에 MaxFuel을 그대로 넘기면 기존 3인자 호출과 결과가 완전히 같다", () =>
        {
            var cases = new (int fuel, long baseline, long now)[]
            {
                (3, 1_000_000L, 1_000_000L),
                (3, 1_000_000L, 900_000L),
                (3, 0L, RaceFuel.RecoverySeconds),
                (RaceFuel.MaxFuel, 0L, 999_999_999L),
                (0, 0L, 300L * 365 * 24 * 3600),
                (-5, 0L, 0L),
                (999, 0L, 500L),
            };
            foreach (var c in cases)
            {
                var viaThree = RaceFuel.Recover(c.fuel, c.baseline, c.now);
                var viaFour = RaceFuel.Recover(c.fuel, c.baseline, c.now, RaceFuel.MaxFuel);
                Assert(viaThree == viaFour, $"입력({c.fuel},{c.baseline},{c.now}) 3인자 {viaThree} != 4인자 {viaFour}");
            }
        });

        Test("연료: maxFuel을 늘려 넘기면(구독 +2) MaxFuel을 넘어서까지 차고, 회복 속도는 그대로다", () =>
        {
            var boosted = RaceFuel.MaxFuel + 2;
            var (fuel, baseline) = RaceFuel.Recover(RaceFuel.MaxFuel, 0L, RaceFuel.RecoverySeconds * 2, boosted);
            Assert(fuel == boosted, $"기본 MaxFuel을 넘어 {boosted}까지 회복 {fuel}");
            Assert(baseline == RaceFuel.RecoverySeconds * 2, $"두 주기 만에 +2 다 채웠으니 기준 시각도 지금 {baseline}");

            var (partial, partialBaseline) = RaceFuel.Recover(RaceFuel.MaxFuel, 0L, RaceFuel.RecoverySeconds, boosted);
            Assert(partial == RaceFuel.MaxFuel + 1, $"한 주기만 지나면 +1만 {partial}");
            Assert(partialBaseline == RaceFuel.RecoverySeconds, "정확히 채운 만큼만 기준 시각 이동");
        });

        Test("연료: maxFuel이 0 이하로 들어와도 방어적으로 0으로 취급해 예외 없이 (0, now)를 돌려준다", () =>
        {
            var (fuel, baseline) = RaceFuel.Recover(5, 0L, 1000L, -3);
            Assert(fuel == 0, $"음수 maxFuel은 0으로 취급 {fuel}");
            Assert(baseline == 1000L, "이미 꽉 찬 것으로 취급해 기준 시각이 지금으로 당겨짐");
        });

        // D10-M: 레이스 연출 도착 시각표(RaceAnimation.BuildSchedule) — 실제 판정 순위를
        // 절대 바꾸지 않는지, 화면에서 동시 도착이 안 생기는지가 핵심.
        Test("레이스 연출: 실제 접전(지터뿐인 결과)이어도 도착 순서가 순위와 정확히 같다", () =>
        {
            var course = DefaultData.QuartzCourses()[0];
            var ai = RaceSimulator.MakeOpponents(5, 40f, 7);
            var entrants = new List<RaceSimulator.Entrant>(ai) { new RaceSimulator.Entrant { Id = "player", IsPlayer = true, Stats = new Stats { Power = 40, Grip = 40, Suspension = 32, Durability = 40, Boost = 20, Aero = 20 } } };
            var results = RaceSimulator.Run(entrants, DefaultData.Planets()[0], course, 42);
            var schedule = RaceAnimation.BuildSchedule(results, 24f);

            Assert(schedule.Count == results.Count, $"인원 수 그대로 {schedule.Count} == {results.Count}");
            for (int i = 0; i < schedule.Count; i++)
                Assert(schedule[i].Rank == i + 1, $"schedule[{i}].Rank == {i + 1} (도착 순서 == 순위)");
            for (int i = 1; i < schedule.Count; i++)
                Assert(schedule[i].ArrivalSeconds > schedule[i - 1].ArrivalSeconds, $"엄격히 증가 [{i - 1}]={schedule[i - 1].ArrivalSeconds:F2} < [{i}]={schedule[i].ArrivalSeconds:F2}");
        });

        Test("레이스 연출: 입력 순서가 뒤섞여 있어도 Rank 기준으로 다시 정렬해서 쓴다", () =>
        {
            var shuffled = new List<RaceSimulator.Result>
            {
                new RaceSimulator.Result { Id = "c", Time = 30f, Rank = 3 },
                new RaceSimulator.Result { Id = "a", Time = 10f, Rank = 1 },
                new RaceSimulator.Result { Id = "b", Time = 20f, Rank = 2 },
            };
            var schedule = RaceAnimation.BuildSchedule(shuffled, 24f);
            Assert(schedule[0].Id == "a" && schedule[1].Id == "b" && schedule[2].Id == "c", "정렬 후 a,b,c 순서");
        });

        Test("레이스 연출: 모든 도착 시각이 (0, duration] 안에 있다", () =>
        {
            var results = new List<RaceSimulator.Result>();
            for (int i = 0; i < 6; i++) results.Add(new RaceSimulator.Result { Id = $"e{i}", Time = 10f + i * 0.1f, Rank = i + 1 });
            var schedule = RaceAnimation.BuildSchedule(results, 24f);
            foreach (var a in schedule)
            {
                Assert(a.ArrivalSeconds > 0f, $"{a.Id} > 0 ({a.ArrivalSeconds:F2})");
                Assert(a.ArrivalSeconds <= 24f, $"{a.Id} <= duration ({a.ArrivalSeconds:F2})");
            }
        });

        Test("레이스 연출: 전원 기록이 완전히 같아도(격차 0) 동시 도착 없이 등수 간격으로 균등 배분", () =>
        {
            var results = new List<RaceSimulator.Result>();
            for (int i = 0; i < 6; i++) results.Add(new RaceSimulator.Result { Id = $"e{i}", Time = 10f, Rank = i + 1 });
            var schedule = RaceAnimation.BuildSchedule(results, 24f);
            for (int i = 1; i < schedule.Count; i++)
                Assert(schedule[i].ArrivalSeconds > schedule[i - 1].ArrivalSeconds, $"격차 0이어도 엄격히 증가 [{i}]");
        });

        Test("레이스 연출: 기록 격차가 극단적으로 커도(수천 초 차) duration 안에서 순서만 유지", () =>
        {
            var results = new List<RaceSimulator.Result>
            {
                new RaceSimulator.Result { Id = "a", Time = 10f, Rank = 1 },
                new RaceSimulator.Result { Id = "b", Time = 5000f, Rank = 2 },
                new RaceSimulator.Result { Id = "c", Time = 9999f, Rank = 3 },
            };
            var schedule = RaceAnimation.BuildSchedule(results, 24f);
            Assert(schedule[0].ArrivalSeconds < schedule[1].ArrivalSeconds && schedule[1].ArrivalSeconds < schedule[2].ArrivalSeconds, "순서 유지");
            Assert(schedule[2].ArrivalSeconds <= 24f, $"꼴찌도 duration 안쪽 ({schedule[2].ArrivalSeconds:F2})");
        });

        Test("레이스 연출: 출전자가 1명뿐이면 WinnerArrivalRatio 지점에서 바로 도착", () =>
        {
            var results = new List<RaceSimulator.Result> { new RaceSimulator.Result { Id = "solo", Time = 12f, Rank = 1 } };
            var schedule = RaceAnimation.BuildSchedule(results, 20f);
            Assert(schedule.Count == 1, "1명");
            AssertNear(20f * RaceAnimation.WinnerArrivalRatio, schedule[0].ArrivalSeconds, "solo arrival");
        });

        Test("레이스 연출: 출전자가 0명이면 빈 목록, 예외 없음", () =>
        {
            var schedule = RaceAnimation.BuildSchedule(new List<RaceSimulator.Result>(), 24f);
            Assert(schedule.Count == 0, "빈 목록");
        });

        Test("레이스 연출: duration이 범위를 벗어나면 Min/MaxDurationSeconds로 방어적으로 잘린다", () =>
        {
            var results = new List<RaceSimulator.Result>
            {
                new RaceSimulator.Result { Id = "a", Time = 10f, Rank = 1 },
                new RaceSimulator.Result { Id = "b", Time = 11f, Rank = 2 },
            };
            var tooShort = RaceAnimation.BuildSchedule(results, 1f);
            Assert(tooShort[1].ArrivalSeconds <= RaceAnimation.MinDurationSeconds, "너무 짧으면 최소값으로 잘림");
            var tooLong = RaceAnimation.BuildSchedule(results, 10_000f);
            Assert(tooLong[1].ArrivalSeconds <= RaceAnimation.MaxDurationSeconds, "너무 길면 최대값으로 잘림");
        });

        Test("레이스 연출: duration이 0이거나 음수여도 예외 없이 MinDurationSeconds로 방어적으로 잘린다", () =>
        {
            var results = new List<RaceSimulator.Result>
            {
                new RaceSimulator.Result { Id = "a", Time = 10f, Rank = 1 },
                new RaceSimulator.Result { Id = "b", Time = 11f, Rank = 2 },
            };
            var zero = RaceAnimation.BuildSchedule(results, 0f);
            Assert(zero[1].ArrivalSeconds <= RaceAnimation.MinDurationSeconds, "0은 최소값으로 잘림");
            var negative = RaceAnimation.BuildSchedule(results, -50f);
            Assert(negative[1].ArrivalSeconds <= RaceAnimation.MinDurationSeconds, "음수도 최소값으로 잘림");
        });

        Test("레이스 연출: 출전자가 많아 최소 간격 총합이 duration을 넘기면(재스케일 경로) 순서·범위가 그대로 유지된다", () =>
        {
            // 40명, 기록이 전부 같아 등수 간격으로 균등 배분 — 39칸 * MinGapSeconds(0.8) = 31.2초로
            // MinDurationSeconds(20) 하나만으로는 못 채우니 재스케일(scale = duration/pos[n-1]) 경로를 탄다.
            var results = new List<RaceSimulator.Result>();
            for (int i = 0; i < 40; i++) results.Add(new RaceSimulator.Result { Id = $"e{i}", Time = 10f, Rank = i + 1 });
            var schedule = RaceAnimation.BuildSchedule(results, RaceAnimation.MinDurationSeconds);

            Assert(schedule[0].ArrivalSeconds > 0f, $"재스케일 후에도 1등 도착이 0보다 큼 ({schedule[0].ArrivalSeconds:F3})");
            for (int i = 1; i < schedule.Count; i++)
                Assert(schedule[i].ArrivalSeconds > schedule[i - 1].ArrivalSeconds, $"재스케일 후에도 엄격히 증가 [{i}]");
            Assert(schedule[^1].ArrivalSeconds <= RaceAnimation.MinDurationSeconds + 0.001f, $"꼴찌도 duration 안쪽 ({schedule[^1].ArrivalSeconds:F3})");
        });

        Test("레이스 연출: Time이 Rank와 역행하는(정의 밖) 입력이어도 도착 순서는 여전히 Rank 순이고 엄격히 증가한다", () =>
        {
            // BuildSchedule은 순위(Rank)만 믿고 Time은 화면 속도 배분에만 쓴다. Time이 Rank와
            // 어긋나는 데이터(예: 판정 로직 버그, 저장 데이터 손상)가 들어와도 totalRealGap이
            // 음수가 될 뿐 예외는 안 나야 하고, 최소 간격 강제 루프가 앞에서부터만 밀기 때문에
            // 결과는 항상 Rank 순 + 엄격히 증가 상태여야 한다.
            var results = new List<RaceSimulator.Result>
            {
                new RaceSimulator.Result { Id = "a", Time = 50f, Rank = 1 },
                new RaceSimulator.Result { Id = "b", Time = 30f, Rank = 2 },
                new RaceSimulator.Result { Id = "c", Time = 10f, Rank = 3 },
            };
            var schedule = RaceAnimation.BuildSchedule(results, 24f);
            Assert(schedule.Count == 3, "인원 수 그대로");
            Assert(schedule[0].Id == "a" && schedule[1].Id == "b" && schedule[2].Id == "c", "Rank 순서 그대로(Time 역행 무시)");
            for (int i = 1; i < schedule.Count; i++)
                Assert(schedule[i].ArrivalSeconds > schedule[i - 1].ArrivalSeconds, $"역행 데이터여도 엄격히 증가 [{i}]");
            foreach (var a in schedule)
            {
                Assert(a.ArrivalSeconds > 0f, $"{a.Id} > 0 ({a.ArrivalSeconds:F2})");
                Assert(a.ArrivalSeconds <= 24f, $"{a.Id} <= duration ({a.ArrivalSeconds:F2})");
            }
        });

        // D03-M: 세이브 데이터가 직렬화→역직렬화를 거쳐도 값을 그대로 보존하는지.
        // 실제 게임은 Unity의 JsonUtility로 쓰지만(Assets/Scripts/Save/SaveService.cs),
        // Core.Tests는 Unity 없이 도는 콘솔이라 .NET 기본 System.Text.Json으로 같은 걸 확인한다.
        // 둘 다 "public 필드를 그대로 직렬화"하는 방식이라 구조가 같으면 결과도 같다.
        Test("세이브: 직렬화→역직렬화 라운드트립이 값을 그대로 보존한다", () =>
        {
            var original = new SaveData
            {
                CurrentPlanetId = "ruby",
                LastSeenUnixSeconds = 1234567890L,
                Rig = new MiningRigSave { ToolLevel = 5, CargoLevel = 3, EngineLevel = 2, DetectorLevel = 1, RefineryLevel = 0 },
                RawMinerals = 12.5f,
                RefinedMinerals = 3.25f,
                OwnedPartIds = new List<string> { "q_engine_c", "q_tire_c" },
                EquippedPartIds = new List<string> { "q_engine_c", "", "", "", "", "" },
            };

            // SaveData는 필드로만 되어 있다(JsonUtility가 프로퍼티를 못 읽어서). System.Text.Json은
            // 기본이 프로퍼티만 보므로 IncludeFields를 켜야 같은 조건으로 비교된다.
            var options = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
            var json = System.Text.Json.JsonSerializer.Serialize(original, options);
            var restored = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json, options);

            Assert(restored != null, "역직렬화 결과가 null이 아니다");
            Assert(restored!.Version == original.Version, "버전 보존");
            Assert(restored.CurrentPlanetId == original.CurrentPlanetId, "행성 id 보존");
            Assert(restored.LastSeenUnixSeconds == original.LastSeenUnixSeconds, "마지막 저장 시각 보존");
            Assert(restored.Rig.ToolLevel == original.Rig.ToolLevel
                && restored.Rig.CargoLevel == original.Rig.CargoLevel
                && restored.Rig.EngineLevel == original.Rig.EngineLevel, "채굴차 레벨 보존");
            AssertNear(restored.RawMinerals, original.RawMinerals, "원석");
            AssertNear(restored.RefinedMinerals, original.RefinedMinerals, "정제 광물");
            Assert(restored.OwnedPartIds.SequenceEqual(original.OwnedPartIds), "보유 부품 목록 보존");
            Assert(restored.EquippedPartIds.SequenceEqual(original.EquippedPartIds), "장착 부품 목록 보존(빈 슬롯 포함)");
        });

        Test("세이브: MiningRigSave ↔ MiningRig 변환이 값을 그대로 옮긴다", () =>
        {
            var rig = new MiningRig { ToolLevel = 7, CargoLevel = 4, EngineLevel = 3, DetectorLevel = 2, RefineryLevel = 1 };
            var save = MiningRigSave.FromCore(rig);
            var back = save.ToCore();
            Assert(back.ToolLevel == rig.ToolLevel && back.CargoLevel == rig.CargoLevel
                && back.EngineLevel == rig.EngineLevel && back.DetectorLevel == rig.DetectorLevel
                && back.RefineryLevel == rig.RefineryLevel, "왕복 후 값 동일");
        });

        // L-04: 오프라인 발견 목록 — 광물(Offline)과 보물(Discover)을 한 번에 계산한다.
        Test("오프라인 발견: 화물칸 상한 안쪽이면 그냥 Discover와 같다", () =>
        {
            var rig = new MiningRig { CargoLevel = 10 }; // 12시간 상한
            var defs = DefaultData.QuartzTreasureDefs();
            var elapsed = 3600.0; // 1시간, 상한보다 훨씬 짧다
            var direct = ExplorationSimulator.Discover(rig, quartz, elapsed, defs, seed: 55);
            var combined = ExplorationSimulator.DiscoverOffline(rig, quartz, elapsed, defs, seed: 55);
            Assert(combined.Treasures.Count == direct.Count, $"발견 수 동일 {combined.Treasures.Count} == {direct.Count}");
            for (var i = 0; i < direct.Count; i++) Assert(combined.Treasures[i].DefId == direct[i].DefId, $"[{i}] id 동일");
        });

        Test("오프라인 발견: 화물칸 상한을 넘긴 시간은 광물처럼 발견도 잘린다", () =>
        {
            var rig = new MiningRig { CargoLevel = 1 }; // 4시간 상한
            var defs = DefaultData.QuartzTreasureDefs();
            var uncappedSeconds = 20 * 3600.0; // 20시간치를 한 번에 줌
            var combined = ExplorationSimulator.DiscoverOffline(rig, quartz, uncappedSeconds, defs, seed: 55);
            var cappedDirect = ExplorationSimulator.Discover(rig, quartz, 4 * 3600.0, defs, seed: 55);
            Assert(Math.Abs(combined.Mining.HoursCounted - 4f) < 0.001f, $"인정 시간 4h {combined.Mining.HoursCounted}");
            Assert(combined.Treasures.Count == cappedDirect.Count,
                $"20시간을 통째로 줘도 발견은 4시간치({cappedDirect.Count})만 인정 — 실제 {combined.Treasures.Count}");
        });

        Test("2026-09-19: DiscoverOffline도 forceFullRefine을 그대로 MiningSimulator.Offline에 전달한다", () =>
        {
            var rig = new MiningRig { CargoLevel = 1, RefineryLevel = 0 }; // 4시간 상한, 제련소 없음
            var defs = DefaultData.QuartzTreasureDefs();
            var uncappedSeconds = 20 * 3600.0;

            // 기존 6인자(chancePerCycle까지) 호출은 forceFullRefine 기본값 false와 완전히 같다(회귀 없음).
            var withoutFlag = ExplorationSimulator.DiscoverOffline(rig, quartz, uncappedSeconds, defs, seed: 55);
            var explicitFalse = ExplorationSimulator.DiscoverOffline(rig, quartz, uncappedSeconds, defs, seed: 55, forceFullRefine: false);
            AssertNear(withoutFlag.Mining.HoursCounted, explicitFalse.Mining.HoursCounted, "기본값==명시적 false HoursCounted 회귀");
            Assert(withoutFlag.Treasures.Count == explicitFalse.Treasures.Count, "기본값==명시적 false 발견 수 회귀");

            // forceFullRefine=true면 제련소 0레벨이라도 화물칸이 절대 안 차서 20시간 전부 인정된다.
            var forced = ExplorationSimulator.DiscoverOffline(rig, quartz, uncappedSeconds, defs, seed: 55, forceFullRefine: true);
            AssertNear(20f, forced.Mining.HoursCounted, "강제 전체 정제면 20시간 전부 인정(낭비 없음)");
            AssertNear(0f, forced.Mining.HoursWasted, "강제 전체 정제면 낭비 시간 0");

            // 인정 시간이 늘어난 만큼 그 시간 동안의 발견도 4시간치보다 많거나 같아야 한다.
            var cappedDirect = ExplorationSimulator.Discover(rig, quartz, 4 * 3600.0, defs, seed: 55);
            var fullDirect = ExplorationSimulator.Discover(rig, quartz, 20 * 3600.0, defs, seed: 55);
            Assert(forced.Treasures.Count == fullDirect.Count,
                $"강제 전체 정제 발견 수({forced.Treasures.Count})는 20시간 그대로({fullDirect.Count})와 같다");
            Assert(forced.Treasures.Count >= cappedDirect.Count,
                "강제 전체 정제 발견 수는 4시간치보다 적을 수 없다");
        });

        // D07-N: 오프라인 보상 화면이 쓰는 두 조각 — 발견 목록에 딸려오는 MineralValue와
        // "지금 캘 수 있는 것만" 합산하는 MineableValue.
        Test("탐험: 발견 목록의 MineralValue가 defs의 값과 같다", () =>
        {
            var rig = new MiningRig { ToolLevel = 30 }; // 전부 캘 수 있게 도구 최고 레벨
            var defs = DefaultData.QuartzTreasureDefs();
            var found = ExplorationSimulator.Discover(rig, quartz, 50000, defs, seed: 3);
            Assert(found.Count > 0, "이 seed·시간이면 최소 하나는 나온다(테스트 전제)");
            foreach (var t in found)
            {
                var def = defs.Single(d => d.Id == t.DefId);
                Assert(Math.Abs(t.MineralValue - def.MineralValue) < 0.001f,
                    $"{t.DefId} MineralValue {t.MineralValue} == def {def.MineralValue}");
            }
        });

        Test("탐험: MineableValue는 CanMineNow인 것만 더한다", () =>
        {
            var treasures = new List<TreasureDiscovery>
            {
                new TreasureDiscovery { DefId = "a", CanMineNow = true, MineralValue = 10f },
                new TreasureDiscovery { DefId = "b", CanMineNow = false, MineralValue = 999f },
                new TreasureDiscovery { DefId = "c", CanMineNow = true, MineralValue = 5f },
            };
            var total = ExplorationSimulator.MineableValue(treasures);
            AssertNear(15f, total, "캘 수 있는 것만 합산(10+5, 999는 제외)");
        });

        Test("탐험: MineableValue는 빈 목록이면 0", () =>
        {
            AssertNear(0f, ExplorationSimulator.MineableValue(new List<TreasureDiscovery>()), "빈 목록 합계 0");
        });

        Test("탐험: 캘 보물 정의가 비어 있으면 사이클이 많아도 빈 목록(예외 없음)", () =>
        {
            var rig = new MiningRig();
            var found = ExplorationSimulator.Discover(rig, quartz, 100000, new List<TreasureDef>(), seed: 1);
            Assert(found.Count == 0, $"빈 정의 목록 → 발견 0 ({found.Count})");
        });

        Test("탐험: 경과 시간 0이면 발견도 0", () =>
        {
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var found = ExplorationSimulator.Discover(rig, quartz, 0, defs, seed: 1);
            Assert(found.Count == 0, $"경과 0초 → 발견 0 ({found.Count})");
        });

        Test("탐험: 음수 경과 시간을 직접 줘도 사이클이 0으로 방어돼 예외 없음", () =>
        {
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var found = ExplorationSimulator.Discover(rig, quartz, -500, defs, seed: 1);
            Assert(found.Count == 0, $"음수 경과 → 발견 0 ({found.Count})");
        });

        Test("탐험: 보물 정의가 딱 하나면 그 하나만 나온다", () =>
        {
            var rig = new MiningRig();
            var one = new List<TreasureDef> { DefaultData.QuartzTreasureDefs()[0] };
            var found = ExplorationSimulator.Discover(rig, quartz, 50000, one, seed: 1, chancePerCycle: 1f);
            Assert(found.Count > 0, "확률 100%면 최소 하나는 나온다");
            Assert(found.TrueForAll(t => t.DefId == one[0].Id), "선택지가 하나뿐이면 전부 그것");
        });

        Test("탐험: chancePerCycle 0이면 사이클이 아무리 많아도 발견 0", () =>
        {
            // DeterministicRandom.NextFloat()은 [0,1) — 0도 나올 수 있으니 ">= 0"은 항상 참,
            // 즉 매 사이클 continue. 실제 게임에서 쓸 값은 아니지만 Discover 자신의 방어를 못 박아 둔다.
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var found = ExplorationSimulator.Discover(rig, quartz, 100000, defs, seed: 1, chancePerCycle: 0f);
            Assert(found.Count == 0, $"확률 0% → 사이클 수와 무관하게 발견 0 ({found.Count})");
        });

        Test("탐험: CyclesIn은 경과 0초에서 0", () =>
        {
            var rig = new MiningRig();
            AssertNear(0f, ExplorationSimulator.CyclesIn(rig, quartz, 0), "0초 → 0사이클");
        });

        Test("탐험: CyclesIn은 음수 경과에서도 0(예외 없음)", () =>
        {
            var rig = new MiningRig();
            AssertNear(0f, ExplorationSimulator.CyclesIn(rig, quartz, -100), "음수 경과 → 0사이클");
        });

        Test("탐험: CyclesIn은 VeinCount 0에서도 나눗셈 예외 없이 VeinCount 1과 같다", () =>
        {
            // travelPerVein = Circumference / Max(1, VeinCount) — 정의 밖(0) 광맥 밀도를 방어하는 자리.
            var rig = new MiningRig();
            var zeroVein = new Planet { Circumference = quartz.Circumference, VeinCount = 0 };
            var oneVein = new Planet { Circumference = quartz.Circumference, VeinCount = 1 };
            var a = ExplorationSimulator.CyclesIn(rig, zeroVein, 10000);
            var b = ExplorationSimulator.CyclesIn(rig, oneVein, 10000);
            AssertNear(b, a, $"VeinCount 0은 1과 같은 값으로 방어됨 {a} == {b}");
        });

        Test("탐험: 뽑기 가중치는 목록 앞쪽(흔한 등급)일수록 더 자주 나온다 — C:B:A:S ≈ 4:3:2:1", () =>
        {
            // PickWeighted 자체는 private라 직접 못 부르니 Discover로 큰 표본을 뽑아 분포로 검증한다.
            // QuartzTreasureDefs는 C/B/A/S 순(흔한 것부터)이라 가중치가 4,3,2,1 — 총 10 중 40/30/20/10%.
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var found = ExplorationSimulator.Discover(rig, quartz, 2_000_000, defs, seed: 42, chancePerCycle: 1f);
            Assert(found.Count > 1000, $"표본이 통계적으로 충분히 커야 한다({found.Count}개)");

            var counts = new Dictionary<string, int>();
            foreach (var t in found)
                counts[t.DefId] = counts.GetValueOrDefault(t.DefId) + 1;
            var c = counts.GetValueOrDefault("q_treasure_c");
            var b = counts.GetValueOrDefault("q_treasure_b");
            var a = counts.GetValueOrDefault("q_treasure_a");
            var s = counts.GetValueOrDefault("q_treasure_s");
            Assert(c > b && b > a && a > s, $"흔한 순서가 뒤집혔다 — C={c} B={b} A={a} S={s}");

            var total = (float)found.Count;
            Assert(Math.Abs(c / total - 0.40f) < 0.05f, $"C 비율이 40%에서 5%p 넘게 벗어남 ({c / total:P1})");
            Assert(Math.Abs(b / total - 0.30f) < 0.05f, $"B 비율이 30%에서 5%p 넘게 벗어남 ({b / total:P1})");
            Assert(Math.Abs(a / total - 0.20f) < 0.05f, $"A 비율이 20%에서 5%p 넘게 벗어남 ({a / total:P1})");
            Assert(Math.Abs(s / total - 0.10f) < 0.05f, $"S 비율이 10%에서 5%p 넘게 벗어남 ({s / total:P1})");
        });

        // D04-N: MiningRunState — MineralsPerHour 공식을 초 단위로 적분한 실시간 루프.
        Test("실시간 채굴: 오래 굴리면 평균 산출이 MineralsPerHour에 수렴한다", () =>
        {
            var rig = new MiningRig { ToolLevel = 3, EngineLevel = 2 };
            var run = new MiningRunState(rig, quartz);
            const float hours = 20f;
            run.Advance(rig, quartz, hours * 3600f);
            var expected = MiningSimulator.MineralsPerHour(rig, quartz) * hours;
            var ratio = run.TotalRawMinerals / expected;
            Assert(ratio > 0.95f && ratio < 1.05f, $"20시간 누적 {run.TotalRawMinerals:F1} ≈ 기대치 {expected:F1} (비율 {ratio:F3})");
        });

        Test("실시간 채굴: 처음엔 이동 중이고, 이동 시간만큼 지나야 채굴 단계로 바뀐다", () =>
        {
            var rig = new MiningRig();
            var run = new MiningRunState(rig, quartz);
            Assert(run.Phase == MiningPhase.Traveling, "시작은 이동 중");
            var travelSeconds = run.PhaseSecondsRemaining;
            var minedBeforeArrival = run.Advance(rig, quartz, travelSeconds * 0.5f);
            Assert(minedBeforeArrival == 0f && run.Phase == MiningPhase.Traveling, "절반만 이동했으면 아직 이동 중, 원석 0");
            run.Advance(rig, quartz, travelSeconds * 0.5f);
            Assert(run.Phase == MiningPhase.MiningVein, "도착하면 채굴 단계로 전환");
        });

        Test("실시간 채굴: 델타를 잘게 나눠도, 한 번에 몰아줘도 누적 원석은 같다", () =>
        {
            var rig = new MiningRig { ToolLevel = 5 };
            const float totalSeconds = 5000f;
            var lump = new MiningRunState(rig, quartz);
            lump.Advance(rig, quartz, totalSeconds);

            var stepped = new MiningRunState(rig, quartz);
            const float dt = 0.1f; // 대략 10fps 프레임 델타
            for (var t = 0f; t < totalSeconds; t += dt) stepped.Advance(rig, quartz, dt);

            var diff = Math.Abs(lump.TotalRawMinerals - stepped.TotalRawMinerals);
            Assert(diff < 0.01f, $"몰아서 {lump.TotalRawMinerals:F3} ≈ 잘게 나눠 {stepped.TotalRawMinerals:F3} (차이 {diff:F4})");
        });

        Test("실시간 채굴: 델타 0은 아무것도 바꾸지 않는다", () =>
        {
            var rig = new MiningRig();
            var run = new MiningRunState(rig, quartz);
            var phaseBefore = run.Phase;
            var remainingBefore = run.PhaseSecondsRemaining;
            var mined = run.Advance(rig, quartz, 0f);
            Assert(mined == 0f, "델타 0이면 원석 0");
            Assert(run.Phase == phaseBefore, "단계도 그대로");
            Assert(run.PhaseSecondsRemaining == remainingBefore, "남은 시간도 그대로");
        });

        Test("실시간 채굴: 음수 델타는 0처럼 방어된다(프레임 델타가 잘못 들어와도 되감기지 않는다)", () =>
        {
            var rig = new MiningRig();
            var run = new MiningRunState(rig, quartz);
            var remainingBefore = run.PhaseSecondsRemaining;
            var mined = run.Advance(rig, quartz, -5f);
            Assert(mined == 0f, "음수 델타는 원석 0");
            Assert(run.PhaseSecondsRemaining == remainingBefore, "남은 시간이 늘어나지 않는다(음수를 그대로 뺐다면 늘어났을 것)");
        });

        Test("실시간 채굴: 델타가 남은 시간과 정확히 같으면 그 즉시 다음 단계로 전환된다(경계값)", () =>
        {
            var rig = new MiningRig();
            var run = new MiningRunState(rig, quartz);
            var travelSeconds = run.PhaseSecondsRemaining;
            run.Advance(rig, quartz, travelSeconds); // 정확히 이동 시간만큼만
            Assert(run.Phase == MiningPhase.MiningVein, "남은 시간과 델타가 같아도(< 아니라 <=) 전환된다");
        });

        Test("실시간 채굴: VeinCount가 0인 행성도 나누기 0 없이 동작한다(Math.Max(1, VeinCount) 방어)", () =>
        {
            var emptyVeinPlanet = new Planet { VeinCount = 0, Circumference = quartz.Circumference };
            var rig = new MiningRig();
            var run = new MiningRunState(rig, emptyVeinPlanet);
            Assert(run.PhaseSecondsRemaining > 0f && !float.IsInfinity(run.PhaseSecondsRemaining),
                $"VeinCount 0이어도 이동 시간이 유한한 값 {run.PhaseSecondsRemaining}");
            var mined = run.Advance(rig, emptyVeinPlanet, 100000f); // 충분히 길게 굴려도 예외/무한루프 없이 끝나야 한다
            Assert(mined >= 0f, "예외 없이 끝남");
        });

        // D06-N: 광맥 위치. 지금까지 광맥은 시간 개념뿐이라 좌표가 없었다 — 화면에 놓으려고
        // VeinLayout(각도)과 MiningRunState.VeinProgress(진행도)를 새로 만들었다. 화면이 속도를
        // 따로 적분하지 않고 이 진행도를 각도로 바꿔 쓰기 때문에, 아래 "정확히 일치" 테스트가
        // 깨지면 채굴차가 광맥을 지나쳐 서거나 광맥 앞이 아닌 곳에서 멈추는 걸로 바로 보인다.
        Test("광맥 배치: 광맥은 대원 위에 같은 간격으로 놓이고, 마지막 광맥이 출발 지점에 온다", () =>
        {
            var step = VeinLayout.AngleStepDegrees(quartz);
            Assert(Math.Abs(step - 360f / quartz.VeinCount) < 1e-4f, $"간격 {step:F3}도 = 360 / {quartz.VeinCount}");
            Assert(Math.Abs(VeinLayout.VeinAngleDegrees(quartz, 0) - step) < 1e-3f, "0번 광맥은 한 칸 앞(출발하자마자 향하는 곳)");
            Assert(Math.Abs(VeinLayout.VeinAngleDegrees(quartz, 3) - step * 4f) < 1e-3f, "3번 광맥은 네 칸 앞");
            var last = VeinLayout.VeinAngleDegrees(quartz, quartz.VeinCount - 1);
            Assert(last < 1e-3f || Math.Abs(last - 360f) < 1e-3f, $"마지막 광맥은 한 바퀴 돌아 출발 지점(0도) — 실제 {last:F4}");
        });

        Test("광맥 배치: 광맥 번호는 몇 바퀴를 돌아도 0..VeinCount-1로 접힌다", () =>
        {
            Assert(VeinLayout.WrapIndex(quartz, 0) == 0, "0번은 0번");
            Assert(VeinLayout.WrapIndex(quartz, quartz.VeinCount) == 0, "한 바퀴 돌면 다시 0번");
            Assert(VeinLayout.WrapIndex(quartz, quartz.VeinCount * 7 + 5) == 5, "일곱 바퀴 뒤 다섯 칸도 5번");
            Assert(VeinLayout.WrapIndex(quartz, -1) == quartz.VeinCount - 1, "음수도 뒤에서부터 접힌다");
            var zeroVein = new Planet { VeinCount = 0, Circumference = quartz.Circumference };
            Assert(VeinLayout.WrapIndex(zeroVein, 3) == 0 && VeinLayout.AngleStepDegrees(zeroVein) == 360f,
                "VeinCount 0이어도 나누기 0 없이 동작(Math.Max(1, ...) 방어)");
            var negVein = new Planet { VeinCount = -5, Circumference = quartz.Circumference };
            Assert(VeinLayout.AngleStepDegrees(negVein) == 360f, "VeinCount가 음수여도 Math.Max(1, ...)로 0과 같은 방어를 받는다(깨진 세이브 대비)");
            Assert(VeinLayout.WrapIndex(negVein, 3) == 0, "음수 VeinCount에서도 WrapIndex가 예외 없이 0을 낸다");
        });

        // Normalize360/ProgressToAngleDegrees는 위 "광맥 도착" 테스트가 간접적으로만 거친다 —
        // 여기서는 두 함수를 직접, 경계값으로 확인한다. 몇 시간을 돌려도 오차가 안 쌓인다는
        // 클래스 주석의 약속이 실제로 지켜지는지가 이 테스트의 핵심이다.
        Test("VeinLayout.Normalize360: 정확히 0/360/-360과 여러 바퀴 뒤에도 0 이상 360 미만으로 접힌다", () =>
        {
            Assert(VeinLayout.Normalize360(0f) == 0f, "0도는 그대로 0도");
            Assert(Math.Abs(VeinLayout.Normalize360(360f)) < 1e-3f, "정확히 360도는 0도(경계값)");
            Assert(Math.Abs(VeinLayout.Normalize360(-360f)) < 1e-3f, "정확히 -360도도 0도");
            Assert(Math.Abs(VeinLayout.Normalize360(359.999f) - 359.999f) < 1e-2f, "359.999도는 그대로(360 바로 아래 경계)");
            Assert(VeinLayout.Normalize360(-0.001f) is var negTiny && negTiny >= 0f && negTiny < 360f,
                $"0도 바로 아래 음수도 0~360 범위 안(실제 {VeinLayout.Normalize360(-0.001f):F4})");
            // 1,000바퀴(=360,090도)에서도 float32(~7자리 정밀도)가 감당하는 오차는 0.1도 미만이다.
            // 처음엔 10만 바퀴(3600만 도)로 써 봤는데 실제로 2도 가까이 어긋나 실패했다 — float32
            // 유효자릿수 한계라 코드 버그가 아니라 테스트 쪽 기대치가 비현실적이었던 것. 값을
            // 낮추고 그 한계를 여기 적어 둔다(게임에서 한 광맥을 십만 바퀴 돌 일은 없다).
            var manyLaps = VeinLayout.Normalize360(360f * 1_000f + 90f);
            Assert(Math.Abs(manyLaps - 90f) < 0.1f, $"천 바퀴를 돌아도 나머지 각도는 그대로(실제 {manyLaps:F4})");
            var negManyLaps = VeinLayout.Normalize360(-(360f * 1_000f) - 90f);
            Assert(negManyLaps >= 0f && negManyLaps < 360f, $"음수 방향으로 십만 바퀴를 돌아도 범위 안(실제 {negManyLaps:F2})");
        });

        Test("광맥 도착: 채굴 단계의 채굴차 각도가 그 광맥의 각도와 정확히 일치한다", () =>
        {
            var rig = new MiningRig { ToolLevel = 2, EngineLevel = 3 };
            var run = new MiningRunState(rig, quartz);
            Assert(run.VeinProgress == 0f, "시작은 출발 지점(0칸)");

            // 광맥 여섯 개를 차례로 캐면서, 멈춰 선 자리가 매번 그 광맥 위인지 본다.
            for (var k = 0; k < 6; k++)
            {
                while (run.Phase != MiningPhase.MiningVein) run.Advance(rig, quartz, 0.25f);
                Assert(run.TargetVeinIndex(quartz) == VeinLayout.WrapIndex(quartz, k), $"{k}번 광맥을 캐는 중");
                var rigAngle = VeinLayout.Normalize360(VeinLayout.ProgressToAngleDegrees(quartz, run.VeinProgress));
                var veinAngle = VeinLayout.VeinAngleDegrees(quartz, k);
                Assert(Math.Abs(rigAngle - veinAngle) < 1e-3f, $"{k}번: 채굴차 {rigAngle:F4}도 = 광맥 {veinAngle:F4}도");
                while (run.Phase == MiningPhase.MiningVein) run.Advance(rig, quartz, 0.25f);
            }
        });

        Test("광맥 도착: 이동 중 진행도는 0에서 1로 매끄럽게 차오른다(화면이 그 사이를 보간한다)", () =>
        {
            var rig = new MiningRig();
            var run = new MiningRunState(rig, quartz);
            var travel = run.PhaseTotalSeconds;
            Assert(travel > 0f && Math.Abs(travel - run.PhaseSecondsRemaining) < 1e-4f, "시작 시점엔 전체 길이 = 남은 시간");

            run.Advance(rig, quartz, travel * 0.25f);
            Assert(Math.Abs(run.VeinProgress - 0.25f) < 1e-3f, $"1/4 지점 진행도 {run.VeinProgress:F4}");
            run.Advance(rig, quartz, travel * 0.5f);
            Assert(Math.Abs(run.VeinProgress - 0.75f) < 1e-3f, $"3/4 지점 진행도 {run.VeinProgress:F4}");
            run.Advance(rig, quartz, travel * 0.25f);
            Assert(run.Phase == MiningPhase.MiningVein && run.VeinProgress == 1f, "도착하면 정확히 1칸 — 소수점이 남아 광맥을 살짝 지나치면 안 된다");
        });

        Test("광맥 도착: 몇 시간을 돌려도 진행도와 캔 광맥 수가 어긋나지 않는다(오차 누적 없음)", () =>
        {
            var rig = new MiningRig { ToolLevel = 4, EngineLevel = 4 };
            var lump = new MiningRunState(rig, quartz);
            lump.Advance(rig, quartz, 3f * 3600f);

            var stepped = new MiningRunState(rig, quartz);
            for (var t = 0; t < 3 * 3600 * 20; t++) stepped.Advance(rig, quartz, 0.05f); // 20fps로 3시간

            Assert(lump.VeinsMined == stepped.VeinsMined, $"몰아서 {lump.VeinsMined}개 = 잘게 나눠 {stepped.VeinsMined}개");
            Assert(Math.Abs(lump.VeinProgress - stepped.VeinProgress) < 0.01f,
                $"진행도도 같다 — 몰아서 {lump.VeinProgress:F3}, 잘게 {stepped.VeinProgress:F3}");
            Assert(lump.VeinsMined > 100, $"3시간이면 광맥을 충분히 많이 캔다({lump.VeinsMined}개) — 오차가 쌓일 기회가 있었다는 뜻");
        });

        // D06-M: 극점 근처 광맥 배치 균등성. VeinLayout은 각도만 내놓고, 실제 3D 위치는
        // SurfaceMover.SetOrbitAngle이 고정축(orbitAxis) 둘레로 방향 벡터를 Quaternion.AngleAxis로
        // 돌려서 만든다(코어가 아니라 Assets/Scripts라 여기서 직접 부를 순 없다) — 그래서 같은 수식을
        // Rodrigues 회전 공식으로 이 테스트 안에 재현해, 광맥 사이 3D 거리(현 거리)가 극점 근처에서도
        // 정말 균등한지를 코어의 VeinLayout 각도값을 입력 삼아 확인한다.
        // 결론(D06-M 답): 회전은 원점을 보존하는 등거리 변환(isometry)이라 축이 어디를 향하든,
        // 시작점이 축의 "극점"(축과 거의 평행한 방향, 즉 원 반지름이 0에 가까운 자리)이 아닌 한
        // 균등하게 남는다 — 심지어 시작점을 축과 수직인 진짜 극점 위에 두어도(아래 테스트) 안 깨진다.
        // SurfaceMover.ApplyTransform의 "접선이 0에 가까울 때" 방어 코드는 방향(회전, LookRotation)에만
        // 관여할 뿐 위치(광맥 간격)에는 영향이 없다 — 그래서 그 방어 코드가 있든 없든 이 결과는 같다.
        Test("D06-M: 광맥 3D 위치는 궤도축이 어느 방향이든 항상 등간격이다(적도 축)", () =>
        {
            AssertUniformVeinSpacing(quartz, orbitAxis: (0.2, 1, 0), startDirection: (1, 0, 0), radius: 20);
        });

        Test("D06-M: 궤도축이 세계 위(0,1,0)와 수직이라 대원이 남북극을 그대로 지나가도 등간격이다", () =>
        {
            // orbitAxis=(1,0,0)이면 회전면이 (0,1,0)-(0,0,1) 평면이라 원이 정확히 두 극점을 지난다.
            AssertUniformVeinSpacing(quartz, orbitAxis: (1, 0, 0), startDirection: (0, 0, 1), radius: 20);
        });

        Test("D06-M: 시작점을 극점 바로 위에 둬도(최악의 경우) 광맥 간격은 여전히 등간격이다", () =>
        {
            // 시작 방향 자체를 세계 위(극점)로 잡은 극단값. 광맥 중 일부가 실제 극점 근처를 지나간다.
            AssertUniformVeinSpacing(quartz, orbitAxis: (1, 0, 0), startDirection: (0, 1, 0), radius: 20);
        });

        Test("D06-M: 광맥 개수가 적은 행성(8개, 라피스)도, 광맥 개수가 홀수(13개)여도 등간격이다", () =>
        {
            var lapis = new Planet { VeinCount = 8, Circumference = 2200 };
            var oddCount = new Planet { VeinCount = 13, Circumference = 1000 };
            AssertUniformVeinSpacing(lapis, orbitAxis: (1, 0, 0), startDirection: (0, 1, 0), radius: 20);
            AssertUniformVeinSpacing(oddCount, orbitAxis: (0.3, 0.7, 0.1), startDirection: (0, 1, 0), radius: 20);
        });

        // 야간 세션(9/17): RigSpeed/YieldPerVein/SecondsPerVein은 지금까지 정상 범위(레벨 1~10대)
        // 값으로만, 그것도 MineralsPerHour 등을 통해 간접적으로만 검증돼 있었다. 세이브 조작이나
        // 미래의 버그로 이 세 함수에 범위 밖 값(0·음수·비정상적으로 큰 레벨)이 들어와도 예외나
        // NaN 없이 상식적인 값을 내는지 직접 확인한다.
        Test("RigSpeed: 엔진 레벨이 비정상적으로 낮아도(음수) 속도는 0 밑으로 안 내려가고 NaN이 없다", () =>
        {
            // 1.12^(레벨-1)이 float 최소 서브노멀보다 작아지면 0으로 언더플로한다 — 그 자체는
            // 이상 없다(음수 속도가 아니라 정지에 가까운 값). MineralsPerHour까지 이어져도
            // NaN이 안 나는지가 진짜 확인 포인트(아래 "고장난 장비" 테스트에서 한 번 더 확인).
            var speed = MiningSimulator.RigSpeed(new MiningRig { EngineLevel = -1000 }, quartz);
            Assert(speed >= 0f, $"속도가 음수는 아님 {speed}");
            Assert(!float.IsNaN(speed), $"NaN 아님 {speed}");
        });

        Test("RigSpeed: 엔진 레벨이 정상 범위(1~10)를 훨씬 넘어도(100) 유한하고 계속 증가한다", () =>
        {
            var normal = MiningSimulator.RigSpeed(new MiningRig { EngineLevel = 10 }, quartz);
            var huge = MiningSimulator.RigSpeed(new MiningRig { EngineLevel = 100 }, quartz);
            Assert(!float.IsNaN(huge) && !float.IsInfinity(huge), $"유한함 {huge}");
            Assert(huge > normal, $"레벨이 높을수록 여전히 더 빠름 {huge} > {normal}");
        });

        Test("RigSpeed: Roughness가 0~1 범위를 벗어나도(음수·1 초과) 지형 배율이 60~100% 안에서 방어된다", () =>
        {
            var flatBase = MiningSimulator.RigSpeed(new MiningRig(), new Planet { Roughness = 0f });
            var overNegative = MiningSimulator.RigSpeed(new MiningRig(), new Planet { Roughness = -5f });
            var overPositive = MiningSimulator.RigSpeed(new MiningRig(), new Planet { Roughness = 5f });
            Assert(overNegative == flatBase, $"음수 Roughness는 0과 같은 취급이어야 함 {overNegative} == {flatBase}");
            Assert(overPositive >= flatBase * 0.6f - 0.001f, $"1을 넘어도 최대 40% 감속에서 멈춤 {overPositive}");
        });

        Test("YieldPerVein: 도구 레벨이 0이거나 음수여도 최소 1레벨로 방어되고 매장량을 넘지 않는다", () =>
        {
            var zero = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = 0 }, quartz);
            var negative = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = -50 }, quartz);
            var lvl1 = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = 1 }, quartz);
            Assert(zero == lvl1 && negative == lvl1, $"0·음수 레벨 모두 1레벨과 같은 값 {zero}/{negative}/{lvl1}");
            Assert(zero <= quartz.VeinYield, "매장량 상한 안");
        });

        Test("YieldPerVein: 도구 레벨이 비정상적으로 높아도 매장량(VeinYield) 상한을 절대 넘지 않는다", () =>
        {
            var huge = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = 500 }, quartz);
            Assert(huge <= quartz.VeinYield, $"매장량 상한 안 {huge} <= {quartz.VeinYield}");
            Assert(!float.IsNaN(huge), "NaN 아님");
        });

        // P-02 (2026-09-17): 10레벨×1.5 점프 두 번 → 5레벨×TierJumpMultiplier 점프 다섯 번.
        // 매장량 상한에 안 걸리게 아주 큰 VeinYield 행성으로 우발적 캡을 피해서 확인한다.
        var uncapped = new Planet { VeinYield = 1_000_000f, Circumference = 1000, VeinCount = 1 };

        Test("YieldPerVein: 5레벨 경계(6·11·16·21·26)에서만 추가로 점프하고 그 사이는 순수 지수 성장이다", () =>
        {
            foreach (var boundary in new[] { 6, 11, 16, 21, 26 })
            {
                var before = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = boundary - 1 }, uncapped);
                var after = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = boundary }, uncapped);
                var withJump = before * 1.15f * MiningSimulator.TierJumpMultiplier;
                AssertNear(withJump, after, $"{boundary - 1}→{boundary}레벨은 지수 성장 + 티어 점프");
            }
            // 경계가 아닌 곳(예: 7→8)은 지수 성장만 있어야 한다.
            var mid = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = 7 }, uncapped);
            var midNext = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = 8 }, uncapped);
            AssertNear(mid * 1.15f, midNext, "7→8레벨은 티어 점프 없이 지수 성장만");
        });

        Test("YieldPerVein: 5레벨 점프 다섯 번(레벨 30)이 옛 10레벨 점프 두 번과 최종 배율이 같다(끝값 보존)", () =>
        {
            var newScheme = MiningSimulator.YieldPerVein(new MiningRig { ToolLevel = 30 }, uncapped);
            var oldSchemeTierMultiplier = MathF.Pow(1.5f, 2f); // 옛 (30-1)/10 = 2번 점프
            var oldScheme = 2f * MathF.Pow(1.15f, 29) * oldSchemeTierMultiplier;
            // 값 자체가 250대라 AssertNear의 절대 오차 0.001은 부동소수점 곱셈 경로 차이만으로도
            // 넘을 수 있다 — 상대 오차(0.1%)로 비교한다.
            var relativeError = Math.Abs(oldScheme - newScheme) / oldScheme;
            Assert(relativeError < 0.001f, $"30레벨 산출은 예전과 사실상 동일해야 함 {oldScheme} == {newScheme} (오차 {relativeError:P3})");
        });

        Test("SecondsPerVein: 도구 레벨이 비정상적으로 높아도 최소 3초 밑으로 안 내려간다", () =>
        {
            var seconds = MiningSimulator.SecondsPerVein(new MiningRig { ToolLevel = 500 });
            Assert(seconds >= 3f, $"최소 3초 방어 {seconds}");
        });

        Test("SecondsPerVein: 도구 레벨이 비정상적으로 낮아도(음수) 값이 유한하거나, 무한이어도 예외 없이 처리된다", () =>
        {
            var seconds = MiningSimulator.SecondsPerVein(new MiningRig { ToolLevel = -1000 });
            Assert(!float.IsNaN(seconds), $"NaN 아님 {seconds}");
            Assert(seconds >= 20f, $"고장난 도구는 기본 20초보다 오래 걸려야 함(방향성) {seconds}");
        });

        Test("실시간 채굴: 극단적으로 고장난 장비(음수 레벨 전부)로도 오래 굴리면 예외·NaN 없이 끝난다", () =>
        {
            var brokenRig = new MiningRig { ToolLevel = -1000, EngineLevel = -1000, CargoLevel = -1000 };
            var run = new MiningRunState(brokenRig, quartz);
            var mined = run.Advance(brokenRig, quartz, 100000f);
            Assert(mined >= 0f && !float.IsNaN(mined), $"음수 없이, NaN 없이 {mined}");

            var offline = MiningSimulator.Offline(brokenRig, quartz, 3600 * 24);
            Assert(!float.IsNaN(offline.Minerals) && !float.IsNaN(offline.RefinedGained),
                "고장난 장비의 오프라인 결과도 NaN 없음");
            Assert(offline.Minerals >= 0f && offline.RefinedGained >= 0f, "음수 산출 없음");
        });

        // D02-M: docs/design/balance/*.csv가 DefaultData.cs와 값이 같은지. 지금은 CSV가
        // DefaultData를 그대로 베낀 것이지만, 앞으로 CSV를 기준으로 바꿀 때 둘이 갈라지면
        // 여기서 바로 잡힌다.
        Test("밸런스 CSV: 행성 표가 DefaultData와 일치한다", () =>
        {
            var parsed = BalanceCsv.ParsePlanets(File.ReadAllText(BalancePath("planets.csv")));
            var expected = DefaultData.Planets();
            Assert(parsed.Count == expected.Count, $"행성 수 {parsed.Count} == {expected.Count}");
            for (var i = 0; i < expected.Count; i++) AssertPlanetEquals(expected[i], parsed[i]);
        });

        Test("밸런스 CSV: 코스 표가 DefaultData와 일치한다", () =>
        {
            var parsed = BalanceCsv.ParseCourses(File.ReadAllText(BalancePath("courses.csv")));
            var expected = DefaultData.QuartzCourses();
            Assert(parsed.Count == expected.Count, $"코스 수 {parsed.Count} == {expected.Count}");
            for (var i = 0; i < expected.Count; i++) AssertCourseEquals(expected[i], parsed[i]);
        });

        Test("밸런스 CSV: 부품 표(C/B/A/S 20종)가 DefaultData와 일치한다 (2026-09-22 P-07 CSV 동기화)", () =>
        {
            var parsed = BalanceCsv.ParseParts(File.ReadAllText(BalancePath("parts.csv")));
            var expected = new List<Part>();
            expected.AddRange(DefaultData.QuartzStarterParts());
            expected.AddRange(DefaultData.QuartzAdvancedParts());
            expected.AddRange(DefaultData.QuartzEpicParts());
            expected.AddRange(DefaultData.QuartzLegendaryParts());
            Assert(parsed.Count == expected.Count, $"부품 수 {parsed.Count} == {expected.Count}");
            for (var i = 0; i < expected.Count; i++) AssertPartEquals(expected[i], parsed[i]);
        });

        // 위 세 테스트는 실제 balance/*.csv 파일이 DefaultData와 같은지만 본다.
        // 파서 자체(BalanceCsv.Rows)의 경계 동작은 실제 파일에 없는 모양이라 따로 짧은 CSV로 확인한다.
        Test("밸런스 CSV: '#' 주석 줄과 빈 줄은 건너뛴다", () =>
        {
            var csv = "id,nameKo,order\n# 주석\n\nquartz,쿼츠,1\n\n#끝\n";
            var parsed = BalanceCsv.ParsePlanets(csv);
            Assert(parsed.Count == 1, $"행 1개만 파싱됨 {parsed.Count}");
            Assert(parsed[0].Id == "quartz" && parsed[0].Order == 1, "값도 정상");
        });

        Test("밸런스 CSV: 헤더 순서가 바뀌어도 이름으로 찾아 값이 맞게 들어간다", () =>
        {
            var csv = "order,id,nameKo\n2,sapphire,사파이어\n";
            var parsed = BalanceCsv.ParsePlanets(csv);
            Assert(parsed[0].Id == "sapphire" && parsed[0].NameKo == "사파이어" && parsed[0].Order == 2,
                $"헤더 순서와 무관하게 매핑됨 id={parsed[0].Id} order={parsed[0].Order}");
        });

        Test("밸런스 CSV: 행의 칸이 헤더보다 모자라면 남은 필드는 기본값(빈 문자열/0)", () =>
        {
            var csv = "id,nameKo,order\nquartz\n";
            var parsed = BalanceCsv.ParsePlanets(csv);
            Assert(parsed.Count == 1, "그래도 행 1개는 만들어짐");
            Assert(parsed[0].Id == "quartz", "첫 칸은 채워짐");
            Assert(parsed[0].NameKo == "" && parsed[0].Order == 0, "모자란 칸은 빈 문자열/0으로 방어");
        });

        Test("밸런스 CSV: 셀 앞뒤 공백은 trim되고, 숫자 칸이 비어 있으면 0", () =>
        {
            var csv = "id, nameKo ,order,circumference\n topaz , 토파즈 ,3,\n";
            var parsed = BalanceCsv.ParsePlanets(csv);
            Assert(parsed[0].Id == "topaz" && parsed[0].NameKo == "토파즈", $"공백 trim됨 '{parsed[0].Id}' '{parsed[0].NameKo}'");
            Assert(parsed[0].Circumference == 0f, "빈 실수 칸은 0");
        });

        Test("밸런스 CSV: 헤더만 있고 데이터 행이 없으면 예외 없이 빈 리스트", () =>
        {
            Assert(BalanceCsv.ParsePlanets("id,nameKo,order\n").Count == 0, "헤더뿐이면 0행");
            Assert(BalanceCsv.ParsePlanets("").Count == 0, "빈 문자열도 0행(헤더조차 없음)");
        });

        Test("밸런스 CSV: 행의 칸이 헤더보다 많으면 초과 칸은 조용히 무시된다", () =>
        {
            var csv = "id,nameKo,order\nquartz,쿼츠,1,여기는안읽힘,여기도\n";
            var parsed = BalanceCsv.ParsePlanets(csv);
            Assert(parsed.Count == 1 && parsed[0].Id == "quartz" && parsed[0].Order == 1,
                "정의된 칸까지만 채워지고 나머지는 무시");
        });

        Test("상점 CSV: skuId가 ShopSkuId에 없는 이름이면(오타 등) 조용히 무시하지 않고 예외를 던진다", () =>
        {
            var csv = "skuId,nameKo,priceKrw\nStarterPac,스타터 팩,1100\n"; // 'k' 하나 빠진 오타
            var threw = false;
            try { BalanceCsv.ParseShopItems(csv); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "정의 밖 skuId 문자열은 ArgumentException — 클래스 주석에 적힌 의도(조용히 무시 금지)가 실제로 지켜진다");
        });

        // D05-M: 업그레이드 비용 공식이 레벨이 오를수록 단조 증가하는지, 최대 레벨에서 멈추는지.
        Test("업그레이드: 세 슬롯 모두 레벨이 오를수록 비용이 늘어난다", () =>
        {
            foreach (var slot in new[] { UpgradeSlot.Tool, UpgradeSlot.Cargo, UpgradeSlot.Engine })
            {
                var rig = new MiningRig();
                var prevCost = UpgradeCost.Cost(slot, rig);
                for (var i = 0; i < 5; i++)
                {
                    rig = UpgradeCost.Apply(slot, rig);
                    var cost = UpgradeCost.Cost(slot, rig);
                    Assert(cost > prevCost, $"{slot} 레벨 {UpgradeCost.CurrentLevel(slot, rig)} 비용 {cost:F1} > 이전 {prevCost:F1}");
                    prevCost = cost;
                }
            }
        });

        Test("업그레이드: 최대 레벨에 도달하면 비용이 무한대, Apply해도 그대로 멈춘다", () =>
        {
            var rig = new MiningRig { CargoLevel = UpgradeCost.CargoMaxLevel };
            Assert(UpgradeCost.AtMax(UpgradeSlot.Cargo, rig), "화물칸 10레벨은 최대");
            Assert(float.IsPositiveInfinity(UpgradeCost.Cost(UpgradeSlot.Cargo, rig)), "최대 레벨 비용은 무한대");
            var after = UpgradeCost.Apply(UpgradeSlot.Cargo, rig);
            Assert(after.CargoLevel == UpgradeCost.CargoMaxLevel, $"최대 레벨을 넘지 않는다 {after.CargoLevel}");
        });

        Test("업그레이드: 세이브 조작으로 레벨이 MaxLevel을 넘어 저장돼 있어도 AtMax/Cost/Apply가 예외 없이 최대치 취급", () =>
        {
            foreach (var slot in new[] { UpgradeSlot.Tool, UpgradeSlot.Cargo, UpgradeSlot.Engine, UpgradeSlot.Refinery })
            {
                var over = UpgradeCost.MaxLevel(slot) + 3;
                var rig = slot switch
                {
                    UpgradeSlot.Tool => new MiningRig { ToolLevel = over },
                    UpgradeSlot.Cargo => new MiningRig { CargoLevel = over },
                    UpgradeSlot.Engine => new MiningRig { EngineLevel = over },
                    UpgradeSlot.Refinery => new MiningRig { RefineryLevel = over },
                    _ => throw new ArgumentOutOfRangeException(),
                };
                Assert(UpgradeCost.AtMax(slot, rig), $"{slot} 레벨 {over}(최대 초과)도 AtMax");
                Assert(float.IsPositiveInfinity(UpgradeCost.Cost(slot, rig)), $"{slot} 레벨 {over} 비용도 무한대");
                var after = UpgradeCost.Apply(slot, rig);
                Assert(UpgradeCost.CurrentLevel(slot, after) == over, $"{slot} Apply해도 레벨이 안 바뀐다(더 안 올림) {UpgradeCost.CurrentLevel(slot, after)}");
            }
        });

        // 2026-09-17: 업그레이드·제작이 사흘 동안 아무것도 안 눌리던 회귀를 막는 테스트들.
        // M-02(a62b48c)가 비용을 원석에서 정제 광물로 옮겼는데, 정제량은 제련소 레벨에 비례하고
        // 제련소 시작 레벨이 0이라 정제 광물이 영원히 0이었다. 제련소를 올릴 길도 업그레이드
        // 화면에 없었다(레이스·상자의 무작위 보상뿐). 아래 셋이 그 고리를 하나씩 붙잡는다.

        Test("업그레이드: 제련소는 원석으로 사고 나머지 셋은 정제 광물로 산다", () =>
        {
            Assert(UpgradeCost.IsPaidWithRawMinerals(UpgradeSlot.Refinery), "제련소는 원석");
            foreach (var slot in new[] { UpgradeSlot.Tool, UpgradeSlot.Cargo, UpgradeSlot.Engine })
                Assert(!UpgradeCost.IsPaidWithRawMinerals(slot), $"{slot}은 정제 광물");
        });

        Test("업그레이드: 아무것도 없는 새 채굴차도 제련소 1레벨까지 갈 길이 있다(막다른 길 방지)", () =>
        {
            var rig = new MiningRig();
            var planet = DefaultData.Planets()[0];

            // 출발선: 정제량이 0이라 정제 광물은 저절로 안 는다
            Assert(rig.RefineryLevel == 0, "제련소는 0레벨로 시작한다");
            Assert(MiningSimulator.RefinePerHour(rig, planet) == 0f,
                   "제련소 0레벨이면 시간당 정제량이 0 — 그래서 정제 광물로만 사면 막힌다");

            // 그런데 제련소는 원석으로 사므로, 원석만 캐면 살 수 있다
            var cost = UpgradeCost.Cost(UpgradeSlot.Refinery, rig);
            Assert(!float.IsPositiveInfinity(cost) && cost > 0f, $"제련소 1레벨 비용이 유한하다 {cost:F0}");

            var perHour = MiningSimulator.MineralsPerHour(rig, planet);
            var hoursToAfford = cost / perHour;
            var cargoHours = MiningSimulator.CargoHours(rig, planet);
            Assert(hoursToAfford < cargoHours,
                   $"화물칸이 차기({cargoHours:F1}h) 전에 제련소를 살 수 있다 — {hoursToAfford:F1}h면 모인다");

            // 사고 나면 정제가 실제로 흐르기 시작한다
            var after = UpgradeCost.Apply(UpgradeSlot.Refinery, rig);
            Assert(after.RefineryLevel == 1, "제련소가 1레벨이 된다");
            Assert(MiningSimulator.RefinePerHour(after, planet) > 0f,
                   "제련소 1레벨부터 정제 광물이 쌓이기 시작한다 — 나머지 업그레이드가 열린다");
        });

        Test("업그레이드: 제련소를 5레벨까지 올리면 캐는 만큼 전부 정제된다", () =>
        {
            var planet = DefaultData.Planets()[0];
            var rig = new MiningRig();
            for (var i = 0; i < UpgradeCost.RefineryMaxLevel; i++)
                rig = UpgradeCost.Apply(UpgradeSlot.Refinery, rig);
            Assert(rig.RefineryLevel == 5, $"5레벨 {rig.RefineryLevel}");
            Assert(UpgradeCost.AtMax(UpgradeSlot.Refinery, rig), "5레벨이 최대");
            var mined = MiningSimulator.MineralsPerHour(rig, planet);
            var refined = MiningSimulator.RefinePerHour(rig, planet);
            Assert(Math.Abs(mined - refined) < 0.001f,
                   $"5레벨이면 산출({mined:F1})과 정제({refined:F1})가 같다 — 화물칸이 사실상 안 찬다");
        });

        Test("업그레이드: 정의 밖 UpgradeSlot 값은 조용히 넘어가지 않고 예외를 던진다", () =>
        {
            var badSlot = (UpgradeSlot)99;
            var rig = new MiningRig();
            var threw = false;
            try { UpgradeCost.MaxLevel(badSlot); }
            catch (ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "MaxLevel: 정의 밖 슬롯은 ArgumentOutOfRangeException");

            threw = false;
            try { UpgradeCost.CurrentLevel(badSlot, rig); }
            catch (ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "CurrentLevel: 정의 밖 슬롯은 ArgumentOutOfRangeException");
        });

        Test("업그레이드: Apply는 해당 슬롯 레벨만 올리고 다른 슬롯은 그대로 둔다", () =>
        {
            var rig = new MiningRig();
            var after = UpgradeCost.Apply(UpgradeSlot.Engine, rig);
            Assert(after.EngineLevel == rig.EngineLevel + 1, $"엔진 +1 {after.EngineLevel}");
            Assert(after.ToolLevel == rig.ToolLevel && after.CargoLevel == rig.CargoLevel
                && after.DetectorLevel == rig.DetectorLevel && after.RefineryLevel == rig.RefineryLevel,
                "다른 슬롯은 그대로");
        });

        Test("업그레이드: 레벨이 오르면 실제 산출(시간당 광물 또는 화물칸 시간)도 좋아진다", () =>
        {
            var toolBefore = new MiningRig();
            var toolAfter = UpgradeCost.Apply(UpgradeSlot.Tool, toolBefore);
            Assert(MiningSimulator.MineralsPerHour(toolAfter, quartz) > MiningSimulator.MineralsPerHour(toolBefore, quartz),
                "도구 업그레이드 → 시간당 산출 증가");

            var cargoBefore = new MiningRig();
            var cargoAfter = UpgradeCost.Apply(UpgradeSlot.Cargo, cargoBefore);
            Assert(MiningSimulator.CargoHours(cargoAfter, quartz) > MiningSimulator.CargoHours(cargoBefore, quartz),
                "화물칸 업그레이드 → 상한 시간 증가");

            var engineBefore = new MiningRig();
            var engineAfter = UpgradeCost.Apply(UpgradeSlot.Engine, engineBefore);
            Assert(MiningSimulator.RigSpeed(engineAfter, quartz) > MiningSimulator.RigSpeed(engineBefore, quartz),
                "엔진 업그레이드 → 이동 속도 증가");
        });

        // D07-M: 시계 되감기(과거 시각) 시 0 처리. MiningSimulator.Offline은 이미
        // Math.Max(0, elapsedSeconds)로 막고 있었다(구현은 그대로) — 여기서는 그 동작을
        // 회귀 테스트로 고정하고, DiscoverOffline까지 사슬로 이어지는지 같이 확인한다.
        Test("오프라인: 저장 시각이 기기 시계보다 미래(음수 경과)면 인정 시간 0", () =>
        {
            var rig = new MiningRig();
            var r = MiningSimulator.Offline(rig, quartz, -3600.0); // 시계를 한 시간 되감은 경우
            AssertNear(0f, r.HoursCounted, "인정 시간");
            AssertNear(0f, r.HoursWasted, "버린 시간도 0(음수를 '버림'으로 셀 이유가 없다)");
            AssertNear(0f, r.Minerals, "광물");
            AssertNear(0f, r.Gems, "보석");
        });

        Test("오프라인 발견: 음수 경과 시간에도 보물 목록은 비어 있을 뿐 예외를 던지지 않는다", () =>
        {
            var rig = new MiningRig();
            var defs = DefaultData.QuartzTreasureDefs();
            var combined = ExplorationSimulator.DiscoverOffline(rig, quartz, -100.0, defs, seed: 55);
            AssertNear(0f, combined.Mining.HoursCounted, "인정 시간");
            Assert(combined.Treasures.Count == 0, $"발견 0개 — 실제 {combined.Treasures.Count}");
        });

        Test("오프라인: 경과 시간이 정확히 0이어도 음수와 같은 결과(경계값)", () =>
        {
            var rig = new MiningRig();
            var atZero = MiningSimulator.Offline(rig, quartz, 0.0);
            var negative = MiningSimulator.Offline(rig, quartz, -1.0);
            AssertNear(atZero.HoursCounted, negative.HoursCounted, "0초와 음수초의 인정 시간이 같다");
            AssertNear(0f, atZero.Minerals, "0초 경과 = 광물 0");
        });

        Test("오프라인: 아주 큰 경과 시간(수백 년)도 화물칸 상한에서 그대로 잘리고 NaN이 안 난다", () =>
        {
            var rig = new MiningRig { CargoLevel = 5 };
            var hugeSeconds = 3600.0 * 24 * 365 * 300; // 300년치를 한 번에 몰아준 극단값(오프라인 캐치업 버그로 가능한 시나리오)
            var r = MiningSimulator.Offline(rig, quartz, hugeSeconds);
            Assert(!float.IsNaN(r.Minerals) && !float.IsInfinity(r.Minerals), $"광물 값이 정상 수({r.Minerals})");
            AssertNear(MiningSimulator.CargoHours(rig, quartz), r.HoursCounted, "인정 시간은 화물칸 상한 그대로");
            Assert(r.HoursWasted > 1000000f, $"버린 시간이 큰 수({r.HoursWasted}h) — 상한을 실제로 넘겼다는 뜻");
        });

        Test("제작: 첫 제작은 성공하고 보유 목록에 id가 더해진다", () =>
        {
            var owned = new List<string>();
            var part = DefaultData.QuartzStarterParts()[0];
            Assert(PartCraft.CanCraft(owned, part), "제작 가능 상태에서 시작");
            owned.Add(part.Id);
            Assert(!PartCraft.CanCraft(owned, part), "만든 뒤엔 다시 못 만든다");
        });

        Test("제작: B등급은 C의 4배 비용(2026-09-20 QuartzAdvancedParts 추가), A/S는 아직 예외", () =>
        {
            AssertNear(DefaultData.PartCostC * 4f, PartCraft.Cost(PartGrade.B), "B등급 비용 = C의 4배");
            var threwA = false;
            try { PartCraft.Cost(PartGrade.A); } catch (NotSupportedException) { threwA = true; }
            Assert(threwA, "A등급 Cost가 NotSupportedException을 던짐");
            var threwS = false;
            try { PartCraft.Cost(PartGrade.S); } catch (NotSupportedException) { threwS = true; }
            Assert(threwS, "S등급 Cost가 NotSupportedException을 던짐");
        });

        Test("제작: QuartzAdvancedParts(B등급)는 5종 — 슬롯 하나씩, 스탯이 C의 2배, id가 C 세트와 안 겹침", () =>
        {
            var starter = DefaultData.QuartzStarterParts();
            var advanced = DefaultData.QuartzAdvancedParts();
            Assert(advanced.Count == 5, "Engine/Tire/Suspension/Body/Booster 5종");
            foreach (var p in advanced)
            {
                Assert(p.Grade == PartGrade.B, $"{p.Id} 등급은 B");
                Assert(!starter.Exists(sp => sp.Id == p.Id), $"{p.Id}는 C 세트 id와 겹치지 않아야 함");
            }
            var starterBySlot = starter.ToDictionary(p => p.Slot, p => p);
            foreach (var p in advanced)
            {
                var c = starterBySlot[p.Slot];
                AssertNear(c.Base.Power * 2f, p.Base.Power, $"{p.Slot} Power 2배");
                AssertNear(c.Base.Grip * 2f, p.Base.Grip, $"{p.Slot} Grip 2배");
                AssertNear(c.Base.Suspension * 2f, p.Base.Suspension, $"{p.Slot} Suspension 2배");
                AssertNear(c.Base.Durability * 2f, p.Base.Durability, $"{p.Slot} Durability 2배");
                AssertNear(c.Base.Aero * 2f, p.Base.Aero, $"{p.Slot} Aero 2배");
                AssertNear(c.Base.Boost * 2f, p.Base.Boost, $"{p.Slot} Boost 2배");
            }
        });

        Test("제작: QuartzEpicParts(A등급)는 5종 — 슬롯 하나씩, 스탯이 C의 4배, id가 C/B 세트와 안 겹침 (2026-09-21 P-07 후속)", () =>
        {
            var starter = DefaultData.QuartzStarterParts();
            var advanced = DefaultData.QuartzAdvancedParts();
            var epic = DefaultData.QuartzEpicParts();
            Assert(epic.Count == 5, "Engine/Tire/Suspension/Body/Booster 5종");
            var starterBySlot = starter.ToDictionary(p => p.Slot, p => p);
            foreach (var p in epic)
            {
                Assert(p.Grade == PartGrade.A, $"{p.Id} 등급은 A");
                Assert(!starter.Exists(sp => sp.Id == p.Id), $"{p.Id}는 C 세트 id와 겹치지 않아야 함");
                Assert(!advanced.Exists(bp => bp.Id == p.Id), $"{p.Id}는 B 세트 id와 겹치지 않아야 함");
                var c = starterBySlot[p.Slot];
                AssertNear(c.Base.Power * 4f, p.Base.Power, $"{p.Slot} Power 4배");
                AssertNear(c.Base.Grip * 4f, p.Base.Grip, $"{p.Slot} Grip 4배");
                AssertNear(c.Base.Suspension * 4f, p.Base.Suspension, $"{p.Slot} Suspension 4배");
                AssertNear(c.Base.Durability * 4f, p.Base.Durability, $"{p.Slot} Durability 4배");
                AssertNear(c.Base.Aero * 4f, p.Base.Aero, $"{p.Slot} Aero 4배");
                AssertNear(c.Base.Boost * 4f, p.Base.Boost, $"{p.Slot} Boost 4배");
            }
        });

        Test("제작: QuartzLegendaryParts(S등급)는 5종 — 스탯이 C의 8배, id가 C/B/A 세트와 안 겹침 (2026-09-21 P-07 후속)", () =>
        {
            var starter = DefaultData.QuartzStarterParts();
            var advanced = DefaultData.QuartzAdvancedParts();
            var epic = DefaultData.QuartzEpicParts();
            var legendary = DefaultData.QuartzLegendaryParts();
            Assert(legendary.Count == 5, "Engine/Tire/Suspension/Body/Booster 5종");
            var starterBySlot = starter.ToDictionary(p => p.Slot, p => p);
            foreach (var p in legendary)
            {
                Assert(p.Grade == PartGrade.S, $"{p.Id} 등급은 S");
                Assert(!starter.Exists(sp => sp.Id == p.Id), $"{p.Id}는 C 세트 id와 겹치지 않아야 함");
                Assert(!advanced.Exists(bp => bp.Id == p.Id), $"{p.Id}는 B 세트 id와 겹치지 않아야 함");
                Assert(!epic.Exists(ap => ap.Id == p.Id), $"{p.Id}는 A 세트 id와 겹치지 않아야 함");
                var c = starterBySlot[p.Slot];
                AssertNear(c.Base.Power * 8f, p.Base.Power, $"{p.Slot} Power 8배");
                AssertNear(c.Base.Grip * 8f, p.Base.Grip, $"{p.Slot} Grip 8배");
                AssertNear(c.Base.Suspension * 8f, p.Base.Suspension, $"{p.Slot} Suspension 8배");
                AssertNear(c.Base.Durability * 8f, p.Base.Durability, $"{p.Slot} Durability 8배");
                AssertNear(c.Base.Aero * 8f, p.Base.Aero, $"{p.Slot} Aero 8배");
                AssertNear(c.Base.Boost * 8f, p.Base.Boost, $"{p.Slot} Boost 8배");
            }
        });

        Test("제작: PartCraft.Recipe(A/S)는 쿼츠+루비 혼합 레시피, C/B는 예외 — Cost와 Recipe가 서로 배타적 (2026-09-21 P-07 후속)", () =>
        {
            var recipeA = PartCraft.Recipe(PartGrade.A);
            Assert(recipeA.Count == 2, "A 레시피는 두 줄(쿼츠+루비)");
            AssertNear(180f, recipeA.Find(c => c.PlanetId == "quartz").Amount, "A 쿼츠 180");
            AssertNear(60f, recipeA.Find(c => c.PlanetId == "ruby").Amount, "A 루비 60(25%)");

            var recipeS = PartCraft.Recipe(PartGrade.S);
            Assert(recipeS.Count == 2, "S 레시피는 두 줄(쿼츠+루비)");
            AssertNear(480f, recipeS.Find(c => c.PlanetId == "quartz").Amount, "S 쿼츠 480");
            AssertNear(480f, recipeS.Find(c => c.PlanetId == "ruby").Amount, "S 루비 480(50%, A보다 루비 의존도 상승)");

            var threwC = false;
            try { PartCraft.Recipe(PartGrade.C); } catch (NotSupportedException) { threwC = true; }
            Assert(threwC, "C등급 Recipe가 NotSupportedException을 던짐 — Cost를 써야 함");
            var threwB = false;
            try { PartCraft.Recipe(PartGrade.B); } catch (NotSupportedException) { threwB = true; }
            Assert(threwB, "B등급 Recipe가 NotSupportedException을 던짐 — Cost를 써야 함");
        });

        Test("제작: PartCraft.Recipe(A)를 PlanetMineralBank/PlanetMineralRecipe로 실제 검사·소비해 보면 all-or-nothing으로 맞물린다 (2026-09-21 P-07 후속)", () =>
        {
            var ids = new List<string> { "quartz", "ruby" };
            var amounts = new List<float> { 180f, 59f }; // 루비가 1 모자람
            var recipe = PartCraft.Recipe(PartGrade.A);
            Assert(!PlanetMineralRecipe.CanAfford(ids, amounts, recipe), "루비가 1 모자라면 감당 불가");
            Assert(!PlanetMineralRecipe.TrySpend(ids, amounts, recipe), "TrySpend도 실패");
            AssertNear(180f, amounts[0], "실패했으니 쿼츠도 안 깎였어야 함(all-or-nothing)");

            amounts[1] = 60f; // 정확히 맞춤
            Assert(PlanetMineralRecipe.TrySpend(ids, amounts, recipe), "정확히 맞으면 성공");
            AssertNear(0f, amounts[0], "쿼츠 180 전액 소비");
            AssertNear(0f, amounts[1], "루비 60 전액 소비");
        });

        Test("장착: 보유하지 않은 부품은 장착할 수 없다", () =>
        {
            var car = new RacingCar();
            var owned = new List<string>();
            var part = DefaultData.QuartzStarterParts()[0];
            var ok = PartEquip.TryEquip(car, owned, part);
            Assert(!ok, "미보유 상태에서 장착 실패해야 함");
            Assert(car.Slots[part.Slot] == null, "슬롯이 그대로 비어 있어야 함");
        });

        Test("장착: 보유한 부품은 자기 슬롯에 들어가고, 다시 장착해도 같은 슬롯 하나만 채운다", () =>
        {
            var car = new RacingCar();
            var owned = new List<string>();
            var part = DefaultData.QuartzStarterParts()[0]; // q_engine_c, Slot=Engine
            owned.Add(part.Id);
            var ok = PartEquip.TryEquip(car, owned, part);
            Assert(ok, "보유한 부품은 장착 성공해야 함");
            Assert(car.Slots[PartSlot.Engine] == part, "Engine 슬롯에 들어감");
            Assert(car.Slots[PartSlot.Tire] == null, "다른 슬롯은 안 건드림(중복 장착 없음)");

            var ok2 = PartEquip.TryEquip(car, owned, part);
            Assert(ok2, "이미 장착 중인 걸 다시 장착해도 성공");
            var equippedCount = 0;
            foreach (var p in car.Slots.Values) if (p == part) equippedCount++;
            Assert(equippedCount == 1, $"같은 부품이 슬롯에 딱 하나만 있어야 함(실제 {equippedCount})");
        });

        Test("장착: 같은 슬롯에 새 부품을 끼우면 기존 부품은 해제되지만 보유 목록엔 남는다", () =>
        {
            var car = new RacingCar();
            var owned = new List<string>();
            var oldPart = new Part { Id = "old_engine", Slot = PartSlot.Engine, Base = new Stats { Power = 10 } };
            var newPart = new Part { Id = "new_engine", Slot = PartSlot.Engine, Base = new Stats { Power = 20 } };
            owned.Add(oldPart.Id); owned.Add(newPart.Id);
            PartEquip.TryEquip(car, owned, oldPart);
            PartEquip.TryEquip(car, owned, newPart);
            Assert(car.Slots[PartSlot.Engine] == newPart, "새 부품이 슬롯을 차지");
            Assert(owned.Contains(oldPart.Id), "예전 부품도 보유 목록엔 그대로 남음(다시 장착 가능)");
        });

        Test("해제: Unequip은 해당 슬롯만 비우고, 이미 빈 슬롯도 안전하다", () =>
        {
            var car = new RacingCar();
            var owned = new List<string>();
            var part = DefaultData.QuartzStarterParts()[0];
            owned.Add(part.Id);
            PartEquip.TryEquip(car, owned, part);
            PartEquip.Unequip(car, PartSlot.Engine);
            Assert(car.Slots[PartSlot.Engine] == null, "해제 후 비어 있어야 함");
            PartEquip.Unequip(car, PartSlot.Tire); // 원래 비어 있던 슬롯 — 예외 없이 넘어가야 함
        });

        // D08-M: 여기부터는 MiningController.TryCraftPart/LoadParts/EquippedIdsInSlotOrder가 하는
        // 일을 코어 조각만으로 그대로 재현한다(Assets/Scripts는 UnityEngine을 참조해서 Core.Tests가
        // 직접 못 부른다 — CLAUDE.md 1번). SlotOrder는 MiningController.cs의 배열과 반드시 같은
        // 순서여야 한다, 어긋나면 세이브를 불러올 때 부품이 엉뚱한 슬롯에 꽂힌다.
        var slotOrder = new[]
        {
            PartSlot.Engine, PartSlot.Tire, PartSlot.Suspension, PartSlot.Body, PartSlot.Booster, PartSlot.Module,
        };

        Test("제작: 비용만큼 정확히 차감되고, 부족하면 아무 것도 안 바뀐다", () =>
        {
            var owned = new List<string>();
            var part = DefaultData.QuartzStarterParts()[0]; // q_engine_c, Cost = PartCostC(15)
            var raw = DefaultData.PartCostC; // 딱 맞는 금액

            bool TrySpend(ref float pool, float amount)
            {
                if (pool < amount) return false;
                pool -= amount;
                return true;
            }

            Assert(TrySpend(ref raw, PartCraft.Cost(part.Grade)), "딱 맞는 금액이면 제작 성공");
            AssertNear(raw, 0f, "차감 후 정확히 0 (더도 덜도 아님)");
            owned.Add(part.Id);

            var poor = PartCraft.Cost(part.Grade) - 1f; // 1 부족
            Assert(!TrySpend(ref poor, PartCraft.Cost(part.Grade)), "부족하면 실패");
            AssertNear(poor, PartCraft.Cost(part.Grade) - 1f, "실패하면 값이 안 바뀌어야 함(부분 차감 없음)");
        });

        Test("통합: 제작→장착→세이브 직렬화→역직렬화→복원까지 한 바퀴 돌아도 장착 상태가 그대로다", () =>
        {
            var parts = DefaultData.QuartzStarterParts(); // Engine/Tire/Suspension/Body/Booster 5종, Module은 없음
            var owned = new List<string>();
            var car = new RacingCar();

            // 제작 + 장착: 엔진과 타이어만 만들어서 장착하고, 서스펜션은 만들기만 하고 장착 안 함.
            foreach (var p in new[] { parts[0], parts[1], parts[2] })
            {
                Assert(PartCraft.CanCraft(owned, p), $"{p.Id} 제작 가능");
                owned.Add(p.Id);
            }
            Assert(PartEquip.TryEquip(car, owned, parts[0]), "엔진 장착");
            Assert(PartEquip.TryEquip(car, owned, parts[1]), "타이어 장착");
            // parts[2](서스펜션)는 보유만 하고 장착은 안 함 — LoadParts가 빈 슬롯을 실제로 비워 두는지 확인용.

            // MiningController.Save()가 하는 일: OwnedPartIds 그대로 복사 + EquippedIdsInSlotOrder.
            var save = new SaveData
            {
                OwnedPartIds = new List<string>(owned),
                EquippedPartIds = slotOrder
                    .Select(slot => car.Slots.TryGetValue(slot, out var p) && p != null ? p.Id : "")
                    .ToList(),
            };
            Assert(save.EquippedPartIds.Count == 6, "6칸 고정(Module 포함, 빈 슬롯도 자리 유지)");
            Assert(save.EquippedPartIds[0] == parts[0].Id && save.EquippedPartIds[1] == parts[1].Id
                && save.EquippedPartIds[2] == "", "Engine/Tire는 채워지고 Suspension은 미장착이라 빈 문자열");

            // 실제 세이브 파일과 같은 경로: JSON 직렬화 → 역직렬화(D03-M과 같은 방식).
            var options = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
            var json = System.Text.Json.JsonSerializer.Serialize(save, options);
            var restored = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json, options);
            Assert(restored != null, "역직렬화 결과가 null이 아니다");

            // MiningController.LoadParts가 하는 일: id로 AvailableParts에서 찾아 슬롯에 되꽂는다.
            var restoredCar = new RacingCar();
            for (var i = 0; i < slotOrder.Length && i < restored!.EquippedPartIds.Count; i++)
            {
                var id = restored.EquippedPartIds[i];
                if (string.IsNullOrEmpty(id)) continue;
                var found = parts.Find(p => p.Id == id);
                if (found != null) restoredCar.Slots[slotOrder[i]] = found;
            }

            Assert(restoredCar.Slots[PartSlot.Engine]?.Id == parts[0].Id, "엔진 슬롯 복원");
            Assert(restoredCar.Slots[PartSlot.Tire]?.Id == parts[1].Id, "타이어 슬롯 복원");
            Assert(restoredCar.Slots[PartSlot.Suspension] == null, "장착 안 했던 서스펜션은 복원 후에도 빈 슬롯");
            Assert(restoredCar.Slots[PartSlot.Body] == null && restoredCar.Slots[PartSlot.Booster] == null
                && restoredCar.Slots[PartSlot.Module] == null, "나머지 슬롯도 전부 빈 채로 유지");
            Assert(restored.OwnedPartIds.SequenceEqual(owned), "보유 목록도 그대로 왕복(장착 안 한 서스펜션 포함)");
        });

        // D11-M: 공구 상자 확률표(LootTable). 가중치 합 1.0 확인 + 10만 회 시뮬레이션으로 실제 분포가
        // 표에 근접하는지, 천장(피티)이 정확히 작동하는지가 핵심.
        Test("공구 상자: 녹슨·강철·티타늄 확률표 모두 가중치 합이 정확히 1.0이다", () =>
        {
            AssertNear(1f, LootTable.Rusty().Sum(w => w.Weight), "녹슨 상자 가중치 합");
            AssertNear(1f, LootTable.Steel().Sum(w => w.Weight), "강철 상자 가중치 합");
            AssertNear(1f, LootTable.Titanium().Sum(w => w.Weight), "티타늄 상자 가중치 합");
        });

        Test("공구 상자: 같은 seed는 항상 같은 등급을 준다(서버 재검증용 재현성)", () =>
        {
            var a = LootTable.Open(LootTable.Steel(), 777);
            var b = LootTable.Open(LootTable.Steel(), 777);
            Assert(a.Grade == b.Grade && a.Guaranteed == b.Guaranteed, $"seed 777 반복 시 항상 {a.Grade}");
        });

        Test("공구 상자: 10만 회 열어 보면 실제 등급 분포가 확률표와 1%p 안쪽으로 근접한다", () =>
        {
            const int trials = 100_000;
            var weights = LootTable.Steel(); // B 0.60 / A 0.35 / S 0.05 — 가장 극단적인(0.05) 줄로 오차를 본다
            var counts = new Dictionary<PartGrade, int>();
            for (var i = 0; i < trials; i++)
            {
                // 연속 정수를 그대로 seed로 쓰면 xorshift 한 번만 돌린 초기 상태라 편향이 남을 수 있어서,
                // 큰 소수를 곱해 int 범위 전체로 흩어 준다(정확한 난수성은 필요 없고 표와 근접하기만 하면 된다).
                var seed = unchecked((int)((long)i * 2654435761L + 40503L));
                var r = LootTable.Open(weights, seed);
                counts[r.Grade] = counts.TryGetValue(r.Grade, out var c) ? c + 1 : 1;
            }
            foreach (var w in weights)
            {
                var actual = counts.TryGetValue(w.Grade, out var c) ? (float)c / trials : 0f;
                Assert(Math.Abs(actual - w.Weight) < 0.01f, $"{w.Grade} 실제 {actual:P1} vs 기대 {w.Weight:P0}");
            }
        });

        Test("공구 상자: 천장(피티) — pityCount번째 개봉은 확률과 무관하게 지정 등급을 확정 지급한다", () =>
        {
            // seed를 S가 절대 안 나올 값으로 고정해도(가중치 0.05짜리라 충분히 있음) 30번째(openedSincePity=29)는 A 확정.
            var weights = LootTable.Steel();
            for (var opened = 0; opened < LootTable.SteelPityCount - 1; opened++)
            {
                var r = LootTable.Open(weights, seed: 1, openedSincePity: opened, pityCount: LootTable.SteelPityCount, pityGrade: LootTable.SteelPityGrade);
                Assert(!r.Guaranteed, $"{opened + 1}번째는 아직 확정 아님");
            }
            var last = LootTable.Open(weights, seed: 1, openedSincePity: LootTable.SteelPityCount - 1, pityCount: LootTable.SteelPityCount, pityGrade: LootTable.SteelPityGrade);
            Assert(last.Guaranteed && last.Grade == LootTable.SteelPityGrade, $"{LootTable.SteelPityCount}번째는 {LootTable.SteelPityGrade} 확정, 실제 {last.Grade} (Guaranteed={last.Guaranteed})");
        });

        Test("공구 상자: 천장이 없는 상자(녹슨, pityCount=0)는 아무리 많이 열어도 확정되지 않는다", () =>
        {
            var r = LootTable.Open(LootTable.Rusty(), seed: 42, openedSincePity: 999_999, pityCount: 0);
            Assert(!r.Guaranteed, "천장 없음이면 openedSincePity가 커도 확률표를 그대로 따른다");
        });

        Test("공구 상자: 빈 확률표나 가중치 합 0은 예외를 던진다(방어적 실패, 조용히 넘어가지 않음)", () =>
        {
            var threwEmpty = false;
            try { LootTable.Open(new List<LootWeight>(), seed: 1); } catch (ArgumentException) { threwEmpty = true; }
            Assert(threwEmpty, "빈 확률표는 예외");

            var threwZero = false;
            try { LootTable.Open(new List<LootWeight> { new LootWeight { Grade = PartGrade.C, Weight = 0f } }, seed: 1); }
            catch (ArgumentException) { threwZero = true; }
            Assert(threwZero, "가중치 합 0은 예외");
        });

        // D11-N(T-07 A안 진행): 공구 상자 등급 → 채굴차 부품 매핑(LootReward)과 그걸 확률표·천장
        // 카운터까지 한 번에 묶는 조립 계층(LootBoxOpener). "등급이 높을수록 희귀 슬롯 우대 +
        // LevelBonus가 크다"는 decisions.md T-07 A안 그대로 검증한다.
        Test("공구 상자 보상: 등급이 높을수록 LevelBonus가 커진다(단조 증가)", () =>
        {
            Assert(LootReward.LevelBonusFor(PartGrade.C) <= LootReward.LevelBonusFor(PartGrade.B), "C <= B");
            Assert(LootReward.LevelBonusFor(PartGrade.B) < LootReward.LevelBonusFor(PartGrade.A), "B < A");
            Assert(LootReward.LevelBonusFor(PartGrade.A) < LootReward.LevelBonusFor(PartGrade.S), "A < S");
        });

        Test("공구 상자 보상: 같은 seed는 항상 같은 슬롯을 준다(재현성)", () =>
        {
            var a = LootReward.PickSlot(PartGrade.A, 555);
            var b = LootReward.PickSlot(PartGrade.A, 555);
            Assert(a == b, $"seed 555 반복 시 항상 {a}");
        });

        Test("공구 상자 보상: C 등급은 희귀 슬롯(Detector/Refinery)이 절대 안 나온다", () =>
        {
            for (var seed = 0; seed < 2000; seed++)
            {
                var slot = LootReward.PickSlot(PartGrade.C, seed);
                Assert(slot != RigSlot.Detector && slot != RigSlot.Refinery, $"seed {seed}: C 등급인데 {slot}");
            }
        });

        Test("공구 상자 보상: S 등급은 흔한 슬롯(Tool/Cargo/Engine)이 절대 안 나온다(희귀 슬롯 우대)", () =>
        {
            for (var seed = 0; seed < 2000; seed++)
            {
                var slot = LootReward.PickSlot(PartGrade.S, seed);
                Assert(slot == RigSlot.Detector || slot == RigSlot.Refinery, $"seed {seed}: S 등급인데 {slot}");
            }
        });

        Test("공구 상자 보상: FromLoot이 만든 RigPartReward를 실제로 적용하면 해당 슬롯 레벨이 오른다", () =>
        {
            var loot = new LootResult { Grade = PartGrade.A, Guaranteed = false };
            var reward = LootReward.FromLoot(loot, slotSeed: 42, courseId: "quartz-local-1");
            var rig = new MiningRig { ToolLevel = 1, CargoLevel = 1, EngineLevel = 1 };
            var after = RigPartApply.Apply(rig, reward);
            Assert(reward.LevelBonus == LootReward.LevelBonusFor(PartGrade.A), "LevelBonus가 등급과 일치");
            Assert(reward.CourseId == "quartz-local-1", "CourseId가 그대로 전달됨");
        });

        Test("공구 상자 보상: 정의 밖 등급(세이브 손상 등)은 LevelBonusFor는 방어되지만 PickSlot은 안 잠겨 있다", () =>
        {
            var undefined = (PartGrade)99;
            // LevelBonusFor는 `_ => 1` 기본값으로 방어돼 있다(PlatformConfig·ShopSkuId처럼).
            Assert(LootReward.LevelBonusFor(undefined) == 1, "정의 밖 등급의 LevelBonus는 1로 방어됨");

            // 반면 PickSlot의 SlotWeights 딕셔너리는 정의 밖 등급을 안 막아 놨다 — 지금 동작(예외)을
            // 그대로 잠가서, 나중에 고칠 때 "던져야 하는데 안 던지는지"가 아니라 "지금은 던진다"부터 알 수 있게 한다.
            var threw = false;
            try { LootReward.PickSlot(undefined, 1); }
            catch (KeyNotFoundException) { threw = true; }
            Assert(threw, "PickSlot(정의 밖 등급)은 KeyNotFoundException을 던진다(SlotWeights에 없는 키)");

            // FromLoot도 내부에서 PickSlot을 부르니 같은 예외가 그대로 올라온다.
            var loot = new LootResult { Grade = undefined, Guaranteed = false };
            var threwFromLoot = false;
            try { LootReward.FromLoot(loot, slotSeed: 1); }
            catch (KeyNotFoundException) { threwFromLoot = true; }
            Assert(threwFromLoot, "FromLoot(정의 밖 등급)도 PickSlot을 통해 같은 예외를 던진다");
        });

        Test("LootBoxOpener: 확률표 뽑기 + 부품 매핑 + 천장 카운터 갱신이 한 번에 맞물린다", () =>
        {
            // pityCount-1번째(마지막 한 번 전) 개봉 — 아직 확정 아님, 카운터가 1 증가한다.
            var beforePity = LootBoxOpener.Open(LootBoxType.Steel, gradeSeed: 1, slotSeed: 1,
                openedSincePity: LootTable.SteelPityCount - 2);
            Assert(!beforePity.Loot.Guaranteed, "아직 확정 아님");
            Assert(beforePity.NextOpenedSincePity == LootTable.SteelPityCount - 1, "카운터 +1");

            // pityCount번째 — 확정 지급, 카운터가 0으로 리셋된다.
            var atPity = LootBoxOpener.Open(LootBoxType.Steel, gradeSeed: 1, slotSeed: 1,
                openedSincePity: LootTable.SteelPityCount - 1);
            Assert(atPity.Loot.Guaranteed && atPity.Loot.Grade == LootTable.SteelPityGrade, "확정 등급 지급");
            Assert(atPity.NextOpenedSincePity == 0, "확정 뒤 카운터 리셋");
            Assert(atPity.Reward.LevelBonus == LootReward.LevelBonusFor(LootTable.SteelPityGrade),
                $"확정 등급({LootTable.SteelPityGrade})도 등급대로 LevelBonus가 매겨진다, 실제 {atPity.Reward.LevelBonus}");
        });

        Test("LootBoxOpener: 녹슨 상자(천장 없음)는 아무리 openedSincePity가 커도 카운터가 계속 늘어난다", () =>
        {
            var r = LootBoxOpener.Open(LootBoxType.Rusty, gradeSeed: 7, slotSeed: 7, openedSincePity: 999);
            Assert(!r.Loot.Guaranteed, "천장 없음이라 확정 안 됨");
            Assert(r.NextOpenedSincePity == 1000, "확정 안 됐으니 그냥 +1(호출하는 쪽이 어차피 안 씀)");
        });

        Test("LootBoxOpener: 처음 여는 상자(openedSincePity=0)도 정상 동작한다(경계값)", () =>
        {
            var r = LootBoxOpener.Open(LootBoxType.Titanium, gradeSeed: 3, slotSeed: 3, openedSincePity: 0);
            Assert(!r.Loot.Guaranteed, "천장(10개)까지 한참 남아 확정 아님");
            Assert(r.NextOpenedSincePity == 1, "카운터가 0에서 1로");
            Assert(r.Reward.Id.StartsWith("loot-"), "부품 보상도 같이 나온다");
        });

        Test("LootBoxOpener: 천장 1개째(pityCount=1과 동치인 openedSincePity)는 첫 개봉부터 바로 확정된다", () =>
        {
            // openedSincePity + 1 >= pityCount 조건이므로, pityCount(10)-1인 9에서 이미 확정이어야 한다.
            var r = LootBoxOpener.Open(LootBoxType.Titanium, gradeSeed: 3, slotSeed: 3,
                openedSincePity: LootTable.TitaniumPityCount - 1);
            Assert(r.Loot.Guaranteed && r.Loot.Grade == LootTable.TitaniumPityGrade, "티타늄 10개째 S 확정");
            Assert(r.NextOpenedSincePity == 0, "확정 뒤 카운터 리셋");
        });

        Test("LootBoxOpener: 알 수 없는 상자 종류(정의 밖 enum 값)는 예외 없이 녹슨 상자 표로 방어된다", () =>
        {
            var unknown = (LootBoxType)999;
            var r = LootBoxOpener.Open(unknown, gradeSeed: 1, slotSeed: 1, openedSincePity: 0);
            Assert(r.Loot.Grade == PartGrade.C || r.Loot.Grade == PartGrade.B || r.Loot.Grade == PartGrade.A,
                $"녹슨 상자 등급 범위 안(C/B/A), 실제 {r.Loot.Grade}");
            Assert(LootBoxOpener.NameKo(unknown) == "999", "이름도 방어값(ToString)으로 떨어진다");
        });

        // D11-N 후속: 레이스 등급 → 공구 상자 매핑(RaceBoxReward). GDD "레이스" 항목(로컬=녹슨,
        // 서킷=강철, 챌린지=티타늄, 그랑프리=워프)이 실제로 코드에 반영됐는지 확인.
        Test("RaceBoxReward: 등급별 매핑이 GDD와 일치한다", () =>
        {
            Assert(RaceBoxReward.ForTier(RaceTier.Local) == LootBoxType.Rusty, "로컬 = 녹슨");
            Assert(RaceBoxReward.ForTier(RaceTier.Circuit) == LootBoxType.Steel, "서킷 = 강철");
            Assert(RaceBoxReward.ForTier(RaceTier.Challenge) == LootBoxType.Titanium, "챌린지 = 티타늄");
            Assert(RaceBoxReward.ForTier(RaceTier.GrandPrix) == null, "그랑프리는 상자가 아니라 워프 — 상자 없음");
        });

        Test("DefaultData.QuartzCourses: 지금은 전부 RaceTier.Local이라 셋 다 녹슨 상자를 준다", () =>
        {
            foreach (var course in DefaultData.QuartzCourses())
                Assert(RaceBoxReward.ForTier(course.Tier) == LootBoxType.Rusty, $"{course.Id}는 로컬 등급이어야 한다");
        });

        Test("RaceBoxReward: 정의 밖 등급(잘못된 세이브 데이터 등)은 예외 없이 상자 없음으로 방어된다", () =>
        {
            Assert(RaceBoxReward.ForTier((RaceTier)999) == null, "정의 안 된 등급은 null(그랑프리와 같은 취급)");
        });

        // D12-N/D12-M: 부품 강화. Part.Enhance/Effective()(+6%/단계)는 D08-N 때 이미 있었고
        // 여기서는 PartEnhance의 비용 곡선·최대치 클램프만 검증한다.
        Test("강화 비용: +0에서 시작해 단계마다 비용이 계속 커진다(가파름)", () =>
        {
            var part = DefaultData.QuartzStarterParts()[0]; // q_engine_c, Grade.C
            var prev = 0f;
            for (int i = 0; i < PartEnhance.MaxLevel; i++)
            {
                var cost = PartEnhance.Cost(part);
                Assert(cost > prev, $"+{part.Enhance}→+{part.Enhance + 1} 비용({cost})이 이전 단계({prev})보다 커야 함");
                Assert(cost > 0f && !float.IsNaN(cost), $"비용이 정상 양수({cost})");
                prev = cost;
                part.Enhance++; // PartEnhance.Apply 없이 직접 올려서 곡선만 본다
            }
        });

        Test("강화 비용: +10(MaxLevel)이면 더 못 올린다 — Cost가 무한대, AtMax가 참", () =>
        {
            var part = DefaultData.QuartzStarterParts()[0];
            part.Enhance = PartEnhance.MaxLevel;
            Assert(PartEnhance.AtMax(part), "+10이면 AtMax");
            Assert(float.IsPositiveInfinity(PartEnhance.Cost(part)), "+10 비용은 PositiveInfinity(UI 비활성 신호)");
        });

        Test("강화 적용: Apply는 실패 없이(GDD) Enhance를 1씩 올리고, +10에서는 더 안 올라간다", () =>
        {
            var part = DefaultData.QuartzStarterParts()[0];
            for (int i = 0; i < PartEnhance.MaxLevel; i++) PartEnhance.Apply(part);
            Assert(part.Enhance == PartEnhance.MaxLevel, $"10번 적용하면 정확히 +{PartEnhance.MaxLevel}({part.Enhance})");

            PartEnhance.Apply(part); // 최대치에서 한 번 더 — 예외 없이 그대로여야 함
            Assert(part.Enhance == PartEnhance.MaxLevel, "최대치를 넘지 않는다(클램프)");
        });

        Test("강화 스탯: +0은 기본치 그대로, +10은 1.6배(6%×10단계, Models.cs Effective() 공식과 일치)", () =>
        {
            var part = new Part { Id = "t", Slot = PartSlot.Engine, Grade = PartGrade.C, Base = new Stats { Power = 100f } };
            AssertNear(100f, part.Effective().Power, "+0 Power");

            part.Enhance = PartEnhance.MaxLevel;
            AssertNear(160f, part.Effective().Power, "+10 Power (100 * 1.6)");
        });

        Test("강화 비용: B등급은 2026-09-20부터 PartCraft.Cost(B)를 그대로 따라간다(+0 = 제작 비용의 절반). A/S는 여전히 예외(PartCraft.Cost와 같은 경계)", () =>
        {
            var partB = new Part { Id = "b_part", Slot = PartSlot.Engine, Grade = PartGrade.B, Base = new Stats() };
            AssertNear(PartCraft.Cost(PartGrade.B) * 0.5f, PartEnhance.Cost(partB), "+0 강화 비용 = 제작 비용의 절반");

            var partA = new Part { Id = "a_part", Slot = PartSlot.Engine, Grade = PartGrade.A, Base = new Stats() };
            var threw = false;
            try { PartEnhance.Cost(partA); } catch (NotSupportedException) { threw = true; }
            Assert(threw, "A등급 부품의 강화 비용은 아직 NotSupportedException을 던짐");
        });

        Test("강화 비용: 세이브가 깨져 Enhance가 MaxLevel을 넘어 있어도(비정상 데이터) AtMax는 참, Cost는 무한대(예외 없음)", () =>
        {
            var part = DefaultData.QuartzStarterParts()[0];
            part.Enhance = PartEnhance.MaxLevel + 5; // 정상 흐름으로는 안 생기지만 세이브 조작·마이그레이션 버그 대비
            Assert(PartEnhance.AtMax(part), "MaxLevel을 넘어도 AtMax는 참");
            Assert(float.IsPositiveInfinity(PartEnhance.Cost(part)), "Cost도 그대로 PositiveInfinity");
            PartEnhance.Apply(part); // 더 안 올라가야 함
            Assert(part.Enhance == PartEnhance.MaxLevel + 5, "Apply도 예외 없이 아무 일 안 함(그대로)");
        });

        Test("강화: Cost(null)/Apply(null)은 ArgumentNullException — 두 진입점 다 부품 없이 못 부른다", () =>
        {
            var threwCost = false;
            try { PartEnhance.Cost(null); } catch (ArgumentNullException) { threwCost = true; }
            Assert(threwCost, "Cost(null)이 ArgumentNullException을 던짐");

            var threwApply = false;
            try { PartEnhance.Apply(null); } catch (ArgumentNullException) { threwApply = true; }
            Assert(threwApply, "Apply(null)이 ArgumentNullException을 던짐");
        });

        Test("설정: 허용값(30/60)은 그대로 돌려준다", () =>
        {
            Assert(GameSettings.NormalizeFrameRate(30) == 30, "30 그대로");
            Assert(GameSettings.NormalizeFrameRate(60) == 60, "60 그대로");
        });

        Test("설정: 45 미만은 30, 45 이상(경계 포함)은 60으로 붙는다", () =>
        {
            Assert(GameSettings.NormalizeFrameRate(44) == 30, "44 → 30");
            Assert(GameSettings.NormalizeFrameRate(45) == 60, "45(동률) → 60(기본값)");
            Assert(GameSettings.NormalizeFrameRate(46) == 60, "46 → 60");
        });

        Test("설정: 말이 안 되는 값(0·음수·아주 큰 값)도 예외 없이 30이나 60으로 떨어진다", () =>
        {
            Assert(GameSettings.NormalizeFrameRate(0) == 30, "0 → 30");
            Assert(GameSettings.NormalizeFrameRate(-144) == 30, "음수 → 30");
            Assert(GameSettings.NormalizeFrameRate(int.MaxValue) == 60, "아주 큰 값 → 60");
            Assert(GameSettings.NormalizeFrameRate(int.MinValue) == 30, "아주 작은 값 → 30");
        });

        Test("M-03: 쿼츠 첫 상한 도달까지 최소 90분 — 봇 시뮬레이션(MiningRunState, 1초 틱)으로 실측", () =>
        {
            // 첫 세션 가정: 전부 기본 레벨(Tool/Cargo/Engine 1, Detector/Refinery 0) — 이 줄이
            // 깨지면(90분보다 이르면) 초반 이탈이 나니 쿼츠 BaseCargoHours를 올려야 한다.
            var rig = new MiningRig();
            var cap = MiningSimulator.CargoCapacityMinerals(rig, quartz);
            var run = new MiningRunState(rig, quartz);
            const float tick = 1f;                // 실제 프레임 루프와 같은 정밀도(1초)
            const float safetyLimitSeconds = 8f * 3600f; // 8시간 안에도 안 닿으면 시뮬레이션 자체가 잘못된 것
            var raw = 0f;
            var seconds = 0f;
            while (raw < cap && seconds < safetyLimitSeconds)
            {
                raw = MiningSimulator.ClampToCargoCapacity(raw + run.Advance(rig, quartz, tick), rig, quartz);
                seconds += tick;
            }
            Assert(raw >= cap - 0.01f, $"안전 한도(8시간) 안에 상한에 도달해야 한다 (도달 {raw:F1}/{cap:F1})");
            var minutes = seconds / 60f;
            Console.WriteLine($"      [측정값] {minutes:F1}분");
            Assert(minutes >= 90f, $"첫 상한 도달까지 {minutes:F1}분 — 90분 미만이면 관문 위반");
        });

        Test("M-05: HoursUntilCargoThreshold — 이미 목표치를 넘었으면 0", () =>
        {
            var rig = new MiningRig();
            var cap = MiningSimulator.CargoCapacityMinerals(rig, quartz);
            AssertNear(0f, MiningSimulator.HoursUntilCargoThreshold(rig, quartz, cap * 0.8f, 0.8f).Value, "정확히 80%");
            AssertNear(0f, MiningSimulator.HoursUntilCargoThreshold(rig, quartz, cap, 0.8f).Value, "80%를 이미 넘은 100%");
        });

        Test("M-05: HoursUntilCargoThreshold — 기본 채굴차가 80%에 닿는 시각은 100% 도달 시각의 80%(제련소 0레벨이라 선형)", () =>
        {
            var rig = new MiningRig();
            var toFull = MiningSimulator.HoursUntilCargoThreshold(rig, quartz, 0f, 1f);
            var to80 = MiningSimulator.HoursUntilCargoThreshold(rig, quartz, 0f, 0.8f);
            Assert(toFull.HasValue && to80.HasValue, "제련소 0레벨이면 둘 다 값이 있어야 함");
            AssertNear(MiningSimulator.CargoHours(rig, quartz), toFull!.Value, "100% 도달 = CargoHours 그대로(원석 유입 전부가 화물칸으로)");
            AssertNear(toFull.Value * 0.8f, to80!.Value, "80% 지점 = 100% 지점의 0.8배(순증가가 상수라 선형)");
        });

        Test("M-05: HoursUntilCargoThreshold — 제련소 5레벨은 원석이 절대 안 쌓여 null(알림 예약 안 함)", () =>
        {
            var rig = new MiningRig { RefineryLevel = 5 };
            Assert(MiningSimulator.HoursUntilCargoThreshold(rig, quartz, 0f, 0.8f) == null, "R<=F면 도달 자체가 없다");
        });

        Test("M-05: HoursUntilCargoThreshold — 경계값(threshold 0·음수·1 초과, 원석 음수)도 예외 없이 상식적인 값", () =>
        {
            var rig = new MiningRig();
            AssertNear(0f, MiningSimulator.HoursUntilCargoThreshold(rig, quartz, 0f, 0f).Value, "threshold 0은 항상 이미 도달");
            AssertNear(0f, MiningSimulator.HoursUntilCargoThreshold(rig, quartz, 0f, -5f).Value, "음수 threshold도 0으로 클램프돼 이미 도달");
            var over1 = MiningSimulator.HoursUntilCargoThreshold(rig, quartz, 0f, 5f);
            AssertNear(MiningSimulator.CargoHours(rig, quartz), over1!.Value, "1을 넘는 threshold는 1로 클램프(=풀 상한)");
            var negativeRaw = MiningSimulator.HoursUntilCargoThreshold(rig, quartz, -100f, 0.8f);
            Assert(negativeRaw.HasValue && negativeRaw.Value > 0f && !float.IsNaN(negativeRaw.Value), "원석이 음수(비정상값)라도 NaN 없이 더 긴 시간이 나올 뿐");
        });

        Test("M-06 Entitlements: 아무것도 안 산 상태는 전부 기본값(배율 1, 오프라인 4시간, 나머지 꺼짐)", () =>
        {
            var e = Entitlements.Effective(default, nowUnixSeconds: 1000L);
            AssertNear(1f, e.CargoMultiplier, "화물칸 배율 기본값");
            AssertNear(4f, e.OfflineCapHours, "오프라인 기본 4시간");
            Assert(!e.AutoRefineryAlwaysOn, "구독 없으면 자동 제련 상시 켜짐 아님");
            Assert(!e.AdsRemoved, "구독·구매 없으면 광고 안 사라짐");
            Assert(e.BonusFuelCapacity == 0, "구독 없으면 대전권 보너스 없음");
            AssertNear(1f, e.MiningYieldMultiplier, "가속 패스 없으면 산출 배율 1");
            Assert(!e.DailyRefinedMineralsGrant, "구독 없으면 매일 정제 광물 지급 대상 아님");
        });

        Test("M-06 Entitlements: 화물칸 확장 단계별 배율 — 0~3단계, 범위 밖(음수·4 이상)은 가까운 끝으로 클램프", () =>
        {
            AssertNear(1f, Entitlements.Effective(new PurchaseState { CargoExpansionLevel = 0 }, 0L).CargoMultiplier, "0단계");
            AssertNear(1.5f, Entitlements.Effective(new PurchaseState { CargoExpansionLevel = 1 }, 0L).CargoMultiplier, "1단계 ×1.5");
            AssertNear(2f, Entitlements.Effective(new PurchaseState { CargoExpansionLevel = 2 }, 0L).CargoMultiplier, "2단계 ×2");
            AssertNear(3f, Entitlements.Effective(new PurchaseState { CargoExpansionLevel = 3 }, 0L).CargoMultiplier, "3단계 ×3");
            AssertNear(1f, Entitlements.Effective(new PurchaseState { CargoExpansionLevel = -1 }, 0L).CargoMultiplier, "음수는 0단계로 클램프");
            AssertNear(3f, Entitlements.Effective(new PurchaseState { CargoExpansionLevel = 99 }, 0L).CargoMultiplier, "범위 밖 큰 값은 3단계로 클램프");
        });

        Test("M-06 Entitlements: 구독 중이면 화물칸 +50% 등 구독 혜택이 전부 켜진다", () =>
        {
            var state = new PurchaseState { SeasonPassSubscriptionExpiryUnixSeconds = 2000L };
            var e = Entitlements.Effective(state, nowUnixSeconds: 1000L);
            AssertNear(1.5f, e.CargoMultiplier, "구독만 있으면 화물칸 ×1.5");
            Assert(e.AutoRefineryAlwaysOn, "구독 중 자동 제련 상시 켜짐");
            Assert(e.AdsRemoved, "구독 중 광고 제거");
            Assert(e.BonusFuelCapacity == 2, "구독 중 대전권 +2");
            Assert(e.DailyRefinedMineralsGrant, "구독 중 매일 정제 광물 지급 대상");
        });

        Test("M-06 Entitlements: 구독 만료 시각이 지금보다 지나면(경계값 포함) 혜택이 전부 꺼진다", () =>
        {
            var expired = new PurchaseState { SeasonPassSubscriptionExpiryUnixSeconds = 1000L };
            Assert(!Entitlements.Effective(expired, nowUnixSeconds: 1000L).AutoRefineryAlwaysOn, "만료 시각과 지금이 정확히 같으면 만료로 본다");
            Assert(!Entitlements.Effective(expired, nowUnixSeconds: 1001L).AutoRefineryAlwaysOn, "만료 시각을 지났으면 당연히 꺼짐");
            Assert(Entitlements.Effective(expired, nowUnixSeconds: 999L).AutoRefineryAlwaysOn, "만료 전이면 아직 켜짐");
        });

        Test("M-06 Entitlements: 화물칸 확장(영구)과 구독이 겹치면 곱하지 않고 더 큰 값 하나만 적용된다(중복 없음)", () =>
        {
            // monetization.md 2-5: "구독과 영구 구매가 겹치면 더 큰 값 적용, 중복 차감 없음".
            // 3단계(×3)를 산 사람이 구독까지 하면 ×3이어야 한다 — ×3×1.5=×4.5로 곱해지면 중복 적용 버그.
            var state = new PurchaseState { CargoExpansionLevel = 3, SeasonPassSubscriptionExpiryUnixSeconds = 2000L };
            AssertNear(3f, Entitlements.Effective(state, nowUnixSeconds: 1000L).CargoMultiplier, "영구 3단계가 구독 배율보다 크면 3단계 값이 이긴다");

            // 반대로 영구 구매가 구독보다 낮으면(0~1단계) 구독 배율(×1.5)이 이겨야 한다.
            var lowLevel = new PurchaseState { CargoExpansionLevel = 0, SeasonPassSubscriptionExpiryUnixSeconds = 2000L };
            AssertNear(1.5f, Entitlements.Effective(lowLevel, nowUnixSeconds: 1000L).CargoMultiplier, "구독 배율이 0단계보다 크면 구독 쪽이 이긴다");
        });

        Test("M-06 Entitlements: Steam 서포터 팩(영구)은 만료 없이 구독과 똑같은 혜택을 준다", () =>
        {
            var e = Entitlements.Effective(new PurchaseState { SteamSupporterPackPurchased = true }, nowUnixSeconds: long.MaxValue / 2);
            Assert(e.AutoRefineryAlwaysOn && e.AdsRemoved && e.BonusFuelCapacity == 2, "구독 만료 개념이 없어 아무리 나중이어도 계속 켜짐");
        });

        Test("M-06 Entitlements: 광고 제거는 개별 구매·구독 아무 쪽이나 있으면 켜진다(OR)", () =>
        {
            Assert(Entitlements.Effective(new PurchaseState { AdRemovalPurchased = true }, 0L).AdsRemoved, "개별 구매만 있어도 켜짐");
            Assert(!Entitlements.Effective(default, 0L).AdsRemoved, "둘 다 없으면 꺼짐");
        });

        Test("M-06 Entitlements: 오프라인 상한 연장은 구독과 무관하게 그 구매 하나로만 결정된다", () =>
        {
            AssertNear(12f, Entitlements.Effective(new PurchaseState { OfflineCapExtensionPurchased = true }, 0L).OfflineCapHours, "구매하면 12시간");
            // monetization.md 2-5의 구독 혜택 목록에 오프라인 상한 연장은 없다 — 구독만으론 안 늘어나야 함.
            var subOnly = new PurchaseState { SeasonPassSubscriptionExpiryUnixSeconds = 2000L };
            AssertNear(4f, Entitlements.Effective(subOnly, nowUnixSeconds: 1000L).OfflineCapHours, "구독만으로는 오프라인 상한이 안 늘어난다");
        });

        Test("M-06 Entitlements: 채굴 가속 패스는 산출 배율만 올리고 화물칸·구독 혜택과는 무관하다", () =>
        {
            var e = Entitlements.Effective(new PurchaseState { MiningAccelPassExpiryUnixSeconds = 2000L }, nowUnixSeconds: 1000L);
            AssertNear(2f, e.MiningYieldMultiplier, "가속 패스 중이면 산출 ×2");
            AssertNear(1f, e.CargoMultiplier, "가속 패스는 화물칸 배율에 영향 없음");
            Assert(!e.AutoRefineryAlwaysOn, "가속 패스는 구독 혜택이 아니다");

            var expired = Entitlements.Effective(new PurchaseState { MiningAccelPassExpiryUnixSeconds = 500L }, nowUnixSeconds: 1000L);
            AssertNear(1f, expired.MiningYieldMultiplier, "만료되면 배율 1로 돌아옴");
        });

        // M-11: Steam 판 분기. Entitlements(구매·구독)와는 완전히 독립 — 판 자체가 다른 것.
        Test("M-11 PlatformConfig: 화물칸 기본 배율은 Steam만 1.5배, 모바일은 그대로", () =>
        {
            AssertNear(1f, PlatformConfig.CargoBaseMultiplier(StorePlatform.Mobile), "모바일 배율 1");
            AssertNear(1.5f, PlatformConfig.CargoBaseMultiplier(StorePlatform.Steam), "Steam 배율 1.5");
        });

        Test("M-11 PlatformConfig: 광고 제거·구독은 Steam에서 빠지고, 서포터 팩은 Steam에서만 보인다", () =>
        {
            Assert(PlatformConfig.IsShopItemAvailable(ShopSkuId.AdRemoval, StorePlatform.Mobile), "모바일엔 광고 제거가 있다");
            Assert(!PlatformConfig.IsShopItemAvailable(ShopSkuId.AdRemoval, StorePlatform.Steam), "Steam엔 광고가 없어 제거 항목도 없다");

            Assert(PlatformConfig.IsShopItemAvailable(ShopSkuId.SeasonPassSubscription, StorePlatform.Mobile), "모바일엔 구독이 있다");
            Assert(!PlatformConfig.IsShopItemAvailable(ShopSkuId.SeasonPassSubscription, StorePlatform.Steam), "Steam엔 구독 대신 서포터 팩");

            Assert(!PlatformConfig.IsShopItemAvailable(ShopSkuId.SteamSupporterPack, StorePlatform.Mobile), "서포터 팩은 모바일엔 안 보인다");
            Assert(PlatformConfig.IsShopItemAvailable(ShopSkuId.SteamSupporterPack, StorePlatform.Steam), "서포터 팩은 Steam 전용");
        });

        Test("M-11 PlatformConfig: 나머지 SKU(화물칸 확장·오프라인 연장·가속 패스·스타터 팩)는 두 판 다 판다", () =>
        {
            foreach (var sku in new[]
                     {
                         ShopSkuId.StarterPack, ShopSkuId.CargoExpansion1, ShopSkuId.CargoExpansion2,
                         ShopSkuId.CargoExpansion3, ShopSkuId.OfflineCapExtension, ShopSkuId.MiningAccelPass,
                     })
            {
                Assert(PlatformConfig.IsShopItemAvailable(sku, StorePlatform.Mobile), $"{sku} 모바일에서 판매");
                Assert(PlatformConfig.IsShopItemAvailable(sku, StorePlatform.Steam), $"{sku} Steam에서도 판매");
            }
        });

        // 둘 다 `platform == StorePlatform.Steam` / `platform != StorePlatform.Steam` 비교로만 갈리는
        // 구조라, ShopSkuId(RigPartApply의 RigSlot과 같은 자리)처럼 정의 밖 값을 막는 방어 코드가 없다.
        // 대신 "Steam이 아니면 전부 모바일과 같다"로 자연히 떨어지는데, 이게 우연이 아니라 계속
        // 이렇게 동작해야 한다는 것을 테스트로 못 박아 둔다 — 빌드 쪽이 잘못된 정수를 캐스팅해 넘겨도
        // 상점이 예외로 죽는 대신 모바일 취급으로 안전하게 떨어져야 한다.
        Test("M-11 PlatformConfig: 정의 밖 StorePlatform은 모바일과 똑같이 취급된다(예외 없음)", () =>
        {
            var undefined = (StorePlatform)99;
            AssertNear(1f, PlatformConfig.CargoBaseMultiplier(undefined), "정의 밖 값도 화물칸 배율은 1(모바일과 동일)");
            Assert(PlatformConfig.IsShopItemAvailable(ShopSkuId.AdRemoval, undefined), "정의 밖 값도 광고 제거는 보인다");
            Assert(PlatformConfig.IsShopItemAvailable(ShopSkuId.SeasonPassSubscription, undefined), "정의 밖 값도 구독은 보인다");
            Assert(!PlatformConfig.IsShopItemAvailable(ShopSkuId.SteamSupporterPack, undefined), "정의 밖 값엔 서포터 팩이 안 보인다(Steam 전용)");
        });

        // M-07: 상점 가격표(CSV) + 구매 반영 함수 + 세이브 왕복.
        Test("상점 CSV: 가격표가 DefaultData와 일치한다", () =>
        {
            var parsed = BalanceCsv.ParseShopItems(File.ReadAllText(BalancePath("shop.csv")));
            var expected = DefaultData.ShopItems();
            Assert(parsed.Count == expected.Count, $"항목 수 {parsed.Count} == {expected.Count}");
            for (var i = 0; i < expected.Count; i++)
            {
                Assert(parsed[i].SkuId == expected[i].SkuId, $"[{i}] skuId {parsed[i].SkuId} == {expected[i].SkuId}");
                Assert(parsed[i].NameKo == expected[i].NameKo, $"[{i}] nameKo {parsed[i].NameKo} == {expected[i].NameKo}");
                Assert(parsed[i].PriceKrw == expected[i].PriceKrw, $"[{i}] priceKrw {parsed[i].PriceKrw} == {expected[i].PriceKrw}");
            }
        });

        Test("ShopPurchase: 화물칸 확장은 낮은 단계를 다시 사도 단계가 안 내려간다", () =>
        {
            var state = new PurchaseState { CargoExpansionLevel = 2 };
            state = ShopPurchase.Apply(state, ShopSkuId.CargoExpansion1, nowUnixSeconds: 0L);
            Assert(state.CargoExpansionLevel == 2, $"2단계 보유 중 1단계를 사도 그대로 2단계, 실제 {state.CargoExpansionLevel}");

            state = ShopPurchase.Apply(state, ShopSkuId.CargoExpansion3, nowUnixSeconds: 0L);
            Assert(state.CargoExpansionLevel == 3, $"3단계를 사면 3단계로 오름, 실제 {state.CargoExpansionLevel}");
        });

        Test("ShopPurchase: CargoExpansion2 SKU를 직접 사면 2단계가 된다", () =>
        {
            // 1·3단계는 위 테스트가 이미 보지만 2단계는 여태 초기 상태로만(직접 Apply 호출 없이)
            // 확인되고 있었다 — switch 케이스가 실수로 1이나 3을 주는 복붙 버그가 있어도 못 잡는 구멍.
            var state = ShopPurchase.Apply(default, ShopSkuId.CargoExpansion2, nowUnixSeconds: 0L);
            Assert(state.CargoExpansionLevel == 2, $"CargoExpansion2를 사면 2단계, 실제 {state.CargoExpansionLevel}");
        });

        Test("ShopPurchase: 스타터 팩은 화물칸 확장 1단계를 준다", () =>
        {
            var state = ShopPurchase.Apply(default, ShopSkuId.StarterPack, nowUnixSeconds: 0L);
            Assert(state.CargoExpansionLevel == 1, "스타터 팩 = 화물칸 확장 1단계");
        });

        Test("ShopPurchase: 기간제(가속 패스·구독)는 활성 중에 또 사면 만료 시각부터 기간이 이어 붙는다", () =>
        {
            var state = new PurchaseState { MiningAccelPassExpiryUnixSeconds = 2000L };
            const long durationSeconds = 30L * 24 * 3600;
            state = ShopPurchase.Apply(state, ShopSkuId.MiningAccelPass, nowUnixSeconds: 1000L);
            Assert(state.MiningAccelPassExpiryUnixSeconds == 2000L + durationSeconds,
                $"기존 만료(2000)부터 30일 더, 실제 {state.MiningAccelPassExpiryUnixSeconds}");

            var expiredState = new PurchaseState { MiningAccelPassExpiryUnixSeconds = 500L };
            expiredState = ShopPurchase.Apply(expiredState, ShopSkuId.MiningAccelPass, nowUnixSeconds: 1000L);
            Assert(expiredState.MiningAccelPassExpiryUnixSeconds == 1000L + durationSeconds,
                $"이미 만료됐으면 지금부터 30일, 실제 {expiredState.MiningAccelPassExpiryUnixSeconds}");
        });

        Test("ShopPurchase: 만료 시각과 지금이 정확히 같으면(경계값) 이미 만료된 것으로 보고 지금부터 이어 붙인다", () =>
        {
            // ExtendFrom은 `currentExpiry > now`일 때만 "아직 활성"으로 본다 — 딱 같은 순간은
            // 활성이 아니라 막 끝난 것으로 취급한다는 뜻. 위 두 테스트는 확실히 활성(2000>1000)과
            // 확실히 만료(500<1000)만 보고 있어서 이 경계 자체는 아무도 확인하지 않고 있었다.
            const long durationSeconds = 30L * 24 * 3600;
            var state = new PurchaseState { MiningAccelPassExpiryUnixSeconds = 1000L };
            state = ShopPurchase.Apply(state, ShopSkuId.MiningAccelPass, nowUnixSeconds: 1000L);
            Assert(state.MiningAccelPassExpiryUnixSeconds == 1000L + durationSeconds,
                $"만료 시각==지금이면 지금부터 30일(활성 취급 아님), 실제 {state.MiningAccelPassExpiryUnixSeconds}");
        });

        Test("ShopPurchase: 영구 항목(오프라인 연장·Steam 팩·광고 제거)은 bool을 켠다", () =>
        {
            var state = default(PurchaseState);
            state = ShopPurchase.Apply(state, ShopSkuId.OfflineCapExtension, 0L);
            state = ShopPurchase.Apply(state, ShopSkuId.SteamSupporterPack, 0L);
            state = ShopPurchase.Apply(state, ShopSkuId.AdRemoval, 0L);
            Assert(state.OfflineCapExtensionPurchased && state.SteamSupporterPackPurchased && state.AdRemovalPurchased,
                "세 bool 전부 켜짐");
        });

        Test("ShopPurchase: 정의 밖 SkuId는 예외를 던진다(카탈로그에 없는 값이 결제 SDK에서 잘못 들어와도 조용히 넘어가지 않는다)", () =>
        {
            var threw = false;
            try { ShopPurchase.Apply(default, (ShopSkuId)9999, nowUnixSeconds: 0L); }
            catch (ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "정의 밖 SkuId는 ArgumentOutOfRangeException");
        });

        Test("ShopPurchase: 만료 시각이 지금과 정확히 같으면(< 아니라 <=) 이미 만료된 것으로 보고 지금부터 다시 잰다", () =>
        {
            const long durationSeconds = 30L * 24 * 3600;
            var state = new PurchaseState { SeasonPassSubscriptionExpiryUnixSeconds = 1000L };
            state = ShopPurchase.Apply(state, ShopSkuId.SeasonPassSubscription, nowUnixSeconds: 1000L);
            Assert(state.SeasonPassSubscriptionExpiryUnixSeconds == 1000L + durationSeconds,
                $"만료 시각==현재 시각이면 그 시각을 기준으로 남은 기간 취급하지 않고 지금부터 30일, 실제 {state.SeasonPassSubscriptionExpiryUnixSeconds}");
        });

        Test("ShopPurchase: 스타터 팩도 화물칸 확장처럼 이미 더 높은 단계를 갖고 있으면 단계를 안 내린다", () =>
        {
            var state = new PurchaseState { CargoExpansionLevel = 3 };
            state = ShopPurchase.Apply(state, ShopSkuId.StarterPack, nowUnixSeconds: 0L);
            Assert(state.CargoExpansionLevel == 3, $"3단계 보유 중 스타터 팩을 사도 그대로 3단계, 실제 {state.CargoExpansionLevel}");
        });

        Test("SaveData: PurchaseState 왕복 — 0은 null로, null은 0으로(JsonUtility가 long?을 못 다뤄서)", () =>
        {
            var save = new SaveData();
            Assert(save.ToPurchaseState().MiningAccelPassExpiryUnixSeconds == null, "기본값 0은 '산 적 없음'(null)");

            var applied = ShopPurchase.Apply(save.ToPurchaseState(), ShopSkuId.SeasonPassSubscription, nowUnixSeconds: 100L);
            save.ApplyPurchaseState(applied);
            Assert(save.SeasonPassSubscriptionExpiryUnixSeconds > 0, "세이브 필드에는 실제 만료 시각(long)이 남는다");
            Assert(save.ToPurchaseState().SeasonPassSubscriptionExpiryUnixSeconds == save.SeasonPassSubscriptionExpiryUnixSeconds,
                "다시 PurchaseState로 바꾸면 같은 값이 null이 아니라 그대로 나온다");
        });

        // M-08: 스타터 팩 노출 판정. "첫 상한 도달 직후 한 번만" — 사든 거절하든 다시 안 뜬다.
        Test("StarterPackOffer: 아직 상한에 안 닿았으면 다른 조건과 무관하게 안 보여준다", () =>
        {
            Assert(!StarterPackOffer.ShouldShow(hasReachedCargoCapBefore: false, declined: false, cargoExpansionLevel: 0),
                "상한에 닿은 적이 없으면 false");
        });

        Test("StarterPackOffer: 상한에 닿았고 거절한 적 없고 화물칸 확장이 없으면 보여준다", () =>
        {
            Assert(StarterPackOffer.ShouldShow(hasReachedCargoCapBefore: true, declined: false, cargoExpansionLevel: 0),
                "세 조건이 다 맞으면 true");
        });

        Test("StarterPackOffer: 이미 거절했으면 상한에 또 닿아도 다시 안 보여준다", () =>
        {
            Assert(!StarterPackOffer.ShouldShow(hasReachedCargoCapBefore: true, declined: true, cargoExpansionLevel: 0),
                "declined=true면 false");
        });

        Test("StarterPackOffer: 화물칸 확장을 이미 가지고 있으면(스타터 팩이든 개별 SKU든) 다시 안 보여준다", () =>
        {
            Assert(!StarterPackOffer.ShouldShow(hasReachedCargoCapBefore: true, declined: false, cargoExpansionLevel: 1),
                "1단계만 있어도 false");
            Assert(!StarterPackOffer.ShouldShow(hasReachedCargoCapBefore: true, declined: false, cargoExpansionLevel: 3),
                "3단계는 물론 false");
        });

        Test("StarterPackOffer: 경계값(화물칸 확장 단계 음수)도 예외 없이 상식적인 값", () =>
        {
            Assert(StarterPackOffer.ShouldShow(hasReachedCargoCapBefore: true, declined: false, cargoExpansionLevel: -1),
                "음수는 '아직 안 삼'과 같게 취급 — true");
        });

        // M-09: 보상형 광고 네 자리의 하루 한도 카운터. monetization.md 3장 — 3/3/2/2회.
        const long Kst = 9 * 3600; // UTC+9, 글루 레이어가 실제로 넘길 값과 같은 오프셋으로 테스트
        Test("RewardAdTracker: 새 상태는 네 자리 전부 오늘 한도만큼 볼 수 있다", () =>
        {
            var state = new RewardAdState();
            long now = 1_800_000_000; // 임의의 UTC 시각
            Assert(RewardAdTracker.CanWatch(state, RewardAdSlot.OfflineRewardDouble, now, Kst), "오프라인 2배 CanWatch");
            Assert(RewardAdTracker.RemainingToday(state, RewardAdSlot.OfflineRewardDouble, now, Kst) == 3, "오프라인 2배 3회");
            Assert(RewardAdTracker.RemainingToday(state, RewardAdSlot.ExtraLootBox, now, Kst) == 3, "상자 1개 더 3회");
            Assert(RewardAdTracker.RemainingToday(state, RewardAdSlot.CargoCapDoubleHour, now, Kst) == 2, "상한 2배 2회");
            Assert(RewardAdTracker.RemainingToday(state, RewardAdSlot.FuelRefill, now, Kst) == 2, "연료 +3 2회");
        });

        Test("RewardAdTracker: RecordWatch는 그 자리 카운트만 올리고 다른 자리는 그대로다", () =>
        {
            var state = new RewardAdState();
            long now = 1_800_000_000;
            state = RewardAdTracker.RecordWatch(state, RewardAdSlot.ExtraLootBox, now, Kst);
            Assert(RewardAdTracker.WatchedToday(state, RewardAdSlot.ExtraLootBox) == 1, "상자 자리만 1회");
            Assert(RewardAdTracker.WatchedToday(state, RewardAdSlot.OfflineRewardDouble) == 0, "다른 자리는 안 건드림");
            Assert(RewardAdTracker.RemainingToday(state, RewardAdSlot.ExtraLootBox, now, Kst) == 2, "남은 횟수 2");
        });

        Test("RewardAdTracker: 한도를 다 쓰면 CanWatch가 false, RecordWatch를 더 불러도 카운트가 안 넘는다", () =>
        {
            var state = new RewardAdState();
            long now = 1_800_000_000;
            for (int i = 0; i < RewardAdTracker.CargoCapDoubleHourDailyLimit; i++)
                state = RewardAdTracker.RecordWatch(state, RewardAdSlot.CargoCapDoubleHour, now, Kst);
            Assert(!RewardAdTracker.CanWatch(state, RewardAdSlot.CargoCapDoubleHour, now, Kst), "2회 다 쓰면 false");

            state = RewardAdTracker.RecordWatch(state, RewardAdSlot.CargoCapDoubleHour, now, Kst); // 한도 넘겨 한 번 더 호출
            Assert(RewardAdTracker.WatchedToday(state, RewardAdSlot.CargoCapDoubleHour) == RewardAdTracker.CargoCapDoubleHourDailyLimit,
                "한도를 넘겨 부르면 카운트가 그대로(2)여야 함 — RaceFuel.Recover가 MaxFuel을 안 넘기는 것과 같은 방어");
        });

        Test("RewardAdTracker: 날짜가 바뀌면(KST 자정을 넘기면) 네 자리 전부 리셋된다", () =>
        {
            var state = new RewardAdState();
            long day1 = 1_800_000_000; // 어떤 날의 KST 낮 시각
            state = RewardAdTracker.RecordWatch(state, RewardAdSlot.OfflineRewardDouble, day1, Kst);
            state = RewardAdTracker.RecordWatch(state, RewardAdSlot.FuelRefill, day1, Kst);
            Assert(RewardAdTracker.WatchedToday(state, RewardAdSlot.OfflineRewardDouble) == 1, "리셋 전 1회");

            long day2 = day1 + 86400; // 정확히 하루 뒤(같은 시각대라 KST 자정을 확실히 넘음)
            var resetState = RewardAdTracker.ResetIfNewDay(state, day2, Kst);
            Assert(RewardAdTracker.WatchedToday(resetState, RewardAdSlot.OfflineRewardDouble) == 0, "다음 날엔 0으로 리셋");
            Assert(RewardAdTracker.WatchedToday(resetState, RewardAdSlot.FuelRefill) == 0, "다른 자리도 같이 리셋");
            Assert(RewardAdTracker.RemainingToday(state, RewardAdSlot.OfflineRewardDouble, day2, Kst) == 3,
                "ResetIfNewDay를 안 거쳐도(RemainingToday 안에서) 새 날짜면 알아서 리셋된 값을 돌려줌");
        });

        Test("RewardAdTracker: 정의 밖 RewardAdSlot 값은 세 진입점 전부 ArgumentOutOfRangeException", () =>
        {
            // enum 자체엔 방어가 없어서(값 검증 없는 캐스팅) DailyLimit/WatchedToday/RecordWatch
            // switch의 default 분기가 실제로 막아 주는지 확인한다 — RemainingToday/CanWatch는
            // 이 셋을 거쳐 가니 따로 안 봐도 됨.
            var badSlot = (RewardAdSlot)99;
            var state = new RewardAdState();

            var threwLimit = false;
            try { RewardAdTracker.DailyLimit(badSlot); } catch (ArgumentOutOfRangeException) { threwLimit = true; }
            Assert(threwLimit, "DailyLimit: 정의 밖 슬롯은 ArgumentOutOfRangeException");

            var threwWatched = false;
            try { RewardAdTracker.WatchedToday(state, badSlot); } catch (ArgumentOutOfRangeException) { threwWatched = true; }
            Assert(threwWatched, "WatchedToday: 정의 밖 슬롯은 ArgumentOutOfRangeException");

            var threwRecord = false;
            try { RewardAdTracker.RecordWatch(state, badSlot, 0, Kst); } catch (ArgumentOutOfRangeException) { threwRecord = true; }
            Assert(threwRecord, "RecordWatch: CanWatch(내부 RemainingToday→DailyLimit)를 거치며 정의 밖 슬롯이 걸림");
        });

        Test("RewardAdTracker: 같은 KST 하루 안에서는(UTC 날짜가 갈려도) 리셋되지 않는다", () =>
        {
            // KST 자정 = UTC 15:00. UTC 23:00(=KST 08:00)과 UTC 23:00+3시간(=KST 11:00)은
            // UTC 날짜는 같은 날 그대로지만, 대신 KST 기준으로도 같은 하루임을 확인한다 —
            // 그리고 UTC 자정을 넘나드는 UTC 22:00→UTC 23:30(KST 07:00→08:30)도 KST로는 같은 날.
            long utc2200 = 1_800_000_000 - (1_800_000_000 % 86400) + 22 * 3600;
            long utc2330 = utc2200 + 90 * 60;
            var state = RewardAdTracker.RecordWatch(new RewardAdState(), RewardAdSlot.ExtraLootBox, utc2200, Kst);
            var later = RewardAdTracker.ResetIfNewDay(state, utc2330, Kst);
            Assert(RewardAdTracker.WatchedToday(later, RewardAdSlot.ExtraLootBox) == 1,
                "UTC 자정을 넘겨도 KST로 같은 하루면 리셋 안 됨");
        });

        Test("RewardAdTracker: DayIndex는 시간대 오프셋이 음수여도(서쪽) 예외 없이 하루 단위로 떨어진다", () =>
        {
            const long Pst = -8 * 3600; // UTC-8
            long midnightUtc = 1_800_000_000 - (1_800_000_000 % 86400); // 어떤 UTC 자정
            var before = RewardAdTracker.DayIndex(midnightUtc - 1, Pst); // PST로는 아직 전날 오후
            var after = RewardAdTracker.DayIndex(midnightUtc, Pst);
            Assert(before == after, "UTC 자정을 넘나들어도 PST 기준으로는 아직 같은 하루");
        });

        Test("RewardAdTracker: DayIndex — 로컬 시각(now+오프셋)이 음수로 떨어지는 경우도 floor로 계산", () =>
        {
            // epoch(0) 근처 + 서쪽 시간대는 로컬 시각이 음수가 된다. C#의 정수 나눗셈은 0쪽으로
            // 버리므로(-28800 / 86400 == 0) floor 보정이 없으면 하루 전(-1)이어야 할 값이 0(1970-01-01)로
            // 잘못 나온다 — 게임 실사용 범위 밖이지만 "0·음수·경계값"은 이 저장소 관례상 항상 확인한다.
            const long Pst = -8 * 3600;
            Assert(RewardAdTracker.DayIndex(0, Pst) == -1, "epoch 0을 PST로 보면 아직 전날(-1)");
            Assert(RewardAdTracker.DayIndex(8 * 3600, Pst) == 0, "정확히 로컬 자정(0)이면 그날(0)");
            Assert(RewardAdTracker.DayIndex(8 * 3600 - 1, Pst) == -1, "그 1초 전은 여전히 전날(-1)");
        });

        Test("RewardAdBoost: 만료 전엔 배율 2, 만료 시각과 정확히 같거나 지나면 1로 꺼진다", () =>
        {
            Assert(RewardAdBoost.CargoCapMultiplier(100, 200) == 2f, "아직 안 지났으면 2배");
            Assert(RewardAdBoost.CargoCapMultiplier(200, 200) == 1f, "만료 시각과 정확히 같으면 이미 꺼짐(경계값)");
            Assert(RewardAdBoost.CargoCapMultiplier(201, 200) == 1f, "지났으면 1배");
            Assert(RewardAdBoost.CargoCapMultiplier(100, 0) == 1f, "한 번도 안 켠 상태(0)는 1배");
        });

        Test("RewardAdBoost: 꺼진 상태에서 보면 지금부터 1시간, 켜진 중에 또 보면 만료 시각부터 이어 붙는다", () =>
        {
            Assert(RewardAdBoost.ExtendCargoCapDoubleHour(0, 1000) == 1000 + RewardAdBoost.CargoCapDoubleHourSeconds,
                "꺼진 상태(0)에서 보면 지금(1000)부터 1시간");
            Assert(RewardAdBoost.ExtendCargoCapDoubleHour(500, 1000) == 1000 + RewardAdBoost.CargoCapDoubleHourSeconds,
                "이미 만료된 과거(500 < 1000)여도 지금부터 1시간(과거 만료 시각을 그대로 더하지 않는다)");
            Assert(RewardAdBoost.ExtendCargoCapDoubleHour(5000, 1000) == 5000 + RewardAdBoost.CargoCapDoubleHourSeconds,
                "아직 켜진 중(5000 > 1000)이면 지금이 아니라 원래 만료 시각부터 이어 붙인다");
        });

        Test("SeasonPassProgress: XP 0은 레벨 0(아직 1레벨도 못 참)", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            Assert(SeasonPassProgress.LevelForXp(tiers, 0) == 0, "XP 0 → 레벨 0");
            Assert(SeasonPassProgress.LevelForXp(tiers, 99) == 0, "레벨 1 문턱(100) 바로 아래는 아직 0");
            Assert(SeasonPassProgress.LevelForXp(tiers, 100) == 1, "정확히 100이면 레벨 1(경계값)");
            Assert(SeasonPassProgress.LevelForXp(tiers, 250) == 2, "레벨 2와 3 사이 XP는 레벨 2");
            Assert(SeasonPassProgress.LevelForXp(tiers, 1000) == 10, "마지막 티어(1000) 정확히 도달");
            Assert(SeasonPassProgress.LevelForXp(tiers, 999999) == 10, "마지막 티어를 넘겨도 10에서 멈춘다(상한)");
        });

        Test("SeasonPassProgress: 레벨에 안 닿았으면 무료든 유료든 못 받는다", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            var state = new SeasonPassState { CurrentXp = 50, OwnsPaidTrack = true };
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Free, 1), "레벨 1도 아직 못 참");
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Paid, 1), "유료도 마찬가지");
        });

        Test("SeasonPassProgress: 무료 트랙은 유료 트랙 보유 여부와 무관하게 받을 수 있다", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            var state = new SeasonPassState { CurrentXp = 100, OwnsPaidTrack = false };
            Assert(SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Free, 1), "무료 트랙은 그냥 받을 수 있다");
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Paid, 1), "유료 트랙 안 샀으면 유료는 못 받는다");
        });

        Test("SeasonPassProgress: 이미 받은 티어는 다시 못 받는다(무료·유료 각각)", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            var state = new SeasonPassState { CurrentXp = 500, OwnsPaidTrack = true };
            state = SeasonPassProgress.Claim(tiers, state, SeasonPassTrack.Free, 1, out var freeReward);
            Assert(freeReward.Kind == SeasonPassRewardKind.RawMinerals, "레벨 1 무료 보상은 원석");
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Free, 1), "무료는 한 번 받으면 끝");
            Assert(SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Paid, 1), "유료 쪽은 아직 안 건드렸으니 그대로 받을 수 있다");

            state = SeasonPassProgress.Claim(tiers, state, SeasonPassTrack.Paid, 1, out var paidReward);
            Assert(paidReward.Kind == SeasonPassRewardKind.RefinedMinerals, "레벨 1 유료 보상은 정제 광물");
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Paid, 1), "유료도 한 번 받으면 끝");
        });

        Test("SeasonPassProgress: CanClaim이 false일 때 Claim을 불러도 상태가 안 바뀐다(방어적 이중 확인)", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            var state = new SeasonPassState { CurrentXp = 0, OwnsPaidTrack = false };
            var result = SeasonPassProgress.Claim(tiers, state, SeasonPassTrack.Free, 1, out var reward);
            Assert(result.ClaimedFreeTierMask == 0, "레벨에 안 닿았으니 마스크가 그대로 0");
            Assert(reward.Kind == default(SeasonPassRewardKind) && reward.Amount == 0f, "reward는 default 그대로");
        });

        Test("SeasonPassProgress: 서로 다른 레벨의 비트마스크가 겹치지 않는다", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            var state = new SeasonPassState { CurrentXp = 1000, OwnsPaidTrack = true };
            state = SeasonPassProgress.Claim(tiers, state, SeasonPassTrack.Free, 3, out _);
            state = SeasonPassProgress.Claim(tiers, state, SeasonPassTrack.Free, 7, out _);
            Assert(SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Free, 3), "3레벨은 받음");
            Assert(SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Free, 7), "7레벨도 받음");
            Assert(!SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Free, 1), "1레벨은 안 건드렸으니 그대로");
            Assert(!SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Free, 4), "4레벨도 안 건드렸으니 그대로");
            Assert(!SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Paid, 3), "유료 마스크는 무료와 별개라 그대로 0");
        });

        Test("SeasonPassProgress: 티어 범위 밖 레벨(0·범위 초과)은 CanClaim이 조용히 false", () =>
        {
            var tiers = DefaultData.SeasonPassTiers();
            var state = new SeasonPassState { CurrentXp = 1000, OwnsPaidTrack = true };
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Free, 0), "레벨 0은 범위 밖");
            Assert(!SeasonPassProgress.CanClaim(tiers, state, SeasonPassTrack.Free, tiers.Length + 1), "티어 개수를 넘는 레벨도 범위 밖");
        });

        Test("SeasonPassProgress.IsClaimed: CanClaim과 달리 범위 방어가 없다 — 범위 밖 레벨은 ArgumentOutOfRangeException", () =>
        {
            // CanClaim은 호출 첫 줄에서 범위를 걸러 false를 돌려주지만, IsClaimed는 그 가드 없이
            // 바로 BitFor(level)을 부른다(SeasonPass.cs). Claim은 항상 CanClaim을 먼저 거쳐서
            // 이 경로를 못 타지만, 화면 코드가 CanClaim 없이 IsClaimed만 직접 물어보면(예: 배지
            // 표시용 조회) 범위 밖 레벨에서 그대로 터진다 — 조용히 false가 아니라 예외임을 고정해 둔다.
            var state = new SeasonPassState();

            var threwZero = false;
            try { SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Free, 0); }
            catch (ArgumentOutOfRangeException) { threwZero = true; }
            Assert(threwZero, "레벨 0은 ArgumentOutOfRangeException");

            var threwOver = false;
            try { SeasonPassProgress.IsClaimed(state, SeasonPassTrack.Free, SeasonPassProgress.MaxTiers + 1); }
            catch (ArgumentOutOfRangeException) { threwOver = true; }
            Assert(threwOver, "MaxTiers(63)를 넘는 레벨도 ArgumentOutOfRangeException — 참고로 실제 콘텐츠는 10티어뿐이라" +
                " tiers.Length+1(11)은 MaxTiers 안이라 여기선 안 터진다, BitFor가 보는 건 콘텐츠 길이가 아니라 MaxTiers다");
        });

        Test("SeasonPassProgress.AddXp: 누적되고, 음수는 예외", () =>
        {
            var state = new SeasonPassState();
            state = SeasonPassProgress.AddXp(state, 30);
            state = SeasonPassProgress.AddXp(state, 45);
            Assert(state.CurrentXp == 75, "30 + 45 = 75");
            var threw = false;
            try { SeasonPassProgress.AddXp(state, -1); }
            catch (ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "음수 XP는 ArgumentOutOfRangeException");
        });

        Test("SeasonPassTiers: 무료 트랙엔 힘(RigPart/LootBox)·원석만, 유료 트랙엔 시간 단축·꾸미기만", () =>
        {
            // monetization.md "절대 팔지 않는 것 — 시즌 패스 유료 트랙의 전투력 보상"을 코드로도 지키는지.
            foreach (var tier in DefaultData.SeasonPassTiers())
            {
                if (tier.FreeReward.HasValue)
                {
                    var kind = tier.FreeReward.Value.Kind;
                    Assert(kind == SeasonPassRewardKind.RigPart || kind == SeasonPassRewardKind.LootBox || kind == SeasonPassRewardKind.RawMinerals,
                        $"레벨 {tier.Level} 무료 보상 종류({kind})가 힘/원석 계열이 아니다");
                }
                if (tier.PaidReward.HasValue)
                {
                    var kind = tier.PaidReward.Value.Kind;
                    Assert(kind != SeasonPassRewardKind.RigPart && kind != SeasonPassRewardKind.LootBox,
                        $"레벨 {tier.Level} 유료 보상이 전투력 보상({kind})이면 안 된다");
                }
            }
        });

        Test("SaveData: SeasonPassState 왕복", () =>
        {
            var save = new SaveData();
            var state = new SeasonPassState
            {
                CurrentXp = 730,
                OwnsPaidTrack = true,
                ClaimedFreeTierMask = 0b1011,
                ClaimedPaidTierMask = 0b0100,
            };
            save.ApplySeasonPassState(state);
            var back = save.ToSeasonPassState();
            Assert(back.CurrentXp == 730, "CurrentXp 왕복");
            Assert(back.OwnsPaidTrack, "OwnsPaidTrack 왕복");
            Assert(back.ClaimedFreeTierMask == 0b1011, "ClaimedFreeTierMask 왕복");
            Assert(back.ClaimedPaidTierMask == 0b0100, "ClaimedPaidTierMask 왕복");
        });

        // M-14: SeasonPass.cs(M-10)에 XP를 실제로 넣어 주는 곳이 코드 전체에 없던 것을 발견하고
        // (docs/backlog.md M-14) MiningController.TryEnterRace의 우승 분기에 얹었다. 매핑 자체는
        // RaceBoxReward.ForTier와 같은 모양이라 테스트도 그 짝을 그대로 따른다.
        Test("SeasonPassRaceXp: 등급이 높을수록 더 준다", () =>
        {
            Assert(SeasonPassRaceXp.ForTier(RaceTier.Local) == 15, "로컬 15");
            Assert(SeasonPassRaceXp.ForTier(RaceTier.Circuit) == 25, "서킷 25");
            Assert(SeasonPassRaceXp.ForTier(RaceTier.Challenge) == 40, "챌린지 40");
            Assert(SeasonPassRaceXp.ForTier(RaceTier.GrandPrix) == 60, "그랑프리 60");
            Assert(SeasonPassRaceXp.ForTier(RaceTier.Local) < SeasonPassRaceXp.ForTier(RaceTier.Circuit)
                && SeasonPassRaceXp.ForTier(RaceTier.Circuit) < SeasonPassRaceXp.ForTier(RaceTier.Challenge)
                && SeasonPassRaceXp.ForTier(RaceTier.Challenge) < SeasonPassRaceXp.ForTier(RaceTier.GrandPrix),
                "등급 오름차순으로 XP도 오름차순");
        });

        Test("SeasonPassRaceXp: 정의 밖 등급(잘못된 세이브 데이터 등)은 예외 없이 XP 0", () =>
        {
            Assert(SeasonPassRaceXp.ForTier((RaceTier)999) == 0, "정의 안 된 등급은 0 — AddXp(state, 0)은 상태를 그대로 둔다");
        });

        Test("SeasonPassRaceXp: 로컬 레이스만 반복해도 4주 시즌(1000XP) 안에 마지막 티어에 닿는다", () =>
        {
            // 레이스 1회 = 연료 RaceFuel.EntryCost(1) 소모, 최소 회복은 시간에 비례하니 "하루 몇 판"이
            // 과장이 아님을 정확한 시뮬레이션 없이 대략치로만 확인 — 로컬 XP 15로 1000을 채우려면
            // 68판(15*67=1005)이 필요한데, 28일 시즌이면 하루 2.4판꼴이라 무리한 수치가 아니다.
            var tiers = DefaultData.SeasonPassTiers();
            var lastLevelXp = tiers[tiers.Length - 1].RequiredXp;
            var racesNeeded = (int)System.Math.Ceiling((double)lastLevelXp / SeasonPassRaceXp.ForTier(RaceTier.Local));
            Assert(racesNeeded <= 28 * 5, $"하루 5판을 28일 다 채워도 되는 판수({28 * 5})를 넘지 않는다, 실제 {racesNeeded}판 필요");
        });

        // M-09: PurchaseState/SeasonPassState/DailyLoginState는 왕복 테스트가 있었는데
        // RewardAdState(자리 네 개 + 리셋 날짜)만 빠져 있었다 — SaveData.ToRewardAdState/
        // ApplyRewardAdState는 다른 셋과 같은 모양(non-nullable 필드 1:1 복사)이라 구조상
        // 실패할 자리는 아니지만, 필드가 다섯 개나 돼서 하나라도 배선이 어긋나면(예: 복붙하다
        // 필드 하나를 빠뜨리면) 잡아 줄 테스트가 없었다.
        Test("SaveData: RewardAdState 왕복", () =>
        {
            var save = new SaveData();
            var state = new RewardAdState
            {
                LastResetDayIndex = 20345,
                OfflineRewardDoubleWatchedToday = 1,
                ExtraLootBoxWatchedToday = 2,
                CargoCapDoubleHourWatchedToday = 3,
                FuelRefillWatchedToday = 4,
            };
            save.ApplyRewardAdState(state);
            var back = save.ToRewardAdState();
            Assert(back.LastResetDayIndex == 20345, "LastResetDayIndex 왕복");
            Assert(back.OfflineRewardDoubleWatchedToday == 1, "OfflineRewardDoubleWatchedToday 왕복");
            Assert(back.ExtraLootBoxWatchedToday == 2, "ExtraLootBoxWatchedToday 왕복");
            Assert(back.CargoCapDoubleHourWatchedToday == 3, "CargoCapDoubleHourWatchedToday 왕복");
            Assert(back.FuelRefillWatchedToday == 4, "FuelRefillWatchedToday 왕복");
        });

        Test("DailyLoginReward: 처음 접속(0)이면 오늘 받을 수 있고, 스트릭은 1로 시작한다", () =>
        {
            var state = new DailyLoginState();
            var now = 1_800_000_000L; // 임의의 미래 시각, 실제 서비스 날짜 범위
            Assert(DailyLoginReward.CanClaim(state, now, Kst), "받은 적 없으면 오늘 받을 수 있다");
            var result = DailyLoginReward.Claim(state, now, Kst);
            Assert(result.StreakDays == 1, $"첫 접속 스트릭은 1, 실제 {result.StreakDays}");
            Assert(result.LastClaimedDayIndex == RewardAdTracker.DayIndex(now, Kst), "받은 날짜가 오늘로 기록된다");
        });

        Test("DailyLoginReward: 같은 날 두 번 접속해도 다시 못 받고 스트릭도 그대로", () =>
        {
            var now = 1_800_000_000L;
            var state = DailyLoginReward.Claim(new DailyLoginState(), now, Kst);
            Assert(!DailyLoginReward.CanClaim(state, now + 3600, Kst), "3시간 뒤 같은 날은 또 못 받는다");
            var again = DailyLoginReward.Claim(state, now + 3600, Kst);
            Assert(again.StreakDays == state.StreakDays && again.LastClaimedDayIndex == state.LastClaimedDayIndex,
                "이미 오늘 받았으면 Claim을 또 불러도 상태가 안 바뀐다(방어적 이중 확인)");
        });

        Test("DailyLoginReward: 다음 날 연속 접속이면 스트릭이 1 늘어난다", () =>
        {
            var day1 = 1_800_000_000L;
            var state = DailyLoginReward.Claim(new DailyLoginState(), day1, Kst);
            var day2 = day1 + 86400;
            Assert(DailyLoginReward.CanClaim(state, day2, Kst), "다음 날은 다시 받을 수 있다");
            state = DailyLoginReward.Claim(state, day2, Kst);
            Assert(state.StreakDays == 2, $"연속 이틀째 스트릭은 2, 실제 {state.StreakDays}");
        });

        Test("DailyLoginReward: 하루를 건너뛰면 스트릭이 1로 되돌아간다(완전 리셋은 아님)", () =>
        {
            var day1 = 1_800_000_000L;
            var state = DailyLoginReward.Claim(new DailyLoginState(), day1, Kst);
            state = DailyLoginReward.Claim(state, day1 + 86400, Kst); // 2일차, 스트릭 2
            var day4 = day1 + 3 * 86400; // 3일차를 건너뛰고 4일차 접속
            state = DailyLoginReward.Claim(state, day4, Kst);
            Assert(state.StreakDays == 1, $"하루 건너뛰면 스트릭이 1로, 실제 {state.StreakDays}");
        });

        Test("DailyLoginReward.RawMineralsFor: 7일 주기로 순환하고, 0 이하는 1일차 값으로 방어", () =>
        {
            Assert(DailyLoginReward.RawMineralsFor(1) == DailyLoginReward.RawMineralsByStreakDay[0], "1일차");
            Assert(DailyLoginReward.RawMineralsFor(7) == DailyLoginReward.RawMineralsByStreakDay[6], "7일차(마지막, 제일 큼)");
            Assert(DailyLoginReward.RawMineralsFor(8) == DailyLoginReward.RawMineralsFor(1), "8일차는 다시 1일차와 같다");
            Assert(DailyLoginReward.RawMineralsFor(14) == DailyLoginReward.RawMineralsFor(7), "14일차는 7일차와 같다");
            Assert(DailyLoginReward.RawMineralsFor(0) == DailyLoginReward.RawMineralsFor(1), "0은 1일차 값으로 방어");
            Assert(DailyLoginReward.RawMineralsFor(-5) == DailyLoginReward.RawMineralsFor(1), "음수도 1일차 값으로 방어");
        });

        Test("DailyLoginReward: 7일차 보상이 1일차보다 커서 완주 유인이 있다", () =>
        {
            Assert(DailyLoginReward.RawMineralsByStreakDay[6] > DailyLoginReward.RawMineralsByStreakDay[0],
                "7일차가 1일차보다 커야 일주일을 채울 유인이 생긴다");
            for (var i = 1; i < DailyLoginReward.RawMineralsByStreakDay.Length; i++)
            {
                Assert(DailyLoginReward.RawMineralsByStreakDay[i] >= DailyLoginReward.RawMineralsByStreakDay[i - 1],
                    $"{i + 1}일차가 {i}일차보다 작으면 안 된다(단조 증가)");
            }
        });

        Test("SaveData: DailyLoginState 왕복", () =>
        {
            var save = new SaveData();
            var state = new DailyLoginState { LastClaimedDayIndex = 20345, StreakDays = 4 };
            save.ApplyDailyLoginState(state);
            var back = save.ToDailyLoginState();
            Assert(back.LastClaimedDayIndex == 20345, "LastClaimedDayIndex 왕복");
            Assert(back.StreakDays == 4, "StreakDays 왕복");
        });

        // M-07 후속: 구독 "매일 정제 광물 지급"(SubscriptionDailyGrant.cs). Entitlements.cs가
        // TODO로 남겨 뒀던 "하루에 한 번만" 청구 타이밍을 DailyLoginReward와 같은 패턴으로 채운다.
        Test("SubscriptionDailyGrant.CanClaim: 구독 아니면 받은 적 없어도 false", () =>
        {
            var now = 1_800_000_000L;
            Assert(!SubscriptionDailyGrant.CanClaim(new SubscriptionGrantState(), false, now, Kst),
                "구독 중이 아니면 날짜와 무관하게 못 받는다");
        });

        Test("SubscriptionDailyGrant.CanClaim: 구독 중이고 받은 적 없으면 true", () =>
        {
            var now = 1_800_000_000L;
            Assert(SubscriptionDailyGrant.CanClaim(new SubscriptionGrantState(), true, now, Kst),
                "구독 중이고 오늘 아직 안 받았으면 받을 수 있다");
        });

        Test("SubscriptionDailyGrant: 같은 날 두 번 못 받는다", () =>
        {
            var now = 1_800_000_000L;
            var state = SubscriptionDailyGrant.Claim(new SubscriptionGrantState(), now, Kst);
            Assert(!SubscriptionDailyGrant.CanClaim(state, true, now + 3600, Kst), "3시간 뒤 같은 날은 또 못 받는다");
            var again = SubscriptionDailyGrant.Claim(state, now + 3600, Kst);
            Assert(again.LastClaimedDayIndex == state.LastClaimedDayIndex,
                "이미 오늘 받았으면 Claim을 또 불러도 상태가 안 바뀐다(방어적 이중 확인)");
        });

        Test("SubscriptionDailyGrant: 다음 날이면 다시 받을 수 있다", () =>
        {
            var day1 = 1_800_000_000L;
            var state = SubscriptionDailyGrant.Claim(new SubscriptionGrantState(), day1, Kst);
            var day2 = day1 + 86400;
            Assert(SubscriptionDailyGrant.CanClaim(state, true, day2, Kst), "다음 날은 다시 받을 수 있다");
            state = SubscriptionDailyGrant.Claim(state, day2, Kst);
            Assert(state.LastClaimedDayIndex == RewardAdTracker.DayIndex(day2, Kst), "받은 날짜가 2일차로 갱신된다");
        });

        Test("SubscriptionDailyGrant.RefinedMineralsPerClaim: 0보다 크다(플레이스홀더값 방어)", () =>
        {
            Assert(SubscriptionDailyGrant.RefinedMineralsPerClaim > 0f,
                $"실제 {SubscriptionDailyGrant.RefinedMineralsPerClaim} — 0 이하면 '지급'이 아무 효과도 없다");
        });

        Test("SaveData: SubscriptionGrantState 왕복", () =>
        {
            var save = new SaveData();
            var state = new SubscriptionGrantState { LastClaimedDayIndex = 20345 };
            save.ApplySubscriptionGrantState(state);
            var back = save.ToSubscriptionGrantState();
            Assert(back.LastClaimedDayIndex == 20345, "LastClaimedDayIndex 왕복");
        });

        Test("SaveData: 새 세이브의 SubscriptionGrantLastClaimedDayIndex는 0(받은 적 없음)", () =>
        {
            var save = new SaveData();
            Assert(save.ToSubscriptionGrantState().LastClaimedDayIndex == 0, "마이그레이션 없이도 안전한 기본값");
        });

        // P-12: 펫 등급(PetGrade.cs)·펫 뽑기 확률표(PetGachaTable.cs). docs/design/pet-gacha.md 2·3절.
        Test("PetGrade: 등급별 종 수 합이 124다(2절 \"합계 124종\", T-12 해결 후 값)", () =>
        {
            Assert(PetGradeInfo.SpeciesCount.Sum() == PetGradeInfo.TotalSpeciesCount,
                $"실제 합 {PetGradeInfo.SpeciesCount.Sum()}");
        });

        Test("PetGrade: 도감 보너스가 등급이 오를수록 커진다(단조 증가)", () =>
        {
            for (var i = 1; i < PetGradeInfo.CollectionBonusPerSpecies.Length; i++)
            {
                Assert(PetGradeInfo.CollectionBonusPerSpecies[i] > PetGradeInfo.CollectionBonusPerSpecies[i - 1],
                    $"등급 {i}이 {i - 1}보다 커야 한다");
            }
        });

        Test("PetGrade: NameKoFor·SpeciesCountFor·CollectionBonusFor가 배열 인덱스와 일치한다", () =>
        {
            Assert(PetGradeInfo.NameKoFor(PetGrade.Transcendent) == "초월", "초월 이름");
            Assert(PetGradeInfo.SpeciesCountFor(PetGrade.Mythic) == 30, "신화 종 수 30");
            AssertNear(0.01f, PetGradeInfo.CollectionBonusFor(PetGrade.Common), "일반 도감 보너스");
        });

        Test("펫 뽑기: 4종 확률표 모두 가중치 합이 정확히 1.0이다", () =>
        {
            AssertNear(1f, PetGachaTable.Free().Sum(w => w.Weight), "무료 뽑기 가중치 합");
            AssertNear(1f, PetGachaTable.Normal().Sum(w => w.Weight), "일반 뽑기 가중치 합");
            AssertNear(1f, PetGachaTable.Advanced().Sum(w => w.Weight), "고급 뽑기 가중치 합");
            AssertNear(1f, PetGachaTable.Special().Sum(w => w.Weight), "특수 뽑기 가중치 합");
        });

        Test("펫 뽑기: 표시용으로 내보내는 값이 표 값과 같다(법적 표시 의무, 8절)", () =>
        {
            // 확률 공개 화면(P-15, 아직 없음)이 이 표를 그대로 읽어서 그릴 것이므로, 표 자체가
            // 진실이면 화면도 자동으로 맞는다 — 여기서는 표가 문서(3절)와 일치하는지만 확인한다.
            var advanced = PetGachaTable.Advanced();
            AssertNear(0.55f, advanced.First(w => w.Grade == PetGrade.Rare).Weight, "고급 뽑기 희귀 55%");
            AssertNear(0.03f, advanced.First(w => w.Grade == PetGrade.Mythic).Weight, "고급 뽑기 신화 3%");
            var special = PetGachaTable.Special();
            AssertNear(0.005f, special.First(w => w.Grade == PetGrade.Transcendent).Weight, "특수 뽑기 초월 0.5%");
        });

        Test("펫 뽑기: 같은 seed는 항상 같은 등급을 준다(서버 재검증용 재현성)", () =>
        {
            var a = PetGachaTable.Open(PetGachaTable.Advanced(), 321);
            var b = PetGachaTable.Open(PetGachaTable.Advanced(), 321);
            Assert(a.Grade == b.Grade && a.Guaranteed == b.Guaranteed, $"seed 321 반복 시 항상 {a.Grade}");
        });

        Test("펫 뽑기: 10만 회 열어 보면 실제 등급 분포가 확률표와 1%p 안쪽으로 근접한다", () =>
        {
            const int trials = 100_000;
            var weights = PetGachaTable.Special(); // 0.5%짜리(초월)가 있어 가장 오차가 드러나기 쉬운 표
            var counts = new Dictionary<PetGrade, int>();
            for (var i = 0; i < trials; i++)
            {
                var seed = unchecked((int)((long)i * 2654435761L + 40503L));
                var r = PetGachaTable.Open(weights, seed);
                counts[r.Grade] = counts.TryGetValue(r.Grade, out var c) ? c + 1 : 1;
            }
            foreach (var w in weights)
            {
                var actual = counts.TryGetValue(w.Grade, out var c) ? (float)c / trials : 0f;
                Assert(Math.Abs(actual - w.Weight) < 0.01f, $"{w.Grade} 실제 {actual:P1} vs 기대 {w.Weight:P0}");
            }
        });

        Test("펫 뽑기: 고급 뽑기 천장 80뽑째는 확률과 무관하게 신화 확정", () =>
        {
            var weights = PetGachaTable.Advanced();
            for (var opened = 0; opened < PetGachaTable.AdvancedPityCount - 1; opened++)
            {
                var r = PetGachaTable.Open(weights, seed: 1, openedSincePity: opened,
                    pityCount: PetGachaTable.AdvancedPityCount, pityGrade: PetGachaTable.AdvancedPityGrade);
                Assert(!r.Guaranteed, $"{opened + 1}번째는 아직 확정 아님");
            }
            var last = PetGachaTable.Open(weights, seed: 1, openedSincePity: PetGachaTable.AdvancedPityCount - 1,
                pityCount: PetGachaTable.AdvancedPityCount, pityGrade: PetGachaTable.AdvancedPityGrade);
            Assert(last.Guaranteed && last.Grade == PetGachaTable.AdvancedPityGrade,
                $"{PetGachaTable.AdvancedPityCount}번째는 {PetGachaTable.AdvancedPityGrade} 확정, 실제 {last.Grade}");
        });

        Test("펫 뽑기: 특수 뽑기 천장 80뽑째는 확률과 무관하게 초월 확정", () =>
        {
            var r = PetGachaTable.Open(PetGachaTable.Special(), seed: 1,
                openedSincePity: PetGachaTable.SpecialPityCount - 1,
                pityCount: PetGachaTable.SpecialPityCount, pityGrade: PetGachaTable.SpecialPityGrade);
            Assert(r.Guaranteed && r.Grade == PetGrade.Transcendent, $"실제 {r.Grade} (Guaranteed={r.Guaranteed})");
        });

        Test("펫 뽑기: 뽑기별 천장 카운터가 서로 안 섞인다 — 무료 뽑기는 아무리 열어도 확정이 없다", () =>
        {
            // 무료 뽑기는 pityCount=0으로 호출한다(카운터 자체가 없는 뽑기) — 고급 뽑기의
            // AdvancedPityCount를 실수로 넘겨도 무료 뽑기 호출부가 pityCount=0을 쓰는 한 안 섞인다.
            var r = PetGachaTable.Open(PetGachaTable.Free(), seed: 1, openedSincePity: 999_999, pityCount: 0);
            Assert(!r.Guaranteed, "무료 뽑기는 천장이 아예 없다");
        });

        Test("펫 뽑기: 10연차는 5등급(전설) 이상이 하나도 없으면 마지막에 하나를 확정으로 채운다", () =>
        {
            // seed 0~9는 전부 낮은 등급만 나오도록(고급 뽑기 표 기준) 실제로 확인 — 만약 우연히
            // 전설 이상이 섞여 있다면 이 테스트 자체가 그 경우를 걸러내고 통과한다(아래 Any 분기).
            var results = PetGachaTable.OpenTen(PetGachaTable.Advanced(), baseSeed: 5000,
                minGrade: PetGachaTable.AdvancedTenPullMinGrade);
            Assert(results.Length == 10, "10연차는 결과 10개");
            Assert(results.Any(r => r.Grade >= PetGachaTable.AdvancedTenPullMinGrade),
                "10개 중 최소 1개는 전설 이상(자연 당첨 또는 보장 채움)");
        });

        Test("펫 뽑기: 10연차 보장은 이미 전설 이상이 있으면 다른 결과를 건드리지 않는다", () =>
        {
            // 10연차 전부가 특수 뽑기 표(전설 80%)라면 자연히 전설 이상이 여럿 나온다 —
            // 그 경우 Guaranteed 채움이 발생하지 않아야 한다(불필요하게 덮어쓰지 않음).
            var results = PetGachaTable.OpenTen(PetGachaTable.Special(), baseSeed: 1,
                minGrade: PetGrade.Legendary);
            var guaranteedCount = results.Count(r => r.Guaranteed);
            Assert(guaranteedCount == 0, $"특수 뽑기 표는 전설 확률이 80%라 자연 당첨될 것, 확정 채움 {guaranteedCount}건");
        });

        Test("펫 뽑기: 빈 확률표나 가중치 합 0은 예외를 던진다(방어적 실패)", () =>
        {
            var threwEmpty = false;
            try { PetGachaTable.Open(new List<PetGachaWeight>(), seed: 1); }
            catch (ArgumentException) { threwEmpty = true; }
            Assert(threwEmpty, "빈 확률표는 예외");
        });

        // P-13: 펫 중복 조각 환산(PetFusion.cs). docs/design/pet-gacha.md 2절 "중복 → 상위 등급"표.
        Test("펫 조각: 같은 등급 조각 3개마다 다른 펫 1마리, 나머지는 버려지지 않는다", () =>
        {
            var (petsGained, remaining) = PetFusion.ExchangeForSameGrade(10);
            Assert(petsGained == 3, $"10개 // 3 = 3, 실제 {petsGained}");
            Assert(remaining == 1, $"10개 % 3 = 1, 실제 {remaining}");
        });

        Test("펫 조각: 3개 미만이면 0마리, 조각은 전부 남는다", () =>
        {
            var (petsGained, remaining) = PetFusion.ExchangeForSameGrade(2);
            Assert(petsGained == 0, "3개 미만이면 못 바꾼다");
            Assert(remaining == 2, "안 쓴 조각은 그대로 남는다");
        });

        Test("펫 조각: 승급 비용이 2절 표와 같다 — 1~4등급 5, 5등급(전설) 8, 6등급(신화) 15", () =>
        {
            Assert(PetFusion.PromotionCost(PetGrade.Common) == 5, "일반→고급 5");
            Assert(PetFusion.PromotionCost(PetGrade.Advanced) == 5, "고급→희귀 5");
            Assert(PetFusion.PromotionCost(PetGrade.Rare) == 5, "희귀→영웅 5");
            Assert(PetFusion.PromotionCost(PetGrade.Epic) == 5, "영웅→전설 5");
            Assert(PetFusion.PromotionCost(PetGrade.Legendary) == 8, "전설→신화 8");
            Assert(PetFusion.PromotionCost(PetGrade.Mythic) == 15, "신화→초월 15");
        });

        Test("펫 조각: 초월(7등급)은 더 위 등급이 없어 승급을 물으면 예외를 던진다", () =>
        {
            var threw = false;
            try { PetFusion.PromotionCost(PetGrade.Transcendent); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "초월 승급 비용을 물으면 예외");

            threw = false;
            try { PetFusion.ExchangeForPromotion(PetGrade.Transcendent, 100); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "초월 승급 환산을 물어도 예외");
        });

        Test("펫 조각: 승급 결과 등급이 한 단계 위다(신화 조각 15개 → 초월 조각 1개)", () =>
        {
            Assert(PetFusion.PromotedGrade(PetGrade.Mythic) == PetGrade.Transcendent, "신화 다음은 초월");
            Assert(PetFusion.PromotedGrade(PetGrade.Common) == PetGrade.Advanced, "일반 다음은 고급");

            var (promotions, remaining) = PetFusion.ExchangeForPromotion(PetGrade.Mythic, 31);
            Assert(promotions == 2, $"31 // 15 = 2, 실제 {promotions}");
            Assert(remaining == 1, $"31 % 15 = 1, 실제 {remaining}");
        });

        Test("펫 조각: 음수 조각 수는 예외를 던진다(방어적 실패)", () =>
        {
            var threw = false;
            try { PetFusion.ExchangeForSameGrade(-1); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "음수 조각(같은 등급 교환)은 예외");

            threw = false;
            try { PetFusion.ExchangeForPromotion(PetGrade.Common, -1); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "음수 조각(승급)은 예외");
        });

        // P-14: 도감·장착 보너스(PetCollection.cs). docs/design/pet-gacha.md 2·8절.
        // 6·7등급(신화·초월) 고유 효과는 종 48개가 아직 안 정해져 대상 밖이다.
        Test("펫 도감: 아무것도 없으면 보너스 0", () =>
        {
            var bonus = PetCollection.CollectionBonus(new int[7]);
            Assert(bonus == 0f, $"보유 0종이면 0, 실제 {bonus}");
        });

        Test("펫 도감: 등급별 보너스 × 보유 종 수를 그대로 합산한다", () =>
        {
            // 일반 3종(+1% x3) + 전설 1종(+12%) = 0.15
            var counts = new int[7];
            counts[(int)PetGrade.Common] = 3;
            counts[(int)PetGrade.Legendary] = 1;
            var bonus = PetCollection.CollectionBonus(counts);
            Assert(Math.Abs(bonus - (0.03f + 0.12f)) < 1e-4f, $"일반 3종+전설 1종 합산, 실제 {bonus}");
        });

        Test("펫 도감: 모든 등급을 종 수 최대치까지 채우면 마리당 보너스 × 종 수의 총합이다", () =>
        {
            var counts = PetGradeInfo.SpeciesCount; // 등급별 최대 종 수(16,16,16,16,20,30,10)
            var bonus = PetCollection.CollectionBonus(counts);
            var expected = 0f;
            for (var i = 0; i < counts.Length; i++)
                expected += PetGradeInfo.CollectionBonusFor((PetGrade)i) * counts[i];
            Assert(Math.Abs(bonus - expected) < 1e-4f, $"만재 도감, 실제 {bonus} / 기대 {expected}");
        });

        Test("펫 도감: null·길이 불일치·음수·최대치 초과는 예외를 던진다(방어적 실패)", () =>
        {
            var threw = false;
            try { PetCollection.CollectionBonus(null); }
            catch (ArgumentNullException) { threw = true; }
            Assert(threw, "null은 예외");

            threw = false;
            try { PetCollection.CollectionBonus(new int[6]); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "등급 수(7)와 길이가 다르면 예외");

            threw = false;
            try { PetCollection.CollectionBonus(new[] { -1, 0, 0, 0, 0, 0, 0 }); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "음수 보유 종 수는 예외");

            threw = false;
            try { PetCollection.CollectionBonus(new[] { 17, 0, 0, 0, 0, 0, 0 }); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "일반은 종이 16개뿐인데 17개는 예외");
        });

        Test("펫 장착: 일반~전설(1~5등급) 보너스가 2절 표와 같다", () =>
        {
            Assert(Math.Abs(PetCollection.EquipBonus(PetGrade.Common) - 0.08f) < 1e-4f, "일반 +8%");
            Assert(Math.Abs(PetCollection.EquipBonus(PetGrade.Advanced) - 0.14f) < 1e-4f, "고급 +14%");
            Assert(Math.Abs(PetCollection.EquipBonus(PetGrade.Rare) - 0.22f) < 1e-4f, "희귀 +22%");
            Assert(Math.Abs(PetCollection.EquipBonus(PetGrade.Epic) - 0.35f) < 1e-4f, "영웅 +35%");
            Assert(Math.Abs(PetCollection.EquipBonus(PetGrade.Legendary) - 0.55f) < 1e-4f, "전설 +55%");
        });

        Test("펫 장착: 신화·초월은 고유 효과 미정이라 예외를 던진다", () =>
        {
            var threw = false;
            try { PetCollection.EquipBonus(PetGrade.Mythic); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "신화는 종별 고유 효과라 수치를 못 줌");

            threw = false;
            try { PetCollection.EquipBonus(PetGrade.Transcendent); }
            catch (ArgumentException) { threw = true; }
            Assert(threw, "초월도 마찬가지");
        });

        Test("코스 생성: 같은 (planetId, tier, index, seed)는 항상 같은 코스를 만든다(서버 재검증용 재현성)", () =>
        {
            var a = CourseGenerator.Generate("quartz", RaceTier.Circuit, 3, 777);
            var b = CourseGenerator.Generate("quartz", RaceTier.Circuit, 3, 777);
            AssertCourseEquals(a, b);
        });

        Test("코스 생성: 세그먼트 비율(평지/험지/부스트) 합이 항상 정확히 1이고 음수가 없다(네 등급 전부)", () =>
        {
            foreach (var tier in new[] { RaceTier.Local, RaceTier.Circuit, RaceTier.Challenge, RaceTier.GrandPrix })
            {
                for (var seed = 0; seed < 200; seed++)
                {
                    var c = CourseGenerator.Generate("quartz", tier, 1, seed);
                    AssertNear(1f, c.FlatRatio + c.RoughRatio + c.BoostRatio, $"{tier} seed={seed} 비율 합");
                    Assert(c.FlatRatio >= 0f && c.RoughRatio >= 0f && c.BoostRatio >= 0f, $"{tier} seed={seed} 비율은 음수가 될 수 없다");
                }
            }
        });

        Test("코스 생성: 등급이 오를수록 평균 평지 비율이 줄고 부스트 비율이 늘어난다(같은 시드 집합 기준)", () =>
        {
            float AvgFlat(RaceTier tier)
            {
                float sum = 0f;
                for (var seed = 0; seed < 500; seed++) sum += CourseGenerator.Generate("quartz", tier, 1, seed).FlatRatio;
                return sum / 500f;
            }
            float AvgBoost(RaceTier tier)
            {
                float sum = 0f;
                for (var seed = 0; seed < 500; seed++) sum += CourseGenerator.Generate("quartz", tier, 1, seed).BoostRatio;
                return sum / 500f;
            }
            var localFlat = AvgFlat(RaceTier.Local);
            var circuitFlat = AvgFlat(RaceTier.Circuit);
            var challengeFlat = AvgFlat(RaceTier.Challenge);
            var grandPrixFlat = AvgFlat(RaceTier.GrandPrix);
            Assert(localFlat > circuitFlat, $"로컬 평지 평균({localFlat})이 서킷({circuitFlat})보다 높아야 한다");
            Assert(circuitFlat > challengeFlat, $"서킷 평지 평균({circuitFlat})이 챌린지({challengeFlat})보다 높아야 한다");
            Assert(challengeFlat > grandPrixFlat, $"챌린지 평지 평균({challengeFlat})이 그랑프리({grandPrixFlat})보다 높아야 한다");
            Assert(AvgBoost(RaceTier.GrandPrix) > AvgBoost(RaceTier.Local), "그랑프리 부스트 평균이 로컬보다 높아야 한다");
        });

        Test("코스 생성: 평지 비율이 등급 보정치보다 작아도 음수로 내려가지 않는다(클램프 경계)", () =>
        {
            for (var seed = 0; seed < 500; seed++)
            {
                var c = CourseGenerator.Generate("quartz", RaceTier.GrandPrix, 1, seed);
                Assert(c.FlatRatio >= 0f, $"seed={seed} 그랑프리 평지가 음수({c.FlatRatio})");
                Assert(c.BoostRatio <= 1f, $"seed={seed} 그랑프리 부스트가 1 초과({c.BoostRatio})");
            }
        });

        Test("코스 생성: 등급별 길이·랩이 정해 둔 범위 밖으로 나오지 않는다", () =>
        {
            var ranges = new (RaceTier tier, float minLen, float maxLen, int minLaps, int maxLaps)[]
            {
                (RaceTier.Local, 400f, 900f, 1, 2),
                (RaceTier.Circuit, 800f, 1500f, 1, 3),
                (RaceTier.Challenge, 1200f, 2000f, 2, 3),
                (RaceTier.GrandPrix, 1800f, 3000f, 3, 4),
            };
            foreach (var r in ranges)
            {
                for (var seed = 0; seed < 100; seed++)
                {
                    var c = CourseGenerator.Generate("ruby", r.tier, 1, seed * 31 + 5);
                    Assert(c.Length >= r.minLen && c.Length <= r.maxLen, $"{r.tier} seed={seed} 길이 {c.Length}가 범위 밖");
                    Assert(c.Laps >= r.minLaps && c.Laps <= r.maxLaps, $"{r.tier} seed={seed} 랩 {c.Laps}가 범위 밖");
                }
            }
        });

        Test("코스 생성: GenerateMany는 요청한 개수만큼 만들고 Id가 서로 겹치지 않는다", () =>
        {
            var list = CourseGenerator.GenerateMany("sapphire", RaceTier.Local, 12, 500);
            Assert(list.Count == 12, "요청한 개수만큼 나와야 한다");
            var ids = new HashSet<string>();
            foreach (var c in list)
            {
                Assert(c.PlanetId == "sapphire", "PlanetId가 그대로 전달돼야 한다");
                Assert(c.Tier == RaceTier.Local, "Tier가 그대로 전달돼야 한다");
                Assert(ids.Add(c.Id), $"Id 중복: {c.Id}");
            }
        });

        Test("코스 생성: GenerateMany(count=0)은 예외 없이 빈 목록을 낸다(경계값)", () =>
        {
            var list = CourseGenerator.GenerateMany("quartz", RaceTier.Local, 0, 1);
            Assert(list.Count == 0, $"count=0이면 빈 목록이어야 한다 (실제 {list.Count})");
        });

        Test("코스 생성: 음수 시드로도 예외 없이 유효한 코스가 나온다(경계값)", () =>
        {
            var c = CourseGenerator.Generate("quartz", RaceTier.Challenge, 1, -777);
            AssertNear(1f, c.FlatRatio + c.RoughRatio + c.BoostRatio, "음수 시드에서도 비율 합은 1");
            Assert(c.FlatRatio >= 0f && c.RoughRatio >= 0f && c.BoostRatio >= 0f, "음수 시드에서도 비율은 음수가 될 수 없다");
            Assert(c.Length >= 1200f && c.Length <= 2000f, $"음수 시드에서도 챌린지 길이 범위 안({c.Length})");

            var same1 = CourseGenerator.Generate("quartz", RaceTier.Challenge, 1, -777);
            AssertCourseEquals(c, same1);
        });

        Test("A-04 자기 최고 기록: 처음 완주는 기록이 없다가 바로 최고 기록이 된다", () =>
        {
            var ids = new List<string>();
            var times = new List<float>();
            var r = RaceRecordBook.Update(ids, times, "ruby_local_01", 42.5f);
            Assert(!r.HasPreviousRecord, "처음이니 이전 기록 없음");
            Assert(r.IsNewRecord, "처음 완주는 항상 신기록");
            AssertNear(0f, r.DeltaSeconds, "이전 기록이 없으면 차이는 0");
            Assert(ids.Count == 1 && times.Count == 1, "리스트에 한 줄 추가돼야 한다");
            AssertNear(42.5f, times[0], "저장된 기록");
        });

        Test("A-04 자기 최고 기록: 더 빠르면 갱신되고 델타가 음수", () =>
        {
            var ids = new List<string> { "ruby_local_01" };
            var times = new List<float> { 42.5f };
            var r = RaceRecordBook.Update(ids, times, "ruby_local_01", 40f);
            Assert(r.HasPreviousRecord, "이전 기록 있음");
            Assert(r.IsNewRecord, "더 빨랐으니 신기록");
            AssertNear(42.5f, r.PreviousBestSeconds, "이전 최고 기록");
            AssertNear(-2.5f, r.DeltaSeconds, "2.5초 단축 → 델타 -2.5");
            AssertNear(40f, times[0], "리스트가 새 기록으로 갱신돼야 한다");
        });

        Test("A-04 자기 최고 기록: 더 느리면 갱신되지 않고 델타가 양수", () =>
        {
            var ids = new List<string> { "ruby_local_01" };
            var times = new List<float> { 40f };
            var r = RaceRecordBook.Update(ids, times, "ruby_local_01", 41f);
            Assert(!r.IsNewRecord, "더 느렸으니 신기록 아님");
            AssertNear(1f, r.DeltaSeconds, "1초 느려짐 → 델타 +1");
            AssertNear(40f, times[0], "리스트는 그대로여야 한다(더 느린 기록으로 덮어쓰면 안 됨)");
        });

        Test("A-04 자기 최고 기록: 동률은 신기록이 아니다(< 비교, <= 아님)", () =>
        {
            var ids = new List<string> { "c1" };
            var times = new List<float> { 30f };
            var r = RaceRecordBook.Update(ids, times, "c1", 30f);
            Assert(!r.IsNewRecord, "정확히 같은 시간은 갱신하지 않는다");
            AssertNear(0f, r.DeltaSeconds, "델타 0");
        });

        Test("A-04 자기 최고 기록: 코스가 여러 개면 서로 안 섞인다", () =>
        {
            var ids = new List<string>();
            var times = new List<float>();
            RaceRecordBook.Update(ids, times, "quartz_local_01", 50f);
            RaceRecordBook.Update(ids, times, "ruby_local_01", 45f);
            var r = RaceRecordBook.Update(ids, times, "quartz_local_01", 48f);
            Assert(r.HasPreviousRecord, "quartz 코스는 기존 기록이 있어야 한다");
            AssertNear(50f, r.PreviousBestSeconds, "quartz 기존 기록");
            AssertNear(45f, RaceRecordBook.BestOf(ids, times, "ruby_local_01") ?? -1f, "ruby 기록은 그대로");
        });

        Test("A-04 자기 최고 기록: BestOf는 없는 코스에 null을 돌려준다", () =>
        {
            var ids = new List<string> { "c1" };
            var times = new List<float> { 30f };
            Assert(RaceRecordBook.BestOf(ids, times, "없는코스") == null, "없는 코스는 null");
        });

        Test("A-04 자기 최고 기록: 경계값(0초 이하·빈 courseId·리스트 길이 불일치)은 예외", () =>
        {
            var ids = new List<string>();
            var times = new List<float>();
            var threwZero = false;
            try { RaceRecordBook.Update(ids, times, "c1", 0f); } catch (ArgumentException) { threwZero = true; }
            Assert(threwZero, "0초는 예외");

            var threwNegative = false;
            try { RaceRecordBook.Update(ids, times, "c1", -1f); } catch (ArgumentException) { threwNegative = true; }
            Assert(threwNegative, "음수 초는 예외");

            var threwEmptyId = false;
            try { RaceRecordBook.Update(ids, times, "", 10f); } catch (ArgumentException) { threwEmptyId = true; }
            Assert(threwEmptyId, "빈 courseId는 예외");

            var threwMismatch = false;
            try { RaceRecordBook.Update(new List<string> { "c1" }, times, "c1", 10f); } catch (ArgumentException) { threwMismatch = true; }
            Assert(threwMismatch, "리스트 길이 불일치는 예외");
        });

        Test("P-03 증폭기: 등급별 증폭률 범위가 amplifier.md 표와 같다(일반 1~5% / 고급 10~25% / 에픽 60~120% / 전설 300~900%)", () =>
        {
            AssertNear(0.01f, Amplifier.MinBonusFor(PartGrade.C), "일반 최소");
            AssertNear(0.05f, Amplifier.MaxBonusFor(PartGrade.C), "일반 최대");
            AssertNear(0.10f, Amplifier.MinBonusFor(PartGrade.B), "고급 최소");
            AssertNear(0.25f, Amplifier.MaxBonusFor(PartGrade.B), "고급 최대");
            AssertNear(0.60f, Amplifier.MinBonusFor(PartGrade.A), "에픽 최소");
            AssertNear(1.20f, Amplifier.MaxBonusFor(PartGrade.A), "에픽 최대");
            AssertNear(3.00f, Amplifier.MinBonusFor(PartGrade.S), "전설 최소");
            AssertNear(9.00f, Amplifier.MaxBonusFor(PartGrade.S), "전설 최대");
        });

        Test("P-03 증폭기: Roll은 같은 seed면 같은 값(서버 재검증 재현성, CLAUDE.md 1번)", () =>
        {
            var a = Amplifier.Roll(PartGrade.A, 12345);
            var b = Amplifier.Roll(PartGrade.A, 12345);
            AssertNear(a, b, "같은 seed는 같은 결과");
        });

        Test("P-03 증폭기: Roll 결과는 항상 그 등급 구간 [min, max) 안이다(등급별 1000회 표본)", () =>
        {
            foreach (PartGrade grade in Enum.GetValues(typeof(PartGrade)))
            {
                var min = Amplifier.MinBonusFor(grade);
                var max = Amplifier.MaxBonusFor(grade);
                for (var seed = 1; seed <= 1000; seed++)
                {
                    var v = Amplifier.Roll(grade, seed * 7919 + (int)grade);
                    Assert(v >= min && v < max, $"{grade} seed{seed} 결과 {v} ∈ [{min}, {max})");
                }
            }
        });

        Test("P-03 증폭기: 등급이 높을수록 구간이 겹치지 않고 더 세다", () =>
        {
            Assert(Amplifier.MaxBonusFor(PartGrade.C) <= Amplifier.MinBonusFor(PartGrade.B), "일반 최대 <= 고급 최소");
            Assert(Amplifier.MaxBonusFor(PartGrade.B) <= Amplifier.MinBonusFor(PartGrade.A), "고급 최대 <= 에픽 최소");
            Assert(Amplifier.MaxBonusFor(PartGrade.A) <= Amplifier.MinBonusFor(PartGrade.S), "에픽 최대 <= 전설 최소");
        });

        Test("P-03 증폭기: Apply는 합(곱 아님)으로 쌓인 증폭률을 곱해 최종 성능을 낸다", () =>
        {
            AssertNear(100f, Amplifier.Apply(100f, 0f), "증폭기 없으면 그대로");
            AssertNear(150f, Amplifier.Apply(100f, 0.5f), "+50% 증폭기 하나");
            // 일반 두 개(+3%, +4%) + 전설 하나(+900%) 같은 칸에 쌓였을 때 — 합산이지 곱이 아니다.
            AssertNear(100f * (1f + 0.03f + 0.04f + 9.00f), Amplifier.Apply(100f, 0.03f + 0.04f + 9.00f), "합산 누적");
        });

        Test("P-03 증폭기: 경계값 — 기본 성능 0이면 결과도 0, 증폭률 0이면 그대로", () =>
        {
            AssertNear(0f, Amplifier.Apply(0f, 5f), "기본 0이면 증폭해도 0");
            AssertNear(100f, Amplifier.Apply(100f, 0f), "증폭률 0이면 변화 없음");
        });

        Test("P-04 상자 보상 종류: RollKind는 항상 정의된 세 값 중 하나이고 같은 seed면 같은 값(재현성)", () =>
        {
            for (var seed = 1; seed <= 2000; seed++)
            {
                var kind = LootReward.RollKind(seed);
                Assert(kind == LootRewardKind.RigPart || kind == LootRewardKind.Amplifier || kind == LootRewardKind.Minerals,
                    $"seed{seed} 결과 {kind}가 정의된 값 밖");
                Assert(LootReward.RollKind(seed) == kind, $"seed{seed} 재현성 깨짐");
            }
        });

        Test("P-04 상자 보상 종류: 2000회 표본 분포가 가중치(부품 55/증폭기 30/광물 15)와 5%p 안에서 맞는다", () =>
        {
            var counts = new Dictionary<LootRewardKind, int>
            {
                { LootRewardKind.RigPart, 0 }, { LootRewardKind.Amplifier, 0 }, { LootRewardKind.Minerals, 0 },
            };
            const int trials = 2000;
            for (var seed = 1; seed <= trials; seed++) counts[LootReward.RollKind(seed * 104729)]++;

            // 2000표본의 통계적 흔들림을 감안해 5%p 안이면 통과(AssertNear의 0.001은 너무 빡빡하다 —
            // Amplifier.Roll 1000표본 테스트처럼 범위 소속만 보는 게 아니라 실제 비율을 재는 자리라서).
            void AssertRatioNear(float expected, int count, string label)
            {
                var actual = count / (float)trials;
                Assert(Math.Abs(expected - actual) < 0.05f, $"{label} {actual} ≈ {expected} (±5%p)");
            }
            AssertRatioNear(0.55f, counts[LootRewardKind.RigPart], "부품 비율");
            AssertRatioNear(0.30f, counts[LootRewardKind.Amplifier], "증폭기 비율");
            AssertRatioNear(0.15f, counts[LootRewardKind.Minerals], "광물 비율");
        });

        Test("P-04 상자 보상 — 원석: 등급별 구간이 표(MineralMinFor/MaxFor)와 일치하고 등급이 높을수록 더 나온다", () =>
        {
            Assert(LootReward.MineralMaxFor(PartGrade.C) <= LootReward.MineralMinFor(PartGrade.B), "일반 최대 <= 고급 최소");
            Assert(LootReward.MineralMaxFor(PartGrade.B) <= LootReward.MineralMinFor(PartGrade.A), "고급 최대 <= 에픽 최소");
            Assert(LootReward.MineralMaxFor(PartGrade.A) <= LootReward.MineralMinFor(PartGrade.S), "에픽 최대 <= 전설 최소");

            foreach (PartGrade grade in Enum.GetValues(typeof(PartGrade)))
            {
                var min = LootReward.MineralMinFor(grade);
                var max = LootReward.MineralMaxFor(grade);
                for (var seed = 1; seed <= 500; seed++)
                {
                    var m = LootReward.MineralsFor(grade, seed * 7919 + (int)grade);
                    Assert(m.Amount >= min && m.Amount < max, $"{grade} seed{seed} 원석량 {m.Amount} ∈ [{min}, {max})");
                }
            }
        });

        Test("P-04 상자 보상 — 원석/증폭기 둘 다 같은 seed면 같은 값(서버 재검증 재현성)", () =>
        {
            var m1 = LootReward.MineralsFor(PartGrade.A, 321);
            var m2 = LootReward.MineralsFor(PartGrade.A, 321);
            AssertNear(m1.Amount, m2.Amount, "MineralsFor 재현성");

            var a1 = LootReward.AmplifierFor(PartGrade.S, 654);
            var a2 = LootReward.AmplifierFor(PartGrade.S, 654);
            AssertNear(a1.Bonus, a2.Bonus, "AmplifierFor 재현성");
            Assert(a1.Grade == PartGrade.S, "AmplifierFor는 넘긴 등급을 그대로 들고 있다");
        });

        Test("LootBoxOpener.Open(옛 3종류 시드 버전)은 P-04 이후에도 Kind가 항상 RigPart로 읽히고 Reward가 그대로 채워진다(회귀)", () =>
        {
            var r = LootBoxOpener.Open(LootBoxType.Steel, gradeSeed: 11, slotSeed: 22, openedSincePity: 0, courseId: "c1");
            Assert(r.Kind == LootRewardKind.RigPart, "Kind 기본값은 RigPart");
            Assert(r.Reward != null, "Reward는 그대로 채워짐");
            Assert(r.AmplifierBonus == null && r.Minerals == null, "새 필드는 비어 있음");
        });

        Test("LootBoxOpener.OpenAny: Kind에 따라 해당 필드만 채워지고 나머지 둘은 비어 있다", () =>
        {
            // kindSeed를 훑어 세 종류가 실제로 다 나오는지, 그때마다 필드가 정확히 Kind와만 맞는지 본다.
            // 연속된 작은 정수는 xorshift 첫 스텝에서 잘 안 섞인다(DeterministicRandom 주석 참고) —
            // LootTable.Open 테스트 등 기존 테스트들도 시드에 소수를 곱해 흩어 놓는다.
            var sawRigPart = false; var sawAmplifier = false; var sawMinerals = false;
            for (var kindSeed = 1; kindSeed <= 200; kindSeed++)
            {
                var r = LootBoxOpener.OpenAny(LootBoxType.Steel, gradeSeed: 1, kindSeed: kindSeed * 104729,
                    slotSeed: 2, mineralSeed: 3, openedSincePity: 0);
                switch (r.Kind)
                {
                    case LootRewardKind.RigPart:
                        sawRigPart = true;
                        Assert(r.Reward != null, "RigPart면 Reward가 채워진다");
                        Assert(r.AmplifierBonus == null && r.Minerals == null, "RigPart면 나머지는 비어 있다");
                        break;
                    case LootRewardKind.Amplifier:
                        sawAmplifier = true;
                        Assert(r.AmplifierBonus != null, "Amplifier면 AmplifierBonus가 채워진다");
                        Assert(r.Reward == null && r.Minerals == null, "Amplifier면 나머지는 비어 있다");
                        break;
                    case LootRewardKind.Minerals:
                        sawMinerals = true;
                        Assert(r.Minerals != null, "Minerals면 Minerals가 채워진다");
                        Assert(r.Reward == null && r.AmplifierBonus == null, "Minerals면 나머지는 비어 있다");
                        break;
                }
            }
            Assert(sawRigPart && sawAmplifier && sawMinerals, "200회 안에 세 종류가 다 나왔다(분포 확인 겸 스모크 테스트)");
        });

        Test("LootBoxOpener.OpenAny: 천장(Guaranteed) 등급도 종류를 부품으로 고정하지 않는다 — 증폭기/원석이면 그 등급 값을 그대로 쓴다", () =>
        {
            // kindSeed를 훑어 Amplifier/Minerals가 나오는 kindSeed를 찾은 뒤, 그 kindSeed로 천장 개봉을 시켜 본다.
            int? amplifierKindSeed = null; int? mineralsKindSeed = null;
            for (var kindSeed = 1; kindSeed <= 200 && (amplifierKindSeed == null || mineralsKindSeed == null); kindSeed++)
            {
                var scattered = kindSeed * 104729;
                var kind = LootReward.RollKind(scattered);
                if (kind == LootRewardKind.Amplifier && amplifierKindSeed == null) amplifierKindSeed = scattered;
                if (kind == LootRewardKind.Minerals && mineralsKindSeed == null) mineralsKindSeed = scattered;
            }
            Assert(amplifierKindSeed != null && mineralsKindSeed != null, "테스트 준비: 200 이내에 둘 다 찾았어야 함");

            var atPityAmp = LootBoxOpener.OpenAny(LootBoxType.Titanium, gradeSeed: 1, kindSeed: amplifierKindSeed!.Value,
                slotSeed: 2, mineralSeed: 3, openedSincePity: LootTable.TitaniumPityCount - 1);
            Assert(atPityAmp.Loot.Guaranteed && atPityAmp.Loot.Grade == LootTable.TitaniumPityGrade, "천장 등급 확정");
            Assert(atPityAmp.AmplifierBonus != null && atPityAmp.AmplifierBonus.Value.Grade == LootTable.TitaniumPityGrade,
                "확정 등급의 증폭기가 그대로 나온다");

            var atPityMin = LootBoxOpener.OpenAny(LootBoxType.Titanium, gradeSeed: 1, kindSeed: mineralsKindSeed!.Value,
                slotSeed: 2, mineralSeed: 3, openedSincePity: LootTable.TitaniumPityCount - 1);
            Assert(atPityMin.Minerals != null, "확정 등급이어도 광물 종류면 광물이 나온다");
            var expectedMin = LootReward.MineralMinFor(LootTable.TitaniumPityGrade);
            var expectedMax = LootReward.MineralMaxFor(LootTable.TitaniumPityGrade);
            Assert(atPityMin.Minerals!.Value.Amount >= expectedMin && atPityMin.Minerals!.Value.Amount < expectedMax,
                "그 확정 등급 구간 안의 원석량");
        });

        Test("P-05 RigAmplifierSave: 칸별로 따로 쌓이고, 0 이하 값은 무시한다", () =>
        {
            var save = new RigAmplifierSave();
            save.Add(RigSlot.Tool, 0.05f);
            save.Add(RigSlot.Tool, 0.10f);
            save.Add(RigSlot.Engine, 0.20f);
            save.Add(RigSlot.Cargo, 0f);      // 무시돼야 함
            save.Add(RigSlot.Refinery, -1f);  // 무시돼야 함
            AssertNear(0.15f, save.Bonus(RigSlot.Tool), "Tool 누적");
            AssertNear(0.20f, save.Bonus(RigSlot.Engine), "Engine 누적");
            AssertNear(0f, save.Bonus(RigSlot.Cargo), "Cargo는 0 그대로");
            AssertNear(0f, save.Bonus(RigSlot.Refinery), "Refinery는 음수 무시");
            AssertNear(0f, save.Bonus(RigSlot.Detector), "안 건드린 칸은 0");
        });

        Test("P-05 SaveData.AddAmplifier가 RigAmplifierSave.Add로 그대로 이어진다", () =>
        {
            var save = new SaveData();
            save.AddAmplifier(RigSlot.Cargo, 0.30f);
            AssertNear(0.30f, save.Amplifiers.Bonus(RigSlot.Cargo), "세이브에 반영");
        });

        Test("P-05 amp=null이면 MiningSimulator 값이 기존(비amp) 오버로드와 완전히 같다 — 회귀 없음", () =>
        {
            var rig = new MiningRig { ToolLevel = 12, CargoLevel = 8, EngineLevel = 6, DetectorLevel = 3, RefineryLevel = 3 };
            AssertNear(MiningSimulator.RigSpeed(rig, quartz), MiningSimulator.RigSpeed(rig, quartz, null), "RigSpeed");
            AssertNear(MiningSimulator.YieldPerVein(rig, quartz), MiningSimulator.YieldPerVein(rig, quartz, null), "YieldPerVein");
            AssertNear(MiningSimulator.MineralsPerHour(rig, quartz), MiningSimulator.MineralsPerHour(rig, quartz, null), "MineralsPerHour");
            AssertNear(MiningSimulator.CargoHours(rig, quartz), MiningSimulator.CargoHours(rig, quartz, null), "CargoHours");
            AssertNear(MiningSimulator.CargoCapacityMinerals(rig, quartz), MiningSimulator.CargoCapacityMinerals(rig, quartz, null), "CargoCapacityMinerals");
            AssertNear(MiningSimulator.GemsPerHour(rig, quartz), MiningSimulator.GemsPerHour(rig, quartz, null), "GemsPerHour");
            AssertNear(MiningSimulator.RefinePerHour(rig, quartz, false), MiningSimulator.RefinePerHour(rig, quartz, false, null), "RefinePerHour");
            var a = MiningSimulator.Offline(rig, quartz, 3600);
            var b = MiningSimulator.Offline(rig, quartz, 3600, false, null);
            AssertNear(a.Minerals, b.Minerals, "Offline.Minerals");
            AssertNear(a.RefinedGained, b.RefinedGained, "Offline.RefinedGained");
        });

        Test("P-05 Tool 증폭기가 YieldPerVein에 그대로 반영되고, 광맥 매장량은 못 넘는다", () =>
        {
            var rig = new MiningRig { ToolLevel = 1 };
            var amp = new RigAmplifierSave();
            amp.Add(RigSlot.Tool, 0.5f); // +50%
            var boosted = MiningSimulator.YieldPerVein(rig, quartz, amp);
            AssertNear(2f * 1.5f, boosted, "레벨1 기본 2 × 1.5");

            var hugeAmp = new RigAmplifierSave();
            hugeAmp.Add(RigSlot.Tool, 100f); // +10000% — quartz.VeinYield(20)를 훨씬 넘는다
            var clamped = MiningSimulator.YieldPerVein(rig, quartz, hugeAmp);
            AssertNear(quartz.VeinYield, clamped, "광맥 매장량 상한에 걸림");
        });

        Test("P-05 Engine 증폭기가 RigSpeed·MineralsPerHour에 반영된다(칸이 중복 곱해지지 않는지)", () =>
        {
            var rig = new MiningRig { ToolLevel = 4, EngineLevel = 4 };
            var amp = new RigAmplifierSave();
            amp.Add(RigSlot.Engine, 1.0f); // 속도 2배

            var speedBase = MiningSimulator.RigSpeed(rig, quartz);
            var speedBoosted = MiningSimulator.RigSpeed(rig, quartz, amp);
            AssertNear(speedBase * 2f, speedBoosted, "속도 정확히 2배");

            // MineralsPerHour는 RigSpeed(amp)를 통해서만 증폭돼야 한다 — YieldPerVein은 Engine 칸과 무관.
            var yield = MiningSimulator.YieldPerVein(rig, quartz);
            var travelPerVein = quartz.Circumference / quartz.VeinCount;
            var secondsPerVein = MiningSimulator.SecondsPerVein(rig);
            var expected = (3600f / (travelPerVein / speedBoosted + secondsPerVein)) * yield;
            AssertNear(expected, MiningSimulator.MineralsPerHour(rig, quartz, amp), "손으로 계산한 값과 일치");
        });

        Test("P-05 Cargo 증폭기는 CargoHours에, CargoCapacityMinerals는 Cargo·Tool·Engine이 함께 반영된다", () =>
        {
            var rig = new MiningRig { ToolLevel = 3, CargoLevel = 5, EngineLevel = 3 };
            var amp = new RigAmplifierSave();
            amp.Add(RigSlot.Cargo, 0.25f);
            amp.Add(RigSlot.Tool, 0.10f);
            amp.Add(RigSlot.Engine, 0.10f);

            var hoursBase = MiningSimulator.CargoHours(rig, quartz);
            var hoursBoosted = MiningSimulator.CargoHours(rig, quartz, amp);
            AssertNear(hoursBase * 1.25f, hoursBoosted, "화물칸 시간 × 1.25");

            var expectedCap = MiningSimulator.MineralsPerHour(rig, quartz, amp) * hoursBoosted;
            AssertNear(expectedCap, MiningSimulator.CargoCapacityMinerals(rig, quartz, amp), "상한 = amp반영 산출 × amp반영 시간");
        });

        Test("P-05 Refinery 증폭기는 최종 정제량에 곱해진다 — 채굴 속도를 넘어서면 화물칸이 절대 안 찬다", () =>
        {
            var rig = new MiningRig { ToolLevel = 5, EngineLevel = 5, RefineryLevel = 1 }; // 1레벨은 35%만 정제 — 기본으로는 화물칸이 찬다
            var noAmp = MiningSimulator.Offline(rig, quartz, 100 * 3600, false, null);
            Assert(noAmp.HoursWasted > 0f, "증폭 없인 화물칸이 찬다(버려지는 시간 있음)");

            var amp = new RigAmplifierSave();
            amp.Add(RigSlot.Refinery, 5f); // +500% — 정제 속도가 채굴 속도를 넘어서게
            var refineRate = MiningSimulator.RefinePerHour(rig, quartz, false, amp);
            var mineRate = MiningSimulator.MineralsPerHour(rig, quartz, amp);
            Assert(refineRate > mineRate, $"증폭 후 정제({refineRate:F1}) > 채굴({mineRate:F1})");

            var boosted = MiningSimulator.Offline(rig, quartz, 100 * 3600, false, amp);
            AssertNear(0f, boosted.HoursWasted, "정제가 채굴을 따라잡아 화물칸이 절대 안 참");
            AssertNear(0f, boosted.Minerals, "원석이 안 쌓임");
        });

        Test("P-05 Detector 증폭기: 탐지기 레벨 0이면 증폭해도 여전히 0", () =>
        {
            var rig = new MiningRig { DetectorLevel = 0 };
            var amp = new RigAmplifierSave();
            amp.Add(RigSlot.Detector, 5f);
            AssertNear(0f, MiningSimulator.GemsPerHour(rig, quartz, amp), "장비가 없으면 증폭기만으론 안 켜짐");

            var rigWithDetector = new MiningRig { DetectorLevel = 2 };
            var baseGems = MiningSimulator.GemsPerHour(rigWithDetector, quartz);
            var boostedGems = MiningSimulator.GemsPerHour(rigWithDetector, quartz, amp);
            AssertNear(baseGems * 6f, boostedGems, "+500% → 6배");
        });

        Test("P-05(레이싱카) PartAmplifierSave: 칸별로 따로 쌓이고, Module과 0 이하 값은 무시한다", () =>
        {
            var save = new PartAmplifierSave();
            save.Add(PartSlot.Engine, 0.05f);
            save.Add(PartSlot.Engine, 0.10f);
            save.Add(PartSlot.Booster, 0.20f);
            save.Add(PartSlot.Tire, 0f);       // 무시돼야 함
            save.Add(PartSlot.Body, -1f);      // 무시돼야 함
            save.Add(PartSlot.Module, 5f);     // 설계 밖 칸이라 무시돼야 함
            AssertNear(0.15f, save.Bonus(PartSlot.Engine), "Engine 누적");
            AssertNear(0.20f, save.Bonus(PartSlot.Booster), "Booster 누적");
            AssertNear(0f, save.Bonus(PartSlot.Tire), "Tire는 0 그대로");
            AssertNear(0f, save.Bonus(PartSlot.Body), "Body는 음수 무시");
            AssertNear(0f, save.Bonus(PartSlot.Suspension), "안 건드린 칸은 0");
            AssertNear(0f, save.Bonus(PartSlot.Module), "Module은 애초에 못 쌓임");
        });

        Test("P-05(레이싱카) SaveData.AddAmplifier(PartSlot)가 PartAmplifiers.Add로 그대로 이어진다", () =>
        {
            var save = new SaveData();
            save.AddAmplifier(PartSlot.Suspension, 0.30f);
            AssertNear(0.30f, save.PartAmplifiers.Bonus(PartSlot.Suspension), "세이브에 반영");
        });

        Test("P-05(레이싱카) amp=null이면 TotalStats가 기존(무인자) 오버로드와 완전히 같다 — 회귀 없음", () =>
        {
            var car = new RacingCar();
            car.Slots[PartSlot.Engine] = new Part { Slot = PartSlot.Engine, Grade = PartGrade.B, Enhance = 3, Base = new Stats { Power = 20, Boost = 5 } };
            car.Slots[PartSlot.Tire] = new Part { Slot = PartSlot.Tire, Grade = PartGrade.A, Base = new Stats { Grip = 15 } };

            var withoutArg = car.TotalStats();
            var withNull = car.TotalStats(null);
            AssertNear(withoutArg.Power, withNull.Power, "Power");
            AssertNear(withoutArg.Grip, withNull.Grip, "Grip");
            AssertNear(withoutArg.Boost, withNull.Boost, "Boost");
        });

        Test("P-05(레이싱카) 빈 슬롯·기본치는 증폭되지 않고, 부품이 꽂힌 칸만 그 칸의 증폭률만큼 오른다", () =>
        {
            var car = new RacingCar(); // 전부 빈 슬롯 — TotalStats는 최소 기본치만 돌려줌
            var amp = new PartAmplifierSave();
            amp.Add(PartSlot.Engine, 5f); // +500%, 하지만 Engine 슬롯이 비어 있음
            var s = car.TotalStats(amp);
            AssertNear(10f, s.Power, "빈 차 기본 Power는 그대로");
            AssertNear(0f, s.Boost, "빈 차 기본 Boost(0)도 증폭 대상 없음");

            car.Slots[PartSlot.Engine] = new Part { Slot = PartSlot.Engine, Grade = PartGrade.S, Base = new Stats { Power = 40, Boost = 10 } };
            var boosted = car.TotalStats(amp);
            AssertNear(10f + 40f * 6f, boosted.Power, "기본 10 + Engine 파츠(40)×6배");
            AssertNear(0f + 10f * 6f, boosted.Boost, "Engine 파츠 Boost(10)×6배");
        });

        Test("P-05(레이싱카) 서로 다른 칸은 각자의 증폭률만 적용된다 — 칸이 섞이지 않는다", () =>
        {
            var car = new RacingCar();
            car.Slots[PartSlot.Engine] = new Part { Slot = PartSlot.Engine, Grade = PartGrade.C, Base = new Stats { Power = 10 } };
            car.Slots[PartSlot.Tire] = new Part { Slot = PartSlot.Tire, Grade = PartGrade.C, Base = new Stats { Grip = 10 } };
            var amp = new PartAmplifierSave();
            amp.Add(PartSlot.Engine, 1.0f); // Engine만 2배

            var s = car.TotalStats(amp);
            AssertNear(10f + 10f * 2f, s.Power, "Engine 칸만 2배(기본 10 + 파츠 10×2)");
            AssertNear(10f + 10f, s.Grip, "Tire 칸은 증폭 없이 기본 + 파츠 그대로");
        });

        Test("P-05(레이싱카) 강화(Enhance)와 증폭기는 곱이 아니라 각자 적용되지만 최종값엔 둘 다 반영된다", () =>
        {
            var car = new RacingCar();
            // Enhance +10 → Effective 배율 1.6배(Part.Effective 주석). 증폭기 +100% 별도.
            car.Slots[PartSlot.Booster] = new Part { Slot = PartSlot.Booster, Grade = PartGrade.A, Enhance = 10, Base = new Stats { Boost = 20 } };
            var amp = new PartAmplifierSave();
            amp.Add(PartSlot.Booster, 1.0f);

            var s = car.TotalStats(amp);
            AssertNear(20f * 1.6f * 2f, s.Boost, "기본20 × 강화1.6 × 증폭2 — 순서와 무관하게 곱셈이라 결합법칙 성립");
        });

        Test("P-07: 행성 여섯 개 전부 MineralNameKo가 채워져 있다", () =>
        {
            foreach (var p in DefaultData.Planets())
                Assert(!string.IsNullOrEmpty(p.MineralNameKo), $"{p.Id}의 MineralNameKo가 비어 있음");
        });

        Test("P-07 PlanetMineralBank: 처음 보는 행성은 0, Add로 새 칸이 생기고 같은 행성은 누적된다", () =>
        {
            var ids = new List<string>(); var amounts = new List<float>();
            AssertNear(0f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "담기기 전엔 0");

            PlanetMineralBank.Add(ids, amounts, "quartz", 10f);
            AssertNear(10f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "10 담김");

            PlanetMineralBank.Add(ids, amounts, "quartz", 5f);
            AssertNear(15f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "같은 행성은 누적(10+5)");

            PlanetMineralBank.Add(ids, amounts, "ruby", 7f);
            AssertNear(15f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "ruby를 담아도 quartz는 그대로");
            AssertNear(7f, PlanetMineralBank.Amount(ids, amounts, "ruby"), "ruby 칸은 따로");
        });

        Test("P-07 PlanetMineralBank.Add: 0 이하는 무시한다", () =>
        {
            var ids = new List<string>(); var amounts = new List<float>();
            PlanetMineralBank.Add(ids, amounts, "quartz", 0f);
            PlanetMineralBank.Add(ids, amounts, "quartz", -5f);
            AssertNear(0f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "0/음수는 무시되어 칸조차 안 생김");
            Assert(ids.Count == 0, "칸이 생기지 않았다");
        });

        Test("P-07 PlanetMineralBank.TrySpend: 모자라면 부분 차감 없이 실패, 충분하면 정확히 깎는다", () =>
        {
            var ids = new List<string>(); var amounts = new List<float>();
            PlanetMineralBank.Add(ids, amounts, "quartz", 10f);

            Assert(!PlanetMineralBank.TrySpend(ids, amounts, "quartz", 20f), "모자라면 실패");
            AssertNear(10f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "실패해도 원래 값 그대로(부분 차감 없음)");

            Assert(!PlanetMineralBank.TrySpend(ids, amounts, "ruby", 1f), "담긴 적 없는 행성도 실패");
            Assert(!PlanetMineralBank.TrySpend(ids, amounts, "quartz", 0f), "0 이하 요청은 실패");
            Assert(!PlanetMineralBank.TrySpend(ids, amounts, "quartz", -1f), "음수 요청은 실패");

            Assert(PlanetMineralBank.TrySpend(ids, amounts, "quartz", 4f), "충분하면 성공");
            AssertNear(6f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "10 - 4 = 6");
        });

        Test("P-07 PlanetMineralRecipe: 여러 행성을 섞은 비용을 all-or-nothing으로 처리한다", () =>
        {
            var ids = new List<string>(); var amounts = new List<float>();
            PlanetMineralBank.Add(ids, amounts, "quartz", 10f);
            PlanetMineralBank.Add(ids, amounts, "ruby", 20f);

            var costs = new List<MineralCost>
            {
                new MineralCost { PlanetId = "quartz", Amount = 10f },
                new MineralCost { PlanetId = "ruby", Amount = 20f },
            };
            Assert(PlanetMineralRecipe.CanAfford(ids, amounts, costs), "딱 맞게 감당 가능");

            var tooMuch = new List<MineralCost>
            {
                new MineralCost { PlanetId = "quartz", Amount = 10f },
                new MineralCost { PlanetId = "ruby", Amount = 999f }, // 모자람
            };
            Assert(!PlanetMineralRecipe.CanAfford(ids, amounts, tooMuch), "하나라도 모자라면 전체 실패");
            Assert(!PlanetMineralRecipe.TrySpend(ids, amounts, tooMuch), "TrySpend도 실패");
            AssertNear(10f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "실패한 TrySpend는 quartz도 안 깎음(all-or-nothing)");
            AssertNear(20f, PlanetMineralBank.Amount(ids, amounts, "ruby"), "실패한 TrySpend는 ruby도 안 깎음");

            Assert(PlanetMineralRecipe.TrySpend(ids, amounts, costs), "감당 가능하면 성공");
            AssertNear(0f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "quartz 10 전부 소비");
            AssertNear(0f, PlanetMineralBank.Amount(ids, amounts, "ruby"), "ruby 20 전부 소비");
        });

        Test("P-07 PlanetMineralRecipe: costs가 빈 목록이면 항상 감당 가능하고 아무것도 안 깎는다(경계값)", () =>
        {
            var ids = new List<string>(); var amounts = new List<float>();
            PlanetMineralBank.Add(ids, amounts, "quartz", 5f);

            var empty = new List<MineralCost>();
            Assert(PlanetMineralRecipe.CanAfford(ids, amounts, empty), "빈 레시피는 항상 감당 가능");
            Assert(PlanetMineralRecipe.TrySpend(ids, amounts, empty), "빈 레시피 TrySpend는 항상 성공");
            AssertNear(5f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "빈 레시피는 아무것도 안 깎는다");

            // 아무것도 캐 본 적 없는(리스트 자체가 빈) 상태에서도 마찬가지.
            var neverMined = new List<string>(); var neverMinedAmounts = new List<float>();
            Assert(PlanetMineralRecipe.CanAfford(neverMined, neverMinedAmounts, empty), "창고가 비어 있어도 빈 레시피는 감당 가능");
            Assert(PlanetMineralRecipe.TrySpend(neverMined, neverMinedAmounts, empty), "창고가 비어 있어도 빈 레시피 TrySpend는 성공");
        });

        Test("P-07 PlanetMineralRecipe: 같은 행성이 costs에 두 줄이면 합쳐서 감당 여부를 본다(부분 차감 버그 회귀)", () =>
        {
            var ids = new List<string>(); var amounts = new List<float>();
            PlanetMineralBank.Add(ids, amounts, "quartz", 1.5f);

            // 각 줄은 보유량(1.5)보다 작지만 합(2.0)은 넘는다 — 줄 단위로만 보면 잘못 통과한다.
            var costs = new List<MineralCost>
            {
                new MineralCost { PlanetId = "quartz", Amount = 1f },
                new MineralCost { PlanetId = "quartz", Amount = 1f },
            };
            Assert(!PlanetMineralRecipe.CanAfford(ids, amounts, costs), "합쳐서 보면 2.0 필요, 1.5뿐이라 실패해야 함");
            Assert(!PlanetMineralRecipe.TrySpend(ids, amounts, costs), "TrySpend도 실패해야 함");
            AssertNear(1.5f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "실패했으니 한 줄도 안 깎여야 함(부분 차감 없음)");

            // 합쳐서 딱 맞으면 성공하고 합계만큼 정확히 깎인다.
            var exact = new List<MineralCost>
            {
                new MineralCost { PlanetId = "quartz", Amount = 0.7f },
                new MineralCost { PlanetId = "quartz", Amount = 0.8f },
            };
            Assert(PlanetMineralRecipe.TrySpend(ids, amounts, exact), "합쳐서 1.5로 딱 맞으면 성공");
            AssertNear(0f, PlanetMineralBank.Amount(ids, amounts, "quartz"), "0.7+0.8=1.5 전부 소비");
        });

        Test("P-07 SaveData: 새 세이브의 PlanetMineralIds/Amounts는 빈 리스트로 시작한다(마이그레이션 불필요)", () =>
        {
            var save = new SaveData();
            Assert(save.PlanetMineralIds.Count == 0, "새 세이브는 빈 리스트");
            Assert(save.PlanetMineralAmounts.Count == 0, "새 세이브는 빈 리스트");
            PlanetMineralBank.Add(save.PlanetMineralIds, save.PlanetMineralAmounts, "quartz", 3f);
            AssertNear(3f, PlanetMineralBank.Amount(save.PlanetMineralIds, save.PlanetMineralAmounts, "quartz"), "SaveData 리스트에 그대로 반영");
        });

        Test("P-06 PlanetToolLevel.Level: 처음 보는 행성은 1, Set으로 새 칸이 생기고 같은 행성은 값을 대입한다", () =>
        {
            var ids = new List<string>(); var levels = new List<int>();
            Assert(PlanetToolLevel.Level(ids, levels, "quartz") == 1, "저장 전엔 1(MiningRig.ToolLevel 기본값과 같음)");

            PlanetToolLevel.Set(ids, levels, "quartz", 10);
            Assert(PlanetToolLevel.Level(ids, levels, "quartz") == 10, "10으로 저장됨");

            PlanetToolLevel.Set(ids, levels, "quartz", 15);
            Assert(PlanetToolLevel.Level(ids, levels, "quartz") == 15, "누적이 아니라 대입 — 15로 덮어씀");

            PlanetToolLevel.Set(ids, levels, "ruby", 3);
            Assert(PlanetToolLevel.Level(ids, levels, "quartz") == 15, "ruby를 저장해도 quartz는 그대로");
            Assert(PlanetToolLevel.Level(ids, levels, "ruby") == 3, "ruby 칸은 따로");
        });

        Test("P-06 PlanetToolLevel.Set: 1 미만은 1로 올려 잡는다(레벨은 0/음수가 될 수 없음)", () =>
        {
            var ids = new List<string>(); var levels = new List<int>();
            PlanetToolLevel.Set(ids, levels, "quartz", 0);
            Assert(PlanetToolLevel.Level(ids, levels, "quartz") == 1, "0은 1로 올려 잡음");

            PlanetToolLevel.Set(ids, levels, "quartz", -5);
            Assert(PlanetToolLevel.Level(ids, levels, "quartz") == 1, "음수도 1로 올려 잡음");
        });

        Test("P-06 PlanetToolLevel.CarryOverStartLevel: 이전 레벨의 40%를 내림, 최소 1레벨 보장", () =>
        {
            Assert(PlanetToolLevel.CarryOverStartLevel(15) == 6, "15 * 0.4 = 6.0 → 6");
            Assert(PlanetToolLevel.CarryOverStartLevel(10) == 4, "10 * 0.4 = 4.0 → 4");
            Assert(PlanetToolLevel.CarryOverStartLevel(3) == 1, "3 * 0.4 = 1.2 → 내림 1");
            Assert(PlanetToolLevel.CarryOverStartLevel(1) == 1, "1 * 0.4 = 0.4 → 내림 0이지만 최소 1 보장");
            Assert(PlanetToolLevel.CarryOverStartLevel(0) == 1, "0레벨에서 옮기는 경우는 없지만 방어적으로 1");
            Assert(PlanetToolLevel.CarryOverStartLevel(-3) == 1, "음수 입력도 방어적으로 1");
            Assert(PlanetToolLevel.CarryOverStartLevel(20, 0.5) == 10, "fraction을 다르게 주면 그 비율로(20 * 0.5 = 10)");
        });

        Test("P-06 SaveData: 새 세이브의 ToolLevelPlanetIds/Values는 빈 리스트로 시작한다(마이그레이션 불필요)", () =>
        {
            var save = new SaveData();
            Assert(save.ToolLevelPlanetIds.Count == 0, "새 세이브는 빈 리스트");
            Assert(save.ToolLevelValues.Count == 0, "새 세이브는 빈 리스트");
            PlanetToolLevel.Set(save.ToolLevelPlanetIds, save.ToolLevelValues, "quartz", 12);
            Assert(PlanetToolLevel.Level(save.ToolLevelPlanetIds, save.ToolLevelValues, "quartz") == 12, "SaveData 리스트에 그대로 반영");
        });

        Test("P-14 SaveData.PetGacha: 새 세이브는 등급 7칸이 전부 0으로 시작(마이그레이션 불필요)", () =>
        {
            var save = new SaveData();
            Assert(save.PetGacha.OwnedSpeciesCountByGrade.Count == 7, "가진 종 수 배열은 7칸");
            Assert(save.PetGacha.ShardsByGrade.Count == 7, "조각 수 배열은 7칸");
            Assert(save.PetGacha.OwnedSpeciesCountByGrade.All(c => c == 0), "새 세이브는 전부 0");
            Assert(save.PetGacha.ShardsByGrade.All(c => c == 0), "새 세이브는 전부 0");
            // PetCollection.CollectionBonus가 그대로 받아도 예외가 안 나야 한다(길이·범위 검사 통과).
            AssertNear(0f, PetCollection.CollectionBonus(save.PetGacha.OwnedSpeciesCountByGrade.ToArray()), "빈 도감은 보너스 0");
        });

        Test("P-14 SaveData.PetGacha: 새 세이브의 OwnedSpeciesIds는 종 수만큼 전부 false로 시작한다(마이그레이션 불필요)", () =>
        {
            var save = new SaveData();
            Assert(save.PetGacha.OwnedSpeciesIds.Count == PetSpeciesTable.All.Count,
                $"종 수({PetSpeciesTable.All.Count})만큼 칸이 있다, 실제 {save.PetGacha.OwnedSpeciesIds.Count}");
            Assert(save.PetGacha.OwnedSpeciesIds.All(b => !b), "새 세이브는 전부 false");
            Assert(!save.PetGacha.OwnsSpecies(0), "OwnsSpecies도 false부터 시작");
        });

        Test("P-14 SaveData.PetGacha.MarkSpeciesOwned: 처음 얻은 종은 도감+등급 카운트를 같이 채우고, 중복은 false만 돌려준다", () =>
        {
            var save = new SaveData();
            var id = PetSpeciesTable.InGrade(PetGrade.Common)[0];

            Assert(save.PetGacha.MarkSpeciesOwned(id), "처음 얻은 종은 true(신규)");
            Assert(save.PetGacha.OwnsSpecies(id), "도감에 표시됨");
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Common) == 1, "그 등급 카운트도 같이 오른다");

            Assert(!save.PetGacha.MarkSpeciesOwned(id), "같은 종을 다시 얻으면 false(중복)");
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Common) == 1, "중복은 카운트를 안 올린다 — 조각 지급은 호출부(PetGachaController) 몫");
        });

        Test("P-14 SaveData.PetGacha.AddOwnedSpecies: 칸별로 독립적으로 쌓이고 등급 최대치에서 멈춘다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddOwnedSpecies(PetGrade.Common);
            save.PetGacha.AddOwnedSpecies(PetGrade.Common);
            save.PetGacha.AddOwnedSpecies(PetGrade.Rare);
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Common) == 2, "일반 2종");
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Rare) == 1, "희귀 1종, 일반과 안 섞임");
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Advanced) == 0, "고급은 그대로 0");

            // 초월(7등급)은 종이 10개뿐 — 11번 채워도 10에서 멈춰야 CollectionBonus가 안 터진다.
            for (var i = 0; i < 11; i++) save.PetGacha.AddOwnedSpecies(PetGrade.Transcendent);
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Transcendent) == 10, "최대 종 수(10)에서 멈춤");
            PetCollection.CollectionBonus(save.PetGacha.OwnedSpeciesCountByGrade.ToArray()); // 예외 없이 통과해야 함
        });

        Test("P-14 SaveData.PetGacha.AddShards/SetShards: 0 이하 무시, 음수 설정은 예외", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Legendary, 8);
            save.PetGacha.AddShards(PetGrade.Legendary, 0);
            save.PetGacha.AddShards(PetGrade.Legendary, -3);
            Assert(save.PetGacha.Shards(PetGrade.Legendary) == 8, "0/음수는 무시, 8만 반영");

            var (promotions, remaining) = PetFusion.ExchangeForPromotion(PetGrade.Legendary, save.PetGacha.Shards(PetGrade.Legendary));
            Assert(promotions == 1 && remaining == 0, "전설 조각 8개 = 승급 1회, 나머지 0(PetFusion.PromotionCost(Legendary)==8)");
            save.PetGacha.SetShards(PetGrade.Legendary, remaining);
            Assert(save.PetGacha.Shards(PetGrade.Legendary) == 0, "SetShards로 나머지 되돌려 쓰기");

            try { save.PetGacha.SetShards(PetGrade.Common, -1); Assert(false, "음수 설정은 예외여야 함"); }
            catch (ArgumentException) { }
        });

        Test("P-14 SaveData.PetGacha.RecordAdvancedPull/RecordSpecialPull: 천장 카운터가 등급마다 따로 움직인다", () =>
        {
            var save = new SaveData();
            save.PetGacha.RecordAdvancedPull(guaranteed: false);
            save.PetGacha.RecordAdvancedPull(guaranteed: false);
            Assert(save.PetGacha.AdvancedOpenedSincePity == 2, "고급 천장 카운터 2회 증가");
            Assert(save.PetGacha.SpecialOpenedSincePity == 0, "특수 천장 카운터는 안 움직임(뽑기별로 완전히 별개, pet-gacha.md 3절)");

            save.PetGacha.RecordAdvancedPull(guaranteed: true);
            Assert(save.PetGacha.AdvancedOpenedSincePity == 0, "확정 지급 후 0으로 리셋");

            save.PetGacha.RecordSpecialPull(guaranteed: false);
            Assert(save.PetGacha.SpecialOpenedSincePity == 1, "특수 천장 카운터 별도로 1");
        });

        Test("P-14 SaveData.PetGacha.ResetDailyIfNewDay: 날짜가 바뀌면 하루 한도만 리셋되고 천장·조각은 그대로", () =>
        {
            var save = new SaveData();
            save.PetGacha.RecordAdvancedPull(guaranteed: false);
            save.PetGacha.AddShards(PetGrade.Common, 2);
            for (var i = 0; i < PetGachaTable.FreePullDailyLimit; i++) save.PetGacha.RecordFreePull();
            save.PetGacha.AdvancedFreePullClaimedToday = true;
            Assert(!save.PetGacha.CanPullFree(), "하루 10회를 다 썼으면 더 못 돌림");

            const long secondsPerDay = 86400;
            save.PetGacha.ResetDailyIfNewDay(nowUnixSeconds: 0, timeZoneOffsetSeconds: 0);
            Assert(save.PetGacha.FreePullsToday == 10, "같은 날이면 그대로");

            save.PetGacha.ResetDailyIfNewDay(nowUnixSeconds: secondsPerDay, timeZoneOffsetSeconds: 0);
            Assert(save.PetGacha.FreePullsToday == 0, "다음 날이면 무료 뽑기 카운트 리셋");
            Assert(!save.PetGacha.AdvancedFreePullClaimedToday, "고급 무료분도 리셋");
            Assert(save.PetGacha.CanPullFree(), "리셋 후 다시 돌릴 수 있음");
            Assert(save.PetGacha.AdvancedOpenedSincePity == 1, "천장 카운터는 하루 리셋과 무관 — 그대로 유지");
            Assert(save.PetGacha.Shards(PetGrade.Common) == 2, "조각도 하루 리셋과 무관 — 그대로 유지");
        });

        Test("P-14 ③ PetGachaController.PullFree: 성공하면 등급이 도감에 반영되고 오늘 뽑은 횟수가 오른다", () =>
        {
            var save = new SaveData();
            var outcome = PetGachaController.PullFree(save, seed: 42);

            Assert(outcome.Success, "한도 안이면 성공");
            Assert(save.PetGacha.FreePullsToday == 1, "무료 뽑기 횟수가 올라간다");
            var total = 0;
            foreach (PetGrade g in Enum.GetValues(typeof(PetGrade))) total += save.PetGacha.OwnedSpeciesCount(g);
            Assert(total == 1, "정확히 한 등급 칸이 하나 늘어난다");
            Assert(save.PetGacha.OwnedSpeciesCount(outcome.Result.Grade) == 1, "그 칸이 바로 뽑힌 등급이다");
        });

        Test("P-14 ③ PetGachaController.PullFree: 오늘 한도를 다 쓰면 안 뽑히고 도감·카운트가 그대로다", () =>
        {
            var save = new SaveData();
            for (var i = 0; i < PetGachaTable.FreePullDailyLimit; i++) save.PetGacha.RecordFreePull();

            var before = save.PetGacha.FreePullsToday;
            var outcome = PetGachaController.PullFree(save, seed: 7);

            Assert(!outcome.Success, "한도를 넘기면 실패");
            Assert(save.PetGacha.FreePullsToday == before, "카운트도 그대로");
        });

        Test("P-14 ③ PetGachaController.PullFree: 같은 seed면 같은 결과(서버 재검증 재현성, CLAUDE.md 1번)", () =>
        {
            var a = PetGachaController.PullFree(new SaveData(), seed: 999);
            var b = PetGachaController.PullFree(new SaveData(), seed: 999);
            Assert(a.Success && b.Success, "둘 다 성공");
            Assert(a.Result.Grade == b.Result.Grade, "같은 seed는 같은 등급");
            Assert(a.Result.SpeciesId == b.Result.SpeciesId, "같은 seed는 같은 종(등급 추첨과 별개로 재현 가능해야 함)");
        });

        Test("P-14 ③ PetGachaController.PullFree: 결과에 실제 종 id가 채워지고, 그 종은 뽑힌 등급 소속이다(P-14 ② 연결)", () =>
        {
            var save = new SaveData();
            var outcome = PetGachaController.PullFree(save, seed: 42);
            Assert(outcome.Result.SpeciesId >= 0 && outcome.Result.SpeciesId < PetSpeciesTable.All.Count,
                $"유효한 종 id 범위, 실제 {outcome.Result.SpeciesId}");
            Assert(PetSpeciesTable.Get(outcome.Result.SpeciesId).Grade == outcome.Result.Grade,
                "뽑힌 종이 뽑힌 등급 소속이어야 한다");
            Assert(outcome.Result.IsNewSpecies, "빈 세이브라 처음 얻은 종이다");
            Assert(save.PetGacha.OwnsSpecies(outcome.Result.SpeciesId), "그 종이 도감에 반영됨");
        });

        Test("P-14 ③ PetGachaController.PullFree: 등급 도감이 이미 가득 찬 칸은 중복 처리로 넘어가(조각 지급) 안 넘친다", () =>
        {
            var save = new SaveData();
            var max = PetGradeInfo.SpeciesCountFor(PetGrade.Common);
            foreach (var id in PetSpeciesTable.InGrade(PetGrade.Common)) save.PetGacha.MarkSpeciesOwned(id);
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Common) == max, "테스트 준비: 일반 등급 도감을 가득 채움");

            // Free() 표에서 일반 등급이 나오는 seed를 찾아, 이미 가득 찬 상태에서 그대로 넣어 본다.
            var seed = 1;
            while (PetGachaTable.Open(PetGachaTable.Free(), seed).Grade != PetGrade.Common && seed < 10000) seed++;
            Assert(seed < 10000, "테스트 준비: 일반 등급이 나오는 seed를 찾았어야 함");

            var shardsBefore = save.PetGacha.Shards(PetGrade.Common);
            var outcome = PetGachaController.PullFree(save, seed);
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Common) == max, "가득 찬 칸은 상한에서 안 넘친다");
            Assert(!outcome.Result.IsNewSpecies, "이미 다 가진 등급이라 어느 종을 뽑아도 중복이다");
            Assert(save.PetGacha.Shards(PetGrade.Common) == shardsBefore + 1, "중복은 대신 조각 1개로 지급된다(P-14 ② 연결)");
        });

        Test("P-14 ③ PetGachaController.PullNormal: 하루 한도가 없어 무료 한도(10)를 넘겨도 계속 반영되고, 무료 카운트는 안 건드린다", () =>
        {
            var save = new SaveData();
            const int pulls = PetGachaTable.FreePullDailyLimit + 5; // 15
            for (var i = 0; i < pulls; i++) PetGachaController.PullNormal(save, seed: i * 104729);

            var total = 0;
            foreach (PetGrade g in Enum.GetValues(typeof(PetGrade))) total += save.PetGacha.OwnedSpeciesCount(g);
            // 등급마다 상한이 넉넉해서(Normal()이 뽑는 6등급 중 가장 작은 상한도 16종) 15회
            // 안에 한 등급이 가득 찰 확률은 낮지만, 그래도 상한에 막힐 수는 있으니
            // "넘지 않는다"까지만 확인한다.
            Assert(total > 0 && total <= pulls, $"뽑은 만큼 도감에 반영되고 pulls({pulls})를 넘지 않는다(실제 {total})");
            Assert(save.PetGacha.FreePullsToday == 0, "일반 뽑기는 무료 뽑기 카운트를 안 건드린다");
        });

        // A-17: 장착 중인 펫(SaveData.PetGachaSave.EquippedSpeciesId). 뽑기 실행 화면·도감이
        // 붙기 전에 세이브 스키마부터 먼저 채운 것(docs/backlog.md A-17 "이어서 할 것 (3)").
        Test("A-17 SaveData.PetGacha: 새 세이브는 미장착(-1)으로 시작한다(마이그레이션 불필요)", () =>
        {
            var save = new SaveData();
            Assert(save.PetGacha.EquippedSpeciesId == -1, "기본값 -1");
            Assert(!save.PetGacha.HasEquippedSpecies, "HasEquippedSpecies도 false");
        });

        Test("A-17 SaveData.PetGacha.EquipSpecies: 도감에 있는 종만 장착할 수 있다", () =>
        {
            var save = new SaveData();
            var id = PetSpeciesTable.InGrade(PetGrade.Common)[0];

            var threw = false;
            try { save.PetGacha.EquipSpecies(id); } catch (ArgumentException) { threw = true; }
            Assert(threw, "아직 도감에 없으면 예외");

            save.PetGacha.MarkSpeciesOwned(id);
            save.PetGacha.EquipSpecies(id);
            Assert(save.PetGacha.EquippedSpeciesId == id, "장착됨");
            Assert(save.PetGacha.HasEquippedSpecies, "HasEquippedSpecies도 true");
        });

        Test("A-17 SaveData.PetGacha.EquipSpecies/UnequipSpecies: 다른 종으로 바꿔 끼우고, 해제하면 -1로 돌아간다", () =>
        {
            var save = new SaveData();
            var commonIds = PetSpeciesTable.InGrade(PetGrade.Common);
            save.PetGacha.MarkSpeciesOwned(commonIds[0]);
            save.PetGacha.MarkSpeciesOwned(commonIds[1]);

            save.PetGacha.EquipSpecies(commonIds[0]);
            save.PetGacha.EquipSpecies(commonIds[1]);
            Assert(save.PetGacha.EquippedSpeciesId == commonIds[1], "나중에 장착한 종으로 바뀐다(한 마리만)");

            save.PetGacha.UnequipSpecies();
            Assert(save.PetGacha.EquippedSpeciesId == -1, "해제하면 -1");
            Assert(!save.PetGacha.HasEquippedSpecies, "HasEquippedSpecies도 false");

            save.PetGacha.UnequipSpecies();
            Assert(save.PetGacha.EquippedSpeciesId == -1, "이미 미장착이어도 그대로 -1(AddShards 0 이하 무시와 같은 관용)");
        });

        // P-16: 초월의 인장(SaveData.PetGachaSave.AddSeal) + 고급/특수 뽑기 컨트롤러.
        Test("P-16 PetGachaSave.AddSeal: 더하고, 0 이하는 무시한다(AddShards와 같은 규칙)", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddSeal(5);
            Assert(save.PetGacha.TranscendentSealCount == 5, "5장");
            save.PetGacha.AddSeal(2);
            Assert(save.PetGacha.TranscendentSealCount == 7, "누적 7장");
            save.PetGacha.AddSeal(0);
            save.PetGacha.AddSeal(-3);
            Assert(save.PetGacha.TranscendentSealCount == 7, "0 이하는 무시되어 그대로 7장");
        });

        Test("P-16 PetGachaController.PullAdvanced: 하루 무료분은 한 번만, 이후는 useFreeDaily=false로 항상 뽑힌다", () =>
        {
            var save = new SaveData();
            var first = PetGachaController.PullAdvanced(save, seed: 1, useFreeDaily: true);
            Assert(first.Success, "첫 무료 뽑기는 성공");
            Assert(save.PetGacha.AdvancedFreePullClaimedToday, "오늘 무료분 소진 표시");

            var second = PetGachaController.PullAdvanced(save, seed: 2, useFreeDaily: true);
            Assert(!second.Success, "같은 날 두 번째 무료 요청은 실패");

            var paid = PetGachaController.PullAdvanced(save, seed: 3, useFreeDaily: false);
            Assert(paid.Success, "유료(useFreeDaily=false)는 무료분과 무관하게 항상 성공");
        });

        Test("P-16 PetGachaController.PullAdvanced: 성공하면 도감에 반영되고 천장 카운터가 올라간다", () =>
        {
            var save = new SaveData();
            PetGachaController.PullAdvanced(save, seed: 42, useFreeDaily: false);
            var total = 0;
            foreach (PetGrade g in Enum.GetValues(typeof(PetGrade))) total += save.PetGacha.OwnedSpeciesCount(g);
            Assert(total == 1, $"도감에 정확히 1마리 반영, 실제 {total}");
            Assert(save.PetGacha.AdvancedOpenedSincePity == 1, "천장 카운터 +1");
        });

        Test("P-16 PetGachaController.PullAdvanced: 80번째 뽑기는 확정(신화)이고 카운터가 0으로 되돌아간다", () =>
        {
            var save = new SaveData();
            for (var i = 0; i < PetGachaTable.AdvancedPityCount - 1; i++)
                PetGachaController.PullAdvanced(save, seed: i * 104729, useFreeDaily: false);
            Assert(save.PetGacha.AdvancedOpenedSincePity == PetGachaTable.AdvancedPityCount - 1, "천장 직전");

            var last = PetGachaController.PullAdvanced(save, seed: 999, useFreeDaily: false);
            Assert(last.Result.Guaranteed && last.Result.Grade == PetGachaTable.AdvancedPityGrade,
                $"{PetGachaTable.AdvancedPityCount}번째는 확정 {PetGachaTable.AdvancedPityGrade}, 실제 {last.Result.Grade}");
            Assert(save.PetGacha.AdvancedOpenedSincePity == 0, "확정 이후 카운터 리셋");
        });

        Test("P-16 PetGachaController.PullAdvancedTen: 10연차는 전설 이상이 없으면 하나를 확정으로 채운다", () =>
        {
            var save = new SaveData();
            var results = PetGachaController.PullAdvancedTen(save, baseSeed: 7);
            Assert(results.Length == 10, "10개");
            Assert(results.Any(r => r.Grade >= PetGachaTable.AdvancedTenPullMinGrade), "전설 이상 최소 1마리 보장");

            // 10연차 안에서도 같은 등급·같은 종이 두 번 나올 수 있다(종 하나에 16~20종뿐이라
            // 생일 문제로 실제로 흔하다) — 그러면 새 종이 아니라 조각으로 간다. 그래서 "정확히
            // 10마리"가 아니라 "새 종 + 중복(조각)을 합치면 10"으로 확인해야 어떤 seed에도 맞다.
            var newSpeciesCount = results.Count(r => r.IsNewSpecies);
            var dexTotal = 0;
            foreach (PetGrade g in Enum.GetValues(typeof(PetGrade))) dexTotal += save.PetGacha.OwnedSpeciesCount(g);
            var shardTotal = 0;
            foreach (PetGrade g in Enum.GetValues(typeof(PetGrade))) shardTotal += save.PetGacha.Shards(g);
            Assert(dexTotal == newSpeciesCount, $"도감 증가분이 새 종 개수와 같다, 실제 {dexTotal} vs {newSpeciesCount}");
            Assert(shardTotal == 10 - newSpeciesCount, $"나머지는 전부 조각으로, 실제 조각 {shardTotal}");
            Assert(dexTotal + shardTotal == 10, "10마리 전부 도감 또는 조각으로 반영된다");
        });

        Test("P-16 PetGachaController.PullAdvancedTen: 천장 카운터가 회차마다 하나씩 이어진다(단일 뽑기와 같은 결과)", () =>
        {
            var single = new SaveData();
            var ten = new SaveData();
            var results = PetGachaController.PullAdvancedTen(ten, baseSeed: 55);
            for (var i = 0; i < 10; i++)
                PetGachaController.PullAdvanced(single, seed: 55 + i, useFreeDaily: false);
            Assert(single.PetGacha.AdvancedOpenedSincePity == ten.PetGacha.AdvancedOpenedSincePity,
                $"10연차와 단일 10회의 최종 천장 카운터가 같다 {ten.PetGacha.AdvancedOpenedSincePity} == {single.PetGacha.AdvancedOpenedSincePity}");
        });

        Test("P-16 PetGachaController.PullAdvancedTen: 10연차 최소 등급 보장으로 확정된 칸이 있어도 진짜 80천장과 안 섞인다", () =>
        {
            // OpenTen의 Guaranteed는 "80천장 도달"과 "10연차 최소 전설 보장" 둘 다 true를 준다
            // (PetGachaResult 주석). 그걸 그대로 믿고 RecordAdvancedPull에 넘기면, 최소 등급
            // 보장으로 확정된 칸에서도 진짜 피티 진행도가 사라진다 — 그래서 항상 정확히 10만큼
            // 늘어나야 한다(80천장 근처가 아닌 한, 위 테스트와 같은 전제).
            var save = new SaveData();
            // baseSeed 여러 개를 훑어 "전설 이상이 하나도 없어 보장이 실제로 발동하는" 배치를 찾는다.
            var foundBackfill = false;
            for (var baseSeed = 1; baseSeed <= 500 && !foundBackfill; baseSeed++)
            {
                var probe = PetGachaTable.OpenTen(PetGachaTable.Advanced(), baseSeed * 104729,
                    PetGachaTable.AdvancedTenPullMinGrade, 0, PetGachaTable.AdvancedPityCount, PetGachaTable.AdvancedPityGrade);
                if (probe.Any(r => r.Guaranteed))
                {
                    foundBackfill = true;
                    PetGachaController.PullAdvancedTen(save, baseSeed * 104729);
                }
            }
            Assert(foundBackfill, "테스트 준비: 500 이내에 최소 등급 보장이 발동하는 배치를 찾았어야 함");
            Assert(save.PetGacha.AdvancedOpenedSincePity == 10,
                $"보장 발동 배치여도 진짜 피티가 80과 거리가 멀면 카운터는 정확히 10, 실제 {save.PetGacha.AdvancedOpenedSincePity}");
        });

        Test("P-16 PetGachaController.PullSpecial: 인장이 없으면 안 뽑히고, 성공하면 인장을 하나 쓴다", () =>
        {
            var save = new SaveData();
            var noSeal = PetGachaController.PullSpecial(save, seed: 1);
            Assert(!noSeal.Success, "인장 0장이면 실패");

            save.PetGacha.AddSeal(2);
            var first = PetGachaController.PullSpecial(save, seed: 2);
            Assert(first.Success, "인장 있으면 성공");
            Assert(save.PetGacha.TranscendentSealCount == 1, "인장 1장 소모");

            var second = PetGachaController.PullSpecial(save, seed: 3);
            Assert(second.Success, "남은 인장으로 한 번 더 성공");
            Assert(save.PetGacha.TranscendentSealCount == 0, "인장 0장으로");

            var third = PetGachaController.PullSpecial(save, seed: 4);
            Assert(!third.Success, "인장 다 쓰면 다시 실패");
        });

        Test("P-16 PetGachaController.PullSpecial: 성공하면 도감에 반영되고 특수 천장 카운터가 올라간다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddSeal(1);
            PetGachaController.PullSpecial(save, seed: 123);
            var total = 0;
            foreach (PetGrade g in Enum.GetValues(typeof(PetGrade))) total += save.PetGacha.OwnedSpeciesCount(g);
            Assert(total == 1, $"도감에 정확히 1마리 반영, 실제 {total}");
            Assert(save.PetGacha.SpecialOpenedSincePity == 1, "특수 천장 카운터 +1");
        });

        Test("P-16 PetGachaController.PullSpecial: 80번째는 확정(초월)이고, 인장이 부족하면 확정 직전에 멈춘다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddSeal(PetGachaTable.SpecialPityCount);
            for (var i = 0; i < PetGachaTable.SpecialPityCount - 1; i++)
                PetGachaController.PullSpecial(save, seed: i * 104729);
            Assert(save.PetGacha.TranscendentSealCount == 1, "천장 직전, 인장 1장 남음");

            var last = PetGachaController.PullSpecial(save, seed: 999);
            Assert(last.Success && last.Result.Guaranteed && last.Result.Grade == PetGachaTable.SpecialPityGrade,
                $"{PetGachaTable.SpecialPityCount}번째는 확정 {PetGachaTable.SpecialPityGrade}, 실제 {last.Result.Grade}");
            Assert(save.PetGacha.SpecialOpenedSincePity == 0, "확정 이후 카운터 리셋");
            Assert(save.PetGacha.TranscendentSealCount == 0, "확정 뽑기도 인장을 소모한다(공짜가 아니다)");
        });

        // A-17 이어서(조각 합성 실행 화면의 코어 쪽): PetFusionController.
        Test("A-17 PetFusionController.FuseSameGrade: 조각이 부족하면 아무 일도 안 일어난다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Common, PetFusion.SameGradeFragmentCost - 1);
            var result = PetFusionController.FuseSameGrade(save, PetGrade.Common, seed: 1);
            Assert(result.Pets.Length == 0, "조각 3개 미만이면 펫 0마리");
            Assert(result.RemainingFragments == PetFusion.SameGradeFragmentCost - 1, "조각은 그대로 남는다");
            Assert(save.PetGacha.Shards(PetGrade.Common) == PetFusion.SameGradeFragmentCost - 1, "세이브에도 그대로");
        });

        Test("A-17 PetFusionController.FuseSameGrade: 조각을 소모한 만큼 펫이 나오고, 조각은 버려지지 않는다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Common, 9); // cost 3 → 3마리, 나머지 0
            var result = PetFusionController.FuseSameGrade(save, PetGrade.Common, seed: 42);
            Assert(result.Pets.Length == 3, $"9 / {PetFusion.SameGradeFragmentCost} = 3마리, 실제 {result.Pets.Length}");
            Assert(result.RemainingFragments == 0, "9는 3으로 나누어떨어지니 나머지 0");

            // 3마리 중 일부가 중복이면 뽑기와 같은 규칙으로 조각으로 자동 전환된다(PetGachaController와
            // 같은 검증 패턴, P-16 PullAdvancedTen 테스트 참고) — 그래서 최종 조각 수는 그 중복 개수와 같아야
            // "조각이 버려지지 않는다"(pet-gacha.md 2절)가 성립한다.
            var newSpeciesCount = result.Pets.Count(p => p.IsNewSpecies);
            var duplicateCount = result.Pets.Length - newSpeciesCount;
            Assert(save.PetGacha.Shards(PetGrade.Common) == duplicateCount,
                $"중복분만큼 조각이 되돌아온다, 실제 {save.PetGacha.Shards(PetGrade.Common)} vs {duplicateCount}");
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Common) == newSpeciesCount, "새 종만큼 도감이 늘어난다");
            foreach (var p in result.Pets) Assert(p.Grade == PetGrade.Common, "합성 결과 등급은 항상 요청한 등급");
        });

        Test("A-17 PetFusionController.FuseSameGrade: 같은 seed면 같은 결과(재현 가능)", () =>
        {
            var a = new SaveData();
            var b = new SaveData();
            a.PetGacha.AddShards(PetGrade.Rare, 6);
            b.PetGacha.AddShards(PetGrade.Rare, 6);
            var resultA = PetFusionController.FuseSameGrade(a, PetGrade.Rare, seed: 777);
            var resultB = PetFusionController.FuseSameGrade(b, PetGrade.Rare, seed: 777);
            Assert(resultA.Pets.Length == resultB.Pets.Length, "펫 수부터 같아야 한다");
            for (var i = 0; i < resultA.Pets.Length; i++)
                Assert(resultA.Pets[i].SpeciesId == resultB.Pets[i].SpeciesId, $"{i}번째 결과 종이 같아야 한다");
        });

        Test("A-17 PetFusionController.FusePromotion: 조각을 소모해 위 등급 조각으로 쌓는다(펫으로 직접 바뀌지 않는다)", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Common, 12); // Common(1~4등급) 승급 비용 5 → 2번, 나머지 2
            var result = PetFusionController.FusePromotion(save, PetGrade.Common);
            Assert(result.Promotions == 2, $"12 / 5 = 2번, 실제 {result.Promotions}");
            Assert(result.RemainingFragments == 2, "나머지 2");
            Assert(save.PetGacha.Shards(PetGrade.Common) == 2, "Common 조각은 나머지만 남는다");
            Assert(save.PetGacha.Shards(PetGrade.Advanced) == 2, "승급 2번 = 위 등급(Advanced) 조각 2개, 펫이 아니다");
            Assert(save.PetGacha.OwnedSpeciesCount(PetGrade.Advanced) == 0, "승급만으로는 도감이 늘지 않는다");
        });

        Test("A-17 PetFusionController.FusePromotion: 조각이 비용 미만이면 승급 0, 조각은 그대로", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Legendary, PetFusion.PromotionCost(PetGrade.Legendary) - 1);
            var result = PetFusionController.FusePromotion(save, PetGrade.Legendary);
            Assert(result.Promotions == 0, "비용 미만이면 승급 0");
            Assert(save.PetGacha.Shards(PetGrade.Mythic) == 0, "위 등급 조각도 안 생긴다");
        });

        Test("A-17 PetFusionController.FusePromotion: Transcendent(7등급)는 더 위가 없어 예외", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Transcendent, 100);
            try
            {
                PetFusionController.FusePromotion(save, PetGrade.Transcendent);
                Assert(false, "예외가 나야 한다");
            }
            catch (ArgumentException) { /* 기대한 예외 */ }
        });

        // 경계값 보강 (2026-09-22 밤 세션) — 위 다섯 개는 "정상 케이스 하나씩"만 봤다.
        // 조각 0개, 기존 조각과의 합산, 등급 끝(Mythic→Transcendent 승급, Transcendent에서
        // FuseSameGrade)처럼 코드 경로는 있지만 아직 테스트가 없던 자리를 채운다.
        Test("A-17 PetFusionController.FuseSameGrade: 조각이 정확히 0개면 아무 일도 안 일어난다", () =>
        {
            var save = new SaveData();
            var result = PetFusionController.FuseSameGrade(save, PetGrade.Epic, seed: 5);
            Assert(result.Pets.Length == 0, "조각 0개면 펫 0마리");
            Assert(result.RemainingFragments == 0, "나머지도 0");
            Assert(save.PetGacha.Shards(PetGrade.Epic) == 0, "세이브도 0 그대로");
        });

        Test("A-17 PetFusionController.FuseSameGrade: 최고 등급(Transcendent)도 위 등급 없이 그대로 합성된다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Transcendent, PetFusion.SameGradeFragmentCost);
            var result = PetFusionController.FuseSameGrade(save, PetGrade.Transcendent, seed: 9);
            Assert(result.Pets.Length == 1, "조각 3개 = 1마리, 등급이 끝이어도 같은 등급 합성은 가능해야 한다");
            foreach (var p in result.Pets) Assert(p.Grade == PetGrade.Transcendent, "결과 등급도 Transcendent");
        });

        Test("A-17 PetFusionController.FusePromotion: 위 등급에 이미 조각이 있으면 새로 쌓이는 게 아니라 더해진다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Advanced, 7); // 승급 결과가 쌓일 자리에 미리 조각을 넣어 둔다
            save.PetGacha.AddShards(PetGrade.Common, PetFusion.PromotionCost(PetGrade.Common)); // 승급 1번
            var result = PetFusionController.FusePromotion(save, PetGrade.Common);
            Assert(result.Promotions == 1, "정확히 비용만큼이면 1번");
            Assert(save.PetGacha.Shards(PetGrade.Advanced) == 7 + 1,
                $"기존 7개에 승급으로 생긴 1개가 더해져야 한다(덮어쓰기 아님), 실제 {save.PetGacha.Shards(PetGrade.Advanced)}");
        });

        Test("A-17 PetFusionController.FusePromotion: 등급 사다리 맨 위(Mythic→Transcendent)도 같은 규칙으로 동작한다", () =>
        {
            var save = new SaveData();
            save.PetGacha.AddShards(PetGrade.Mythic, PetFusion.PromotionCost(PetGrade.Mythic) * 2); // 정확히 2번
            var result = PetFusionController.FusePromotion(save, PetGrade.Mythic);
            Assert(result.Promotions == 2, $"실제 {result.Promotions}");
            Assert(result.RemainingFragments == 0, "나누어떨어지므로 나머지 0");
            Assert(save.PetGacha.Shards(PetGrade.Transcendent) == 2, "Mythic 승급 결과는 Transcendent 조각으로 쌓인다");
        });

        // P-14 ②: 종 ID 목록(PetSpeciesTable). 이름·아트 없이 id+등급+계열만으로 먼저 만든 것.
        Test("PetSpeciesTable: 전체 종 수가 124이고 id가 0부터 빈틈 없이 이어진다", () =>
        {
            var all = PetSpeciesTable.All;
            Assert(all.Count == PetGradeInfo.TotalSpeciesCount, $"124종이어야 하는데 {all.Count}");
            for (var i = 0; i < all.Count; i++)
                Assert(all[i].Id == i, $"id는 순서대로여야 한다, {i}번째 요소의 Id={all[i].Id}");
        });

        Test("PetSpeciesTable: 등급별 종 수가 PetGradeInfo.SpeciesCount와 정확히 같다", () =>
        {
            foreach (PetGrade grade in Enum.GetValues(typeof(PetGrade)))
            {
                var ids = PetSpeciesTable.InGrade(grade);
                Assert(ids.Length == PetGradeInfo.SpeciesCountFor(grade),
                    $"{PetGradeInfo.NameKoFor(grade)}은 {PetGradeInfo.SpeciesCountFor(grade)}종이어야 하는데 {ids.Length}");
            }
        });

        Test("PetSpeciesTable: 1~4등급은 계열 4개 × 색 4개, 5등급은 계열 4개 × 색 5개(+주사)다", () =>
        {
            for (var g = 0; g <= 3; g++)
            {
                var grade = (PetGrade)g;
                foreach (var family in PetSpeciesTable.FamilyOrder)
                {
                    var colors = new HashSet<string>();
                    foreach (var id in PetSpeciesTable.InGrade(grade))
                    {
                        var def = PetSpeciesTable.Get(id);
                        if (def.Family == family) colors.Add(def.PlanetId);
                    }
                    Assert(colors.Count == 4, $"{PetGradeInfo.NameKoFor(grade)} {family}은 4색이어야 하는데 {colors.Count}");
                }
            }
            foreach (var family in PetSpeciesTable.FamilyOrder)
            {
                var colors = new HashSet<string>();
                foreach (var id in PetSpeciesTable.InGrade(PetGrade.Legendary))
                {
                    var def = PetSpeciesTable.Get(id);
                    if (def.Family == family) colors.Add(def.PlanetId);
                }
                Assert(colors.Count == 5, $"전설 {family}은 5색이어야 하는데 {colors.Count}");
                Assert(colors.Contains("cinnabar"), "전설 등급에는 주사(cinnabar) 색이 있어야 한다");
            }
        });

        Test("PetSpeciesTable: 6등급(신화)은 계열별 8·8·7·7이고 PlanetId가 없다(낱개 디자인)", () =>
        {
            var expected = new[] { 8, 8, 7, 7 };
            for (var f = 0; f < PetSpeciesTable.FamilyOrder.Length; f++)
            {
                var family = PetSpeciesTable.FamilyOrder[f];
                var count = 0;
                foreach (var id in PetSpeciesTable.InGrade(PetGrade.Mythic))
                {
                    var def = PetSpeciesTable.Get(id);
                    if (def.Family == family)
                    {
                        count++;
                        Assert(def.PlanetId == null, "신화는 PlanetId가 없어야 한다(낱개 디자인)");
                    }
                }
                Assert(count == expected[f], $"신화 {family}은 {expected[f]}종이어야 하는데 {count}");
            }
        });

        Test("PetSpeciesTable: 7등급(초월)은 10종이고 PlanetId가 없다", () =>
        {
            var ids = PetSpeciesTable.InGrade(PetGrade.Transcendent);
            Assert(ids.Length == 10, $"초월은 10종이어야 하는데 {ids.Length}");
            foreach (var id in ids)
                Assert(PetSpeciesTable.Get(id).PlanetId == null, "초월은 PlanetId가 없어야 한다(낱개 디자인)");
        });

        Test("PetSpeciesTable.Get: 범위 밖 id는 예외", () =>
        {
            var threw = false;
            try { PetSpeciesTable.Get(-1); } catch (ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "음수 id는 예외여야 한다");
            threw = false;
            try { PetSpeciesTable.Get(PetGradeInfo.TotalSpeciesCount); } catch (ArgumentOutOfRangeException) { threw = true; }
            Assert(threw, "124 이상 id는 예외여야 한다");
        });

        Test("PetSpeciesTable.PickInGrade: 같은 seed면 같은 종, 결과는 항상 그 등급 소속이다", () =>
        {
            var a = PetSpeciesTable.PickInGrade(PetGrade.Rare, seed: 777);
            var b = PetSpeciesTable.PickInGrade(PetGrade.Rare, seed: 777);
            Assert(a.Id == b.Id, "같은 seed는 같은 종을 뽑아야 한다(서버 재검증용 재현성)");
            Assert(a.Grade == PetGrade.Rare, "뽑은 종은 요청한 등급 소속이어야 한다");
        });

        Test("PetSpeciesTable.PickInGrade: seed를 여러 번 바꾸면 그 등급 종 전부가 나올 수 있다", () =>
        {
            var seen = new HashSet<int>();
            for (var seed = 0; seed < 2000; seed++)
                seen.Add(PetSpeciesTable.PickInGrade(PetGrade.Common, seed).Id);
            Assert(seen.Count == PetGradeInfo.SpeciesCountFor(PetGrade.Common),
                $"일반 {PetGradeInfo.SpeciesCountFor(PetGrade.Common)}종이 2000번 안에 전부 나와야 하는데 {seen.Count}종만 나왔다");
        });

        Test("PetSpeciesTable.MechanicalDisplayNameKo: 1~5등급은 이름이 나오고, 6·7등급은 예외(이름 미정)", () =>
        {
            var quartzWheel = PetSpeciesTable.Get(0);
            Assert(quartzWheel.Family == PetFamily.Wheel && quartzWheel.PlanetId == "quartz", "0번째는 쿼츠 바퀴족이어야 한다");
            Assert(PetSpeciesTable.MechanicalDisplayNameKo(quartzWheel) == "쿼츠 바퀴족",
                $"실제 이름: {PetSpeciesTable.MechanicalDisplayNameKo(quartzWheel)}");

            var mythic = PetSpeciesTable.Get(PetSpeciesTable.InGrade(PetGrade.Mythic)[0]);
            var threw = false;
            try { PetSpeciesTable.MechanicalDisplayNameKo(mythic); } catch (ArgumentException) { threw = true; }
            Assert(threw, "신화는 이름이 아직 없어 예외를 던져야 한다");
        });

        // A-17: 화면이 6·7등급에서도 죽지 않게 하는 자리 표시자 이름. 결과 화면·도감이
        // 예외 없이 항상 부를 수 있어야 한다는 게 이 함수가 있는 이유라 "예외를 안 던진다"가
        // 제일 중요한 확인이고, 그 다음이 값의 모양이다.
        Test("PetSpeciesTable.DisplayNameKo: 전체 124종 어디서도 예외를 던지지 않는다", () =>
        {
            foreach (var def in PetSpeciesTable.All)
            {
                var name = PetSpeciesTable.DisplayNameKo(def);
                Assert(!string.IsNullOrEmpty(name), $"종 id {def.Id}의 표시 이름이 비어 있으면 안 된다");
            }
        });

        Test("PetSpeciesTable.DisplayNameKo: 1~5등급은 MechanicalDisplayNameKo와 정확히 같다", () =>
        {
            var quartzWheel = PetSpeciesTable.Get(0);
            Assert(PetSpeciesTable.DisplayNameKo(quartzWheel) == PetSpeciesTable.MechanicalDisplayNameKo(quartzWheel),
                "1~5등급은 두 함수가 같은 값을 돌려줘야 한다");
        });

        Test("PetSpeciesTable.DisplayNameKo: 6등급은 계열별로 순번이 1부터 다시 시작하고 안 겹친다", () =>
        {
            var seen = new HashSet<string>();
            foreach (var family in PetSpeciesTable.FamilyOrder)
            {
                var index = 1;
                foreach (var id in PetSpeciesTable.InGrade(PetGrade.Mythic))
                {
                    var def = PetSpeciesTable.Get(id);
                    if (def.Family != family) continue;
                    var expected = $"{PetSpeciesTable.FamilyNameKo[Array.IndexOf(PetSpeciesTable.FamilyOrder, family)]} 신화 #{index:D2}";
                    Assert(PetSpeciesTable.DisplayNameKo(def) == expected,
                        $"실제: {PetSpeciesTable.DisplayNameKo(def)} != {expected}");
                    Assert(seen.Add(expected), $"이름이 겹치면 안 된다: {expected}");
                    index++;
                }
            }
        });

        Test("PetSpeciesTable.DisplayNameKo: 7등급은 TranscendentAxisKo 순서와 그대로 대응한다", () =>
        {
            var ids = PetSpeciesTable.InGrade(PetGrade.Transcendent);
            for (var i = 0; i < ids.Length; i++)
            {
                var expected = $"초월 · {PetSpeciesTable.TranscendentAxisKo[i]}";
                Assert(PetSpeciesTable.DisplayNameKo(PetSpeciesTable.Get(ids[i])) == expected,
                    $"{i}번째 초월: {PetSpeciesTable.DisplayNameKo(PetSpeciesTable.Get(ids[i]))} != {expected}");
            }
        });

        // A-17 준비: PetArt.ResourcePath. Resources/Art/Pets 실제 파일 목록과 등급별 이름 규칙이
        // 어긋나면 도감·뽑기 결과 화면에서 그림이 안 뜨니, 여기서 실제 디스크 파일과 대조까지 한다.
        Test("PetArt.ResourcePath: 1등급·5등급은 쿼츠도 색 접미사를 붙인다", () =>
        {
            var common = PetSpeciesTable.Get(0); // 0번 = 쿼츠 바퀴족(1등급)
            Assert(common.Grade == PetGrade.Common && common.PlanetId == "quartz", "0번은 쿼츠 바퀴족 1등급이어야 한다");
            Assert(PetArt.ResourcePath(common) == "Art/Pets/1-common/wheel-quartz",
                $"실제: {PetArt.ResourcePath(common)}");

            var legendQuartz = Array.Find(PetSpeciesTable.All.ToArray(),
                d => d.Grade == PetGrade.Legendary && d.Family == PetFamily.Wing && d.PlanetId == "quartz");
            Assert(PetArt.ResourcePath(legendQuartz) == "Art/Pets/5-legend/wing-quartz",
                $"실제: {PetArt.ResourcePath(legendQuartz)}");
        });

        Test("PetArt.ResourcePath: 2~4등급은 쿼츠만 색 접미사가 없다", () =>
        {
            foreach (var grade in new[] { PetGrade.Advanced, PetGrade.Rare, PetGrade.Epic })
            {
                var folder = PetArt.GradeFolder[(int)grade];
                var quartz = Array.Find(PetSpeciesTable.All.ToArray(),
                    d => d.Grade == grade && d.Family == PetFamily.Cargo && d.PlanetId == "quartz");
                Assert(PetArt.ResourcePath(quartz) == $"Art/Pets/{folder}/haul",
                    $"쿼츠는 접미사가 없어야 한다, 실제: {PetArt.ResourcePath(quartz)}");

                var ruby = Array.Find(PetSpeciesTable.All.ToArray(),
                    d => d.Grade == grade && d.Family == PetFamily.Cargo && d.PlanetId == "ruby");
                Assert(PetArt.ResourcePath(ruby) == $"Art/Pets/{folder}/haul-ruby",
                    $"루비는 접미사가 있어야 한다, 실제: {PetArt.ResourcePath(ruby)}");
            }
        });

        Test("PetArt.ResourcePath: 6등급(신화)은 계열 안 순서대로 01부터 두 자리 번호를 매긴다", () =>
        {
            var wheelIds = Array.FindAll(PetSpeciesTable.InGrade(PetGrade.Mythic),
                id => PetSpeciesTable.Get(id).Family == PetFamily.Wheel);
            for (var i = 0; i < wheelIds.Length; i++)
            {
                var expected = $"Art/Pets/6-myth/wheel-{(i + 1):D2}";
                Assert(PetArt.ResourcePath(PetSpeciesTable.Get(wheelIds[i])) == expected,
                    $"{i}번째 바퀴족 신화: {PetArt.ResourcePath(PetSpeciesTable.Get(wheelIds[i]))} != {expected}");
            }
        });

        Test("PetArt.ResourcePath: 7등급(초월)은 축 순서(TranscendentAxisKo)와 같은 순서로 파일명이 정해진다", () =>
        {
            var ids = PetSpeciesTable.InGrade(PetGrade.Transcendent);
            var expected = new[]
            {
                "drill-sovereign", "refinery-sage", "vault-titan", "comet-racer", "burst-phoenix",
                "fortune-key", "beacon-herald", "dream-keeper", "shard-weaver", "ember-heart",
            };
            Assert(ids.Length == expected.Length, $"초월은 {expected.Length}종이어야 하는데 {ids.Length}");
            for (var i = 0; i < ids.Length; i++)
            {
                var path = PetArt.ResourcePath(PetSpeciesTable.Get(ids[i]));
                Assert(path == $"Art/Pets/7-transcend/{expected[i]}", $"{i}번째 초월: {path} != Art/Pets/7-transcend/{expected[i]}");
            }
        });

        Test("PetArt.ResourcePath: 124종 전부가 실제 Resources/Art/Pets 파일과 맞는다", () =>
        {
            // 2026-09-22 밤: ore-06이 이미지 세션에서 들어와(커밋 d6a2a06) 더 이상 예외가 없다 —
            // 이 테스트가 "누락 0"을 기대하는 순간 이미지 세션이 대기열을 다 채웠다는 뜻이 된다.
            var root = RepoRoot();
            var missing = new List<string>();
            foreach (var def in PetSpeciesTable.All)
            {
                var relative = PetArt.ResourcePath(def).Replace('/', Path.DirectorySeparatorChar);
                var full = Path.Combine(root, "PlanetRacer", "Assets", "Resources", relative + ".png");
                if (!File.Exists(full)) missing.Add(PetArt.ResourcePath(def));
            }
            Assert(missing.Count == 0, $"누락 없어야 하는데 실제로 없는 것: {string.Join(", ", missing)}");
        });

        // A-20: RigArt.TierIndex/ResourcePath. Models.cs:41의 "1~30, 10단계씩 티어" 주석과
        // DefaultData.cs 보물 RequiredToolLevel 경계(1·11·21)가 정확히 이 3구간이라, 경계값을
        // 촘촘히 확인해 둔다 — 여기서 어긋나면 채굴차가 레벨업해도 그림이 안 바뀐다.
        Test("RigArt.TierIndex: 1~10은 0(곡괭이), 11~20은 1(드릴), 21~30은 2(레이저)", () =>
        {
            foreach (var lvl in new[] { 1, 5, 10 })
                Assert(RigArt.TierIndex(lvl) == 0, $"레벨 {lvl}은 0티어여야 하는데 {RigArt.TierIndex(lvl)}");
            foreach (var lvl in new[] { 11, 15, 20 })
                Assert(RigArt.TierIndex(lvl) == 1, $"레벨 {lvl}은 1티어여야 하는데 {RigArt.TierIndex(lvl)}");
            foreach (var lvl in new[] { 21, 25, 30 })
                Assert(RigArt.TierIndex(lvl) == 2, $"레벨 {lvl}은 2티어여야 하는데 {RigArt.TierIndex(lvl)}");
        });

        Test("RigArt.TierIndex: 범위 밖 값(0 이하·30 초과)은 양 끝으로 자른다", () =>
        {
            Assert(RigArt.TierIndex(0) == 0, "0은 최소값 1과 같은 0티어여야 한다");
            Assert(RigArt.TierIndex(-5) == 0, "음수도 0티어여야 한다");
            Assert(RigArt.TierIndex(31) == 2, "31은 최대값 30과 같은 2티어여야 한다");
            Assert(RigArt.TierIndex(999) == 2, "아주 큰 값도 2티어여야 한다");
        });

        Test("RigArt.ResourcePath: 티어 인덱스에 맞는 rig-tiers-sheet 조각 이름을 돌려준다", () =>
        {
            Assert(RigArt.ResourcePath(1) == "Art/Rigs/rig-tier-pickaxe", $"실제: {RigArt.ResourcePath(1)}");
            Assert(RigArt.ResourcePath(11) == "Art/Rigs/rig-tier-drill", $"실제: {RigArt.ResourcePath(11)}");
            Assert(RigArt.ResourcePath(21) == "Art/Rigs/rig-tier-laser", $"실제: {RigArt.ResourcePath(21)}");
        });

        Test("RigArt.ResourcePath: 세 파일이 실제로 Resources/Art/Rigs에 있다", () =>
        {
            var root = RepoRoot();
            foreach (var lvl in new[] { 1, 11, 21 })
            {
                var relative = RigArt.ResourcePath(lvl).Replace('/', Path.DirectorySeparatorChar);
                var full = Path.Combine(root, "PlanetRacer", "Assets", "Resources", relative + ".png");
                Assert(File.Exists(full), $"파일이 없다: {full}");
            }
        });

        Console.WriteLine();
        Console.WriteLine($"통과 {_pass} / 실패 {_fail}");
        return _fail == 0 ? 0 : 1;
    }

    static void Test(string name, Action body)
    {
        try { body(); _pass++; Console.WriteLine($"  ok   {name}"); }
        catch (Exception e) { _fail++; Console.WriteLine($"  FAIL {name}\n       {e.Message}"); }
    }

    static void Assert(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
    }

    /// <summary>Core.Tests를 어디서 실행하든(프로젝트 폴더든 bin 폴더든) 저장소 루트를 찾아
    /// docs/design/balance/ 경로를 만든다. "docs" 폴더가 나올 때까지 위로 올라간다.</summary>
    static string BalancePath(string fileName)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "docs")))
            dir = dir.Parent;
        if (dir == null) throw new Exception("저장소 루트(docs 폴더)를 못 찾음");
        return Path.Combine(dir.FullName, "docs", "design", "balance", fileName);
    }

    /// <summary>BalancePath와 같은 방식으로 저장소 루트("docs" 폴더가 있는 곳)를 찾는다 —
    /// PetArt 테스트가 Assets/Resources/Art/Pets 실제 파일과 대조할 때 쓴다.</summary>
    static string RepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "docs")))
            dir = dir.Parent;
        if (dir == null) throw new Exception("저장소 루트(docs 폴더)를 못 찾음");
        return dir.FullName;
    }

    static void AssertNear(float expected, float actual, string label) =>
        Assert(Math.Abs(expected - actual) < 0.001f, $"{label} {actual} == {expected}");

    /// <summary>D06-M: SurfaceMover.SetOrbitAngle이 쓰는 Quaternion.AngleAxis 회전(로드리게스 회전 공식)을
    /// UnityEngine 없이 그대로 재현한 것. 축·시작 벡터는 정규화하지 않고 넘겨도 된다(내부에서 정규화).</summary>
    static (double x, double y, double z) RotateAroundAxis(
        (double x, double y, double z) v, (double x, double y, double z) axis, double angleDeg)
    {
        var axisLen = Math.Sqrt(axis.x * axis.x + axis.y * axis.y + axis.z * axis.z);
        var (ax, ay, az) = (axis.x / axisLen, axis.y / axisLen, axis.z / axisLen);
        var rad = angleDeg * Math.PI / 180.0;
        var (cos, sin) = (Math.Cos(rad), Math.Sin(rad));

        // v*cos + (axis x v)*sin + axis*(axis·v)*(1-cos)
        var dot = ax * v.x + ay * v.y + az * v.z;
        var (cx, cy, cz) = (ay * v.z - az * v.y, az * v.x - ax * v.z, ax * v.y - ay * v.x);
        return (
            v.x * cos + cx * sin + ax * dot * (1 - cos),
            v.y * cos + cy * sin + ay * dot * (1 - cos),
            v.z * cos + cz * sin + az * dot * (1 - cos));
    }

    /// <summary>D06-M: planet.VeinCount개 광맥의 3D 위치(중심 원점, 반지름 radius)를 orbitAxis 둘레로
    /// startDirection을 회전시켜 구하고, 이웃한 광맥끼리의 현 거리(chord distance)가 전부 같은지 확인한다.
    /// 하나라도 달라지면(허용오차 1e-6 초과) 극점 근처에서 간격이 벌어지거나 좁아졌다는 뜻이다.</summary>
    static void AssertUniformVeinSpacing(
        Planet planet, (double x, double y, double z) orbitAxis, (double x, double y, double z) startDirection, double radius)
    {
        var count = Math.Max(1, planet.VeinCount);
        var positions = new (double x, double y, double z)[count];
        for (var i = 0; i < count; i++)
        {
            var angle = VeinLayout.VeinAngleDegrees(planet, i);
            var dir = RotateAroundAxis(startDirection, orbitAxis, angle);
            positions[i] = (dir.x * radius, dir.y * radius, dir.z * radius);
        }

        double ChordDistance((double x, double y, double z) a, (double x, double y, double z) b) =>
            Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2) + Math.Pow(a.z - b.z, 2));

        double referenceDistance = count > 1 ? ChordDistance(positions[0], positions[1]) : 0;

        for (var i = 0; i < count; i++)
        {
            var next = positions[(i + 1) % count];
            var d = ChordDistance(positions[i], next);
            // VeinLayout.VeinAngleDegrees는 float(single) 정밀도라 각도 자체에 ~1e-6대 잡음이 있다 —
            // 그게 그대로 거리 계산까지 전해진 것은 실제 기하 왜곡이 아니라 부동소수 오차다.
            // 극점 근처에서 진짜로 간격이 벌어지면 이 정도가 아니라 %대로 어긋나므로 1e-4면 충분히 엄격하다.
            Assert(Math.Abs(d - referenceDistance) < 1e-4,
                $"{i}번-{(i + 1) % count}번 광맥 간 거리 {d:F6} == 기준 거리 {referenceDistance:F6}(광맥 {i}개째 안 맞음)");
        }
    }

    static void AssertPlanetEquals(Planet e, Planet a)
    {
        Assert(e.Id == a.Id, $"id {a.Id} == {e.Id}");
        Assert(e.NameKo == a.NameKo, $"nameKo {a.NameKo} == {e.NameKo}");
        Assert(e.Order == a.Order, $"order {a.Order} == {e.Order}");
        AssertNear(e.Circumference, a.Circumference, "circumference");
        AssertNear(e.Heat, a.Heat, "heat");
        AssertNear(e.Cold, a.Cold, "cold");
        AssertNear(e.Roughness, a.Roughness, "roughness");
        AssertNear(e.Liquid, a.Liquid, "liquid");
        AssertNear(e.Toxic, a.Toxic, "toxic");
        AssertNear(e.Gravity, a.Gravity, "gravity");
        AssertNear(e.Atmosphere, a.Atmosphere, "atmosphere");
        Assert(e.VeinCount == a.VeinCount, $"veinCount {a.VeinCount} == {e.VeinCount}");
        AssertNear(e.VeinYield, a.VeinYield, "veinYield");
    }

    static void AssertCourseEquals(Course e, Course a)
    {
        Assert(e.Id == a.Id, $"course id {a.Id} == {e.Id}");
        Assert(e.NameKo == a.NameKo, $"course nameKo {a.NameKo} == {e.NameKo}");
        Assert(e.PlanetId == a.PlanetId, $"course planetId {a.PlanetId} == {e.PlanetId}");
        AssertNear(e.Length, a.Length, "length");
        Assert(e.Laps == a.Laps, $"laps {a.Laps} == {e.Laps}");
        AssertNear(e.FlatRatio, a.FlatRatio, "flatRatio");
        AssertNear(e.RoughRatio, a.RoughRatio, "roughRatio");
        AssertNear(e.BoostRatio, a.BoostRatio, "boostRatio");
    }

    static void AssertPartEquals(Part e, Part a)
    {
        Assert(e.Id == a.Id, $"part id {a.Id} == {e.Id}");
        Assert(e.NameKo == a.NameKo, $"part nameKo {a.NameKo} == {e.NameKo}");
        Assert(e.Slot == a.Slot, $"slot {a.Slot} == {e.Slot}");
        Assert(e.Grade == a.Grade, $"grade {a.Grade} == {e.Grade}");
        Assert(e.PlanetId == a.PlanetId, $"part planetId {a.PlanetId} == {e.PlanetId}");
        AssertNear(e.Base.Power, a.Base.Power, "power");
        AssertNear(e.Base.Grip, a.Base.Grip, "grip");
        AssertNear(e.Base.Suspension, a.Base.Suspension, "suspension");
        AssertNear(e.Base.Durability, a.Base.Durability, "durability");
        AssertNear(e.Base.Boost, a.Base.Boost, "boost");
        AssertNear(e.Base.Aero, a.Base.Aero, "aero");
        AssertNear(e.Base.HeatResist, a.Base.HeatResist, "heatResist");
        AssertNear(e.Base.Seal, a.Base.Seal, "seal");
        AssertNear(e.Base.Filter, a.Base.Filter, "filter");
    }
}
