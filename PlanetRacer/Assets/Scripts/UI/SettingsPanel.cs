using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Audio;
using GemRacer.Mining;

namespace GemRacer.UI
{
    /// <summary>D14-N: 설정 화면. Crafting/LootBox 패널과 같은 오버레이 패턴 — 값 자체는
    /// MiningController.SoundEnabled/TargetFrameRate가 그대로 들고 있고(설정 저장은
    /// MiningController.Save()가 이미 한다), 여기는 표시와 클릭 전달만 한다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SettingsPanel : MonoBehaviour
    {
        [Tooltip("설정 대상. 비워두면 씬에서 하나 찾는다.")]
        public MiningController target;

        [Tooltip("버튼을 누를 때 탭 효과음을 낼 대상. 비워두면 무음.")]
        public AudioHub audioHub;

        VisualElement _root;
        Button _soundButton, _fps30Button, _fps60Button;

        void OnEnable()
        {
            if (target == null) target = FindFirstObjectByType<MiningController>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            _soundButton = _root.Q<Button>("sound-button");
            _fps30Button = _root.Q<Button>("fps30-button");
            _fps60Button = _root.Q<Button>("fps60-button");

            _soundButton.clicked += OnSoundClicked;
            _fps30Button.clicked += () => OnFrameRateClicked(30);
            _fps60Button.clicked += () => OnFrameRateClicked(60);

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
    }
}
