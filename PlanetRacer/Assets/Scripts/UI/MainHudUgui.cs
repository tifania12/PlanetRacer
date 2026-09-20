using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Audio;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// D08(2026-09-15): MainHud를 일반 UI(uGUI)로 옮긴 것. Tifania가 UI Toolkit에 익숙하지
    /// 않아 나중에 직접 고치기 어렵다고 해서 옮긴다. 화면 구성은 그대로다 —
    /// 위에 상태바(행성 이름·원석), 그 아래 화물칸 게이지, 맨 아래 버튼 다섯 개.
    ///
    /// 옛 MainHud.cs는 253줄이었는데 여기서는 절반도 안 된다. 줄어든 대부분이
    /// EnsureXxxHiddenOnce 다섯 벌이었다. UIDocument의 rootVisualElement가 늦게 만들어져서
    /// 필요했던 방어 코드인데, uGUI에서는 패널이 그냥 GameObject라 필요가 없다.
    ///
    /// 엘리먼트는 이름으로 찾는다(UiKit.Find). 인스펙터 연결에 기대지 않아서
    /// 부트스트랩이 계층을 다시 만들어도 이름만 같으면 그대로 돈다.
    /// </summary>
    public sealed class MainHudUgui : MonoBehaviour
    {
        [Tooltip("HUD가 읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Header("여닫을 패널들 (비워두면 그 버튼은 꺼진 채로 남는다)")]
        public UiPanel upgradePanel;
        public UiPanel craftPanel;
        public UiPanel racePanel;
        public UiPanel boxPanel;
        public UiPanel settingsPanel;
        public UiPanel shopPanel;
        public UiPanel gachaOddsPanel;

        [Tooltip("버튼을 누를 때 탭 효과음을 낼 대상. 비워두면 무음.")]
        public AudioHub audioHub;

        TMP_Text _planetName, _mineralCount;
        Image _cargoFill;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _planetName   = UiKit.Find<TMP_Text>(transform, "planet-name");
            _mineralCount = UiKit.Find<TMP_Text>(transform, "mineral-count");
            _cargoFill    = UiKit.Find<Image>(transform, "cargo-gauge-fill");

            // A-16(2026-09-20): 원석 옆 아이콘. 자리가 없으면(GemRacer/24를 아직 안 돌린 씬)
            // 조용히 넘어간다(UiKit.SetIcon 자체가 그렇게 만들어져 있다).
            UiKit.SetIcon(transform, "mineral-icon", "icon-raw-mineral");

            // 게이지는 Image의 fillAmount로 채운다. 부트스트랩이 Filled/Horizontal로 만들어 둔다.
            if (_cargoFill != null)
            {
                _cargoFill.type = Image.Type.Filled;
                _cargoFill.fillMethod = Image.FillMethod.Horizontal;
                _cargoFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

            Wire("btn-mine",     "업그레이드", upgradePanel, "아직 준비되지 않음 (D05 업그레이드)");
            Wire("btn-craft",    "제작",       craftPanel,   "아직 준비되지 않음 (D08 부품 제작)");
            Wire("btn-race",     "레이스",     racePanel,    "아직 준비되지 않음 (D09 레이스 출전)");
            Wire("btn-box",      "상자",       boxPanel,     "아직 준비되지 않음 (D11 공구 상자)");
            Wire("btn-settings", "설정",       settingsPanel,"아직 준비되지 않음 (D14 설정 화면)");
            Wire("btn-shop",     "상점",       shopPanel,    "아직 준비되지 않음 (M-07 상점) — 씬에 버튼이 아직 없으면 'GemRacer/23' 먼저");
            Wire("btn-gacha-odds", "확률",     gachaOddsPanel, "아직 준비되지 않음 (P-15 뽑기 확률 공개) — 액션 줄에 여는 버튼이 아직 없다");
        }

        /// <summary>버튼 하나를 패널 하나에 묶는다. 패널이 안 물려 있으면 버튼을 꺼서
        /// 조용히 알아챌 수 있게 한다 — 옛 MainHud가 tooltip으로 하던 것과 같은 뜻이다.
        /// uGUI 버튼에는 tooltip이 없어서 라벨 뒤에 표시를 붙인다.</summary>
        void Wire(string buttonName, string label, UiPanel panel, string notReadyNote)
        {
            var btn = UiKit.Find<Button>(transform, buttonName);
            if (btn == null) return;

            var text = btn.GetComponentInChildren<TMP_Text>();

            if (panel == null)
            {
                btn.interactable = false;
                if (text != null) text.text = label;
                Debug.Log($"[GemRacer] HUD 버튼 '{label}' 꺼 둠 — {notReadyNote}");
                return;
            }

            btn.interactable = true;
            if (text != null) text.text = label;
            btn.onClick.AddListener(() =>
            {
                audioHub?.PlayUiTap();
                panel.Toggle();
            });
        }

        void Update()
        {
            if (target == null || target.CurrentPlanet == null) return;

            if (_planetName != null)   _planetName.text   = $"{target.CurrentPlanet.NameKo} 행성";
            if (_mineralCount != null) _mineralCount.text = $"원석 {target.RawMinerals:F1}";

            if (_cargoFill != null)
            {
                var capacity = target.CargoCapacityMinerals;
                _cargoFill.fillAmount = capacity > 0f
                    ? Mathf.Clamp01(target.RawMinerals / capacity)
                    : 0f;
            }
        }
    }
}
