using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>레이스 등급(GDD "레이스" 항목: 행성당 로컬 5·서킷 3·챌린지 1·그랑프리 1).
    /// 등급별 4등급 해금 구조 자체는 아직 안 만들었다(docs/backlog.md W2에서) — 여기서는 우승 시
    /// 어떤 공구 상자가 나오는지(RaceBoxReward.ForTier)만 이 값으로 정한다.</summary>
    public enum RaceTier { Local, Circuit, Challenge, GrandPrix }

    /// <summary>레이스 코스. 행성 위 한 구간. 세그먼트 비율의 합은 1.</summary>
    public sealed class Course
    {
        public string Id = "";
        public string NameKo = "";
        public string PlanetId = "";
        public float Length = 1200f;     // m
        public int Laps = 1;
        public float FlatRatio = 0.5f;   // 평지: 출력·공력
        public float RoughRatio = 0.3f;  // 험지: 접지·서스펜션
        public float BoostRatio = 0.2f;  // 가속 구간: 부스터
        public RaceTier Tier = RaceTier.Local;
    }

    /// <summary>
    /// 레이스 결과를 스탯만으로 결정한다. 난수는 seed 하나에서만 나온다.
    /// 같은 입력이면 같은 결과. 서버가 이 코드를 그대로 돌려 검증한다.
    /// </summary>
    public static class RaceSimulator
    {
        /// <summary>코스 완주 시간(초). 낮을수록 좋다.</summary>
        public static float LapTime(Stats s, Planet planet, Course course)
        {
            // 각 구간 속도(m/s). 스탯 100 기준으로 대략 40m/s.
            var heatPenalty = 1f - 0.5f * Clamp01((planet.Heat - 0.5f) * 2f) * (1f - Sat(s.HeatResist, 80f));
            var coldPenalty = 1f - 0.4f * Clamp01((planet.Cold - 0.5f) * 2f) * (1f - Sat(s.Grip, 120f));
            var toxicPenalty = 1f - 0.6f * planet.Toxic * (1f - Sat(s.Filter, 60f));
            var liquidPenalty = 1f - 0.5f * planet.Liquid * (1f - Sat(s.Seal, 60f));
            var gravity = 1f - 0.3f * (planet.Gravity - 0.5f) * 2f;              // 고중력이면 느려짐
            var thinAir = 1f + 0.25f * (0.5f - planet.Atmosphere) * 2f;           // 희박하면 최고속↑

            var env = heatPenalty * coldPenalty * toxicPenalty * liquidPenalty;

            var flatSpeed = (20f + 0.2f * s.Power + 0.08f * s.Aero) * gravity * thinAir * env;
            var roughSpeed = (12f + 0.12f * s.Grip + 0.12f * s.Suspension) * (1f - 0.5f * Clamp01((planet.Roughness - 0.5f) * 2f) * (1f - Sat(s.Suspension, 100f))) * env;
            var boostSpeed = (20f + 0.2f * s.Power + 0.25f * s.Boost) * thinAir * thinAir * env;

            var total = course.Length * course.Laps;
            var t = total * course.FlatRatio / Math.Max(1f, flatSpeed)
                  + total * course.RoughRatio / Math.Max(1f, roughSpeed)
                  + total * course.BoostRatio / Math.Max(1f, boostSpeed);
            return t;
        }

        public sealed class Entrant
        {
            public string Id = "";
            public Stats Stats;
            public bool IsPlayer;
        }

        public struct Result
        {
            public string Id;
            public float Time;
            public int Rank;
        }

        /// <summary>
        /// 출전자 전원을 달리게 하고 순위를 매긴다. 각자의 시간에 ±3% 안의 결정론적 편차를 준다.
        /// 편차는 seed·출전자 순서로만 결정되므로 재현 가능하다.
        /// </summary>
        public static List<Result> Run(IList<Entrant> entrants, Planet planet, Course course, int seed)
        {
            var rng = new DeterministicRandom(seed);
            var results = new List<Result>(entrants.Count);
            foreach (var e in entrants)
            {
                var baseTime = LapTime(e.Stats, planet, course);
                var jitter = 1f + (rng.NextFloat() * 2f - 1f) * 0.03f;
                results.Add(new Result { Id = e.Id, Time = baseTime * jitter });
            }
            results.Sort((a, b) => a.Time.CompareTo(b.Time));
            for (var i = 0; i < results.Count; i++)
            {
                var r = results[i]; r.Rank = i + 1; results[i] = r;
            }
            return results;
        }

        /// <summary>표준 AI 상대 생성. 코스 난이도(targetTime)에 맞춰 스탯 스케일을 잡는다.
        /// count가 음수면(잘못된 코스 데이터 등) 0명으로 방어한다 — `new List&lt;T&gt;(count)`가
        /// 음수 용량에 ArgumentOutOfRangeException을 던지는 걸 여기서 막는다.</summary>
        public static List<Entrant> MakeOpponents(int count, float strength, int seed)
        {
            count = Math.Max(0, count);
            var rng = new DeterministicRandom(seed ^ 0x5bd1e995);
            var list = new List<Entrant>(count);
            for (var i = 0; i < count; i++)
            {
                var v = strength * (0.85f + rng.NextFloat() * 0.3f);
                list.Add(new Entrant
                {
                    Id = "ai_" + i,
                    Stats = new Stats { Power = v, Grip = v, Suspension = v * 0.8f, Durability = v, Boost = v * 0.5f, Aero = v * 0.5f, HeatResist = v * 0.4f, Seal = v * 0.4f, Filter = v * 0.4f }
                });
            }
            return list;
        }

        /// <summary>0..1 포화 함수. stat이 half일 때 0.5.</summary>
        static float Sat(float stat, float half) => stat <= 0 ? 0f : stat / (stat + half);
        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }

    /// <summary>플랫폼·런타임에 관계없이 같은 수열을 내는 xorshift32.</summary>
    public sealed class DeterministicRandom
    {
        uint _state;
        public DeterministicRandom(int seed) { _state = (uint)seed == 0 ? 0x9E3779B9u : (uint)seed; }
        public uint NextUInt()
        {
            var x = _state;
            x ^= x << 13; x ^= x >> 17; x ^= x << 5;
            _state = x; return x;
        }
        /// <summary>[0,1)</summary>
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);
        public int NextInt(int minInclusive, int maxExclusive) => minInclusive + (int)(NextUInt() % (uint)(maxExclusive - minInclusive));
    }
}
