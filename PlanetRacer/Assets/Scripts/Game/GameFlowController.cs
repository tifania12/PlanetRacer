using UnityEngine;
using GemRacer.Mining;

namespace GemRacer.Game
{
    /// <summary>D04-N: 채굴/레이스/결과 중 지금이 어느 상태인지 하나로 관리한다.
    /// 레이스·결과 화면은 아직 없어서(코어 루프는 있지만 D09-N 이후에 화면이 생긴다) 지금은
    /// MiningController를 켜고 끄는 것뿐이지만, 나중에 레이스 화면이 생기면 같은 방식으로
    /// "이 상태일 때 뭘 켜고 끌지"만 늘리면 된다.</summary>
    [DisallowMultipleComponent]
    public class GameFlowController : MonoBehaviour
    {
        [Tooltip("Mining 상태일 때만 이 컴포넌트가 켜진다. 부트스트랩이 연결해 준다 — " +
                 "비어 있으면 경고만 남기고 아무것도 안 한다.")]
        public MiningController miningController;

        [SerializeField] GameState state = GameState.Mining;
        public GameState State => state;

        void Awake() => Apply();

        public void SetState(GameState next)
        {
            state = next;
            Apply();
        }

        void Apply()
        {
            if (miningController == null)
            {
                Debug.LogWarning("[GemRacer] GameFlowController에 MiningController가 연결되어 있지 않다.");
                return;
            }
            miningController.enabled = state == GameState.Mining;
        }
    }
}
