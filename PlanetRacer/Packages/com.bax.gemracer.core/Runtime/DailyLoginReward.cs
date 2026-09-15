using System;

namespace GemRacer.Core
{
    /// <summary>D18-N: 리텐션 훅 중 "하루 첫 접속 보상" 쪽. 화물칸 알림(M-05)과 짝을 이루는
    /// 두 축인데, 그쪽은 Unity Mobile Notifications 패키지가 있어야 해서 에디터 세션 몫으로
    /// 남겨 두고(docs/backlog.md M-05 참고 — Package Manager를 클라우드 세션이 건드리면
    /// URP 셰이더 사고처럼 빌드가 죽을 위험이 있다는 이유로 손 안 댐) 이쪽만 먼저 core로 끝낸다.
    /// 오늘 처음 접속했을 때 한 번 받는 무료 보상이라 monetization.md의 "시간은 팔고 힘은 팔지
    /// 않는다"와는 애초에 무관하다(파는 게 아니라 매일 공짜로 주는 것).
    /// 날짜 판정은 RewardAdTracker.DayIndex를 그대로 쓴다 — "하루"의 경계(KST 자정)를 이미
    /// 검증된 곳에서 재사용해 같은 계산을 두 번 만들지 않는다.</summary>
    public struct DailyLoginState
    {
        /// <summary>마지막으로 보상을 받은 날(RewardAdTracker.DayIndex 값). 기본값 0은 "받은 적
        /// 없음"을 뜻한다 — 실제 서비스 날짜는 1970-01-01보다 한참 뒤라 0과 겹칠 일이 없다
        /// (RewardAdState.LastResetDayIndex와 같은 관례).</summary>
        public long LastClaimedDayIndex;

        /// <summary>연속 접속 일수(1부터 시작). 어제 안 받고 오늘 받으면(하루 이상 건너뜀) 1로
        /// 되돌아간다 — 완전히 처음으로 리셋되는 건 아니고 스트릭만 끊긴다.</summary>
        public int StreakDays;
    }

    public static class DailyLoginReward
    {
        /// <summary>스트릭 7일 주기 보상표(원석, RawMinerals). 6일차까지 완만히 오르다 7일차에
        /// 크게 준다 — "일주일 다 채우면 좋은 게 나온다"는 감. 8일째부터는 다시 1일차 보상으로
        /// 돈다(월 단위로 초기화하지 않고 접속만 하면 계속 도는 루프). 구체 수치는 플레이스홀더,
        /// 다른 밸런스 표들과 같이 P4 봇 시뮬레이션에서 재조정.</summary>
        public static readonly float[] RawMineralsByStreakDay = { 5f, 8f, 10f, 12f, 15f, 18f, 30f };

        public const int CycleLength = 7;

        /// <summary>streakDays(1부터 시작, 그 이상은 7일 주기로 순환)에 맞는 보상량. 0 이하가
        /// 들어와도(호출 실수 방어) 1일차 값을 돌려준다 — 배열 인덱스 예외를 내지 않는다.</summary>
        public static float RawMineralsFor(int streakDays)
        {
            if (streakDays < 1) streakDays = 1;
            var index = (streakDays - 1) % CycleLength;
            return RawMineralsByStreakDay[index];
        }

        /// <summary>오늘 이미 받았으면 false. 하루에 여러 번 접속해도 두 번 못 받는다는 뜻
        /// (RewardAdTracker의 하루 한도 개념과 같은 결).</summary>
        public static bool CanClaim(DailyLoginState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            return state.LastClaimedDayIndex != today;
        }

        /// <summary>오늘 첫 접속에서 부른다. 이미 오늘 받았으면 상태를 그대로 돌려준다(방어적
        /// 이중 확인, RewardAdTracker.RecordWatch와 같은 태도). 실제 지급량은 호출부가
        /// RawMineralsFor(결과.StreakDays)로 계산해서 직접 준다 — 여기는 날짜·스트릭 상태만
        /// 갖고 있고 무엇을 주는지는 모른다(RewardAdTracker와 같은 역할 분리).</summary>
        public static DailyLoginState Claim(DailyLoginState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            if (state.LastClaimedDayIndex == today) return state;

            var isConsecutiveDay = state.LastClaimedDayIndex == today - 1;
            var nextStreak = isConsecutiveDay ? state.StreakDays + 1 : 1;

            return new DailyLoginState { LastClaimedDayIndex = today, StreakDays = nextStreak };
        }
    }
}
