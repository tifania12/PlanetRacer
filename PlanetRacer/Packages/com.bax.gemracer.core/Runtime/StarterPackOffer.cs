namespace GemRacer.Core
{
    /// <summary>M-08: 스타터 팩(monetization.md 2-1) 노출 판정. "노출은 첫 상한 도달 직후 한 번" —
    /// 한 번 보여준 뒤에는 사든 거절하든 다시 안 띄운다. 실제 팝업을 그리는 건 Assets 쪽
    /// (CargoFullPanel) 몫이고, 여기는 "지금 보여줘야 하는가" 한 가지만 순수하게 판정한다.</summary>
    public static class StarterPackOffer
    {
        /// <summary>
        /// hasReachedCargoCapBefore: 화물칸이 상한에 한 번이라도 닿은 적이 있는가. 이번 세션의
        /// 엣지 트리거(MiningController.CargoJustFilled)와는 다르게 세이브에 영구히 남는 값이다 —
        /// 그래야 다음 접속에서 상한에 안 닿아도(이미 닿았던 적이 있으면) 계속 "안 보여줌" 판정을
        /// 유지할 수 있다.
        /// declined: 이미 이 제안을 거절(닫기)한 적이 있는가.
        /// cargoExpansionLevel: PurchaseState.CargoExpansionLevel. 스타터 팩이든 화물칸 확장 SKU를
        /// 따로 샀든 1단계 이상이면 이미 같은 효과를 가지고 있다(ShopPurchase.cs — 둘 다 이 필드를
        /// 올린다) — 더 보여줄 이유가 없다.
        /// </summary>
        public static bool ShouldShow(bool hasReachedCargoCapBefore, bool declined, int cargoExpansionLevel)
        {
            return hasReachedCargoCapBefore && !declined && cargoExpansionLevel <= 0;
        }
    }
}
