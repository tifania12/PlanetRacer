namespace GemRacer.Core
{
    public enum DungeonGrade { C = 0, B = 1, A = 2, S = 3 }

    /// <summary>하루 안에 던전에 몇 번 들어갔는지. DayIndex는 RewardAdTracker.DayIndex 값이다.
    /// 옛 세이브엔 없어서 0으로 시작하고, 0은 "아직 안 들어감"으로 읽힌다(날짜가 다르면 횟수를 0으로 본다).</summary>
    public struct DungeonState
    {
        public long DayIndex;
        public int EntriesToday;
    }

    /// <summary>E-08(economy-v2.md 6절): 일일 던전. 하루 3회(로컬 자정 초기화), 요일마다 행성 하나,
    /// 결과 등급 C/B/A/S에 따라 강화석 2/3/5/7개. 평균 4개 × 3회 ≈ 하루 12개가 목표다.
    ///
    /// 이 파일은 "입장 횟수·요일 행성·등급별 보상"만 다룬다. 60초 채굴 점수와 레이스 순위를 등급으로
    /// 바꾸는 규칙(GradeFor)은 플레이스홀더다 — 실제 점수 분포를 본 뒤 밸런스 세션이 정한다.
    /// 시간은 전부 인자로 받는다(CLAUDE.md 1번).</summary>
    public static class DailyDungeon
    {
        public const int MaxEntriesPerDay = 3;
        public const long SecondsPerDay = 86400;

        /// <summary>월(0)~토(5)는 행성 하나로 고정. 일요일(6)은 "전부 중 선택"이라 null — 호출부가 고른 행성을 쓴다.</summary>
        static readonly string[] WeekdayPlanets = { "quartz", "ruby", "sapphire", "aquamarine", "cinnabar", "lapis", null };

        /// <summary>0=월 … 6=일. 유닉스 시각 0일(1970-01-01)이 목요일이라 3을 더한다.</summary>
        public static int Weekday(long dayIndex)
        {
            var w = (dayIndex + 3) % 7;
            if (w < 0) w += 7;
            return (int)w;
        }

        public static string PlanetIdForDay(long dayIndex) => WeekdayPlanets[Weekday(dayIndex)];

        public static int StonesFor(DungeonGrade grade) => grade switch
        {
            DungeonGrade.C => 2,
            DungeonGrade.B => 3,
            DungeonGrade.A => 5,
            DungeonGrade.S => 7,
            _ => 0,
        };

        public static int EntriesLeft(DungeonState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            var used = state.DayIndex == today ? state.EntriesToday : 0;
            var left = MaxEntriesPerDay - used;
            return left < 0 ? 0 : left;
        }

        /// <summary>입장 한 번을 소비한다. 남은 횟수가 없으면 false에 상태 그대로.</summary>
        public static bool TryEnter(ref DungeonState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            if (EntriesLeft(state, nowUnixSeconds, timeZoneOffsetSeconds) <= 0) return false;
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            var used = state.DayIndex == today ? state.EntriesToday : 0;
            state = new DungeonState { DayIndex = today, EntriesToday = used + 1 };
            return true;
        }

        /// <summary>플레이스홀더: 채굴 점수(0~1, 60초 안에 캔 양을 만점 기준으로 나눈 값)와 레이스 순위로 등급을 매긴다.
        /// 순위 1·점수 0.8 이상이 S, 이후 단계적으로 내려간다. 기준값은 실제 플레이 데이터가 생기면 바꾼다.</summary>
        public static DungeonGrade GradeFor(double miningScore, int rank)
        {
            if (rank <= 0) return DungeonGrade.C;
            var rankPart = rank == 1 ? 1.0 : rank == 2 ? 0.6 : rank == 3 ? 0.3 : 0.0;
            var m = miningScore < 0.0 ? 0.0 : miningScore > 1.0 ? 1.0 : miningScore;
            var total = 0.5 * m + 0.5 * rankPart;
            if (total >= 0.8) return DungeonGrade.S;
            if (total >= 0.55) return DungeonGrade.A;
            if (total >= 0.3) return DungeonGrade.B;
            return DungeonGrade.C;
        }
    }
}
