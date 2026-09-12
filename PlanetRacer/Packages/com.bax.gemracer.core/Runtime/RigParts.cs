using System;

namespace GemRacer.Core
{
    /// <summary>채굴차 부품 슬롯. MiningRig의 다섯 레벨 필드와 1:1로 대응한다(L-03 결정,
    /// docs/decisions.md 참고) — 레이싱카처럼 별도 부품 오브젝트를 슬롯에 끼우는 대신,
    /// 레벨을 올리는 방식을 그대로 쓰되 "레이스 보상이 슬롯 레벨을 올린다"는 개념만 더한다.</summary>
    public enum RigSlot { Tool, Cargo, Engine, Detector, Refinery }

    /// <summary>레이스 우승 보상으로 나오는 채굴차 부품. 광물이 아니라 이것이 나와야
    /// 코어 루프가 상호 강화 나선이 된다(docs/design/core-loop.md — "레이스 보상은 광물이 아니라
    /// 채굴차 부품이어야 한다").</summary>
    public sealed class RigPartReward
    {
        public string Id = "";
        public string NameKo = "";
        public RigSlot Slot;
        /// <summary>장착 시 해당 슬롯 레벨에 더하는 값. 지금은 +1 고정, 등급별 차등은 P4에서.</summary>
        public int LevelBonus = 1;
        /// <summary>어느 코스에서 나오는지(참고용, 세트 판정 등에는 아직 안 쓴다).</summary>
        public string CourseId = "";
    }

    /// <summary>RigPartReward를 MiningRig에 적용한다. 순수 함수 — 원본은 건드리지 않고 새 값을 돌려준다.
    /// 각 슬롯 레벨 상한(Models.cs의 MiningRig 필드 주석 — Tool 1~30, Cargo/Engine 1~10,
    /// Detector/Refinery 0~5)을 넘지 않게 자른다. L-05 봇 시뮬레이션에서 레이스 무료 보상이
    /// UpgradeCost의 상한(D05-N)을 무시하고 레벨을 계속 올려 버리는 것을 발견해 고쳤다 —
    /// 상점(UpgradeCost.Apply)은 원래도 상한에서 멈췄지만 이 함수는 그렇지 않았다.</summary>
    public static class RigPartApply
    {
        public static MiningRig Apply(MiningRig rig, RigPartReward reward)
        {
            var r = new MiningRig
            {
                ToolLevel = rig.ToolLevel, CargoLevel = rig.CargoLevel, EngineLevel = rig.EngineLevel,
                DetectorLevel = rig.DetectorLevel, RefineryLevel = rig.RefineryLevel,
            };
            switch (reward.Slot)
            {
                case RigSlot.Tool: r.ToolLevel = Math.Min(30, r.ToolLevel + reward.LevelBonus); break;
                case RigSlot.Cargo: r.CargoLevel = Math.Min(10, r.CargoLevel + reward.LevelBonus); break;
                case RigSlot.Engine: r.EngineLevel = Math.Min(10, r.EngineLevel + reward.LevelBonus); break;
                case RigSlot.Detector: r.DetectorLevel = Math.Min(5, r.DetectorLevel + reward.LevelBonus); break;
                case RigSlot.Refinery: r.RefineryLevel = Math.Min(5, r.RefineryLevel + reward.LevelBonus); break;
            }
            return r;
        }
    }
}
