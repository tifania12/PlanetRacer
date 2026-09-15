using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-03(2026-09-15): CraftingPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// MiningController.AvailableParts(쿼츠 C등급 5종, 엔진/타이어/서스펜션/차체/부스터 순서)를
    /// 다섯 줄에 매핑한다. 버튼 하나가 상태에 따라 다른 일을 한다: 미보유면 제작, 보유 중이면
    /// 장착, 장착 중이면 해제 — 비용·중복 방지 같은 판단은 전부 코어(PartCraft/PartEquip,
    /// MiningController)에 있고 여기는 화면 갱신과 클릭 전달만 한다(UpgradeUgui와 같은 역할 분담).
    /// </summary>
    public sealed class CraftingUgui : MonoBehaviour
    {
        [Tooltip("제작 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        TMP_Text _currency;
        TMP_Text[] _nameLabels;
        TMP_Text[] _stateLabels;
        TMP_Text[] _enhanceLabels;
        Button[] _buttons;
        TMP_Text[] _buttonLabels;
        Button[] _enhanceButtons;
        TMP_Text[] _enhanceButtonLabels;

        static readonly string[] Prefixes = { "engine", "tire", "suspension", "body", "booster" };

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _currency = UiKit.Find<TMP_Text>(transform, "currency-label");

            _nameLabels = new TMP_Text[Prefixes.Length];
            _stateLabels = new TMP_Text[Prefixes.Length];
            _enhanceLabels = new TMP_Text[Prefixes.Length];
            _buttons = new Button[Prefixes.Length];
            _buttonLabels = new TMP_Text[Prefixes.Length];
            _enhanceButtons = new Button[Prefixes.Length];
            _enhanceButtonLabels = new TMP_Text[Prefixes.Length];

            for (int i = 0; i < Prefixes.Length; i++)
            {
                var p = Prefixes[i];
                _nameLabels[i] = UiKit.Find<TMP_Text>(transform, $"{p}-name");
                _stateLabels[i] = UiKit.Find<TMP_Text>(transform, $"{p}-state");
                _enhanceLabels[i] = UiKit.Find<TMP_Text>(transform, $"{p}-enhance-level");
                _buttons[i] = UiKit.Find<Button>(transform, $"{p}-button");
                _buttonLabels[i] = _buttons[i] != null ? _buttons[i].GetComponentInChildren<TMP_Text>() : null;
                _enhanceButtons[i] = UiKit.Find<Button>(transform, $"{p}-enhance-button");
                _enhanceButtonLabels[i] = _enhanceButtons[i] != null ? _enhanceButtons[i].GetComponentInChildren<TMP_Text>() : null;

                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                _buttons[index]?.onClick.AddListener(() => OnRowButtonClicked(index));
                _enhanceButtons[index]?.onClick.AddListener(() => OnEnhanceButtonClicked(index));
            }

            // UXML 라벨은 슬롯 이름(엔진 등) 기본값만 갖고 있다 — 실제 부품 이름(NameKo)으로 덮는다.
            if (target != null)
            {
                var parts = target.AvailableParts;
                for (int i = 0; i < _nameLabels.Length && i < parts.Count; i++)
                    if (_nameLabels[i] != null) _nameLabels[i].text = parts[i].NameKo;
            }
        }

        // 정제 광물·보유 상태가 계속 바뀌니(자동 채굴+제련, 다른 패널에서의 제작) 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            // M-02: 제작·강화 둘 다 정제 광물로 낸다.
            if (_currency != null) _currency.text = $"정제 광물 {target.RefinedMinerals:F1}";

            var parts = target.AvailableParts;
            for (int i = 0; i < parts.Count && i < _stateLabels.Length; i++)
                SetRow(i, parts[i]);
        }

        void SetRow(int index, Part part)
        {
            var owned = target.OwnedPartIds.Contains(part.Id);
            var equipped = IsEquipped(part);

            if (!owned)
            {
                var cost = PartCraft.Cost(part.Grade);
                if (_stateLabels[index] != null) _stateLabels[index].text = "미보유";
                if (_buttonLabels[index] != null) _buttonLabels[index].text = $"제작 ({cost:F0})";
                if (_buttons[index] != null) _buttons[index].interactable = cost <= target.RefinedMinerals;
            }
            else if (equipped)
            {
                if (_stateLabels[index] != null) _stateLabels[index].text = "장착됨";
                if (_buttonLabels[index] != null) _buttonLabels[index].text = "해제";
                if (_buttons[index] != null) _buttons[index].interactable = true;
            }
            else
            {
                if (_stateLabels[index] != null) _stateLabels[index].text = "보유 중";
                if (_buttonLabels[index] != null) _buttonLabels[index].text = "장착";
                if (_buttons[index] != null) _buttons[index].interactable = true;
            }

            // D12-N: 강화는 보유 여부와 무관하게 라벨은 항상 보여주되(미보유면 +0), 버튼은
            // 보유했을 때만 누를 수 있다 — 미보유 부품을 강화한다는 개념 자체가 없다.
            if (_enhanceLabels[index] != null) _enhanceLabels[index].text = $"+{part.Enhance}";
            if (!owned)
            {
                if (_enhanceButtonLabels[index] != null) _enhanceButtonLabels[index].text = "강화";
                if (_enhanceButtons[index] != null) _enhanceButtons[index].interactable = false;
            }
            else if (PartEnhance.AtMax(part))
            {
                if (_enhanceButtonLabels[index] != null) _enhanceButtonLabels[index].text = "MAX";
                if (_enhanceButtons[index] != null) _enhanceButtons[index].interactable = false;
            }
            else
            {
                var enhanceCost = PartEnhance.Cost(part);
                if (_enhanceButtonLabels[index] != null) _enhanceButtonLabels[index].text = $"강화 ({enhanceCost:F0})";
                if (_enhanceButtons[index] != null) _enhanceButtons[index].interactable = enhanceCost <= target.RefinedMinerals;
            }
        }

        void OnRowButtonClicked(int index)
        {
            if (target == null) return;
            var parts = target.AvailableParts;
            if (index >= parts.Count) return;
            var part = parts[index];

            if (!target.OwnedPartIds.Contains(part.Id)) { target.TryCraftPart(part); return; }

            if (IsEquipped(part)) target.UnequipPart(part.Slot);
            else target.TryEquipPart(part);
        }

        void OnEnhanceButtonClicked(int index)
        {
            if (target == null) return;
            var parts = target.AvailableParts;
            if (index >= parts.Count) return;
            target.TryEnhancePart(parts[index]);
        }

        bool IsEquipped(Part part) =>
            target.Car.Slots.TryGetValue(part.Slot, out var equippedPart) && equippedPart != null && equippedPart.Id == part.Id;
    }
}
