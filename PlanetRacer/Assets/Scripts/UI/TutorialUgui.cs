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

        /// <summary>A-28-2: 이 화면의 주 버튼. 말풍선에 있는 단추가 「다음」 하나뿐이고
        /// 그게 튜토리얼이 시키는 일이라 주 버튼으로 둔다(마지막 단계에서는 「시작하기!」로 글자만 바뀐다).
        /// 부트스트랩(`GemRacer/14`)도 이 배열을 그대로 쓴다 — 양쪽이 어긋나지 않게.</summary>
        public static readonly string[] PrimaryButtons = { "tutorial-next-button" };

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

            // A-28-2: 키트 스킨을 여기서도 한 번 입힌다 — 부트스트랩(`GemRacer/14`)과
            // **같은 함수**다. 씬을 다시 저장하지 못한 채로 빌드가 나가도 회색 상자로
            // 돌아가지 않게(오프라인 보상 화면과 같은 방식).
            // 창으로 보는 노드는 말풍선(`tutorial-bubble`)이다 — 이 화면은 루트에 Image가
            // 없어서(밖 클릭을 통과시키려고) 가림막이 아예 없고, 그래서 가림막을 피할 걱정도 없다.
            UiSkin.ApplyToPopup(transform, "tutorial-bubble", PrimaryButtons);
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
            // A-29(2026-10-18): 첫 실행에서 접속 보상 팝업과 말풍선이 동시에 떠 「다음」이 보상 팝업의
            // 제목 줄을 덮었다. 접속 보상을 먼저 확인받고, 그게 닫힌 뒤에 말풍선을 띄운다.
            // 말풍선만 숨기고 단계(TutorialStep)는 건드리지 않으니 보상 확인 뒤 이어서 시작한다.
            if (target.HasPendingDailyRewardNotice)
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
