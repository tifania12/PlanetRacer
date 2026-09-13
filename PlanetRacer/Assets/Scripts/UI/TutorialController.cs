using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D13-N: 튜토리얼 첫 5분. MiningController.TutorialStep(0~4)을 그대로 따라가며
    /// 안내 말풍선 4개(첫 접속 → 채굴 시작 → 첫 부품 제작 → 첫 레이스)를 순서대로 보여준다.
    ///
    /// 다른 오버레이(UpgradePanel 등)와 달리 화면을 막지 않는다 — root의 pickingMode를 Ignore로
    /// 둬서 배너 밖 클릭은 그대로 아래 HUD(제작/레이스 버튼)로 통과하고, 말풍선 자체(bubble)만
    /// 다시 Position으로 되돌려 "다음" 버튼은 정상적으로 눌리게 한다.
    ///
    /// 4단계를 다 지나면(TutorialStep >= TutorialStepCount) 스스로 숨는다 — 세이브에 남으므로
    /// 다음 Play부터는 아예 안 뜬다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TutorialController : MonoBehaviour
    {
        [Tooltip("진행 상황을 읽을 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        // D13-M: 문구는 여기 하나에만 있다 — 다음에 다듬을 때 이 배열만 고치면 된다.
        static readonly string[] Messages =
        {
            "쿼츠 행성에 온 걸 환영한다! 채굴차가 표면을 돌며 자동으로 원석을 캔다 — 화면 위 숫자를 지켜보자.",
            "화물칸이 다 차면 그 이상은 못 담는다. 원석 숫자 아래 막대가 얼마나 찼는지 가끔 들여다보자.",
            "원석이 모였다면 아래 '제작' 버튼을 눌러 채굴차 부품을 만들어 보자.",
            "부품을 갖췄다면 '레이스' 버튼으로 첫 레이스에 도전해 보자 — 우승하면 부품과 공구 상자를 받는다.",
        };

        VisualElement _root;
        Label _stepLabel, _messageLabel;
        Button _nextButton;

        // 마지막으로 화면에 그린 단계. 같은 단계를 매 프레임 다시 그릴 필요는 없고, 이 값이
        // 바뀌었을 때만(= AdvanceTutorial이 실제로 한 단계 올렸을 때만) 라벨을 새로 채우고
        // 버튼을 다시 켠다 — "다음"을 눌러 버튼이 잠긴 뒤 다음 단계가 되면 자동으로 풀리는 지점.
        int _shownStep = -1;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.pickingMode = PickingMode.Ignore;

            var bubble = _root.Q<VisualElement>("tutorial-bubble");
            if (bubble != null) bubble.pickingMode = PickingMode.Position;

            _stepLabel = _root.Q<Label>("tutorial-step-label");
            _messageLabel = _root.Q<Label>("tutorial-message-label");
            _nextButton = _root.Q<Button>("tutorial-next-button");
            _nextButton.clicked += OnNextClicked;
        }

        void Update() => Refresh();

        void Refresh()
        {
            if (_root == null || target == null) return;

            var step = target.TutorialStep;
            if (step >= MiningController.TutorialStepCount)
            {
                _root.style.display = DisplayStyle.None;
                return;
            }
            _root.style.display = DisplayStyle.Flex;

            if (step == _shownStep) return;
            _shownStep = step;

            _stepLabel.text = $"{step + 1} / {MiningController.TutorialStepCount}";
            _messageLabel.text = Messages[step];
            _nextButton.text = step == MiningController.TutorialStepCount - 1 ? "시작하기!" : "다음";
            _nextButton.SetEnabled(true);
        }

        // D13-M "단계 건너뛰기 방지": 버튼을 누른 즉시 비활성화해 둔다 — 다음 프레임(Refresh)이
        // 오기 전에 같은 버튼을 두 번 눌러도 AdvanceTutorial이 두 번 불리지 않는다. 다시 켜지는
        // 시점은 Refresh가 새 단계를 실제로 그렸을 때뿐이라, 한 번 클릭이 정확히 한 단계만 넘긴다.
        void OnNextClicked()
        {
            _nextButton.SetEnabled(false);
            target?.AdvanceTutorial();
        }
    }
}
