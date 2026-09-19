namespace GemRacer.Core
{
    /// <summary>
    /// P-03: 증폭기 — 상자에서 나와 채굴 장비·레이싱카 부품 칸에 쌓이는 성능 배율.
    /// docs/design/amplifier.md, T-10(2026-09-19, A안 확정) 참고.
    /// 등급은 LootTable과 같은 PartGrade(C/B/A/S)를 쓴다 — 상자가 이미 그 등급 확률표로 뽑고 있으니
    /// 증폭기도 같은 등급을 그대로 물려받는다(일반=C, 고급=B, 에픽=A, 전설=S).
    /// 증폭기는 **더하는 값**이고 깎이는 경우가 없다 — 곱이 아니라 합으로 쌓아야
    /// 전설 하나가 확 체감되면서도 숫자가 안 터진다(곱이면 전설 몇 개에 폭주한다).
    ///
    ///     최종 성능 = 기본 성능(레벨) × (1 + 그 칸에 쌓인 증폭률 합)
    ///
    /// 누적 상한(칸당 얼마까지 쌓일 수 있는지)은 아직 정해지지 않았다(amplifier.md "정해야 하는 것") —
    /// 이 파일은 상한을 강제하지 않는다. 상한이 정해지면 Sum을 받는 쪽(P-05, SaveData)에서 자른다.
    /// </summary>
    public static class Amplifier
    {
        // 등급 순서는 PartGrade와 같다: C(일반) B(고급) A(에픽) S(전설).
        static readonly float[] MinBonus = { 0.01f, 0.10f, 0.60f, 3.00f };
        static readonly float[] MaxBonus = { 0.05f, 0.25f, 1.20f, 9.00f };

        public static float MinBonusFor(PartGrade grade) => MinBonus[(int)grade];
        public static float MaxBonusFor(PartGrade grade) => MaxBonus[(int)grade];

        /// <summary>seed로 그 등급 범위 안에서 증폭률 하나를 뽑는다(서버 재검증용 재현 가능,
        /// CLAUDE.md 1번). [MinBonusFor, MaxBonusFor) 구간 균등분포.</summary>
        public static float Roll(PartGrade grade, int seed)
        {
            var rng = new DeterministicRandom(seed);
            var min = MinBonusFor(grade);
            var max = MaxBonusFor(grade);
            return min + rng.NextFloat() * (max - min);
        }

        /// <summary>기본 성능에 칸의 증폭률 합을 적용한다. 합(totalBonus)은 그 칸에 쌓인 증폭기들의
        /// Roll 결과를 더한 값 — 이 함수는 더하지 않고 이미 더해진 값을 받기만 한다.</summary>
        public static float Apply(float basePerformance, float totalBonus) => basePerformance * (1f + totalBonus);
    }
}
