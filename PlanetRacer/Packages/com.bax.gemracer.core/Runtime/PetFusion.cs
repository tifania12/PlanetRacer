using System;

namespace GemRacer.Core
{
    /// <summary>
    /// P-13: 펫 중복 조각 환산(docs/design/pet-gacha.md 2절 "중복 → 상위 등급"표). 같은 펫이
    /// 또 나오면 조각이 되고, 조각은 버려지지 않는다 — 이 파일은 조각 몇 개로 무엇을 바꿀 수
    /// 있는지(같은 등급 다른 펫 / 상위 등급 승급)만 정의한다. 실제로 몇 개를 갖고 있는지, 바꾼
    /// 뒤 어떤 펫을 받을지 고르는 것은 SaveData·화면 몫(P-14)이다.
    /// </summary>
    public static class PetFusion
    {
        /// <summary>같은 등급 조각 3개 → 그 등급의 다른 펫 하나(2절 표). 모든 등급에 공통이다.</summary>
        public const int SameGradeFragmentCost = 3;

        /// <summary>grade에서 한 등급 위로 승급하는 데 필요한 조각 수. 1~4등급(Common~Epic)은 5,
        /// 5등급(Legendary)은 8, 6등급(Mythic)은 15 — 2절 표 그대로("6→7이 15인 건 초월이 합성으로도
        /// 도달 가능해야 하기 때문"). 배열 길이가 6이라 Transcendent(7등급, 더 위가 없음)는 여기 없다.</summary>
        private static readonly int[] PromotionCostByGrade = { 5, 5, 5, 5, 8, 15 };

        /// <summary>grade에서 한 등급 위로 승급하는 데 필요한 조각 수. Transcendent(초월)는 더 위
        /// 등급이 없어 승급 대상이 아니다 — 예외를 던진다.</summary>
        public static int PromotionCost(PetGrade grade)
        {
            if (grade == PetGrade.Transcendent)
                throw new ArgumentException("초월(7등급)은 더 위 등급이 없어 승급할 수 없다.");
            return PromotionCostByGrade[(int)grade];
        }

        /// <summary>grade에서 한 등급 위로 승급했을 때의 결과 등급.</summary>
        public static PetGrade PromotedGrade(PetGrade grade) => grade + 1;

        /// <summary>grade 등급 조각 fragments개로 "같은 등급 다른 펫"을 몇 마리까지 바꿀 수 있는지와
        /// 남는 조각 수. 조각은 버려지지 않는다 — 나머지가 그대로 돌려주는 값이다(호출부가 SaveData에
        /// 되돌려 쓴다).</summary>
        public static (int petsGained, int remainingFragments) ExchangeForSameGrade(int fragments)
        {
            if (fragments < 0) throw new ArgumentException("조각 수는 음수일 수 없다.");
            return (fragments / SameGradeFragmentCost, fragments % SameGradeFragmentCost);
        }

        /// <summary>grade 등급 조각 fragments개로 몇 번 승급할 수 있는지(승급마다 PromotionCost(grade)개를
        /// 소모해 위 등급 조각 1개가 된다)와 남는 조각 수.</summary>
        public static (int promotions, int remainingFragments) ExchangeForPromotion(PetGrade grade, int fragments)
        {
            if (fragments < 0) throw new ArgumentException("조각 수는 음수일 수 없다.");
            var cost = PromotionCost(grade);
            return (fragments / cost, fragments % cost);
        }
    }
}
