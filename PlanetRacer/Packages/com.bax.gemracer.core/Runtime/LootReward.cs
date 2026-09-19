using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>P-04: 상자 하나를 열었을 때 실제로 무엇을 주는지의 종류. 기본값(0)이 RigPart인 것은
    /// 우연이 아니다 — 기존 LootBoxOpener.Open()이 채우는 LootBoxOpenResult.Kind가 이 필드를
    /// 한 번도 안 건드려도 항상 RigPart로 읽혀서, 예전 호출부(MiningController.TryOpenBox 등)가
    /// 컴파일·동작 모두 그대로 유지된다.</summary>
    public enum LootRewardKind { RigPart, Amplifier, Minerals }

    /// <summary>상자에서 나온 증폭기. 등급과 Amplifier.Roll로 이미 확정된 증폭률 값을 들고 있다 —
    /// 어느 칸(곡괭이·화물칸·엔진·제련소 / 레이싱카 다섯 칸)에 끼우는지는 아직 정하지 않는다.
    /// amplifier.md가 "소모품이 아니라 끼우는 것"일 수도 있다고 남겨 둔 열린 질문이라, 칸 배정은
    /// 세이브·UI(P-05) 몫으로 남긴다.</summary>
    public struct AmplifierReward
    {
        public PartGrade Grade;
        public float Bonus;
    }

    /// <summary>상자에서 나온 원석(정제 전). 화물칸에 바로 더해지는 값 — Tifania: "물론 상자를
    /// 까서 광물이 나올수도있고".</summary>
    public struct MineralReward
    {
        public float Amount;
    }

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

        // P-04 첫 값 — 상자가 실제로 무엇을 주는지의 종류 가중치. 부품 위주를 유지한다(클래스 위
        // 주석의 코어 루프 원칙 — "레이스 보상은 광물이 아니라 부품이어야 한다"는 여전히 유효하니
        // 부품이 절반을 넘게 잡았다). 증폭기·광물은 상자 종류(Rusty/Steel/Titanium)와 무관하게
        // 같은 비율 — 실제로 플레이해 보고 조정될 여지가 있는 첫 값이다(CourseGenerator.RoughnessBias와
        // 같은 성격).
        static readonly (LootRewardKind Kind, float Weight)[] KindWeights =
        {
            (LootRewardKind.RigPart, 0.55f),
            (LootRewardKind.Amplifier, 0.30f),
            (LootRewardKind.Minerals, 0.15f),
        };

        /// <summary>상자 하나가 부품/증폭기/광물 중 무엇을 줄지 뽑는다. seed 하나로 재현 가능 —
        /// 등급을 뽑은 시드(LootTable.Open)와 달라야 등급 뽑기와 종류 뽑기가 서로 안 섞인다.</summary>
        public static LootRewardKind RollKind(int seed)
        {
            var total = 0f;
            foreach (var w in KindWeights) total += w.Weight;

            var rng = new DeterministicRandom(seed);
            var roll = rng.NextFloat() * total;
            var acc = 0f;
            foreach (var w in KindWeights)
            {
                acc += w.Weight;
                if (roll < acc) return w.Kind;
            }
            return KindWeights[KindWeights.Length - 1].Kind; // 부동소수점 오차 방어
        }

        /// <summary>등급별 원석 보상 범위. Amplifier.MinBonus/MaxBonus와 같은 구간-균등분포 패턴 —
        /// 등급이 높을수록 더 많이 나온다. 첫 값(P-04) — 실제 채굴 산출(MiningSimulator.YieldPerVein)과
        /// 견줘 보고 조정될 여지가 있다.</summary>
        static readonly float[] MineralMin = { 10f, 25f, 60f, 150f };   // C, B, A, S
        static readonly float[] MineralMax = { 20f, 50f, 120f, 300f };

        public static float MineralMinFor(PartGrade grade) => MineralMin[(int)grade];
        public static float MineralMaxFor(PartGrade grade) => MineralMax[(int)grade];

        /// <summary>상자에서 나온 증폭기. Amplifier.Roll을 그대로 감싸 등급을 들고 다니게 한다.</summary>
        public static AmplifierReward AmplifierFor(PartGrade grade, int seed) => new AmplifierReward
        {
            Grade = grade,
            Bonus = Amplifier.Roll(grade, seed),
        };

        /// <summary>상자에서 나온 원석. [MineralMinFor, MineralMaxFor) 구간 균등분포, seed로 재현 가능.</summary>
        public static MineralReward MineralsFor(PartGrade grade, int seed)
        {
            var rng = new DeterministicRandom(seed);
            var min = MineralMinFor(grade);
            var max = MineralMaxFor(grade);
            return new MineralReward { Amount = min + rng.NextFloat() * (max - min) };
        }

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
