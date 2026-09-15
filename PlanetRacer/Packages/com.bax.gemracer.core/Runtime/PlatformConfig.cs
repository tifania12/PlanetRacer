namespace GemRacer.Core
{
    /// <summary>M-11: 모바일/Steam 판 갈림길. 대전 결과를 정하는 레이싱카 부품 스탯은 어느 쪽도
    /// 건드리지 않는다(docs/design/monetization.md 4장) — 갈리는 건 파는 것과 화물칸 기본값뿐이라
    /// 플래그 하나로 충분하다. 지금 어느 판인지는 빌드 쪽(Assets 글루)이 정해서 넘겨준다 — 코어는
    /// "지금 이 판이면 뭐가 다른가"만 안다.</summary>
    public enum StorePlatform
    {
        Mobile,
        Steam,
    }

    public static class PlatformConfig
    {
        // monetization.md 4장 "화물칸 기본 상한을 모바일보다 1.5배 넉넉하게" — 구매 여부와 무관하게
        // 판 자체가 다르다. Entitlements.CargoMultiplier(구매·구독)와는 독립이라 곱해서 합친다
        // (MiningController.CargoCapacityMinerals가 RewardAdBoost.CargoCapMultiplier를 곱하는 것과 같은 자리).
        const float SteamCargoBaseMultiplier = 1.5f;

        /// <summary>Planet.BaseCargoHours에 곱할 판별 배율. 모바일은 1(기본값 그대로).</summary>
        public static float CargoBaseMultiplier(StorePlatform platform)
            => platform == StorePlatform.Steam ? SteamCargoBaseMultiplier : 1f;

        /// <summary>이 SKU를 이 판의 상점에 보여줄지. monetization.md 4장 — Steam엔 광고가 없으니
        /// 광고 제거를 팔 것도 없고, 구독을 싫어하는 시장이라 구독 대신 서포터 팩 하나로 대신한다.
        /// 나머지 SKU(화물칸 확장·오프라인 연장·가속 패스·스타터 팩)는 두 판 다 그대로 판다.</summary>
        public static bool IsShopItemAvailable(ShopSkuId skuId, StorePlatform platform)
        {
            switch (skuId)
            {
                case ShopSkuId.AdRemoval:
                    return platform != StorePlatform.Steam;
                case ShopSkuId.SeasonPassSubscription:
                    return platform != StorePlatform.Steam;
                case ShopSkuId.SteamSupporterPack:
                    return platform == StorePlatform.Steam;
                default:
                    return true;
            }
        }
    }
}
