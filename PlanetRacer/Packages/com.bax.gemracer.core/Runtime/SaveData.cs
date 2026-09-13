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
