namespace GemRacer.Core
{
    /// <summary>M-07: 상점에서 파는 것의 목록. 처음 아홉 개는 `Entitlements.PurchaseState`의
    /// 필드 이름과 1:1로 맞춰 뒀다 — 스킨(monetization.md 2-7)은 아직 종류가 정해지지 않아 뺐다.
    /// 시즌 패스 유료 트랙(2-6)은 M-14로 추가됐는데, 이건 PurchaseState가 아니라
    /// `SeasonPassState.OwnsPaidTrack`을 켜는 구매라 나머지 아홉 개와 다르게 취급된다 —
    /// `ShopPurchase.Apply`가 아니라 `MiningController.DebugPurchase`가 직접 처리한다
    /// (ShopPurchase.cs 주석 참고). 그래서 기존 아홉 개 뒤에 덧붙였다 — 중간에 끼워 넣으면
    /// ShopUgui.Prefixes가 인덱스로 맞춰 둔 나머지 줄이 전부 한 칸씩 밀린다.
    ///
    /// P-16: `TranscendentSeal1`/`TranscendentSeal10`(pet-gacha.md 3절 "인장 획득 경로" 표의
    /// "유료 구매" 줄, 500원/1장·4,500원/10장 — 실물결제 단가라 Tifania 결정 없이 그대로 옮길 수
    /// 있었다)도 `SeasonPassPaidTrack`과 같은 이유로 여기 맨 뒤에 붙는다 — `SaveData.PetGachaSave.
    /// TranscendentSealCount`를 늘리는 구매라 `PurchaseState`가 아니다.</summary>
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
        SeasonPassPaidTrack,
        TranscendentSeal1,
        TranscendentSeal10,
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
