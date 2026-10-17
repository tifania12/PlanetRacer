using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>
    /// A-27 결승 아치. 주행로를 가로질러 서 있는 납작한 판 한 장(<c>Resources/Art/Race/race-finish-banner</c>)이다.
    ///
    /// 카메라를 따라 돌지 않는다. 아치는 실제로 그 자리에 서 있는 물건이고, 차가 그 아래를 통과하기
    /// 때문에 카메라를 보고 돌면 지나가는 순간 판이 휙 돌아 버린다. 그래서 자리와 방향은 고정이고
    /// 그림 가운데가 비어 있어서(투명) 차가 그대로 지나간다.
    ///
    /// 자리는 <see cref="lapStartDirection"/> — 한 바퀴의 출발·결승 지점이다.
    /// <see cref="SurfaceMover"/>가 차를 <see cref="orbitAxis"/> 둘레로 돌리므로, 그 축과 출발 방향으로
    /// 진행 방향(<c>cross(축, 방향)</c>)을 구해 그 반대쪽을 바라보게 세운다 — 차가 정면으로 들어온다.
    ///
    /// 행성이 구체라서 판을 표면에 접하게만 두면 양쪽 아래 끝이 공중에 뜬다. 그 뜨는 높이는
    /// 폭과 반지름으로 계산되므로(<c>(폭/2)² / 2R</c>) 그만큼 묻어서 끝이 땅에 닿게 한다.
    ///
    /// 그림이 없으면 판을 끄고 경고만 남긴다 — 레이스 화면이 비지는 않는다.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class RaceFinishArch : MonoBehaviour
    {
        [Tooltip("행성 중심 트랜스폼. 비우면 월드 원점을 쓴다")]
        public Transform planetCenter;

        [Tooltip("행성 반지름(m). 부트스트랩이 씬의 구체 크기에서 넣어 준다")]
        public float radius = 60f;

        [Tooltip("궤도 회전축. SurfaceMover.orbitAxis와 같은 값이어야 진행 방향이 맞는다")]
        public Vector3 orbitAxis = new Vector3(0.15f, 1f, 0f);

        [Tooltip("결승선 지점의 표면 방향(행성 중심 기준). 차의 출발 지점과 같게 두면 한 바퀴의 결승선이 된다")]
        public Vector3 lapStartDirection = new Vector3(0f, 0f, 1f);

        [Tooltip("아치 폭(m). 주행로 폭(8m)보다 넉넉해야 양쪽 기둥이 길 밖에 선다")]
        public float width = 14f;

        [Tooltip("계산된 깊이에 더해 조금 더 묻는 양(m). 지면 장식과 겹쳐 틈이 보일 때만 올린다")]
        public float extraSink = 0.1f;

        const string ResourcePath = "Art/Race/race-finish-banner";

        MeshRenderer _renderer;
        float _textureAspect = 1672f / 941f;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            LoadTexture();
            Place();
        }

        /// <summary>결승선 지점을 바꾼다. 실행 중에 확인용으로도 쓴다.</summary>
        public void PlaceAt(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            lapStartDirection = direction.normalized;
            Place();
        }

        /// <summary>표면 위 제자리에 세운다. 값이 바뀌었으면 다시 부르면 된다.</summary>
        public void Place()
        {
            var up = lapStartDirection.sqrMagnitude > 0.0001f ? lapStartDirection.normalized : Vector3.forward;
            var axis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;

            // 진행 방향. 축과 표면 방향이 거의 평행하면(궤도의 극점) 구할 수 없으니 아무 접선이나 쓴다.
            var travel = Vector3.Cross(axis, up);
            if (travel.sqrMagnitude < 0.0001f) travel = Vector3.Cross(up, Vector3.right);
            if (travel.sqrMagnitude < 0.0001f) travel = Vector3.Cross(up, Vector3.forward);
            travel.Normalize();

            float w = Mathf.Max(0.1f, width);
            float h = w / Mathf.Max(0.01f, _textureAspect);
            float half = w * 0.5f;
            float r = Mathf.Max(0.1f, radius);
            float sag = half * half / (2f * r);   // 구면에서 양 끝이 뜨는 높이

            var center = planetCenter != null ? planetCenter.position : Vector3.zero;
            var t = transform;
            t.position = center + up * (r + h * 0.5f - sag - extraSink);
            // Quad의 면은 자기 좌표 -Z를 본다. 차는 travel의 반대쪽에서 들어오므로 +Z를 travel로 두면 정면이 된다.
            t.rotation = Quaternion.LookRotation(travel, up);
            t.localScale = new Vector3(w, h, 1f);
        }

        void LoadTexture()
        {
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null) return;

            var tex = _renderer.sharedMaterial != null ? _renderer.sharedMaterial.mainTexture as Texture2D : null;
            if (tex == null) tex = Resources.Load<Texture2D>(ResourcePath);
            if (tex == null)
            {
                Debug.LogWarning($"[A-27] 결승 아치 그림을 못 찾았다: Resources/{ResourcePath}. 판을 끈다.");
                _renderer.enabled = false;
                return;
            }

            if (_renderer.sharedMaterial != null && _renderer.sharedMaterial.mainTexture == null)
            {
                _renderer.sharedMaterial.mainTexture = tex;
            }
            if (tex.height > 0) _textureAspect = (float)tex.width / tex.height;
            _renderer.enabled = true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying) Place();
        }
#endif
    }
}
