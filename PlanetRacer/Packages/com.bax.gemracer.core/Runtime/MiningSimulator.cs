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

        /// <summary>화물칸 상한(시간). 행성 기본값(Planet.BaseCargoHours, docs/design/monetization.md
        /// M-01)에 CargoLevel 배율(1레벨 ×1, 10레벨 ×3)을 곱한다 — 쿼츠(기본 4h)는 옛 수식과 그대로
        /// 같다(4h~12h), 후반 행성은 기본값이 더 커서 같은 배율에서도 상한이 더 크다.
        /// 오프라인 누적 상한과 접속 중(온라인) 상한이 같은 값을 쓴다(CargoCapacityMinerals).</summary>
        public static float CargoHours(MiningRig rig, Planet planet)
        {
            var lvl = Clamp(rig.CargoLevel, 1, 10);
            var multiplier = 1f + (3f - 1f) * (lvl - 1) / 9f;
            return planet.BaseCargoHours * multiplier;
        }

        /// <summary>화물칸 상한(원석 단위) = 시간당 산출 × 화물칸 상한(시간). 오프라인·온라인이
        /// 같은 이 값을 쓴다. 정제 광물은 여기 안 들어간다 — 원석만 화물칸을 차지한다(M-02 몫).</summary>
        public static float CargoCapacityMinerals(MiningRig rig, Planet planet)
            => MineralsPerHour(rig, planet) * CargoHours(rig, planet);

        /// <summary>접속 중(온라인) 화물칸 상한 적용. 새로 캔 원석을 더한 뒤 상한을 넘으면 자른다
        /// (docs/design/monetization.md "2026-09-14 변경: 접속 중에도 화물칸이 차면 채굴이 멈춘다",
        /// decisions.md T-06 A안으로 해결). Math.Min이라 이미 상한을 넘어 저장돼 있던 값(이 기능이
        /// 생기기 전 세이브 등)도 한 번은 상한까지 깎인다 — 그 뒤로는 다시 늘지 않을 뿐 매 틱 계속
        /// 깎지는 않는다.</summary>
        public static float ClampToCargoCapacity(float rawMinerals, MiningRig rig, Planet planet)
            => Math.Min(rawMinerals, CargoCapacityMinerals(rig, planet));

        /// <summary>오프라인 보상. 경과 시간을 화물칸 상한으로 자른 뒤 시간당 산출을 곱한다.</summary>
        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds)
        {
            var hours = (float)Math.Max(0, elapsedSeconds) / 3600f;
            var capped = Math.Min(hours, CargoHours(rig, planet));
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
