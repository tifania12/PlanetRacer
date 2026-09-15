using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Audio;
using GemRacer.Diagnostics;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>
    /// U-06(2026-09-16): SettingsPanel(UI Toolkit)을 일반 UI(uGUI)로 옮긴 것. 로직은 그대로다 —
    /// 값 자체는 MiningController.SoundEnabled/TargetFrameRate가 들고 있고(저장은
    /// MiningController.Save()가 이미 한다), 여기는 표시와 클릭 전달만 한다. 피드백 줄만 예외로
    /// MiningController를 거치지 않고 FeedbackLog(로컬 파일 append + 클립보드 복사)를 직접 부른다
    /// (세이브 상태가 아니라서 target이 필요 없다).
    ///
    /// UI Toolkit판은 `EnableInClassList("selected", ...)`로 프레임 버튼 색을 바꿨는데, uGUI에는
    /// 그런 클래스 토글이 없어서(ugui-migration.md 변환표) 버튼의 Image.color를 직접 바꾼다.
    /// </summary>
    public sealed class SettingsUgui : MonoBehaviour
    {
        [Tooltip("설정 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("버튼을 누를 때 탭 효과음을 낼 대상. 비워두면 무음.")]
        public AudioHub audioHub;

        static readonly Color FpsSelected = new Color(0.35f, 0.51f, 0.86f);
        static readonly Color FpsIdle = new Color(0.28f, 0.34f, 0.62f);

        Button _soundButton, _fps30Button, _fps60Button, _feedbackSaveButton;
        TMP_Text _soundButtonLabel;
        Image _fps30Image, _fps60Image;
        TMP_InputField _feedbackField;
        TMP_Text _feedbackStatusLabel;

        void Awake()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();

            _soundButton = UiKit.Find<Button>(transform, "sound-button");
            _fps30Button = UiKit.Find<Button>(transform, "fps30-button");
            _fps60Button = UiKit.Find<Button>(transform, "fps60-button");
            _feedbackField = UiKit.Find<TMP_InputField>(transform, "feedback-field");
            _feedbackSaveButton = UiKit.Find<Button>(transform, "feedback-save-button");
            _feedbackStatusLabel = UiKit.Find<TMP_Text>(transform, "feedback-status");

            _soundButtonLabel = _soundButton != null ? _soundButton.GetComponentInChildren<TMP_Text>() : null;
            _fps30Image = _fps30Button != null ? _fps30Button.GetComponent<Image>() : null;
            _fps60Image = _fps60Button != null ? _fps60Button.GetComponent<Image>() : null;

            _soundButton?.onClick.AddListener(OnSoundClicked);
            _fps30Button?.onClick.AddListener(() => OnFrameRateClicked(30));
            _fps60Button?.onClick.AddListener(() => OnFrameRateClicked(60));
            _feedbackSaveButton?.onClick.AddListener(OnFeedbackSaveClicked);

            Refresh();
        }

        // 다른 패널이 소리를 끄거나 프레임을 바꿀 리는 없지만(이 화면에서만 바뀐다), Crafting/LootBox
        // 패널과 같은 패턴을 맞추기 위해 매 프레임 다시 그린다 — 비용도 라벨 두 개뿐이라 무시할 만하다.
        void Update() => Refresh();

        void Refresh()
        {
            if (target == null) return;

            if (_soundButtonLabel != null) _soundButtonLabel.text = target.SoundEnabled ? "켜짐" : "꺼짐";

            if (_fps30Image != null) _fps30Image.color = target.TargetFrameRate == 30 ? FpsSelected : FpsIdle;
            if (_fps60Image != null) _fps60Image.color = target.TargetFrameRate == 60 ? FpsSelected : FpsIdle;
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
            var text = _feedbackField != null ? _feedbackField.text : null;
            if (FeedbackLog.Append(text))
            {
                if (_feedbackField != null) _feedbackField.text = "";
                if (_feedbackStatusLabel != null) _feedbackStatusLabel.text = "저장됐어요 (클립보드에도 복사됨)";
            }
            else
            {
                if (_feedbackStatusLabel != null) _feedbackStatusLabel.text = "내용을 적어 주세요";
            }
        }
    }
}
