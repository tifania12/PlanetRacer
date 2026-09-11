using System.Collections.Generic;
using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>
    /// 행성 하나의 겉모습 한 벌. 지면 텍스처·하늘색·환경광·장식 색을 묶어 둔다.
    /// 행성을 늘릴 때 여기 한 줄만 추가하면 화면 전체 색이 바뀐다.
    ///
    /// 같은 에셋을 써도 행성마다 색이 통째로 달라지면 여섯 행성이 전혀 다른 화면으로 보인다.
    /// 로우폴리 조합이 "어디서 본 것 같다"는 소리를 듣지 않게 하는 지점이 여기다.
    ///
    /// 지면 텍스처는 tools/gen_planet_texture.py로 만든다. AI 생성 대신 코드로 만드는 이유는
    /// 감싸도는 거리로 계산해 이음새가 원천적으로 없고, 행성을 늘려도 비용이 0이기 때문이다.
    /// </summary>
    [System.Serializable]
    public struct PlanetLook
    {
        public string Id;
        public string NameKo;
        /// <summary>Assets 기준 지면 타일 텍스처 경로. 비면 단색으로 간다.</summary>
        public string GroundTexturePath;
        /// <summary>지면 타일 하나의 크기(m). 9m 근처가 속도감이 잘 읽혔다.</summary>
        public float GroundTileMeters;
        public Color SkyColor;
        /// <summary>지면에서 올라오는 반사광. 하늘색과 지면색 사이로 잡는다.</summary>
        public Color AmbientColor;
        public Color RockColor;
        public Color CrystalColor;
        public Color SunColor;
        public float SunIntensity;
        /// <summary>차가 배경에 묻히면 안 된다. 붉은 행성엔 밝은 차, 밝은 행성엔 짙은 차.</summary>
        public Color CarColor;

        static Color C(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);
        static string Tex(string id) => $"Assets/Art/Textures/planet_{id}.png";

        public static PlanetLook Quartz => new PlanetLook
        {
            Id = "quartz", NameKo = "쿼츠", GroundTexturePath = Tex("quartz"), GroundTileMeters = 9f,
            SkyColor = C(96, 122, 148), AmbientColor = C(150, 158, 170),
            RockColor = C(132, 138, 150), CrystalColor = C(210, 232, 248),
            SunColor = Color.white, SunIntensity = 1.15f, CarColor = C(196, 62, 48),
        };

        public static PlanetLook Ruby => new PlanetLook
        {
            Id = "ruby", NameKo = "루비", GroundTexturePath = Tex("ruby"), GroundTileMeters = 9f,
            SkyColor = C(41, 13, 26), AmbientColor = C(107, 51, 61),
            RockColor = C(133, 77, 79), CrystalColor = C(255, 107, 122),
            SunColor = C(255, 235, 219), SunIntensity = 1.35f, CarColor = C(242, 237, 224),
        };

        public static PlanetLook Sapphire => new PlanetLook
        {
            Id = "sapphire", NameKo = "사파이어", GroundTexturePath = Tex("sapphire"), GroundTileMeters = 9f,
            SkyColor = C(14, 22, 48), AmbientColor = C(56, 78, 128),
            RockColor = C(72, 92, 140), CrystalColor = C(120, 186, 248),
            SunColor = C(222, 236, 255), SunIntensity = 1.25f, CarColor = C(248, 214, 120),
        };

        public static PlanetLook Aquamarine => new PlanetLook
        {
            Id = "aquamarine", NameKo = "아쿠아마린", GroundTexturePath = Tex("aquamarine"), GroundTileMeters = 9f,
            SkyColor = C(30, 74, 82), AmbientColor = C(118, 176, 174),
            RockColor = C(110, 156, 154), CrystalColor = C(196, 246, 238),
            SunColor = C(255, 250, 234), SunIntensity = 1.2f, CarColor = C(226, 92, 70),
        };

        public static PlanetLook Cinnabar => new PlanetLook
        {
            Id = "cinnabar", NameKo = "주사", GroundTexturePath = Tex("cinnabar"), GroundTileMeters = 9f,
            SkyColor = C(54, 26, 16), AmbientColor = C(140, 82, 56),
            RockColor = C(146, 96, 70), CrystalColor = C(255, 158, 84),
            SunColor = C(255, 226, 190), SunIntensity = 1.3f, CarColor = C(226, 238, 244),
        };

        public static PlanetLook Lapis => new PlanetLook
        {
            Id = "lapis", NameKo = "라피스 라줄리", GroundTexturePath = Tex("lapis"), GroundTileMeters = 9f,
            SkyColor = C(8, 10, 30), AmbientColor = C(48, 60, 116),
            RockColor = C(62, 74, 128), CrystalColor = C(240, 212, 120),
            SunColor = C(214, 226, 255), SunIntensity = 1.1f, CarColor = C(255, 176, 72),
        };

        /// <summary>기획서 행성 순서대로.</summary>
        public static IReadOnlyList<PlanetLook> All => new[]
        {
            Quartz, Ruby, Sapphire, Aquamarine, Cinnabar, Lapis
        };

        public static PlanetLook ById(string id)
        {
            foreach (var p in All) if (p.Id == id) return p;
            Debug.LogWarning($"[GemRacer] 모르는 행성 id: {id}. 쿼츠로 대체한다.");
            return Quartz;
        }
    }
}
