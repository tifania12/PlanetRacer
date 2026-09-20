namespace GemRacer.Core
{
    /// <summary>
    /// P-14 ③: 뽑기 4종을 실제로 돌려 <see cref="SaveData.PetGacha"/>에 반영하는 컨트롤러.
    /// 등급(PetGachaTable)까지 확률로 정한 뒤, 그 등급 안에서 어느 종인지는
    /// <see cref="PetSpeciesTable.PickInGrade"/>가 정한다(P-14 ② 연결, 더 이상 등급까지만
    /// 반영하는 임시 형태가 아니다). 처음 얻은 종이면 도감(<see cref="PetGachaSave.MarkSpeciesOwned"/>)에
    /// 채우고, 이미 가진 종(중복)이면 대신 조각 1개를 지급한다(pet-gacha.md 2절 "중복 → 조각",
    /// 조각 3개 = 같은 등급 다른 펫 — PetFusion 참고). 등급 확률표가 쓰는 seed와 그 안에서 종을
    /// 고르는 seed는 서로 다르게 파생한다(RaceSimulator.cs의 `seed ^ 0x5bd1e995`와 같은 패턴) —
    /// 같은 seed를 그대로 재사용하면 두 추첨이 상관관계를 가질 수 있어서다.
    ///
    /// P-16: 고급/특수 뽑기(PullAdvanced/PullSpecial)도 여기 있다 — 소비 쪽(뽑기 하나 돌리고
    /// 세이브에 반영하는 것)은 전부 design(pet-gacha.md 3절)에 숫자가 이미 있어서 결정이 필요
    /// 없었다(1 인장 = 특수 뽑기 1회, 고급은 하루 1회 무료 + 나머지는 유료). **아직 없는 건
    /// 인장을 얻는 쪽 중 하나뿐이다** — "티타늄 상자 희귀 드롭"이 정확히 상자 하나당 몇 %인지는
    /// 그 상자를 주는 챌린지 레이스가 주당 몇 번 열리는지(W2, 아직 설계 전)를 모르면 못 정한다.
    /// 나머지 세 경로(행성 클리어 5장 / 시즌 패스 주 2장 / 유료 구매)는 전부 고정 수량이라
    /// `SaveData.PetGachaSave.AddSeal(amount)`를 그 값으로 부르기만 하면 된다 — 그 호출 자리들은
    /// 각자의 시스템(행성 클리어 판정 P-08, 시즌 패스 SeasonPass.cs, 상점 ShopPurchase.cs)이
    /// 아직 없거나 이 값을 안 불러서 다음 세션들 몫이다.
    ///
    /// 재화를 낼 수 있는지(광고 시청 여부, 뽑기 비용 지불)는 이 클래스의 책임이 아니다 —
    /// 호출하는 쪽이 먼저 확인하고 나서 Pull*을 부른다. 여기서 확인하는 건 "오늘 무료 뽑기
    /// 한도"뿐, 그것도 <see cref="PetGachaSave"/>가 이미 들고 있는 규칙을 그대로 따른다.
    /// </summary>
    public static class PetGachaController
    {
        /// <summary>Pull* 하나의 결과 — 등급/천장 여부(PetGachaTable.Open 그대로) + 실제로 어느
        /// 종인지 + 처음 얻은 종(도감에 새로 채워짐)인지 중복(조각으로 대신 지급됨)인지.</summary>
        public struct PetPullOutcome
        {
            public PetGrade Grade;
            public bool Guaranteed;
            public int SpeciesId;
            public bool IsNewSpecies;
        }

        /// <summary>등급까지 정해진 결과 하나를 받아 그 안에서 종을 고르고 세이브에 반영한다
        /// (신규면 도감, 중복이면 조각 1개). Pull* 메서드들이 공통으로 쓰는 자리.</summary>
        private static PetPullOutcome ResolveAndRecordSpecies(SaveData save, PetGachaResult result, int seed)
        {
            var speciesId = PetSpeciesTable.PickInGrade(result.Grade, seed ^ 0x5bd1e995).Id;
            var isNew = save.PetGacha.MarkSpeciesOwned(speciesId);
            if (!isNew) save.PetGacha.AddShards(result.Grade, 1);
            return new PetPullOutcome
            {
                Grade = result.Grade,
                Guaranteed = result.Guaranteed,
                SpeciesId = speciesId,
                IsNewSpecies = isNew,
            };
        }

        public struct FreePullOutcome
        {
            public PetPullOutcome Result;

            /// <summary>false면 오늘 무료 뽑기 한도를 이미 다 써서 아예 안 뽑혔다는 뜻 —
            /// 이때 Result는 기본값(의미 없음)이니 호출하는 쪽이 먼저 이 값을 봐야 한다.</summary>
            public bool Success;
        }

        /// <summary>무료 뽑기(광고 시청) 하나. 오늘 한도(PetGachaTable.FreePullDailyLimit)를
        /// 넘겼으면 뽑지 않고 Success=false를 돌려준다 — 광고 시청 자체를 막는 게 아니라
        /// 세이브에 반영하는 이 함수가 마지막 방어선이다. 성공하면 등급·종을 뽑아 도감(또는
        /// 중복이면 조각)에 반영하고 오늘 뽑은 횟수를 올린다.</summary>
        public static FreePullOutcome PullFree(SaveData save, int seed)
        {
            if (!save.PetGacha.CanPullFree())
                return new FreePullOutcome { Success = false };

            var result = PetGachaTable.Open(PetGachaTable.Free(), seed);
            var outcome = ResolveAndRecordSpecies(save, result, seed);
            save.PetGacha.RecordFreePull();
            return new FreePullOutcome { Result = outcome, Success = true };
        }

        /// <summary>일반 뽑기(레이싱 재화 소모, 무과금 진행선). 하루 한도가 없어서 항상 뽑힌다 —
        /// 재화가 충분한지는 호출하는 쪽이 먼저 확인하고 나서 부른다.</summary>
        public static PetPullOutcome PullNormal(SaveData save, int seed)
        {
            var result = PetGachaTable.Open(PetGachaTable.Normal(), seed);
            return ResolveAndRecordSpecies(save, result, seed);
        }

        public struct AdvancedPullOutcome
        {
            public PetPullOutcome Result;

            /// <summary>false면 useFreeDaily=true인데 오늘 무료분을 이미 썼다는 뜻 — 이때 Result는
            /// 기본값(의미 없음). useFreeDaily=false(유료)면 항상 true다.</summary>
            public bool Success;
        }

        /// <summary>고급 뽑기 하나(pet-gacha.md 3절 — 3~6등급, 80뽑째 신화 확정). useFreeDaily가
        /// true면 하루 1개 무료분을 쓴다(이미 오늘 썼으면 Success=false, 안 뽑힘) — false면 유료
        /// 뽑기로 취급해 무료분 여부와 무관하게 항상 뽑힌다(뽑기 비용을 실제로 낼 수 있는지는
        /// 호출하는 쪽이 먼저 확인, 클래스 상단 주석 참고).</summary>
        public static AdvancedPullOutcome PullAdvanced(SaveData save, int seed, bool useFreeDaily)
        {
            if (useFreeDaily && save.PetGacha.AdvancedFreePullClaimedToday)
                return new AdvancedPullOutcome { Success = false };

            var result = PetGachaTable.Open(PetGachaTable.Advanced(), seed,
                save.PetGacha.AdvancedOpenedSincePity, PetGachaTable.AdvancedPityCount, PetGachaTable.AdvancedPityGrade);
            var outcome = ResolveAndRecordSpecies(save, result, seed);
            save.PetGacha.RecordAdvancedPull(result.Guaranteed);
            if (useFreeDaily) save.PetGacha.AdvancedFreePullClaimedToday = true;
            return new AdvancedPullOutcome { Result = outcome, Success = true };
        }

        /// <summary>고급 뽑기 10연차 — 5등급(전설) 이상이 하나도 없으면 마지막에 하나를 확정으로
        /// 채운다(PetGachaTable.OpenTen). 하루 무료분과는 무관하다(10연차는 pet-gacha.md 4절대로
        /// 항상 유료) — 비용은 호출하는 쪽 몫.</summary>
        public static PetPullOutcome[] PullAdvancedTen(SaveData save, int baseSeed)
        {
            var startingPity = save.PetGacha.AdvancedOpenedSincePity;
            var results = PetGachaTable.OpenTen(PetGachaTable.Advanced(), baseSeed, PetGachaTable.AdvancedTenPullMinGrade,
                startingPity, PetGachaTable.AdvancedPityCount, PetGachaTable.AdvancedPityGrade);
            var outcomes = new PetPullOutcome[results.Length];
            for (var i = 0; i < results.Length; i++)
            {
                outcomes[i] = ResolveAndRecordSpecies(save, results[i], baseSeed + i);
                // results[i].Guaranteed는 확정 연출용 표시라 80천장 도달과 10연차 최소 등급 보장
                // 둘 다 true를 준다(PetGachaResult 주석 그대로) — 그대로 RecordAdvancedPull에
                // 넘기면 10연차 보장으로 확정된 칸에서도 피티 카운터가 0으로 리셋돼 버려서, 진짜
                // 80천장까지 실제로 쌓인 진행도가 사라진다(80천장이 부당하게 늦춰짐). 그래서
                // OpenTen이 그 회차에 내부적으로 쓴 것과 같은 식으로 "진짜 피티 도달"만 직접
                // 다시 계산해서 넘긴다.
                var truePity = PetGachaTable.AdvancedPityCount > 0 && startingPity + i + 1 >= PetGachaTable.AdvancedPityCount;
                save.PetGacha.RecordAdvancedPull(truePity);
            }
            return outcomes;
        }

        public struct SpecialPullOutcome
        {
            public PetPullOutcome Result;

            /// <summary>false면 인장이 하나도 없어 안 뽑혔다는 뜻(Result는 기본값, 의미 없음).</summary>
            public bool Success;
        }

        /// <summary>특수 뽑기 하나(pet-gacha.md 3절 — 초월이 나오는 유일한 곳, 5~7등급, 80뽑째
        /// 초월 확정). 인장(<see cref="PetGachaSave.TranscendentSealCount"/>) 1개를 쓴다 —
        /// pet-gacha.md "모으면 특수 뽑기 1회" 그대로 1:1 교환이라 별도 결정이 필요 없었다.
        /// 인장이 없으면 뽑지 않고 Success=false(인장을 어디서 얻는지는 PetGachaController 클래스
        /// 주석 참고 — 아직 다 안 붙었다).</summary>
        public static SpecialPullOutcome PullSpecial(SaveData save, int seed)
        {
            if (save.PetGacha.TranscendentSealCount < 1)
                return new SpecialPullOutcome { Success = false };

            var result = PetGachaTable.Open(PetGachaTable.Special(), seed,
                save.PetGacha.SpecialOpenedSincePity, PetGachaTable.SpecialPityCount, PetGachaTable.SpecialPityGrade);
            var outcome = ResolveAndRecordSpecies(save, result, seed);
            save.PetGacha.TranscendentSealCount--;
            save.PetGacha.RecordSpecialPull(result.Guaranteed);
            return new SpecialPullOutcome { Result = outcome, Success = true };
        }
    }
}
