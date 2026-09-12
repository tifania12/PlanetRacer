using System;
using System.Collections.Generic;
using UnityEngine;
using GemRacer.Core;
// `Assets/Scripts/Planet/` 쪽이 네임스페이스를 GemRacer.Planet으로 쓰고 있어서,
// 여기서 그냥 Planet이라고 쓰면 컴파일러가 코어의 Planet 타입 대신 그 네임스페이스로
// 해석해 버린다(CS0118). 그래서 코어 타입만 별칭을 준다.
using CorePlanet = GemRacer.Core.Planet;

namespace GemRacer.Data
{
    /// <summary>
    /// D02-N: docs/design/balance/*.csv를 가져와 담아 두는 ScriptableObject.
    /// Assets/Editor/ImportBalance.cs(메뉴 "GemRacer/2. 밸런스 CSV 가져오기")가 채운다.
    /// 코어의 Planet/Course/Part는 UnityEngine을 못 쓰는 순수 클래스라 인스펙터에 안 나오므로,
    /// 여기서는 필드가 그대로 보이는 값 타입(Entry)에 담고, ToXxx()로 코어 모델로 바꿔 쓴다.
    /// 이 에셋이 없거나 비어 있을 때는 지금처럼 Core.DefaultData를 폴백으로 계속 쓴다.
    /// </summary>
    [CreateAssetMenu(menuName = "GemRacer/밸런스 테이블", fileName = "Balance")]
    public sealed class BalanceTable : ScriptableObject
    {
        [Serializable]
        public struct PlanetEntry
        {
            public string id, nameKo;
            public int order;
            public float circumference, heat, cold, roughness, liquid, toxic, gravity, atmosphere;
            public int veinCount;
            public float veinYield;
        }

        [Serializable]
        public struct CourseEntry
        {
            public string id, nameKo, planetId;
            public float length;
            public int laps;
            public float flatRatio, roughRatio, boostRatio;
        }

        [Serializable]
        public struct PartEntry
        {
            public string id, nameKo, slot, grade, planetId;
            public float power, grip, suspension, durability, boost, aero, heatResist, seal, filter;
        }

        public List<PlanetEntry> planets = new List<PlanetEntry>();
        public List<CourseEntry> courses = new List<CourseEntry>();
        public List<PartEntry> parts = new List<PartEntry>();

        public List<CorePlanet> ToPlanets()
        {
            var list = new List<CorePlanet>(planets.Count);
            foreach (var e in planets)
                list.Add(new CorePlanet
                {
                    Id = e.id, NameKo = e.nameKo, Order = e.order, Circumference = e.circumference,
                    Heat = e.heat, Cold = e.cold, Roughness = e.roughness, Liquid = e.liquid,
                    Toxic = e.toxic, Gravity = e.gravity, Atmosphere = e.atmosphere,
                    VeinCount = e.veinCount, VeinYield = e.veinYield,
                });
            return list;
        }

        public List<Course> ToCourses()
        {
            var list = new List<Course>(courses.Count);
            foreach (var e in courses)
                list.Add(new Course
                {
                    Id = e.id, NameKo = e.nameKo, PlanetId = e.planetId, Length = e.length, Laps = e.laps,
                    FlatRatio = e.flatRatio, RoughRatio = e.roughRatio, BoostRatio = e.boostRatio,
                });
            return list;
        }

        public List<Part> ToParts()
        {
            var list = new List<Part>(parts.Count);
            foreach (var e in parts)
                list.Add(new Part
                {
                    Id = e.id, NameKo = e.nameKo,
                    Slot = (PartSlot)Enum.Parse(typeof(PartSlot), e.slot),
                    Grade = (PartGrade)Enum.Parse(typeof(PartGrade), e.grade),
                    PlanetId = e.planetId,
                    Base = new Stats
                    {
                        Power = e.power, Grip = e.grip, Suspension = e.suspension, Durability = e.durability,
                        Boost = e.boost, Aero = e.aero, HeatResist = e.heatResist, Seal = e.seal, Filter = e.filter,
                    }
                });
            return list;
        }
    }
}
