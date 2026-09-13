using System;
using System.Collections.Generic;
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

        /// <summary>이번 세션 + 이전 세이브에서 이어진 원석(정제 전) 총량.</summary>
        public float RawMinerals { get; private set; }
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
            _run = new MiningRunState(rig, _planet);
            if (surfaceMover == null) surfaceMover = GetComponent<SurfaceMover>();

            LoadParts(_save);
            ComputeOfflineReward(_save.LastSeenUnixSeconds);

            Fuel = _save.Fuel;
            _fuelBaselineUnixSeconds = _save.FuelBaselineUnixSeconds;
            RecoverFuel(); // baseline이 0(첫 세이브)이거나 오래 지났으면 여기서 바로 맞춰 둔다
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

        /// <summary>지금 상태(행성·채굴차·원석·부품)를 세이브에 반영하고 디스크에 쓴다. 이
        /// 컨트롤러가 안 다루는 필드(정제 광물)는 로드된 값을 그대로 둔다 — 제련(Refinery)이
        /// 생기면 그때 다룰 필드.</summary>
        public void Save()
        {
            _save.CurrentPlanetId = planetId;
            _save.LastSeenUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _save.Rig = MiningRigSave.FromCore(rig);
            _save.RawMinerals = RawMinerals;
            _save.OwnedPartIds = new List<string>(OwnedPartIds);
            _save.OwnedPartEnhanceLevels = OwnedPartIds.ConvertAll(id => _partEnhanceLevels.TryGetValue(id, out var lvl) ? lvl : 0);
            _save.EquippedPartIds = EquippedIdsInSlotOrder();
            _save.Fuel = Fuel;
            _save.FuelBaselineUnixSeconds = _fuelBaselineUnixSeconds;
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

        /// <summary>비용을 내고 부품을 제작해 보유 목록에 더한다. 이미 보유했거나 원석이 부족하면
        /// 아무 일도 안 하고 false — UI(CraftingPanel)는 이 하나만 부르면 된다.</summary>
        public bool TryCraftPart(Part part)
        {
            if (!PartCraft.CanCraft(OwnedPartIds, part)) return false;
            var cost = PartCraft.Cost(part.Grade);
            if (!TrySpendRawMinerals(cost)) return false;
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
            if (float.IsPositiveInfinity(cost) || !TrySpendRawMinerals(cost)) return false;

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
                $"원석 {RawMinerals:F1}\n{phaseLabel} (남은 시간 {PhaseSecondsRemaining:F1}s)");
        }
    }
}
