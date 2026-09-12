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

        void Awake()
        {
            _planet = ResolvePlanet(planetId);
            _run = new MiningRunState(rig, _planet);
            if (surfaceMover == null) surfaceMover = GetComponent<SurfaceMover>();
        }

        void Update()
        {
            RawMinerals += _run.Advance(rig, _planet, Time.deltaTime);
            if (surfaceMover != null) surfaceMover.isMoving = _run.Phase == MiningPhase.Traveling;
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
