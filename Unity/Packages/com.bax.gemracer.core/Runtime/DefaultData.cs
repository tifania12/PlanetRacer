using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>
    /// 시트→ScriptableObject 파이프라인이 생기기 전까지 쓰는 기본 데이터.
    /// P0 관문용 초안 값. 밸런스는 P4 봇 시뮬레이션에서 다시 잡는다.
    /// </summary>
    public static class DefaultData
    {
        public static List<Planet> Planets() => new List<Planet>
        {
            new Planet { Id = "quartz",    NameKo = "쿼츠",        Order = 1, Circumference = 1000, VeinCount = 14, VeinYield = 20 },
            new Planet { Id = "ruby",      NameKo = "루비",        Order = 2, Circumference = 1300, Heat = 0.9f, Roughness = 0.6f, VeinCount = 12, VeinYield = 26 },
            new Planet { Id = "sapphire",  NameKo = "사파이어",    Order = 3, Circumference = 1500, Cold = 0.9f, Roughness = 0.4f, VeinCount = 12, VeinYield = 32 },
            new Planet { Id = "aquamarine",NameKo = "아쿠아마린",  Order = 4, Circumference = 1700, Liquid = 0.5f, VeinCount = 10, VeinYield = 40 },
            new Planet { Id = "cinnabar",  NameKo = "주사",        Order = 5, Circumference = 1900, Toxic = 0.7f, Roughness = 0.7f, VeinCount = 10, VeinYield = 50 },
            new Planet { Id = "lapis",     NameKo = "라피스 라줄리",Order = 6, Circumference = 2200, Gravity = 0.25f, Atmosphere = 0.15f, VeinCount = 8, VeinYield = 64 },
        };

        /// <summary>쿼츠 행성의 로컬 레이스 3개. P1 프로토타입 범위.</summary>
        public static List<Course> QuartzCourses() => new List<Course>
        {
            new Course { Id = "quartz_local_1", NameKo = "석영 평원 스프린트", PlanetId = "quartz", Length = 600,  Laps = 1, FlatRatio = 0.7f, RoughRatio = 0.2f, BoostRatio = 0.1f },
            new Course { Id = "quartz_local_2", NameKo = "결정 능선 루프",    PlanetId = "quartz", Length = 800,  Laps = 1, FlatRatio = 0.4f, RoughRatio = 0.5f, BoostRatio = 0.1f },
            new Course { Id = "quartz_local_3", NameKo = "반사면 직선로",     PlanetId = "quartz", Length = 500,  Laps = 2, FlatRatio = 0.5f, RoughRatio = 0.1f, BoostRatio = 0.4f },
        };

        /// <summary>C등급 기본 부품 한 세트(쿼츠). 제작 비용은 정제 광물 단위.</summary>
        public static List<Part> QuartzStarterParts() => new List<Part>
        {
            new Part { Id = "q_engine_c", NameKo = "석영 엔진",     Slot = PartSlot.Engine,     Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Power = 30 } },
            new Part { Id = "q_tire_c",   NameKo = "석영 타이어",   Slot = PartSlot.Tire,       Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Grip = 30 } },
            new Part { Id = "q_susp_c",   NameKo = "석영 서스펜션", Slot = PartSlot.Suspension, Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Suspension = 30 } },
            new Part { Id = "q_body_c",   NameKo = "석영 차체",     Slot = PartSlot.Body,       Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Durability = 30, Aero = 10 } },
            new Part { Id = "q_boost_c",  NameKo = "석영 부스터",   Slot = PartSlot.Booster,    Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Boost = 30 } },
        };

        /// <summary>C등급 부품 제작 비용(정제 광물). 첫 부품까지 5분 목표에 맞춘 값.</summary>
        public const float PartCostC = 15f;
    }
}
