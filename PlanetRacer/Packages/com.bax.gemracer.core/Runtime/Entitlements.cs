using System;

namespace GemRacer.Core
{
    /// <summary>M-06: 지금까지 산/구독한 것의 원 데이터. 영구 구매는 레벨·bool로, 기간제는
    /// 만료 시각(UTC epoch초)로 둔다 — 시간은 인자로만 받는다(CLAUDE.md 1번). "지금 몇 시인지"는
    /// Assets 글루 레이어가 DateTimeOffset.UtcNow로 구해서 Entitlements.Effective에 넘긴다.
    /// 이 구조체 자체는 영수증 검증 결과를 그대로 받아 앉히는 자리라 계산이 없다 — 계산은
    /// 전부 Entitlements.Effective 한 곳에서만 한다(docs/design/monetization.md 6장:
    /// "구독과 개별 구매의 중복... 코어 한 곳에서만 계산하고 UI는 그 결과만 읽게 한다").</summary>
    public struct PurchaseState
    {
        /// <summary>화물칸 확장(영구, 누적) 단계. 0(안 삼)~3. monetization.md 2-2.</summary>
        public int CargoExpansionLevel;

        /// <summary>오프라인 상한 연장(영구). monetization.md 2-3 — 4시간 인정 → 12시간 인정.</summary>
        public bool OfflineCapExtensionPurchased;

        /// <summary>채굴 가속 패스(30일, ×2 산출). 산 적 없으면 null. monetization.md 2-4.</summary>
        public long? MiningAccelPassExpiryUnixSeconds;

        /// <summary>행성 통행증 구독(월). 구독 아니면 null. monetization.md 2-5.</summary>
        public long? SeasonPassSubscriptionExpiryUnixSeconds;

        /// <summary>Steam 서포터 팩(영구, 일회성) — 통행증 혜택을 영구로 준다. monetization.md 4장.
        /// 만료가 없다는 점만 빼면 구독과 같은 혜택이라 Effective에서 구독과 같은 취급을 한다.</summary>
        public bool SteamSupporterPackPurchased;

        /// <summary>광고 제거(영구, 모바일에서만 판매). monetization.md 2-8. 구독에도 포함돼
        /// 있어서(2-5) Effective에서는 이 값과 구독 여부를 OR로 합친다 — 따로 산 적 없어도
        /// 구독 중이면 광고가 사라진다.</summary>
        public bool AdRemovalPurchased;
    }

    /// <summary>실제로 지금 적용해야 할 값. UI·다른 코어 계산은 전부 이것만 읽는다 — PurchaseState를
    /// 직접 들여다보고 구독/영구 구매를 각자 따로 판단하면 "중복 차감"이나 "구독 만료 후에도 계속
    /// 적용" 같은 실수가 생기기 쉽다(monetization.md 6장이 경고하는 바로 그 문제).</summary>
    public struct Entitlements
    {
        /// <summary>화물칸 상한(MiningSimulator.CargoHours 결과)에 곱할 배율. 기본 1.</summary>
        public float CargoMultiplier;

        /// <summary>오프라인 캐치업이 인정하는 최대 시간. 화물칸 상한(행성마다 다름, M-01)과는
        /// 별개의 하한 보장치다 — 예를 들어 쿼츠(기본 4h)는 이 값이 그대로 오프라인 상한이 되지만,
        /// 화물칸 확장으로 이미 4h보다 큰 행성에서는 이 값이 더 작아도 화물칸 쪽 상한이 이긴다.
        /// 실제 적용은 소비하는 쪽이 Math.Max(화물칸 상한, 이 값)로 합친다(TODO — MiningController
        /// 배선은 아직 안 됨).</summary>
        public float OfflineCapHours;

        /// <summary>자동 제련소가 상시로 켜진 것처럼 취급(구독 혜택). monetization.md 2-5.</summary>
        public bool AutoRefineryAlwaysOn;

        /// <summary>광고 전부 안 뜸(구매 또는 구독). monetization.md 2-8, 2-5.</summary>
        public bool AdsRemoved;

        /// <summary>대전권(레이스 연료) 최대치에 더할 보너스. 구독 중에만 +2. monetization.md 2-5.
        /// RaceFuel.MaxFuel에 더해서 쓴다(TODO — MiningController 배선은 아직 안 됨).</summary>
        public int BonusFuelCapacity;

        /// <summary>방치 채굴 산출(MineralsPerHour)에 곱할 배율. 가속 패스만 올린다 — 캘 수 있는
        /// 등급은 안 바뀐다(monetization.md 2-4 "많이 캐는 것과 좋은 걸 캐는 것은 다르다").</summary>
        public float MiningYieldMultiplier;

        /// <summary>매일 정제 광물 지급(구독 혜택, monetization.md 2-5) 대상인지만 알려 준다 —
        /// "하루에 한 번만"이라는 청구 타이밍은 SaveData에 마지막 지급 날짜를 남겨 별도로 관리해야
        /// 하는 상태값이라 여기서는 계산하지 않는다(TODO, 순수 함수인 이 계산에는 안 맞음).</summary>
        public bool DailyRefinedMineralsGrant;

        // 화물칸 확장 단계별 배율. monetization.md 2-2 — 1단계 ×1.5, 2단계 ×2, 3단계 ×3.
        static readonly float[] CargoExpansionMultiplier = { 1f, 1.5f, 2f, 3f };

        const float SubscriptionCargoMultiplier = 1.5f; // monetization.md 2-5 "화물칸 +50%"
        const float BaseOfflineCapHours = 4f;
        const float ExtendedOfflineCapHours = 12f; // monetization.md 2-3 "4시간 → 12시간"
        const int SubscriptionBonusFuelCapacity = 2; // monetization.md 2-5 "대전권 +2"
        const float MiningAccelPassMultiplier = 2f; // monetization.md 2-4 "산출 ×2"

        /// <summary>지금(nowUnixSeconds) 시점에 실제로 적용해야 할 값을 계산한다. 기간제 항목은
        /// 만료 시각이 지금보다 미래여야 유효하다(경계값: 정확히 지금이면 만료된 것으로 본다 —
        /// RaceFuel.Recover 등 다른 코어 코드와 같은 "엄격히 이후" 관례).</summary>
        public static Entitlements Effective(PurchaseState state, long nowUnixSeconds)
        {
            bool subscriptionActive = state.SteamSupporterPackPurchased || IsActive(state.SeasonPassSubscriptionExpiryUnixSeconds, nowUnixSeconds);
            bool miningPassActive = IsActive(state.MiningAccelPassExpiryUnixSeconds, nowUnixSeconds);

            var level = state.CargoExpansionLevel;
            if (level < 0) level = 0;
            if (level >= CargoExpansionMultiplier.Length) level = CargoExpansionMultiplier.Length - 1;
            var permanentCargoMultiplier = CargoExpansionMultiplier[level];
            var subscriptionCargoMultiplier = subscriptionActive ? SubscriptionCargoMultiplier : 1f;

            return new Entitlements
            {
                // "구독과 영구 구매가 겹치면 더 큰 값 적용, 중복 차감 없음"(monetization.md 2-5) —
                // 곱하지 않고 Max를 쓴다. 곱하면 화물칸 확장 3단계(×3)를 산 구독자가 ×4.5를 받게
                // 되는데, 그건 "더 큰 값 적용"이 아니라 중복 적용이다.
                CargoMultiplier = Math.Max(permanentCargoMultiplier, subscriptionCargoMultiplier),
                OfflineCapHours = state.OfflineCapExtensionPurchased ? ExtendedOfflineCapHours : BaseOfflineCapHours,
                AutoRefineryAlwaysOn = subscriptionActive,
                AdsRemoved = state.AdRemovalPurchased || subscriptionActive,
                BonusFuelCapacity = subscriptionActive ? SubscriptionBonusFuelCapacity : 0,
                MiningYieldMultiplier = miningPassActive ? MiningAccelPassMultiplier : 1f,
                DailyRefinedMineralsGrant = subscriptionActive,
            };
        }

        static bool IsActive(long? expiryUnixSeconds, long nowUnixSeconds)
        {
            return expiryUnixSeconds.HasValue && expiryUnixSeconds.Value > nowUnixSeconds;
        }
    }
}
