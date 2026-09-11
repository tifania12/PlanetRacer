using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>행성 ID → 보석 색상. 코어 DefaultData(Packages/com.bax.gemracer.core)의
    /// 6개 행성과 순서·ID를 맞춰서 쓴다. 실제 아트가 들어오기 전까지 임시 색상.</summary>
    public static class GemColors
    {
        /// <summary>DefaultData.Planets()와 같은 순서. 부트스트랩에서 머티리얼 6개를 만들 때 순회용.</summary>
        public static readonly string[] PlanetIdsInOrder =
        {
            "quartz", "ruby", "sapphire", "aquamarine", "cinnabar", "lapis"
        };

        public static Color For(string planetId)
        {
            switch (planetId)
            {
                case "quartz":     return new Color(0.90f, 0.90f, 0.94f); // 백수정 — 거의 흰색, 살짝 푸른 기
                case "ruby":       return new Color(0.75f, 0.06f, 0.16f); // 루비 레드
                case "sapphire":   return new Color(0.07f, 0.22f, 0.66f); // 사파이어 블루
                case "aquamarine": return new Color(0.35f, 0.85f, 0.80f); // 아쿠아마린 — 청록
                case "cinnabar":   return new Color(0.85f, 0.30f, 0.06f); // 주사 — 주홍(버밀리언)
                case "lapis":      return new Color(0.11f, 0.16f, 0.55f); // 라피스 라줄리 — 짙은 군청
                default:           return Color.gray;                    // 알 수 없는 행성 ID
            }
        }

        public static string NameKoFor(string planetId)
        {
            switch (planetId)
            {
                case "quartz":     return "쿼츠";
                case "ruby":       return "루비";
                case "sapphire":   return "사파이어";
                case "aquamarine": return "아쿠아마린";
                case "cinnabar":   return "주사";
                case "lapis":      return "라피스 라줄리";
                default:           return planetId;
            }
        }
    }
}
