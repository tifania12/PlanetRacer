using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>P-14 ②: 펫 계열(docs/design/pet-gacha.md 7절 "같은 생물이 자란 모습").</summary>
    public enum PetFamily { Wheel, Wing, Ore, Cargo }

    /// <summary>종 하나. 1~5등급은 계열×행성 색으로 기계적으로 정해지고(PlanetId 있음),
    /// 6·7등급은 낱개 디자인이라 PlanetId가 없다(null) — 이름·고유 효과는 여전히 Tifania 결정
    /// 대기(art-requests.md, PetGradeInfo.EquipBonusFor 주석과 같은 블로커, 신화 30 + 초월 10 = 48종).</summary>
    public readonly struct PetSpeciesDef
    {
        public readonly int Id;
        public readonly PetGrade Grade;
        public readonly PetFamily Family;

        /// <summary>1~5등급만 값이 있다(quartz/ruby/sapphire/aquamarine/cinnabar 중 하나).
        /// 6·7등급은 null — 낱개 디자인이라 행성 색과 안 묶인다.</summary>
        public readonly string PlanetId;

        public PetSpeciesDef(int id, PetGrade grade, PetFamily family, string planetId)
        {
            Id = id;
            Grade = grade;
            Family = family;
            PlanetId = planetId;
        }
    }

    /// <summary>
    /// P-14 ②: 124종 전체 명단(docs/design/pet-gacha.md 2·7절). backlog.md 2026-09-20 "다음에
    /// 할 만한 것"이 제안한 대로 "이름·아트 없이 숫자 id + 등급만으로" 먼저 만든다 — 신화 30 +
    /// 초월 10 = 48종은 이름·고유 효과가 아직 안 정해졌지만, 자리(id·등급·계열)는 여기서 고정해도
    /// 나중에 이름만 붙이면 되니 미리 만들 수 있다.
    ///
    /// 1~4등급(16종 = 4계열×4색)과 5등급(20종 = 4계열×5색)은 "계열×행성 색"으로 기계적으로 정해져서
    /// 대응표 없이 이 클래스가 직접 만든다 — 2026-09-19 T-12가 종 수를 4로 나눠떨어지게 고친 이유가
    /// 바로 이거다(전에는 12/12/14/16/18이라 이 조합 자체가 안 됐다). 6등급(신화, 계열별 8·8·7·7)과
    /// 7등급(초월, 능력 축 10개)은 낱개 디자인이라 계열×색 규칙이 없다 — 여기서는 표시 순서만
    /// 고정해 둔다(7등급의 Family는 뜻이 없고 표시 그룹핑용으로만 순환시킨 값이다).
    ///
    /// <b>아직 여기 없는 것</b> — 이 종 목록으로 뽑기 결과를 "등급"에서 "종"으로 바꾸는 일
    /// (PetGachaController가 PetGachaResult에 종 id를 채우고, SaveData.PetGachaSave가 등급별
    /// 카운트 대신 종별 보유 여부를 담고, 중복이면 PetFusion 조각으로 자동 전환하는 것)은
    /// PetGachaController/SaveData를 같이 고쳐야 해서 다음 세션 몫이다.
    /// </summary>
    public static class PetSpeciesTable
    {
        /// <summary>1~5등급에서 쓰는 행성 색 순서(7절 "행성 순서대로") — 라피스는 안 쓴다(5등급까지만 색이 필요).</summary>
        public static readonly string[] PlanetColorOrder = { "quartz", "ruby", "sapphire", "aquamarine", "cinnabar" };

        /// <summary>계열 순서(7절 표 그대로) — 6등급 "계열별 8·8·7·7"이 이 순서와 대응한다.</summary>
        public static readonly PetFamily[] FamilyOrder = { PetFamily.Wheel, PetFamily.Wing, PetFamily.Ore, PetFamily.Cargo };

        public static readonly string[] FamilyNameKo = { "바퀴족", "날개족", "광석족", "짐꾼족" };

        /// <summary>7등급 10마리가 하나씩 맡는 축(7절 "서로 다른 축"). 축 자체는 design에 이미 있어
        /// 고정할 수 있지만, 종 이름은 여전히 Tifania 결정 대기다.</summary>
        public static readonly string[] TranscendentAxisKo =
        {
            "채굴 산출", "정제 속도", "화물칸", "레이스 속도", "부스트",
            "상자 등급", "광고 보상", "오프라인 상한", "조각 획득", "연료 회복",
        };

        private static readonly PetSpeciesDef[] AllSpecies = Build();
        private static readonly Dictionary<PetGrade, int[]> IdsByGrade = BuildIndex();

        public static IReadOnlyList<PetSpeciesDef> All => AllSpecies;

        private static PetSpeciesDef[] Build()
        {
            var list = new List<PetSpeciesDef>(PetGradeInfo.TotalSpeciesCount);
            var nextId = 0;

            // 1~4등급: 4계열 × 4색(쿼츠·루비·사파이어·아쿠아마린)
            for (var g = 0; g <= 3; g++)
            {
                var grade = (PetGrade)g;
                foreach (var family in FamilyOrder)
                    for (var c = 0; c < 4; c++)
                        list.Add(new PetSpeciesDef(nextId++, grade, family, PlanetColorOrder[c]));
            }

            // 5등급(전설): 4계열 × 5색(+주사)
            foreach (var family in FamilyOrder)
                for (var c = 0; c < 5; c++)
                    list.Add(new PetSpeciesDef(nextId++, PetGrade.Legendary, family, PlanetColorOrder[c]));

            // 6등급(신화): 계열별 8·8·7·7, 낱개 디자인이라 PlanetId 없음
            var mythicCountByFamily = new[] { 8, 8, 7, 7 };
            for (var f = 0; f < FamilyOrder.Length; f++)
                for (var i = 0; i < mythicCountByFamily[f]; i++)
                    list.Add(new PetSpeciesDef(nextId++, PetGrade.Mythic, FamilyOrder[f], null));

            // 7등급(초월): 능력 축 10개, 계열 구분이 뜻 없어 표시용으로만 순환
            for (var i = 0; i < TranscendentAxisKo.Length; i++)
                list.Add(new PetSpeciesDef(nextId++, PetGrade.Transcendent, FamilyOrder[i % FamilyOrder.Length], null));

            return list.ToArray();
        }

        private static Dictionary<PetGrade, int[]> BuildIndex()
        {
            var byGrade = new Dictionary<PetGrade, List<int>>();
            foreach (var def in AllSpecies)
            {
                if (!byGrade.TryGetValue(def.Grade, out var ids))
                    byGrade[def.Grade] = ids = new List<int>();
                ids.Add(def.Id);
            }
            var result = new Dictionary<PetGrade, int[]>();
            foreach (var kv in byGrade) result[kv.Key] = kv.Value.ToArray();
            return result;
        }

        /// <summary>그 등급에 속한 종 id 전부(Build()가 만든 순서 그대로 고정).</summary>
        public static int[] InGrade(PetGrade grade) => IdsByGrade[grade];

        public static PetSpeciesDef Get(int id)
        {
            if (id < 0 || id >= AllSpecies.Length)
                throw new ArgumentOutOfRangeException(nameof(id), $"종 id는 0~{AllSpecies.Length - 1} 범위여야 한다, 실제 {id}");
            return AllSpecies[id];
        }

        /// <summary>그 등급 안에서 종 하나를 균등 확률로 고른다 — 뽑기가 등급까지만 정하고
        /// (PetGachaTable.Open) 그 안의 종은 여기서 정한다는 역할 분리다. 같은 seed면 같은 종.</summary>
        public static PetSpeciesDef PickInGrade(PetGrade grade, int seed)
        {
            var ids = InGrade(grade);
            var rng = new DeterministicRandom(seed);
            var index = rng.NextInt(0, ids.Length);
            return AllSpecies[ids[index]];
        }

        /// <summary>1~5등급(계열×색으로 정해진 종)의 기계적 표시 이름 — "쿼츠 바퀴족" 식.
        /// 6·7등급은 이름이 아직 없어 예외를 던진다(PetGradeInfo.EquipBonusFor와 같은 자리).</summary>
        public static string MechanicalDisplayNameKo(PetSpeciesDef def)
        {
            if (def.PlanetId == null)
                throw new ArgumentException(
                    $"{PetGradeInfo.NameKoFor(def.Grade)} 종(id {def.Id})은 낱개 디자인이라 이름이 아직 안 정해졌다.");
            var planet = Array.IndexOf(PlanetColorOrder, def.PlanetId);
            if (planet < 0)
                throw new ArgumentException($"알 수 없는 행성 id: {def.PlanetId}");
            return $"{PlanetNameKo(def.PlanetId)} {FamilyNameKo[Array.IndexOf(FamilyOrder, def.Family)]}";
        }

        private static string PlanetNameKo(string planetId) => planetId switch
        {
            "quartz" => "쿼츠",
            "ruby" => "루비",
            "sapphire" => "사파이어",
            "aquamarine" => "아쿠아마린",
            "cinnabar" => "주사",
            _ => throw new ArgumentException($"알 수 없는 행성 id: {planetId}"),
        };

        /// <summary>A-17: 화면(뽑기 결과·도감)이 예외 걱정 없이 항상 부를 수 있는 표시 이름.
        /// 1~5등급은 MechanicalDisplayNameKo 그대로. 6·7등급은 정식 이름이 아직 없어
        /// (MechanicalDisplayNameKo가 예외를 던지는 그 자리, PetGradeInfo.EquipBonusFor와 같은
        /// 블로커) 화면이 죽지 않게 자리 표시자 이름을 대신 돌려준다 — 6등급은 계열 + 신화 안
        /// 순번("바퀴족 신화 #03"), 7등급은 이미 design에 고정된 능력 축 이름을 그대로 쓴다
        /// (TranscendentAxisKo, 최소한 뜻은 있는 이름이라 숫자 id보다 낫다). 정식 이름이 정해지면
        /// 이 메서드 안의 6·7등급 분기만 고치면 되고, 부르는 쪽(화면)은 안 바뀐다.</summary>
        public static string DisplayNameKo(PetSpeciesDef def)
        {
            if (def.Grade <= PetGrade.Legendary) return MechanicalDisplayNameKo(def);

            if (def.Grade == PetGrade.Mythic)
            {
                var familyIndex = Array.IndexOf(FamilyOrder, def.Family);
                return $"{FamilyNameKo[familyIndex]} 신화 #{IndexWithinFamily(def) + 1:D2}";
            }

            return $"초월 · {TranscendentAxisKo[IndexWithinGrade(def)]}";
        }

        /// <summary>같은 등급·계열 안에서 이 종이 몇 번째인지(0부터) — PetArt.ResourcePath가
        /// 신화 파일 순번을 찾는 것과 같은 계산이라 여기서도 그대로 쓴다.</summary>
        private static int IndexWithinFamily(PetSpeciesDef def)
        {
            var index = 0;
            foreach (var d in AllSpecies)
            {
                if (d.Grade != def.Grade || d.Family != def.Family) continue;
                if (d.Id == def.Id) return index;
                index++;
            }
            throw new ArgumentException($"종 id {def.Id}를 같은 등급·계열 안에서 찾지 못했다.");
        }

        /// <summary>같은 등급 안에서 이 종이 몇 번째인지(0부터) — 초월 10종의 축 순서를 찾는다.</summary>
        private static int IndexWithinGrade(PetSpeciesDef def)
        {
            var index = 0;
            foreach (var d in AllSpecies)
            {
                if (d.Grade != def.Grade) continue;
                if (d.Id == def.Id) return index;
                index++;
            }
            throw new ArgumentException($"종 id {def.Id}를 같은 등급 안에서 찾지 못했다.");
        }
    }
}
