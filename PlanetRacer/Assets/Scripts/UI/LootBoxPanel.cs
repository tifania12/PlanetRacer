using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D11-N 후속: 공구 상자 개봉 화면. Crafting/Upgrade 패널과 같은 오버레이 패턴이다 —
    /// 코어가 이미 MiningController.TryOpenBox 하나로 확률 뽑기·천장·부품 적용·저장을 전부
    /// 묶어 뒀으니 여기는 화면 갱신과 클릭 전달만 한다. 상자 세 종류(녹슨/강철/티타늄)를 한 줄씩
    /// 보여주고, 마지막으로 연 결과를 아래 카드에 남긴다 — 천장(피티) 확정이면 문구를 다르게 보여준다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class LootBoxPanel : MonoBehaviour
    {
        [Tooltip("개봉 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        static readonly LootBoxType[] BoxOrder = { LootBoxType.Rusty, LootBoxType.Steel, LootBoxType.Titanium };

        VisualElement _root;
        Label[] _countLabels;
        Button[] _openButtons;
        Label _resultLabel;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _countLabels = new[]
            {
                _root.Q<Label>("rusty-count"), _root.Q<Label>("steel-count"), _root.Q<Label>("titanium-count"),
            };
            _openButtons = new[]
            {
                _root.Q<Button>("rusty-button"), _root.Q<Button>("steel-button"), _root.Q<Button>("titanium-button"),
            };
            _resultLabel = _root.Q<Label>("result-label");

            for (int i = 0; i < _openButtons.Length; i++)
            {
                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정(CraftingPanel과 같은 이유)
                _openButtons[index].clicked += () => OnOpenClicked(BoxOrder[index]);
            }

            Refresh();
        }

        // 상자 개수는 레이스 우승(다른 패널)으로 바뀌니 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            SetRow(0, target.RustyBoxCount);
            SetRow(1, target.SteelBoxCount);
            SetRow(2, target.TitaniumBoxCount);
        }

        void SetRow(int index, int count)
        {
            _countLabels[index].text = $"보유 {count}개";
            _openButtons[index].SetEnabled(count > 0);
        }

        void OnOpenClicked(LootBoxType type)
        {
            if (target == null) return;
            if (!target.TryOpenBox(type, out var result)) return; // 보유 0개면 조용히 무시(버튼이 이미 비활성이라 보통 여기 안 옴)

            var guaranteed = result.Loot.Guaranteed ? " (천장 확정)" : "";
            _resultLabel.text =
                $"{LootBoxOpener.NameKo(type)} 개봉: {result.Loot.Grade}등급{guaranteed} → " +
                $"{result.Reward.NameKo} +{result.Reward.LevelBonus}";
        }
    }
}
