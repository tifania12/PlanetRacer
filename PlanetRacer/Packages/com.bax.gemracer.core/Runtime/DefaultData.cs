using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>
    /// 시트→ScriptableObject 파이프라인이 생기기 전까지 쓰는 기본 데이터.
    /// P0 관문용 초안 값. 밸런스는 P4 봇 시뮬레이션에서 다시 잡는다.
    /// </summary>
    public static class DefaultData
    {
        public static List<Planet> Planets() => new List<Planet>
        {
            new Planet { Id = "quartz",    NameKo = "쿼츠",        Order = 1, Circumference = 1000, VeinCount = 14, VeinYield = 20, BaseCargoHours = 4f },
            new Planet { Id = "ruby",      NameKo = "루비",        Order = 2, Circumference = 1300, Heat = 0.9f, Roughness = 0.6f, VeinCount = 12, VeinYield = 26, BaseCargoHours = 4f },
            new Planet { Id = "sapphire",  NameKo = "사파이어",    Order = 3, Circumference = 1500, Cold = 0.9f, Roughness = 0.4f, VeinCount = 12, VeinYield = 32, BaseCargoHours = 5f },
            new Planet { Id = "aquamarine",NameKo = "아쿠아마린",  Order = 4, Circumference = 1700, Liquid = 0.5f, VeinCount = 10, VeinYield = 40, BaseCargoHours = 5f },
            new Planet { Id = "cinnabar",  NameKo = "주사",        Order = 5, Circumference = 1900, Toxic = 0.7f, Roughness = 0.7f, VeinCount = 10, VeinYield = 50, BaseCargoHours = 6f },
            new Planet { Id = "lapis",     NameKo = "라피스 라줄리",Order = 6, Circumference = 2200, Gravity = 0.25f, Atmosphere = 0.15f, VeinCount = 8, VeinYield = 64, BaseCargoHours = 6f },
        };

        /// <summary>쿼츠 행성의 로컬 레이스 3개. P1 프로토타입 범위. 전부 RaceTier.Local —
        /// 서킷·챌린지 코스는 아직 없다(docs/backlog.md W2에서 4등급 해금 구조와 함께 만들 예정).</summary>
        public static List<Course> QuartzCourses() => new List<Course>
        {
            new Course { Id = "quartz_local_1", NameKo = "석영 평원 스프린트", PlanetId = "quartz", Length = 600,  Laps = 1, FlatRatio = 0.7f, RoughRatio = 0.2f, BoostRatio = 0.1f, Tier = RaceTier.Local },
            new Course { Id = "quartz_local_2", NameKo = "결정 능선 루프",    PlanetId = "quartz", Length = 800,  Laps = 1, FlatRatio = 0.4f, RoughRatio = 0.5f, BoostRatio = 0.1f, Tier = RaceTier.Local },
            new Course { Id = "quartz_local_3", NameKo = "반사면 직선로",     PlanetId = "quartz", Length = 500,  Laps = 2, FlatRatio = 0.5f, RoughRatio = 0.1f, BoostRatio = 0.4f, Tier = RaceTier.Local },
        };

        /// <summary>C등급 기본 부품 한 세트(쿼츠). 제작 비용은 정제 광물 단위.</summary>
        public static List<Part> QuartzStarterParts() => new List<Part>
        {
            new Part { Id = "q_engine_c", NameKo = "석영 엔진",     Slot = PartSlot.Engine,     Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Power = 30 } },
            new Part { Id = "q_tire_c",   NameKo = "석영 타이어",   Slot = PartSlot.Tire,       Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Grip = 30 } },
            new Part { Id = "q_susp_c",   NameKo = "석영 서스펜션", Slot = PartSlot.Suspension, Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Suspension = 30 } },
            new Part { Id = "q_body_c",   NameKo = "석영 차체",     Slot = PartSlot.Body,       Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Durability = 30, Aero = 10 } },
            new Part { Id = "q_boost_c",  NameKo = "석영 부스터",   Slot = PartSlot.Booster,    Grade = PartGrade.C, PlanetId = "quartz", Base = new Stats { Boost = 30 } },
        };

        /// <summary>C등급 부품 제작 비용(정제 광물). 첫 부품까지 5분 목표에 맞춘 값.</summary>
        public const float PartCostC = 15f;

        /// <summary>쿼츠 행성 탐험 중 발견되는 보물 종류. 등급이 오를수록 요구 도구 레벨도 오른다
        /// (0=곡괭이 티어, 10=드릴 티어, 20=레이저 티어 — MiningSimulator의 티어 경계와 맞춘다).
        /// 구체 수치는 플레이스홀더, P4 봇 시뮬레이션에서 재조정한다.</summary>
        public static List<TreasureDef> QuartzTreasureDefs() => new List<TreasureDef>
        {
            new TreasureDef { Id = "q_treasure_c", NameKo = "석영 원석 주머니", Grade = TreasureGrade.C, RequiredToolLevel = 1,  MineralValue = 8f },
            new TreasureDef { Id = "q_treasure_b", NameKo = "결정 상자",       Grade = TreasureGrade.B, RequiredToolLevel = 11, MineralValue = 20f },
            new TreasureDef { Id = "q_treasure_a", NameKo = "봉인된 광맥",     Grade = TreasureGrade.A, RequiredToolLevel = 21, MineralValue = 45f },
            new TreasureDef { Id = "q_treasure_s", NameKo = "쿼츠의 심장",     Grade = TreasureGrade.S, RequiredToolLevel = 26, MineralValue = 90f },
        };

        /// <summary>쿼츠 로컬 레이스 3개의 우승 보상 — 광물이 아니라 채굴차 부품 슬롯을 올린다
        /// (코어 루프가 상호 강화 나선이 되는 지점, docs/design/core-loop.md). 로컬 레이스라 슬롯을
        /// 하나씩 돌아가며 준다 — 어느 코스를 먼저 이겨도 어느 한 축만 커지지 않게.</summary>
        public static List<RigPartReward> QuartzLocalRaceRewards() => new List<RigPartReward>
        {
            new RigPartReward { Id = "q_reward_tool",   NameKo = "석영 곡괭이날",   Slot = RigSlot.Tool,   LevelBonus = 1, CourseId = "quartz_local_1" },
            new RigPartReward { Id = "q_reward_cargo",  NameKo = "석영 화물칸 확장", Slot = RigSlot.Cargo,  LevelBonus = 1, CourseId = "quartz_local_2" },
            new RigPartReward { Id = "q_reward_engine", NameKo = "석영 엔진 부스터", Slot = RigSlot.Engine, LevelBonus = 1, CourseId = "quartz_local_3" },
        };

        /// <summary>M-07: 상점 가격표. docs/design/balance/shop.csv와 값이 같아야 한다(Core.Tests가
        /// 검사, 다른 밸런스 표와 같은 관례) — 가격을 CSV에서만 조정하고 여기를 깜빡 안 고치면
        /// 그 테스트가 걸린다.</summary>
        public static List<ShopItem> ShopItems() => new List<ShopItem>
        {
            new ShopItem { SkuId = ShopSkuId.StarterPack,             NameKo = "스타터 팩",              PriceKrw = 1100 },
            new ShopItem { SkuId = ShopSkuId.CargoExpansion1,         NameKo = "화물칸 확장 1단계",       PriceKrw = 3300 },
            new ShopItem { SkuId = ShopSkuId.CargoExpansion2,         NameKo = "화물칸 확장 2단계",       PriceKrw = 6600 },
            new ShopItem { SkuId = ShopSkuId.CargoExpansion3,         NameKo = "화물칸 확장 3단계",       PriceKrw = 12000 },
            new ShopItem { SkuId = ShopSkuId.OfflineCapExtension,     NameKo = "오프라인 상한 연장",      PriceKrw = 5500 },
            new ShopItem { SkuId = ShopSkuId.MiningAccelPass,         NameKo = "채굴 가속 패스(30일)",    PriceKrw = 9900 },
            new ShopItem { SkuId = ShopSkuId.SeasonPassSubscription,  NameKo = "행성 통행증 구독(월)",    PriceKrw = 9900 },
            new ShopItem { SkuId = ShopSkuId.SteamSupporterPack,      NameKo = "Steam 서포터 팩",        PriceKrw = 29000 },
            new ShopItem { SkuId = ShopSkuId.AdRemoval,               NameKo = "광고 제거",              PriceKrw = 5500 },
        };

        /// <summary>M-10: 시즌 패스 10티어 초안(monetization.md 2-6). 레벨당 필요 XP는 우선 등차
        /// (레벨×100)로 잡았다 — 4주 시즌·정확한 XP 획득량은 아직 안 정해서(레이스 승리당 XP 같은
        /// 값이 필요, P4 봇 시뮬레이션에서 재조정) 구조만 먼저 세운다. 무료 트랙엔 채굴차 부품·상자·
        /// 소량 원석만(힘), 유료 트랙엔 정제 광물·화물칸 임시 확장·스킨만(시간 단축·꾸미기) —
        /// monetization.md "절대 팔지 않는 것"을 그대로 지켰다.</summary>
        public static SeasonPassTier[] SeasonPassTiers() => new[]
        {
            new SeasonPassTier
            {
                Level = 1, RequiredXp = 100,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RawMinerals, Amount = 5f },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RefinedMinerals, Amount = 5f },
            },
            new SeasonPassTier
            {
                Level = 2, RequiredXp = 200,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RigPart, RigSlot = RigSlot.Tool },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.Cosmetic, CosmeticId = "sp_skin_1" },
            },
            new SeasonPassTier
            {
                Level = 3, RequiredXp = 300,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.LootBox, LootBox = LootBoxType.Rusty },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RefinedMinerals, Amount = 8f },
            },
            new SeasonPassTier
            {
                Level = 4, RequiredXp = 400,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RigPart, RigSlot = RigSlot.Cargo },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.CargoCapBoostHours, Amount = 2f },
            },
            new SeasonPassTier
            {
                Level = 5, RequiredXp = 500,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RawMinerals, Amount = 10f },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RefinedMinerals, Amount = 10f },
            },
            new SeasonPassTier
            {
                Level = 6, RequiredXp = 600,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RigPart, RigSlot = RigSlot.Engine },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.Cosmetic, CosmeticId = "sp_skin_2" },
            },
            new SeasonPassTier
            {
                Level = 7, RequiredXp = 700,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.LootBox, LootBox = LootBoxType.Rusty },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RefinedMinerals, Amount = 12f },
            },
            new SeasonPassTier
            {
                Level = 8, RequiredXp = 800,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RigPart, RigSlot = RigSlot.Detector },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.CargoCapBoostHours, Amount = 3f },
            },
            new SeasonPassTier
            {
                Level = 9, RequiredXp = 900,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RawMinerals, Amount = 15f },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RefinedMinerals, Amount = 15f },
            },
            new SeasonPassTier
            {
                Level = 10, RequiredXp = 1000,
                FreeReward = new SeasonPassReward { Kind = SeasonPassRewardKind.RigPart, RigSlot = RigSlot.Refinery },
                PaidReward = new SeasonPassReward { Kind = SeasonPassRewardKind.Cosmetic, CosmeticId = "sp_skin_final" },
            },
        };
    }
}
