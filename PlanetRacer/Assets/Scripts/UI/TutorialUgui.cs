using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-01(2026-09-15): TutorialController를 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// MiningController.TutorialStep(0~4)을 따라가며 말풍선 네 개를 순서대로 보여주고,
    /// 네 단계를 다 지나면 스스로 숨는다.
    ///
    /// UI Toolkit판에는 `_root.pickingMode = Ignore` / `bubble.pickingMode = Position` 이
    /// 있었다. 배너 밖 클릭이 아래 HUD로 통과해야 하기 때문이다. uGUI에서는 그 설정 자체가
    /// 필요 없다 — 루트에 Image를 안 붙이면 레이캐스트 대상이 아니라서 클릭이 그냥 통과한다.
    /// 말풍선만 Image를 갖는다. 부트스트랩이 그렇게 만든다.
    /// </summary>
    public sealed class TutorialUgui : MonoBehaviour
    {
        [Tooltip("진행 상황을 읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        // 문구는 여기 하나에만 있다 — 다듬을 때 이 배열만 고치면 된다.
        static readonly string[] Messages =
        {
            "쿼츠 행성에 온 걸 환영한다! 채굴차가 표면을 돌며 자동으로 원석을 캔다 — 화면 위 숫자를 지켜보자.",
            "화물칸이 다 차면 그 이상은 못 담는다. 원석 숫자 아래 막대가 얼마나 찼는지 가끔 들여다보자.",
            "원석이 모였다면 아래 '제작' 버튼을 눌러 채굴차 부품을 만들어 보자.",
            "부품을 갖췄다면 '레이스' 버튼으로 첫 레이스에 도전해 보자 — 우승하면 부품과 공구 상자를 받는다.",
        };

        TMP_Text _stepLabel, _messageLabel, _nextLabel;
        Button _nextButton;
        GameObject _bubble;

        // 마지막으로 그린 단계. 이 값이 바뀌었을 때만 라벨을 새로 채우고 버튼을 다시 켠다 —
        // "다음"을 눌러 잠긴 버튼이 다음 단계가 되면 자동으로 풀리는 지점이다.
        int _shownStep = -1;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _bubble       = UiKit.FindObject(transform, "tutorial-bubble");
            _stepLabel    = UiKit.Find<TMP_Text>(transform, "tutorial-step-label");
            _messageLabel = UiKit.Find<TMP_Text>(transform, "tutorial-message-label");
            _nextButton   = UiKit.Find<Button>(transform, "tutorial-next-button");
            if (_nextButton != null)
            {
                _nextLabel = _nextButton.GetComponentInChildren<TMP_Text>();
                _nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        void Update()
        {
            if (target == null) return;

            var step = target.TutorialStep;
            if (step >= MiningController.TutorialStepCount)
            {
                if (_bubble != null && _bubble.activeSelf) _bubble.SetActive(false);
                return;
            }
            if (_bubble != null && !_bubble.activeSelf) _bubble.SetActive(true);

            if (step == _shownStep) return;
            _shownStep = step;

            if (_stepLabel != null)    _stepLabel.text = $"{step + 1} / {MiningController.TutorialStepCount}";
            if (_messageLabel != null) _messageLabel.text = Messages[step];
            if (_nextLabel != null)
                _nextLabel.text = step == MiningController.TutorialStepCount - 1 ? "시작하기!" : "다음";
            if (_nextButton != null)   _nextButton.interactable = true;
        }

        /// <summary>단계 건너뛰기 방지 — 누른 즉시 버튼을 잠근다. 다음 프레임이 오기 전에 두 번
        /// 눌러도 AdvanceTutorial이 두 번 불리지 않는다. 다시 켜지는 건 Update가 새 단계를
        /// 실제로 그렸을 때뿐이라, 한 번 클릭이 정확히 한 단계만 넘긴다.</summary>
        void OnNextClicked()
        {
            if (_nextButton != null) _nextButton.interactable = false;
            target?.AdvanceTutorial();
        }
    }
}
