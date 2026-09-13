using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Audio;
using GemRacer.Diagnostics;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D14-N: 설정 화면. Crafting/LootBox 패널과 같은 오버레이 패턴 — 값 자체는
    /// MiningController.SoundEnabled/TargetFrameRate가 그대로 들고 있고(설정 저장은
    /// MiningController.Save()가 이미 한다), 여기는 표시와 클릭 전달만 한다.
    /// D17-N: 피드백 줄만 예외 — MiningController를 거치지 않고 FeedbackLog(로컬 파일 append +
    /// 클립보드 복사)를 이 패널이 직접 부른다. 세이브 상태가 아니라서 target이 필요 없다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SettingsPanel : MonoBehaviour
    {
        [Tooltip("설정 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("버튼을 누를 때 탭 효과음을 낼 대상. 비워두면 무음.")]
        public AudioHub audioHub;

        VisualElement _root;
        Button _soundButton, _fps30Button, _fps60Button, _feedbackSaveButton;
        TextField _feedbackField;
        Label _feedbackStatusLabel;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _soundButton = _root.Q<Button>("sound-button");
            _fps30Button = _root.Q<Button>("fps30-button");
            _fps60Button = _root.Q<Button>("fps60-button");
            _feedbackField = _root.Q<TextField>("feedback-field");
            _feedbackSaveButton = _root.Q<Button>("feedback-save-button");
            _feedbackStatusLabel = _root.Q<Label>("feedback-status");

            _soundButton.clicked += OnSoundClicked;
            _fps30Button.clicked += () => OnFrameRateClicked(30);
            _fps60Button.clicked += () => OnFrameRateClicked(60);
            _feedbackSaveButton.clicked += OnFeedbackSaveClicked;

            Refresh();
        }

        // 다른 패널이 소리를 끄거나 프레임을 바꿀 리는 없지만(이 화면에서만 바뀐다), Crafting/LootBox
        // 패널과 같은 패턴을 맞추기 위해 매 프레임 다시 그린다 — 비용도 라벨 두 개뿐이라 무시할 만하다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;

            _soundButton.text = target.SoundEnabled ? "켜짐" : "꺼짐";

            _fps30Button.EnableInClassList("selected", target.TargetFrameRate == 30);
            _fps60Button.EnableInClassList("selected", target.TargetFrameRate == 60);
        }

        void OnSoundClicked()
        {
            if (target == null) return;
            target.SetSoundEnabled(!target.SoundEnabled);
            audioHub?.PlayUiTap();
        }

        void OnFrameRateClicked(int fps)
        {
            if (target == null) return;
            target.SetTargetFrameRate(fps);
            audioHub?.PlayUiTap();
        }

        /// <summary>빈 칸으로 누르면(공백만 있어도) 아무 일도 안 하고 안내만 바꾼다 — FeedbackLog.Append가
        /// 이미 그 판단을 하니 결과(bool)만 보고 문구를 고른다.</summary>
        void OnFeedbackSaveClicked()
        {
            audioHub?.PlayUiTap();
            if (FeedbackLog.Append(_feedbackField.value))
            {
                _feedbackField.value = "";
                _feedbackStatusLabel.text = "저장됐어요 (클립보드에도 복사됨)";
            }
            else
            {
                _feedbackStatusLabel.text = "내용을 적어 주세요";
            }
        }
    }
}
