using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>P-07(행성별 광물 종류, planet-progression.md 2절 "캘 수 있는 자원이 다르다"):
    /// 정제 광물을 행성별로 나눠 보관하는 창고. 지금 채굴 중인 SaveData.RawMinerals/RefinedMinerals와는
    /// 다른 값이다 — 저건 "지금 캐는 행성에서 진행 중인" 값이고, 이 창고는 상위 행성 부품을 만들 때
    /// 이전 행성 광물을 다시 꺼내 쓸 수 있도록 행성별로 나눠 보관하는 자리다(그래서 이전 행성으로
    /// 돌아갈 이유가 생긴다). 정제 광물을 이 창고로 언제 옮겨 담을지(레이스 우승 시점? 행성을
    /// 떠날 때?)는 아직 안 정해졌다 — 이 파일은 창고 자체를 다루는 순수 함수만 제공한다.
    /// SaveData가 JsonUtility 직렬화라 Dictionary를 못 써서(SaveData.cs 상단 주석과 같은 이유)
    /// planetId·보유량을 병렬 리스트로 들고 다닌다(RaceRecordBook.cs와 같은 패턴) — 리스트 자체는
    /// SaveData.PlanetMineralIds/PlanetMineralAmounts를 그대로 넘기면 된다.</summary>
    public static class PlanetMineralBank
    {
        /// <summary>그 행성 창고의 보유량. 한 번도 담긴 적 없으면 0.</summary>
        public static float Amount(List<string> planetIds, List<float> amounts, string planetId)
        {
            var idx = planetIds.IndexOf(planetId);
            return idx < 0 ? 0f : amounts[idx];
        }

        /// <summary>그 행성 창고에 더한다. 0 이하는 무시한다(RigAmplifierSave.Add와 같은 규칙).
        /// 처음 담기는 planetId면 새 칸을 만든다.</summary>
        public static void Add(List<string> planetIds, List<float> amounts, string planetId, float amount)
        {
            if (amount <= 0f) return;
            var idx = planetIds.IndexOf(planetId);
            if (idx < 0) { planetIds.Add(planetId); amounts.Add(amount); }
            else amounts[idx] += amount;
        }

        /// <summary>모자라면 아무것도 깎지 않고 false를 돌려준다(부분 차감 없음). 0 이하 요청도 실패.</summary>
        public static bool TrySpend(List<string> planetIds, List<float> amounts, string planetId, float amount)
        {
            if (amount <= 0f) return false;
            var idx = planetIds.IndexOf(planetId);
            if (idx < 0 || amounts[idx] < amount) return false;
            amounts[idx] -= amount;
            return true;
        }
    }

    /// <summary>여러 행성 광물을 섞어 요구하는 제작 비용 한 줄. 상위 부품이 "쿼츠 원석 10 + 루비 원석
    /// 20"처럼 여러 행성을 섞어 요구할 수 있게 한다 — B/A/S 등급 부품 정의는 아직 없어서(DefaultData.cs엔
    /// C등급뿐) 실제 레시피를 채우는 건 다음 세션 몫이다. 이 구조는 그 레시피를 검사·소비하는
    /// 메커니즘만 미리 만들어 둔 것(CourseGenerator가 먼저 만들어지고 나중에 붙은 것과 같은 순서).</summary>
    public struct MineralCost
    {
        public string PlanetId;
        public float Amount;
    }

    public static class PlanetMineralRecipe
    {
        /// <summary>costs 전부를 지금 보유량으로 감당할 수 있는지만 본다(아무것도 깎지 않음).</summary>
        public static bool CanAfford(List<string> planetIds, List<float> amounts, IReadOnlyList<MineralCost> costs)
        {
            foreach (var cost in costs)
                if (PlanetMineralBank.Amount(planetIds, amounts, cost.PlanetId) < cost.Amount) return false;
            return true;
        }

        /// <summary>전부 감당 가능할 때만 실제로 깎는다(all-or-nothing) — 하나라도 모자라면
        /// 아무것도 깎지 않고 false.</summary>
        public static bool TrySpend(List<string> planetIds, List<float> amounts, IReadOnlyList<MineralCost> costs)
        {
            if (!CanAfford(planetIds, amounts, costs)) return false;
            foreach (var cost in costs)
                PlanetMineralBank.TrySpend(planetIds, amounts, cost.PlanetId, cost.Amount);
            return true;
        }
    }
}
