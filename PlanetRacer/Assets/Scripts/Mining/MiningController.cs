using System;
using UnityEngine;
using GemRacer.Core;
using GemRacer.Planet;
using GemRacer.Save;
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

        /// <summary>자리를 비운 뒤 돌아왔을 때 화면(D07-N)이 참고하는 요약값. 순수 데이터 struct라
        /// Core에 둘 수도 있지만, "지금 UTC 시각"을 구하는 부분은 CLAUDE.md 1번 규칙상 Core에
        /// 둘 수 없어서(코어는 DateTime.Now 금지) 이 글루 레이어에 남긴다.</summary>
        public struct OfflineRewardSummary
        {
            public float ElapsedHours, CountedHours, WastedHours, Minerals, TreasureValue;
            public int TreasuresFound, TreasuresMineable;
        }

        /// <summary>너무 짧은 재시작(에디터에서 Play를 다시 누르는 정도)엔 보상 화면을 띄우지 않는다.</summary>
        const long MinOfflineSecondsForReward = 30;
        const float AutosaveIntervalSeconds = 30f;

        CorePlanet _planet;
        MiningRunState _run;
        SaveData _save;
        OfflineRewardSummary? _pendingOfflineReward;
        float _autosaveTimer;

        /// <summary>이번 세션 + 이전 세이브에서 이어진 원석(정제 전) 총량.</summary>
        public float RawMinerals { get; private set; }
        public MiningPhase Phase => _run.Phase;
        public float PhaseSecondsRemaining => _run.PhaseSecondsRemaining;
        public CorePlanet CurrentPlanet => _planet;

        /// <summary>D07-N: 계산은 끝났지만 아직 "받기"를 안 누른 오프라인 보상. 없으면 null —
        /// 첫 실행이거나, 마지막 저장 뒤 MinOfflineSecondsForReward보다 짧게 지났을 때.</summary>
        public OfflineRewardSummary? PendingOfflineReward => _pendingOfflineReward;

        void Awake()
        {
            _save = SaveService.Load();
            if (!string.IsNullOrEmpty(_save.CurrentPlanetId)) planetId = _save.CurrentPlanetId;
            _planet = ResolvePlanet(planetId);
            rig = _save.Rig.ToCore();
            RawMinerals = _save.RawMinerals;
            _run = new MiningRunState(rig, _planet);
            if (surfaceMover == null) surfaceMover = GetComponent<SurfaceMover>();

            ComputeOfflineReward(_save.LastSeenUnixSeconds);
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

            _autosaveTimer += Time.deltaTime;
            if (_autosaveTimer >= AutosaveIntervalSeconds)
            {
                _autosaveTimer = 0f;
                Save();
            }
        }

        // 모바일에서 홈 버튼을 누르는 순간(백그라운드 전환)이 "정상 종료"에 가장 가깝다 —
        // OnApplicationQuit은 모바일에서 호출이 보장되지 않는다. WebGL도 탭을 닫을 때 호출이 안
        // 보장되긴 마찬가지라 AutosaveIntervalSeconds 주기 저장이 최후 방어선이다.
        void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        void OnApplicationQuit() => Save();

        /// <summary>지금 상태(행성·채굴차·원석)를 세이브에 반영하고 디스크에 쓴다. 이 컨트롤러가
        /// 안 다루는 필드(정제 광물·보유/장착 부품)는 로드된 값을 그대로 둔다 — D08-N 등 다른
        /// 시스템이 그 필드를 쓰기 시작해도 여기서 덮어써 날리지 않는다.</summary>
        public void Save()
        {
            _save.CurrentPlanetId = planetId;
            _save.LastSeenUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _save.Rig = MiningRigSave.FromCore(rig);
            _save.RawMinerals = RawMinerals;
            SaveService.Save(_save);
        }

        /// <summary>D07-N: 지난 세이브 시각과 지금 UTC 시각의 차를 오프라인 경과로 보고 광물·보물을
        /// 계산해 둔다. 실제 지급은 ClaimOfflineReward가 "받기"를 눌렀을 때만 한다 — 여기서는
        /// 화면에 보여줄 값만 준비한다(값을 두 번 계산하지 않도록 같은 seed로 재계산 가능).</summary>
        void ComputeOfflineReward(long lastSeenUnixSeconds)
        {
            if (lastSeenUnixSeconds <= 0) return; // 세이브가 없던 첫 실행 — 오프라인 보상 대상 아님

            var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var elapsedSeconds = nowUnixSeconds - lastSeenUnixSeconds;
            if (elapsedSeconds < MinOfflineSecondsForReward) return;

            // TODO: 행성별 보물 정의가 생기면(지금은 쿼츠뿐) planetId로 분기할 자리.
            var defs = DefaultData.QuartzTreasureDefs();
            // 저장 시각을 그대로 seed로 쓴다 — 같은 마지막 저장 시각이면 서버가 같은 발견 목록을
            // 재현할 수 있다(ExplorationSimulator 주석과 같은 이유).
            var seed = unchecked((int)lastSeenUnixSeconds);
            var discoveries = ExplorationSimulator.DiscoverOffline(rig, _planet, elapsedSeconds, defs, seed);

            var mineableNow = 0;
            foreach (var t in discoveries.Treasures) if (t.CanMineNow) mineableNow++;

            _pendingOfflineReward = new OfflineRewardSummary
            {
                ElapsedHours = (float)(elapsedSeconds / 3600.0),
                CountedHours = discoveries.Mining.HoursCounted,
                WastedHours = discoveries.Mining.HoursWasted,
                Minerals = discoveries.Mining.Minerals,
                TreasureValue = ExplorationSimulator.MineableValue(discoveries.Treasures),
                TreasuresFound = discoveries.Treasures.Count,
                TreasuresMineable = mineableNow,
            };
        }

        /// <summary>화면의 "받기" 버튼 하나가 이 함수를 부른다. 보상을 원석에 더하고(제련 로직이
        /// 아직 없어서 TreasureValue도 TrySpendRawMinerals와 같은 이유로 일단 원석에 합친다 —
        /// 제련이 생기면 RefinedMinerals로 옮길 지점) 즉시 저장한다. 보상이 없으면 false.
        /// 알려진 한계: 보상 값 자체는 세이브 파일에 안 남고 이번 세션 메모리에만 있다 — "받기"를
        /// 누르기 전에 자동 저장(AutosaveIntervalSeconds)이나 일시정지 저장이 먼저 일어나 버리면
        /// LastSeenUnixSeconds가 앞당겨지긴 해도 이미 계산해 둔 값은 그대로 살아 있어 괜찮지만,
        /// 화면을 아예 안 보고 앱을 껐다 켜면(그 사이 저장이 한 번이라도 있었다면) 다음 실행 때는
        /// 짧아진 경과 시간만 남아 그 보상이 사라진다. 받지 않은 보상을 세이브에 그대로 들고
        /// 다니게 하려면 SaveData에 pending 필드를 추가해야 하는데, 지금은 첫 구현이라 범위를
        /// 좁혀 뒀다 — 실제로 문제가 되면(플레이테스트에서 보상이 자꾸 사라진다는 피드백 등) 그때 늘릴 것.</summary>
        public bool ClaimOfflineReward()
        {
            if (_pendingOfflineReward == null) return false;
            var reward = _pendingOfflineReward.Value;
            RawMinerals += reward.Minerals + reward.TreasureValue;
            _pendingOfflineReward = null;
            Save();
            return true;
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
