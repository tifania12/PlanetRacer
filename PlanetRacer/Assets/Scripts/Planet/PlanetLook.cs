using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>
    /// 행성 하나의 겉모습 한 벌. 지면 텍스처·하늘색·환경광·장식 색을 묶어 둔다.
    /// 행성을 늘릴 때 이 한 줄만 추가하면 화면 전체 색이 바뀐다.
    ///
    /// 같은 에셋을 써도 행성마다 색이 통째로 달라지면 여섯 행성이 전혀 다른 화면으로 보인다.
    /// 로우폴리 에셋 조합이 "어디서 본 것 같다"는 소리를 듣지 않게 하는 지점이 여기다.
    /// </summary>
    [System.Serializable]
    public struct PlanetLook
    {
        public string Id;
        public string NameKo;
        /// <summary>Assets 기준 지면 타일 텍스처 경로. 없으면 BaseColor 단색으로 간다.</summary>
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
        /// <summary>붉은 행성엔 밝은 차, 밝은 행성엔 짙은 차. 차가 배경에 묻히면 안 된다.</summary>
        public Color CarColor;

        public static PlanetLook Ruby => new PlanetLook
        {
            Id = "ruby",
            NameKo = "루비",
            GroundTexturePath = "Assets/Art/Textures/planet_ruby.png",
            GroundTileMeters = 9f,
            SkyColor = new Color(0.16f, 0.05f, 0.10f),
            AmbientColor = new Color(0.42f, 0.20f, 0.24f),
            RockColor = new Color(0.52f, 0.30f, 0.31f),
            CrystalColor = new Color(1.00f, 0.42f, 0.48f),
            SunColor = new Color(1.00f, 0.92f, 0.86f),
            SunIntensity = 1.35f,
            CarColor = new Color(0.95f, 0.93f, 0.88f),
        };

        /// <summary>텍스처가 아직 없는 행성용 임시. 색만 바꿔 둔 것이라 지면이 밋밋하다.</summary>
        public static PlanetLook Quartz => new PlanetLook
        {
            Id = "quartz",
            NameKo = "쿼츠",
            GroundTexturePath = null,
            GroundTileMeters = 9f,
            SkyColor = new Color(0.55f, 0.66f, 0.78f),
            AmbientColor = new Color(0.62f, 0.65f, 0.70f),
            RockColor = new Color(0.52f, 0.55f, 0.62f),
            CrystalColor = new Color(0.55f, 0.78f, 0.95f),
            SunColor = Color.white,
            SunIntensity = 1.15f,
            CarColor = new Color(0.90f, 0.29f, 0.24f),
        };
    }
}
