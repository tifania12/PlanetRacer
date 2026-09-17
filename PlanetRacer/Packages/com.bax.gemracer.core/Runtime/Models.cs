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
        /// <summary>화물칸 기본 상한(시간) — "그 행성 기준 N시간치 산출"(docs/design/monetization.md,
        /// M-01). 후반 행성일수록 커서 같은 압력이 걸린다. MiningRig.CargoLevel은 여기에 배율로
        /// 곱해질 뿐이다(MiningSimulator.CargoHours).</summary>
        public float BaseCargoHours = 4f;
    }

    /// <summary>채굴차. 장비 레벨은 1부터.
    /// [Serializable]은 System 표준 애트리뷰트라 UnityEngine을 참조하지 않는다(CLAUDE.md 1번) —
    /// MiningController(Assets/Scripts)가 인스펙터에 그대로 노출해 값을 바로 조정해 볼 수 있게 붙였다.</summary>
    [Serializable]
    public sealed class MiningRig
    {
        public int ToolLevel = 1;      // 곡괭이→드릴→레이저, 1~30 (10단계씩 티어)
        public int CargoLevel = 1;     // 화물칸 1~30 (2026-09-17 P-01: 10→30)
        public int EngineLevel = 1;    // 채굴차 엔진 1~30 (2026-09-17 P-01: 10→30)
        public int DetectorLevel = 0;  // 탐지기 0~5
        public int RefineryLevel = 0;  // 제련소 0~5
    }

    public enum PartSlot { Engine, Tire, Suspension, Body, Booster, Module }
    public enum PartGrade { C, B, A, S }

    /// <summary>보물 등급. 등급이 높을수록 요구 채굴 도구 레벨도 높다(docs/design/core-loop.md 참고).</summary>
    public enum TreasureGrade { C, B, A, S }

    /// <summary>보물 종류 정의. 발견은 등급과 무관하게 된다 — 캐려면(선택) 도구 레벨 조건을 만족해야 한다.
    /// 못 캐도 목록에는 남아서 "도구를 올려야 캘 수 있다"는 다음 목표가 된다.</summary>
    public sealed class TreasureDef
    {
        public string Id = "";
        public string NameKo = "";
        public TreasureGrade Grade;
        /// <summary>캐는 데 필요한 최소 MiningRig.ToolLevel.</summary>
        public int RequiredToolLevel;
        /// <summary>캤을 때 얻는 정제 광물 환산치. 플레이스홀더 — 구체 수치는 P4 봇 시뮬레이션에서 재조정.</summary>
        public float MineralValue;
    }

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
