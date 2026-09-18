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

        /// <summary>티어 점프 하나의 배율. 5레벨마다 한 번씩 곱해진다. 1.5^(2/5) ≈ 1.176 —
        /// 5번 곱하면(레벨 30 지점) 옛 10레벨×1.5 두 번(1.5^2 = 2.25)과 정확히 같아진다.</summary>
        public static readonly float TierJumpMultiplier = MathF.Pow(1.5f, 2f / 5f);

        /// <summary>티어 하나의 길이(레벨 수). 2026-09-17 P-02: 10 → 5로 촘촘하게.</summary>
        public const int TierSpanLevels = 5;

        /// <summary>광맥 하나에서 캐는 양. 도구 레벨 1에서 2단위, 레벨당 +15%, 5레벨마다 티어 점프.
        /// 2026-09-17 P-02: 예전엔 10레벨마다 ×1.5 점프가 두 번뿐이라 idle-research.md 3절이 지적한
        /// "긴 구간을 끊는 장치"가 성기었다(AdVenture Capitalist는 25·50·100·200에 배수를 붙인다).
        /// 그렇다고 점프를 더 세게 넣으면 최종 산출량(레벨 30)이 커져서 이미 확정한 행성별 천장표
        /// (planet-progression.md)가 크게 어긋난다 — 그래서 점프 배율을 1.5^(2/5)로 낮춰
        /// **5번의 작은 점프가 30레벨 지점에서 예전 2번의 큰 점프(누적 ×2.25)와 정확히 같은 배율이
        /// 되도록** 맞췄다. 끝값은 그대로고 중간(레벨 6·11·16·21·26)만 더 자주 튄다 — 행성별
        /// 천장 레벨은 ±1~2 정도만 움직인다(정확한 새 값은 위 문서에 갱신).</summary>
        public static float YieldPerVein(MiningRig rig, Planet planet)
        {
            var lvl = Math.Max(1, rig.ToolLevel);
            var tier = (lvl - 1) / TierSpanLevels;
            var y = 2f * MathF.Pow(1.15f, lvl - 1) * MathF.Pow(TierJumpMultiplier, tier);
            return Math.Min(y, planet.VeinYield);      // 광맥 매장량을 넘길 수는 없다
        }

        /// <summary>광맥 하나에서 머무는 시간(초). 도구가 좋을수록 짧다. 최소 3초.</summary>
        public static float SecondsPerVein(MiningRig rig)
        {
            return Math.Max(3f, 20f * MathF.Pow(0.93f, rig.ToolLevel - 1));
        }

        /// <summary>시간당 원석 산출 — 화물칸(원석 전용, M-01)을 채우는 값이다. 예전 주석에는
        /// "정제 광물"이라 적혀 있었는데 실제로는 정제 전 원석이다(M-02에서 RefinedMinerals가
        /// 따로 생기면서 드러난 이름-실체 불일치라 바로잡는다). 실제 정제 산출은 RefinePerHour.</summary>
        public static float MineralsPerHour(MiningRig rig, Planet planet)
        {
            var speed = RigSpeed(rig, planet);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            var secondsPerCycle = travelPerVein / speed + SecondsPerVein(rig);
            var veinsPerHour = 3600f / secondsPerCycle;
            return veinsPerHour * YieldPerVein(rig, planet);
        }

        /// <summary>제련소 시간당 원석→정제 변환량(M-02). 0레벨(제련소 없음)은 0. 5레벨(최대)에서
        /// 그 채굴차의 시간당 원석 산출(MineralsPerHour)과 정확히 같아져서, 캐는 만큼 바로 정제되어
        /// 화물칸이 사실상 다시는 안 찬다. 레이스 승리로만 오르는 슬롯이라(RigParts.cs
        /// RigSlot.Refinery) 이게 "돈을 안 써도 화물칸 상한 문제가 풀리는" 무료 해법이다
        /// (docs/design/monetization.md "정제 광물은 화물칸을 차지하지 않는다").</summary>
        /// <summary>제련소 레벨별 정제 비율. 2026-09-17 전에는 lvl/5(0·0.2·0.4·0.6·0.8·1.0)였는데,
        /// 1레벨에서 20%밖에 안 넘어가 정제 광물이 너무 느리게 쌓였다 — 첫 업그레이드까지 24분.
        /// 앞을 올리고 뒤를 완만하게 바꿔서 1레벨을 사는 순간 바로 돌아가는 느낌이 나게 했다.
        /// 0레벨 0과 5레벨 1.0(캐는 만큼 전부 정제)은 그대로다 — 그 두 끝은 설계 문서와 테스트가 잡고 있다.</summary>
        static readonly float[] RefineShare = { 0f, 0.35f, 0.55f, 0.72f, 0.87f, 1f };

        public static float RefinePerHour(MiningRig rig, Planet planet) => RefinePerHour(rig, planet, false);

        /// <summary>2026-09-19: Entitlements.AutoRefineryAlwaysOn(구독 중 자동 제련 상시 켜짐,
        /// monetization.md 2-5)을 나중에 배선할 자리를 미리 만들어 둔 오버로드 — forceFullRefine이
        /// true면 레벨과 무관하게 5레벨(캐는 만큼 전부 정제)과 같은 값을 돌려준다. 기존 2인자
        /// 호출은 전부 false를 넘기는 것과 완전히 같아서(바로 위 오버로드), 이미 2인자로 부르던
        /// 곳은 동작이 하나도 안 바뀐다 — 실제로 구독 여부에 따라 true/false를 갈라 넘기는 배선은
        /// MonoBehaviour 쪽(MiningController)이라 컴파일 확인이 되는 Unity 세션 몫으로 남긴다
        /// (docs/decisions.md "M-06/M-07이 아직 안 붙인 값" 참고).</summary>
        public static float RefinePerHour(MiningRig rig, Planet planet, bool forceFullRefine)
        {
            if (forceFullRefine) return MineralsPerHour(rig, planet);
            var lvl = Clamp(rig.RefineryLevel, 0, 5);
            return MineralsPerHour(rig, planet) * RefineShare[lvl];
        }

        /// <summary>이번 프레임(deltaSeconds) 동안 원석→정제로 실제로 넘어가는 양. 가진 원석보다
        /// 많이 못 넘기고, 음수 델타나 원석 0은 0을 돌려준다. 접속 중(MiningController.Update)
        /// 매 프레임 이 값만큼 RawMinerals를 깎고 RefinedMinerals에 더하는 용도 — 오프라인
        /// 캐치업은 경과 시간이 프레임 단위로 쪼개기엔 너무 길 수 있어(수백 년 단위 테스트 있음)
        /// 이 함수 대신 Offline()의 닫힌 형태 계산을 따로 쓴다(초당 비율 자체는 같다).</summary>
        public static float Refine(float rawMinerals, MiningRig rig, Planet planet, float deltaSeconds) =>
            Refine(rawMinerals, rig, planet, deltaSeconds, false);

        /// <summary>2026-09-19: AutoRefineryAlwaysOn 배선용 오버로드. forceFullRefine은 그대로
        /// RefinePerHour(rig, planet, forceFullRefine)로 넘어간다 — 위 주석 참고.</summary>
        public static float Refine(float rawMinerals, MiningRig rig, Planet planet, float deltaSeconds, bool forceFullRefine)
        {
            if (deltaSeconds <= 0f || rawMinerals <= 0f) return 0f;
            var perSecond = RefinePerHour(rig, planet, forceFullRefine) / 3600f;
            return Math.Min(rawMinerals, perSecond * deltaSeconds);
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
        /// M-01)에 CargoLevel 배율을 곱한다. 2026-09-17 P-01(상한 10→30)부터는 배율이 레벨당 ×1.12
        /// 지수식이다(RigUpgrade.cs Cost의 비용 성장률 1.18과 짝 — 비율 1.054, idle-research.md 1절).
        /// 예전 식(1레벨 ×1~10레벨 ×3 선형)은 10레벨에서 멈추는 걸 전제로 한 것이라 30레벨까지
        /// 못 늘린다 — 1레벨 배율은 그대로 ×1이라 쿼츠 기본 4h는 안 바뀐다.
        /// 오프라인 누적 상한과 접속 중(온라인) 상한이 같은 값을 쓴다(CargoCapacityMinerals).</summary>
        public static float CargoHours(MiningRig rig, Planet planet)
        {
            var lvl = Clamp(rig.CargoLevel, 1, UpgradeCost.CargoMaxLevel);
            var multiplier = MathF.Pow(1.12f, lvl - 1);
            return planet.BaseCargoHours * multiplier;
        }

        /// <summary>화물칸 상한(원석 단위) = 시간당 산출 × 화물칸 상한(시간). 오프라인·온라인이
        /// 같은 이 값을 쓴다. 정제 광물은 여기 안 들어간다 — 원석만 화물칸을 차지한다(M-02).</summary>
        public static float CargoCapacityMinerals(MiningRig rig, Planet planet)
            => MineralsPerHour(rig, planet) * CargoHours(rig, planet);

        /// <summary>접속 중(온라인) 화물칸 상한 적용. 새로 캔 원석을 더한 뒤 상한을 넘으면 자른다
        /// (docs/design/monetization.md "2026-09-14 변경: 접속 중에도 화물칸이 차면 채굴이 멈춘다",
        /// decisions.md T-06 A안으로 해결). Math.Min이라 이미 상한을 넘어 저장돼 있던 값(이 기능이
        /// 생기기 전 세이브 등)도 한 번은 상한까지 깎인다 — 그 뒤로는 다시 늘지 않을 뿐 매 틱 계속
        /// 깎지는 않는다.</summary>
        public static float ClampToCargoCapacity(float rawMinerals, MiningRig rig, Planet planet)
            => Math.Min(rawMinerals, CargoCapacityMinerals(rig, planet));

        /// <summary>오프라인 보상. 원석은 화물칸 상한(M-01)에서 막히지만, 제련소가 있으면 그동안에도
        /// 원석 일부가 계속 정제로 빠져나간다 — 그래서 상한에 닿는 시점이 늦춰지거나(레벨 5면 아예
        /// 안 막힌다) 한다. 이게 M-02 "정제 광물은 화물칸을 차지하지 않는다"가 실제로 상한을
        /// 올리는 방식이다.
        ///
        /// 원석 유입 속도 R(MineralsPerHour), 정제 속도 F(RefinePerHour), 화물칸 상한 Cap이 이
        /// 경과 시간 동안 전부 상수라서 프레임 단위로 안 쪼개고 닫힌 형태로 한 번에 푼다(수백 년
        /// 오프라인도 안전). 오프라인 진입 시점 원석은 항상 0으로 본다 — 접속 중 남아 있던 원석은
        /// 이미 화물칸에 든 값이라 ClaimOfflineReward가 그 위에 이 결과를 더하는 기존 방식 그대로.
        ///
        /// - R &lt;= F(레벨 5, 또는 산출이 극단적으로 낮은 경우): 원석은 들어오는 족족 정제되어
        ///   0 근처에 머물고, 상한을 절대 못 넘는다 — HoursWasted는 항상 0.
        /// - R &gt; F: 원석이 (R-F) 속도로 쌓이다 hoursToCap = Cap / (R-F) 시점에 상한에 닿는다.
        ///   그 뒤로도 채굴 자체는 멈추지 않고 초과분(R-F)만 버려진다 — 그래서 RefinedGained는
        ///   상한을 넘겼든 안 넘겼든 항상 F × 전체 경과 시간이다.
        ///
        /// 알려진 근사: 보물·희귀 광맥 발견(ExplorationSimulator.DiscoverOffline)은 예전 방식
        /// 그대로 HoursCounted(=hoursToCap 이내)만 인정한다 — "화물칸이 차면 채굴차가 멈춘 셈"이라는
        /// 옛 가정인데, 지금은 위에서 보듯 채굴 자체는 안 멈추고 원석만 버려지는 쪽이 맞다. 다만
        /// 발견 로직까지 바꾸는 건 이번 항목(M-02) 범위 밖이라 그대로 뒀다 — 다음에 손볼 것.</summary>
        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds) =>
            Offline(rig, planet, elapsedSeconds, false);

        /// <summary>2026-09-19: AutoRefineryAlwaysOn 배선용 오버로드 — RefinePerHour/Refine과 같은
        /// 패턴이다. forceFullRefine이 true면 제련소 레벨과 무관하게 원석 유입 속도(rate)와 정제
        /// 속도(refineRate)가 같아져서 rate&lt;=refineRate 분기(원석 0, 상한 절대 안 닿음)로 항상
        /// 빠진다 — 구독 중에는 오프라인에서도 접속 중과 똑같이 화물칸이 안 찬다는 뜻이다. 기존
        /// 3인자 호출은 그대로 false를 넘기는 것과 완전히 같다(회귀 없음). 실제로 구독 여부를 여기
        /// 넘기는 배선은 MiningController.ClaimOfflineReward 쪽(Unity 세션 몫)에 남겨 둔다.</summary>
        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds, bool forceFullRefine)
        {
            var hours = (float)Math.Max(0, elapsedSeconds) / 3600f;
            var rate = MineralsPerHour(rig, planet);
            var refineRate = RefinePerHour(rig, planet, forceFullRefine);
            var cap = CargoCapacityMinerals(rig, planet);

            float raw, refined, counted;
            if (rate <= refineRate)
            {
                raw = 0f;
                refined = rate * hours;
                counted = hours;
            }
            else
            {
                var netGrowth = rate - refineRate;
                var hoursToCap = cap / netGrowth;
                counted = Math.Min(hours, hoursToCap);
                raw = netGrowth * counted;          // counted<=hoursToCap이라 절대 cap을 못 넘는다
                refined = refineRate * hours;        // 상한 이후에도 정제는 그대로 F만큼 계속 나온다
            }

            return new OfflineResult
            {
                HoursCounted = counted,
                HoursWasted = Math.Max(0, hours - counted),
                Minerals = raw,
                RefinedGained = refined,
                Gems = GemsPerHour(rig, planet) * counted
            };
        }

        public struct OfflineResult
        {
            public float HoursCounted, HoursWasted, Minerals, RefinedGained, Gems;
        }

        /// <summary>화물칸이 thresholdFraction(0~1) 비율에 닿기까지 남은 시간(시간 단위). M-05
        /// "화물칸 80% 푸시 알림"의 예약 시각 계산 몫 — Offline()과 같은 모델(원석 유입 R=
        /// MineralsPerHour, 정제 F=RefinePerHour, 순증가 R-F)을 그대로 쓴다. 이미 그 비율을
        /// 넘었으면(현재 원석 >= 목표치) 0을 돌려준다. 정제소가 유입을 따라잡아(R&lt;=F, 예:
        /// 제련소 5레벨) 원석이 그 이상 절대 안 쌓이면 도달 자체가 없다는 뜻으로 null을 돌려준다 —
        /// 이 경우 호출 쪽(Unity, 에디터가 있는 세션 몫)은 알림을 예약하지 않아야 한다. 실제
        /// 로컬 알림 API 호출(Unity Mobile Notifications 패키지)은 여기 core에 없다 — 여기는
        /// "언제"만 순수 계산으로 낸다(서버·클라이언트가 같은 값을 내야 하는 값이라).</summary>
        public static float? HoursUntilCargoThreshold(MiningRig rig, Planet planet, float currentRawMinerals, float thresholdFraction)
        {
            var target = CargoCapacityMinerals(rig, planet) * Clamp01(thresholdFraction);
            if (currentRawMinerals >= target) return 0f;

            var netGrowth = MineralsPerHour(rig, planet) - RefinePerHour(rig, planet);
            if (netGrowth <= 0f) return null;

            return (target - currentRawMinerals) / netGrowth;
        }

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
        static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
