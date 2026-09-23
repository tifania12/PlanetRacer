namespace GemRacer.Core
{
    /// <summary>monetization.md 2-5 "행성 통행증" 구독 혜택 중 "매일 정제 광물 지급" 쪽.
    /// Entitlements.DailyRefinedMineralsGrant(구독 중인지 bool)는 이미 있는데 "하루에 한 번만"이라는
    /// 청구 타이밍은 순수 계산에 안 맞는 상태값이라 Entitlements.cs 주석이 TODO로 남겨 뒀던 것 —
    /// DailyLoginReward.cs와 같은 역할 분리로 그 TODO를 채운다(날짜 판정만 여기서 맡고, 구독
    /// 여부는 호출하는 쪽이 Entitlements.Effective(...).DailyRefinedMineralsGrant로 먼저 확인한다).
    /// 날짜 경계는 RewardAdTracker.DayIndex를 그대로 재사용(중복 정의 없음, DailyLoginReward와 동일).</summary>
    public struct SubscriptionGrantState
    {
        /// <summary>마지막으로 지급받은 날(RewardAdTracker.DayIndex 값). 기본값 0은 "받은 적 없음"
        /// (DailyLoginState.LastClaimedDayIndex와 같은 관례).</summary>
        public long LastClaimedDayIndex;
    }

    public static class SubscriptionDailyGrant
    {
        /// <summary>한 번 지급하는 정제 광물 양. 구체 수치는 플레이스홀더다(DailyLoginReward의
        /// RawMineralsByStreakDay와 같은 처지 — P4 봇 시뮬레이션에서 재조정). PartCostC(C등급
        /// 부품 제작 비용 15, DefaultData.cs)를 기준으로 잡았다 — monetization.md가 이 혜택을
        /// "소량"이라고만 적어 뒀는데(정확한 액수는 없음), 매일 공짜로 주는 값이 구독하지 않고도
        /// 버는 정제 광물 경제를 흔들면 안 되니 "하루에 C등급 하나 만들 정도"를 첫 값으로 잡았다.</summary>
        public const float RefinedMineralsPerClaim = 15f;

        /// <summary>오늘 아직 못 받았고 구독 중이면 true. subscriptionActive는 호출하는 쪽이
        /// Entitlements.Effective(...).DailyRefinedMineralsGrant로 구해서 넘긴다 — 이 함수는
        /// 날짜 판정만 하고 구독 판정은 하지 않는다(역할 분리, 클래스 주석 참고).</summary>
        public static bool CanClaim(SubscriptionGrantState state, bool subscriptionActive, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            if (!subscriptionActive) return false;
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            return state.LastClaimedDayIndex != today;
        }

        /// <summary>오늘 첫 지급에서 부른다. 이미 오늘 받았으면 상태를 그대로 돌려준다(방어적
        /// 이중 확인, DailyLoginReward.Claim과 같은 태도) — 구독 여부는 다시 확인하지 않으니
        /// 호출 전에 CanClaim으로 먼저 걸러야 한다. 실제 지급량은 호출부가 RefinedMineralsPerClaim을
        /// SaveData.RefinedMinerals에 직접 더한다(여기는 날짜 상태만 갖고 있다,
        /// DailyLoginReward/RewardAdTracker와 같은 역할 분리).</summary>
        public static SubscriptionGrantState Claim(SubscriptionGrantState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            if (state.LastClaimedDayIndex == today) return state;
            return new SubscriptionGrantState { LastClaimedDayIndex = today };
        }
    }
}
