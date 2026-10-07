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

    /// <summary>던전 한 판의 60초 채굴 구간. 시간은 호출부가 델타로 밀어 준다(프레임 수가 아니라 초 단위라
    /// 30fps·고주사율에서 같은 결과). 끝난 뒤에 들어오는 틱·채굴량은 무시해 점수가 새지 않는다.</summary>
    public struct DungeonMiningRun
    {
        public const double DurationSeconds = 60.0;

        public double Elapsed;
        public double Mined;

        public bool IsOver => Elapsed >= DurationSeconds;
        public double Remaining => IsOver ? 0.0 : DurationSeconds - Elapsed;

        /// <summary>deltaSeconds만큼 시간을 보내고 그 사이 캔 양을 더한다. 남은 시간을 넘는 델타는 남은 만큼만 센다
        /// (마지막 틱이 크게 튀어도 60초를 넘겨 쳐 주지 않는다). 음수·NaN은 0으로 본다.</summary>
        public void Advance(double deltaSeconds, double minedInDelta)
        {
            if (IsOver) return;
            var d = deltaSeconds > 0.0 ? deltaSeconds : 0.0;
            var m = minedInDelta > 0.0 ? minedInDelta : 0.0;
            if (d > Remaining)
            {
                // 잘린 시간 비율만큼만 채굴량을 인정한다
                m = d > 0.0 ? m * (Remaining / d) : 0.0;
                d = Remaining;
            }
            Elapsed += d;
            Mined += m;
        }
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

        /// <summary>한 판이 끝났을 때 받는 강화석. 연구 "강화석 감정" 배율(<see cref="ResearchState.StoneMultiplier"/>)을
        /// 곱하고 내림한다 — 소수 강화석이 생기지 않게. 배율이 1 미만이거나 비정상이면 1로 본다.</summary>
        public static int StonesReward(DungeonGrade grade, double stoneMultiplier)
        {
            var m = stoneMultiplier >= 1.0 && stoneMultiplier < 1000.0 ? stoneMultiplier : 1.0;
            return (int)System.Math.Floor(StonesFor(grade) * m + 1e-9);
        }

        public static int EntriesLeft(DungeonState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            var used = state.DayIndex == today ? state.EntriesToday : 0;
            if (used < 0) used = 0; // 오염된 세이브가 횟수를 늘려 주지 못하게
            var left = MaxEntriesPerDay - used;
            return left < 0 ? 0 : left;
        }

        /// <summary>입장 한 번을 소비한다. 남은 횟수가 없으면 false에 상태 그대로.</summary>
        public static bool TryEnter(ref DungeonState state, long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            if (EntriesLeft(state, nowUnixSeconds, timeZoneOffsetSeconds) <= 0) return false;
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            var used = state.DayIndex == today ? state.EntriesToday : 0;
            if (used < 0) used = 0;
            state = new DungeonState { DayIndex = today, EntriesToday = used + 1 };
            return true;
        }

        /// <summary>던전 60초 동안 캔 양을 0~1 점수로 바꾼다(<see cref="GradeFor"/>의 miningScore 입력).
        /// 만점 기준(fullMarks)은 호출부가 그 행성의 60초 기대 채굴량으로 정한다 — 밸런스 값이라 여기선 모른다.
        /// 기준이 0 이하·NaN이면 0점(0으로 나누기·무한대 점수 방지), 음수·NaN 채굴량도 0점, 기준을 넘으면 1로 자른다.</summary>
        public static double MiningScore(double mined, double fullMarks)
        {
            if (!(fullMarks > 0.0) || double.IsInfinity(fullMarks)) return 0.0;
            if (!(mined > 0.0)) return 0.0;
            var r = mined / fullMarks;
            return r > 1.0 ? 1.0 : r;
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
