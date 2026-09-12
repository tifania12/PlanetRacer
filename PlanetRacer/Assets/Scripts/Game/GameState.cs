namespace GemRacer.Game
{
    /// <summary>D04-N: 게임 전체 흐름 상태. 지금 어느 화면(로직)이 활성인지를 결정한다.
    /// 점수·보상 계산 같은 서버 검증이 필요한 규칙이 아니라 화면 전환용 상태라서
    /// 코어(Packages/com.bax.gemracer.core)가 아니라 여기(Assets/Scripts)에 둔다.</summary>
    public enum GameState { Mining, Racing, Result }
}
