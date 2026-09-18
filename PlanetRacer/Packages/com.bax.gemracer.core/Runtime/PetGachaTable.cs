using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>펫 뽑기 확률표 한 줄.</summary>
    public struct PetGachaWeight
    {
        public PetGrade Grade;
        public float Weight;
    }

    /// <summary>펫 뽑기 결과.</summary>
    public struct PetGachaResult
    {
        public PetGrade Grade;
        /// <summary>확률이 아니라 천장(피티) 또는 10연차 보장으로 나온 결과인지.</summary>
        public bool Guaranteed;
    }

    /// <summary>
    /// P-12: 펫 뽑기 4종의 확률표와 천장(docs/design/pet-gacha.md 3절). 순수 함수, seed 하나로
    /// 재현 가능(서버 재검증·리플레이용, CLAUDE.md 1번). LootTable.cs와 같은 구조 —
    /// 확률표는 여기, "지금까지 몇 번 열었는지(openedSincePity)"는 호출하는 쪽(SaveData, 아직
    /// 연결 안 됨, P-14)이 들고 있다가 확정 지급 후 0으로 되돌린다.
    ///
    /// **뽑기마다 표·천장 카운터가 완전히 별개다** — 무료 뽑기를 아무리 돌려도 고급 뽑기의
    /// 천장 카운터는 안 움직인다. 이 파일은 그 분리를 강제하지 않는다(둘 다 그냥 int
    /// 매개변수라서) — 대신 호출하는 쪽이 SaveData에 뽑기 종류마다 별도 필드를 두는 것으로
    /// 지킨다. Core.Tests에서 "같은 카운터 값을 무료 표와 고급 표에 각각 넣어도 서로 다른
    /// 시점에 확정이 뜬다"로 분리가 실제로 의미 있음을 확인한다.
    /// </summary>
    public static class PetGachaTable
    {
        /// <summary>무료 뽑기 — 광고 시청(하루 10회). 1~4등급 위주, 5등급 소량. 천장 없음.</summary>
        public static List<PetGachaWeight> Free() => new List<PetGachaWeight>
        {
            new PetGachaWeight { Grade = PetGrade.Common, Weight = 0.55f },
            new PetGachaWeight { Grade = PetGrade.Advanced, Weight = 0.28f },
            new PetGachaWeight { Grade = PetGrade.Rare, Weight = 0.12f },
            new PetGachaWeight { Grade = PetGrade.Epic, Weight = 0.045f },
            new PetGachaWeight { Grade = PetGrade.Legendary, Weight = 0.005f },
        };

        /// <summary>일반 뽑기 — 레이싱 재화(무과금 진행선). 1~6등급, 천장 없음.</summary>
        public static List<PetGachaWeight> Normal() => new List<PetGachaWeight>
        {
            new PetGachaWeight { Grade = PetGrade.Common, Weight = 0.40f },
            new PetGachaWeight { Grade = PetGrade.Advanced, Weight = 0.30f },
            new PetGachaWeight { Grade = PetGrade.Rare, Weight = 0.18f },
            new PetGachaWeight { Grade = PetGrade.Epic, Weight = 0.08f },
            new PetGachaWeight { Grade = PetGrade.Legendary, Weight = 0.035f },
            new PetGachaWeight { Grade = PetGrade.Mythic, Weight = 0.005f },
        };

        /// <summary>고급 뽑기 — 하루 1개 무료 / 유료. 사실상 메인 매출. 3~6등급, 80뽑째 신화 확정.
        /// 10연차는 5등급(전설) 이상 1마리 확정 — OpenTen()이 그 보장을 처리한다.</summary>
        public static List<PetGachaWeight> Advanced() => new List<PetGachaWeight>
        {
            new PetGachaWeight { Grade = PetGrade.Rare, Weight = 0.55f },
            new PetGachaWeight { Grade = PetGrade.Epic, Weight = 0.30f },
            new PetGachaWeight { Grade = PetGrade.Legendary, Weight = 0.12f },
            new PetGachaWeight { Grade = PetGrade.Mythic, Weight = 0.03f },
        };
        public const int AdvancedPityCount = 80;
        public const PetGrade AdvancedPityGrade = PetGrade.Mythic;
        public const PetGrade AdvancedTenPullMinGrade = PetGrade.Legendary;

        /// <summary>특수 뽑기 — 초월의 인장으로만 입장. 초월이 나오는 유일한 곳. 5~7등급,
        /// 80뽑째 초월 확정(3·4절 기준 — 8절 테스트 체크리스트의 "150"은 오기, 이번에 고쳤다).</summary>
        public static List<PetGachaWeight> Special() => new List<PetGachaWeight>
        {
            new PetGachaWeight { Grade = PetGrade.Legendary, Weight = 0.80f },
            new PetGachaWeight { Grade = PetGrade.Mythic, Weight = 0.195f },
            new PetGachaWeight { Grade = PetGrade.Transcendent, Weight = 0.005f },
        };
        public const int SpecialPityCount = 80;
        public const PetGrade SpecialPityGrade = PetGrade.Transcendent;

        /// <summary>확률표 하나에서 등급 하나를 뽑는다. LootTable.Open과 같은 구조 —
        /// weights의 Weight 합은 1.0이어야 기획과 맞지만(Core.Tests에서 확인), 합으로 정규화하니
        /// 합이 달라도 동작은 한다. pityCount가 0 이하면 천장 없음(무료·일반 뽑기).
        /// openedSincePity는 "이 뽑기 종류를 마지막 확정 이후 몇 번 돌렸는지"(0부터) — 이번
        /// 회차까지 합쳐 pityCount에 도달하면 확정 지급한다.</summary>
        public static PetGachaResult Open(IList<PetGachaWeight> weights, int seed, int openedSincePity = 0,
            int pityCount = 0, PetGrade pityGrade = PetGrade.Transcendent)
        {
            if (weights.Count == 0) throw new ArgumentException("확률표가 비어 있다.");
            if (pityCount > 0 && openedSincePity + 1 >= pityCount)
                return new PetGachaResult { Grade = pityGrade, Guaranteed = true };

            var total = 0f;
            for (var i = 0; i < weights.Count; i++) total += weights[i].Weight;
            if (total <= 0f) throw new ArgumentException("확률 가중치 합이 0 이하다.");

            var rng = new DeterministicRandom(seed);
            var roll = rng.NextFloat() * total;
            var acc = 0f;
            for (var i = 0; i < weights.Count; i++)
            {
                acc += weights[i].Weight;
                if (roll < acc) return new PetGachaResult { Grade = weights[i].Grade, Guaranteed = false };
            }
            // 부동소수점 오차로 acc가 roll에 살짝 못 미치는 극히 드문 경우의 방어 — 마지막 등급을 준다.
            return new PetGachaResult { Grade = weights[weights.Count - 1].Grade, Guaranteed = false };
        }

        /// <summary>10연차 — 10번을 잇달아 뽑되, 그중 minGrade 이상이 하나도 없으면 가장 낮은
        /// 결과 하나를 minGrade로 올려서 보장한다(고급 뽑기: 5등급 이상 확정). 각 회차는
        /// baseSeed + i로 파생되고 openedSincePity도 회차마다 하나씩 올라간다 — 그래서 10연차
        /// 도중에 천장에 걸리면 그 회차만 확정 지급되고 이후 회차는 카운터가 이어진다.
        /// 반환값의 마지막 요소가 다음 openedSincePity(호출부가 세이브에 반영).</summary>
        public static PetGachaResult[] OpenTen(IList<PetGachaWeight> weights, int baseSeed, PetGrade minGrade,
            int openedSincePity = 0, int pityCount = 0, PetGrade pityGrade = PetGrade.Transcendent)
        {
            const int count = 10;
            var results = new PetGachaResult[count];
            var hasMinGrade = false;
            var worstIndex = 0;
            for (var i = 0; i < count; i++)
            {
                results[i] = Open(weights, baseSeed + i, openedSincePity + i, pityCount, pityGrade);
                if (results[i].Grade >= minGrade) hasMinGrade = true;
                if (results[i].Grade < results[worstIndex].Grade) worstIndex = i;
            }
            if (!hasMinGrade)
                results[worstIndex] = new PetGachaResult { Grade = minGrade, Guaranteed = true };
            return results;
        }
    }
}
