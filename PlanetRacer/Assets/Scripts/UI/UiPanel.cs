using UnityEngine;

namespace GemRacer.UI
{
    /// <summary>
    /// D08(2026-09-15): UI Toolkit의 UIDocument가 하던 "이 패널 하나" 역할을 대신한다.
    ///
    /// UI Toolkit 시절에는 패널을 여닫으려면 `document.rootVisualElement.style.display` 를
    /// 건드려야 했는데, rootVisualElement가 그 컴포넌트 자신의 OnEnable에서야 만들어지는 탓에
    /// "몇 프레임 동안 계속 시도하다 성공하면 멈추는" 코드가 패널마다 붙어 있었다
    /// (MainHud.cs에 EnsureXxxHiddenOnce가 다섯 벌 있었다).
    ///
    /// uGUI에서는 그냥 GameObject라서 그 문제가 없다. 켜고 끄는 것이 전부다.
    /// 그래서 이 컴포넌트는 얇다 — 이게 이 이사의 이득이기도 하다.
    /// </summary>
    public sealed class UiPanel : MonoBehaviour
    {
        [Tooltip("시작할 때 닫힌 채로 둘지. 오버레이 패널은 켜 두고, HUD처럼 항상 보이는 것은 끈다.")]
        public bool hiddenOnStart = true;

        [Tooltip("열고 닫을 때 소리를 낼지. AudioHub는 여는 쪽에서 넘긴다.")]
        public bool silent = false;

        public bool IsOpen => gameObject.activeSelf;

        void Awake()
        {
            if (hiddenOnStart) gameObject.SetActive(false);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        /// <summary>여닫고 나서 열렸는지 돌려준다.</summary>
        public bool Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            return gameObject.activeSelf;
        }
    }
}
