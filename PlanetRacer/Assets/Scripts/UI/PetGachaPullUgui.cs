using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// A-17(2026-09-21, pet-gacha.md 9-1): 펫 뽑기를 실제로 돌리는 화면. 확률표는 이미
    /// PetGachaOddsUgui가 그려 두므로 여기는 버튼을 누르면 MiningController.Pull*Pet를 불러
    /// 세이브에 반영하고 결과를 보여주기만 한다 — ShopUgui(target 찾기·Refresh 패턴)와
    /// 같은 모양이다.
    ///
    /// 조각 합성(PetFusion) 실행 화면은 PetFusionUgui(GemRacer/29)다 — btn-fusion이 그 패널을
    /// 연다(2026-09-22). fusionPanel을 안 물려 두면 MainHudUgui.Wire와 같은 이유로 버튼이 꺼진다.
    ///
    /// P-16(2026-09-26): 인장 구매 버튼 두 칸(btn-buy-seal-1/10) — ShopUgui.cs가 남겨 둔 제안대로
    /// 여기 붙였다. `ShopSkuId.TranscendentSeal1`/`10`은 `ShopPurchase.Apply`가 아니라
    /// `MiningController.DebugPurchase`가 직접 `PetGachaSave.AddSeal`을 늘리는 특수 SKU라
    /// ShopUgui의 Prefixes 배열(아홉 줄 고정)에는 안 들어간다 — 여기서 SeasonPassUgui의
    /// paidtrack-button과 같은 방식(DebugPurchase 직접 호출)으로 잇는다.
    /// </summary>
    public sealed class PetGachaPullUgui : MonoBehaviour
    {
        [Tooltip("뽑기 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("btn-fusion이 열 조각 합성 화면. 비워두면 그 버튼은 꺼진 채로 남는다.")]
        public UiPanel fusionPanel;

        TMP_Text _mineralsLabel, _sealsLabel, _freeLimitLabel, _advancedPityLabel, _specialPityLabel;
        Button _freeBtn, _normalBtn, _advancedBtn, _advancedTenBtn, _specialBtn, _fusionBtn;
        Button _buySeal1Btn, _buySeal10Btn;
        GameObject _resultPanel, _resultSingle, _resultGrid;
        TMP_Text _resultNameLabel, _resultNoteLabel;
        Image[] _resultGridImages;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _mineralsLabel = UiKit.Find<TMP_Text>(transform, "currency-minerals", false);
            _sealsLabel = UiKit.Find<TMP_Text>(transform, "currency-seals", false);
            _freeLimitLabel = UiKit.Find<TMP_Text>(transform, "free-limit", false);
            _advancedPityLabel = UiKit.Find<TMP_Text>(transform, "advanced-pity", false);
            _specialPityLabel = UiKit.Find<TMP_Text>(transform, "special-pity", false);

            _freeBtn = UiKit.Find<Button>(transform, "btn-pull-free", false);
            _normalBtn = UiKit.Find<Button>(transform, "btn-pull-normal", false);
            _advancedBtn = UiKit.Find<Button>(transform, "btn-pull-advanced", false);
            _advancedTenBtn = UiKit.Find<Button>(transform, "btn-pull-advanced-ten", false);
            _specialBtn = UiKit.Find<Button>(transform, "btn-pull-special", false);
            _fusionBtn = UiKit.Find<Button>(transform, "btn-fusion", false);
            _buySeal1Btn = UiKit.Find<Button>(transform, "btn-buy-seal-1", false);
            _buySeal10Btn = UiKit.Find<Button>(transform, "btn-buy-seal-10", false);

            _resultPanel = UiKit.FindObject(transform, "result-panel", false);
            _resultSingle = UiKit.FindObject(transform, "result-single", false);
            _resultGrid = UiKit.FindObject(transform, "result-grid", false);
            _resultNameLabel = UiKit.Find<TMP_Text>(transform, "result-name", false);
            _resultNoteLabel = UiKit.Find<TMP_Text>(transform, "result-note", false);

            _resultGridImages = new Image[10];
            for (var i = 0; i < _resultGridImages.Length; i++)
                _resultGridImages[i] = UiKit.Find<Image>(transform, $"result-portrait-{i}", false);

            _freeBtn?.onClick.AddListener(OnFreeClicked);
            _normalBtn?.onClick.AddListener(OnNormalClicked);
            _advancedBtn?.onClick.AddListener(OnAdvancedClicked);
            _advancedTenBtn?.onClick.AddListener(OnAdvancedTenClicked);
            _specialBtn?.onClick.AddListener(OnSpecialClicked);

            // 가격은 DefaultData.ShopItems()(=shop.csv)에서 읽는다 — ShopUgui가 하는 것과 같은
            // 이유로 여기 문구에 값을 직접 박아 두지 않는다.
            var items = DefaultData.ShopItems();
            WireSealButton(_buySeal1Btn, items, ShopSkuId.TranscendentSeal1);
            WireSealButton(_buySeal10Btn, items, ShopSkuId.TranscendentSeal10);

            // MainHudUgui.Wire와 같은 규칙 — fusionPanel이 안 물려 있으면 버튼을 꺼서
            // "아직 씬 배선이 안 됐다"는 걸 조용히 알 수 있게 한다.
            if (_fusionBtn != null)
            {
                if (fusionPanel != null)
                {
                    _fusionBtn.interactable = true;
                    var label = _fusionBtn.GetComponentInChildren<TMP_Text>();
                    if (label != null) label.text = "조각 합성";
                    _fusionBtn.onClick.AddListener(() => fusionPanel.Toggle());
                }
                else
                {
                    _fusionBtn.interactable = false;
                }
            }

            if (_resultPanel != null) _resultPanel.SetActive(false);
        }

        // 재화·천장 진행도는 이 화면 밖(채굴·다른 뽑기)에서도 바뀔 수 있으니 ShopUgui.Refresh와
        // 같은 이유로 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var gacha = target.PetGacha;

            if (_mineralsLabel != null) _mineralsLabel.text = $"원석 {target.RawMinerals:F1}";
            if (_sealsLabel != null) _sealsLabel.text = $"초월의 인장 {gacha.TranscendentSealCount}개";

            var canFree = gacha.CanPullFree();
            if (_freeLimitLabel != null)
                _freeLimitLabel.text = canFree
                    ? $"오늘 {gacha.FreePullsToday}/{PetGachaTable.FreePullDailyLimit}회"
                    : "오늘 다 썼다 — 내일 다시";
            if (_freeBtn != null) _freeBtn.interactable = canFree;

            if (_advancedPityLabel != null)
                _advancedPityLabel.text = $"천장 {gacha.AdvancedOpenedSincePity}/{PetGachaTable.AdvancedPityCount}" +
                    (gacha.AdvancedFreePullClaimedToday ? " · 오늘 무료분 사용함" : " · 오늘 무료 1회 남음");

            var hasSeal = gacha.TranscendentSealCount >= 1;
            if (_specialPityLabel != null)
                _specialPityLabel.text = $"천장 {gacha.SpecialOpenedSincePity}/{PetGachaTable.SpecialPityCount} · " +
                    (hasSeal ? $"인장 {gacha.TranscendentSealCount}개 보유" : "인장 없음");
            if (_specialBtn != null) _specialBtn.interactable = hasSeal;
        }

        // btn-buy-seal-1/10 공통 배선 — 라벨에 CSV 가격을 채우고 DebugPurchase로 보낸다.
        // 목록에 SkuId가 없으면(shop.csv가 이 두 줄을 지워 버리는 사고 등) 조용히 꺼 둔다 —
        // ShopUgui의 "안 물린 버튼은 끈다" 규칙과 같은 태도.
        void WireSealButton(Button btn, List<ShopItem> items, ShopSkuId skuId)
        {
            if (btn == null) return;
            var index = items.FindIndex(i => i.SkuId == skuId);
            if (index < 0) { btn.interactable = false; return; }

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = $"{items[index].NameKo} ({items[index].PriceKrw:N0}원)";
            btn.onClick.AddListener(() => target?.DebugPurchase(skuId));
        }

        static int NextSeed() => Random.Range(int.MinValue, int.MaxValue);

        void OnFreeClicked()
        {
            if (target == null) return;
            var outcome = target.PullFreePet(NextSeed());
            if (outcome.Success) ShowSingleResult(outcome.Result);
        }

        void OnNormalClicked()
        {
            if (target == null) return;
            ShowSingleResult(target.PullNormalPet(NextSeed()));
        }

        void OnAdvancedClicked()
        {
            if (target == null) return;
            var useFreeDaily = !target.PetGacha.AdvancedFreePullClaimedToday;
            var outcome = target.PullAdvancedPet(NextSeed(), useFreeDaily);
            if (outcome.Success) ShowSingleResult(outcome.Result);
        }

        void OnAdvancedTenClicked()
        {
            if (target == null) return;
            ShowGridResult(target.PullAdvancedTenPet(NextSeed()));
        }

        void OnSpecialClicked()
        {
            if (target == null) return;
            var outcome = target.PullSpecialPet(NextSeed());
            if (outcome.Success) ShowSingleResult(outcome.Result);
        }

        void ShowSingleResult(PetGachaController.PetPullOutcome outcome)
        {
            if (_resultPanel == null) return;
            _resultPanel.SetActive(true);
            _resultSingle?.SetActive(true);
            _resultGrid?.SetActive(false);

            var def = PetSpeciesTable.Get(outcome.SpeciesId);
            UiKit.SetSpriteAtPath(transform, "result-portrait", PetArt.ResourcePath(def));
            if (_resultNameLabel != null)
                _resultNameLabel.text = $"{PetGradeInfo.NameKoFor(outcome.Grade)} · {PetSpeciesTable.DisplayNameKo(def)}";
            if (_resultNoteLabel != null)
                _resultNoteLabel.text = outcome.IsNewSpecies ? "새로운 종!" : "조각 +1 (이미 보유한 종)";
        }

        void ShowGridResult(PetGachaController.PetPullOutcome[] outcomes)
        {
            if (_resultPanel == null) return;
            _resultPanel.SetActive(true);
            _resultSingle?.SetActive(false);
            _resultGrid?.SetActive(true);

            var newCount = 0;
            for (var i = 0; i < _resultGridImages.Length; i++)
            {
                if (_resultGridImages[i] == null) continue;
                if (i >= outcomes.Length) { _resultGridImages[i].gameObject.SetActive(false); continue; }
                _resultGridImages[i].gameObject.SetActive(true);
                var def = PetSpeciesTable.Get(outcomes[i].SpeciesId);
                UiKit.SetSpriteAtPath(transform, $"result-portrait-{i}", PetArt.ResourcePath(def));
                if (outcomes[i].IsNewSpecies) newCount++;
            }

            if (_resultNameLabel != null) _resultNameLabel.text = "10연차 결과";
            if (_resultNoteLabel != null)
                _resultNoteLabel.text = $"새로운 종 {newCount}마리 · 중복 {outcomes.Length - newCount}마리 (조각으로 전환됨)";
        }
    }
}
