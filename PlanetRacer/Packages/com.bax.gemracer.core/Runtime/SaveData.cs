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
