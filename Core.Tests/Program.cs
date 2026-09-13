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
            var full = new MiningRig { CargoLevel = 10 };
            Assert(Math.Abs(MiningSimulator.CargoHours(full) - 12f) < 0.001f, "10레벨 = 12시간");
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

        Test("난수: xorshift는 플랫폼 무관하게 같은 수열", () =>
        {
            var a = new DeterministicRandom(42); var b = new DeterministicRandom(42);
            for (var i = 0; i < 100; i++) Assert(a.NextUInt() == b.NextUInt(), "수열 동일");
            var z = new DeterministicRandom(0);
            Assert(z.NextUInt() != 0, "seed 0도 동작");
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
            var maxedRig = new MiningRig { CargoLevel = 10 };
            var cargoReward = DefaultData.QuartzLocalRaceRewards()[1]; // Cargo
            var after = RigPartApply.Apply(maxedRig, cargoReward);
            Assert(after.CargoLevel == 10, $"화물칸 10레벨에서 보상을 받아도 그대로 {after.CargoLevel}");
        });

        Test("레이스 보상: 쿼츠 로컬 레이스 3개가 서로 다른 슬롯을 준다", () =>
        {
            var rewards = DefaultData.QuartzLocalRaceRewards();
            Assert(rewards.Count == 3, $"보상 3개 {rewards.Count}");
            var slots = new HashSet<RigSlot>();
            foreach (var r in rewards) slots.Add(r.Slot);
            Assert(slots.Count == 3, $"슬롯 3종류 서로 다름 {slots.Count}");
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

        Test("밸런스 CSV: 부품 표가 DefaultData와 일치한다", () =>
        {
            var parsed = BalanceCsv.ParseParts(File.ReadAllText(BalancePath("parts.csv")));
            var expected = DefaultData.QuartzStarterParts();
            Assert(parsed.Count == expected.Count, $"부품 수 {parsed.Count} == {expected.Count}");
            for (var i = 0; i < expected.Count; i++) AssertPartEquals(expected[i], parsed[i]);
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
            Assert(MiningSimulator.CargoHours(cargoAfter) > MiningSimulator.CargoHours(cargoBefore),
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
            AssertNear(MiningSimulator.CargoHours(rig), r.HoursCounted, "인정 시간은 화물칸 상한 그대로");
            Assert(r.HoursWasted > 1000000f, $"버린 시간이 큰 수({r.HoursWasted}h) — 상한을 실제로 넘겼다는 뜻");
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

    static void AssertNear(float expected, float actual, string label) =>
        Assert(Math.Abs(expected - actual) < 0.001f, $"{label} {actual} == {expected}");

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
