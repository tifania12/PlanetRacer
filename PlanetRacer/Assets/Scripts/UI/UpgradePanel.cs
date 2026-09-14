using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D05-N: 채굴 장비 업그레이드 화면. MiningController.rig(코어 MiningRig)를 그대로
    /// 읽고, 버튼을 누르면 MiningController.TryUpgrade를 부른다 — 비용 계산·레벨 상한은 전부
    /// 코어(RigUpgrade.cs)에 있고 여기는 화면 갱신만 한다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UpgradePanel : MonoBehaviour
    {
        [Tooltip("업그레이드 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        VisualElement _root;
        Label _currency, _toolLevel, _cargoLevel, _engineLevel, _toolEffect, _cargoEffect, _engineEffect;
        Button _toolButton, _cargoButton, _engineButton;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _currency = _root.Q<Label>("currency-label");
            _toolLevel = _root.Q<Label>("tool-level");
            _cargoLevel = _root.Q<Label>("cargo-level");
            _engineLevel = _root.Q<Label>("engine-level");
            _toolEffect = _root.Q<Label>("tool-effect");
            _cargoEffect = _root.Q<Label>("cargo-effect");
            _engineEffect = _root.Q<Label>("engine-effect");

            _toolButton = _root.Q<Button>("tool-button");
            _cargoButton = _root.Q<Button>("cargo-button");
            _engineButton = _root.Q<Button>("engine-button");
            _toolButton.clicked += () => target?.TryUpgrade(UpgradeSlot.Tool);
            _cargoButton.clicked += () => target?.TryUpgrade(UpgradeSlot.Cargo);
            _engineButton.clicked += () => target?.TryUpgrade(UpgradeSlot.Engine);

            Refresh();
        }

        // 정제 광물이 매 프레임 쌓이니(MiningController) 버튼이 켜지는 순간을 놓치지 않게 매 프레임 갱신한다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var rig = target.rig;
            var planet = target.CurrentPlanet;

            // M-02: 업그레이드는 정제 광물로 낸다(원석은 화물칸 상한이 있는 중간 자원일 뿐이다).
            _currency.text = $"정제 광물 {target.RefinedMinerals:F1}";

            SetRow(UpgradeSlot.Tool, rig, _toolLevel, _toolEffect, _toolButton,
                $"곡괭이 Lv.{rig.ToolLevel}",
                $"다음: 시간당 {MiningSimulator.MineralsPerHour(UpgradeCost.Apply(UpgradeSlot.Tool, rig), planet):F0} " +
                $"(현재 {MiningSimulator.MineralsPerHour(rig, planet):F0})");

            SetRow(UpgradeSlot.Cargo, rig, _cargoLevel, _cargoEffect, _cargoButton,
                $"화물칸 Lv.{rig.CargoLevel}",
                $"다음: 상한 {MiningSimulator.CargoHours(UpgradeCost.Apply(UpgradeSlot.Cargo, rig), planet):F1}h " +
                $"(현재 {MiningSimulator.CargoHours(rig, planet):F1}h)");

            SetRow(UpgradeSlot.Engine, rig, _engineLevel, _engineEffect, _engineButton,
                $"엔진 Lv.{rig.EngineLevel}",
                $"다음: 속도 {MiningSimulator.RigSpeed(UpgradeCost.Apply(UpgradeSlot.Engine, rig), planet):F1}m/s " +
                $"(현재 {MiningSimulator.RigSpeed(rig, planet):F1}m/s)");
        }

        void SetRow(UpgradeSlot slot, MiningRig rig, Label levelLabel, Label effectLabel, Button button, string levelText, string effectText)
        {
            levelLabel.text = levelText;
            var atMax = UpgradeCost.AtMax(slot, rig);
            effectLabel.text = atMax ? "최대 레벨" : effectText;
            var cost = UpgradeCost.Cost(slot, rig);
            button.text = atMax ? "MAX" : $"업그레이드 ({cost:F0})";
            button.SetEnabled(!atMax && cost <= target.RefinedMinerals);
        }
    }
}
