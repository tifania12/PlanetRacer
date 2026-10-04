namespace GemRacer.Core
{
    /// <summary>E-09(economy-v2.md 4절 "돈"): 레이스 상금. 돈은 여기서만 들어오고 연구(E-06)에 쓴다.
    /// 상금 = 기본 × 등급 배수(로컬 1·서킷 3·챌린지 8·그랑프리 20) × 행성 배율, 1등이 아니면 20%.
    ///
    /// 기본값(BasePrize)은 플레이스홀더다. 설계 문서가 "연구 비용과 짝으로 BalanceSim에서 정한다"고 했고
    /// 연구소(E-06)가 아직 없어 비교할 비용이 없다. 행성 배율도 같은 이유로 인자로 받는다 —
    /// 코어에 행성별 표를 미리 박아 두면 밸런스 세션이 고칠 때 호출부까지 건드리게 된다.
    /// 연구 "상금 협상"(+5%씩)은 research 쪽에서 bonusMultiplier로 넘긴다.</summary>
    public static class RacePrize
    {
        public const double BasePrize = 100.0;
        public const double LoserShare = 0.20;

        public static double TierMultiplier(RaceTier tier) => tier switch
        {
            RaceTier.Local => 1.0,
            RaceTier.Circuit => 3.0,
            RaceTier.Challenge => 8.0,
            RaceTier.GrandPrix => 20.0,
            _ => 0.0,
        };

        /// <param name="rank">1부터 센 순위. 0 이하는 완주 못 한 것으로 보고 0을 돌려준다.</param>
        public static double Compute(RaceTier tier, int rank, double planetMultiplier = 1.0, double bonusMultiplier = 1.0)
        {
            // NaN은 `<= 0` 비교가 전부 거짓이라 가드를 그냥 통과한다 — !(x > 0)으로 NaN도 같이 거른다.
            if (rank <= 0 || !(planetMultiplier > 0.0) || !(bonusMultiplier > 0.0)
                || double.IsInfinity(planetMultiplier) || double.IsInfinity(bonusMultiplier)) return 0.0;
            var full = BasePrize * TierMultiplier(tier) * planetMultiplier * bonusMultiplier;
            return rank == 1 ? full : full * LoserShare;
        }
    }
}
