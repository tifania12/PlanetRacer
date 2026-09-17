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
        public const int ToolMaxLevel = 30;   // 곡괭이(1~10)·드릴(11~20)·레이저(21~30), MiningSimulator 티어 경계와 동일
        public const int CargoMaxLevel = 10;
        public const int EngineMaxLevel = 10;
        public const int RefineryMaxLevel = 5;   // MiningSimulator.RefinePerHour가 lvl/5로 나누므로 5가 상한

        /// <summary>이 슬롯을 원석(RawMinerals)으로 사는가. 제련소만 그렇다 — 정제 광물을 만드는
        /// 장치를 정제 광물로 사게 하면 첫 구매가 불가능해지기 때문이다. 나머지는 정제 광물로 산다.</summary>
        public static bool IsPaidWithRawMinerals(UpgradeSlot slot) => slot == UpgradeSlot.Refinery;

        /// <summary>다음 레벨로 올리는 데 드는 정제 광물. 이미 최대 레벨이면 오를 수 없다는
        /// 뜻으로 float.PositiveInfinity를 돌려준다(UI가 버튼을 비활성화하는 신호로 쓴다).</summary>
        public static float Cost(UpgradeSlot slot, MiningRig rig)
        {
            if (AtMax(slot, rig)) return float.PositiveInfinity;
            var level = CurrentLevel(slot, rig);
            return slot switch
            {
                UpgradeSlot.Tool => 15f * MathF.Pow(1.28f, level - 1),
                UpgradeSlot.Cargo => 25f * MathF.Pow(1.48f, level - 1),
                UpgradeSlot.Engine => 20f * MathF.Pow(1.42f, level - 1),
                // 제련소는 원석으로 산다. 레벨 0에서 시작하므로 level-1이 아니라 level을 지수로 쓴다.
                // 1레벨 120원석 — 기본 채굴차(시간당 190원석)로 40분 남짓, 화물칸 상한 4.0h보다 한참 앞이라
                // 상한에 처음 닿기 전에 살 수 있다. 5레벨까지 총 1,800원석쯤 든다.
                UpgradeSlot.Refinery => 120f * MathF.Pow(1.75f, level),
                _ => throw new ArgumentOutOfRangeException(nameof(slot)),
            };
        }

        public static bool AtMax(UpgradeSlot slot, MiningRig rig) => CurrentLevel(slot, rig) >= MaxLevel(slot);

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

        /// <summary>해당 슬롯 레벨만 1 올린 새 MiningRig를 돌려준다(원본은 안 건드림). 이미 최대면
        /// 그대로 돌려준다 — 비용을 안 냈는데 레벨이 오르면 안 되니 UI는 항상 Cost로 AtMax부터 확인할 것.</summary>
        public static MiningRig Apply(UpgradeSlot slot, MiningRig rig)
        {
            var r = new MiningRig
            {
                ToolLevel = rig.ToolLevel, CargoLevel = rig.CargoLevel, EngineLevel = rig.EngineLevel,
                DetectorLevel = rig.DetectorLevel, RefineryLevel = rig.RefineryLevel,
            };
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
    }
}
