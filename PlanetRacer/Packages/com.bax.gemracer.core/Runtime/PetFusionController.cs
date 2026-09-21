namespace GemRacer.Core
{
    /// <summary>
    /// A-17 이어서(범위 밖으로 미뤄 뒀던 "조각 합성 실행 화면"의 코어 쪽): PetFusion(조각 환산
    /// 규칙)을 실제로 SaveData에 반영한다. PetGachaController가 "뽑기 결과"를 세이브에 반영하듯,
    /// 이쪽은 "조각 교환"을 반영한다 — 둘 다 재화(조각)를 내는 쪽 확인은 하지 않는다(조각이
    /// 이미 SaveData에 있는 만큼만 교환하므로 별도 확인이 필요 없다, PetGachaController 클래스
    /// 주석과 같은 책임 분리).
    /// </summary>
    public static class PetFusionController
    {
        public struct SameGradeFusionResult
        {
            public PetGachaController.PetPullOutcome[] Pets;
            public int RemainingFragments;
        }

        /// <summary>grade 조각을 최대한 "같은 등급 다른 펫"으로 바꾼다(pet-gacha.md 2절 표,
        /// PetFusion.SameGradeFragmentCost개당 1마리). 종 선택은 PetGachaController.ResolveFusedSpecies
        /// 몫이다 — 뽑기와 같은 규칙을 쓰므로 결과가 이미 보유한 종이면 조각 1개로 자동 전환된다.
        /// 조각이 SameGradeFragmentCost개 미만이면 Pets는 빈 배열이고 RemainingFragments는
        /// 그대로(아무 일도 안 일어남).</summary>
        public static SameGradeFusionResult FuseSameGrade(SaveData save, PetGrade grade, int seed)
        {
            var (count, remaining) = PetFusion.ExchangeForSameGrade(save.PetGacha.Shards(grade));
            save.PetGacha.SetShards(grade, remaining);

            var pets = new PetGachaController.PetPullOutcome[count];
            for (var i = 0; i < count; i++)
                pets[i] = PetGachaController.ResolveFusedSpecies(save, grade, seed + i);

            return new SameGradeFusionResult { Pets = pets, RemainingFragments = remaining };
        }

        public struct PromotionFusionResult
        {
            public int Promotions;
            public int RemainingFragments;
        }

        /// <summary>grade 조각을 최대한 한 등급 위 조각으로 승급시킨다(PetFusion.PromotionCost(grade)개당
        /// 위 등급 조각 1개). 승급된 조각은 곧바로 위 등급의 ShardsByGrade에 쌓인다 — 실제 펫이 되려면
        /// 그 등급에서 다시 FuseSameGrade를 불러야 한다(2단계 경로, pet-gacha.md 2절 표 그대로:
        /// 예를 들어 1등급 조각 5개 → 2등급 조각 1개, 2등급 조각이 3개 모이면 그때 2등급 펫 1마리).
        /// grade가 Transcendent(7등급)면 더 위가 없어 PetFusion.PromotionCost가 예외를 던진다 —
        /// 화면(호출부)이 승급 버튼 자체를 7등급에서 숨기는 것으로 막아야 한다.</summary>
        public static PromotionFusionResult FusePromotion(SaveData save, PetGrade grade)
        {
            var (promotions, remaining) = PetFusion.ExchangeForPromotion(grade, save.PetGacha.Shards(grade));
            save.PetGacha.SetShards(grade, remaining);
            if (promotions > 0)
                save.PetGacha.AddShards(PetFusion.PromotedGrade(grade), promotions);

            return new PromotionFusionResult { Promotions = promotions, RemainingFragments = remaining };
        }
    }
}
