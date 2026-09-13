namespace GemRacer.Core
{
    /// <summary>D14-N: 설정 화면이 다루는 값 중 "규칙"이라 할 만한 건 프레임 제한 하나뿐이다 —
    /// 허용값이 30/60 두 개로 정해져 있다는 것. 소리 켜짐/꺼짐은 그냥 bool이라 여기 둘 게 없다.
    /// UnityEngine을 참조하지 않는 순수 함수라 Core.Tests가 직접 검증한다(CLAUDE.md 1번).</summary>
    public static class GameSettings
    {
        public const int DefaultFrameRate = 60;
        public const int LowFrameRate = 30;

        /// <summary>허용값(30/60)이 아니면 가까운 쪽으로 붙인다. 45 미만이면 30, 45 이상(정확히
        /// 45 포함)이면 60 — 동률일 때 기본값(60) 쪽으로 떨어지게 정했다. 음수·0처럼 말이 안
        /// 되는 값도, 아주 큰 값(예전 세이브가 깨졌거나 다른 기기 값을 잘못 읽은 경우)도 안전하게
        /// 둘 중 하나로 떨어진다 — 예외를 던지지 않는다(설정 화면이 못 여는 것보다 낫다).</summary>
        public static int NormalizeFrameRate(int requested)
        {
            if (requested == LowFrameRate) return LowFrameRate;
            if (requested == DefaultFrameRate) return DefaultFrameRate;
            return requested < 45 ? LowFrameRate : DefaultFrameRate;
        }
    }
}
