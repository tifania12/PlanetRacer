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
        public static float RigSpeed(MiningRig rig, Planet planet) => RigSpeed(rig, planet, null);

        /// <summary>P-05: 엔진 칸 증폭기 반영판. UpgradeCost.Cost의 "생산(속도) ×1.12"가 Engine
        /// 슬롯의 성능 지표라고 이미 정해 둔 것과 같은 매핑이다 — 새 결정이 아니라 기존 결정을
        /// 증폭기에도 그대로 적용한 것. amp가 null이면 증폭기 없음(기존 동작과 완전히 같다).</summary>
        public static float RigSpeed(MiningRig rig, Planet planet, RigAmplifierSave amp)
        {
            // E-05(2026-09-29): 25레벨마다 이정표 ×2, 단 보호 구간(1~30레벨) 밖에서만
            // (Breakthrough.MilestoneMultiplierBeyond 주석 참고). 지수식 자체는 500레벨까지 안
            // 바꾼다(1.12^499도 float 범위 안이라 별도 구간을 안 나눴다).
            var baseSpeed = 4f * MathF.Pow(1.12f, rig.EngineLevel - 1) *
                (float)Breakthrough.MilestoneMultiplierBeyond(rig.EngineLevel, UpgradeCost.EngineProtectedMaxLevel);
            // 거친 지형은 속도를 최대 40% 깎는다.
            var terrain = 1f - 0.4f * Clamp01((planet.Roughness - 0.5f) * 2f);
            var speed = baseSpeed * terrain;
            return amp == null ? speed : Amplifier.Apply(speed, amp.Engine);
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
        public static float YieldPerVein(MiningRig rig, Planet planet) => YieldPerVein(rig, planet, null);

        /// <summary>P-05: 곡괭이 칸 증폭기 반영판. UpgradeCost.Cost의 "생산 ×1.15"(YieldPerVein)가
        /// Tool 슬롯의 성능 지표라고 이미 정해 둔 매핑을 그대로 쓴다 — SecondsPerVein(체류 시간)은
        /// Tool 레벨의 영향을 받지만 증폭기 대상은 아니다(칸당 "성능"은 amplifier.md가 하나만
        /// 가리킨다).
        /// E-04(2026-09-28, economy-v2.md 3-2): 옛날엔 여기서 `Math.Min(y, planet.VeinYield)`로
        /// 광맥 매장량을 천장으로 썼다 — 도구를 아무리 올려도 그 행성에서는 VeinYield 이상 못 캤다.
        /// 그런데 그러면 행성을 옮길 이유가 "새 천장"이 아니라 "막힌 벽"이 되고, 도구 레벨이 어느
        /// 선을 넘으면 더 올려도 산출이 그대로라 진행의 뜻이 사라진다. 그래서 천장을 없애고,
        /// 대신 VeinYield(쿼츠 20 기준)를 20으로 나눈 값을 **행성 매장 배율**로 곱한다 — 쿼츠 ×1,
        /// 루비 ×1.3(26/20), 사파이어 ×1.6, 아쿠아마린 ×2.0, 주사 ×2.5, 라피스 ×3.2(64/20,
        /// DefaultData.cs 실제 값과 일치). 이제 VeinYield는 "벽"이 아니라 "그 행성에서 캐면
        /// 얼마나 더 버는가"를 뜻한다.</summary>
        /// <summary>E-05(2026-09-29): 보호 구간(1~30레벨) 밖에서 이어 붙이는 성장률. 1.15^레벨에
        /// TierJumpMultiplier^(레벨/5)와 25레벨 이정표(×2)까지 그대로 계속 곱하면 500레벨에서
        /// float 상한(약 3.4e38)을 훨씬 넘어 Infinity가 된다(실측: 1.4e43) — 그래서 31레벨부터는
        /// 5레벨 점프를 끊고 더 완만한 지수 하나로 이어 붙인다. RefineCapacityExtendedGrowth(1.03)와
        /// 같은 값 — 넷 다 "보호 구간 밖은 완만하게, 이정표로 숨통 튼다"는 같은 철학을 쓴다.</summary>
        const float YieldExtendedGrowth = 1.03f;

        public static float YieldPerVein(MiningRig rig, Planet planet, RigAmplifierSave amp)
        {
            var lvl = Math.Max(1, rig.ToolLevel);
            var reserveMultiplier = planet.VeinYield / 20f;
            var protectedMax = UpgradeCost.ToolProtectedMaxLevel;
            float baseYield;
            if (lvl <= protectedMax)
            {
                var tier = (lvl - 1) / TierSpanLevels;
                baseYield = 2f * MathF.Pow(1.15f, lvl - 1) * MathF.Pow(TierJumpMultiplier, tier);
            }
            else
            {
                var tierAtProtectedMax = (protectedMax - 1) / TierSpanLevels;
                var atProtectedMax = 2f * MathF.Pow(1.15f, protectedMax - 1) * MathF.Pow(TierJumpMultiplier, tierAtProtectedMax);
                baseYield = atProtectedMax * MathF.Pow(YieldExtendedGrowth, lvl - protectedMax);
            }
            var y = baseYield * reserveMultiplier;
            // 25레벨마다 이정표 ×2, 단 보호 구간(1~30레벨) 밖에서만(Breakthrough.MilestoneMultiplierBeyond
            // 주석 참고). 5레벨마다의 TierJumpMultiplier(작은 점프)와는 별개 장치다.
            y *= (float)Breakthrough.MilestoneMultiplierBeyond(lvl, protectedMax);
            if (amp != null) y = Amplifier.Apply(y, amp.Tool);
            return y;
        }

        /// <summary>광맥 하나에서 머무는 시간(초). 도구가 좋을수록 짧다. 최소 3초.</summary>
        public static float SecondsPerVein(MiningRig rig)
        {
            return Math.Max(3f, 20f * MathF.Pow(0.93f, rig.ToolLevel - 1));
        }

        /// <summary>시간당 원석 산출 — 화물칸(원석 전용, M-01)을 채우는 값이다. 예전 주석에는
        /// "정제 광물"이라 적혀 있었는데 실제로는 정제 전 원석이다(M-02에서 RefinedMinerals가
        /// 따로 생기면서 드러난 이름-실체 불일치라 바로잡는다). 실제 정제 산출은 RefineCapacity.</summary>
        public static float MineralsPerHour(MiningRig rig, Planet planet) => MineralsPerHour(rig, planet, null);

        /// <summary>P-05: amp-aware판. 여기서는 별도로 증폭기를 적용하지 않는다 — RigSpeed·
        /// YieldPerVein을 amp-aware로 불러서 Engine·Tool 증폭이 이미 이 값 안에 섞여 들어와
        /// 있다(같은 칸을 두 번 증폭하면 안 된다).</summary>
        public static float MineralsPerHour(MiningRig rig, Planet planet, RigAmplifierSave amp)
        {
            var speed = RigSpeed(rig, planet, amp);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            var secondsPerCycle = travelPerVein / speed + SecondsPerVein(rig);
            var veinsPerHour = 3600f / secondsPerCycle;
            return veinsPerHour * YieldPerVein(rig, planet, amp);
        }

        /// <summary>제련소 시간당 원석→정제 변환 처리량(M-02, E-04로 economy-v2.md 3-2에 맞춰 재설계).
        /// 예전(RefinePerHour)엔 "채굴 산출(P)의 몇 %"라는 비율이라 곡괭이를 올리면 제련소를 손대지
        /// 않아도 정제량이 저절로 같이 늘었다 — 그러면 제련소가 진짜 병목이 될 일이 없다. 지금은
        /// **P와 무관한 독립 값**이다(레벨과 행성 매장 배율에만 비례) — 곡괭이를 올려 P가 제련소보다
        /// 커지면 남는 원석이 화물칸에 쌓이기 시작한다(economy-v2.md "정제 수입 = min(P, R)").
        /// 0레벨(제련소 없음)은 0. E-05(2026-09-29)로 상한이 500까지 늘었다 — 5레벨 이후는
        /// RefineCapacityExtendedGrowth로 이어 붙이고, 25레벨마다 이정표 ×2가 곱해진다.</summary>
        public static float RefineCapacity(MiningRig rig, Planet planet) => RefineCapacity(rig, planet, false);

        /// <summary>레벨 1(제련소를 막 산 직후)의 처리량 — 쿼츠(매장 배율 ×1) 기준 시간당 90원석
        /// (기준 채굴차 산출 ~190/h보다 낮다 — 1레벨만으로는 아직 다 못 따라잡는다는 뜻).
        /// 레벨당 ×4.0배로 늘어 5레벨(보호 구간 끝)에서 시간당 90×4.0^4 = 23,040원석. tempo.md 5절이
        /// 잡아 둔 첫 두 지표(첫 정제 광물 구매 0.20h·제련소 5레벨 1.75h)를 다시 맞추려고
        /// `RigUpgrade.cs`의 제련소 비용 성장률(2.8→4.3)과 함께 BalanceSim으로 실측하며 고른 값이다
        /// (E-04 착수 전 템포 고정 테스트, Core.Tests/Program.cs) — 다른 상수처럼 유도식이 없다.
        /// 값을 바꾸려면 반드시 `dotnet run`으로 그 테스트가 여전히 통과하는지 다시 봐야 한다
        /// (두 상수가 서로 묶여 있어 하나만 옮기면 대개 깨진다).</summary>
        const float RefineCapacityBaseAt1 = 90f;
        const float RefineCapacityGrowth = 4.0f;
        const int RefineCapacityProtectedMaxLevel = 5; // UpgradeCost의 RefineryProtectedMaxLevel과 짝

        /// <summary>E-05: 5레벨 이후 이어 붙이는 성장률. 4.0/레벨을 500레벨까지 그대로 두면
        /// MathF.Pow가 float 상한(약 3.4e38, 4.0^65 근처)을 훌쩍 넘어 Infinity가 된다 — 그래서
        /// 완만한 성장으로 갈아 끼운다. UpgradeCost.ExtendedCostGrowth(1.065)보다 낮게 잡아
        /// (E-04의 목적이었던) "제련소가 채굴 산출보다 느리게 커져 계속 병목으로 남는다"는 성질을
        /// 5레벨 너머에서도 지킨다 — 25레벨마다 붙는 이정표 ×2가 그 대신 숨통을 틔워 준다.</summary>
        const float RefineCapacityExtendedGrowth = 1.03f;

        /// <summary>2026-09-19: Entitlements.AutoRefineryAlwaysOn(구독 중 자동 제련 상시 켜짐,
        /// monetization.md 2-5)을 나중에 배선할 자리를 미리 만들어 둔 오버로드 — forceFullRefine이
        /// true면 레벨과 무관하게 그 순간의 채굴 산출(P)과 같은 값을 돌려준다(=min(P,R)에서 R이
        /// 항상 이겨서 화물칸이 안 참). 기존 2인자 호출은 전부 false를 넘기는 것과 완전히 같아서
        /// (바로 위 오버로드), 이미 2인자로 부르던 곳은 동작이 하나도 안 바뀐다 — 실제로 구독
        /// 여부에 따라 true/false를 갈라 넘기는 배선은 MonoBehaviour 쪽(MiningController)이라
        /// 컴파일 확인이 되는 Unity 세션 몫으로 남긴다(docs/decisions.md "M-06/M-07이 아직 안
        /// 붙인 값" 참고).</summary>
        public static float RefineCapacity(MiningRig rig, Planet planet, bool forceFullRefine) =>
            RefineCapacity(rig, planet, forceFullRefine, null);

        /// <summary>P-05: 제련소 칸 증폭기 반영판. 최종 처리량에 증폭을 곱한다 — 제련소 증폭기가
        /// 세면 처리량이 채굴 속도를 넘어설 수 있고, Offline()의 rate&lt;=refineRate 분기가 이미
        /// 그 경우를 다룬다(화물칸이 절대 안 참). forceFullRefine이어도 증폭은 그대로 적용한다
        /// (구독 중이라고 증폭기 효과가 죽을 이유가 없다).</summary>
        public static float RefineCapacity(MiningRig rig, Planet planet, bool forceFullRefine, RigAmplifierSave amp)
        {
            float rate;
            if (forceFullRefine) rate = MineralsPerHour(rig, planet, amp);
            else
            {
                var lvl = Clamp(rig.RefineryLevel, 0, UpgradeCost.RefineryMaxLevel);
                if (lvl <= 0) rate = 0f;
                else
                {
                    var reserveMultiplier = planet.VeinYield / 20f;
                    // E-05: 보호 구간(1~4)은 옛 식 그대로, 5레벨부터는 더 완만한 성장으로 이어 붙인다
                    // (anchor = 옛 식을 보호 구간 끝에서 그대로 계산한 값이라 경계에서 안 끊긴다).
                    var baseRate = lvl < RefineCapacityProtectedMaxLevel
                        ? MathF.Pow(RefineCapacityGrowth, lvl - 1)
                        : MathF.Pow(RefineCapacityGrowth, RefineCapacityProtectedMaxLevel - 1) *
                          MathF.Pow(RefineCapacityExtendedGrowth, lvl - RefineCapacityProtectedMaxLevel);
                    // 보호 구간(1~4레벨) 밖에서만 이정표 — 제련소는 보호 구간이 5라 25 이정표보다
                    // 훨씬 안쪽이라 사실상 항상 적용된다(Breakthrough.MilestoneMultiplierBeyond 주석).
                    var milestone = Breakthrough.MilestoneMultiplierBeyond(lvl, UpgradeCost.RefineryProtectedMaxLevel);
                    rate = RefineCapacityBaseAt1 * reserveMultiplier * baseRate * (float)milestone;
                }
            }
            return amp == null ? rate : Amplifier.Apply(rate, amp.Refinery);
        }

        /// <summary>이번 프레임(deltaSeconds) 동안 원석→정제로 실제로 넘어가는 양. 가진 원석보다
        /// 많이 못 넘기고, 음수 델타나 원석 0은 0을 돌려준다. 접속 중(MiningController.Update)
        /// 매 프레임 이 값만큼 RawMinerals를 깎고 RefinedMinerals에 더하는 용도 — 오프라인
        /// 캐치업은 경과 시간이 프레임 단위로 쪼개기엔 너무 길 수 있어(수백 년 단위 테스트 있음)
        /// 이 함수 대신 Offline()의 닫힌 형태 계산을 따로 쓴다(초당 비율 자체는 같다).
        /// E-01(2026-09-28): rawMinerals를 double로 받는다 — 호출부가 SaveData.RawMinerals(double)에
        /// 이번 틱 채굴량을 더한 값을 그대로 넘기기 때문(MiningController.Update). 반환값(이번 틱
        /// 정제량, 델타)은 작은 값이라 float 그대로 둔다.</summary>
        public static float Refine(double rawMinerals, MiningRig rig, Planet planet, float deltaSeconds) =>
            Refine(rawMinerals, rig, planet, deltaSeconds, false);

        /// <summary>2026-09-19: AutoRefineryAlwaysOn 배선용 오버로드. forceFullRefine은 그대로
        /// RefineCapacity(rig, planet, forceFullRefine)로 넘어간다 — 위 주석 참고.</summary>
        public static float Refine(double rawMinerals, MiningRig rig, Planet planet, float deltaSeconds, bool forceFullRefine) =>
            Refine(rawMinerals, rig, planet, deltaSeconds, forceFullRefine, 1.0);

        /// <summary>E-06: 정제 촉매 연구 배선용 오버로드. throughputMultiplier(ResearchState의
        /// RefineCatalyst 배율, 레벨 0이면 1)를 처리량에 곱한다. 가진 원석보다 많이는 못 처리한다.</summary>
        public static float Refine(double rawMinerals, MiningRig rig, Planet planet, float deltaSeconds, bool forceFullRefine, double throughputMultiplier)
        {
            if (deltaSeconds <= 0f || rawMinerals <= 0f) return 0f;
            var perSecond = RefineCapacity(rig, planet, forceFullRefine) * (float)Math.Max(0.0, throughputMultiplier) / 3600f;
            return (float)Math.Min(rawMinerals, perSecond * deltaSeconds);
        }

        /// <summary>희귀 광맥(보석 원석) 시간당 기대 개수. 탐지기 0이면 0.</summary>
        public static float GemsPerHour(MiningRig rig, Planet planet) => GemsPerHour(rig, planet, null);

        /// <summary>P-05: 탐지기 칸 증폭기 반영판. DetectorLevel이 0이면 amp가 있어도 여전히 0이다
        /// (chance 자체가 0이라 증폭할 대상이 없다 — 장비 없이 증폭기만으로 기능이 켜지지는 않는다).</summary>
        public static float GemsPerHour(MiningRig rig, Planet planet, RigAmplifierSave amp)
        {
            if (rig.DetectorLevel <= 0) return 0f;
            var speed = RigSpeed(rig, planet, amp);
            var travelPerVein = planet.Circumference / Math.Max(1, planet.VeinCount);
            var veinsPerHour = 3600f / (travelPerVein / speed + SecondsPerVein(rig));
            var chance = 0.01f * rig.DetectorLevel; // 레벨당 1%
            var gems = veinsPerHour * chance;
            return amp == null ? gems : Amplifier.Apply(gems, amp.Detector);
        }

        /// <summary>기준 채굴차 — 곡괭이·엔진 둘 다 1레벨(MiningRig 기본값 그대로). 화물칸·제련소
        /// 용량을 "지금 이 순간의 산출(P)"이 아니라 **고정된 기준선**으로 잡을 때 쓴다
        /// (E-04, 아래 CargoCapacity 주석 참고).</summary>
        static readonly MiningRig ReferenceRig = new MiningRig();

        /// <summary>화물칸 상한(원석 개수, M-01). E-04(2026-09-28, economy-v2.md 3-2)로 단위가
        /// 시간(CargoHours)에서 원석 개수로 바뀌었다. 예전엔 "그 채굴차 기준 N시간치"라 곡괭이를
        /// 올려 P가 커지면 화물칸 상한도 같은 비율로 저절로 커졌다 — 그러면 화물칸이 절대 병목이
        /// 못 된다. 이제는 **1레벨 기준 채굴차(ReferenceRig)의 산출**을 기준선으로 고정하고, 거기에
        /// CargoLevel 배율만 곱한다 — 곡괭이를 올려도 화물칸 자체를 안 올리면 상한은 그대로다.
        /// 배율은 그대로 레벨당 ×1.12 지수식(RigUpgrade.cs Cost의 비용 성장률 1.18과 짝, 비율 1.054,
        /// idle-research.md 1절) — 1레벨 배율은 ×1이라 게임 시작 시점(곡괭이·엔진 1레벨) 값은
        /// 예전과 완전히 같다. 오프라인 누적 상한과 접속 중(온라인) 상한이 같은 값을 쓴다.</summary>
        public static float CargoCapacity(MiningRig rig, Planet planet) => CargoCapacity(rig, planet, null);

        /// <summary>P-05: 화물칸 칸 증폭기 반영판. 기준선(ReferenceRig)에는 그 채굴차의 실제 Tool·
        /// Engine 증폭이 섞이면 안 된다(기준선은 "1레벨 고정"이 의미다) — 그래서 amp 없이 계산한
        /// baseline에 Cargo 증폭만 곱한다.</summary>
        public static float CargoCapacity(MiningRig rig, Planet planet, RigAmplifierSave amp)
        {
            var lvl = Clamp(rig.CargoLevel, 1, UpgradeCost.CargoMaxLevel);
            // E-05(2026-09-29): 25레벨마다 이정표 ×2, 단 보호 구간(1~30레벨) 밖에서만
            // (Breakthrough.MilestoneMultiplierBeyond 주석 참고). 지수식 자체(1.12^499)는 float
            // 범위 안이라 Refinery처럼 별도 구간을 안 나눴다.
            var multiplier = MathF.Pow(1.12f, lvl - 1) *
                (float)Breakthrough.MilestoneMultiplierBeyond(lvl, UpgradeCost.CargoProtectedMaxLevel);
            var baseline = MineralsPerHour(ReferenceRig, planet) * planet.BaseCargoHours;
            var capacity = baseline * multiplier;
            return amp == null ? capacity : Amplifier.Apply(capacity, amp.Cargo);
        }

        /// <summary>접속 중(온라인) 화물칸 상한 적용. 새로 캔 원석을 더한 뒤 상한을 넘으면 자른다
        /// (docs/design/monetization.md "2026-09-14 변경: 접속 중에도 화물칸이 차면 채굴이 멈춘다",
        /// decisions.md T-06 A안으로 해결). Math.Min이라 이미 상한을 넘어 저장돼 있던 값(이 기능이
        /// 생기기 전 세이브 등)도 한 번은 상한까지 깎인다 — 그 뒤로는 다시 늘지 않을 뿐 매 틱 계속
        /// 깎지는 않는다.</summary>
        public static float ClampToCargoCapacity(float rawMinerals, MiningRig rig, Planet planet) =>
            ClampToCargoCapacity(rawMinerals, rig, planet, null);

        public static float ClampToCargoCapacity(float rawMinerals, MiningRig rig, Planet planet, RigAmplifierSave amp)
            => Math.Min(rawMinerals, CargoCapacity(rig, planet, amp));

        /// <summary>오프라인 보상. 원석은 화물칸 상한(M-01)에서 막히지만, 제련소가 있으면 그동안에도
        /// 원석 일부가 계속 정제로 빠져나간다 — 그래서 상한에 닿는 시점이 늦춰지거나(레벨 5면 아예
        /// 안 막힌다) 한다. 이게 M-02 "정제 광물은 화물칸을 차지하지 않는다"가 실제로 상한을
        /// 올리는 방식이다.
        ///
        /// 원석 유입 속도 R(MineralsPerHour), 정제 속도 F(RefineCapacity), 화물칸 상한 Cap이 이
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
        /// <summary>E-02(economy-v2.md 1절): 오프라인 경과 시간을 Entitlements.OfflineCapHours로 자른다.
        /// 원석·정제 광물·젬·탐험(ExplorationSimulator.DiscoverOffline)이 전부 이 경과 시간 하나를
        /// 같이 쓰기 때문에, 여기서 한 번만 자르면 넷이 한꺼번에 상한을 받는다 — Offline() 자체를
        /// 고칠 필요가 없다. 호출부(MiningController.ComputeOfflineReward)가 Offline/DiscoverOffline에
        /// 넘기기 전에 이 함수를 거친다. 음수 경과·음수 상한 둘 다 0으로 본다.</summary>
        public static double ClampOfflineElapsedSeconds(double elapsedSeconds, float offlineCapHours)
        {
            var capSeconds = Math.Max(0f, offlineCapHours) * 3600.0;
            return Math.Min(Math.Max(0, elapsedSeconds), capSeconds);
        }

        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds) =>
            Offline(rig, planet, elapsedSeconds, false, null);

        /// <summary>2026-09-19: AutoRefineryAlwaysOn 배선용 오버로드 — RefineCapacity/Refine과 같은
        /// 패턴이다. forceFullRefine이 true면 제련소 레벨과 무관하게 원석 유입 속도(rate)와 정제
        /// 속도(refineRate)가 같아져서 rate&lt;=refineRate 분기(원석 0, 상한 절대 안 닿음)로 항상
        /// 빠진다 — 구독 중에는 오프라인에서도 접속 중과 똑같이 화물칸이 안 찬다는 뜻이다. 기존
        /// 3인자 호출은 그대로 false를 넘기는 것과 완전히 같다(회귀 없음). 실제로 구독 여부를 여기
        /// 넘기는 배선은 MiningController.ClaimOfflineReward 쪽(Unity 세션 몫)에 남겨 둔다.</summary>
        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds, bool forceFullRefine) =>
            Offline(rig, planet, elapsedSeconds, forceFullRefine, null);

        /// <summary>P-05: amp-aware판 — rate/refineRate/cap이 이미 각자 amp-aware 오버로드를 통해
        /// 해당 칸의 증폭을 반영한다(Tool·Engine→rate, Refinery→refineRate, Cargo(+Tool·Engine)→cap,
        /// Detector→Gems). 분기 로직 자체는 안 바뀐다 — 값만 커진 채로 같은 계산을 탄다.</summary>
        public static OfflineResult Offline(MiningRig rig, Planet planet, double elapsedSeconds, bool forceFullRefine, RigAmplifierSave amp)
        {
            var hours = (float)Math.Max(0, elapsedSeconds) / 3600f;
            var rate = MineralsPerHour(rig, planet, amp);
            var refineRate = RefineCapacity(rig, planet, forceFullRefine, amp);
            var cap = CargoCapacity(rig, planet, amp);

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
                Gems = GemsPerHour(rig, planet, amp) * counted
            };
        }

        public struct OfflineResult
        {
            public float HoursCounted, HoursWasted, Minerals, RefinedGained, Gems;
        }

        /// <summary>화물칸이 thresholdFraction(0~1) 비율에 닿기까지 남은 시간(시간 단위). M-05
        /// "화물칸 80% 푸시 알림"의 예약 시각 계산 몫 — Offline()과 같은 모델(원석 유입 R=
        /// MineralsPerHour, 정제 F=RefineCapacity, 순증가 R-F)을 그대로 쓴다. 이미 그 비율을
        /// 넘었으면(현재 원석 >= 목표치) 0을 돌려준다. 정제소가 유입을 따라잡아(R&lt;=F, 예:
        /// 제련소 5레벨) 원석이 그 이상 절대 안 쌓이면 도달 자체가 없다는 뜻으로 null을 돌려준다 —
        /// 이 경우 호출 쪽(Unity, 에디터가 있는 세션 몫)은 알림을 예약하지 않아야 한다. 실제
        /// 로컬 알림 API 호출(Unity Mobile Notifications 패키지)은 여기 core에 없다 — 여기는
        /// "언제"만 순수 계산으로 낸다(서버·클라이언트가 같은 값을 내야 하는 값이라).</summary>
        /// <summary>E-01(2026-09-28): currentRawMinerals를 double로 받는다 — SaveData.RawMinerals가
        /// double이 됐으니 호출부(MiningController.RawMinerals)가 그대로 넘길 수 있어야 한다. 반환값
        /// 자체는 알림 예약용 시간(초 단위 아님, 시간 단위) 힌트라 정밀도가 중요하지 않아 float 그대로 둔다.</summary>
        public static float? HoursUntilCargoThreshold(MiningRig rig, Planet planet, double currentRawMinerals, float thresholdFraction)
        {
            var target = CargoCapacity(rig, planet) * Clamp01(thresholdFraction);
            if (currentRawMinerals >= target) return 0f;

            var netGrowth = MineralsPerHour(rig, planet) - RefineCapacity(rig, planet);
            if (netGrowth <= 0f) return null;

            return (float)((target - currentRawMinerals) / netGrowth);
        }

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
        static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
