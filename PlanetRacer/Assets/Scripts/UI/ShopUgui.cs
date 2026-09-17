using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-10(2026-09-16): ShopPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// DefaultData.ShopItems()(=CSV, docs/design/balance/shop.csv)가 아홉 줄의 이름·가격을 주고,
    /// 여기는 화면 갱신과 클릭 전달만 한다. 실제 결제 SDK(P3)가 붙기 전이라 버튼을 누르면
    /// MiningController.DebugPurchase가 바로 적용되는 디버그 구매다.
    ///
    /// 아홉 줄은 DefaultData.ShopItems()가 돌려주는 순서(ShopSkuId enum 선언 순서)와
    /// 줄 순서(Prefixes)가 정확히 같다고 가정한다 — CraftingUgui가 AvailableParts와
    /// 다섯 줄을 맞추는 것과 같은 방식.
    /// </summary>
    public sealed class ShopUgui : MonoBehaviour
    {
        [Tooltip("구매 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        TMP_Text[] _nameLabels;
        TMP_Text[] _stateLabels;
        Button[] _buttons;
        TMP_Text[] _buttonLabels;

        static readonly string[] Prefixes =
        {
            "starter", "cargo1", "cargo2", "cargo3", "offlinecap", "accel", "season", "steam", "adremoval",
        };

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _nameLabels = new TMP_Text[Prefixes.Length];
            _stateLabels = new TMP_Text[Prefixes.Length];
            _buttons = new Button[Prefixes.Length];
            _buttonLabels = new TMP_Text[Prefixes.Length];

            for (int i = 0; i < Prefixes.Length; i++)
            {
                var p = Prefixes[i];
                _nameLabels[i] = UiKit.Find<TMP_Text>(transform, $"{p}-name");
                _stateLabels[i] = UiKit.Find<TMP_Text>(transform, $"{p}-state");
                _buttons[i] = UiKit.Find<Button>(transform, $"{p}-button");
                _buttonLabels[i] = _buttons[i] != null ? _buttons[i].GetComponentInChildren<TMP_Text>() : null;
            }

            // 이름·가격은 DefaultData.ShopItems()에서 읽는다 — UXML(부트스트랩)의 기본 문구는
            // 자리 표시자일 뿐이라 가격이 바뀌어도 여기서 자동으로 맞는다.
            var items = DefaultData.ShopItems();
            for (int i = 0; i < items.Count && i < _nameLabels.Length; i++)
            {
                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정
                var item = items[index];
                if (_nameLabels[index] != null) _nameLabels[index].text = item.NameKo;
                if (_buttonLabels[index] != null) _buttonLabels[index].text = $"구매 ({item.PriceKrw:N0}원)";
                _buttons[index]?.onClick.AddListener(() => OnBuyClicked(item.SkuId));
            }

            ApplyPlatform(items);
        }

        /// <summary>M-11: 이 판에 없는 SKU 줄을 통째로 감춘다(Steam엔 광고 제거·시즌 패스 구독이
        /// 없고, 모바일엔 Steam 서포터 팩이 없다 — monetization.md 4장).
        ///
        /// **목록에서 빼지 않고 줄을 끄는 이유**가 이 함수의 전부다. 위 Awake와 아래 Refresh는
        /// 아홉 줄의 순서(Prefixes)가 DefaultData.ShopItems()의 순서와 1:1로 같다고 못박고
        /// 인덱스로 값을 찍는다. 목록에서 SKU를 걸러 내면 그 뒤 줄들의 인덱스가 전부 한 칸씩
        /// 밀려서 엉뚱한 줄에 엉뚱한 이름·상태가 찍힌다. 그래서 배열은 아홉 개를 그대로 두고
        /// 화면에서만 끈다 — 인덱스는 하나도 안 움직인다.</summary>
        void ApplyPlatform(System.Collections.Generic.IReadOnlyList<ShopItem> items)
        {
            var platform = target != null ? target.Platform : GamePlatform.Build;

            for (int i = 0; i < items.Count && i < Prefixes.Length; i++)
            {
                var row = UiKit.Find<Transform>(transform, $"row-{Prefixes[i]}", warnIfMissing: false);
                if (row == null)
                {
                    Debug.LogWarning($"[GemRacer] 상점 줄 'row-{Prefixes[i]}'를 못 찾았다. " +
                                     "'GemRacer/22'로 다시 세워야 할 수 있다.");
                    continue;
                }

                // 끄는 쪽뿐 아니라 켜는 쪽도 같이 써 준다 — 몇 번을 돌려도 결과가 같게.
                row.gameObject.SetActive(PlatformConfig.IsShopItemAvailable(items[i].SkuId, platform));
            }
        }

        // 구독 만료·화물칸 단계는 시간이 지나면 저절로 바뀌니(예: 자정 넘어 구독 만료) 매 프레임 다시 그린다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;
            var purchases = target.Purchases;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            SetState(0, purchases.CargoExpansionLevel >= 1 ? "보유 중" : "미보유"); // StarterPack
            SetState(1, purchases.CargoExpansionLevel >= 1 ? "보유 중" : "미보유"); // CargoExpansion1
            SetState(2, purchases.CargoExpansionLevel >= 2 ? "보유 중" : "미보유"); // CargoExpansion2
            SetState(3, purchases.CargoExpansionLevel >= 3 ? "보유 중" : "미보유"); // CargoExpansion3
            SetState(4, purchases.OfflineCapExtensionPurchased ? "구매함" : "미구매"); // OfflineCapExtension
            SetState(5, ExpiryStateText(purchases.MiningAccelPassExpiryUnixSeconds, now)); // MiningAccelPass
            SetState(6, ExpiryStateText(purchases.SeasonPassSubscriptionExpiryUnixSeconds, now)); // SeasonPassSubscription
            SetState(7, purchases.SteamSupporterPackPurchased ? "보유 중" : "미보유"); // SteamSupporterPack
            SetState(8, purchases.AdRemovalPurchased ? "제거됨" : "표시 중"); // AdRemoval
        }

        void SetState(int index, string text)
        {
            if (index < _stateLabels.Length && _stateLabels[index] != null) _stateLabels[index].text = text;
        }

        static string ExpiryStateText(long? expiryUnixSeconds, long nowUnixSeconds)
        {
            if (!expiryUnixSeconds.HasValue || expiryUnixSeconds.Value <= nowUnixSeconds) return "비활성";
            var daysLeft = Math.Ceiling((expiryUnixSeconds.Value - nowUnixSeconds) / 86400.0);
            return $"활성 ({daysLeft:F0}일 남음)";
        }

        void OnBuyClicked(ShopSkuId skuId)
        {
            target?.DebugPurchase(skuId);
        }
    }
}
