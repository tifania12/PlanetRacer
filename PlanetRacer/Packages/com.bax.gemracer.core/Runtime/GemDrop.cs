using System;

namespace GemRacer.Core
{
    /// <summary>E-07(economy-v2.md 4절): 젬은 광맥 하나를 다 캘 때마다 확률로 나온다.
    /// 접속 중에는 광맥마다 실제로 굴리고(seed로 재현 — 서버 검증용), 오프라인은 기대값으로 준다
    /// (오프라인에서 굴리면 운 나쁜 날 0개가 나와 억울하다는 Tifania 결정).
    /// 연구 "젬 탐지기"(E-06)가 붙으면 chanceMultiplier로 들어온다 — 지금은 늘 1.</summary>
    public static class GemDrop
    {
        /// <summary>광맥 하나당 젬 1개가 나올 시작 확률.</summary>
        public const float BaseChancePerVein = 0.02f;

        /// <summary>보물·상자에서 나오는 보조 젬 개수(운이 계속 나쁠 때의 안전판). 상자 하나에 1개.</summary>
        public const int BonusGemsPerTreasure = 1;

        public static float ChancePerVein(float chanceMultiplier = 1f) =>
            Math.Min(1f, BaseChancePerVein * Math.Max(0f, chanceMultiplier));

        /// <summary>광맥 하나를 캤을 때 젬이 나왔으면 1, 아니면 0. 같은 rng 상태면 같은 결과다.</summary>
        public static int RollVein(DeterministicRandom rng, float chanceMultiplier = 1f) =>
            rng.NextFloat() < ChancePerVein(chanceMultiplier) ? 1 : 0;

        /// <summary>시간당 광맥 수 — 이동 시간 + 채굴 체류 시간을 한 사이클로 본다
        /// (MiningSimulator.GemsPerHour의 veinsPerHour와 같은 식).</summary>
        public static float VeinsPerHour(MiningRig rig, Planet planet)
        {
            var speed = MiningSimulator.RigSpeed(rig, planet);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            return 3600f / (travelPerVein / speed + MiningSimulator.SecondsPerVein(rig));
        }

        /// <summary>오프라인 기대값: 인정 시간 × 시간당 광맥 × 확률.</summary>
        public static double ExpectedOffline(MiningRig rig, Planet planet, double countedHours, float chanceMultiplier = 1f) =>
            Math.Max(0.0, countedHours) * VeinsPerHour(rig, planet) * ChancePerVein(chanceMultiplier);

        /// <summary>일반 펫 뽑기 1회 비용(젬). 규칙: 쿼츠 초반 채굴차(1레벨)로
        /// 하루 4시간 접속 + 8시간 오프라인 = 12시간 채굴했을 때 일반 뽑기가 3~5회 나오게(A-17-G ②).
        /// 12시간 기대 젬 22.8개 ÷ 6 = 3.8회(2026-09-29 측정, 광맥 95.1개/시간 × 2%). 값은 테스트가 규칙 범위 안인지 지킨다.</summary>
        public const int NormalPullCostGems = 6;
    }
}
