using System;

namespace GemRacer.Core
{
    /// <summary>업그레이드 화면에 노출하는 장비. 채굴차 슬롯은 다섯 개(RigParts.cs RigSlot)인데
    /// Detector는 아직 쓰는 화면이 없어서(탐지 기능 미구현) 뺐다.
    ///
    /// Refinery는 2026-09-17에 다시 넣었다. 그 전까지는 "쓰는 화면이 없다"고 빼 뒀는데,
    /// M-02(커밋 a62b48c)가 업그레이드·제작 비용을 원석에서 정제 광물로 옮기면서 막다른 길이 됐다 —
    /// 정제량은 MiningSimulator.RefinePerHour가 제련소 레벨에 비례하고 시작 레벨이 0이라
    /// 정제 광물이 영원히 0이고, 제련소를 올릴 길은 레이스·상자의 무작위 보상뿐이었다.
    /// 그래서 원석만 쌓이고 업그레이드도 제작도 아무것도 안 눌리는 상태가 사흘 갔다.
    /// 이제 제련소는 이 화면에서 **원석으로** 산다(IsPaidWithRawMinerals). 진행이
    /// 원석 → 제련소 → 정제 광물 → 나머지 업그레이드로 이어진다.</summary>
    public enum UpgradeSlot { Tool, Cargo, Engine, Refinery }

    /// <summary>
    /// 채굴 장비 업그레이드 비용. 순수 함수, Unity 의존 없음.
    /// docs/design/core-loop.md "아직 안 정한 것" — "체감 효과는 강화 비용을 지수로 올려서 넣는다"를
    /// 여기서 구현한다. 상호 강화 나선이라 산출은 계속 오르므로, 다음 레벨 비용도 계속 올라야
    /// 무한정 가속되지 않는다(L-05 봇 시뮬레이션이 실제로 그런지 확인).
    /// </summary>
    public static class UpgradeCost
    {
        // E-05(2026-09-29, economy-v2.md 3-3): 30/30/30/5 → 500 전부 동일. 실제로 살 수 있는 레벨은
        // 이 하드 상한이 아니라 돌파 횟수로 더 낮게 묶인다 — EffectiveMaxLevel 참고. RigParts.cs·
        // MiningSimulator.cs의 Clamp가 이 상수를 그대로 참조하므로 여기만 바꾸면 같이 늘어난다.
        public const int ToolMaxLevel = Breakthrough.HardCap;
        public const int CargoMaxLevel = Breakthrough.HardCap;
        public const int EngineMaxLevel = Breakthrough.HardCap;
        public const int RefineryMaxLevel = Breakthrough.HardCap;

        // 보호 구간 — E-05 이전 공식이 그대로 쓰이는 마지막 레벨. tempo.md 5절이 실측해 둔 초반
        // 체감(첫 정제 광물 구매 0.20h·제련소 5레벨 1.75h)이 걸린 구간이라 이 안쪽 공식은 한 글자도
        // 안 바꾼다 — 레벨이 이 값 미만일 때만 옛 식을 쓰고, 그 이상은 ExtendedCostGrowth로 이어 붙인다.
        // public인 이유: MiningSimulator.cs가 25레벨 이정표(Breakthrough.MilestoneMultiplier)를
        // 이 경계 밖에서만 곱하려고 같은 값을 참조한다(아래 "이정표는 보호 구간 밖에서만" 참고).
        public const int ToolProtectedMaxLevel = 30;
        public const int CargoProtectedMaxLevel = 30;
        public const int EngineProtectedMaxLevel = 30;
        public const int RefineryProtectedMaxLevel = 5;

        // economy-v2.md 3-4: "시작값으로는 레벨당 비용 ×1.06~1.07(30레벨 뒤)을 권한다" — 권장 구간의
        // 중간값. 네 칸 전부 같은 성장률을 쓴다(칸마다 다른 값을 고를 근거가 아직 없다 — BalanceSim으로
        // 기준 5개가 안 맞으면 그때 칸별로 갈라도 된다, decisions.md).
        const double ExtendedCostGrowth = 1.065;

        /// <summary>이 슬롯을 원석(RawMinerals)으로 사는가. 제련소만 그렇다 — 정제 광물을 만드는
        /// 장치를 정제 광물로 사게 하면 첫 구매가 불가능해지기 때문이다. 나머지는 정제 광물로 산다.</summary>
        public static bool IsPaidWithRawMinerals(UpgradeSlot slot) => slot == UpgradeSlot.Refinery;

        /// <summary>다음 레벨로 올리는 데 드는 정제 광물. 이미 (돌파 횟수 기준) 최대 레벨이면
        /// 오를 수 없다는 뜻으로 double.PositiveInfinity를 돌려준다(UI가 버튼을 비활성화하는 신호로 쓴다).
        /// E-01(2026-09-28): float→double — 레벨 상한이 500까지 올라가면(economy-v2.md 3-3) 이
        /// 지수식 비용이 float 정밀도(유효숫자 7자리)를 넘는다.</summary>
        public static double Cost(UpgradeSlot slot, MiningRig rig)
        {
            if (AtMax(slot, rig)) return double.PositiveInfinity;
            var level = CurrentLevel(slot, rig);
            return slot switch
            {
                UpgradeSlot.Tool => ExtendedCost(ToolProtectedCost, level, ToolProtectedMaxLevel),
                UpgradeSlot.Cargo => ExtendedCost(CargoProtectedCost, level, CargoProtectedMaxLevel),
                UpgradeSlot.Engine => ExtendedCost(EngineProtectedCost, level, EngineProtectedMaxLevel),
                UpgradeSlot.Refinery => ExtendedCost(RefineryProtectedCost, level, RefineryProtectedMaxLevel),
                _ => throw new ArgumentOutOfRangeException(nameof(slot)),
            };
        }

        /// <summary>레벨이 보호 구간 안이면 옛 공식을, 밖이면 보호 구간 마지막 레벨의 비용을
        /// 기준으로 ExtendedCostGrowth를 곱해 이어 붙인다 — 경계에서 값이 그대로 이어져 끊기지
        /// 않는다(anchor가 옛 공식을 그 레벨에서 그대로 계산한 값이라 그렇다).</summary>
        static double ExtendedCost(Func<int, double> protectedFormula, int level, int protectedMaxLevel) =>
            level < protectedMaxLevel
                ? protectedFormula(level)
                : protectedFormula(protectedMaxLevel) * Math.Pow(ExtendedCostGrowth, level - protectedMaxLevel);

        // 2026-09-17 템포 조정. 중요한 건 성장률 자체가 아니라 **비용 성장률 ÷ 생산 성장률**이다.
        // 그 비율이 곧 "구매 간격이 레벨마다 몇 %씩 늘어나는가"다.
        // 업계 통설은 생산 ×1.10 / 비용 ×1.15, 즉 비율 1.045 — 20레벨 뒤에도 간격이 2.4배밖에
        // 안 는다(출처: docs/design/balance/idle-research.md). AdVenture Capitalist는 1.07을 쓴다.
        //
        // 우리 곡괭이는 생산이 레벨당 ×1.15(YieldPerVein)라 비용 ×1.20이면 비율 1.043으로 맞는다.
        // 처음에 1.34로 잡았다가 비율이 1.165(20레벨 뒤 21배)가 되는 걸 계산해 보고 되돌렸다 —
        // 초반을 촘촘하게 만들려고 성장률을 올리는 건 방향이 거꾸로였다.
        static double ToolProtectedCost(int level) => 5.0 * Math.Pow(1.20, level - 1);     // 생산 ×1.15 → 비율 1.043

        // 2026-09-17 P-01: 상한을 10→30으로 올리며 화물칸도 지수 생산으로 바꿨다(MiningSimulator.CargoHours).
        // 예전엔 "생산이 선형이라 비용을 조금 높게(1.25)" 잡았는데, 선형 생산 + 지수 비용을
        // 30레벨까지 끌고 가면 뒤로 갈수록 비용만 폭발한다 — 그래서 생산도 지수(×1.12,
        // 엔진과 같은 기울기)로 바꾸고 비용은 1.18로 낮췄다. 비율 1.18/1.12 = 1.054
        static double CargoProtectedCost(int level) => 9.0 * Math.Pow(1.18, level - 1);
        static double EngineProtectedCost(int level) => 7.0 * Math.Pow(1.17, level - 1);   // 생산(속도) ×1.12 → 비율 1.045

        // 제련소는 원석으로 산다. 레벨 0에서 시작하므로 level-1이 아니라 level을 지수로 쓴다.
        // 1레벨 12원석 — 기본 채굴차(시간당 190원석)로 4분이면 닿는다. 2026-09-17 전에는
        // 120이라 38분이 걸렸고, 그동안 화면에 회색 버튼만 있었다. 첫 관문은 빨리 열려야 한다.
        // E-04(2026-09-28): 성장률만 2.8→4.3으로 올렸다(1레벨 비용 12는 그대로 — 첫 관문은
        // 안 늦춘다). MiningSimulator.RefineCapacity가 "P의 몇 %"에서 "P와 무관한 독립 값"으로
        // 바뀌면서(economy-v2.md 3-2), 곡괭이·엔진을 계속 올려 P가 빨리 커지는 지금 구조에서는
        // 옛 2.8로 두면 제련소가 너무 빨리 5레벨까지 차 버렸다(tempo.md 5절의 1.75시간이
        // 1.1시간대로 앞당겨짐) — BalanceSim으로 다시 맞춘 값이다.
        static double RefineryProtectedCost(int level) => 12.0 * Math.Pow(4.3, level);

        /// <summary>돌파 횟수 기준 지금 레벨 상한. 하드 상한(500)과 Breakthrough.EffectiveMaxLevel
        /// 중 더 작은 쪽 — 지금은 항상 후자가 더 작거나 같다(둘 다 500에서 만난다).</summary>
        public static int EffectiveMaxLevel(UpgradeSlot slot, MiningRig rig) =>
            Math.Min(MaxLevel(slot), Breakthrough.EffectiveMaxLevel(BreakthroughCount(slot, rig)));

        public static bool AtMax(UpgradeSlot slot, MiningRig rig) => CurrentLevel(slot, rig) >= EffectiveMaxLevel(slot, rig);

        /// <summary>하드 상한(500, 돌파 여부와 무관) — Cost/AtMax는 EffectiveMaxLevel을 쓴다.
        /// 이 값은 RigParts.cs·MiningSimulator.cs가 레벨을 안전 범위로 자를 때 쓰는 절대 천장이다.</summary>
        public static int MaxLevel(UpgradeSlot slot) => slot switch
        {
            UpgradeSlot.Tool => ToolMaxLevel,
            UpgradeSlot.Cargo => CargoMaxLevel,
            UpgradeSlot.Engine => EngineMaxLevel,
            UpgradeSlot.Refinery => RefineryMaxLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(slot)),
        };

        public static int CurrentLevel(UpgradeSlot slot, MiningRig rig) => slot switch
        {
            UpgradeSlot.Tool => rig.ToolLevel,
            UpgradeSlot.Cargo => rig.CargoLevel,
            UpgradeSlot.Engine => rig.EngineLevel,
            UpgradeSlot.Refinery => rig.RefineryLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(slot)),
        };

        /// <summary>E-05: 이 슬롯이 이미 끝낸 돌파 횟수(0~9). 곡괭이는 행성마다 ToolLevel이 갈리지만
        /// 돌파는 행성과 무관하게 MiningRig 본체에 하나뿐이다(Models.cs 주석 참고).</summary>
        public static int BreakthroughCount(UpgradeSlot slot, MiningRig rig) => slot switch
        {
            UpgradeSlot.Tool => rig.ToolBreakthroughs,
            UpgradeSlot.Cargo => rig.CargoBreakthroughs,
            UpgradeSlot.Engine => rig.EngineBreakthroughs,
            UpgradeSlot.Refinery => rig.RefineryBreakthroughs,
            _ => throw new ArgumentOutOfRangeException(nameof(slot)),
        };

        /// <summary>다음에 살 돌파의 번호(1~9). 이미 아홉 번 다 끝냈으면 -1.</summary>
        public static int NextBreakthroughIndex(UpgradeSlot slot, MiningRig rig)
        {
            var done = BreakthroughCount(slot, rig);
            return done >= Breakthrough.Count ? -1 : done + 1;
        }

        /// <summary>다음 돌파에 드는 강화석. 더 살 돌파가 없으면 Infinity(Cost와 같은 관례,
        /// UI 버튼 비활성화 신호).</summary>
        public static double BreakthroughCost(UpgradeSlot slot, MiningRig rig)
        {
            var k = NextBreakthroughIndex(slot, rig);
            return k < 0 ? double.PositiveInfinity : Breakthrough.Cost(k);
        }

        /// <summary>강화석 잔고에서 다음 돌파 비용을 뗄 수 있으면 떼고 새 MiningRig·남은 잔고를
        /// 돌려준다. 일일 던전(E-08)이 아직 없어 강화석이 늘 0인 동안은 stones가 전부 0이라
        /// 항상 실패로 끝난다 — economy-v2.md 3-3 "50레벨이 사실상 상한"이 이 함수로 자연스럽게
        /// 성립한다(따로 막는 코드가 필요 없다).</summary>
        public static bool TryBreakthrough(UpgradeSlot slot, MiningRig rig, double stones, out MiningRig result, out double remainingStones)
        {
            var k = NextBreakthroughIndex(slot, rig);
            if (k < 0) { result = Copy(rig); remainingStones = stones; return false; }
            var cost = Breakthrough.Cost(k);
            if (stones < cost) { result = Copy(rig); remainingStones = stones; return false; }
            result = ApplyBreakthrough(slot, rig);
            remainingStones = stones - cost;
            return true;
        }

        /// <summary>돌파 횟수만 1 올린 새 MiningRig. 강화석을 실제로 뗐는지는 신경 안 쓴다 —
        /// 잔고 확인·차감은 TryBreakthrough(순수 함수라 SaveData를 모른다) 또는 호출하는 쪽 몫.</summary>
        public static MiningRig ApplyBreakthrough(UpgradeSlot slot, MiningRig rig)
        {
            var r = Copy(rig);
            if (NextBreakthroughIndex(slot, rig) < 0) return r;
            switch (slot)
            {
                case UpgradeSlot.Tool: r.ToolBreakthroughs++; break;
                case UpgradeSlot.Cargo: r.CargoBreakthroughs++; break;
                case UpgradeSlot.Engine: r.EngineBreakthroughs++; break;
                case UpgradeSlot.Refinery: r.RefineryBreakthroughs++; break;
            }
            return r;
        }

        /// <summary>해당 슬롯 레벨만 1 올린 새 MiningRig를 돌려준다(원본은 안 건드림). 이미 (돌파
        /// 기준) 최대면 그대로 돌려준다 — 비용을 안 냈는데 레벨이 오르면 안 되니 UI는 항상 Cost로
        /// AtMax부터 확인할 것.</summary>
        public static MiningRig Apply(UpgradeSlot slot, MiningRig rig)
        {
            var r = Copy(rig);
            if (AtMax(slot, rig)) return r;
            switch (slot)
            {
                case UpgradeSlot.Tool: r.ToolLevel++; break;
                case UpgradeSlot.Cargo: r.CargoLevel++; break;
                case UpgradeSlot.Engine: r.EngineLevel++; break;
                case UpgradeSlot.Refinery: r.RefineryLevel++; break;
            }
            return r;
        }

        /// <summary>MiningRig 필드 전부(레벨 다섯 + E-05 돌파 넷)를 복사한 새 인스턴스.
        /// Apply/ApplyBreakthrough가 공유하는 헬퍼 — 둘 중 하나만 필드를 나열하면 다음에 필드가
        /// 늘 때 한쪽이 빠뜨리기 쉽다(실제로 RigParts.cs가 이 문제를 겪었다, 아래 참고).</summary>
        static MiningRig Copy(MiningRig rig) => new MiningRig
        {
            ToolLevel = rig.ToolLevel, CargoLevel = rig.CargoLevel, EngineLevel = rig.EngineLevel,
            DetectorLevel = rig.DetectorLevel, RefineryLevel = rig.RefineryLevel,
            ToolBreakthroughs = rig.ToolBreakthroughs, CargoBreakthroughs = rig.CargoBreakthroughs,
            EngineBreakthroughs = rig.EngineBreakthroughs, RefineryBreakthroughs = rig.RefineryBreakthroughs,
        };
    }
}
