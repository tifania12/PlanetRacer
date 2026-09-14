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

        public float RawMinerals;
        public float RefinedMinerals;

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
}
