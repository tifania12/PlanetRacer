using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>D10-N: 레이스 결과가 이미 정해진 뒤(RaceSimulator.Run) 화면에 보여줄 도착 시각표를
    /// 만드는 순수 함수. 실제 기록(Result.Time)은 ±3% 지터뿐이라 서로 몇 초 차이 안 나는 경우가
    /// 많은데, 그걸 그대로 연출 속도에 쓰면 6대가 거의 동시에 결승선을 통과해서 순위를 눈으로
    /// 구분하기 어렵다. 그래서 "순위는 절대 바꾸지 않되 화면에서는 잘 보이게 도착 간격을 벌리는"
    /// 속도 보정을 여기서 계산한다. 판정(RaceSimulator.Run)에는 전혀 관여하지 않는 표시 전용
    /// 계산이고, 시간은 인자로만 받는다(CLAUDE.md 1번) — 연출 길이를 몇 초로 뽑을지(Min~MaxDuration
    /// 범위 안에서 무작위)는 Assets/Scripts 글루 레이어(RaceEntryPanel)가 UnityEngine.Random으로
    /// 정해서 넘긴다. 도착 순서 검증은 D10-M.</summary>
    public static class RaceAnimation
    {
        public const float MinDurationSeconds = 20f;
        public const float MaxDurationSeconds = 30f;

        /// <summary>1등이 연출의 이 비율 지점에서 결승선을 통과한다. 나머지 (1 - 이 값)만큼의
        /// 시간에 남은 순위가 순서대로 들어온다 — 시작하자마자 1등이 바로 들어오면 어색해서
        /// 도입부만큼 여유를 둔다.</summary>
        public const float WinnerArrivalRatio = 0.55f;

        /// <summary>연속한 두 순위 사이 최소 도착 간격(초). 실제 기록이 아무리 붙어 있어도
        /// 화면에서는 최소 이만큼 떨어져서 도착해야 순위가 눈에 보인다.</summary>
        public const float MinGapSeconds = 0.8f;

        public struct Arrival
        {
            public string Id;
            public int Rank;

            /// <summary>연출 시작(0초)부터 결승선을 통과하기까지 걸리는 시간(초).</summary>
            public float ArrivalSeconds;
        }

        /// <summary>results는 입력 순서를 신경 쓰지 않아도 된다 — Rank 기준으로 다시 정렬해서 쓴다.
        /// 반환값은 항상 Rank 오름차순 = ArrivalSeconds 오름차순(동시 도착 없이 엄격히 증가)이고,
        /// 모든 ArrivalSeconds는 (0, durationSeconds] 안에 있다. durationSeconds는 방어적으로
        /// Min/MaxDurationSeconds 범위로 잘라서 쓴다.</summary>
        public static List<Arrival> BuildSchedule(IList<RaceSimulator.Result> results, float durationSeconds)
        {
            var duration = Math.Min(MaxDurationSeconds, Math.Max(MinDurationSeconds, durationSeconds));

            var sorted = new List<RaceSimulator.Result>(results);
            sorted.Sort((a, b) => a.Rank.CompareTo(b.Rank));
            var n = sorted.Count;
            var arrivals = new List<Arrival>(n);
            if (n == 0) return arrivals;

            if (n == 1)
            {
                arrivals.Add(new Arrival { Id = sorted[0].Id, Rank = sorted[0].Rank, ArrivalSeconds = duration * WinnerArrivalRatio });
                return arrivals;
            }

            // 실제 기록 격차를 0(1등)~1(꼴찌)로 정규화. 전부 시간이 같으면(격차 0) 등수 간격으로 균등 배분.
            var totalRealGap = sorted[n - 1].Time - sorted[0].Time;
            var ratio = new float[n];
            for (var i = 0; i < n; i++)
                ratio[i] = totalRealGap > 0f ? (sorted[i].Time - sorted[0].Time) / totalRealGap : (float)i / (n - 1);

            var winnerArrival = duration * WinnerArrivalRatio;
            var spreadBudget = duration - winnerArrival;

            var pos = new float[n];
            for (var i = 0; i < n; i++) pos[i] = winnerArrival + ratio[i] * spreadBudget;

            // 앞에서부터 최소 간격을 강제한다 — 뒤로만 밀리므로 이 루프가 끝나면 항상 엄격히 증가한다.
            for (var i = 1; i < n; i++)
                if (pos[i] < pos[i - 1] + MinGapSeconds) pos[i] = pos[i - 1] + MinGapSeconds;

            // 최소 간격을 강제하다 duration을 넘겼으면(극단적으로 출전자가 많거나 duration이 짧을 때),
            // 전체를 0 기준으로 duration 안에 눌러 담는다. 양수 배율만 곱하므로 순서는 그대로 유지된다.
            if (pos[n - 1] > duration)
            {
                var scale = duration / pos[n - 1];
                for (var i = 0; i < n; i++) pos[i] *= scale;
            }

            for (var i = 0; i < n; i++)
                arrivals.Add(new Arrival { Id = sorted[i].Id, Rank = sorted[i].Rank, ArrivalSeconds = pos[i] });
            return arrivals;
        }
    }
}
