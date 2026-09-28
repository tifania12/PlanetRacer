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
    /// 각 슬롯 레벨 상한(UpgradeCost의 *MaxLevel — Tool/Cargo/Engine 1~30, Refinery 0~5. Detector는
    /// UpgradeSlot에 없어 참조할 상수가 없으므로 0~5를 그대로 하드코딩)을 넘지 않게 자른다.
    /// L-05 봇 시뮬레이션에서 레이스 무료 보상이 UpgradeCost의 상한(D05-N)을 무시하고 레벨을
    /// 계속 올려 버리는 것을 발견해 고쳤다 — 상점(UpgradeCost.Apply)은 원래도 상한에서 멈췄지만
    /// 이 함수는 그렇지 않았다. Cargo/Engine 상한은 2026-09-17 P-01로 10→30 — Tool/Cargo/Engine/
    /// Refinery 네 슬롯은 하드코딩 대신 UpgradeCost.*MaxLevel을 직접 참조해 두 곳이 다시
    /// 어긋나지 않게 했다(Refinery는 2026-09-23 야간 세션이 마저 맞췄다 — 그 전엔 5가
    /// 하드코딩돼 있어서 UpgradeCost.RefineryMaxLevel이 바뀌면 여기만 안 따라갈 뻔했다).</summary>
    public static class RigPartApply
    {
        public static MiningRig Apply(MiningRig rig, RigPartReward reward)
        {
            // E-05(2026-09-29): 필드가 다섯(레벨)+넷(돌파)으로 늘어서 수동 나열은 하나라도 빠뜨리면
            // 돌파 횟수가 조용히 0으로 리셋된다 — 실제로 이 함수가 그 문제를 겪고 있었다(ToolBreakthroughs
            // 등 새 필드를 안 옮겨서 레이스 보상을 받을 때마다 돌파가 날아갈 뻔했다). 그래서
            // MiningRig 자체를 얕은 복사(레퍼런스 타입 필드가 없어 문제없다)하고 대상 슬롯만 고친다.
            var r = new MiningRig
            {
                ToolLevel = rig.ToolLevel, CargoLevel = rig.CargoLevel, EngineLevel = rig.EngineLevel,
                DetectorLevel = rig.DetectorLevel, RefineryLevel = rig.RefineryLevel,
                ToolBreakthroughs = rig.ToolBreakthroughs, CargoBreakthroughs = rig.CargoBreakthroughs,
                EngineBreakthroughs = rig.EngineBreakthroughs, RefineryBreakthroughs = rig.RefineryBreakthroughs,
            };
            // E-05: 상한을 하드캡(500, UpgradeCost.*MaxLevel)이 아니라 돌파 기준 EffectiveMaxLevel로
            // 자른다 — 레이스 무료 보상도 돌파 벽은 못 넘는다(상점 구매와 같은 규칙).
            switch (reward.Slot)
            {
                case RigSlot.Tool: r.ToolLevel = Math.Min(UpgradeCost.EffectiveMaxLevel(UpgradeSlot.Tool, rig), r.ToolLevel + reward.LevelBonus); break;
                case RigSlot.Cargo: r.CargoLevel = Math.Min(UpgradeCost.EffectiveMaxLevel(UpgradeSlot.Cargo, rig), r.CargoLevel + reward.LevelBonus); break;
                case RigSlot.Engine: r.EngineLevel = Math.Min(UpgradeCost.EffectiveMaxLevel(UpgradeSlot.Engine, rig), r.EngineLevel + reward.LevelBonus); break;
                // Detector는 UpgradeSlot에 없어(UpgradeCost 위 주석 참고) 참조할 상수가 없다 — 5는 그대로 하드코딩.
                // E-05 돌파 대상도 아니다(economy-v2.md 3절 표에 Detector가 없다).
                case RigSlot.Detector: r.DetectorLevel = Math.Min(5, r.DetectorLevel + reward.LevelBonus); break;
                case RigSlot.Refinery: r.RefineryLevel = Math.Min(UpgradeCost.EffectiveMaxLevel(UpgradeSlot.Refinery, rig), r.RefineryLevel + reward.LevelBonus); break;
            }
            return r;
        }
    }
}
