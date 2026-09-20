using System;

namespace GemRacer.Core
{
    /// <summary>P-12: 펫 등급 7단계(docs/design/pet-gacha.md 2절). 숫자가 클수록 희귀하다 —
    /// PetGachaTable의 10연차 최소 등급 비교(&gt;=)가 이 순서에 기댄다.</summary>
    public enum PetGrade { Common, Advanced, Rare, Epic, Legendary, Mythic, Transcendent }

    /// <summary>등급별 고정값(종 수·도감 보너스·장착 효과). pet-gacha.md 2절 표를 그대로 옮긴 것 —
    /// 6·7등급 고유 효과는 여기 없다(종 48개의 이름·효과가 아직 안 정해짐, PetCollection.cs P-14 몫).</summary>
    public static class PetGradeInfo
    {
        public static readonly string[] NameKo = { "일반", "고급", "희귀", "영웅", "전설", "신화", "초월" };

        /// <summary>등급별 종 수. 합이 124(2절 "합계 124종", 2026-09-19 T-12 해결 — 계열 4로
        /// 나눠떨어지게 12/12/14/16/18에서 16/16/16/16/20으로 고쳤다. 6·7등급은 그대로).</summary>
        public static readonly int[] SpeciesCount = { 16, 16, 16, 16, 20, 30, 10 };

        /// <summary>도감 보너스, 마리당(같은 종 여러 마리를 도감에 채웠다는 뜻이 아니라 등급 내
        /// "가진 종" 하나당). 예: 일반 한 종을 가지고 있으면 +1%, 두 종이면 +2%.</summary>
        public static readonly float[] CollectionBonusPerSpecies = { 0.01f, 0.02f, 0.04f, 0.07f, 0.12f, 0.20f, 0.35f };

        /// <summary>장착 효과(그 한 마리를 태웠을 때). 일반~전설(1~5등급)까지만 있다 — 신화·초월은
        /// 수치 %가 아니라 종마다 다른 고유 효과라 이 배열엔 없다. EquipBonusFor가 그 둘을 걸러 예외를 던진다.</summary>
        private static readonly float[] EquipBonusForNumericGrades = { 0.08f, 0.14f, 0.22f, 0.35f, 0.55f };

        public const int TotalSpeciesCount = 124;

        public static string NameKoFor(PetGrade grade) => NameKo[(int)grade];
        public static int SpeciesCountFor(PetGrade grade) => SpeciesCount[(int)grade];
        public static float CollectionBonusFor(PetGrade grade) => CollectionBonusPerSpecies[(int)grade];

        /// <summary>grade 한 마리를 장착했을 때의 보너스. 신화·초월은 고유 효과(종별로 다름, 아직
        /// 안 정해짐)라 예외를 던진다 — PetFusion.PromotionCost가 초월에서 예외 던지는 것과 같은 자리다.</summary>
        public static float EquipBonusFor(PetGrade grade)
        {
            if (grade == PetGrade.Mythic || grade == PetGrade.Transcendent)
                throw new ArgumentException(
                    $"{NameKoFor(grade)}({grade})은 수치형 장착 효과가 아니라 종별 고유 효과다 — " +
                    "고유 효과 48종(신화 30·초월 10)의 이름과 값이 아직 안 정해져 값을 줄 수 없다.");
            return EquipBonusForNumericGrades[(int)grade];
        }
    }
}
