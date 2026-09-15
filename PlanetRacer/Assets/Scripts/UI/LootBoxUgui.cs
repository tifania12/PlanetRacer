using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Audio;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-05(2026-09-16): LootBoxPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// MiningController.TryOpenBox 하나로 확률 뽑기·천장·부품 적용·저장을 전부 코어가 묶어서
    /// 처리하니 여기는 보유 개수 표시와 클릭 전달, 결과 문구 갱신만 한다(UpgradeUgui·CraftingUgui와
    /// 같은 역할 분담). 상자 세 종류(녹슨/강철/티타늄)를 세 줄로 보여주고 마지막 개봉 결과를
    /// 아래 카드에 남긴다.
    /// </summary>
    public sealed class LootBoxUgui : MonoBehaviour
    {
        [Tooltip("개봉 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("상자를 열 때 효과음을 낼 대상. 비워두면 무음.")]
        public AudioHub audioHub;

        static readonly LootBoxType[] BoxOrder = { LootBoxType.Rusty, LootBoxType.Steel, LootBoxType.Titanium };
        static readonly string[] Prefixes = { "rusty", "steel", "titanium" };

        TMP_Text[] _countLabels;
        Button[] _openButtons;
        TMP_Text _resultLabel;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _countLabels = new TMP_Text[Prefixes.Length];
            _openButtons = new Button[Prefixes.Length];

            for (int i = 0; i < Prefixes.Length; i++)
            {
                var p = Prefixes[i];
                _countLabels[i] = UiKit.Find<TMP_Text>(transform, $"{p}-count");
                _openButtons[i] = UiKit.Find<Button>(transform, $"{p}-button");

                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                _openButtons[index]?.onClick.AddListener(() => OnOpenClicked(BoxOrder[index]));
            }

            _resultLabel = UiKit.Find<TMP_Text>(transform, "result-label");
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
            if (_countLabels[index] != null) _countLabels[index].text = $"보유 {count}개";
            if (_openButtons[index] != null) _openButtons[index].interactable = count > 0;
        }

        void OnOpenClicked(LootBoxType type)
        {
            if (target == null) return;
            if (!target.TryOpenBox(type, out var result)) return; // 보유 0개면 조용히 무시(버튼이 이미 비활성이라 보통 여기 안 옴)
            audioHub?.PlayBoxOpen();

            var guaranteed = result.Loot.Guaranteed ? " (천장 확정)" : "";
            if (_resultLabel != null)
                _resultLabel.text =
                    $"{LootBoxOpener.NameKo(type)} 개봉: {result.Loot.Grade}등급{guaranteed} → " +
                    $"{result.Reward.NameKo} +{result.Reward.LevelBonus}";
        }
    }
}
