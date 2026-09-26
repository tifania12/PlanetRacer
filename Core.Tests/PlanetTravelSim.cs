using System;
using System.Collections.Generic;
using GemRacer.Core;

// P-06(2026-09-26 11시 주말 세션이 MiningController.TravelTo에 배선): 행성을 떠날 때 곡괭이
// 레벨의 40%를 새 행성 시작값으로 물려준다(PlanetToolLevel.CarryOverStartLevel). 그런데 이
// 40%는 planet-progression.md 2절에 "예: 이전 최고 레벨의 40%"로 적힌 예시값일 뿐, 실제로
// 그 배율이 새 행성에서 얼마나 큰 머리 시작을 주는지는 한 번도 숫자로 확인한 적이 없다 —
// 이 리포트가 그 확인이다. 테스트가 아니라 참고 자료라 BalanceSim/BalanceSimParts와 같은 이유로
// `dotnet run -- sim-planet-travel`로만 돈다.
//
// 보고 싶은 것: (1) 떠나기 직전 레벨(30=완주, 15=절반) 기준으로 물려받는 시작 레벨이 몇인지,
// (2) 그 시작 레벨이 새 행성에서 레벨 1 대비 시간당 산출을 몇 배로 만드는지(엔진·화물칸은
// 새 행성에서도 1레벨이라고 가정 — 곡괭이만 물려받으므로 다른 변수를 섞지 않으려는 것),
// (3) 레벨 1부터 그 물려받은 레벨까지 정상적으로 올리려면 정제 광물이 얼마나 드는지(UpgradeCost.Cost
// 누적 — 비용 곡선은 행성과 무관해서 참고용으로만 싣는다, "이만큼을 건너뛴다"는 감을 주는 값).
static class PlanetTravelSim
{
    public static void Run()
    {
        var planets = DefaultData.Planets();
        Console.WriteLine("=== P-06 곡괭이 물려주기(40%) — 새 행성 머리 시작 배율 ===");
        Console.WriteLine("(엔진·화물칸은 양쪽 다 1레벨 가정 — 곡괭이 레벨 하나만의 효과를 보려는 것)");
        Console.WriteLine();

        foreach (var fraction in new[] { PlanetToolLevel.DefaultCarryOverFraction })
        {
            foreach (var departLevel in new[] { 30, 15 })
            {
                var carryLevel = PlanetToolLevel.CarryOverStartLevel(departLevel, fraction);
                Console.WriteLine($"-- 이전 행성 이탈 레벨 {departLevel} → 물려받는 시작 레벨 {carryLevel}" +
                    $"({fraction * 100:F0}%) --");

                for (var i = 1; i < planets.Count; i++)
                {
                    var dest = planets[i];
                    var fresh = new MiningRig { ToolLevel = 1 };
                    var carried = new MiningRig { ToolLevel = carryLevel };
                    var freshRate = MiningSimulator.MineralsPerHour(fresh, dest);
                    var carriedRate = MiningSimulator.MineralsPerHour(carried, dest);
                    var ratio = carriedRate / freshRate;

                    var costToClimb = CumulativeToolCost(1, carryLevel);
                    Console.WriteLine($"  {dest.NameKo,-8} 레벨1 {freshRate,7:F1}/h → 레벨{carryLevel,2} {carriedRate,7:F1}/h" +
                        $"  (×{ratio,5:F2})   레벨1→{carryLevel}을 정상 업그레이드로 올리려면 정제 광물 {costToClimb,8:F0} 필요");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("(×배율이 도착 직후 한 틱만의 산출비다 — 시간이 지나 새 행성에서도 곡괭이를 올리면");
        Console.WriteLine(" 격차는 자연히 줄어든다. 여기서 보려는 건 \"도착 첫 순간 얼마나 유리한가\"뿐이다.)");
    }

    /// <summary>Tool 슬롯을 fromLevel(포함)에서 toLevel(포함, 도달할 때까지)까지 올리는 데 드는
    /// 정제 광물 누적 비용. UpgradeCost.Cost(Tool, rig)는 rig.ToolLevel(현재 레벨)만 보고 다음
    /// 레벨로 가는 비용을 돌려주므로, fromLevel부터 toLevel-1까지 레벨을 하나씩 올려가며 더한다.</summary>
    static float CumulativeToolCost(int fromLevel, int toLevel)
    {
        var total = 0f;
        var rig = new MiningRig { ToolLevel = fromLevel };
        while (rig.ToolLevel < toLevel)
        {
            total += UpgradeCost.Cost(UpgradeSlot.Tool, rig);
            rig = UpgradeCost.Apply(UpgradeSlot.Tool, rig);
        }
        return total;
    }
}
