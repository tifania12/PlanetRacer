using System;
using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Core;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>M-07: 상점 화면. CraftingPanel·LootBoxPanel과 같은 역할 분담 — 코어
    /// (DefaultData.ShopItems, ShopPurchase.Apply, Entitlements)가 목록·가격·구매 반영을 전부
    /// 계산하고, 여기는 화면 갱신과 클릭 전달만 한다. 실제 결제 SDK(영수증 검증)는 P3 몫이라
    /// 지금은 버튼을 누르면 MiningController.DebugPurchase가 바로 ShopPurchase.Apply를 적용하는
    /// 디버그 구매다 — 화면 제목에도 "테스트 구매"라고 박아 뒀다.
    ///
    /// 아홉 줄은 DefaultData.ShopItems()가 돌려주는 순서(ShopSkuId enum 선언 순서, ShopCatalog.cs
    /// 참고)와 Shop.uxml의 줄 순서가 정확히 같다고 가정한다 — CraftingPanel이 AvailableParts와
    /// 다섯 줄을 맞추는 것과 같은 방식.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ShopPanel : MonoBehaviour
    {
        [Tooltip("구매 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        VisualElement _root;
        Label[] _nameLabels;
        Label[] _stateLabels;
        Button[] _buttons;
        Button _closeButton;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _nameLabels = new[]
            {
                _root.Q<Label>("starter-name"), _root.Q<Label>("cargo1-name"), _root.Q<Label>("cargo2-name"),
                _root.Q<Label>("cargo3-name"), _root.Q<Label>("offlinecap-name"), _root.Q<Label>("accel-name"),
                _root.Q<Label>("season-name"), _root.Q<Label>("steam-name"), _root.Q<Label>("adremoval-name"),
            };
            _stateLabels = new[]
            {
                _root.Q<Label>("starter-state"), _root.Q<Label>("cargo1-state"), _root.Q<Label>("cargo2-state"),
                _root.Q<Label>("cargo3-state"), _root.Q<Label>("offlinecap-state"), _root.Q<Label>("accel-state"),
                _root.Q<Label>("season-state"), _root.Q<Label>("steam-state"), _root.Q<Label>("adremoval-state"),
            };
            _buttons = new[]
            {
                _root.Q<Button>("starter-button"), _root.Q<Button>("cargo1-button"), _root.Q<Button>("cargo2-button"),
                _root.Q<Button>("cargo3-button"), _root.Q<Button>("offlinecap-button"), _root.Q<Button>("accel-button"),
                _root.Q<Button>("season-button"), _root.Q<Button>("steam-button"), _root.Q<Button>("adremoval-button"),
            };
            _closeButton = _root.Q<Button>("close-button");

            // 이름·가격은 DefaultData.ShopItems()(=CSV, docs/design/balance/shop.csv)에서 읽는다 —
            // UXML의 기본 문구는 자리 표시자일 뿐이라 가격이 바뀌어도 여기서 자동으로 맞는다.
            var items = DefaultData.ShopItems();
            for (int i = 0; i < items.Count && i < _nameLabels.Length; i++)
            {
                var index = i; // 람다가 반복 변수를 그대로 캡처하지 않게 지역 변수로 고정(다른 패널과 같은 이유)
                var item = items[index];
                _nameLabels[index].text = item.NameKo;
                _buttons[index].text = $"구매 ({item.PriceKrw:N0}원)";
                _buttons[index].clicked += () => OnBuyClicked(item.SkuId);
            }

            _closeButton.clicked += Close;

            ApplyPlatform(items);
            Refresh();
        }

        /// <summary>M-11: 이 판에 없는 SKU 줄을 통째로 감춘다 — ShopUgui.ApplyPlatform과 같은
        /// 이유로 목록에서 빼지 않고 줄만 끈다(아홉 줄이 인덱스로 고정돼 있어 걸러 내면 뒤가 밀린다).
        /// 여기는 UI Toolkit 쪽 옛 화면이라 display를 None으로 준다 — Visibility.Hidden과 달리
        /// 자리까지 접혀서 uGUI의 SetActive(false)와 같은 결과가 된다.</summary>
        void ApplyPlatform(System.Collections.Generic.IReadOnlyList<ShopItem> items)
        {
            var platform = target != null ? target.Platform : GamePlatform.Build;

            for (int i = 0; i < items.Count && i < RowNames.Length; i++)
            {
                var row = _root.Q<VisualElement>(RowNames[i]);
                if (row == null) continue;

                // OnEnable마다 다시 도니 켜는 쪽도 같이 써 준다 — 그래야 몇 번을 열고 닫아도
                // 결과가 같다(부트스트랩 메뉴를 멱등하게 만드는 것과 같은 이유).
                row.style.display = PlatformConfig.IsShopItemAvailable(items[i].SkuId, platform)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        static readonly string[] RowNames =
        {
            "row-starter", "row-cargo1", "row-cargo2", "row-cargo3", "row-offlinecap",
            "row-accel", "row-season", "row-steam", "row-adremoval",
        };

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
            if (index < _stateLabels.Length) _stateLabels[index].text = text;
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

        void Close() => _root.style.display = DisplayStyle.None;
    }
}
