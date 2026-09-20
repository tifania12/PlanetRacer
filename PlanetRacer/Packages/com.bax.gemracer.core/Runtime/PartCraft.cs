using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>D08-N: 레이싱카 부품 제작. 채굴차 업그레이드(RigUpgrade.cs)와 달리 레벨을 올리는
    /// 게 아니라 "정의(Part)를 만들어서 보유 목록에 추가"하는 방식이다 — 도감처럼 한 번 만들면
    /// 영구히 갖는다(강화는 D12에서 부품 위에 따로 붙는다, Part.Enhance 필드).</summary>
    public static class PartCraft
    {
        /// <summary>등급별 제작 비용(정제 광물 단일 자원). 2026-09-20 B등급 추가(P-07, DefaultData.
        /// QuartzAdvancedParts). A/S는 이 메서드로 못 낸다 — 단일 자원이 아니라 여러 행성 광물을
        /// 섞은 레시피이기 때문이다(아래 Recipe 참고). 부르면 여전히 예외.</summary>
        public static float Cost(PartGrade grade) => grade switch
        {
            PartGrade.C => DefaultData.PartCostC,
            PartGrade.B => DefaultData.PartCostB,
            _ => throw new NotSupportedException($"{grade} 등급은 단일 정제 광물 비용이 없다 — Recipe(grade)를 쓸 것."),
        };

        /// <summary>2026-09-21 P-07 후속: A/S 등급 제작 레시피(PlanetMineralRecipe로 검사·소비,
        /// PlanetMineralBank 기준 — 지금 캐는 중인 RawMinerals/RefinedMinerals가 아니라 행성별로
        /// 나눠 담아 둔 창고에서 깎인다). C/B는 Cost(grade)의 단일 자원 그대로라 여기선 예외 —
        /// Cost와 Recipe는 서로 배타적인 두 등급 구간을 나눠 맡는다.</summary>
        public static List<MineralCost> Recipe(PartGrade grade) => grade switch
        {
            PartGrade.A => DefaultData.QuartzEpicRecipe(),
            PartGrade.S => DefaultData.QuartzLegendaryRecipe(),
            _ => throw new NotSupportedException($"{grade} 등급은 혼합 레시피가 없다 — Cost(grade)를 쓸 것."),
        };

        /// <summary>이미 보유 중이면 다시 만들 수 없다(중복 제작 방지) — 도감 개념이라 똑같은
        /// 부품을 두 개 가질 이유가 없다. 광물이 모자란지는 여기서 안 본다 — 그건 Cost를 보고
        /// 호출하는 쪽(MiningController.TrySpendRefinedMinerals)이 판단한다, TryUpgrade와 같은 방식.</summary>
        public static bool CanCraft(List<string> ownedPartIds, Part part) => !ownedPartIds.Contains(part.Id);
    }

    /// <summary>부품 장착·해제. Part.Slot이 제작 시점에 고정돼 있어서 엉뚱한 슬롯에 끼울 수가
    /// 없다 — TryEquip은 항상 part.Slot 자리에 넣는다. 그래서 "같은 부품이 두 슬롯에 동시에
    /// 있는" 경우는 구조적으로 생기지 않는다(D08-M "중복 장착 방지"는 이 구조 위에서 회귀
    /// 테스트로 고정할 것).</summary>
    public static class PartEquip
    {
        /// <summary>보유하지 않은 부품은 장착할 수 없다. 그 슬롯에 이미 있던 부품은 자동으로
        /// 해제되어 사라진다(보유 목록에서 지워지진 않으니 다시 장착할 수 있다).</summary>
        public static bool TryEquip(RacingCar car, List<string> ownedPartIds, Part part)
        {
            if (!ownedPartIds.Contains(part.Id)) return false;
            car.Slots[part.Slot] = part;
            return true;
        }

        /// <summary>해당 슬롯을 비운다. 이미 비어 있어도 안전하다.</summary>
        public static void Unequip(RacingCar car, PartSlot slot) => car.Slots[slot] = null;
    }
}
