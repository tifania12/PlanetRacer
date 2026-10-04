using System;
using GemRacer.Core;

// E-06 ③: 연구소 비용·시간 곡선이 설계 기준을 지키는지 숫자로 본다. 테스트가 아니라 참고 자료라
// BalanceSim과 같은 이유로 `dotnet run -- sim-research`로만 돈다.
// 기준(economy-v2.md 5절): 오프라인 저장고 18단계를 약 3~4주에, 첫 주에 6→12시간(6단계)은 닿게.
// 연구는 슬롯 1개라 시간은 전부 직렬이다 — 종류를 섞어도 총 대기 시간은 더하기뿐이다.
// 사람이 접속해 걸어 두는 간격은 모르므로 "쉬지 않고 이어 걸었을 때"의 하한만 낸다.
static class ResearchSim
{
    public static void Run()
    {
        Console.WriteLine("=== E-06 연구소 곡선 — 직렬 대기 시간(슬롯 1)과 누적 돈 ===");
        Console.WriteLine("종류               | 단계 | 누적 돈      | 누적 시간(일)");
        double allMoney = 0, allDays = 0;
        foreach (ResearchKind kind in Enum.GetValues(typeof(ResearchKind)))
        {
            double money = 0, secs = 0;
            for (var l = 0; l < Research.MaxLevel(kind); l++)
            {
                money += Research.CostToNext(kind, l);
                secs += Research.SecondsToNext(kind, l);
            }
            allMoney += money; allDays += secs / 86400.0;
            Console.WriteLine($"{kind,-18} | {Research.MaxLevel(kind),4} | {money,12:N0} | {secs / 86400.0,8:F1}");
        }
        Console.WriteLine($"{"전부",-18} | {"",4} | {allMoney,12:N0} | {allDays,8:F1}");
        Console.WriteLine();

        Console.WriteLine("오프라인 저장고 단계별(첫 주 기준 확인)");
        Console.WriteLine("단계 | 비용        | 시간(h) | 누적 시간(일) | 상한(h)");
        double cum = 0;
        for (var l = 0; l < Research.MaxLevel(ResearchKind.OfflineStorage); l++)
        {
            var s = Research.SecondsToNext(ResearchKind.OfflineStorage, l);
            cum += s;
            Console.WriteLine($"{l + 1,4} | {Research.CostToNext(ResearchKind.OfflineStorage, l),11:N0} | {s / 3600.0,7:F2} | {cum / 86400.0,13:F2} | {Research.OfflineCapHours(6, l + 1),6:F0}");
        }
    }
}
