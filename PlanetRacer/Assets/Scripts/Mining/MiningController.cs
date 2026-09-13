using UnityEngine;
using GemRacer.Core;
using GemRacer.Planet;
// Assets/Scripts/Planet/가 네임스페이스를 GemRacer.Planet으로 쓰고 있어서, 여기서 그냥 Planet이라고
// 쓰면 컴파일러가 코어의 Planet 타입 대신 그 네임스페이스로 해석해 버린다(CS0118 — BalanceTable.cs에서
// 이미 한 번 겪은 문제, docs/backlog.md W-07 참고). 그래서 코어 타입만 별칭을 준다.
using CorePlanet = GemRacer.Core.Planet;

namespace GemRacer.Mining
{
    /// <summary>D04-N: 실시간 채굴 루프. 코어 MiningRunState를 매 프레임 흘려보내고,
    /// 결과(누적 원석·지금 단계)를 HUD가 읽을 수 있게 공개한다. SurfaceMover가 있으면
    /// 채굴 단계(MiningVein) 동안 이동을 멈춰서 "광맥 앞에 서서 캔다"는 느낌을 만든다.
    ///
    /// 실제 게이지·업그레이드 UI는 D05-N 이후에 붙는다 — 지금은 임시 OnGUI로 눈으로만 확인한다.
    /// GameFlowController가 State != Mining이면 이 컴포넌트를 꺼서(enabled = false) 루프를 멈춘다.</summary>
    [DisallowMultipleComponent]
    public class MiningController : MonoBehaviour
    {
        [Tooltip("코어 DefaultData.Planets()의 행성 id. GemColors.PlanetIdsInOrder와 같은 값 중 하나.")]
        public string planetId = "quartz";

        [Tooltip("채굴차 장비 레벨. Core.MiningRig 그대로 — 인스펙터에서 바로 조정해 확인할 수 있다.")]
        public MiningRig rig = new MiningRig();

        [Tooltip("채굴 단계 동안 이 SurfaceMover를 멈춘다. 비워두면 같은 오브젝트에서 찾는다.")]
        public SurfaceMover surfaceMover;

        [Tooltip("임시 확인용 화면 표시(OnGUI). D05-N에서 실제 HUD가 붙으면 꺼도 된다.")]
        public bool showDebugGui = true;

        CorePlanet _planet;
        MiningRunState _run;

        /// <summary>이번 세션에서 누적된 원석(정제 전) 총량. 세이브 반영은 SaveService 쪽 몫.</summary>
        public float RawMinerals { get; private set; }
        public MiningPhase Phase => _run.Phase;
        public float PhaseSecondsRemaining => _run.PhaseSecondsRemaining;
        public CorePlanet CurrentPlanet => _planet;

        void Awake()
        {
            _planet = ResolvePlanet(planetId);
            _run = new MiningRunState(rig, _planet);
            if (surfaceMover == null) surfaceMover = GetComponent<SurfaceMover>();
        }

        void Update()
        {
            RawMinerals += _run.Advance(rig, _planet, Time.deltaTime);
            if (surfaceMover != null)
            {
                surfaceMover.isMoving = _run.Phase == MiningPhase.Traveling;
                // 발견한 버그(오후 3시 세션): 지금까지 이동 속도가 SurfaceMover.speed 고정값이라
                // 엔진을 업그레이드해도(코어 RigSpeed는 올라가는데) 화면상 채굴차는 그대로 느리게 돌았다.
                // 업그레이드 패널의 "다음: 속도 X m/s" 문구가 실제로 눈에 보이게 매 프레임 맞춰 준다.
                surfaceMover.speed = MiningSimulator.RigSpeed(rig, _planet);
            }
        }

        /// <summary>화물칸 상한(원석 기준). HUD 게이지가 이 값 대비 RawMinerals를 채워서 보여준다.
        /// 주의: 지금은 표시용일 뿐 실시간 채굴 자체를 이 값에서 멈추지 않는다 — 오프라인 캐치업
        /// (MiningSimulator.Offline)에만 상한이 걸려 있다. 접속 중에도 막을지는 아직 정하지 않았다
        /// (docs/decisions.md 참고).</summary>
        public float CargoCapacityMinerals => MiningSimulator.MineralsPerHour(rig, _planet) * MiningSimulator.CargoHours(rig);

        /// <summary>D05-N: 업그레이드 화면이 이 함수 하나로 원석을 낸다. 아직 제련(RefineryLevel)
        /// 로직이 없어서 정제 광물 대신 원석(RawMinerals)을 그대로 쓴다 — 제련이 생기면 그때
        /// RefinedMinerals로 바꿀 지점(TODO). 실패해도(원석 부족) 예외 없이 false만 돌려준다.</summary>
        public bool TrySpendRawMinerals(float amount)
        {
            if (amount > RawMinerals) return false;
            RawMinerals -= amount;
            return true;
        }

        /// <summary>비용을 내고 해당 슬롯 레벨을 올린다. 이미 최대 레벨이거나 원석이 모자라면 아무 일도
        /// 안 하고 false를 돌려준다 — UI는 이 하나만 부르면 된다(UpgradePanel).</summary>
        public bool TryUpgrade(UpgradeSlot slot)
        {
            var cost = UpgradeCost.Cost(slot, rig);
            if (float.IsPositiveInfinity(cost) || !TrySpendRawMinerals(cost)) return false;
            rig = UpgradeCost.Apply(slot, rig);
            return true;
        }

        static CorePlanet ResolvePlanet(string id)
        {
            foreach (var p in DefaultData.Planets()) if (p.Id == id) return p;
            Debug.LogWarning($"[GemRacer] 행성 id '{id}'를 DefaultData에서 못 찾음. 쿼츠로 대신 씀.");
            return DefaultData.Planets()[0];
        }

        void OnGUI()
        {
            if (!showDebugGui) return;
            var phaseLabel = Phase == MiningPhase.MiningVein ? "채굴 중" : "이동 중";
            GUI.Label(new Rect(10, 10, 320, 60),
                $"원석 {RawMinerals:F1}\n{phaseLabel} (남은 시간 {PhaseSecondsRemaining:F1}s)");
        }
    }
}
