using System;
using System.Collections.Generic;
using GemRacer.Core;

// P-07 여러 세션(2026-09-20~21)이 남긴 "첫 값(P4 봇 시뮬레이션에서 재조정 여지 있음)"이라는
// 각주가 QuartzAdvancedParts/QuartzEpicParts/QuartzLegendaryParts 세 군데에 그대로 남아 있었다
// — 실제로 그 시뮬레이션이 한 번도 안 돌아갔다는 뜻이다. BalanceSim.cs(L-05)는 채굴차 업그레이드
// 속도만 보고 레이싱카 부품 등급(C/B/A/S)은 안 다룬다. 이 파일이 그 빈자리를 채운다.
//
// 보고 싶은 것: 등급이 오를수록(스탯 배수 1→2→4→8, 제작 비용 배수 1→4→16→64) 실제 레이스
// 완주 시간이 어떻게 줄어드는가. 스탯은 배수로 늘지만 LapTime 공식은 속도에 선형으로 더해질 뿐
// 이동식 자체는 "거리/속도"라 시간 감소분은 갈수록 완만해지는 게 정상이다(수확 체감) —
// 그런데 비용은 매 등급 ×4씩 뛰므로, "비용 대비 효율"이 등급이 오를수록 나빠지는 것 자체는
// 의도된 설계(C.f. RigUpgrade의 지수 비용과 같은 취지)다. 이 리포트가 실제로 보려는 건
// 그 나빠지는 정도가 상식적인 범위인지(예: S가 C보다 8배 비싼데 시간은 1%만 준다면 과하게
// 낭비적인 등급)와, 등급 사이에 "타는 게 무의미할 만큼" 이가 빠진 구간이 있는지다.
//
// `dotnet run -- sim-parts`로만 돈다 — sim(L-05)과 같은 이유로 평소 `dotnet run`(테스트)에는 안 낀다.
static class BalanceSimParts
{
    public static void Run()
    {
        var quartz = DefaultData.Planets()[0];
        var courses = DefaultData.QuartzCourses();

        Console.WriteLine("=== P-07 레이싱카 부품 등급 밸런스 시뮬레이션 (쿼츠, C/B/A/S) ===");
        Console.WriteLine("맨몸(부품 0개) 대비, 그 등급 5부품(엔진·타이어·서스펜션·차체·부스터)을 전부 장착했을 때.");
        Console.WriteLine();

        var bare = new RacingCar();
        var bareStats = bare.TotalStats();

        var grades = new (PartGrade grade, List<Part> parts, string costDesc, float questzEquivCost)[]
        {
            (PartGrade.C, DefaultData.QuartzStarterParts(),   $"정제 광물 {DefaultData.PartCostC * 5:F0}",  DefaultData.PartCostC * 5f),
            (PartGrade.B, DefaultData.QuartzAdvancedParts(),  $"정제 광물 {DefaultData.PartCostB * 5:F0}",  DefaultData.PartCostB * 5f),
            (PartGrade.A, DefaultData.QuartzEpicParts(),      RecipeDesc(DefaultData.QuartzEpicRecipe(), 5), RecipeQuartzEquiv(DefaultData.QuartzEpicRecipe(), 5)),
            (PartGrade.S, DefaultData.QuartzLegendaryParts(), RecipeDesc(DefaultData.QuartzLegendaryRecipe(), 5), RecipeQuartzEquiv(DefaultData.QuartzLegendaryRecipe(), 5)),
        };

        // 코스별 맨몸 시간(기준점).
        var bareTimes = new Dictionary<string, float>();
        foreach (var course in courses)
            bareTimes[course.Id] = RaceSimulator.LapTime(bareStats, quartz, course);

        float? prevAvgTime = null;
        float prevCost = 0f;
        foreach (var (grade, parts, costDesc, questzEquiv) in grades)
        {
            var car = new RacingCar();
            foreach (var part in parts) car.Slots[part.Slot] = part;
            var stats = car.TotalStats();

            var times = new List<float>();
            Console.WriteLine($"[{grade}] 제작 비용: {costDesc} (쿼츠 환산 대략 {questzEquiv:F0})");
            foreach (var course in courses)
            {
                var t = RaceSimulator.LapTime(stats, quartz, course);
                var bare0 = bareTimes[course.Id];
                var reduction = (bare0 - t) / bare0 * 100f;
                times.Add(t);
                Console.WriteLine($"    {course.NameKo,-14} 맨몸 {bare0,6:F1}초 → {t,6:F1}초  ({reduction,5:F1}% 단축)");
            }

            var avgTime = Average(times);
            if (prevAvgTime != null)
            {
                var timeDropPct = (prevAvgTime.Value - avgTime) / prevAvgTime.Value * 100f;
                var costMultiplier = prevCost > 0 ? questzEquiv / prevCost : 0f;
                Console.WriteLine($"    → 직전 등급 대비: 평균 시간 {timeDropPct,5:F1}% 추가 단축, 비용은 {costMultiplier,4:F1}배");
            }
            prevAvgTime = avgTime;
            prevCost = questzEquiv;
            Console.WriteLine();
        }

        Console.WriteLine("(비용은 매 등급 ×4씩 뛰는데 시간 단축 폭은 등급이 오를수록 줄어드는 게 정상 —");
        Console.WriteLine(" LapTime이 속도의 역수라 스탯을 배로 늘려도 시간은 선형으로 안 줄기 때문이다.");
        Console.WriteLine(" 아래 실제 수치로 그 감소 폭이 상식적인지 판단할 것.)");
    }

    static string RecipeDesc(List<MineralCost> recipe, int multiplier)
    {
        var parts = new List<string>();
        foreach (var c in recipe) parts.Add($"{c.PlanetId} {c.Amount * multiplier:F0}");
        return string.Join(" + ", parts);
    }

    /// <summary>서로 다른 행성 광물을 쿼츠 하나의 "환산치"로 묶어 등급 간 비용을 한 숫자로 비교하기
    /// 위한 것 — 정확한 교환비가 정해진 게 없어서(P-07 결정 대기 항목이 아니라 아직 아무도 안 물은
    /// 질문) 1:1로 그냥 더한다. 정확한 가치가 아니라 "총 투입량이 등급마다 몇 배씩 느는지" 큰
    /// 그림만 보려는 용도라 이 단순화로 충분하다.</summary>
    static float RecipeQuartzEquiv(List<MineralCost> recipe, int multiplier)
    {
        var total = 0f;
        foreach (var c in recipe) total += c.Amount * multiplier;
        return total;
    }

    static float Average(List<float> values)
    {
        var sum = 0f;
        foreach (var v in values) sum += v;
        return sum / values.Count;
    }
}
