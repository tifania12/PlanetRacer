namespace GemRacer.Core
{
    /// <summary>
    /// P-14 ③: 무료·일반 뽑기를 실제로 돌려 <see cref="SaveData.PetGacha"/>에 반영하는 컨트롤러.
    /// backlog.md가 남긴 "종 없이 등급 카운트만 늘리는 임시 형태" 그대로다 — 종 ID 데이터(②)가
    /// 아직 없어서 "이 등급에서 정확히 어떤 종을 얻었는지"는 못 담고, 등급만 확정해
    /// <see cref="PetGachaSave.AddOwnedSpecies"/>로 그 등급 도감 칸을 하나 채운다. 그래서 지금은
    /// "새 종을 얻었다"와 "이미 다 모은 등급이라 원래는 중복이었을 것"을 구분하지 못한다 — 등급
    /// 도감이 이미 가득 찼으면(AddOwnedSpecies가 상한에서 조용히 멈춤) 그 뽑기는 그냥 버려진다.
    /// 종 ID가 생기면 이 자리를 "그 등급 안에서 어느 종인지 뽑고, 이미 있으면 조각으로"
    /// (PetFusion 참고) 바꿔야 한다. 고급/특수 뽑기는 여기 없다 — P-16(초월의 인장 획득 경로)
    /// 결정이 먼저 필요하다(backlog.md P-14 ③ 참고).
    ///
    /// 재화를 낼 수 있는지(광고 시청 여부, 뽑기 비용 지불)는 이 클래스의 책임이 아니다 —
    /// 호출하는 쪽이 먼저 확인하고 나서 Pull*을 부른다. 여기서 확인하는 건 "오늘 무료 뽑기
    /// 한도"뿐, 그것도 <see cref="PetGachaSave"/>가 이미 들고 있는 규칙을 그대로 따른다.
    /// </summary>
    public static class PetGachaController
    {
        public struct FreePullOutcome
        {
            public PetGachaResult Result;

            /// <summary>false면 오늘 무료 뽑기 한도를 이미 다 써서 아예 안 뽑혔다는 뜻 —
            /// 이때 Result는 기본값(의미 없음)이니 호출하는 쪽이 먼저 이 값을 봐야 한다.</summary>
            public bool Success;
        }

        /// <summary>무료 뽑기(광고 시청) 하나. 오늘 한도(PetGachaTable.FreePullDailyLimit)를
        /// 넘겼으면 뽑지 않고 Success=false를 돌려준다 — 광고 시청 자체를 막는 게 아니라
        /// 세이브에 반영하는 이 함수가 마지막 방어선이다. 성공하면 등급을 뽑아 도감(임시 형태,
        /// 클래스 주석 참고)에 반영하고 오늘 뽑은 횟수를 올린다.</summary>
        public static FreePullOutcome PullFree(SaveData save, int seed)
        {
            if (!save.PetGacha.CanPullFree())
                return new FreePullOutcome { Success = false };

            var result = PetGachaTable.Open(PetGachaTable.Free(), seed);
            save.PetGacha.AddOwnedSpecies(result.Grade);
            save.PetGacha.RecordFreePull();
            return new FreePullOutcome { Result = result, Success = true };
        }

        /// <summary>일반 뽑기(레이싱 재화 소모, 무과금 진행선). 하루 한도가 없어서 항상 뽑힌다 —
        /// 재화가 충분한지는 호출하는 쪽이 먼저 확인하고 나서 부른다.</summary>
        public static PetGachaResult PullNormal(SaveData save, int seed)
        {
            var result = PetGachaTable.Open(PetGachaTable.Normal(), seed);
            save.PetGacha.AddOwnedSpecies(result.Grade);
            return result;
        }
    }
}
