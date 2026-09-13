using System;

namespace GemRacer.Core
{
    /// <summary>D09-N: 레이스 출전 연료. 10분(RecoverySeconds)마다 1개씩 차서 최대 10개.
    /// 시간은 인자로만 받는다(CLAUDE.md 1번) — "지금 몇 시인지"는 Assets/Scripts 글루 레이어가
    /// DateTimeOffset.UtcNow로 구해서 넘긴다. 화물칸 오프라인 캐치업(MiningSimulator.Offline)과
    /// 같은 정책: 이미 꽉 찬 상태에서 흘러간 시간은 버린다(캐리 없음) — 나중에 로그인이 뜸해도
    /// 연료가 무한정 쌓이지 않는다.</summary>
    public static class RaceFuel
    {
        public const int MaxFuel = 10;
        public const int RecoverySeconds = 600; // 10분당 1개

        /// <summary>레이스 한 번 출전에 드는 연료. 지금은 등급 구분 없이 1 고정 —
        /// P2에서 레이스 4등급이 생기면 등급별 차등을 검토할 자리(TODO).</summary>
        public const int EntryCost = 1;

        /// <summary>기준 시각(baselineUnixSeconds)부터 지금(nowUnixSeconds)까지 흐른 시간으로
        /// 회복된 연료 개수를 계산한다. 이미 꽉 찼거나(회복 시계를 돌릴 필요가 없다) 회복이
        /// 최대치를 채우고도 남으면, 새 기준 시각을 그냥 지금으로 당겨서 남는 시간을 버린다 —
        /// 그래야 나중에 연료를 다시 쓰기 시작할 때부터 정확히 10분 단위로 회복이 시작된다.
        /// 그 외에는 정확히 회복된 만큼만 기준 시각을 앞으로 밀어서, 다음 회복까지 남은 몫이
        /// 다음 호출에도 그대로 이어진다(잘게 쪼개 불러도 큰 델타로 한 번에 불러도 결과가 같다 —
        /// D04-M의 오프라인 채굴 델타 테스트와 같은 이유).</summary>
        public static (int fuel, long baselineUnixSeconds) Recover(int currentFuel, long baselineUnixSeconds, long nowUnixSeconds)
        {
            currentFuel = Math.Max(0, Math.Min(MaxFuel, currentFuel));
            if (currentFuel >= MaxFuel) return (MaxFuel, nowUnixSeconds);

            var elapsedSeconds = Math.Max(0L, nowUnixSeconds - baselineUnixSeconds);
            var missing = MaxFuel - currentFuel;
            var recovered = elapsedSeconds / RecoverySeconds; // long, missing은 최대 10이라 안전

            if (recovered >= missing) return (MaxFuel, nowUnixSeconds);

            var newBaseline = baselineUnixSeconds + recovered * RecoverySeconds;
            return (currentFuel + (int)recovered, newBaseline);
        }
    }
}
