using UnityEngine;

namespace GemRacer.Audio
{
    /// <summary>D14-N: 사운드 자리 배선. 소스 네 개(엔진·채굴·UI 탭·상자)를 미리 만들어 두되,
    /// 지금은 오디오 클립이 하나도 없어서(에셋 팩이 아직 안 들어왔다 — W3 몫) 전부 무음
    /// 플레이스홀더다: 클립이 비어 있으면 아래 메서드는 전부 조용히 아무 일도 안 한다. 나중에
    /// 인스펙터에서 클립만 채워 넣으면(코드 수정 없이) 그대로 소리가 난다.
    ///
    /// MiningController.SetSoundEnabled는 AudioListener.volume을 0으로 낮추는 식으로 전체
    /// 음소거를 다루므로, 여기서는 개별 소스 볼륨을 따로 건드리지 않는다.</summary>
    [DisallowMultipleComponent]
    public sealed class AudioHub : MonoBehaviour
    {
        [Tooltip("채굴차가 이동 중일 때 도는 엔진 루프음. 클립이 없으면 무음.")]
        public AudioSource engineSource;

        [Tooltip("채굴 단계(광맥 앞에 멈춰 캐는 동안) 루프음. 클립이 없으면 무음.")]
        public AudioSource miningSource;

        [Tooltip("업그레이드/제작/레이스/상자/설정 버튼을 누를 때 나는 탭 효과음. 클립이 없으면 무음.")]
        public AudioSource uiTapSource;

        [Tooltip("공구 상자 개봉 효과음. 클립이 없으면 무음.")]
        public AudioSource boxSource;

        public void PlayUiTap() => PlayOneShot(uiTapSource);

        public void PlayBoxOpen() => PlayOneShot(boxSource);

        /// <summary>이동/채굴 전환마다 부른다 — 엔진과 채굴 루프음이 동시에 겹치지 않게 서로
        /// 반대로 켠다.</summary>
        public void SetMovementLoop(bool isMoving)
        {
            SetLoopPlaying(engineSource, isMoving);
            SetLoopPlaying(miningSource, !isMoving);
        }

        static void PlayOneShot(AudioSource source)
        {
            if (source == null || source.clip == null) return; // 무음 플레이스홀더
            source.PlayOneShot(source.clip);
        }

        static void SetLoopPlaying(AudioSource source, bool playing)
        {
            if (source == null || source.clip == null) return; // 무음 플레이스홀더
            if (playing && !source.isPlaying) source.Play();
            else if (!playing && source.isPlaying) source.Stop();
        }
    }
}
