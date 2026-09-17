using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-02(2026-09-15): UpgradePanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// MiningController.rig를 읽어 각 줄의 레벨·다음 효과·비용을 표시하고, 버튼을 누르면
    /// MiningController.TryUpgrade를 부른다. 비용·상한 계산은 전부 코어(RigUpgrade.cs)에 있고
    /// 여기는 조회·표시만 한다.
    ///
    /// 2026-09-17: 네 번째 줄로 **제련소**가 들어왔고, 화폐가 줄마다 다르다.
    /// 제련소만 원석으로 사고 나머지 셋은 정제 광물로 산다(RigUpgrade.IsPaidWithRawMinerals).
    /// 그래서 머리글에 두 화폐를 같이 띄우고, 버튼 글자에도 어느 화폐인지 적는다 —
    /// 비용 숫자만 있으면 왜 못 누르는지 화면만 보고는 알 수가 없다.
    /// </summary>
    public sealed class UpgradeUgui : MonoBehaviour
    {
        [Tooltip("업그레이드 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        TMP_Text _currency;
        TMP_Text _toolLevel, _cargoLevel, _engineLevel, _refineryLevel;
        TMP_Text _toolEffect, _cargoEffect, _engineEffect, _refineryEffect;
        Button _toolButton, _cargoButton, _engineButton, _refineryButton;
        TMP_Text _toolButtonLabel, _cargoButtonLabel, _engineButtonLabel, _refineryButtonLabel;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _currency    = UiKit.Find<TMP_Text>(transform, "currency-label");
            _toolLevel   = UiKit.Find<TMP_Text>(transform, "tool-level");
            _cargoLevel  = UiKit.Find<TMP_Text>(transform, "cargo-level");
            _engineLevel = UiKit.Find<TMP_Text>(transform, "engine-level");
            _toolEffect   = UiKit.Find<TMP_Text>(transform, "tool-effect");
            _cargoEffect  = UiKit.Find<TMP_Text>(transform, "cargo-effect");
            _engineEffect = UiKit.Find<TMP_Text>(transform, "engine-effect");

            _toolButton   = UiKit.Find<Button>(transform, "tool-button");
            _cargoButton  = UiKit.Find<Button>(transform, "cargo-button");
            _engineButton = UiKit.Find<Button>(transform, "engine-button");

            // 제련소 줄은 2026-09-17에 들어왔다. 씬이 아직 안 고쳐진 빌드에서도 나머지 세 줄은
            // 그대로 돌아야 하니 없어도 경고를 안 찍는다(warnIfMissing: false).
            _refineryLevel  = UiKit.Find<TMP_Text>(transform, "refinery-level", false);
            _refineryEffect = UiKit.Find<TMP_Text>(transform, "refinery-effect", false);
            _refineryButton = UiKit.Find<Button>(transform, "refinery-button", false);

            _toolButtonLabel      = LabelOf(_toolButton);
            _cargoButtonLabel     = LabelOf(_cargoButton);
            _engineButtonLabel    = LabelOf(_engineButton);
            _refineryButtonLabel  = LabelOf(_refineryButton);

            _toolButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Tool));
            _cargoButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Cargo));
            _engineButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Engine));
            _refineryButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Refinery));
        }

        static TMP_Text LabelOf(Button b) => b != null ? b.GetComponentInChildren<TMP_Text>() : null;

        // 정제 광물이 매 프레임 쌓이니(MiningController) 버튼이 켜지는 순간을 놓치지 않게 매 프레임
        // 갱신한다. 패널이 꺼져 있으면(SetActive(false)) uGUI는 Update 자체를 안 불러서 따로
        // 막을 필요가 없다 — UI Toolkit판이 매 프레임 Refresh를 부르던 것과 같은 동작이다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var rig = target.rig;
            var planet = target.CurrentPlanet;
            if (planet == null) return;

            if (_currency != null)
                _currency.text = $"원석 {target.RawMinerals:F1}   ·   정제 광물 {target.RefinedMinerals:F1}";

            // 화면에서도 제련소가 맨 앞이다(BootstrapUpgradeUgui 주석 참고).
            // 제련소 0레벨은 정제량이 0이라 "현재 0"이 그대로 나온다. 그게 지금 상태를 정확히
            // 말해 주는 문구라 특별 취급하지 않는다.
            SetRow(UpgradeSlot.Refinery, rig, _refineryLevel, _refineryEffect, _refineryButton, _refineryButtonLabel,
                $"제련소 Lv.{rig.RefineryLevel}",
                $"다음: 시간당 정제 {MiningSimulator.RefinePerHour(UpgradeCost.Apply(UpgradeSlot.Refinery, rig), planet):F0} " +
                $"(현재 {MiningSimulator.RefinePerHour(rig, planet):F0})");

            SetRow(UpgradeSlot.Tool, rig, _toolLevel, _toolEffect, _toolButton, _toolButtonLabel,
                $"곡괭이 Lv.{rig.ToolLevel}",
                $"다음: 시간당 {MiningSimulator.MineralsPerHour(UpgradeCost.Apply(UpgradeSlot.Tool, rig), planet):F0} " +
                $"(현재 {MiningSimulator.MineralsPerHour(rig, planet):F0})");

            SetRow(UpgradeSlot.Cargo, rig, _cargoLevel, _cargoEffect, _cargoButton, _cargoButtonLabel,
                $"화물칸 Lv.{rig.CargoLevel}",
                $"다음: 상한 {MiningSimulator.CargoHours(UpgradeCost.Apply(UpgradeSlot.Cargo, rig), planet):F1}h " +
                $"(현재 {MiningSimulator.CargoHours(rig, planet):F1}h)");

            SetRow(UpgradeSlot.Engine, rig, _engineLevel, _engineEffect, _engineButton, _engineButtonLabel,
                $"엔진 Lv.{rig.EngineLevel}",
                $"다음: 속도 {MiningSimulator.RigSpeed(UpgradeCost.Apply(UpgradeSlot.Engine, rig), planet):F1}m/s " +
                $"(현재 {MiningSimulator.RigSpeed(rig, planet):F1}m/s)");
        }

        // E-02(2026-09-18): 회색 버튼만 봐서는 "돈만 모으면 되는지" "아예 막힌 건지" 구분이
        // 안 된다는 피드백 — 새 UI 요소 없이 이미 있는 effectLabel(word wrap 켜져 있고 60px로
        // 여유 있게 잡은 자리) 끝에 한 줄만 덧붙인다.
        const string HintRefinedShort = " 정제 광물이 부족해요 — 제련소를 올리면 더 빨리 쌓여요.";
        const string HintRawShort = " 원석이 부족해요 — 채굴이 좀 더 쌓일 때까지 기다려 보세요.";

        void SetRow(UpgradeSlot slot, MiningRig rig, TMP_Text levelLabel, TMP_Text effectLabel,
                    Button button, TMP_Text buttonLabel, string levelText, string effectText)
        {
            if (levelLabel != null) levelLabel.text = levelText;
            var atMax = UpgradeCost.AtMax(slot, rig);

            var cost = UpgradeCost.Cost(slot, rig);
            var payWithRaw = UpgradeCost.IsPaidWithRawMinerals(slot);
            var held = payWithRaw ? target.RawMinerals : target.RefinedMinerals;
            var unit = payWithRaw ? "원석" : "정제";
            var isShortOnFunds = !atMax && cost > held;

            if (effectLabel != null)
                effectLabel.text = atMax ? "최대 레벨"
                    : isShortOnFunds ? effectText + (payWithRaw ? HintRawShort : HintRefinedShort)
                    : effectText;

            if (buttonLabel != null) buttonLabel.text = atMax ? "MAX" : $"업그레이드 ({unit} {cost:F0})";
            if (button != null) button.interactable = !atMax && cost <= held;
        }
    }
}
