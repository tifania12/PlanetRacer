using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>
    /// docs/backlog.md P-10, docs/design/planet-progression.md 6절.
    /// 코스를 행성마다 손으로 수십 개 만들 수 없어서, 길이·랩·구간 비율 네 값을 시드에서 뽑는다.
    /// 보이는 지형(조각 조립)은 별개 작업(P-11) — 이 클래스는 데이터만 만든다.
    /// 등급별 길이·랩 범위는 손으로 만든 쿼츠 로컬 3개(500~800m, 1~2랩)를 기준으로
    /// 등급이 오를수록 길고 랩이 늘게 잡은 첫 값이라, 실제로 달려 보고 조정될 여지가 있다.
    /// </summary>
    public static class CourseGenerator
    {
        struct TierRange
        {
            public float MinLength, MaxLength;
            public int MinLaps, MaxLaps;
        }

        static TierRange RangeFor(RaceTier tier) => tier switch
        {
            RaceTier.Local => new TierRange { MinLength = 400f, MaxLength = 900f, MinLaps = 1, MaxLaps = 2 },
            RaceTier.Circuit => new TierRange { MinLength = 800f, MaxLength = 1500f, MinLaps = 1, MaxLaps = 3 },
            RaceTier.Challenge => new TierRange { MinLength = 1200f, MaxLength = 2000f, MinLaps = 2, MaxLaps = 3 },
            RaceTier.GrandPrix => new TierRange { MinLength = 1800f, MaxLength = 3000f, MinLaps = 3, MaxLaps = 4 },
            _ => throw new ArgumentOutOfRangeException(nameof(tier)),
        };

        static string TierTag(RaceTier tier) => tier switch
        {
            RaceTier.Local => "local",
            RaceTier.Circuit => "circuit",
            RaceTier.Challenge => "challenge",
            RaceTier.GrandPrix => "grandprix",
            _ => throw new ArgumentOutOfRangeException(nameof(tier)),
        };

        static string TierNameKo(RaceTier tier) => tier switch
        {
            RaceTier.Local => "로컬",
            RaceTier.Circuit => "서킷",
            RaceTier.Challenge => "챌린지",
            RaceTier.GrandPrix => "그랑프리",
            _ => throw new ArgumentOutOfRangeException(nameof(tier)),
        };

        /// <summary>
        /// 코스 하나를 시드에서 뽑는다. 같은 (planetId, tier, index, seed)면 항상 같은 코스가 나온다.
        /// 세그먼트 비율(평지/험지/부스트) 셋은 [0,1) 안에서 두 절단점을 뽑아 정렬하는 방식으로
        /// 합이 정확히 1이 되게 한다.
        /// </summary>
        public static Course Generate(string planetId, RaceTier tier, int index, int seed)
        {
            var rng = new DeterministicRandom(seed);
            var range = RangeFor(tier);

            var length = range.MinLength + rng.NextFloat() * (range.MaxLength - range.MinLength);
            var laps = rng.NextInt(range.MinLaps, range.MaxLaps + 1);

            var a = rng.NextFloat();
            var b = rng.NextFloat();
            if (a > b) { (a, b) = (b, a); }
            var flat = a;
            var rough = b - a;
            var boost = 1f - b;

            return new Course
            {
                Id = $"{planetId}_{TierTag(tier)}_{index}",
                NameKo = $"{TierNameKo(tier)} 코스 {index}",
                PlanetId = planetId,
                Length = length,
                Laps = laps,
                FlatRatio = flat,
                RoughRatio = rough,
                BoostRatio = boost,
                Tier = tier,
            };
        }

        /// <summary>count개를 한 번에 뽑는다. i번째 코스는 seed(baseSeed+i)로 만들어져 서로 다르다.</summary>
        public static List<Course> GenerateMany(string planetId, RaceTier tier, int count, int baseSeed)
        {
            var result = new List<Course>(count);
            for (var i = 0; i < count; i++)
                result.Add(Generate(planetId, tier, i + 1, baseSeed + i));
            return result;
        }
    }
}
