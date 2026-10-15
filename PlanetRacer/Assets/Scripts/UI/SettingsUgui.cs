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

        // A-28-2(2026-10-15): 키트 스킨을 얹으면 그림 자체에 색이 들어 있어서, 전에 쓰던 파란 두 색
        // (0.35,0.51,0.86 / 0.28,0.34,0.62)을 곱하면 둘 다 어둡게 물들고 고른 칸과 안 고른 칸의
        // 차이도 거의 안 보인다. 그래서 **고른 칸은 그림 색 그대로**(흰 tint), 안 고른 칸만 한
        // 단계 흐리게 둔다 — 꺼진 버튼이 흐려지는 것과 같은 신호라 따로 배울 것이 없다.
        static readonly Color FpsSelected = Color.white;
        static readonly Color FpsIdle = new Color(0.62f, 0.64f, 0.72f);

        /// <summary>A-28-2: 이 화면의 주 버튼. 「저장」 한 칸뿐이다 — 설정 화면에서 값을 바깥으로
        /// 내보내는(파일에 적고 클립보드에 복사하는) 유일한 칸이라 여기만 도드라지게 한다.
        /// 소리·프레임 네 칸은 고르는 칸이지 실행하는 칸이 아니고(고른 표시는 위 tint가 준다),
        /// 「닫기」도 보조 버튼이다 — 지금까지 붙인 화면 전부와 같은 판단이다.
        /// 부트스트랩(`GemRacer/20`)도 이 배열을 그대로 쓴다 — 양쪽이 어긋나지 않게.</summary>
        public static readonly string[] PrimaryButtons = { "feedback-save-button" };

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
            _feedbackField = UiKit.Find<TMP_InputField>(transform, "slot-feedback-field");
            _feedbackSaveButton = UiKit.Find<Button>(transform, "feedback-save-button");
            _feedbackStatusLabel = UiKit.Find<TMP_Text>(transform, "feedback-status");

            _soundButtonLabel = _soundButton != null ? _soundButton.GetComponentInChildren<TMP_Text>() : null;
            _fps30Image = _fps30Button != null ? _fps30Button.GetComponent<Image>() : null;
            _fps60Image = _fps60Button != null ? _fps60Button.GetComponent<Image>() : null;

            _soundButton?.onClick.AddListener(OnSoundClicked);
            _fps30Button?.onClick.AddListener(() => OnFrameRateClicked(30));
            _fps60Button?.onClick.AddListener(() => OnFrameRateClicked(60));
            _feedbackSaveButton?.onClick.AddListener(OnFeedbackSaveClicked);

            // A-28-2(2026-10-15): 키트 스킨. 부트스트랩(`GemRacer/20`)과 **같은 함수**를 부른다 —
            // 씬을 다시 저장하지 못한 채 빌드가 나가도 화면이 회색 상자로 돌아가지 않게.
            //
            // **창으로 볼 노드가 없다.** 이 화면도 루트 Image가 화면을 꽉 채우는 가림막이라
            // 둥근 창을 얹을 자리가 아니다(제작·상자·레이스 출전과 같은 이유). 카드 이름을
            // 비우면 함수가 화면 전체를 훑어서 줄 셋(`row-*`)·입력칸(`slot-*`)·버튼 다섯이
            // 한 번에 걸린다.
            UiSkin.ApplyToPopup(transform, null, PrimaryButtons);

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
