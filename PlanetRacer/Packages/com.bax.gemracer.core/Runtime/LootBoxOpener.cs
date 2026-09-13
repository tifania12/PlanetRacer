using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>D11-N: 상자 종류. GDD의 세 종류 그대로 — 로컬/서킷/챌린지 레이스 보상.</summary>
    public enum LootBoxType { Rusty, Steel, Titanium }

    /// <summary>LootBoxOpener.Open 한 번의 결과 — 등급 뽑기 결과, 실제 채굴차 부품 보상,
    /// 다음에 저장할 천장 카운터까지 한 번에 묶는다(SaveData.SteelOpenedSincePity 등에 그대로 쓰면 됨).</summary>
    public struct LootBoxOpenResult
    {
        public LootResult Loot;
        public RigPartReward Reward;
        public int NextOpenedSincePity;
    }

    /// <summary>상자 하나를 "연다" — 확률표 뽑기(LootTable.Open) + 천장 카운터 갱신 + 등급을
    /// 채굴차 부품으로 바꾸는 것(LootReward.FromLoot)까지 한 번에 묶는 얇은 조립 계층.
    /// 순수 함수라 SaveData를 직접 건드리지 않는다 — 상자 개수 차감, 카운터 저장, RigPartApply.Apply
    /// 호출은 부르는 쪽(다음 세션의 개봉 화면 몫) 일이다.</summary>
    public static class LootBoxOpener
    {
        public static LootBoxOpenResult Open(LootBoxType type, int gradeSeed, int slotSeed,
            int openedSincePity, string courseId = "")
        {
            var (weights, pityCount, pityGrade) = TableFor(type);
            var loot = LootTable.Open(weights, gradeSeed, openedSincePity, pityCount, pityGrade);
            var reward = LootReward.FromLoot(loot, slotSeed, courseId);
            return new LootBoxOpenResult
            {
                Loot = loot,
                Reward = reward,
                // 확정 지급(Guaranteed)이 나온 다음엔 카운터를 0으로 되돌린다 — LootTable.cs 클래스
                // 주석의 "Guaranteed가 나온 다음에 0으로 되돌려야 한다"를 여기서 대신 해 준다.
                NextOpenedSincePity = loot.Guaranteed ? 0 : openedSincePity + 1,
            };
        }

        static (List<LootWeight> Weights, int PityCount, PartGrade PityGrade) TableFor(LootBoxType type) => type switch
        {
            LootBoxType.Rusty => (LootTable.Rusty(), 0, PartGrade.S),
            LootBoxType.Steel => (LootTable.Steel(), LootTable.SteelPityCount, LootTable.SteelPityGrade),
            LootBoxType.Titanium => (LootTable.Titanium(), LootTable.TitaniumPityCount, LootTable.TitaniumPityGrade),
            _ => (LootTable.Rusty(), 0, PartGrade.S),
        };
    }
}
