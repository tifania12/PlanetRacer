using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    public enum ResearchKind
    {
        OfflineStorage,   // 오프라인 저장고: 오프라인 상한 +1시간
        GemDetector,      // 젬 탐지기: 광맥당 젬 확률 +0.2%p
        RefineCatalyst,   // 정제 촉매: 제련소 처리량 +5%
        PrizeNegotiation, // 상금 협상: 레이스 상금 +5%
        StoneAppraisal,   // 강화석 감정: 던전 강화석 +3%
    }

    /// <summary>진행 중인 연구 한 칸. 끝나는 시각을 저장하고 지금 시각은 인자로 받는다
    /// (코어에서 DateTime.Now 금지 — 서버가 같은 코드로 검증한다).</summary>
    [Serializable]
    public class ActiveResearch
    {
        public ResearchKind Kind;
        public double EndAtSeconds;
    }

    /// <summary>E-06(economy-v2.md 5절): 연구소 코어. 화면(강화 화면 안 탭)은 Unity 세션 몫.
    /// 비용 곡선·시간 곡선은 플레이스홀더다 — 설계 문서가 "BalanceSim으로 정한다"고 했고
    /// 돈의 유입(E-09 상금 기본값)이 아직 플레이스홀더라 지금은 비교할 기준이 없다.
    /// 시간은 1단계 10분에서 시작해 단계마다 25%씩 늘고 8시간에서 멈춘다.</summary>
    public static class Research
    {
        public const double MinSeconds = 600.0;
        public const double MaxSeconds = 8.0 * 3600.0;
        public const double TimeGrowth = 1.25;
        public const double BaseCost = 200.0;
        public const double CostGrowth = 1.35;

        public static int MaxLevel(ResearchKind kind) => kind switch
        {
            ResearchKind.OfflineStorage => 18,
            ResearchKind.GemDetector => 20,
            ResearchKind.RefineCatalyst => 40,
            ResearchKind.PrizeNegotiation => 40,
            ResearchKind.StoneAppraisal => 20,
            _ => 0,
        };

        /// <summary>한 단계가 올려 주는 양. 오프라인은 시간(h), 젬은 확률(%p가 아니라 0.002 = 0.2%p),
        /// 나머지는 비율(0.05 = 5%).</summary>
        public static double PerLevel(ResearchKind kind) => kind switch
        {
            ResearchKind.OfflineStorage => 1.0,
            ResearchKind.GemDetector => 0.002,
            ResearchKind.RefineCatalyst => 0.05,
            ResearchKind.PrizeNegotiation => 0.05,
            ResearchKind.StoneAppraisal => 0.03,
            _ => 0.0,
        };

        /// <summary>level 단계를 이미 찍은 상태에서 다음 단계(level+1)로 가는 돈. 최대면 0.</summary>
        public static double CostToNext(ResearchKind kind, int level)
        {
            if (level < 0 || level >= MaxLevel(kind)) return 0.0;
            return Math.Round(BaseCost * Math.Pow(CostGrowth, level));
        }

        public static double SecondsToNext(ResearchKind kind, int level)
        {
            if (level < 0 || level >= MaxLevel(kind)) return 0.0;
            return Math.Min(MaxSeconds, MinSeconds * Math.Pow(TimeGrowth, level));
        }

        /// <summary>레벨이 올려 준 총량(PerLevel × level). 범위 밖 레벨은 잘라서 센다.</summary>
        public static double Total(ResearchKind kind, int level)
        {
            var l = Math.Max(0, Math.Min(level, MaxLevel(kind)));
            return PerLevel(kind) * l;
        }

        /// <summary>E-02의 기본 6시간에 저장고 연구분을 더한 오프라인 상한(시간). 최대 24.</summary>
        public static double OfflineCapHours(double baseHours, int storageLevel) =>
            baseHours + Total(ResearchKind.OfflineStorage, storageLevel);

        /// <summary>곱셈 보너스(정제 촉매·상금 협상·강화석 감정)용 배율. 레벨 0이면 1.</summary>
        public static double Multiplier(ResearchKind kind, int level) => 1.0 + Total(kind, level);
    }

    /// <summary>세이브에 들어가는 연구소 상태. 레벨은 종류 순서(enum 값)대로 리스트에 둔다
    /// (JsonUtility가 Dictionary를 못 다뤄서 다른 세이브 필드와 같은 병렬 리스트 패턴).</summary>
    [Serializable]
    public class ResearchState
    {
        public List<int> Levels = new List<int>();
        public List<ActiveResearch> Active = new List<ActiveResearch>();

        public int GetLevel(ResearchKind kind)
        {
            var i = (int)kind;
            return i >= 0 && i < Levels.Count ? Levels[i] : 0;
        }

        void SetLevel(ResearchKind kind, int value)
        {
            var i = (int)kind;
            while (Levels.Count <= i) Levels.Add(0);
            Levels[i] = value;
        }

        public bool IsRunning(ResearchKind kind)
        {
            foreach (var a in Active) if (a.Kind == kind) return true;
            return false;
        }

        /// <summary>연구를 시작한다. 돈은 시작할 때 낸다. slots는 Entitlements의 연구 슬롯 수(E-11, 기본 1).
        /// 실패하면 상태·돈이 그대로다.</summary>
        public bool TryStart(ResearchKind kind, double nowSeconds, ref double money, int slots = 1)
        {
            var level = GetLevel(kind);
            if (level >= Research.MaxLevel(kind)) return false;
            if (Active.Count >= Math.Max(1, slots)) return false;
            if (IsRunning(kind)) return false;
            var cost = Research.CostToNext(kind, level);
            if (money < cost) return false;
            money -= cost;
            Active.Add(new ActiveResearch { Kind = kind, EndAtSeconds = nowSeconds + Research.SecondsToNext(kind, level) });
            return true;
        }

        /// <summary>끝난 연구를 반영하고 몇 개가 끝났는지 돌려준다. 오프라인 중에 끝난 것도 한꺼번에 처리된다.
        /// 시계가 뒤로 가도(nowSeconds가 작아져도) 끝난 연구는 되돌아가지 않고 아직 안 끝난 것만 남는다.</summary>
        public int Collect(double nowSeconds)
        {
            var done = 0;
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                if (Active[i].EndAtSeconds > nowSeconds) continue;
                var k = Active[i].Kind;
                SetLevel(k, Math.Min(Research.MaxLevel(k), GetLevel(k) + 1));
                Active.RemoveAt(i);
                done++;
            }
            return done;
        }

        /// <summary>상금 협상 연구분 배율. RacePrize.Compute의 bonusMultiplier에 그대로 넘긴다.</summary>
        public double PrizeMultiplier() => Research.Multiplier(ResearchKind.PrizeNegotiation, GetLevel(ResearchKind.PrizeNegotiation));

        /// <summary>젬 탐지기 연구분을 GemDrop의 chanceMultiplier로 바꾼 값.
        /// 기본 확률(2%)에 레벨당 +0.2%p를 더한 확률 ÷ 기본 확률이라 레벨당 +0.1, 최대 20단계에서 3배.</summary>
        public float GemChanceMultiplier() =>
            (float)((GemDrop.BaseChancePerVein + Research.Total(ResearchKind.GemDetector, GetLevel(ResearchKind.GemDetector)))
                    / GemDrop.BaseChancePerVein);

        /// <summary>정제 촉매 연구분 배율. MiningSimulator.Refine의 throughputMultiplier에 넘긴다.</summary>
        public double RefineMultiplier() => Research.Multiplier(ResearchKind.RefineCatalyst, GetLevel(ResearchKind.RefineCatalyst));

        /// <summary>강화석 감정 연구분 배율(던전 강화석 수량에 곱한다. 던전은 E-08 몫).</summary>
        public double StoneMultiplier() => Research.Multiplier(ResearchKind.StoneAppraisal, GetLevel(ResearchKind.StoneAppraisal));

        public double RemainingSeconds(ResearchKind kind, double nowSeconds)
        {
            foreach (var a in Active)
                if (a.Kind == kind) return Math.Max(0.0, a.EndAtSeconds - nowSeconds);
            return 0.0;
        }
    }
}
