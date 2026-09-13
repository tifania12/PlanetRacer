namespace GemRacer.Core
{
    /// <summary>D11-N 후속: 레이스 등급(RaceTier)이 우승 시 어떤 공구 상자를 주는지의 매핑.
    /// docs/GDD.md "레이스" 항목 그대로 — 로컬=녹슨, 서킷=강철, 챌린지=티타늄. 그랑프리는 상자가
    /// 아니라 "워프"라는 별도 보상이라(GDD) 여기서 다루지 않는다(null).
    ///
    /// 쿼츠 로컬 레이스 3개는 이미 L-03 결정으로 코스별 고정 RigPartReward를 직접 준다
    /// (DefaultData.QuartzLocalRaceRewards, 슬롯을 하나씩 돌아가며 확정 지급 — 온보딩 예측 가능성
    /// 때문에 일부러 그렇게 했다). 이 매핑은 그것과 겹쳐서 "로컬 레이스는 확정 슬롯 보상 + 녹슨
    /// 상자 1개"가 되게 한다 — GDD에 있던 상자 보상(로컬=녹슨)이 지금까지 코드 어디에도 실제로
    /// 반영돼 있지 않았던 걸 채우는 것뿐이고, 기존 확정 슬롯 보상을 바꾸거나 대체하지 않는다.
    ///
    /// 서킷·챌린지 코스 자체는 아직 없다(docs/backlog.md W2 "레이스 4등급 해금 구조"에서 함께
    /// 만들 예정) — 코스가 생겨서 Tier를 Circuit/Challenge로 채우기만 하면 이 매핑이 그대로
    /// 작동한다, 코드를 더 안 고쳐도 된다.</summary>
    public static class RaceBoxReward
    {
        public static LootBoxType? ForTier(RaceTier tier) => tier switch
        {
            RaceTier.Local => LootBoxType.Rusty,
            RaceTier.Circuit => LootBoxType.Steel,
            RaceTier.Challenge => LootBoxType.Titanium,
            RaceTier.GrandPrix => null,
            _ => null,
        };
    }
}
