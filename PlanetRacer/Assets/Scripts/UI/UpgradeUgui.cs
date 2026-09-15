using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-02(2026-09-15): UpgradePanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// MiningController.rig를 읽어 세 줄(곡괭이/화물칸/엔진)의 레벨·다음 효과·비용을 표시하고,
    /// 버튼을 누르면 MiningController.TryUpgrade를 부른다. 비용·상한 계산은 전부 코어
    /// (RigUpgrade.cs)에 있고 여기는 조회·표시만 한다.
    /// </summary>
    public sealed class UpgradeUgui : MonoBehaviour
    {
        [Tooltip("업그레이드 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        TMP_Text _currency, _toolLevel, _cargoLevel, _engineLevel, _toolEffect, _cargoEffect, _engineEffect;
        Button _toolButton, _cargoButton, _engineButton;
        TMP_Text _toolButtonLabel, _cargoButtonLabel, _engineButtonLabel;

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
            _toolButtonLabel   = _toolButton != null ? _toolButton.GetComponentInChildren<TMP_Text>() : null;
            _cargoButtonLabel  = _cargoButton != null ? _cargoButton.GetComponentInChildren<TMP_Text>() : null;
            _engineButtonLabel = _engineButton != null ? _engineButton.GetComponentInChildren<TMP_Text>() : null;

            _toolButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Tool));
            _cargoButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Cargo));
            _engineButton?.onClick.AddListener(() => target?.TryUpgrade(UpgradeSlot.Engine));
        }

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

            if (_currency != null) _currency.text = $"정제 광물 {target.RefinedMinerals:F1}";

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

        void SetRow(UpgradeSlot slot, MiningRig rig, TMP_Text levelLabel, TMP_Text effectLabel,
                    Button button, TMP_Text buttonLabel, string levelText, string effectText)
        {
            if (levelLabel != null) levelLabel.text = levelText;
            var atMax = UpgradeCost.AtMax(slot, rig);
            if (effectLabel != null) effectLabel.text = atMax ? "최대 레벨" : effectText;
            var cost = UpgradeCost.Cost(slot, rig);
            if (buttonLabel != null) buttonLabel.text = atMax ? "MAX" : $"업그레이드 ({cost:F0})";
            if (button != null) button.interactable = !atMax && cost <= target.RefinedMinerals;
        }
    }
}
