using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>T-07 결정(docs/decisions.md 참고): A안 — 공구 상자 등급은 전부 채굴차 부품
    /// (RigPartReward)으로 바뀐다. 세 세션째 결정이 안 나서, 지난 세션이 남긴 "애매하면 A안 기본
    /// 진행"대로 이번 세션이 골랐다 — Tifania가 나중에 B안(A/S 등급은 레이싱카 부품 청사진)을
    /// 고르면 이 파일의 매핑만 바꾸면 된다(LootTable/RigParts는 그대로 재사용 가능).
    ///
    /// 등급은 두 가지에 반영한다 — ① 어느 슬롯이 오르는지(등급이 높을수록 희귀 슬롯 Detector/
    /// Refinery 확률이 커진다, decisions.md T-07 A안 "S=희귀 슬롯 우대") ② 슬롯이 한 번에
    /// 얼마나 오르는지(LevelBonus, 등급이 높을수록 크다). 상한 클램프는 RigPartApply가 이미
    /// 하니 여기서는 신경 쓰지 않는다.</summary>
    public static class LootReward
    {
        static readonly RigSlot[] SlotOrder =
            { RigSlot.Tool, RigSlot.Cargo, RigSlot.Engine, RigSlot.Detector, RigSlot.Refinery };

        /// <summary>등급별 슬롯 가중치. 인덱스는 SlotOrder와 같은 순서.</summary>
        static readonly Dictionary<PartGrade, float[]> SlotWeights = new Dictionary<PartGrade, float[]>
        {
            { PartGrade.C, new[] { 0.34f, 0.33f, 0.33f, 0.00f, 0.00f } },
            { PartGrade.B, new[] { 0.30f, 0.25f, 0.25f, 0.12f, 0.08f } },
            { PartGrade.A, new[] { 0.15f, 0.15f, 0.15f, 0.30f, 0.25f } },
            { PartGrade.S, new[] { 0.00f, 0.00f, 0.00f, 0.50f, 0.50f } },
        };

        /// <summary>등급별 레벨 보너스. 등급이 높을수록 한 번에 더 많이 오른다.</summary>
        public static int LevelBonusFor(PartGrade grade) => grade switch
        {
            PartGrade.C => 1,
            PartGrade.B => 1,
            PartGrade.A => 2,
            PartGrade.S => 3,
            _ => 1,
        };

        /// <summary>등급 가중치대로 슬롯 하나를 뽑는다. seed 하나로 재현 가능(LootTable.Open과
        /// 같은 원리) — 등급을 뽑은 시드와 다른 시드를 줘야 등급 뽑기와 슬롯 뽑기가 서로 안 섞인다.</summary>
        public static RigSlot PickSlot(PartGrade grade, int seed)
        {
            var weights = SlotWeights[grade];
            var total = 0f;
            for (var i = 0; i < weights.Length; i++) total += weights[i];

            var rng = new DeterministicRandom(seed);
            var roll = rng.NextFloat() * total;
            var acc = 0f;
            for (var i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (roll < acc) return SlotOrder[i];
            }
            // 부동소수점 오차 방어 — 마지막으로 가중치가 0보다 큰 슬롯을 돌려준다(LootTable.Open과 동일 패턴).
            for (var i = weights.Length - 1; i >= 0; i--) if (weights[i] > 0f) return SlotOrder[i];
            return SlotOrder[SlotOrder.Length - 1];
        }

        /// <summary>공구 상자 개봉 결과를 실제 채굴차 부품 보상으로 바꾼다.</summary>
        public static RigPartReward FromLoot(LootResult loot, int slotSeed, string courseId = "")
        {
            var slot = PickSlot(loot.Grade, slotSeed);
            return new RigPartReward
            {
                Id = $"loot-{loot.Grade}-{slot}",
                NameKo = $"{loot.Grade}등급 {SlotNameKo(slot)} 부품",
                Slot = slot,
                LevelBonus = LevelBonusFor(loot.Grade),
                CourseId = courseId,
            };
        }

        static string SlotNameKo(RigSlot slot) => slot switch
        {
            RigSlot.Tool => "곡괭이",
            RigSlot.Cargo => "화물칸",
            RigSlot.Engine => "엔진",
            RigSlot.Detector => "탐지기",
            RigSlot.Refinery => "제련기",
            _ => slot.ToString(),
        };
    }
}
