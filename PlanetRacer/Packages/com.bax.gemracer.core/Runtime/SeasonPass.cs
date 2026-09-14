using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>M-10: 시즌 패스(monetization.md 2-6, ₩12,000/4주)의 레벨·보상 데이터 구조.
    /// M-06 `Entitlements.SeasonPassSubscriptionExpiryUnixSeconds`(2-5 "행성 통행증" 월간 구독)와
    /// 이름은 비슷하지만 다른 상품이다 — 2-5는 매달 자동 갱신되는 편의 구독이고, 이건 시즌(4주)
    /// 동안 레벨을 올려 보상을 받는 배틀패스형 트랙이다. 두 상품이 같은 SKU를 공유하지 않는다.
    /// 이번 세션은 "레벨은 플레이로만 오른다(레벨 구매 없음)"는 전제 아래 정의·검증·레벨 계산까지만
    /// 다룬다 — XP를 실제로 무엇이 얼마나 주는지, SaveData에 진행도·수령 여부를 어떻게 저장할지,
    /// 화면 배선은 아직이다(TODO, 다음 세션).</summary>
    public enum SeasonPassRewardKind
    {
        // 무료 트랙 전용 — monetization.md 2-6 "무료 트랙 — 부품, 청사진, 공구 상자.
        // 즉 힘에 해당하는 것은 전부 여기 있다".
        Part,
        Blueprint,
        ToolBox,

        // 유료 트랙 전용 — monetization.md 2-6 "유료 트랙 — 스킨 세트, 정제 광물, 화물칸 임시
        // 확장, 자동화 해금. 꾸미기와 시간 단축만". "유료 트랙에 부품을 넣으면 그 순간
        // '돈으로 강해질 수 없다'가 거짓말이 된다. 넣지 않는다" — 이 넷은 절대 전투력이 아니다.
        SkinSet,
        RefinedMinerals,
        CargoCapBoost,
        AutomationUnlock,
    }

    public static class SeasonPassRewardKindExtensions
    {
        /// <summary>이 보상 종류가 "힘"(장비·제작 재료)에 해당하는지. 유료 트랙 검증
        /// (SeasonPassCatalog.Validate)이 이걸로 "유료 트랙엔 힘을 안 판다"를 강제한다.</summary>
        public static bool IsPower(this SeasonPassRewardKind kind)
        {
            return kind == SeasonPassRewardKind.Part
                || kind == SeasonPassRewardKind.Blueprint
                || kind == SeasonPassRewardKind.ToolBox;
        }
    }

    /// <summary>레벨 한 칸에서 실제로 받는 것 하나. ItemId는 Part/ToolBox 등 구체적인 무엇인지를
    /// 가리키는 자리 표시자 문자열(카탈로그가 아직 없는 종류는 비워 둬도 된다 — RefinedMinerals처럼
    /// Amount만으로 뜻이 통하는 보상은 ItemId가 필요 없다), Amount는 개수 또는 수량 환산치.</summary>
    public struct SeasonPassReward
    {
        public SeasonPassRewardKind Kind;
        public string ItemId;
        public float Amount;
    }

    /// <summary>시즌 패스 레벨 한 칸. 무료 보상은 항상 있고, 유료 보상은 레벨에 따라 없을 수도
    /// 있다(nullable) — monetization.md에 "매 레벨마다 반드시 유료 보상"이라는 규정은 없다.</summary>
    public struct SeasonPassLevelDef
    {
        /// <summary>1부터 시작. 0은 쓰지 않는다(0레벨 = 아직 아무것도 안 받은 상태를 가리키는 값으로 남겨 둔다).</summary>
        public int Level;
        /// <summary>이 레벨에 도달하는 데 필요한 누적 경험치. 오름차순이어야 한다(Validate가 검사).</summary>
        public int RequiredXp;
        public SeasonPassReward FreeReward;
        public SeasonPassReward? PaidReward;
    }

    /// <summary>레벨 목록 검증 + XP→레벨 계산. 목록 자체(어떤 시즌에 무엇을 주는지)는
    /// DefaultData.QuartzSeasonPassLevels() 같은 곳에 있고, 이 클래스는 순수 계산만 한다.</summary>
    public static class SeasonPassCatalog
    {
        /// <summary>목록이 "레벨은 1부터 오름차순, RequiredXp도 오름차순(레벨 구매가 없으니 같은
        /// 값이 두 번 나오면 안 됨), 유료 보상엔 힘이 없다"를 전부 지키는지 검사한다. 위반 사항을
        /// 사람이 읽을 문장으로 모아서 돌려준다 — 비어 있으면 통과. 예외를 던지지 않는 이유는
        /// 에디터 임포트 도구(다음 세션 몫)가 잘못된 CSV를 사람이 고치기 쉬운 목록으로 보여주기
        /// 좋게 하기 위함(BalanceCsv 쪽 파싱 에러 취급과 같은 결).</summary>
        public static List<string> Validate(IReadOnlyList<SeasonPassLevelDef> levels)
        {
            var errors = new List<string>();
            if (levels == null || levels.Count == 0)
            {
                errors.Add("시즌 패스 레벨이 비어 있음");
                return errors;
            }

            var expectedLevel = 1;
            var prevXp = -1;
            foreach (var def in levels)
            {
                if (def.Level != expectedLevel)
                    errors.Add($"레벨 {def.Level}: {expectedLevel}이어야 함(1부터 빠짐없이 오름차순)");
                if (def.RequiredXp <= prevXp)
                    errors.Add($"레벨 {def.Level}: RequiredXp({def.RequiredXp})가 이전 레벨({prevXp}) 이하 — 레벨 구매가 없으니 반드시 더 커야 함");
                if (def.PaidReward.HasValue && def.PaidReward.Value.Kind.IsPower())
                    errors.Add($"레벨 {def.Level}: 유료 트랙에 힘({def.PaidReward.Value.Kind}) 보상 — monetization.md 2-6 위반");

                expectedLevel++;
                prevXp = def.RequiredXp;
            }
            return errors;
        }

        /// <summary>지금 누적 경험치로 도달한 레벨. levels가 오름차순(Validate 통과 전제)이라
        /// RequiredXp를 만족하는 가장 높은 레벨을 찾는다. 0레벨(아직 1레벨도 안 됨)도 가능.
        /// 시즌이 끝나기 전까지 상한은 없다 — 목록의 마지막 레벨을 넘는 XP는 그냥 그 레벨에 머문다
        /// (다음 시즌 목록이 갱신되기 전까지는 초과분이 버려짐, 시즌 패스 관례와 같음).</summary>
        public static int LevelForXp(IReadOnlyList<SeasonPassLevelDef> levels, int xp)
        {
            var reached = 0;
            foreach (var def in levels)
            {
                if (xp < def.RequiredXp) break;
                reached = def.Level;
            }
            return reached;
        }

        /// <summary>다음 레벨까지 남은 경험치. 이미 목록의 마지막 레벨이면 null(더 올릴 데가 없음 —
        /// 화면은 이걸로 "만렙" 표시를 한다).</summary>
        public static int? XpToNextLevel(IReadOnlyList<SeasonPassLevelDef> levels, int xp)
        {
            foreach (var def in levels)
            {
                if (xp < def.RequiredXp) return def.RequiredXp - xp;
            }
            return null;
        }

        /// <summary>level까지 도달했을 때 받을 수 있는 보상 전부(무료는 항상, 유료는
        /// ownsPaidTrack일 때만). 이미 받았는지(SaveData 진행도)는 여기서 모른다 — 호출부가
        /// 자기가 아는 "받은 레벨" 기록과 대조해서 걸러 내야 한다(TODO, 다음 세션).</summary>
        public static List<SeasonPassReward> RewardsUpToLevel(IReadOnlyList<SeasonPassLevelDef> levels, int level, bool ownsPaidTrack)
        {
            var rewards = new List<SeasonPassReward>();
            foreach (var def in levels)
            {
                if (def.Level > level) break;
                rewards.Add(def.FreeReward);
                if (ownsPaidTrack && def.PaidReward.HasValue) rewards.Add(def.PaidReward.Value);
            }
            return rewards;
        }
    }
}
