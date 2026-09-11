using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GemRacer.Core;

// NuGet 없이 돌아가는 최소 테스트 러너. `dotnet run` 으로 실행. 실패가 하나라도 있으면 종료 코드 1.
static class Program
{
    static int _pass, _fail;

    static int Main()
    {
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
