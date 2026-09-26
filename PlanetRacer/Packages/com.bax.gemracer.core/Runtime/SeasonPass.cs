using System;

namespace GemRacer.Core
{
    /// <summary>M-10: monetization.md 2-6 "시즌 패스" — 4주짜리 무료/유료 트랙 배틀패스.
    /// 레벨은 플레이(XP)로만 오르고, 레벨 자체를 돈으로 사지 않는다("레벨 구매 없음").
    ///
    /// 주의: 이 SeasonPass는 monetization.md 2-5 "행성 통행증(구독)"과는 다른 상품이다 — 그건
    /// 매달 자동 갱신되는 구독이고 이미 Entitlements.PurchaseState.SeasonPassSubscriptionExpiryUnixSeconds/
    /// ShopSkuId.SeasonPassSubscription으로 구현돼 있다. 기획 문서가 두 상품 모두 "시즌 패스"/
    /// "행성 통행증"이라는 이름을 먼저 썼기 때문에 이름이 겹친다 — 여기서는 타입 이름
    /// (SeasonPassTier/SeasonPassState/SeasonPassProgress)으로만 구분하고, 기존 필드 이름은
    /// 건드리지 않는다.</summary>
    public enum SeasonPassTrack { Free, Paid }

    /// <summary>보상 종류. 힘에 해당하는 것(RigPart/LootBox)은 무료 트랙에만 나온다 — 유료 트랙에
    /// 넣는 순간 monetization.md의 "절대 팔지 않는 것"(시즌 패스 유료 트랙의 전투력 보상)을 어긴다.</summary>
    public enum SeasonPassRewardKind
    {
        RigPart,            // 무료 — 채굴차 부품 슬롯 레벨(RigPartReward와 같은 개념)
        LootBox,            // 무료 — 공구 상자
        RawMinerals,        // 무료 — 소량 원석(빈 레벨 채움용)
        RefinedMinerals,    // 유료 — 정제 광물(시간 단축, 화물칸을 안 먹는다)
        CargoCapBoostHours, // 유료 — 화물칸 임시 확장(그 시간 동안 상한 2배, RewardAdBoost와 같은 성격)
        Cosmetic,           // 유료 — 스킨(스탯 0). 꾸미기 카탈로그가 아직 없어 문자열 id만 들고 있다
    }

    /// <summary>티어 하나의 보상 하나. Kind에 따라 나머지 필드 중 하나만 의미를 가진다 — 정적
    /// 테이블(DefaultData.SeasonPassTiers)에만 쓰여서 세이브 직렬화 대상이 아니므로 유니온 대신
    /// 필드를 그냥 넉넉히 둔다.</summary>
    public struct SeasonPassReward
    {
        /// <summary>RawMinerals/RefinedMinerals 수량, CargoCapBoostHours는 시간(시간 단위).</summary>
        public float Amount;
        public SeasonPassRewardKind Kind;
        public LootBoxType LootBox; // Kind == LootBox일 때만
        public RigSlot RigSlot;     // Kind == RigPart일 때만
        public string CosmeticId;   // Kind == Cosmetic일 때만
    }

    /// <summary>티어 하나 = 레벨 하나. RequiredXp는 그 레벨에 도달하는 데 필요한 누적 XP다
    /// (레벨 N 도달 ⇔ CurrentXp >= Tiers[N-1].RequiredXp). FreeReward/PaidReward가 둘 다 null이면
    /// 그 레벨엔 받을 게 없다는 뜻 — 매 레벨을 꽉 채울 필요는 없다.</summary>
    public sealed class SeasonPassTier
    {
        public int Level;
        public int RequiredXp;
        public SeasonPassReward? FreeReward;
        public SeasonPassReward? PaidReward;
    }

    /// <summary>플레이어 진행 상태. 세이브에 그대로 저장한다(SaveData.ToSeasonPassState/
    /// ApplySeasonPassState). ClaimedFreeTierMask/ClaimedPaidTierMask는 비트 하나가 레벨 하나
    /// (레벨 1 = bit 0) — 시즌 하나가 SeasonPassProgress.MaxTiers(63)레벨을 넘을 일은 없다고 보고
    /// long 하나로 충분하다.</summary>
    public struct SeasonPassState
    {
        public int CurrentXp;
        public bool OwnsPaidTrack;
        public long ClaimedFreeTierMask;
        public long ClaimedPaidTierMask;
    }

    /// <summary>레벨 계산 + 보상 수령 판정/적용. 전부 순수 함수 — 시간이 필요 없는 계산이라
    /// 다른 코어 파일과 달리 인자로 받을 시각이 없다(CLAUDE.md 1번은 "쓸 때 인자로 받으라"는
    /// 뜻이지 안 쓰는데 억지로 받으라는 뜻이 아니다).</summary>
    public static class SeasonPassProgress
    {
        public const int MaxTiers = 63;

        /// <summary>지금 XP로 도달한 최고 레벨(0 = 아직 1레벨도 못 참). tiers는 Level 오름차순이라고
        /// 가정한다(DefaultData.SeasonPassTiers가 그렇게 만든다) — 마지막 티어보다 XP가 많아도
        /// 그냥 마지막 레벨에 머문다(상한 없이 계속 오르지 않는다).</summary>
        public static int LevelForXp(SeasonPassTier[] tiers, int currentXp)
        {
            var level = 0;
            foreach (var tier in tiers)
            {
                if (currentXp < tier.RequiredXp) break;
                level = tier.Level;
            }
            return level;
        }

        static long BitFor(int level)
        {
            if (level < 1 || level > MaxTiers)
                throw new ArgumentOutOfRangeException(nameof(level), level, $"시즌 패스 레벨은 1~{MaxTiers}만 지원한다.");
            return 1L << (level - 1);
        }

        public static bool IsClaimed(SeasonPassState state, SeasonPassTrack track, int level)
        {
            var bit = BitFor(level);
            return track == SeasonPassTrack.Free
                ? (state.ClaimedFreeTierMask & bit) != 0
                : (state.ClaimedPaidTierMask & bit) != 0;
        }

        /// <summary>지금 받을 수 있는지 — 레벨에 도달했고, 유료 트랙이면 보유 중이고, 아직 안 받았고,
        /// 그 레벨에 그 트랙 보상이 실제로 있을 때만 true.</summary>
        public static bool CanClaim(SeasonPassTier[] tiers, SeasonPassState state, SeasonPassTrack track, int level)
        {
            if (level < 1 || level > tiers.Length) return false;
            if (LevelForXp(tiers, state.CurrentXp) < level) return false;
            if (track == SeasonPassTrack.Paid && !state.OwnsPaidTrack) return false;
            if (IsClaimed(state, track, level)) return false;

            var tier = tiers[level - 1];
            var reward = track == SeasonPassTrack.Free ? tier.FreeReward : tier.PaidReward;
            return reward.HasValue;
        }

        /// <summary>보상을 받는다. CanClaim이 false면 상태를 그대로 돌려주고 reward는 default다 —
        /// 화면은 항상 CanClaim으로 먼저 버튼 활성화 여부를 정하고, 이 함수는 그 판정을 방어적으로
        /// 한 번 더 검사한다(ShopPurchase.Apply 등 다른 코어 함수와 같은 이중 확인 패턴).</summary>
        public static SeasonPassState Claim(SeasonPassTier[] tiers, SeasonPassState state, SeasonPassTrack track, int level, out SeasonPassReward reward)
        {
            reward = default;
            if (!CanClaim(tiers, state, track, level)) return state;

            var tier = tiers[level - 1];
            reward = (track == SeasonPassTrack.Free ? tier.FreeReward : tier.PaidReward)!.Value;

            var bit = BitFor(level);
            if (track == SeasonPassTrack.Free) state.ClaimedFreeTierMask |= bit;
            else state.ClaimedPaidTierMask |= bit;
            return state;
        }

        /// <summary>XP를 더한다. 상한은 안 둔다 — 마지막 티어를 넘는 XP는 그냥 쌓이기만 하고
        /// LevelForXp가 마지막 레벨로 클램프하니 문제없다.</summary>
        public static SeasonPassState AddXp(SeasonPassState state, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "XP는 음수로 줄 수 없다.");
            state.CurrentXp += amount;
            return state;
        }

        /// <summary>M-14: 유료 트랙을 산다(ShopSkuId.SeasonPassPaidTrack). 이미 보유 중이면 그대로
        /// 둔다 — ShopPurchase.Apply의 Math.Max 패턴과 같은 이유로, 중복 구매를 눌러도 손해가
        /// 없어야 한다. `ShopSkuId.SeasonPassSubscription`(매달 자동 갱신)과는 다른 상품이라
        /// PurchaseState가 아니라 이 SeasonPassState를 바꾼다 — 그래서 ShopPurchase.Apply의
        /// switch가 아니라 여기, SeasonPassProgress 쪽에 둔다(MiningController.DebugPurchase가
        /// skuId로 이 함수와 ShopPurchase.Apply 중 하나를 고른다).</summary>
        public static SeasonPassState PurchasePaidTrack(SeasonPassState state)
        {
            state.OwnsPaidTrack = true;
            return state;
        }
    }
}
