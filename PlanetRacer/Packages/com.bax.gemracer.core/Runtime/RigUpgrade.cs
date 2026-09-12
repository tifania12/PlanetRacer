using System;

namespace GemRacer.Core
{
    /// <summary>D05-N에서 업그레이드 화면에 노출하는 세 장비. 채굴차 슬롯은 다섯 개(RigParts.cs
    /// RigSlot)지만 Detector·Refinery는 아직 쓰는 화면이 없어서(탐지·정제 기능 자체가 미구현) 뺐다.</summary>
    public enum UpgradeSlot { Tool, Cargo, Engine }

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
                _ => throw new ArgumentOutOfRangeException(nameof(slot)),
            };
        }

        public static bool AtMax(UpgradeSlot slot, MiningRig rig) => CurrentLevel(slot, rig) >= MaxLevel(slot);

        public static int MaxLevel(UpgradeSlot slot) => slot switch
        {
            UpgradeSlot.Tool => ToolMaxLevel,
            UpgradeSlot.Cargo => CargoMaxLevel,
            UpgradeSlot.Engine => EngineMaxLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(slot)),
        };

        public static int CurrentLevel(UpgradeSlot slot, MiningRig rig) => slot switch
        {
            UpgradeSlot.Tool => rig.ToolLevel,
            UpgradeSlot.Cargo => rig.CargoLevel,
            UpgradeSlot.Engine => rig.EngineLevel,
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
            }
            return r;
        }
    }
}
