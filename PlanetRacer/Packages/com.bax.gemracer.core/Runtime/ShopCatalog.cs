namespace GemRacer.Core
{
    /// <summary>M-07: 상점에서 파는 것의 목록. `Entitlements.PurchaseState`의 필드 이름과 1:1로
    /// 맞춰 뒀다 — 스킨(monetization.md 2-7)은 아직 종류가 정해지지 않아 뺐고, 시즌 패스(2-6)도
    /// 레벨·트랙 구조가 따로 필요해서 뺐다. 지금 여기 있는 아홉 개는 전부 Entitlements가 이미
    /// 계산할 줄 아는 것들이다.</summary>
    public enum ShopSkuId
    {
        StarterPack,
        CargoExpansion1,
        CargoExpansion2,
        CargoExpansion3,
        OfflineCapExtension,
        MiningAccelPass,
        SeasonPassSubscription,
        SteamSupporterPack,
        AdRemoval,
    }

    /// <summary>상점 화면이 그대로 그릴 수 있는 한 줄. 가격은 CSV(docs/design/balance/shop.csv)에서
    /// 읽는다(backlog M-07) — 가격만 바꿀 때는 코드를 다시 안 빌드해도 된다.</summary>
    public struct ShopItem
    {
        public ShopSkuId SkuId;
        public string NameKo;
        public int PriceKrw;
    }
}
