using System;
using System.Collections.Generic;
using UnityEngine;
using GemRacer.Audio;
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
    // 화면 스크립트(MainHud, UpgradePanel, SettingsPanel 등 여덟 개)는 전부 OnEnable에서
    // MiningController의 프로퍼티를 읽는다. 그런데 이 클래스는 Awake에서 _save와 _planet을
    // 채운다. 유니티는 씬 로드 때 게임오브젝트 사이의 Awake/OnEnable 순서를 보장하지 않아서,
    // 패널이 먼저 깨면 _save가 아직 null인 채로 읽혀 NullReferenceException이 난다
    // (2026-09-14 에디터에서 Play로 직접 확인 — MiningSimulator.RigSpeed와 SoundEnabled 두 군데).
    // 실행 순서를 앞으로 당겨서 Awake가 항상 먼저 끝나게 한다.
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class MiningController : MonoBehaviour
    {
        [Tooltip("코어 DefaultData.Planets()의 행성 id. GemColors.PlanetIdsInOrder와 같은 값 중 하나.")]
        public string planetId = "quartz";

        [Tooltip("채굴차 장비 레벨. Core.MiningRig 그대로 — 인스펙터에서 바로 조정해 확인할 수 있다.")]
        public MiningRig rig = new MiningRig();

        [Tooltip("채굴 단계 동안 이 SurfaceMover를 멈춘다. 비워두면 같은 오브젝트에서 찾는다.")]
        public SurfaceMover surfaceMover;

        [Tooltip("D06-N: 표면에 광맥을 놓는 VeinField. 비워두면 씬에서 찾는다. 없어도 채굴은 그대로 돈다.")]
        public VeinField veinField;

        [Tooltip("임시 확인용 화면 표시(OnGUI). D05-N에서 실제 HUD가 붙으면 꺼도 된다.")]
        public bool showDebugGui = true;

        [Tooltip("M-11: 판별을 빌드 정의(GEMRACER_STEAM) 대신 손으로 지정한다. 에디터에서 Steam 판 " +
                 "화면을 확인할 때만 켠다 — 빌드로 나갈 씬에는 켠 채로 두지 말 것.")]
        public bool overrideStorePlatform = false;

        [Tooltip("위를 켰을 때 쓸 판. 꺼져 있으면 빌드 정의가 정한 값(GamePlatform.Build)을 쓴다.")]
        public StorePlatform storePlatformOverride = StorePlatform.Mobile;

        [Tooltip("D14-N: 엔진·채굴 루프음 + 탭 효과음을 낼 대상. 비워두면 사운드 전부 무음(클립이 없어도 어차피 무음).")]
        public AudioHub audioHub;

        /// <summary>자리를 비운 뒤 돌아왔을 때 화면(D07-N)이 참고하는 요약값. 순수 데이터 struct라
        /// Core에 둘 수도 있지만, "지금 UTC 시각"을 구하는 부분은 CLAUDE.md 1번 규칙상 Core에
        /// 둘 수 없어서(코어는 DateTime.Now 금지) 이 글루 레이어에 남긴다.</summary>
        public struct OfflineRewardSummary
        {
            public float ElapsedHours, CountedHours, WastedHours, Minerals, RefinedGained, TreasureValue;
            public int TreasuresFound, TreasuresMineable;
        }

        /// <summary>너무 짧은 재시작(에디터에서 Play를 다시 누르는 정도)엔 보상 화면을 띄우지 않는다.</summary>
        const long MinOfflineSecondsForReward = 30;
        const float AutosaveIntervalSeconds = 30f;

        /// <summary>SaveData.EquippedPartIds가 슬롯 순서를 문자열 6칸으로 저장하는 순서.
        /// SaveData.cs 주석("PartSlot enum 순서와 동일")과 반드시 맞춰야 한다.</summary>
        static readonly PartSlot[] SlotOrder =
            { PartSlot.Engine, PartSlot.Tire, PartSlot.Suspension, PartSlot.Body, PartSlot.Booster, PartSlot.Module };

        CorePlanet _planet;
        MiningRunState _run;
        SaveData _save;
        OfflineRewardSummary? _pendingOfflineReward;
        float _autosaveTimer;
        long _fuelBaselineUnixSeconds;

        /// <summary>M-07: 지금까지 상점에서 산 것의 원 데이터. Entitlements.Effective로만 읽는다 —
        /// 화면(ShopPanel)이 이 값을 직접 들여다보고 판단하면 M-06 Entitlements.cs 주석이 경고하는
        /// "중복 차감" 실수가 생기기 쉽다. Purchases 프로퍼티는 화면이 "보유 중" 같은 상태 문구를
        /// 그릴 때만 읽는다(실제 배율 계산은 전부 Entitlements를 거친다).</summary>
        PurchaseState _purchases;

        /// <summary>M-09: 보상형 광고 네 자리의 오늘 시청 횟수. RewardAdTracker.CanWatch/RemainingToday/
        /// RecordWatch로만 다룬다 — 화면이 이 값을 직접 들여다보지 않는다(_purchases와 같은 이유).</summary>
        RewardAdState _rewardAds;

        /// <summary>M-09: "하루"의 경계로 쓸 시간대 오프셋. 이 게임은 한국 유저 기준이라(monetization.md
        /// "가격은 한국 기준") KST(UTC+9)로 고정한다 — TimeZoneInfo로 기기 시간대를 읽는 방법도
        /// 있지만 WebGL 빌드에서 플랫폼별 IANA 시간대 DB 가용성이 갈려 위험하다(과거 URP 셰이더
        /// 사고처럼 플랫폼마다 다르게 깨질 수 있는 지점은 피한다). RewardAdTracker 자체는 이
        /// 오프셋 값에 얽매이지 않는다(RewardAd.cs 주석 참고) — 나중에 리전을 넓히면 여기만 바꾸면 됨.</summary>
        const long KstOffsetSeconds = 9 * 3600;

        /// <summary>제작해서 보유 중인 부품 id 목록(장착 여부와 무관). D08-N.</summary>
        public List<string> OwnedPartIds { get; private set; } = new List<string>();

        /// <summary>D12-N: 부품 id → 강화 단계(+0~+10). DefaultData.QuartzStarterParts()가 매번
        /// 새 Part 인스턴스를 만들어서(Enhance는 인스턴스 필드) 강화 수치를 인스턴스에만 두면
        /// 다음 조회 때 사라진다 — 그래서 진짜 값은 여기(그리고 세이브)에 두고, AvailableParts가
        /// 새 인스턴스를 만들 때마다 다시 입혀 준다.</summary>
        readonly Dictionary<string, int> _partEnhanceLevels = new Dictionary<string, int>();

        /// <summary>지금 조립된 레이싱카. Slots는 항상 6칸(PartSlot enum 전부), 빈 슬롯은 null.</summary>
        public RacingCar Car { get; private set; } = new RacingCar();

        /// <summary>D08-N: 지금 제작 화면에서 고를 수 있는 부품 정의 목록. 아직 쿼츠 하나뿐이라
        /// planetId로 분기하지 않는다 — 다른 행성 부품이 생기면 여기서 분기할 자리(TODO,
        /// ComputeOfflineReward의 QuartzTreasureDefs와 같은 이유). D12-N: 매번 새로 만든 Part
        /// 인스턴스에 저장된 강화 단계를 입혀서 돌려준다 — 호출할 때마다 새 인스턴스라 이 보정이
        /// 없으면 화면에 강화 전 상태만 보인다.</summary>
        public List<Part> AvailableParts
        {
            get
            {
                var parts = DefaultData.QuartzStarterParts();
                foreach (var p in parts)
                    if (_partEnhanceLevels.TryGetValue(p.Id, out var level)) p.Enhance = level;
                return parts;
            }
        }

        /// <summary>이번 세션 + 이전 세이브에서 이어진 원석(정제 전) 총량. 화물칸 상한(M-01)에
        /// 걸리는 건 이 값뿐이다.</summary>
        public float RawMinerals { get; private set; }

        /// <summary>M-02: 정제 광물 총량 — 업그레이드·제작·강화가 실제로 쓰는 화폐. 화물칸 상한과
        /// 무관하게 쌓인다(docs/design/monetization.md "정제 광물은 화물칸을 차지하지 않는다").</summary>
        public float RefinedMinerals { get; private set; }

        /// <summary>M-04: 화물칸이 방금(이전 프레임엔 안 찼다가 이번 프레임에) 상한에 닿았다는
        /// 신호. CargoFullPanel이 이 값을 보고 "정제로 돌리시겠어요?" 화면을 한 번 띄운 뒤
        /// AcknowledgeCargoFull로 끈다 — 엣지 트리거라 원석이 상한 아래로 내려갔다가(정제나
        /// 소비로) 다시 차면 또 한 번 뜬다. 매 프레임 계속 띄우지 않으려고 이렇게 만들었다.</summary>
        public bool CargoJustFilled { get; private set; }
        bool _wasCargoFull;

        public MiningPhase Phase => _run.Phase;
        public float PhaseSecondsRemaining => _run.PhaseSecondsRemaining;
        public CorePlanet CurrentPlanet => _planet;

        /// <summary>D07-N: 계산은 끝났지만 아직 "받기"를 안 누른 오프라인 보상. 없으면 null —
        /// 첫 실행이거나, 마지막 저장 뒤 MinOfflineSecondsForReward보다 짧게 지났을 때.</summary>
        public OfflineRewardSummary? PendingOfflineReward => _pendingOfflineReward;

        /// <summary>D09-N: 지금 남은 레이스 출전 연료. RaceFuel.MaxFuel까지.</summary>
        public int Fuel { get; private set; }

        /// <summary>다음 연료 1개가 회복되기까지 남은 시간(초). 이미 꽉 찼으면 0.</summary>
        public float SecondsUntilNextFuel
        {
            get
            {
                if (Fuel >= RaceFuel.MaxFuel) return 0f;
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var elapsed = now - _fuelBaselineUnixSeconds;
                var remaining = RaceFuel.RecoverySeconds - (elapsed % RaceFuel.RecoverySeconds);
                return remaining <= 0 ? 0f : remaining;
            }
        }

        void Awake()
        {
            _save = SaveService.Load();
            if (!string.IsNullOrEmpty(_save.CurrentPlanetId)) planetId = _save.CurrentPlanetId;
            _planet = ResolvePlanet(planetId);
            rig = _save.Rig.ToCore();
            RawMinerals = _save.RawMinerals;
            RefinedMinerals = _save.RefinedMinerals;
            _run = new MiningRunState(rig, _planet);
            if (surfaceMover == null) surfaceMover = GetComponent<SurfaceMover>();

            // D06-N: 광맥은 행성마다 개수가 다르니(VeinCount) 씬이 아니라 여기서 만든다 —
            // 나중에 행성을 갈아타면 Build를 다시 부르면 된다.
#if UNITY_2022_2_OR_NEWER
            if (veinField == null) veinField = FindFirstObjectByType<VeinField>();
#else
            if (veinField == null) veinField = FindObjectOfType<VeinField>();
#endif
            if (veinField != null) veinField.Build(_planet);

            LoadParts(_save);
            _purchases = _save.ToPurchaseState();
            _rewardAds = _save.ToRewardAdState();
            ComputeOfflineReward(_save.LastSeenUnixSeconds);

            Fuel = _save.Fuel;
            _fuelBaselineUnixSeconds = _save.FuelBaselineUnixSeconds;
            RecoverFuel(); // baseline이 0(첫 세이브)이거나 오래 지났으면 여기서 바로 맞춰 둔다

            // D14-N: 저장된 설정을 실제로 적용한다. TargetFrameRate는 옛 세이브에 이상한 값이
            // 남아 있을 수도 있어(30/60 도입 전 기본값 0 등) 정규화해서 다시 써 둔다.
            _save.TargetFrameRate = GameSettings.NormalizeFrameRate(_save.TargetFrameRate);
            Application.targetFrameRate = _save.TargetFrameRate;
            AudioListener.volume = _save.SoundEnabled ? 1f : 0f;
        }

        void Update()
        {
            // M-01: 접속 중에도 화물칸 상한에서 멈춘다. 채굴차는 그대로 이동·채굴 애니메이션을 계속
            // 돌지만(연출은 손 안 댐) 원석은 상한 이상 안 쌓인다. 상한 도달 화면은 M-04, 아래
            // CargoJustFilled 참고.
            // M-02: 상한을 적용하기 전에 제련소가 원석 일부를 정제로 빼간다 — 이게 상한을 실제로
            // 늦추거나(레벨 5는 아예 없앤다) 만드는 지점이다. 정제 광물은 화물칸을 안 타니 그대로 더한다.
            // M-07: 채굴 가속 패스(Entitlements.MiningYieldMultiplier)는 접속 중 산출에만 곱한다 —
            // 정제 속도(RefinePerHour)는 그대로 둬서, 가속 패스를 산 사람이 오히려 원석을 더 빨리
            // 상한까지 채워 버리는 것도 의도한 그대로다(캘 수 있는 등급은 안 바뀐다는 monetization.md
            // 2-4 원칙과 같은 결로, 산출만 늘 뿐 정제 능력이 같이 느는 게 아니다). 오프라인 계산
            // (ComputeOfflineReward → MiningSimulator.Offline)은 아직 이 배율을 모른다 — 그쪽은
            // core 함수 시그니처를 같이 바꿔야 해서 에디터로 컴파일을 확인할 수 있는 세션 몫으로 남긴다.
            var minedThisTick = _run.Advance(rig, _planet, Time.deltaTime) * Entitlements.MiningYieldMultiplier;
            var rawAfterMining = RawMinerals + minedThisTick;
            var refinedNow = MiningSimulator.Refine(rawAfterMining, rig, _planet, Time.deltaTime);
            // M-07: 상한 자체(CargoCapacityMinerals 프로퍼티)가 이미 Entitlements.CargoMultiplier를
            // 곱한 값이라, 코어 ClampToCargoCapacity(배율을 모른다) 대신 그 값으로 직접 자른다.
            RawMinerals = Mathf.Min(rawAfterMining - refinedNow, CargoCapacityMinerals);
            RefinedMinerals += refinedNow;

            // M-04: 상한에 막 닿은 프레임만 잡아서 CargoJustFilled를 켠다(엣지 트리거).
            var isCargoFull = RawMinerals >= CargoCapacityMinerals - 0.001f;
            if (isCargoFull && !_wasCargoFull)
            {
                CargoJustFilled = true;
                // M-08: "첫 상한 도달"은 세이브에 영구히 남긴다 — CargoJustFilled와 달리 이후
                // AcknowledgeCargoFull로 안 꺼진다. StarterPackOffer.ShouldShow가 이 값을 본다.
                _save.HasReachedCargoCapBefore = true;
            }
            _wasCargoFull = isCargoFull;

            var isMoving = _run.Phase == MiningPhase.Traveling;
            if (surfaceMover != null)
            {
                surfaceMover.isMoving = isMoving;
                // 발견한 버그(오후 3시 세션): 지금까지 이동 속도가 SurfaceMover.speed 고정값이라
                // 엔진을 업그레이드해도(코어 RigSpeed는 올라가는데) 화면상 채굴차는 그대로 느리게 돌았다.
                // 업그레이드 패널의 "다음: 속도 X m/s" 문구가 실제로 눈에 보이게 매 프레임 맞춰 준다.
                surfaceMover.speed = MiningSimulator.RigSpeed(rig, _planet);

                // D06-N: 이제 광맥이 표면에 실제 좌표를 갖는다. 채굴차 위치를 화면에서 따로 적분하지
                // 않고 코어의 진행도(VeinProgress)를 각도로 바꿔 그대로 찍는다 — 그래야 몇 시간을
                // 돌려도 "코어는 도착했다는데 화면은 아직 가는 중"이 안 생긴다. 옛 자유 주행 모드는
                // SurfaceMover에 그대로 남아 있다(레이스 화면·실험 씬이 쓴다).
                surfaceMover.externallyDriven = true;
                surfaceMover.SetOrbitAngle(VeinLayout.ProgressToAngleDegrees(_planet, _run.VeinProgress));

                // 캐는 동안 차체가 잘게 떨린다. 프레임 수가 아니라 시간에 비례한다(CLAUDE.md 5번).
                surfaceMover.positionOffset = isMoving
                    ? Vector3.zero
                    : new Vector3(Mathf.Sin(Time.time * 41f), Mathf.Sin(Time.time * 33f), Mathf.Sin(Time.time * 47f)) * 0.06f;
            }

            if (veinField != null)
            {
                // 이동 중에도 다음 광맥을 알려 준다 — 강조는 채굴 중에만 켜진다.
                veinField.SetActiveVein(_run.TargetVeinIndex(_planet), !isMoving);
            }
            // D14-N: 이동/채굴 전환마다 루프음을 맞바꾼다. 클립이 없으면(지금은 전부 그렇다)
            // AudioHub가 스스로 조용히 아무 일도 안 한다 — 무음 플레이스홀더.
            audioHub?.SetMovementLoop(isMoving);

            RecoverFuel();

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

        /// <summary>지금 상태(행성·채굴차·원석·정제 광물·부품)를 세이브에 반영하고 디스크에 쓴다.</summary>
        public void Save()
        {
            _save.CurrentPlanetId = planetId;
            _save.LastSeenUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _save.Rig = MiningRigSave.FromCore(rig);
            _save.RawMinerals = RawMinerals;
            _save.RefinedMinerals = RefinedMinerals;
            _save.OwnedPartIds = new List<string>(OwnedPartIds);
            _save.OwnedPartEnhanceLevels = OwnedPartIds.ConvertAll(id => _partEnhanceLevels.TryGetValue(id, out var lvl) ? lvl : 0);
            _save.EquippedPartIds = EquippedIdsInSlotOrder();
            _save.Fuel = Fuel;
            _save.FuelBaselineUnixSeconds = _fuelBaselineUnixSeconds;
            _save.ApplyPurchaseState(_purchases);
            _save.ApplyRewardAdState(_rewardAds);
            SaveService.Save(_save);
        }

        /// <summary>D09-N: 코어 RaceFuel.Recover를 지금 시각으로 부른다. 매 프레임 불러도 싼
        /// 정수 나눗셈 하나뿐이라 문제없다 — RecoverySeconds(10분)가 지나기 전까진 그냥 그대로.</summary>
        void RecoverFuel()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            (Fuel, _fuelBaselineUnixSeconds) = RaceFuel.Recover(Fuel, _fuelBaselineUnixSeconds, now);
        }

        /// <summary>D08-N: 세이브에 담긴 보유/장착 부품 id를 코어 RacingCar로 복원한다. id로
        /// 실제 Part를 찾을 때는 AvailableParts(지금은 쿼츠 5종)에서 찾는다 — 카탈로그에 없는
        /// id(예: 나중에 부품이 개편돼 id가 바뀐 경우)는 조용히 건너뛴다, 세이브가 깨지는 것보다
        /// 그 부품만 잃는 쪽이 낫다.</summary>
        void LoadParts(SaveData save)
        {
            OwnedPartIds = new List<string>(save.OwnedPartIds ?? new List<string>());

            // 강화 단계(병렬 리스트)를 OwnedPartIds보다 먼저 채워 둔다 — 아래 FindPart가
            // AvailableParts(강화 단계를 입혀서 돌려준다)를 부르기 때문에 순서가 중요하다.
            _partEnhanceLevels.Clear();
            var levels = save.OwnedPartEnhanceLevels;
            for (int i = 0; i < OwnedPartIds.Count; i++)
                _partEnhanceLevels[OwnedPartIds[i]] = levels != null && i < levels.Count ? levels[i] : 0;

            Car = new RacingCar();

            var equipped = save.EquippedPartIds;
            if (equipped == null) return;
            for (int i = 0; i < SlotOrder.Length && i < equipped.Count; i++)
            {
                var id = equipped[i];
                if (string.IsNullOrEmpty(id)) continue;
                var part = FindPart(id);
                if (part != null) Car.Slots[SlotOrder[i]] = part;
            }
        }

        List<string> EquippedIdsInSlotOrder()
        {
            var list = new List<string>(SlotOrder.Length);
            foreach (var slot in SlotOrder)
                list.Add(Car.Slots.TryGetValue(slot, out var p) && p != null ? p.Id : "");
            return list;
        }

        Part FindPart(string id)
        {
            foreach (var p in AvailableParts) if (p.Id == id) return p;
            return null;
        }

        /// <summary>비용을 내고 부품을 제작해 보유 목록에 더한다. 이미 보유했거나 정제 광물이
        /// 부족하면 아무 일도 안 하고 false — UI(CraftingPanel)는 이 하나만 부르면 된다.</summary>
        public bool TryCraftPart(Part part)
        {
            if (!PartCraft.CanCraft(OwnedPartIds, part)) return false;
            var cost = PartCraft.Cost(part.Grade);
            if (!TrySpendRefinedMinerals(cost)) return false;
            OwnedPartIds.Add(part.Id);
            return true;
        }

        /// <summary>보유한 부품을 자기 슬롯에 장착한다. 그 슬롯에 이미 있던 부품은 해제된다
        /// (보유 목록에는 남아 다시 장착 가능). 미보유 부품이면 false.</summary>
        public bool TryEquipPart(Part part) => PartEquip.TryEquip(Car, OwnedPartIds, part);

        /// <summary>해당 슬롯을 비운다.</summary>
        public void UnequipPart(PartSlot slot) => PartEquip.Unequip(Car, slot);

        /// <summary>D12-N: 보유한 부품을 한 단계 강화한다(+10까지, 실패 없음 — GDD). 비용은
        /// PartEnhance.Cost, 실제 반영은 PartEnhance.Apply(part.Enhance++). 강화 결과는
        /// _partEnhanceLevels(→세이브)에 기록해서 다음 조회(AvailableParts)에서도 유지되게
        /// 한다. 지금 장착 중인 슬롯이 같은 부품이면 그 인스턴스도 즉시 맞춰 준다 — 장착된
        /// Part는 AvailableParts가 아니라 Awake 시점에 만든 별도 인스턴스라(Car.Slots에 저장된
        /// 그대로), 안 맞춰 주면 화면(강화)과 실제 레이스 스탯(장착)이 따로 논다.</summary>
        public bool TryEnhancePart(Part part)
        {
            if (part == null || !OwnedPartIds.Contains(part.Id)) return false;
            var cost = PartEnhance.Cost(part);
            if (float.IsPositiveInfinity(cost) || !TrySpendRefinedMinerals(cost)) return false;

            PartEnhance.Apply(part);
            _partEnhanceLevels[part.Id] = part.Enhance;

            foreach (var slot in SlotOrder)
            {
                var equipped = Car.Slots[slot];
                if (equipped != null && equipped.Id == part.Id) equipped.Enhance = part.Enhance;
            }

            Save();
            return true;
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
                RefinedGained = discoveries.Mining.RefinedGained,
                TreasureValue = ExplorationSimulator.MineableValue(discoveries.Treasures),
                TreasuresFound = discoveries.Treasures.Count,
                TreasuresMineable = mineableNow,
            };
        }

        /// <summary>화면의 "받기" 버튼 하나가 이 함수를 부른다. M-02: 원석(Minerals)은 원석대로,
        /// 정제 산출(RefinedGained)과 보물 환산치(TreasureValue — TreasureDef 주석대로 원래
        /// "정제 광물 환산치"다)는 정제 광물로 나눠서 더한다. 원석 쪽은 접속 중 이미 화물칸에
        /// 남아 있던 값과 합치는 것이라 다시 한번 ClampToCargoCapacity로 상한을 확인한다 —
        /// 안 그러면 둘을 더한 값이 상한을 넘을 수 있다(오프라인 계산 자체는 항상 원석 0에서
        /// 시작한다고 가정하므로). 보상이 없으면 false.
        /// 알려진 한계: 보상 값 자체는 세이브 파일에 안 남고 이번 세션 메모리에만 있다 — "받기"를
        /// 누르기 전에 자동 저장(AutosaveIntervalSeconds)이나 일시정지 저장이 먼저 일어나 버리면
        /// LastSeenUnixSeconds가 앞당겨지긴 해도 이미 계산해 둔 값은 그대로 살아 있어 괜찮지만,
        /// 화면을 아예 안 보고 앱을 껐다 켜면(그 사이 저장이 한 번이라도 있었다면) 다음 실행 때는
        /// 짧아진 경과 시간만 남아 그 보상이 사라진다. 받지 않은 보상을 세이브에 그대로 들고
        /// 다니게 하려면 SaveData에 pending 필드를 추가해야 하는데, 지금은 첫 구현이라 범위를
        /// 좁혀 뒀다 — 실제로 문제가 되면(플레이테스트에서 보상이 자꾸 사라진다는 피드백 등) 그때 늘릴 것.</summary>
        public bool ClaimOfflineReward() => ApplyPendingOfflineReward(1f);

        /// <summary>M-09 후속: 오프라인 보상 화면의 "광고 보고 2배 받기" 버튼 하나가 이 함수만
        /// 부른다. CanWatchRewardAd로 오늘 한도를 먼저 확인하고(넘겼으면 아무 일도 안 하고 false —
        /// 화면이 버튼을 disable해 두는 게 정상 경로지만 방어적으로 한 번 더 본다), 통과하면
        /// RecordRewardAdWatched로 카운트를 올린 다음 2배로 지급한다.</summary>
        public bool ClaimOfflineRewardDoubled()
        {
            if (_pendingOfflineReward == null || !CanWatchRewardAd(RewardAdSlot.OfflineRewardDouble)) return false;
            RecordRewardAdWatched(RewardAdSlot.OfflineRewardDouble);
            return ApplyPendingOfflineReward(2f);
        }

        /// <summary>ClaimOfflineReward/ClaimOfflineRewardDoubled가 공유하는 실제 지급 로직.
        /// M-02: 원석(Minerals)은 원석대로, 정제 산출(RefinedGained)과 보물 환산치(TreasureValue —
        /// TreasureDef 주석대로 원래 "정제 광물 환산치"다)는 정제 광물로 나눠서 더한다. 원석 쪽은
        /// 접속 중 이미 화물칸에 남아 있던 값과 합치는 것이라 다시 한번 CargoCapacityMinerals로
        /// 상한을 확인한다 — 안 그러면 둘을 더한 값이 상한을 넘을 수 있다(오프라인 계산 자체는
        /// 항상 원석 0에서 시작한다고 가정하므로). 보상이 없으면 false.
        /// 알려진 한계: 보상 값 자체는 세이브 파일에 안 남고 이번 세션 메모리에만 있다 — "받기"를
        /// 누르기 전에 자동 저장(AutosaveIntervalSeconds)이나 일시정지 저장이 먼저 일어나 버리면
        /// LastSeenUnixSeconds가 앞당겨지긴 해도 이미 계산해 둔 값은 그대로 살아 있어 괜찮지만,
        /// 화면을 아예 안 보고 앱을 껐다 켜면(그 사이 저장이 한 번이라도 있었다면) 다음 실행 때는
        /// 짧아진 경과 시간만 남아 그 보상이 사라진다. 받지 않은 보상을 세이브에 그대로 들고
        /// 다니게 하려면 SaveData에 pending 필드를 추가해야 하는데, 지금은 첫 구현이라 범위를
        /// 좁혀 뒀다 — 실제로 문제가 되면(플레이테스트에서 보상이 자꾸 사라진다는 피드백 등) 그때 늘릴 것.</summary>
        bool ApplyPendingOfflineReward(float multiplier)
        {
            if (_pendingOfflineReward == null) return false;
            var reward = _pendingOfflineReward.Value;
            RawMinerals = Mathf.Min(RawMinerals + reward.Minerals * multiplier, CargoCapacityMinerals);
            RefinedMinerals += (reward.RefinedGained + reward.TreasureValue) * multiplier;
            _pendingOfflineReward = null;
            Save();
            return true;
        }

        /// <summary>화물칸 상한(원석 기준). HUD 게이지가 이 값 대비 RawMinerals를 채워서 보여준다.
        /// M-01(2026-09-14)부터 접속 중에도 실제로 이 값에서 채굴이 멈춘다(Update의 클램프) —
        /// decisions.md T-06이 A안(온라인에도 적용)으로 정리됨. M-07부터 화물칸 확장(상점)·구독
        /// 배율(Entitlements.CargoMultiplier)도 여기서 곱한다 — 이 프로퍼티 하나만 쓰면 어디서
        /// 읽든(HUD 게이지, Update의 클램프, 오프라인 보상) 항상 같은 상한을 본다. M-09 후속:
        /// "화물칸 가득 참" 광고 보상(1시간 2배, RewardAdBoost)도 여기서 같이 곱한다 — Entitlements
        /// 배율과는 독립적인 별개 배율이라(둘 다 켜져 있으면 곱해져서 겹친다, monetization.md에
        /// 중복 방지 대상으로 명시된 게 아니라서) Math.Max가 아니라 곱셈으로 합친다.</summary>
        // M-11(2026-09-18): 판별 배율(Steam ×1.5)도 여기서 같이 곱한다. Entitlements(구매·구독)와는
        // 완전히 독립이다 — 무엇을 샀는지가 아니라 판 자체가 다른 것이라, monetization.md 4장이
        // "Steam은 화물칸 기본 상한을 1.5배 넉넉하게"라고 정해 둔 그 값이다. CargoHours가
        // BaseCargoHours에 정비례하니 여기서 결과에 곱하는 것과 BaseCargoHours에 곱하는 것이 같다.
        // 모바일은 1배라 지금 빌드에서는 아무것도 안 바뀐다.
        public float CargoCapacityMinerals => MiningSimulator.CargoCapacityMinerals(rig, _planet)
            * Entitlements.CargoMultiplier
            * PlatformConfig.CargoBaseMultiplier(Platform)
            * RewardAdBoost.CargoCapMultiplier(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), _save.CargoCapDoubleHourExpiresUnixSeconds);

        /// <summary>M-11: 지금 이 판(모바일/Steam). 상점 목록(ShopUgui·ShopPanel)과 위의 화물칸
        /// 상한이 이것을 읽는다. 기본은 빌드 정의가 정하고, 인스펙터에서 켜면 손으로 덮어쓸 수 있다.</summary>
        public StorePlatform Platform => overrideStorePlatform ? storePlatformOverride : GamePlatform.Build;

        /// <summary>M-07: 지금 적용해야 할 구매·구독 효과. Entitlements.Effective 한 곳에서만
        /// 계산한다(M-06 주석 참고) — 다른 코드는 PurchaseState를 직접 들여다보지 않고 이것만 읽는다.</summary>
        public Entitlements Entitlements => Entitlements.Effective(_purchases, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        /// <summary>화면(ShopPanel)이 "보유 중"/"활성" 같은 상태 문구를 그릴 때만 읽는 원 데이터.
        /// 배율·값 계산에는 쓰지 않는다(Entitlements 프로퍼티 주석 참고).</summary>
        public PurchaseState Purchases => _purchases;

        /// <summary>M-07: 상점 화면의 구매 버튼 하나가 이 함수만 부른다. 실제 결제 SDK(영수증 검증,
        /// P3)가 붙기 전이라 지금은 누르면 바로 결제가 성공한 것으로 치는 디버그 구매다 — 나중에
        /// 영수증 검증이 들어오면 이 함수를 부르기 전 단계에 넣을 자리(TODO). 실패하는 경우가
        /// 없어서(ShopPurchase.Apply는 항상 성공, 값은 상태 변경 정도) 반환값이 없다.</summary>
        public void DebugPurchase(ShopSkuId skuId)
        {
            _purchases = ShopPurchase.Apply(_purchases, skuId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Save();
        }

        /// <summary>M-04: CargoFullPanel이 "정제로 돌리시겠어요?" 화면을 닫을 때 부른다.</summary>
        public void AcknowledgeCargoFull() => CargoJustFilled = false;

        /// <summary>M-08: 지금 스타터 팩 제안을 같이 보여줘야 하는가(monetization.md 2-1 "노출은
        /// 첫 상한 도달 직후 한 번"). CargoFullPanel이 CargoJustFilled로 화면 자체를 띄운 다음,
        /// 이 값이 true일 때만 스타터 팩 칸을 추가로 그린다 — 그래서 이 판정은 CargoJustFilled와
        /// 별개로 세이브에 영구히 남은 "첫 상한 도달 여부"만 본다(단순 엣지 트리거가 아니다).</summary>
        public bool ShouldShowStarterPackOffer =>
            StarterPackOffer.ShouldShow(_save.HasReachedCargoCapBefore, _save.StarterPackOfferDeclined, _purchases.CargoExpansionLevel);

        /// <summary>스타터 팩 칸의 "괜찮아요" 버튼이 이 함수만 부른다. 한 번 거절하면 다시 안
        /// 뜬다(ShouldShowStarterPackOffer가 이후 계속 false) — 구매했을 때는 CargoExpansionLevel이
        /// 이미 올라가 있어서 따로 이 함수를 부를 필요가 없다(ShouldShowStarterPackOffer 참고).</summary>
        public void DeclineStarterPackOffer()
        {
            _save.StarterPackOfferDeclined = true;
            Save();
        }

        /// <summary>M-09: 이 자리에서 오늘 보상형 광고를 더 볼 수 있는가. 네 화면(오프라인 보상·
        /// 레이스 결과·화물칸 가득 참·연료 부족)의 광고 버튼이 활성/비활성을 정할 때 이것만 본다.</summary>
        public bool CanWatchRewardAd(RewardAdSlot slot) =>
            RewardAdTracker.CanWatch(_rewardAds, slot, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), KstOffsetSeconds);

        /// <summary>오늘 이 자리에서 몇 번 더 볼 수 있는지("2/3회 남음" 같은 표시용).</summary>
        public int RemainingRewardAdsToday(RewardAdSlot slot) =>
            RewardAdTracker.RemainingToday(_rewardAds, slot, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), KstOffsetSeconds);

        /// <summary>광고 재생이 끝나고 보상 콜백이 들어온 순간 부른다 — 카운터만 올리고 저장한다.
        /// 실제 보상(오프라인 2배/상자 1개 더/상한 2배 1시간/연료 +3)을 실제로 지급하는 것은 각
        /// 화면이 이 함수 호출 뒤에 자기 로직으로 한다(예: 상자는 RustyBoxCount++, 연료는
        /// Fuel += 3) — 광고 SDK 연동 자체가 P3라 지금은 자리와 카운터만 잇는다(backlog M-09).</summary>
        public void RecordRewardAdWatched(RewardAdSlot slot)
        {
            _rewardAds = RewardAdTracker.RecordWatch(_rewardAds, slot, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), KstOffsetSeconds);
            Save();
        }

        /// <summary>M-09 후속: 레이스 결과 화면의 "광고 보고 상자 1개 더" 버튼 하나가 이 함수만
        /// 부른다. 어떤 등급의 상자를 더 줄지는 방금 이긴 코스의 RaceBoxReward.ForTier로 화면이
        /// 이미 알고 있는 값을 그대로 넘긴다(TryEnterRace가 준 것과 같은 등급) — 지금 코어 보상이
        /// 없는데 광고로 없던 등급을 만들어 낼 순 없으니, 화면이 nullable을 미리 걸러서 이긴 판이
        /// 아니면 애초에 이 버튼을 안 보여줄 것.</summary>
        public bool WatchAdForExtraLootBox(LootBoxType type)
        {
            if (!CanWatchRewardAd(RewardAdSlot.ExtraLootBox)) return false;
            RecordRewardAdWatched(RewardAdSlot.ExtraLootBox);
            if (type == LootBoxType.Rusty) _save.RustyBoxCount++;
            else if (type == LootBoxType.Steel) _save.SteelBoxCount++;
            else if (type == LootBoxType.Titanium) _save.TitaniumBoxCount++;
            Save();
            return true;
        }

        /// <summary>M-09 후속: 화물칸 가득 참 화면의 "광고 보고 1시간 상한 2배" 버튼 하나가 이
        /// 함수만 부른다. RewardAdBoost.ExtendCargoCapDoubleHour가 이미 켜진 중이면 만료 시각부터,
        /// 꺼져 있으면 지금부터 1시간을 계산해 주므로 여기서는 그 결과를 세이브에 앉히기만 한다 —
        /// CargoCapacityMinerals가 다음 프레임부터 바로 2배로 읽는다.</summary>
        public bool WatchAdForCargoCapDouble()
        {
            if (!CanWatchRewardAd(RewardAdSlot.CargoCapDoubleHour)) return false;
            RecordRewardAdWatched(RewardAdSlot.CargoCapDoubleHour);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _save.CargoCapDoubleHourExpiresUnixSeconds = RewardAdBoost.ExtendCargoCapDoubleHour(_save.CargoCapDoubleHourExpiresUnixSeconds, now);
            Save();
            return true;
        }

        /// <summary>화물칸 상한 2배가 지금부터 몇 초 남았는지(꺼져 있으면 0) — 화면이 "42:10 남음"
        /// 같은 표시를 하고 싶을 때만 읽는다. CargoCapacityMinerals 계산 자체는 이 값이 아니라
        /// 만료 시각을 직접 RewardAdBoost.CargoCapMultiplier에 넘겨서 판정한다.</summary>
        public long CargoCapDoubleHourRemainingSeconds =>
            Math.Max(0L, _save.CargoCapDoubleHourExpiresUnixSeconds - DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        /// <summary>M-09 후속: 연료 부족 화면(RaceEntryPanel의 entry-view, 연료가 EntryCost 미만일 때)의
        /// "광고 보고 연료 +3" 버튼 하나가 이 함수만 부른다. RaceFuel.MaxFuel을 넘기지 않는다 —
        /// RaceFuel.Recover가 최대치를 절대 안 넘기는 것과 같은 방어.</summary>
        public bool WatchAdForFuelRefill()
        {
            if (!CanWatchRewardAd(RewardAdSlot.FuelRefill)) return false;
            RecordRewardAdWatched(RewardAdSlot.FuelRefill);
            Fuel = Math.Min(RaceFuel.MaxFuel, Fuel + 3);
            Save();
            return true;
        }

        /// <summary>D05-N, M-02부터 정제 광물로 냄: 업그레이드·제작·강화가 전부 이 함수 하나로
        /// 값을 낸다(RigUpgrade.cs·PartCraft.cs·PartEnhance.cs 주석에 이미 "정제 광물"이라
        /// 적혀 있던 그대로). 실패해도(정제 광물 부족) 예외 없이 false만 돌려준다.</summary>
        public bool TrySpendRefinedMinerals(float amount)
        {
            if (amount > RefinedMinerals) return false;
            RefinedMinerals -= amount;
            return true;
        }

        /// <summary>원석으로 값을 낸다. 지금은 제련소 업그레이드 한 군데만 쓴다
        /// (RigUpgrade.IsPaidWithRawMinerals). 실패해도 예외 없이 false만 돌려준다.</summary>
        public bool TrySpendRawMinerals(float amount)
        {
            if (amount > RawMinerals) return false;
            RawMinerals -= amount;
            return true;
        }

        /// <summary>비용을 내고 해당 슬롯 레벨을 올린다. 이미 최대 레벨이거나 화폐가 모자라면
        /// 아무 일도 안 하고 false를 돌려준다 — UI는 이 하나만 부르면 된다(UpgradeUgui).
        ///
        /// 화폐가 슬롯마다 다르다. 제련소만 **원석**으로 사고 나머지는 정제 광물로 산다
        /// (2026-09-17). 정제 광물을 만드는 장치를 정제 광물로 사게 해 두면 첫 구매가 영영
        /// 불가능해지기 때문이다 — 그 상태가 실제로 사흘 갔다. RigUpgrade.cs UpgradeSlot 주석 참고.</summary>
        public bool TryUpgrade(UpgradeSlot slot)
        {
            var cost = UpgradeCost.Cost(slot, rig);
            if (float.IsPositiveInfinity(cost)) return false;
            var paid = UpgradeCost.IsPaidWithRawMinerals(slot)
                ? TrySpendRawMinerals(cost)
                : TrySpendRefinedMinerals(cost);
            if (!paid) return false;
            rig = UpgradeCost.Apply(slot, rig);
            return true;
        }

        /// <summary>D11-N 후속: 지금 보유한 공구 상자 개수. _save를 그대로 읽기만 한다 — Save()가
        /// 안 다루는 필드는 로드된 값이 _save에 그대로 남아 있으니(위 Save() 주석과 같은 원리)
        /// 따로 캐시 필드를 안 둬도 된다.</summary>
        public int RustyBoxCount => _save.RustyBoxCount;
        public int SteelBoxCount => _save.SteelBoxCount;
        public int TitaniumBoxCount => _save.TitaniumBoxCount;

        /// <summary>D11-N 후속(개봉 화면): 상자 하나를 연다. 보유 개수가 0이면 false — 화면(LootBoxPanel)은
        /// 이 하나만 부르면 된다. 등급·슬롯 뽑기 seed는 TryEnterRace와 같은 이유로 여기서
        /// UnityEngine.Random으로 매번 다르게 뽑는다(코어는 seed를 인자로만 받는다, CLAUDE.md 1번).
        /// 결과(LootBoxOpener.Open)를 즉시 RigPartApply.Apply로 적용하고, 상자 개수를 깎고,
        /// 강철·티타늄의 천장 카운터(_save.SteelOpenedSincePity 등)를 갱신한 뒤 저장한다.</summary>
        public bool TryOpenBox(LootBoxType type, out LootBoxOpenResult result)
        {
            result = default;
            if (BoxCount(type) <= 0) return false;

            var openedSincePity = type == LootBoxType.Steel ? _save.SteelOpenedSincePity
                : type == LootBoxType.Titanium ? _save.TitaniumOpenedSincePity : 0;

            var gradeSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var slotSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            result = LootBoxOpener.Open(type, gradeSeed, slotSeed, openedSincePity);

            rig = RigPartApply.Apply(rig, result.Reward);

            if (type == LootBoxType.Rusty) _save.RustyBoxCount--;
            else if (type == LootBoxType.Steel) { _save.SteelBoxCount--; _save.SteelOpenedSincePity = result.NextOpenedSincePity; }
            else if (type == LootBoxType.Titanium) { _save.TitaniumBoxCount--; _save.TitaniumOpenedSincePity = result.NextOpenedSincePity; }

            Save();
            return true;
        }

        int BoxCount(LootBoxType type) => type == LootBoxType.Rusty ? RustyBoxCount
            : type == LootBoxType.Steel ? SteelBoxCount
            : type == LootBoxType.Titanium ? TitaniumBoxCount : 0;

        /// <summary>D13-N: 튜토리얼 안내 말풍선 개수(첫 접속·채굴 시작·첫 부품 제작·첫 레이스).</summary>
        public const int TutorialStepCount = 4;

        /// <summary>지금 몇 번째 말풍선까지 봤는지(0~TutorialStepCount). _save를 그대로 읽는다 —
        /// RustyBoxCount 등과 같은 이유로 별도 캐시 필드가 필요 없다.</summary>
        public int TutorialStep => _save.TutorialStep;

        /// <summary>화면(TutorialController)의 "다음" 버튼 하나가 이 함수만 부른다. 정확히 한
        /// 단계만 올리고 저장한다 — 화면이 TutorialStep을 직접 못 건드리게 프로퍼티를 읽기 전용으로
        /// 두고 이 함수 하나만 통로로 남긴 게 "단계 건너뛰기 방지"(D13-M)의 전부다. 이미 다 지났으면
        /// (TutorialStep >= TutorialStepCount) 아무 일도 안 하고 false.</summary>
        public bool AdvanceTutorial()
        {
            if (_save.TutorialStep >= TutorialStepCount) return false;
            _save.TutorialStep++;
            Save();
            return true;
        }

        /// <summary>D14-N: 설정 화면이 읽는 값 둘. _save를 그대로 읽는다 — RustyBoxCount 등과
        /// 같은 이유로 별도 캐시 필드가 필요 없다.</summary>
        public bool SoundEnabled => _save.SoundEnabled;
        public int TargetFrameRate => _save.TargetFrameRate;

        /// <summary>설정 화면의 소리 토글 버튼 하나가 이 함수만 부른다. AudioListener.volume을
        /// 전역으로 낮추는 방식이라 — 아직 클립이 하나도 없어도(AudioHub가 전부 무음 플레이스홀더)
        /// 미리 배선해 두면 나중에 클립만 채워 넣어도 바로 먹는다.</summary>
        public void SetSoundEnabled(bool enabled)
        {
            _save.SoundEnabled = enabled;
            AudioListener.volume = enabled ? 1f : 0f;
            Save();
        }

        /// <summary>설정 화면의 30/60 버튼이 이 함수만 부른다. GameSettings.NormalizeFrameRate로
        /// 정규화해서 저장하고 Application.targetFrameRate에도 즉시 반영한다.</summary>
        public void SetTargetFrameRate(int fps)
        {
            var normalized = GameSettings.NormalizeFrameRate(fps);
            _save.TargetFrameRate = normalized;
            Application.targetFrameRate = normalized;
            Save();
        }

        /// <summary>D09-N: 쿼츠 로컬 레이스 3개 중 하나에 출전한다. 연료(RaceFuel.EntryCost)를
        /// 먼저 내고(부족하면 false, 아무 것도 안 바뀜) 코어 RaceSimulator로 순위를 계산한다.
        /// 1등이면 그 코스의 RigPartReward(L-03)를 적용해 채굴차 슬롯 레벨을 올리고, 코스 등급
        /// (RaceTier)에 맞는 공구 상자를 하나 준다(RaceBoxReward, D11-N 후속 — GDD "로컬=녹슨"이
        /// 지금까지 코드에 실제로 반영돼 있지 않았다). 상자는 _save에 바로 더한다 — 개봉은 아직
        /// 안 만든 화면(다음 세션, LootBoxOpener.Open을 부르면 됨) 몫이라 세는 것까지만 한다.
        /// 결과 자체는 세이브에 남기지 않는다 — 재연출이 필요하다고 판단되면 그때 SaveData에
        /// 필드를 추가할 것(D07-N의 pending 보상과 같은 확장 지점).
        /// seed는 매 출전마다 다르게(UnityEngine.Random) 뽑는다 — 코어는 seed를 인자로만 받을 뿐
        /// 스스로 난수를 안 쓰니(CLAUDE.md 1번) "이번 판의 seed를 정하는" 몫은 이 글루 레이어가 진다.</summary>
        public bool TryEnterRace(Course course, out List<RaceSimulator.Result> results, out bool won)
        {
            results = null; won = false;
            if (Fuel < RaceFuel.EntryCost) return false;

            Fuel -= RaceFuel.EntryCost;

            var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var playerStats = Car.TotalStats();
            var entrants = new List<RaceSimulator.Entrant>
            {
                new RaceSimulator.Entrant { Id = "player", Stats = playerStats, IsPlayer = true },
            };
            entrants.AddRange(RaceSimulator.MakeOpponents(5, EstimateOpponentStrength(playerStats), seed));

            results = RaceSimulator.Run(entrants, _planet, course, seed);
            var playerRank = results.Find(r => r.Id == "player").Rank;
            won = playerRank == 1;

            if (won)
            {
                var reward = DefaultData.QuartzLocalRaceRewards().Find(r => r.CourseId == course.Id);
                if (reward != null) rig = RigPartApply.Apply(rig, reward);

                // switch가 아니라 == 비교로 쓴다 — nullable enum(LootBoxType?)을 switch로 매치하는
                // 문법이 이 Unity 버전에서 확실히 되는지 에디터 없이는 못 미더워서(CLAUDE.md 규칙),
                // 오래전부터 있던 단순한 lifted 비교 연산자만 쓴다.
                var box = RaceBoxReward.ForTier(course.Tier);
                if (box == LootBoxType.Rusty) _save.RustyBoxCount++;
                else if (box == LootBoxType.Steel) _save.SteelBoxCount++;
                else if (box == LootBoxType.Titanium) _save.TitaniumBoxCount++;
            }

            Save();
            return true;
        }

        /// <summary>임시 난이도 곡선: 플레이어 평균 스탯의 90%로 상대를 잡아서 첫 레이스는 이길 수
        /// 있게 한다. 실제 값은 P4 봇 시뮬레이션에서 재조정할 플레이스홀더(TODO).</summary>
        static float EstimateOpponentStrength(Stats s)
        {
            var avg = (s.Power + s.Grip + s.Suspension + s.Durability + s.Boost + s.Aero) / 6f;
            return avg * 0.9f;
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
                $"원석 {RawMinerals:F1} / 정제 {RefinedMinerals:F1}\n{phaseLabel} (남은 시간 {PhaseSecondsRemaining:F1}s)");
        }
    }
}
