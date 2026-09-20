using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>
    /// 세이브 파일 전체. 순수 데이터 클래스라 Unity의 JsonUtility로도, 서버 쪽 JSON
    /// 라이브러리로도 그대로 직렬화된다 — 그래서 전부 public 필드, 리스트/문자열/숫자로만
    /// 구성한다(Dictionary·프로퍼티는 JsonUtility가 못 다룬다).
    /// [Serializable]은 System 표준 애트리뷰트라 UnityEngine을 참조하지 않는다(CLAUDE.md 1번).
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        /// <summary>세이브 포맷이 바뀌면 올린다. 지금은 마이그레이션 로직이 없다 — 필요해지면 여기서 분기.</summary>
        public int Version = 1;

        public string CurrentPlanetId = "quartz";

        /// <summary>마지막으로 저장한 시각(UTC epoch 초). 오프라인 채굴 계산에 쓴다.
        /// TODO: 지금은 로컬 시계 값이고, 대전이 붙으면 서버 인증 시각으로 바꿀 자리다(D07-N 참고).</summary>
        public long LastSeenUnixSeconds;

        public MiningRigSave Rig = new MiningRigSave();

        /// <summary>P-05: 채굴 장비 다섯 칸에 쌓인 증폭률 합(amplifier.md, Amplifier.cs 참고).
        /// 누적 상한은 아직 안 정해져서(amplifier.md "정해야 하는 것") 여기서도 자르지 않는다 —
        /// 상한이 정해지면 AddAmplifier가 그때 자르면 된다.</summary>
        public RigAmplifierSave Amplifiers = new RigAmplifierSave();

        /// <summary>상자에서 나온 증폭기(AmplifierReward) 하나를 해당 칸에 더한다. 어느 칸에
        /// 꽂을지(무작위/플레이어 선택)는 아직 안 정해진 채라(amplifier.md, LootReward.cs
        /// AmplifierReward 주석) 이 함수는 "칸이 정해지면 더하는" 마지막 단계만 맡는다 —
        /// 호출하는 쪽(Unity, 아직 없음)이 슬롯을 고른다.</summary>
        public void AddAmplifier(RigSlot slot, float bonus) => Amplifiers.Add(slot, bonus);

        /// <summary>P-05(레이싱카 쪽): 레이싱카 다섯 칸(Engine/Tire/Suspension/Body/Booster)에
        /// 쌓인 증폭률 합. 채굴 쪽(Amplifiers)과 같은 이유로 상한을 안 자른다.</summary>
        public PartAmplifierSave PartAmplifiers = new PartAmplifierSave();

        /// <summary>레이싱카 부품 칸용 AddAmplifier — 위 RigSlot 버전과 같은 진입 패턴,
        /// 슬롯을 고르는 쪽은 여전히 화면(Unity) 몫이다.</summary>
        public void AddAmplifier(PartSlot slot, float bonus) => PartAmplifiers.Add(slot, bonus);

        public float RawMinerals;
        public float RefinedMinerals;

        /// <summary>P-07: 행성별로 나뉘어 "보관된" 정제 광물 창고(PlanetMineralBank.cs 참고).
        /// 위 RawMinerals/RefinedMinerals(지금 캐는 행성에서 진행 중인 값)와는 다르다 — Dictionary
        /// 대신 병렬 리스트인 이유는 클래스 상단 주석과 같다(JsonUtility가 Dictionary를 못 다룬다).</summary>
        public List<string> PlanetMineralIds = new List<string>();
        public List<float> PlanetMineralAmounts = new List<float>();

        /// <summary>보유 부품 id 목록(제작은 됐지만 장착 안 한 것 포함).</summary>
        public List<string> OwnedPartIds = new List<string>();

        /// <summary>D12-N: 부품별 강화 단계(+0~+10). OwnedPartIds와 같은 인덱스가 같은 부품을
        /// 가리키는 병렬 리스트다(JsonUtility가 Dictionary를 못 다뤄서, EquippedPartIds와 같은
        /// 이유). MiningController.AvailableParts가 매번 DefaultData에서 새 Part 인스턴스를
        /// 만들기 때문에(강화 단계가 인스턴스에만 있으면 다음 조회 때 사라진다) 강화 수치는
        /// 반드시 여기 저장했다가 새 인스턴스에 다시 입혀야 한다.</summary>
        public List<int> OwnedPartEnhanceLevels = new List<int>();

        /// <summary>장착 중인 부품 id. 빈 슬롯은 빈 문자열로 채워서 항상 6칸
        /// (Engine, Tire, Suspension, Body, Booster, Module 순서, PartSlot enum 순서와 동일).</summary>
        public List<string> EquippedPartIds = new List<string> { "", "", "", "", "", "" };

        /// <summary>D09-N: 레이스 출전 연료. 새 세이브는 꽉 찬 채로 시작.</summary>
        public int Fuel = RaceFuel.MaxFuel;

        /// <summary>연료 회복 시계의 기준 시각(UTC epoch초). RaceFuel.Recover의 baselineUnixSeconds
        /// 그대로 — 0이어도 문제없다(Fuel이 이미 MaxFuel이면 Recover가 첫 호출에서 지금 시각으로
        /// 당겨 준다, LastSeenUnixSeconds와 달리 "저장 안 해 봄"을 별도로 구분할 필요가 없다).</summary>
        public long FuelBaselineUnixSeconds;

        /// <summary>A-04: 코스별 자기 최고 기록(초). RaceRecordBook이 이 두 리스트를 같은 인덱스로
        /// 병렬 관리한다(OwnedPartIds/OwnedPartEnhanceLevels와 같은 이유 — JsonUtility가
        /// Dictionary를 못 다룬다, 클래스 상단 주석 참고).</summary>
        public List<string> RaceRecordCourseIds = new List<string>();
        public List<float> RaceRecordBestSeconds = new List<float>();

        /// <summary>D11-N: 공구 상자 보유 개수. 세 종류(녹슨/강철/티타늄)뿐이라 Dictionary 대신
        /// 필드 세 개로 둔다(JsonUtility가 Dictionary를 못 다룬다, 클래스 상단 주석 참고).</summary>
        public int RustyBoxCount;
        public int SteelBoxCount;
        public int TitaniumBoxCount;

        /// <summary>천장(피티) 카운터 — "이 종류를 마지막 확정 이후 몇 개 열었는지". 녹슨 상자는
        /// 천장이 없어서(LootTable.Rusty, pityCount=0) 카운터가 필요 없다. LootTable.Open이
        /// Guaranteed를 돌려준 다음 0으로 되돌리는 건 호출하는 쪽(다음 세션의 개봉 화면) 몫.</summary>
        public int SteelOpenedSincePity;
        public int TitaniumOpenedSincePity;

        /// <summary>D13-N: 튜토리얼 진행 단계(0~4). 0이면 아직 첫 말풍선도 안 봤다는 뜻, 4면 넷 다
        /// 지나서 다시 안 뜬다. MiningController.AdvanceTutorial()로만 올라간다 — 화면이 이 값을
        /// 직접 건드리지 않고, 그 함수를 부르는 것만으로 정확히 한 단계씩만 넘어간다(순서 보장).</summary>
        public int TutorialStep;

        /// <summary>D14-N: 소리 켜짐/꺼짐. 지금은 오디오 클립이 하나도 없어서(AudioHub가 전부
        /// 무음 플레이스홀더) 당장 체감은 없지만, 클립이 들어오는 순간부터 이 값이 그대로 먹는다.</summary>
        public bool SoundEnabled = true;

        /// <summary>D14-N: 목표 프레임 레이트. GameSettings.NormalizeFrameRate가 30 또는 60으로만
        /// 정규화한다. 기본은 60 — 저사양 모바일에서 버벅이면 설정 화면에서 30으로 낮출 수 있다.</summary>
        public int TargetFrameRate = GameSettings.DefaultFrameRate;

        // M-07: 상점에서 산 것. Entitlements.PurchaseState와 필드가 1:1이지만 JsonUtility가
        // nullable(long?)을 못 다뤄서(클래스 상단 주석과 같은 이유) 만료 시각 두 개는 0을
        // "산 적 없음/만료됨"으로 쓴다. ToPurchaseState()/FromPurchaseState()가 0↔null을 바꿔 준다.
        public int CargoExpansionLevel;
        public bool OfflineCapExtensionPurchased;
        public long MiningAccelPassExpiryUnixSeconds;
        public long SeasonPassSubscriptionExpiryUnixSeconds;
        public bool SteamSupporterPackPurchased;
        public bool AdRemovalPurchased;

        // M-08: 스타터 팩 노출은 세이브에 영구히 남아야 "한 번만"이 지켜진다(StarterPackOffer.cs 참고).
        // HasReachedCargoCapBefore는 MiningController.CargoJustFilled가 처음 켜지는 순간 같이
        // 켜진다(이후 상한에 다시 안 닿아도 계속 true). StarterPackOfferDeclined는 제안을 닫기로
        // 거절했을 때만 켜진다 — 산 경우는 CargoExpansionLevel로 이미 판별되니 따로 안 둔다.
        public bool HasReachedCargoCapBefore;
        public bool StarterPackOfferDeclined;

        // M-09: 보상형 광고 네 자리의 오늘 시청 횟수. RewardAdState와 필드가 1:1이고 전부
        // 비-nullable(long/int)이라 PurchaseState처럼 0↔null 변환이 따로 필요 없다 —
        // ToRewardAdState()/ApplyRewardAdState()는 그래도 만들어 뒀다(화면이 SaveData 필드를
        // 직접 안 만지고 RewardAdTracker의 결과만 읽게 하려는 것, monetization.md 6장과 같은 이유).
        public long RewardAdLastResetDayIndex;
        public int OfflineRewardDoubleWatchedToday;
        public int ExtraLootBoxWatchedToday;
        public int CargoCapDoubleHourWatchedToday;
        public int FuelRefillWatchedToday;

        /// <summary>M-09 후속: "화물칸 가득 참" 자리 보상(1시간 동안 상한 2배)이 지금 몇 시까지
        /// 켜져 있는지(UTC epoch초). 0이면 꺼져 있음 — RewardAdBoost.CargoCapMultiplier가
        /// 이 값과 지금 시각만으로 배율을 계산한다(다른 카운트들과 달리 하루 리셋과는 무관).</summary>
        public long CargoCapDoubleHourExpiresUnixSeconds;

        // M-10: 시즌 패스(SeasonPass.cs, monetization.md 2-6) 진행 상태. SeasonPassState와 필드가
        // 1:1이라 M-09 RewardAdState와 같은 이유로 전부 non-nullable(0/false가 "아직 없음"이라
        // 별도 변환이 필요 없다). ClaimedFreeTierMask/ClaimedPaidTierMask는 비트마스크라 그대로
        // long 하나씩 — JsonUtility가 long은 문제없이 다룬다.
        public int SeasonPassXp;
        public bool SeasonPassOwnsPaidTrack;
        public long SeasonPassClaimedFreeTierMask;
        public long SeasonPassClaimedPaidTierMask;

        // D18-N: 하루 첫 접속 보상(DailyLoginReward.cs). DailyLoginState와 필드가 1:1이고
        // RewardAdState처럼 전부 non-nullable이라 0이 "아직 없음"을 뜻한다.
        public long DailyLoginLastClaimedDayIndex;
        public int DailyLoginStreakDays;

        /// <summary>P-14 첫 조각: 펫 뽑기 진행 상태(pet-gacha.md 3절). 종 ID 데이터가 아직 없어서
        /// (P-17 미정, docs/backlog.md 참고) "어느 종을 가졌는지"는 못 담는다 — 대신
        /// PetCollection.CollectionBonus·PetFusion.ExchangeFor*가 실제로 받는 값(등급별
        /// "가진 종 수"·조각 수)까지만 담는다. 종 ID가 정해지면 실제 종 목록 저장을 더 얹으면
        /// 되고, 가진 종 수는 그 목록 길이로 다시 계산할 수 있다. 실제로 뽑기를 돌려 이 값을
        /// 갱신하는 컨트롤러는 아직 없다(Assets 쪽, 다음 세션 몫) — 여기는 저장 자리만 만든다.</summary>
        public PetGachaSave PetGacha = new PetGachaSave();

        /// <summary>Entitlements.Effective에 그대로 넘길 수 있는 형태로 바꾼다.</summary>
        public PurchaseState ToPurchaseState() => new PurchaseState
        {
            CargoExpansionLevel = CargoExpansionLevel,
            OfflineCapExtensionPurchased = OfflineCapExtensionPurchased,
            MiningAccelPassExpiryUnixSeconds = MiningAccelPassExpiryUnixSeconds > 0 ? MiningAccelPassExpiryUnixSeconds : (long?)null,
            SeasonPassSubscriptionExpiryUnixSeconds = SeasonPassSubscriptionExpiryUnixSeconds > 0 ? SeasonPassSubscriptionExpiryUnixSeconds : (long?)null,
            SteamSupporterPackPurchased = SteamSupporterPackPurchased,
            AdRemovalPurchased = AdRemovalPurchased,
        };

        /// <summary>ShopPurchase.Apply가 돌려준 PurchaseState를 세이브에 다시 새긴다
        /// (ShopPurchase.Apply(save.ToPurchaseState(), sku, now)의 결과를 여기로 되돌리는 용도).</summary>
        public void ApplyPurchaseState(PurchaseState state)
        {
            CargoExpansionLevel = state.CargoExpansionLevel;
            OfflineCapExtensionPurchased = state.OfflineCapExtensionPurchased;
            MiningAccelPassExpiryUnixSeconds = state.MiningAccelPassExpiryUnixSeconds ?? 0;
            SeasonPassSubscriptionExpiryUnixSeconds = state.SeasonPassSubscriptionExpiryUnixSeconds ?? 0;
            SteamSupporterPackPurchased = state.SteamSupporterPackPurchased;
            AdRemovalPurchased = state.AdRemovalPurchased;
        }

        /// <summary>RewardAdTracker에 그대로 넘길 수 있는 형태로 바꾼다.</summary>
        public RewardAdState ToRewardAdState() => new RewardAdState
        {
            LastResetDayIndex = RewardAdLastResetDayIndex,
            OfflineRewardDoubleWatchedToday = OfflineRewardDoubleWatchedToday,
            ExtraLootBoxWatchedToday = ExtraLootBoxWatchedToday,
            CargoCapDoubleHourWatchedToday = CargoCapDoubleHourWatchedToday,
            FuelRefillWatchedToday = FuelRefillWatchedToday,
        };

        /// <summary>RewardAdTracker.ResetIfNewDay/RecordWatch가 돌려준 상태를 세이브에 다시 새긴다.</summary>
        public void ApplyRewardAdState(RewardAdState state)
        {
            RewardAdLastResetDayIndex = state.LastResetDayIndex;
            OfflineRewardDoubleWatchedToday = state.OfflineRewardDoubleWatchedToday;
            ExtraLootBoxWatchedToday = state.ExtraLootBoxWatchedToday;
            CargoCapDoubleHourWatchedToday = state.CargoCapDoubleHourWatchedToday;
            FuelRefillWatchedToday = state.FuelRefillWatchedToday;
        }

        /// <summary>SeasonPassProgress에 그대로 넘길 수 있는 형태로 바꾼다.</summary>
        public SeasonPassState ToSeasonPassState() => new SeasonPassState
        {
            CurrentXp = SeasonPassXp,
            OwnsPaidTrack = SeasonPassOwnsPaidTrack,
            ClaimedFreeTierMask = SeasonPassClaimedFreeTierMask,
            ClaimedPaidTierMask = SeasonPassClaimedPaidTierMask,
        };

        /// <summary>SeasonPassProgress.AddXp/Claim이 돌려준 상태를 세이브에 다시 새긴다.</summary>
        public void ApplySeasonPassState(SeasonPassState state)
        {
            SeasonPassXp = state.CurrentXp;
            SeasonPassOwnsPaidTrack = state.OwnsPaidTrack;
            SeasonPassClaimedFreeTierMask = state.ClaimedFreeTierMask;
            SeasonPassClaimedPaidTierMask = state.ClaimedPaidTierMask;
        }

        /// <summary>DailyLoginReward.CanClaim/Claim에 그대로 넘길 수 있는 형태로 바꾼다.</summary>
        public DailyLoginState ToDailyLoginState() => new DailyLoginState
        {
            LastClaimedDayIndex = DailyLoginLastClaimedDayIndex,
            StreakDays = DailyLoginStreakDays,
        };

        /// <summary>DailyLoginReward.Claim이 돌려준 상태를 세이브에 다시 새긴다.</summary>
        public void ApplyDailyLoginState(DailyLoginState state)
        {
            DailyLoginLastClaimedDayIndex = state.LastClaimedDayIndex;
            DailyLoginStreakDays = state.StreakDays;
        }
    }

    /// <summary>MiningRig 저장용. Core.MiningRig와 필드를 맞춰 뒀다.</summary>
    [Serializable]
    public sealed class MiningRigSave
    {
        public int ToolLevel = 1;
        public int CargoLevel = 1;
        public int EngineLevel = 1;
        public int DetectorLevel;
        public int RefineryLevel;

        public MiningRig ToCore() => new MiningRig
        {
            ToolLevel = ToolLevel, CargoLevel = CargoLevel, EngineLevel = EngineLevel,
            DetectorLevel = DetectorLevel, RefineryLevel = RefineryLevel,
        };

        public static MiningRigSave FromCore(MiningRig rig) => new MiningRigSave
        {
            ToolLevel = rig.ToolLevel, CargoLevel = rig.CargoLevel, EngineLevel = rig.EngineLevel,
            DetectorLevel = rig.DetectorLevel, RefineryLevel = rig.RefineryLevel,
        };
    }

    /// <summary>P-05: RigSlot 다섯 칸(Tool/Cargo/Engine/Detector/Refinery)에 쌓인 증폭률 합.
    /// Dictionary 대신 필드 다섯 개인 이유는 SaveData 클래스 상단 주석과 같다(JsonUtility가
    /// Dictionary를 못 다룬다) — RustyBoxCount/SteelBoxCount/TitaniumBoxCount와 같은 패턴,
    /// 칸 개수가 고정(5)이라 병렬 리스트보다 이쪽이 더 간단하다.
    /// MiningSimulator의 amp-aware 오버로드(RigSpeed/YieldPerVein/CargoHours/GemsPerHour/
    /// RefinePerHour 등)가 이 값을 그대로 받아 Amplifier.Apply로 적용한다.</summary>
    [Serializable]
    public sealed class RigAmplifierSave
    {
        public float Tool;
        public float Cargo;
        public float Engine;
        public float Detector;
        public float Refinery;

        public float Bonus(RigSlot slot) => slot switch
        {
            RigSlot.Tool => Tool,
            RigSlot.Cargo => Cargo,
            RigSlot.Engine => Engine,
            RigSlot.Detector => Detector,
            RigSlot.Refinery => Refinery,
            _ => 0f,
        };

        /// <summary>그 칸에 증폭률을 더한다(상자를 깔 때마다 계속 쌓인다 — 누적 상한 없음, 클래스
        /// 위 주석 참고). 음수는 무시한다 — 증폭기는 깎는 경우가 없다(amplifier.md).</summary>
        public void Add(RigSlot slot, float bonus)
        {
            if (bonus <= 0f) return;
            switch (slot)
            {
                case RigSlot.Tool: Tool += bonus; break;
                case RigSlot.Cargo: Cargo += bonus; break;
                case RigSlot.Engine: Engine += bonus; break;
                case RigSlot.Detector: Detector += bonus; break;
                case RigSlot.Refinery: Refinery += bonus; break;
            }
        }
    }

    /// <summary>P-05(레이싱카 쪽): PartSlot 중 amplifier.md가 명시한 다섯 칸(Engine/Tire/
    /// Suspension/Body/Booster)에 쌓인 증폭률 합. PartSlot을 그대로 키로 쓰지만 Module은
    /// amplifier.md "무엇에 붙나"에 없는 칸이라 필드 자체가 없다 — Bonus(Module)은 항상 0,
    /// Add(Module, ...)은 조용히 무시한다(RigAmplifierSave의 알 수 없는 슬롯 처리와 같은 패턴).
    /// RacingCar.TotalStats(PartAmplifierSave)가 이 값을 슬롯별로 그 칸 Part의 Effective()에
    /// 곱한다.</summary>
    [Serializable]
    public sealed class PartAmplifierSave
    {
        public float Engine;
        public float Tire;
        public float Suspension;
        public float Body;
        public float Booster;

        public float Bonus(PartSlot slot) => slot switch
        {
            PartSlot.Engine => Engine,
            PartSlot.Tire => Tire,
            PartSlot.Suspension => Suspension,
            PartSlot.Body => Body,
            PartSlot.Booster => Booster,
            _ => 0f, // Module: amplifier.md 설계 밖
        };

        /// <summary>그 칸에 증폭률을 더한다. 0 이하는 무시(RigAmplifierSave.Add와 같은 규칙),
        /// Module 슬롯도 조용히 무시한다(위 클래스 주석 참고).</summary>
        public void Add(PartSlot slot, float bonus)
        {
            if (bonus <= 0f) return;
            switch (slot)
            {
                case PartSlot.Engine: Engine += bonus; break;
                case PartSlot.Tire: Tire += bonus; break;
                case PartSlot.Suspension: Suspension += bonus; break;
                case PartSlot.Body: Body += bonus; break;
                case PartSlot.Booster: Booster += bonus; break;
                // Module: 무시
            }
        }
    }

    /// <summary>P-14 첫 조각: 등급별(PetGrade enum 순서, 7칸 고정 — EquippedPartIds처럼 항상
    /// 이 길이를 지킨다) "가진 종 수"·조각 수 + 뽑기 4종의 천장·하루 한도. PetGachaTable.Open의
    /// openedSincePity, PetCollection.CollectionBonus의 ownedSpeciesCountByGrade, PetFusion의
    /// 조각 계산에 그대로 넘길 수 있는 형태다.</summary>
    [Serializable]
    public sealed class PetGachaSave
    {
        public List<int> OwnedSpeciesCountByGrade = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        public List<int> ShardsByGrade = new List<int> { 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>P-14 ② 연결: 종 124개 전부의 보유 여부(PetSpeciesTable.All 순서, 인덱스 = 종 id).
        /// 새 세이브는 전부 false로 시작해 마이그레이션이 필요 없다(클래스 상단 규칙 그대로) —
        /// JSON에 이 필드가 없으면 이 초기화 값이 그대로 남는다. OwnedSpeciesCountByGrade는 이제
        /// 이 리스트에서도 파생될 수 있다 — MarkSpeciesOwned가 둘 다 같이 갱신한다.</summary>
        public List<bool> OwnedSpeciesIds = new List<bool>(new bool[PetSpeciesTable.All.Count]);

        // 고급/특수만 천장이 있다(pet-gacha.md 3절) — 무료·일반은 카운터 자체가 필요 없다.
        public int AdvancedOpenedSincePity;
        public int SpecialOpenedSincePity;

        // 하루 한도 — RewardAdState와 같은 패턴(자정이 아니라 DayIndex 하나로 리셋 판정).
        public long DailyResetDayIndex;
        public int FreePullsToday;                 // 무료 뽑기(광고), 하루 10회
        public bool AdvancedFreePullClaimedToday;   // 고급 뽑기 하루 1개 무료

        // 특수 뽑기 입장권. TitaniumBoxCount(상자 자체)와는 다른 재화다.
        public int TranscendentSealCount;

        /// <summary>인장을 더한다. 0 이하는 무시(AddShards와 같은 규칙). pet-gacha.md 3절의 네 경로
        /// (티타늄 상자 희귀 드롭 / 행성 클리어 보상 5장 / 시즌 패스 무료 트랙 주 2장 / 유료 구매)가
        /// 전부 이 메서드 하나로 들어온다 — 어디서 왔는지는 이 클래스가 구분하지 않는다.</summary>
        public void AddSeal(int amount)
        {
            if (amount <= 0) return;
            TranscendentSealCount += amount;
        }

        public int OwnedSpeciesCount(PetGrade grade) => OwnedSpeciesCountByGrade[(int)grade];
        public int Shards(PetGrade grade) => ShardsByGrade[(int)grade];

        /// <summary>새 종 하나를 도감에 채운다(중복이 아니라 처음 얻은 종). 등급의 최대 종 수를
        /// 이미 채웠으면 조용히 무시한다 — PetCollection.CollectionBonus가 이 리스트를 그대로
        /// 받으면 초과값에 예외를 던지므로 저장 시점에 미리 막는다.</summary>
        public void AddOwnedSpecies(PetGrade grade)
        {
            var idx = (int)grade;
            if (OwnedSpeciesCountByGrade[idx] < PetGradeInfo.SpeciesCountFor(grade))
                OwnedSpeciesCountByGrade[idx]++;
        }

        public bool OwnsSpecies(int speciesId) =>
            speciesId >= 0 && speciesId < OwnedSpeciesIds.Count && OwnedSpeciesIds[speciesId];

        /// <summary>P-14 ② 연결: 종 하나를 뽑았을 때 호출한다. 처음 얻은 종이면 도감(OwnedSpeciesIds)과
        /// 등급별 카운트(OwnedSpeciesCountByGrade)를 같이 채우고 true를 돌려준다 — 한 등급의 종은
        /// 전부 서로 다른 id라 AddOwnedSpecies와 달리 별도 상한 클램프가 필요 없다(그 등급 종 수를
        /// 넘게 부를 수가 없다). 이미 가진 종(중복)이면 도감은 그대로 두고 false를 돌려준다 —
        /// 호출하는 쪽(PetGachaController)이 false를 보면 AddShards로 조각을 대신 지급해야 한다
        /// (pet-gacha.md 2절 "중복 → 조각").</summary>
        public bool MarkSpeciesOwned(int speciesId)
        {
            if (OwnsSpecies(speciesId)) return false;
            OwnedSpeciesIds[speciesId] = true;
            OwnedSpeciesCountByGrade[(int)PetSpeciesTable.Get(speciesId).Grade]++;
            return true;
        }

        /// <summary>조각을 더한다. 0 이하는 무시(RigAmplifierSave.Add와 같은 규칙).</summary>
        public void AddShards(PetGrade grade, int amount)
        {
            if (amount <= 0) return;
            ShardsByGrade[(int)grade] += amount;
        }

        /// <summary>PetFusion.ExchangeForSameGrade/ExchangeForPromotion이 돌려준 나머지 조각
        /// 수를 되돌려 쓸 때 쓴다.</summary>
        public void SetShards(PetGrade grade, int amount)
        {
            if (amount < 0) throw new ArgumentException("조각 수는 음수일 수 없다.");
            ShardsByGrade[(int)grade] = amount;
        }

        /// <summary>고급/특수 뽑기 결과 하나를 반영해 천장 카운터를 갱신한다(Guaranteed면 0으로
        /// 리셋, 아니면 1 증가) — PetGachaTable.Open을 부른 다음 그 결과를 여기 넘기면 된다.</summary>
        public void RecordAdvancedPull(bool guaranteed) =>
            AdvancedOpenedSincePity = guaranteed ? 0 : AdvancedOpenedSincePity + 1;

        public void RecordSpecialPull(bool guaranteed) =>
            SpecialOpenedSincePity = guaranteed ? 0 : SpecialOpenedSincePity + 1;

        /// <summary>날짜가 바뀌었으면 무료 뽑기 하루 카운트·고급 무료분을 초기화한다.
        /// RewardAdTracker.DayIndex와 같은 하루 경계 계산을 그대로 재사용한다(중복 정의 없음).</summary>
        public void ResetDailyIfNewDay(long nowUnixSeconds, long timeZoneOffsetSeconds)
        {
            var today = RewardAdTracker.DayIndex(nowUnixSeconds, timeZoneOffsetSeconds);
            if (DailyResetDayIndex == today) return;
            DailyResetDayIndex = today;
            FreePullsToday = 0;
            AdvancedFreePullClaimedToday = false;
        }

        public bool CanPullFree() => FreePullsToday < PetGachaTable.FreePullDailyLimit;

        /// <summary>무료 뽑기를 실제로 돌린 뒤(콜백에서) 부른다. 이미 오늘 한도를 다 썼으면
        /// 카운트를 안 올린다 — RewardAdTracker.RecordWatch와 같은 방어.</summary>
        public void RecordFreePull()
        {
            if (CanPullFree()) FreePullsToday++;
        }
    }
}
