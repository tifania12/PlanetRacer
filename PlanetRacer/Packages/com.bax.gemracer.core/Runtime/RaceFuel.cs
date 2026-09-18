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
        // 2026-09-17 템포 조정. 전에는 10개/10분이었는데, 레이스 한 판이 25초라 시작하자마자
        // 5분 만에 10판을 다 돌리고 그 뒤로는 10분에 한 판이 됐다. 한 방 터지고 멈추는 모양이라
        // "계속 돌아가는 느낌"이 안 났다(시뮬레이션: 0~10분 10회 → 10~20분 2회).
        // 최대치를 줄여 초반 몰림을 눕히고 회복을 빠르게 해서 시간당 15판이 꾸준히 돌게 했다.
        public const int MaxFuel = 8;
        public const int RecoverySeconds = 240; // 4분당 1개

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
            => Recover(currentFuel, baselineUnixSeconds, nowUnixSeconds, MaxFuel);

        /// <summary>2026-09-19: Entitlements.BonusFuelCapacity(구독 중 대전권 +2, monetization.md 2-5)를
        /// 나중에 배선할 자리를 미리 만들어 둔 오버로드 — maxFuel을 인자로 받는다. 기존 3인자
        /// 호출은 전부 이 함수에 MaxFuel을 그대로 넘기는 것과 완전히 같아서(바로 위 오버로드),
        /// MiningController.cs 등 이미 3인자로 부르던 곳은 동작이 하나도 안 바뀐다 — 실제로
        /// maxFuel을 다르게 넘기는 배선은 Unity 세션이 컴파일을 확인하며 할 몫으로 남긴다
        /// (docs/decisions.md 2026-09-15 "M-06/M-07이 아직 안 붙인 값" 참고).</summary>
        public static (int fuel, long baselineUnixSeconds) Recover(int currentFuel, long baselineUnixSeconds, long nowUnixSeconds, int maxFuel)
        {
            maxFuel = Math.Max(0, maxFuel);
            currentFuel = Math.Max(0, Math.Min(maxFuel, currentFuel));
            if (currentFuel >= maxFuel) return (maxFuel, nowUnixSeconds);

            var elapsedSeconds = Math.Max(0L, nowUnixSeconds - baselineUnixSeconds);
            var missing = maxFuel - currentFuel;
            var recovered = elapsedSeconds / RecoverySeconds; // long, missing은 작은 정수라 안전

            if (recovered >= missing) return (maxFuel, nowUnixSeconds);

            var newBaseline = baselineUnixSeconds + recovered * RecoverySeconds;
            return (currentFuel + (int)recovered, newBaseline);
        }
    }
}
