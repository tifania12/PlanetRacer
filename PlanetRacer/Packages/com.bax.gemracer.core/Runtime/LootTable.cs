using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>공구 상자 확률표 한 줄 — 등급 하나가 나올 가중치. 상자를 열었을 때 실제로 무엇을
    /// 주는지(채굴차 부품 보상인지 레이싱 부품 청사진인지)는 이 파일이 정하지 않는다 — 등급만
    /// 뽑고, 등급을 실제 보상으로 바꾸는 건 받는 쪽(Assets/Scripts, 아직 없음) 몫이다.
    /// docs/decisions.md "공구 상자가 담는 것" 항목 참고.</summary>
    public struct LootWeight
    {
        public PartGrade Grade;
        public float Weight;
    }

    /// <summary>공구 상자 개봉 결과.</summary>
    public struct LootResult
    {
        public PartGrade Grade;
        /// <summary>확률이 아니라 천장(피티)으로 확정된 결과인지 — 연출에서 다르게(예: 확정 연출) 보여줄 수 있게.</summary>
        public bool Guaranteed;
    }

    /// <summary>
    /// D11-N: 공구 상자 확률표. 순수 함수, seed 하나로 재현 가능(서버 재검증·리플레이용, CLAUDE.md 1번).
    /// GDD의 상자 세 종류(녹슨/강철/티타늄)마다 표 하나 — 등급이 오를수록 좋은 상자다.
    /// 천장(피티): 같은 종류 상자를 pityCount개 열 때마다 확률과 무관하게 pityGrade를 확정 지급한다
    /// (GDD: "강철 30개 A 확정, 티타늄 10개 S 청사진 확정"). 카운터를 매번 리셋하는 로직은 이 파일이
    /// 하지 않는다 — 호출하는 쪽이 "이 상자를 지금까지 몇 개 열었는지(openedSincePity)"를 들고 있다가
    /// Guaranteed가 나온 다음에 0으로 되돌려야 한다(세이브에 카운터를 저장할 자리도 그쪽 몫).
    /// </summary>
    public static class LootTable
    {
        /// <summary>녹슨 상자 — 로컬 레이스 보상(GDD). 낮은 등급 위주, 천장 없음(GDD에 언급 없음).</summary>
        public static List<LootWeight> Rusty() => new List<LootWeight>
        {
            new LootWeight { Grade = PartGrade.C, Weight = 0.70f },
            new LootWeight { Grade = PartGrade.B, Weight = 0.25f },
            new LootWeight { Grade = PartGrade.A, Weight = 0.05f },
        };

        /// <summary>강철 상자 — 서킷 레이스 보상(GDD). B/A 위주, 30개째마다 A 확정.</summary>
        public static List<LootWeight> Steel() => new List<LootWeight>
        {
            new LootWeight { Grade = PartGrade.B, Weight = 0.60f },
            new LootWeight { Grade = PartGrade.A, Weight = 0.35f },
            new LootWeight { Grade = PartGrade.S, Weight = 0.05f },
        };
        public const int SteelPityCount = 30;
        public const PartGrade SteelPityGrade = PartGrade.A;

        /// <summary>티타늄 상자 — 챌린지 레이스 보상(GDD). A/S 위주, 10개째마다 S 확정.</summary>
        public static List<LootWeight> Titanium() => new List<LootWeight>
        {
            new LootWeight { Grade = PartGrade.A, Weight = 0.70f },
            new LootWeight { Grade = PartGrade.S, Weight = 0.30f },
        };
        public const int TitaniumPityCount = 10;
        public const PartGrade TitaniumPityGrade = PartGrade.S;

        /// <summary>확률표 하나에서 등급 하나를 뽑는다. weights의 Weight 합은 1.0이어야 기획 문서와
        /// 맞아떨어지지만(Core.Tests에서 확인), 여기서는 합으로 나눠 정규화하니 합이 달라도 동작은 한다.
        /// pityCount가 0 이하면 천장 없음(녹슨 상자). openedSincePity는 "이 상자를 마지막 확정 이후
        /// 몇 개 열었는지"(0부터 시작) — 이번 개봉까지 합쳐 pityCount에 도달하면 확정 지급한다.</summary>
        public static LootResult Open(IList<LootWeight> weights, int seed, int openedSincePity = 0,
            int pityCount = 0, PartGrade pityGrade = PartGrade.S)
        {
            if (weights.Count == 0) throw new ArgumentException("확률표가 비어 있다.");
            if (pityCount > 0 && openedSincePity + 1 >= pityCount)
                return new LootResult { Grade = pityGrade, Guaranteed = true };

            var total = 0f;
            for (var i = 0; i < weights.Count; i++) total += weights[i].Weight;
            if (total <= 0f) throw new ArgumentException("확률 가중치 합이 0 이하다.");

            var rng = new DeterministicRandom(seed);
            var roll = rng.NextFloat() * total;
            var acc = 0f;
            for (var i = 0; i < weights.Count; i++)
            {
                acc += weights[i].Weight;
                if (roll < acc) return new LootResult { Grade = weights[i].Grade, Guaranteed = false };
            }
            // 부동소수점 오차로 acc가 roll에 살짝 못 미치는 극히 드문 경우의 방어 — 마지막 등급을 준다.
            return new LootResult { Grade = weights[weights.Count - 1].Grade, Guaranteed = false };
        }
    }
}
