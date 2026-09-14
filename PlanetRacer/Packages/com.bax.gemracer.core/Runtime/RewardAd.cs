using System;

namespace GemRacer.Core
{
    /// <summary>M-09: 보상형 광고 네 자리. monetization.md 3장 — 전면 광고 없음, 유저가 누를
    /// 때만, 하루 한도까지. 실제 SDK 연동(광고 로드·재생·보상 지급)은 P3라 지금은 "이 자리에서
    /// 오늘 몇 번 더 볼 수 있나"만 코어로 계산한다. 보상 자체(2배 지급, 상자 1개 더 등)는 각
    /// 자리를 부르는 쪽(MiningController)이 CanWatch로 확인한 뒤 자기 로직으로 직접 준다 — 여기는
    /// 카운터만 갖고 있고 어떤 보상인지는 모른다.</summary>
    public enum RewardAdSlot
    {
        /// <summary>오프라인 보상 화면 — 보상 2배. monetization.md 3장.</summary>
        OfflineRewardDouble,

        /// <summary>레이스 결과 — 공구 상자 1개 더. monetization.md 3장.</summary>
        ExtraLootBox,

        /// <summary>화물칸 가득 참 — 1시간 동안 상한 2배. monetization.md 3장.</summary>
        CargoCapDoubleHour,

        /// <summary>연료 부족 — 연료 +3. monetization.md 3장.</summary>
        FuelRefill,
    }

    /// <summary>자리별 오늘 시청 횟수 + 마지막으로 리셋한 날짜. 세이브에 그대로 저장한다
    /// (SaveData.ToRewardAdState/ApplyRewardAdState). JsonUtility가 Dictionary를 못 다뤄서
    /// (SaveData.cs 클래스 상단 주석과 같은 이유) 자리 네 개를 필드로 따로 둔다.</summary>
    public struct RewardAdState
    {
        /// <summary>이 카운트들이 어느 "하루"에 속하는지. RewardAdTracker.DayIndex가 만든 값 —
        /// 절대 시각이 아니라 그냥 날짜 하나를 가리키는 정수라 직접 비교(==)만 하면 된다.</summary>
        public long LastResetDayIndex;

        public int OfflineRewardDoubleWatchedToday;
        public int ExtraLootBoxWatchedToday;
        public int CargoCapDoubleHourWatchedToday;
        public int FuelRefillWatchedToday;
    }

    /// <summary>하루 한도 계산. 시간은 전부 인자로만 받는다(CLAUDE.md 1번) — "지금 몇 시인지"는
    /// Assets 글루 레이어가 DateTimeOffset.UtcNow로 구해서 넘긴다. "하루"의 경계는 UTC 자정이
    /// 아니라 timeZoneOffsetSeconds만큼 민 자정이다 — 이 게임은 한국 유저 기준이라 글루 레이어는
    /// 보통 32400(KST, UTC+9)을 넘길 것을 예상하지만, 어떤 값을 넘기든 이 계산 자체는 UTC에
    /// 얽매이지 않는다(CLAUDE.md 1번 "시간은 인자로 받는다"를 시간대까지 포함해서 지킨 것).</summary>
    public static class RewardAdTracker
    {
        public const int OfflineRewardDoubleDailyLimit = 3;
        public const int ExtraLootBoxDailyLimit = 3;
        public const int CargoCapDoubleHourDailyLimit = 2;
        public const int FuelRefillDailyLimit = 2;

        const long SecondsPerDay = 86400;

        public static int DailyLimit(RewardAdSlot slot)
        {
            switch (slot)
            {
                case RewardAdSlot.OfflineRewardDouble: return OfflineRewardDoubleDailyLimit;
                case RewardAdSlot.ExtraLootBox: return ExtraLootBoxDailyLimit;
                case RewardAdSlot.CargoCapDoubleHour: return CargoCapDoubleHourDailyLimit;
                case RewardAdSlot.FuelRefill: return FuelRefillDailyLimit;
                default: throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        /// <summary>nowUnixSeconds를 timeZoneOffsetSeconds만큼 민 다음 날짜 단위로 몫을 낸다.
        /// C#의 정수 나눗셈은 0쪽으로 버림이라 로컬 시각이 음수(UTC 자정 근처에서 서쪽 시간대)일
        /// 때 하루가 밀리는 걸 floor 나눗셈으로 바로잡는다.</summary>
        public static long DayIndex(long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var local = nowUnixSeconds + timeZoneOffsetSeconds;
            var day = local / SecondsPerDay;
            if (local % SecondsPerDay != 0 && local < 0) day--;
            return day;
        }

        /// <summary>날짜가 바뀌었으면 네 카운트를 전부 0으로 되돌린 새 상태를 돌려준다. 그대로면
        /// 입력을 그대로 돌려준다(호출부가 매번 이 함수를 거쳐도 안전하게 no-op이라는 뜻).</summary>
        public static RewardAdState ResetIfNewDay(RewardAdState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            if (state.LastResetDayIndex == today) return state;
            return new RewardAdState { LastResetDayIndex = today };
        }

        public static int WatchedToday(RewardAdState state, RewardAdSlot slot)
        {
            switch (slot)
            {
                case RewardAdSlot.OfflineRewardDouble: return state.OfflineRewardDoubleWatchedToday;
                case RewardAdSlot.ExtraLootBox: return state.ExtraLootBoxWatchedToday;
                case RewardAdSlot.CargoCapDoubleHour: return state.CargoCapDoubleHourWatchedToday;
                case RewardAdSlot.FuelRefill: return state.FuelRefillWatchedToday;
                default: throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        /// <summary>오늘 이 자리를 몇 번 더 볼 수 있는지(음수 없이 0까지). 화면이 "3/3" 같은
        /// 표시를 하려면 DailyLimit(slot) - WatchedToday를 따로 계산해도 된다 — 이 함수는 버튼을
        /// 누를 수 있는지만 알면 되는 쪽을 위한 것.</summary>
        public static int RemainingToday(RewardAdState state, RewardAdSlot slot, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            state = ResetIfNewDay(state, nowUnixSeconds, timeZoneOffsetSeconds);
            var remaining = DailyLimit(slot) - WatchedToday(state, slot);
            return Math.Max(0, remaining);
        }

        public static bool CanWatch(RewardAdState state, RewardAdSlot slot, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            return RemainingToday(state, slot, nowUnixSeconds, timeZoneOffsetSeconds) > 0;
        }

        /// <summary>광고를 실제로 다 보고 나서(콜백에서) 부른다. 이미 오늘 한도를 다 썼으면
        /// 카운트를 안 올리고 그대로 돌려준다 — 호출부가 CanWatch를 먼저 확인하는 게 정상 경로지만,
        /// 확인을 건너뛰고 불러도 한도를 넘기지 않게 방어한다(RaceFuel.Recover가 MaxFuel을 절대
        /// 안 넘기는 것과 같은 태도).</summary>
        public static RewardAdState RecordWatch(RewardAdState state, RewardAdSlot slot, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            state = ResetIfNewDay(state, nowUnixSeconds, timeZoneOffsetSeconds);
            if (!CanWatch(state, slot, nowUnixSeconds, timeZoneOffsetSeconds)) return state;

            switch (slot)
            {
                case RewardAdSlot.OfflineRewardDouble: state.OfflineRewardDoubleWatchedToday++; break;
                case RewardAdSlot.ExtraLootBox: state.ExtraLootBoxWatchedToday++; break;
                case RewardAdSlot.CargoCapDoubleHour: state.CargoCapDoubleHourWatchedToday++; break;
                case RewardAdSlot.FuelRefill: state.FuelRefillWatchedToday++; break;
                default: throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
            return state;
        }
    }
}
