using System;

namespace GemRacer.Core
{
    /// <summary>E-06 ②: 연구 탭이 그대로 찍을 문구. 화면 코드는 이 값을 TMP에 넣기만 하면 되고,
    /// 이름·효과·시간 표기 규칙은 여기서 테스트로 고정한다(Unity 없이 검증 가능하게).</summary>
    public static class ResearchLabels
    {
        public static string Name(ResearchKind kind) => kind switch
        {
            ResearchKind.OfflineStorage => "오프라인 저장고",
            ResearchKind.GemDetector => "젬 탐지기",
            ResearchKind.RefineCatalyst => "정제 촉매",
            ResearchKind.PrizeNegotiation => "상금 협상",
            ResearchKind.StoneAppraisal => "강화석 감정",
            _ => "",
        };

        /// <summary>현재 레벨이 주는 효과 한 줄. 예: "오프라인 상한 +3시간", "상금 +10%".</summary>
        public static string Effect(ResearchKind kind, int level)
        {
            var total = Research.Total(kind, level);
            return kind switch
            {
                ResearchKind.OfflineStorage => $"오프라인 상한 +{total:0.#}시간",
                ResearchKind.GemDetector => $"광맥당 젬 확률 +{total * 100.0:0.#}%p",
                ResearchKind.RefineCatalyst => $"제련 처리량 +{total * 100.0:0.#}%",
                ResearchKind.PrizeNegotiation => $"레이스 상금 +{total * 100.0:0.#}%",
                ResearchKind.StoneAppraisal => $"던전 강화석 +{total * 100.0:0.#}%",
                _ => "",
            };
        }

        /// <summary>초를 "10분", "1시간 20분", "8시간"으로. 1분 미만은 "1분 미만", 0 이하는 "0분".</summary>
        public static string Duration(double seconds)
        {
            if (seconds <= 0.0) return "0분";
            if (seconds < 60.0) return "1분 미만";
            var totalMin = (long)Math.Ceiling(seconds / 60.0);
            var h = totalMin / 60;
            var m = totalMin % 60;
            if (h == 0) return $"{m}분";
            return m == 0 ? $"{h}시간" : $"{h}시간 {m}분";
        }

        /// <summary>한 줄 요약. 최대 레벨이면 "Lv N/N (최대)", 아니면 "Lv N/M".</summary>
        public static string Level(ResearchKind kind, int level)
        {
            var max = Research.MaxLevel(kind);
            return level >= max ? $"Lv {max}/{max} (최대)" : $"Lv {Math.Max(0, level)}/{max}";
        }
    }
}
