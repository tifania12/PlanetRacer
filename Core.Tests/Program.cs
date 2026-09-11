using System;
using System.Collections.Generic;
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
}
