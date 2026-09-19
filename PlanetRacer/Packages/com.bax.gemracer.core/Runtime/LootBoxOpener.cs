using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>D11-N: 상자 종류. GDD의 세 종류 그대로 — 로컬/서킷/챌린지 레이스 보상.</summary>
    public enum LootBoxType { Rusty, Steel, Titanium }

    /// <summary>LootBoxOpener.Open 한 번의 결과 — 등급 뽑기 결과, 실제 채굴차 부품 보상,
    /// 다음에 저장할 천장 카운터까지 한 번에 묶는다(SaveData.SteelOpenedSincePity 등에 그대로 쓰면 됨).
    /// P-04: Kind가 실제로 어느 필드를 봐야 하는지 가리킨다 — Kind==RigPart면 Reward, Amplifier면
    /// AmplifierBonus, Minerals면 Minerals. Kind는 default(0)가 RigPart라, 옛 Open()이 채우지 않고
    /// 넘어가도 항상 RigPart로 읽혀서 기존 호출부(MiningController.TryOpenBox·LootBoxPanel·
    /// LootBoxUgui)는 Reward가 그대로 채워진 채 동작이 하나도 안 바뀐다.</summary>
    public struct LootBoxOpenResult
    {
        public LootResult Loot;
        public LootRewardKind Kind;
        public RigPartReward Reward;
        public AmplifierReward? AmplifierBonus;
        public MineralReward? Minerals;
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

        /// <summary>P-04: Open과 같지만 부품 대신 증폭기·광물이 나올 수도 있다(LootReward.RollKind).
        /// 천장(Guaranteed)은 등급만 확정할 뿐 종류까지 부품으로 고정하지 않는다 — 확정 등급의
        /// 증폭기·원석도 그 등급 값 그대로 나온다. kindSeed·mineralSeed는 gradeSeed·slotSeed와
        /// 겹치면 안 된다(서로 다른 뽑기가 같은 시드를 쓰면 결과가 섞인다).</summary>
        public static LootBoxOpenResult OpenAny(LootBoxType type, int gradeSeed, int kindSeed, int slotSeed,
            int mineralSeed, int openedSincePity, string courseId = "")
        {
            var (weights, pityCount, pityGrade) = TableFor(type);
            var loot = LootTable.Open(weights, gradeSeed, openedSincePity, pityCount, pityGrade);
            var kind = LootReward.RollKind(kindSeed);
            var result = new LootBoxOpenResult
            {
                Loot = loot,
                Kind = kind,
                NextOpenedSincePity = loot.Guaranteed ? 0 : openedSincePity + 1,
            };
            switch (kind)
            {
                case LootRewardKind.Amplifier:
                    result.AmplifierBonus = LootReward.AmplifierFor(loot.Grade, slotSeed);
                    break;
                case LootRewardKind.Minerals:
                    result.Minerals = LootReward.MineralsFor(loot.Grade, mineralSeed);
                    break;
                default: // RigPart — 정의 밖 enum 값도 이 쪽으로 방어(LootReward.RollKind가 늘 정의된 값만 주지만)
                    result.Reward = LootReward.FromLoot(loot, slotSeed, courseId);
                    break;
            }
            return result;
        }

        static (List<LootWeight> Weights, int PityCount, PartGrade PityGrade) TableFor(LootBoxType type) => type switch
        {
            LootBoxType.Rusty => (LootTable.Rusty(), 0, PartGrade.S),
            LootBoxType.Steel => (LootTable.Steel(), LootTable.SteelPityCount, LootTable.SteelPityGrade),
            LootBoxType.Titanium => (LootTable.Titanium(), LootTable.TitaniumPityCount, LootTable.TitaniumPityGrade),
            _ => (LootTable.Rusty(), 0, PartGrade.S),
        };

        /// <summary>화면 표시용 한국어 이름. 개봉 화면(다음 세션)과 레이스 결과 화면이 같이 쓴다.</summary>
        public static string NameKo(LootBoxType type) => type switch
        {
            LootBoxType.Rusty => "녹슨 상자",
            LootBoxType.Steel => "강철 상자",
            LootBoxType.Titanium => "티타늄 상자",
            _ => type.ToString(),
        };
    }
}
