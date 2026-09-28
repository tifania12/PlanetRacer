using System;

namespace GemRacer.Core
{
    /// <summary>E-05(2026-09-29, economy-v2.md 3-3): 레벨 500 상한을 지루하지 않게 만드는 두 장치.
    /// 순수 함수, Unity 의존 없음.
    ///
    /// **돌파(50레벨마다)**: 특수 강화석으로 50·100…450레벨 벽을 넘어야 다음 50레벨이 열린다
    /// (칸마다 9번). 일일 던전(E-08)이 아직 없어서 강화석 공급이 0이므로, 지금은 강화석이 생길
    /// 때까지 모든 칸이 50레벨에서 사실상 멈춘다 — economy-v2.md 3-3이 "일일 던전이 아직 없는
    /// 동안에는 50레벨이 사실상 상한"이라고 명시한 그대로다. 이 파일은 "틀"만 만든다 — 강화석
    /// 잔고(SaveData.EnhancementStones)를 실제로 채워 주는 건 E-08 몫.
    ///
    /// **이정표(25레벨마다)**: 그 칸의 성능이 자동으로 ×2(25·50·75…500, 칸마다 20번). 돌파 벽과
    /// 25레벨 간격으로 절반씩 겹친다(50·100…이 두 체계의 공통 지점) — 우연이 아니라 "돌파한 직후
    /// 성능도 확 뛴다"는 체감을 노린 설계다(economy-v2.md 3-4 기준 5 "이정표 ×2가 체감되는지").</summary>
    public static class Breakthrough
    {
        /// <summary>돌파 한 번의 레벨 폭.</summary>
        public const int Span = 50;

        /// <summary>돌파 횟수 — 50에서 500까지 벽 아홉 개(economy-v2.md 3-3 "칸마다 9번").</summary>
        public const int Count = 9;

        /// <summary>레벨 하드 상한. Span × (Count+1) = 500.</summary>
        public const int HardCap = Span * (Count + 1);

        /// <summary>k번째 돌파(k=1..9) 비용(강화석 개수, 반올림).
        /// economy-v2.md 3-3: 5×k^1.5 → 5·14·26·40·56·73·93·113·135(한 칸 합 555, 네 칸 2,220).</summary>
        public static int Cost(int k)
        {
            if (k < 1 || k > Count) throw new ArgumentOutOfRangeException(nameof(k), "돌파는 1~9번째만 있다");
            return (int)Math.Round(5.0 * Math.Pow(k, 1.5), MidpointRounding.AwayFromZero);
        }

        /// <summary>이미 끝낸 돌파 횟수(0~9)로 정해지는 그 칸의 지금 레벨 상한.
        /// 0회 = 50(일일 던전이 붙기 전 사실상 상한), 9회(전부) = 500.</summary>
        public static int EffectiveMaxLevel(int breakthroughsDone)
        {
            var b = breakthroughsDone < 0 ? 0 : breakthroughsDone > Count ? Count : breakthroughsDone;
            return Span * (b + 1);
        }

        /// <summary>지금 레벨의 이정표 성능 배율 — 25레벨마다 ×2, 20단(500/25).
        /// 레벨이 0 이하(예: 제련소 미보유)면 1을 돌려준다 — "아직 없음" 상태는 성능 계산 쪽이
        /// 따로 0으로 처리하므로 여기서 배율을 곱해도 0×1=0으로 안전하다.</summary>
        public static double MilestoneMultiplier(int level)
        {
            if (level <= 0) return 1.0;
            var tier = level / 25; // 정수 나눗셈(음수 아님) — 25=1단, 49=1단, 50=2단
            return Math.Pow(2.0, tier);
        }

        /// <summary>UpgradeCost의 보호 구간(ToolProtectedMaxLevel 등) 밖에서만 이정표를 적용하는
        /// 버전. 곡괭이·화물칸·엔진의 보호 구간(30레벨)이 25레벨 이정표 지점보다 넓어서, 절대
        /// 레벨(25)에 그대로 이정표를 끼우면 economy-v2.md 3-2가 못 박은 "1~30레벨은 지금 체감을
        /// 지킨다"(tempo.md 5절이 잠가 둔 값, Core.Tests의 "끝값 보존" 테스트)가 깨진다 — 그래서
        /// 보호 구간이 3-3의 25레벨 이정표보다 우선한다고 보고, 보호 구간 안에서는 이정표를 아예
        /// 끄고 보호 구간을 벗어난 첫 레벨부터 적용한다. 제련소는 보호 구간이 5레벨이라 25
        /// 이정표보다 훨씬 안쪽에서 끝나므로 이 게이트가 사실상 아무 영향이 없다(굳이 갈라 쓸
        /// 필요는 없지만 네 슬롯이 같은 함수를 쓰게 통일했다).</summary>
        public static double MilestoneMultiplierBeyond(int level, int protectedMaxLevel) =>
            level > protectedMaxLevel ? MilestoneMultiplier(level) : 1.0;
    }
}
