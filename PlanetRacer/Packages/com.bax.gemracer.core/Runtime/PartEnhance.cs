using System;

namespace GemRacer.Core
{
    /// <summary>D12-N: 레이싱카 부품 강화. Models.cs의 Part.Enhance(+0~+10)와 Part.Effective()
    /// (강화 1당 +6%)는 이미 있었다(D08-N 주석 "강화는 D12에서 부품 위에 따로 붙는다") — 여기서
    /// 비용 곡선과 적용만 채운다. GDD "슬롯 5 + 특수 모듈 1. 등급 C/B/A/S. 강화 +10 실패 없음,
    /// 비용 가파름"을 그대로 구현 — 실패 롤 자체가 없다(RigUpgrade.TryUpgrade와 같은 패턴: 비용만
    /// 내면 무조건 성공, 확률 개입 없음).</summary>
    public static class PartEnhance
    {
        public const int MaxLevel = 10;

        /// <summary>강화 1단계당 비용 배율. "가파름"을 등급별 제작 비용(PartCraft.Cost) 대비
        /// 배수로 표현했다 — +0→+1이 제작 비용의 절반 정도(첫 강화는 가볍게), +9→+10 근처는
        /// 제작 비용의 수십 배(마지막 단계는 묵직하게)가 되도록 잡은 값. 정확한 수치는 P4 봇
        /// 시뮬레이션에서 재조정할 플레이스홀더(RigUpgrade.Cost의 지수 성장과 같은 방식).</summary>
        const float BaseCostRatio = 0.5f;
        const float GrowthPerLevel = 1.9f;

        /// <summary>다음 강화 단계로 올리는 데 드는 정제 광물. 이미 +10이면 못 올린다는 뜻으로
        /// float.PositiveInfinity를 돌려준다(UI가 버튼을 비활성화하는 신호로 쓴다, UpgradeCost와
        /// 같은 규약).</summary>
        public static float Cost(Part part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            if (AtMax(part)) return float.PositiveInfinity;
            var baseCost = PartCraft.Cost(part.Grade);
            return baseCost * BaseCostRatio * MathF.Pow(GrowthPerLevel, part.Enhance);
        }

        public static bool AtMax(Part part) => part.Enhance >= MaxLevel;

        /// <summary>Enhance를 1 올린다. 실패 없음(GDD) — 비용은 호출하는 쪽(MiningController)이
        /// 이미 냈다고 가정한다. 이미 최대면 아무 일도 안 한다. Part는 참조 타입이라 원본 인스턴스를
        /// 그대로 고친다 — MiningRig처럼 복사본을 돌려주는 대신, Car.Slots가 들고 있는 같은
        /// 인스턴스를 바로 갱신해야 레이스 스탯 계산에 즉시 반영된다.</summary>
        public static void Apply(Part part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            if (AtMax(part)) return;
            part.Enhance++;
        }
    }
}
