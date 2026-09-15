using UnityEngine;

namespace GemRacer.UI
{
    /// <summary>
    /// 2026-09-15: 테스트용 시간 배속. 방치형이라 정상 속도로는 화면을 들여다봐도
    /// 아무 일도 안 일어나는 것처럼 보인다(첫 화물칸이 차는 데 90분을 잡아 뒀다).
    /// 그래서 주소 뒤에 `?fast=10` 을 붙이면 시간이 10배로 흐르게 한다.
    ///
    ///     https://planetracer-daz.pages.dev/?fast=10
    ///
    /// 1~100 사이만 받는다. 값이 없거나 이상하면 1배, 즉 아무 일도 안 한다.
    /// Time.timeScale만 건드리므로 게임 로직은 손댈 필요가 없다.
    ///
    /// **출시 빌드에서는 빠져야 한다.** GEMRACER_DEBUG 심볼이 있을 때만 동작하게 해 두면
    /// 되는데, 지금은 테스트가 급해서 항상 켜 뒀다 — 출시 전에 아래 #if를 살릴 것.
    /// </summary>
    public sealed class DebugTimeScale : MonoBehaviour
    {
        [Tooltip("주소에 ?fast= 가 없을 때 쓸 배속. 1이면 정상 속도.")]
        public float fallbackScale = 1f;

        void Start()
        {
            float scale = fallbackScale;

#if UNITY_WEBGL && !UNITY_EDITOR
            var url = Application.absoluteURL;
            var parsed = ParseFast(url);
            if (parsed > 0f) scale = parsed;
#endif

            scale = Mathf.Clamp(scale, 1f, 100f);
            Time.timeScale = scale;
            if (!Mathf.Approximately(scale, 1f))
                Debug.Log($"[GemRacer] 테스트 배속 {scale}x — 주소의 ?fast= 로 켜졌다.");
        }

        /// <summary>주소에서 fast 값을 뽑는다. 못 뽑으면 0을 돌려준다.
        /// 쿼리 파서를 따로 안 쓰고 문자열로만 처리한다 — WebGL에서 의존성을 늘리지 않으려고.</summary>
        public static float ParseFast(string url)
        {
            if (string.IsNullOrEmpty(url)) return 0f;
            int q = url.IndexOf("fast=", System.StringComparison.OrdinalIgnoreCase);
            if (q < 0) return 0f;
            int start = q + 5;
            int end = start;
            while (end < url.Length && (char.IsDigit(url[end]) || url[end] == '.')) end++;
            if (end == start) return 0f;
            return float.TryParse(url.Substring(start, end - start),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0f;
        }

        void OnDestroy()
        {
            // 에디터에서 Play를 멈춘 뒤에도 배속이 남아 있으면 다음 Play가 이상해진다.
            Time.timeScale = 1f;
        }
    }
}
