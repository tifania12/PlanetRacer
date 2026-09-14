using System;

namespace GemRacer.Core
{
    /// <summary>M-07: skuId 하나를 산 결과를 PurchaseState에 반영하는 순수 함수. "환불 없음, 중복
    /// 구매는 손해 안 보게"라는 정책을 여기 한 곳에 모아 둔다 — 상점 화면(다음 세션, 에디터 필요)은
    /// 이 함수만 부르면 된다. 실제 결제(영수증 검증)는 이 함수 밖의 일이다 — 여기는 "결제가 이미
    /// 성공했다"는 걸 상태에 반영하는 마지막 단계만 맡는다.</summary>
    public static class ShopPurchase
    {
        // monetization.md 2-4/2-5 — 가속 패스 30일, 통행증 구독 "월". 서버 캘린더 개념이 없어서
        // 30일로 근사한다(다른 코어 코드도 "월"을 실제 달력으로 다루지 않는다).
        const long AccelPassDurationSeconds = 30L * 24 * 3600;
        const long SubscriptionDurationSeconds = 30L * 24 * 3600;

        /// <summary>skuId를 nowUnixSeconds에 산 것으로 반영한 새 PurchaseState를 돌려준다(state
        /// 자체는 값 타입이라 원본은 안 바뀐다, 호출부가 반환값을 다시 저장해야 한다).
        /// 영구 항목(화물칸 확장 등)은 이미 산 것보다 낮은 단계를 다시 사도 단계가 안 내려간다
        /// (Math.Max) — 실수로 낮은 SKU를 눌러도 손해가 없어야 한다. 기간제(가속 패스·구독)는
        /// 이미 활성 중이면 지금이 아니라 만료 시각부터 기간을 이어 붙인다 — 안 그러면 미리 사 둔
        /// 사람이 남은 기간을 버리게 된다.</summary>
        public static PurchaseState Apply(PurchaseState state, ShopSkuId skuId, long nowUnixSeconds)
        {
            switch (skuId)
            {
                case ShopSkuId.StarterPack:
                    // 스타터 팩(monetization.md 2-1)의 화물칸 확장 1단계만 PurchaseState에 남는다.
                    // 스킨 1종·정제 광물 소량은 소비성 지급이라 이 상태와는 별개(TODO — 지급 로직은
                    // 상점 화면이 생길 때 같이, 스킨 시스템 자체가 아직 없다).
                    state.CargoExpansionLevel = Math.Max(state.CargoExpansionLevel, 1);
                    return state;
                case ShopSkuId.CargoExpansion1:
                    state.CargoExpansionLevel = Math.Max(state.CargoExpansionLevel, 1);
                    return state;
                case ShopSkuId.CargoExpansion2:
                    state.CargoExpansionLevel = Math.Max(state.CargoExpansionLevel, 2);
                    return state;
                case ShopSkuId.CargoExpansion3:
                    state.CargoExpansionLevel = Math.Max(state.CargoExpansionLevel, 3);
                    return state;
                case ShopSkuId.OfflineCapExtension:
                    state.OfflineCapExtensionPurchased = true;
                    return state;
                case ShopSkuId.MiningAccelPass:
                    state.MiningAccelPassExpiryUnixSeconds =
                        ExtendFrom(state.MiningAccelPassExpiryUnixSeconds, nowUnixSeconds, AccelPassDurationSeconds);
                    return state;
                case ShopSkuId.SeasonPassSubscription:
                    state.SeasonPassSubscriptionExpiryUnixSeconds =
                        ExtendFrom(state.SeasonPassSubscriptionExpiryUnixSeconds, nowUnixSeconds, SubscriptionDurationSeconds);
                    return state;
                case ShopSkuId.SteamSupporterPack:
                    state.SteamSupporterPackPurchased = true;
                    return state;
                case ShopSkuId.AdRemoval:
                    state.AdRemovalPurchased = true;
                    return state;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skuId), skuId, "M-07: 이 SKU는 아직 ShopPurchase.Apply가 모른다");
            }
        }

        static long ExtendFrom(long? currentExpiry, long nowUnixSeconds, long durationSeconds)
        {
            var basis = currentExpiry.HasValue && currentExpiry.Value > nowUnixSeconds
                ? currentExpiry.Value
                : nowUnixSeconds;
            return basis + durationSeconds;
        }
    }
}
