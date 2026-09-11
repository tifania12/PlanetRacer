using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>보석 행성. 환경 파라미터는 0~1 정규화 값, 0.5가 중간.</summary>
    public sealed class Planet
    {
        public string Id = "";
        public string NameKo = "";
        public int Order;
        /// <summary>행성 둘레(m). 채굴차·레이싱카 한 바퀴 거리.</summary>
        public float Circumference = 1200f;
        public float Heat = 0.5f;        // 고온 → 엔진 효율 저하
        public float Cold = 0.5f;        // 저온 → 접지 저하
        public float Roughness = 0.5f;   // 거친 지형 → 내구 소모, 험지 속도 페널티
        public float Liquid = 0f;        // 액체 구간 비율
        public float Toxic = 0f;         // 독성 대기 → 지속 내구 감소
        public float Gravity = 0.5f;     // 0.5 = 1g
        public float Atmosphere = 0.5f;  // 희박 → 부스터 효율↑, 최고속도↑
        /// <summary>광맥 밀도. 둘레당 광맥 개수.</summary>
        public int VeinCount = 12;
        /// <summary>광맥 하나의 기본 매장량(정제 전 원석 단위).</summary>
        public float VeinYield = 20f;
    }

    /// <summary>채굴차. 장비 레벨은 1부터.</summary>
    public sealed class MiningRig
    {
        public int ToolLevel = 1;      // 곡괭이→드릴→레이저, 1~30 (10단계씩 티어)
        public int CargoLevel = 1;     // 화물칸 1~10
        public int EngineLevel = 1;    // 채굴차 엔진 1~10
        public int DetectorLevel = 0;  // 탐지기 0~5
        public int RefineryLevel = 0;  // 제련소 0~5
    }

    public enum PartSlot { Engine, Tire, Suspension, Body, Booster, Module }
    public enum PartGrade { C, B, A, S }

    public sealed class Part
    {
        public string Id = "";
        public string NameKo = "";
        public PartSlot Slot;
        public PartGrade Grade;
        public string PlanetId = "";   // 세트 판정용
        public int Enhance;            // +0 ~ +10
        public Stats Base = new Stats();

        public Stats Effective()
        {
            // 강화 1당 +6%. +10에서 1.6배.
            var m = 1f + Enhance * 0.06f;
            return Base.Scale(m);
        }
    }

    /// <summary>레이싱카 스탯. 모두 양수, 대략 10~200 범위.</summary>
    public struct Stats
    {
        public float Power, Grip, Suspension, Durability, Boost, Aero, HeatResist, Seal, Filter;

        public Stats Scale(float m) => new Stats
        {
            Power = Power * m, Grip = Grip * m, Suspension = Suspension * m, Durability = Durability * m,
            Boost = Boost * m, Aero = Aero * m, HeatResist = HeatResist * m, Seal = Seal * m, Filter = Filter * m
        };

        public static Stats operator +(Stats a, Stats b) => new Stats
        {
            Power = a.Power + b.Power, Grip = a.Grip + b.Grip, Suspension = a.Suspension + b.Suspension,
            Durability = a.Durability + b.Durability, Boost = a.Boost + b.Boost, Aero = a.Aero + b.Aero,
            HeatResist = a.HeatResist + b.HeatResist, Seal = a.Seal + b.Seal, Filter = a.Filter + b.Filter
        };
    }

    public sealed class RacingCar
    {
        public readonly Dictionary<PartSlot, Part?> Slots = new Dictionary<PartSlot, Part?>
        {
            { PartSlot.Engine, null }, { PartSlot.Tire, null }, { PartSlot.Suspension, null },
            { PartSlot.Body, null }, { PartSlot.Booster, null }, { PartSlot.Module, null }
        };

        public Stats TotalStats()
        {
            // 부품이 비어 있으면 최소 기본치를 준다. 차가 아예 안 움직이는 상황은 없다.
            var s = new Stats { Power = 10, Grip = 10, Suspension = 10, Durability = 10, Boost = 0, Aero = 5 };
            foreach (var p in Slots.Values) if (p != null) s += p.Effective();
            return s;
        }

        /// <summary>같은 행성 부품 개수(세트 판정). 가장 많은 행성의 개수를 돌려준다.</summary>
        public int SetCount(out string planetId)
        {
            var count = new Dictionary<string, int>();
            foreach (var p in Slots.Values)
                if (p != null && p.PlanetId.Length > 0)
                    count[p.PlanetId] = count.TryGetValue(p.PlanetId, out var c) ? c + 1 : 1;
            planetId = ""; var best = 0;
            foreach (var kv in count) if (kv.Value > best) { best = kv.Value; planetId = kv.Key; }
            return best;
        }
    }
}
