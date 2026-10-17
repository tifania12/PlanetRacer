using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>
    /// A-27 레이스 하늘 판. 레이스 카메라 자식으로 붙는 평면 한 장에 행성별 하늘 그림을 물린다.
    ///
    /// 스카이박스가 아니라 평면인 이유: 하늘 그림 여섯 장이 파노라마가 아니라 한 장짜리 16:9 그림이다.
    /// 스카이박스 큐브맵으로 쓰면 이어 붙는 자리가 드러난다. 카메라 자식 평면은 늘 같은 구도로 보이고,
    /// 행성을 늘릴 때 그림 한 장만 추가하면 된다.
    ///
    /// 레이스 카메라는 속도에 따라 시야각을 62~88도로 움직인다(<see cref="RaceCamera"/>).
    /// 시야각이 넓어지면 판이 화면보다 작아져 테두리에 배경색이 비치므로 매 프레임 크기를 다시 잡는다.
    /// 화면을 꽉 채우되 그림 비율은 지킨다 — 넘치는 쪽이 잘린다(세로 화면에서는 좌우가 잘린다).
    ///
    /// 그림은 <c>Resources/Art/Race/race-sky-{행성 아이디}</c>. 없으면 판을 끄고
    /// 카메라 배경색(<see cref="PlanetLook.SkyColor"/>)이 그대로 보인다 — 화면이 비지는 않는다.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class RaceSkyPlane : MonoBehaviour
    {
        /// <summary>행성 아이디. <see cref="PlanetLook.Id"/>와 같은 값(quartz·ruby·sapphire·aquamarine·cinnabar·lapis).</summary>
        [Tooltip("행성 아이디. PlanetLook.Id와 같은 값(quartz·ruby·sapphire·aquamarine·cinnabar·lapis)")]
        public string planetId = "ruby";

        /// <summary>카메라에서 이 거리에 판을 세운다. 행성(반지름 60m)보다 멀고 farClipPlane보다 가까워야 한다.</summary>
        [Tooltip("카메라에서 판까지 거리(m). 행성보다 멀고 카메라 farClipPlane보다 가까워야 한다")]
        public float distance = 300f;

        const string ResourceFolder = "Art/Race/race-sky-";

        Camera _camera;
        MeshRenderer _renderer;
        Material _material;
        string _appliedId;
        float _textureAspect = 16f / 9f;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _camera = GetComponentInParent<Camera>();
            Apply(planetId);
        }

        /// <summary>행성이 바뀔 때 부른다. 같은 행성이면 아무 일도 하지 않는다.</summary>
        public void SetPlanet(string id)
        {
            if (!string.IsNullOrEmpty(id) && id == _appliedId) return;
            planetId = id;
            Apply(id);
        }

        void Apply(string id)
        {
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null) return;

            var tex = string.IsNullOrEmpty(id) ? null : Resources.Load<Texture2D>(ResourceFolder + id);
            if (tex == null)
            {
                Debug.LogWarning($"[A-27] 하늘 그림을 못 찾았다: Resources/{ResourceFolder}{id}. 판을 끄고 카메라 배경색으로 간다.");
                _renderer.enabled = false;
                _appliedId = id;
                return;
            }

            // 공유 머티리얼을 그대로 건드리면 에셋 파일이 더러워진다. 실행 중에는 사본 하나만 쓴다.
            if (_material == null)
            {
                _material = new Material(_renderer.sharedMaterial) { name = "RaceSky (instance)" };
                _renderer.material = _material;
            }
            _material.mainTexture = tex;
            _renderer.enabled = true;
            _textureAspect = tex.height > 0 ? (float)tex.width / tex.height : 16f / 9f;
            _appliedId = id;
        }

        // 카메라가 움직이고 시야각을 바꾼 뒤에 크기를 잡아야 한다.
        void LateUpdate()
        {
            if (_renderer == null || !_renderer.enabled) return;
            if (_camera == null)
            {
                _camera = GetComponentInParent<Camera>();
                if (_camera == null) return;
            }

            float far = _camera.farClipPlane;
            float d = Mathf.Clamp(distance, _camera.nearClipPlane + 1f, far * 0.9f);

            // 이 거리에서 화면이 덮는 크기. 시야각은 세로 기준이다.
            float frustumHeight = 2f * d * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * Mathf.Max(0.01f, _camera.aspect);

            // 그림 비율을 지키면서 화면을 덮는다(넘치는 쪽을 자른다).
            float width, height;
            if (frustumWidth / frustumHeight > _textureAspect)
            {
                width = frustumWidth;
                height = frustumWidth / _textureAspect;
            }
            else
            {
                height = frustumHeight;
                width = frustumHeight * _textureAspect;
            }

            var t = transform;
            t.localPosition = new Vector3(0f, 0f, d);
            t.localRotation = Quaternion.identity;
            t.localScale = new Vector3(width, height, 1f);
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }
    }
}
