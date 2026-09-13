using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>W-04: 지금까지 따로 놀던 조각들 — 3D 채굴 씬(D01·D04), Root.uxml 반응형 HUD
    /// 자리 표시자(W-03), 업그레이드 패널(D05-N) — 을 한 화면으로 묶는다.
    ///
    /// Root.uxml/Root.uss 자체는 그대로 두고(ResponsiveUITest 씬이 여전히 그 자리 표시자를
    /// 쓴다), 이 스크립트가 실행될 때만 viewport-area에 "live" 클래스를 붙여 자리 표시자
    /// 배경·문구를 지운다 — 그러면 이 UIDocument 뒤에서 실제로 돌고 있는 3D 카메라가 그대로
    /// 비친다. 상태바(행성 이름·광물 수)와 화물칸 게이지도 MiningController를 읽어 채운다.
    ///
    /// action-row의 "채굴" 버튼은 실제로는 채굴은 이미 자동으로 돌고 있어서 할 일이 없다 —
    /// 대신 업그레이드 패널을 열고 닫는 용도로 재활용했다. "제작"(D08)·"레이스"(D09)도 각각
    /// 자기 패널을 열고 닫는 용도로 재활용했다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainHud : MonoBehaviour
    {
        [Tooltip("HUD가 읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("'채굴' 버튼으로 여닫을 업그레이드 패널의 UIDocument. 비워두면 이 버튼은 아무 일도 안 한다.")]
        public UIDocument upgradeDocument;

        [Tooltip("'제작' 버튼으로 여닫을 부품 제작 패널의 UIDocument. 비워두면 이 버튼은 비활성 상태로 남는다.")]
        public UIDocument craftDocument;

        [Tooltip("'레이스' 버튼으로 여닫을 레이스 출전 패널의 UIDocument. 비워두면 이 버튼은 비활성 상태로 남는다.")]
        public UIDocument raceDocument;

        VisualElement _root, _viewport, _cargoFill;
        Label _planetName, _mineralCount;
        Button _btnMine, _btnCraft, _btnRace;
        bool _upgradeRootInitialized;
        bool _craftRootInitialized;
        bool _raceRootInitialized;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _viewport = _root.Q<VisualElement>("viewport-area");
            _viewport?.AddToClassList("live");

            _planetName = _root.Q<Label>("planet-name");
            _mineralCount = _root.Q<Label>("mineral-count");
            _cargoFill = _root.Q<VisualElement>("cargo-gauge-fill");

            _btnMine = _root.Q<Button>("btn-mine");
            _btnCraft = _root.Q<Button>("btn-craft");
            _btnRace = _root.Q<Button>("btn-race");

            _btnMine.text = "업그레이드";
            _btnMine.clicked += ToggleUpgradePanel;

            // D08-N: 제작 화면이 생겼으니 버튼을 켠다. craftDocument가 안 물려 있으면(부트스트랩이
            // 아직 옛 버전이거나 실수로 안 넣었으면) 예전처럼 비활성 상태로 남겨서 조용히 알아챌
            // 수 있게 했다.
            if (craftDocument != null)
            {
                _btnCraft.SetEnabled(true);
                _btnCraft.tooltip = "";
                _btnCraft.clicked += ToggleCraftPanel;
            }
            else
            {
                _btnCraft.SetEnabled(false);
                _btnCraft.tooltip = "아직 준비되지 않음 (D08 부품 제작)";
            }

            // D09-N: 레이스 출전 화면이 생겼으니 버튼을 켠다. craftDocument와 같은 패턴 —
            // raceDocument가 안 물려 있으면(부트스트랩이 옛 버전이면) 비활성 상태로 남는다.
            if (raceDocument != null)
            {
                _btnRace.SetEnabled(true);
                _btnRace.tooltip = "";
                _btnRace.clicked += ToggleRacePanel;
            }
            else
            {
                _btnRace.SetEnabled(false);
                _btnRace.tooltip = "아직 준비되지 않음 (D09 레이스 출전)";
            }
        }

        void Update()
        {
            EnsureUpgradeRootHiddenOnce();
            EnsureCraftRootHiddenOnce();
            EnsureRaceRootHiddenOnce();
            Refresh();
        }

        void Refresh()
        {
            if (target == null || target.CurrentPlanet == null) return;

            _planetName.text = $"{target.CurrentPlanet.NameKo} 행성";
            _mineralCount.text = $"원석 {target.RawMinerals:F1}";

            if (_cargoFill != null)
            {
                var capacity = target.CargoCapacityMinerals;
                var fillPercent = capacity > 0f ? Mathf.Clamp01(target.RawMinerals / capacity) * 100f : 0f;
                _cargoFill.style.width = Length.Percent(fillPercent);
            }
        }

        // UIDocument의 rootVisualElement는 그 컴포넌트 자신의 OnEnable에서 만들어진다.
        // 다른 GameObject에 있는 UIDocument라 MainHud.OnEnable 시점엔 아직 준비 안 됐을 수도
        // 있어서, 처음 몇 프레임 동안 계속 시도하다가 한 번 성공하면 멈춘다.
        void EnsureUpgradeRootHiddenOnce()
        {
            if (_upgradeRootInitialized || upgradeDocument == null) return;
            var upgradeRoot = upgradeDocument.rootVisualElement;
            if (upgradeRoot == null) return;
            upgradeRoot.style.display = DisplayStyle.None;
            _upgradeRootInitialized = true;
        }

        void ToggleUpgradePanel()
        {
            if (upgradeDocument == null) return;
            var upgradeRoot = upgradeDocument.rootVisualElement;
            if (upgradeRoot == null) return;
            bool hidden = upgradeRoot.style.display == DisplayStyle.None;
            upgradeRoot.style.display = hidden ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // 업그레이드 패널과 같은 이유로(UIDocument.rootVisualElement가 다른 GameObject의
        // OnEnable에서 만들어지므로) 처음 몇 프레임 동안 계속 시도하다가 한 번 성공하면 멈춘다.
        void EnsureCraftRootHiddenOnce()
        {
            if (_craftRootInitialized || craftDocument == null) return;
            var craftRoot = craftDocument.rootVisualElement;
            if (craftRoot == null) return;
            craftRoot.style.display = DisplayStyle.None;
            _craftRootInitialized = true;
        }

        void ToggleCraftPanel()
        {
            if (craftDocument == null) return;
            var craftRoot = craftDocument.rootVisualElement;
            if (craftRoot == null) return;
            bool hidden = craftRoot.style.display == DisplayStyle.None;
            craftRoot.style.display = hidden ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // 업그레이드·제작 패널과 같은 이유로 처음 몇 프레임 동안 계속 시도하다가 한 번 성공하면 멈춘다.
        void EnsureRaceRootHiddenOnce()
        {
            if (_raceRootInitialized || raceDocument == null) return;
            var raceRoot = raceDocument.rootVisualElement;
            if (raceRoot == null) return;
            raceRoot.style.display = DisplayStyle.None;
            _raceRootInitialized = true;
        }

        void ToggleRacePanel()
        {
            if (raceDocument == null) return;
            var raceRoot = raceDocument.rootVisualElement;
            if (raceRoot == null) return;
            bool hidden = raceRoot.style.display == DisplayStyle.None;
            raceRoot.style.display = hidden ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
