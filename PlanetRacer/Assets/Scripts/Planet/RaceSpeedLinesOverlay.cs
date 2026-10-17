using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;

namespace GemRacer.Planet
{
    /// <summary>
    /// A-27 레이스 속도선 겹침. 화면 가장자리에 속도선 그림을 한 장 깔고, 레이스 카메라의 속도에 따라
    /// 투명도만 바꾼다. 투명도 곡선은 코어(<see cref="RaceSpeedLines"/>)에 있고 여기서는 부르기만 한다.
    ///
    /// 속도 기준은 <see cref="RaceCamera.fovFullSpeed"/> — 시야각이 최대로 벌어지는 속도에서 속도선도 가장
    /// 진하다. 두 연출이 같은 순간에 정점을 찍어야 따로 놀아 보이지 않는다.
    /// 투명도도 시간에 비례해 따라가게 한다(프레임 수와 무관) — 속도가 출렁여도 깜빡이지 않는다.
    ///
    /// 그림은 <c>Resources/Art/Race/race-speedlines</c>. 없으면 아무것도 그리지 않는다.
    /// 클릭을 막지 않는다(raycastTarget off).
    /// </summary>
    public class RaceSpeedLinesOverlay : MonoBehaviour
    {
        [Tooltip("속도를 읽을 레이스 카메라. 비우면 씬에서 찾는다")]
        public RaceCamera raceCamera;

        [Tooltip("속도선 RawImage. 비우면 이 오브젝트나 자식에서 찾는다")]
        public RawImage image;

        [Range(1f, 30f)]
        [Tooltip("투명도가 목표를 따라가는 빠르기. 클수록 날카롭다")]
        public float fadeSpeed = 8f;

        const string ResourcePath = "Art/Race/race-speedlines";

        float _alpha;

        void Awake()
        {
            if (image == null) image = GetComponentInChildren<RawImage>(true);
            if (image == null) return;
            image.raycastTarget = false;
            var tex = Resources.Load<Texture2D>(ResourcePath);
            image.texture = tex;
            image.enabled = tex != null;
            SetAlpha(0f);
        }

        void Update()
        {
            if (image == null || !image.enabled) return;
            if (raceCamera == null) raceCamera = FindFirstObjectByType<RaceCamera>();
            float target = 0f;
            if (raceCamera != null)
                target = (float)RaceSpeedLines.Opacity(raceCamera.Speed, raceCamera.fovFullSpeed);
            _alpha = Mathf.Lerp(_alpha, target, 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime));
            SetAlpha(_alpha);
        }

        void SetAlpha(float a)
        {
            var c = image.color;
            c.a = a;
            image.color = c;
        }
    }
}
