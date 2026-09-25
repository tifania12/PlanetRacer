namespace GemRacer.Core
{
    /// <summary>M-14: 레이스 우승 시 시즌 패스(SeasonPass.cs, M-10) XP. monetization.md 2-6
    /// "레벨은 플레이로만 오른다"를 실제로 채우는 첫 값이다 — 값 자체는 다른 M 시리즈
    /// 플레이스홀더(DailyRefinedMineralsGrant 등)와 같은 자리, P4 봇 시뮬레이션에서 재조정한다.
    ///
    /// DefaultData.SeasonPassTiers()가 레벨당 100XP씩 늘어 레벨10(마지막 티어)에 누적 1000XP를
    /// 요구한다 — 4주 시즌 동안 로컬 레이스(하루 몇 판, RaceFuel로 자연히 제한됨) 위주로도
    /// 끝까지 갈 수 있도록 등급이 높을수록 더 주는 정도로만 잡았다. RaceBoxReward.ForTier와
    /// 같은 모양(등급→값 매핑)이라 서킷·챌린지 코스가 실제로 생겨도(W2) 손댈 곳이 없다.</summary>
    public static class SeasonPassRaceXp
    {
        public static int ForTier(RaceTier tier) => tier switch
        {
            RaceTier.Local => 15,
            RaceTier.Circuit => 25,
            RaceTier.Challenge => 40,
            RaceTier.GrandPrix => 60,
            _ => 0,
        };
    }
}
