using System;

namespace GemRacer.Core
{
    /// <summary>P-14: 도감 보너스 합산 + 장착 보너스(docs/design/pet-gacha.md 2·8절).
    /// 6·7등급(신화·초월) 고유 효과는 여기 없다 — 종 48개(신화 30 + 초월 10)의 이름·효과가 아직
    /// 안 정해져서다(art-requests.md, 밤 세션이 혼자 정할 일이 아니라고 이미 판단됨). 정해지면
    /// 이 클래스에 등급 대신 종 ID로 찾는 고유 효과 조회를 더한다. 지금은 도감 보너스(1~7등급 전부,
    /// "가진 종 수 × 등급별 수치"라 이미 계산 가능)와 장착 보너스(1~5등급만, 6·7등급은 예외)까지.</summary>
    public static class PetCollection
    {
        /// <summary>도감 보너스 합계. ownedSpeciesCountByGrade[grade]는 그 등급에서 "가진 종"의 수
        /// (같은 종 여러 마리가 아니라 몇 종을 도감에 채웠는지 — 2절 "마리당"의 뜻). 배열 길이는
        /// 반드시 등급 수(7)와 같아야 하고, 각 칸은 0 이상 그 등급의 SpeciesCountFor 이하여야 한다.</summary>
        public static float CollectionBonus(int[] ownedSpeciesCountByGrade)
        {
            if (ownedSpeciesCountByGrade == null)
                throw new ArgumentNullException(nameof(ownedSpeciesCountByGrade));
            if (ownedSpeciesCountByGrade.Length != PetGradeInfo.NameKo.Length)
                throw new ArgumentException(
                    $"등급 수({PetGradeInfo.NameKo.Length})만큼 있어야 한다, 실제 {ownedSpeciesCountByGrade.Length}");

            var total = 0f;
            for (var i = 0; i < ownedSpeciesCountByGrade.Length; i++)
            {
                var grade = (PetGrade)i;
                var count = ownedSpeciesCountByGrade[i];
                if (count < 0)
                    throw new ArgumentException($"보유 종 수는 음수일 수 없다({PetGradeInfo.NameKoFor(grade)}, {count})");
                var maxForGrade = PetGradeInfo.SpeciesCountFor(grade);
                if (count > maxForGrade)
                    throw new ArgumentException(
                        $"{PetGradeInfo.NameKoFor(grade)}은 종이 {maxForGrade}개뿐인데 {count}개를 가졌다고 한다");
                total += PetGradeInfo.CollectionBonusFor(grade) * count;
            }
            return total;
        }

        /// <summary>한 마리를 장착했을 때의 보너스. 일반~전설(1~5등급)만 값을 준다 — 신화·초월은
        /// PetGradeInfo.EquipBonusFor 그대로 예외를 던진다(고유 효과 미정, 위 클래스 설명 참고).</summary>
        public static float EquipBonus(PetGrade grade) => PetGradeInfo.EquipBonusFor(grade);
    }
}
