using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D08-N: 레이싱카 부품 제작 화면. MiningController.AvailableParts(지금은 쿼츠
    /// C등급 5종)를 Crafting.uxml의 다섯 줄에 그대로 매핑한다 — 순서가 같다고 가정한다(둘 다
    /// DefaultData.QuartzStarterParts() 선언 순서, Engine/Tire/Suspension/Body/Booster).
    /// 버튼 하나가 상태에 따라 다른 일을 한다: 미보유면 제작, 보유 중이면 장착, 장착 중이면
    /// 해제 — 비용·중복 방지 같은 판단은 전부 코어(PartCraft/PartEquip, MiningController)에
    /// 있고 여기는 화면 갱신과 클릭 전달만 한다(UpgradePanel과 같은 역할 분담).</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CraftingPanel : MonoBehaviour
    {
        [Tooltip("제작 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        VisualElement _root;
        Label _currency;
        Label[] _stateLabels;
        Button[] _buttons;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _currency = _root.Q<Label>("currency-label");

            var nameLabels = new[]
            {
                _root.Q<Label>("engine-name"), _root.Q<Label>("tire-name"), _root.Q<Label>("suspension-name"),
                _root.Q<Label>("body-name"), _root.Q<Label>("booster-name"),
            };
            _stateLabels = new[]
            {
                _root.Q<Label>("engine-state"), _root.Q<Label>("tire-state"), _root.Q<Label>("suspension-state"),
                _root.Q<Label>("body-state"), _root.Q<Label>("booster-state"),
            };
            _buttons = new[]
            {
                _root.Q<Button>("engine-button"), _root.Q<Button>("tire-button"), _root.Q<Button>("suspension-button"),
                _root.Q<Button>("body-button"), _root.Q<Button>("booster-button"),
            };

            // UXML 라벨은 슬롯 이름(엔진 등) 기본값만 갖고 있다 — 실제 부품 이름(NameKo)으로 덮는다.
            if (target != null)
            {
                var parts = target.AvailableParts;
                for (int i = 0; i < nameLabels.Length && i < parts.Count; i++)
                    nameLabels[i].text = parts[i].NameKo;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                _buttons[index].clicked += () => OnRowButtonClicked(index);
            }

            Refresh();
        }

        // 원석·보유 상태가 계속 바뀌니(자동 채굴, 다른 패널에서의 제작) 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            _currency.text = $"원석 {target.RawMinerals:F1}";

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
                _stateLabels[index].text = "미보유";
                _buttons[index].text = $"제작 ({cost:F0})";
                _buttons[index].SetEnabled(cost <= target.RawMinerals);
            }
            else if (equipped)
            {
                _stateLabels[index].text = "장착됨";
                _buttons[index].text = "해제";
                _buttons[index].SetEnabled(true);
            }
            else
            {
                _stateLabels[index].text = "보유 중";
                _buttons[index].text = "장착";
                _buttons[index].SetEnabled(true);
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

        bool IsEquipped(Part part) =>
            target.Car.Slots.TryGetValue(part.Slot, out var equippedPart) && equippedPart != null && equippedPart.Id == part.Id;
    }
}
