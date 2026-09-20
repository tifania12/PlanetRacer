using System;

namespace GemRacer.Core
{
    /// <summary>A-17 준비: 종(PetSpeciesDef) → `Resources/Art/Pets` 파일 경로 변환.
    /// 도감·뽑기 결과 화면이 둘 다 이 매핑이 필요해서(UI 코드 두 벌에 흩어지면 어긋나기 쉬워서)
    /// 화면보다 먼저 코어에 둔다 — Unity 없이도 `Core.Tests`로 파일명 규칙을 검증할 수 있다.
    /// 실제 스프라이트 로드(`Resources.Load`)는 UI 쪽(`UiKit`류)이 이 경로 문자열을 받아서 한다.
    ///
    /// 등급마다 폴더 밑 파일명 규칙이 다르다(2026-09-21 조사, `Assets/Resources/Art/Pets` 실제
    /// 파일 목록으로 확인) — 하나로 못 묶는 이유이자 이 클래스가 있는 이유다.
    /// - 1등급(일반)·5등급(전설): `{계열}-{색}` — 쿼츠도 색 접미사를 붙인다.
    /// - 2~4등급(고급·희귀·에픽): `{계열}-{색}`이지만 **쿼츠만 접미사 없이 `{계열}` 그대로**다.
    /// - 6등급(신화): `{계열}-{두 자리 순번}` — 계열 안에서 Build()가 만든 순서(0부터) + 1.
    /// - 7등급(초월): 계열·색과 무관한 고유 이름 10개 — `art-requests.md`에 이미 "초월 N/10 —
    ///   이름 (축)"으로 축 순서(TranscendentAxisKo)와 나란히 기록돼 있어 그 순서를 그대로 썼다.
    /// </summary>
    public static class PetArt
    {
        /// <summary>PetGrade(Common=0 ~ Transcendent=6) 인덱스와 같은 순서.</summary>
        public static readonly string[] GradeFolder =
        {
            "1-common", "2-base", "3-base", "4-base", "5-legend", "6-myth", "7-transcend",
        };

        /// <summary>PetFamily(Wheel/Wing/Ore/Cargo) 인덱스와 같은 순서. Cargo만 파일명이
        /// "haul"이라(짐꾼족 = 나르다) enum 이름과 다르다 — 그대로 매핑하면 파일을 못 찾는다.</summary>
        static readonly string[] FamilyFilePrefix = { "wheel", "wing", "ore", "haul" };

        /// <summary>초월 10종의 파일명 — `TranscendentAxisKo`(채굴 산출ㆍ정제 속도ㆍ...) 순서와
        /// 정확히 같다(`docs/design/art-requests.md` "들어온 것" 절, 초월 1/10~10/10 기록 그대로).</summary>
        static readonly string[] TranscendentFile =
        {
            "drill-sovereign", "refinery-sage", "vault-titan", "comet-racer", "burst-phoenix",
            "fortune-key", "beacon-herald", "dream-keeper", "shard-weaver", "ember-heart",
        };

        /// <summary>`Resources.Load&lt;Sprite&gt;`에 그대로 넘길 수 있는 경로(확장자 없음),
        /// 예: "Art/Pets/6-myth/wheel-03". 파일이 실제로 있는지는 확인하지 않는다 — 없으면
        /// 호출부(Resources.Load)가 null을 돌려주니 그쪽에서 조용히 자리 표시자로 넘어간다
        /// (2026-09-15 "이미지가 없다고 작업을 멈추지 않는다" 원칙, `6-myth/ore-06.png` 누락 대응).</summary>
        public static string ResourcePath(PetSpeciesDef def)
        {
            var folder = GradeFolder[(int)def.Grade];
            var prefix = FamilyFilePrefix[(int)def.Family];
            string file;
            switch (def.Grade)
            {
                case PetGrade.Transcendent:
                    file = TranscendentFile[IndexWithinGrade(def)];
                    break;
                case PetGrade.Advanced:
                case PetGrade.Rare:
                case PetGrade.Epic:
                    file = def.PlanetId == "quartz" ? prefix : $"{prefix}-{def.PlanetId}";
                    break;
                case PetGrade.Mythic:
                    file = $"{prefix}-{(IndexWithinFamily(def) + 1):D2}";
                    break;
                default: // Common, Legendary: 색 접미사를 항상 붙인다(쿼츠 포함).
                    file = $"{prefix}-{def.PlanetId}";
                    break;
            }
            return $"Art/Pets/{folder}/{file}";
        }

        /// <summary>같은 등급·계열 안에서 이 종이 몇 번째인지(0부터) — `PetSpeciesTable.Build()`가
        /// 만든 순서 그대로이므로 신화 파일 순번(01~08)과 바로 대응한다.</summary>
        static int IndexWithinFamily(PetSpeciesDef def)
        {
            var index = 0;
            foreach (var d in PetSpeciesTable.All)
            {
                if (d.Grade != def.Grade || d.Family != def.Family) continue;
                if (d.Id == def.Id) return index;
                index++;
            }
            throw new ArgumentException($"종 id {def.Id}를 같은 등급·계열 안에서 찾지 못했다.");
        }

        /// <summary>같은 등급 안에서 이 종이 몇 번째인지(0부터) — 초월 10종의 축 순서(=파일 순서)를
        /// 찾는 데 쓴다.</summary>
        static int IndexWithinGrade(PetSpeciesDef def)
        {
            var index = 0;
            foreach (var d in PetSpeciesTable.All)
            {
                if (d.Grade != def.Grade) continue;
                if (d.Id == def.Id) return index;
                index++;
            }
            throw new ArgumentException($"종 id {def.Id}를 같은 등급 안에서 찾지 못했다.");
        }
    }
}
