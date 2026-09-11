using System;

namespace GemRacer.Core
{
    /// <summary>
    /// 채굴차의 시간당 산출을 계산한다. 순수 함수. Unity 의존 없음.
    /// 서버와 클라이언트가 같은 코드를 쓴다.
    /// </summary>
    public static class MiningSimulator
    {
        /// <summary>채굴차 이동 속도(m/s). 엔진 1레벨 4m/s, 레벨당 +12%.</summary>
        public static float RigSpeed(MiningRig rig, Planet planet)
        {
            var baseSpeed = 4f * MathF.Pow(1.12f, rig.EngineLevel - 1);
            // 거친 지형은 속도를 최대 40% 깎는다.
            var terrain = 1f - 0.4f * Clamp01((planet.Roughness - 0.5f) * 2f);
            return baseSpeed * terrain;
        }

        /// <summary>광맥 하나에서 캐는 양. 도구 레벨 1에서 2단위, 레벨당 +15%, 티어(10레벨) 넘을 때 1.5배 점프.</summary>
        public static float YieldPerVein(MiningRig rig, Planet planet)
        {
            var lvl = Math.Max(1, rig.ToolLevel);
            var tier = (lvl - 1) / 10;                 // 0 곡괭이, 1 드릴, 2 레이저
            var y = 2f * MathF.Pow(1.15f, lvl - 1) * MathF.Pow(1.5f, tier);
            return Math.Min(y, planet.VeinYield);      // 광맥 매장량을 넘길 수는 없다
        }

        /// <summary>광맥 하나에서 머무는 시간(초). 도구가 좋을수록 짧다. 최소 3초.</summary>
        public static float SecondsPerVein(MiningRig rig)
        {
            return Math.Max(3f, 20f * MathF.Pow(0.93f, rig.ToolLevel - 1));
        }

        /// <summary>시간당 정제 광물 산출.</summary>
        public static float MineralsPerHour(MiningRig rig, Planet planet)
        {
            var speed = RigSpeed(rig, planet);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            var secondsPerCycle = travelPerVein / speed + SecondsPerVein(rig);
            var veinsPerHour = 3600f / secondsPerCycle;
            return veinsPerHour * YieldPerVein(rig, planet);
        }

        /// <summary>희귀 광맥(보석 원석) 시간당 기대 개수. 탐지기 0이면 0.</summary>
        public static float GemsPerHour(MiningRig rig, Planet planet)
        {
            if (rig.DetectorLevel <= 0) return 0f;
            var speed = RigSpeed(rig, planet);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            var veinsPerHour = 3600f / (travelPerVein / speed + SecondsPerVein(rig));
            var chance = 0.01f * rig.DetectorLevel; // 레벨당 1%
            return veinsPerHour * chance;
        }

        /// <summary>화물칸 상한(시간). 1레벨 4시간, 10레벨 12시간. 오프라인 누적 상한과 같다.</summary>
        public static float CargoHours(MiningRig rig)
        {
            var lvl = Clamp(rig.CargoLevel, 1, 10);
            return 4f + (12f - 4f) * (lvl - 1) / 9f;
        }

        /// <summary>오프라인 보상. 경과 시간을 화물칸 상한으로 자른 뒤 시간당 산출을 곱한다.</summary>
        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds)
        {
            var hours = (float)Math.Max(0, elapsedSeconds) / 3600f;
            var capped = Math.Min(hours, CargoHours(rig));
            return new OfflineResult
            {
                HoursCounted = capped,
                HoursWasted = Math.Max(0, hours - capped),
                Minerals = MineralsPerHour(rig, planet) * capped,
                Gems = GemsPerHour(rig, planet) * capped
            };
        }

        public struct OfflineResult
        {
            public float HoursCounted, HoursWasted, Minerals, Gems;
        }

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
        static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
