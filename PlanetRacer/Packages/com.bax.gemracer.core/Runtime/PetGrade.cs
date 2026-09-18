namespace GemRacer.Core
{
    /// <summary>P-12: 펫 등급 7단계(docs/design/pet-gacha.md 2절). 숫자가 클수록 희귀하다 —
    /// PetGachaTable의 10연차 최소 등급 비교(&gt;=)가 이 순서에 기댄다.</summary>
    public enum PetGrade { Common, Advanced, Rare, Epic, Legendary, Mythic, Transcendent }

    /// <summary>등급별 고정값(종 수·도감 보너스). pet-gacha.md 2절 표를 그대로 옮긴 것 —
    /// 장착 효과·6·7등급 고유 효과는 여기 없다(PetCollection.cs, 아직 없음, P-14 몫).</summary>
    public static class PetGradeInfo
    {
        public static readonly string[] NameKo = { "일반", "고급", "희귀", "영웅", "전설", "신화", "초월" };

        /// <summary>등급별 종 수. 합이 112(2절 "합계 112종").</summary>
        public static readonly int[] SpeciesCount = { 12, 12, 14, 16, 18, 30, 10 };

        /// <summary>도감 보너스, 마리당(같은 종 여러 마리를 도감에 채웠다는 뜻이 아니라 등급 내
        /// "가진 종" 하나당). 예: 일반 한 종을 가지고 있으면 +1%, 두 종이면 +2%.</summary>
        public static readonly float[] CollectionBonusPerSpecies = { 0.01f, 0.02f, 0.04f, 0.07f, 0.12f, 0.20f, 0.35f };

        public const int TotalSpeciesCount = 112;

        public static string NameKoFor(PetGrade grade) => NameKo[(int)grade];
        public static int SpeciesCountFor(PetGrade grade) => SpeciesCount[(int)grade];
        public static float CollectionBonusFor(PetGrade grade) => CollectionBonusPerSpecies[(int)grade];
    }
}
