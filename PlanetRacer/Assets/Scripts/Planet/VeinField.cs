using System.Collections.Generic;
using UnityEngine;
using GemRacer.Core;
// 이 파일이 GemRacer.Planet 네임스페이스 안에 있어서 그냥 Planet이라고 쓰면 네임스페이스로 읽힌다.
// MiningController.cs와 같은 별칭을 쓴다.
using CorePlanet = GemRacer.Core.Planet;

namespace GemRacer.Planet
{
    /// <summary>
    /// D06-N: 행성 표면에 광맥을 놓고, 지금 캐고 있는 광맥을 눈에 띄게 한다.
    ///
    /// 위치는 전부 코어의 VeinLayout이 정한다 — 채굴차가 도는 대원(SurfaceMover.orbitAxis에 수직인
    /// 큰 원) 위에 VeinCount개를 같은 간격으로. 채굴차도 같은 각도 계산으로 움직이니(MiningController가
    /// SurfaceMover.SetOrbitAngle을 부른다) 둘은 절대 어긋나지 않는다.
    ///
    /// 주행선 위에 그대로 놓으면 채굴차가 광맥에 파묻히니, 축 방향으로 sideOffsetMeters만큼 옆으로
    /// 비켜 놓는다 — 채굴차가 광맥 옆에 나란히 서는 그림이 된다.
    ///
    /// 광맥 모양은 임시다. 실제 아트가 들어오면 프리팹으로 갈아 끼운다(veinPrefab을 채우면 그걸 쓴다).
    /// </summary>
    [DisallowMultipleComponent]
    public class VeinField : MonoBehaviour
    {
        [Tooltip("행성 중심 트랜스폼. 비워두면 월드 원점.")]
        public Transform planetCenter;

        [Tooltip("행성 반지름(m). SurfaceMover.radius와 같아야 한다.")]
        public float radius = 20f;

        [Tooltip("채굴차 궤도 회전축. SurfaceMover.orbitAxis와 같아야 한다.")]
        public Vector3 orbitAxis = new Vector3(0.2f, 1f, 0f);

        [Tooltip("각도 0도의 기준 방향. 채굴차의 출발 위치 방향과 같아야 한다.")]
        public Vector3 startDirection = new Vector3(0f, 0f, 1f);

        [Tooltip("주행선에서 옆으로 비켜 놓는 거리(m). 0이면 채굴차가 광맥 위에 겹쳐 선다.")]
        public float sideOffsetMeters = 2.2f;

        [Tooltip("광맥 한 덩이의 크기(m).")]
        public float veinScale = 1.1f;

        [Tooltip("평소 광맥 재질.")]
        public Material veinMaterial;

        [Tooltip("지금 캐고 있는 광맥 재질. 비워두면 색만 밝게 바꾸지 않고 크기 맥동만 준다.")]
        public Material activeVeinMaterial;

        [Tooltip("채굴 파티클 재질. 부트스트랩이 만들어 넣는다 — 런타임 Shader.Find는 웹 빌드에서 " +
                 "셰이더가 빠져 분홍색이 될 수 있어서 씬에서 참조로 들고 있어야 한다.")]
        public Material dustMaterial;

        [Tooltip("채워 넣으면 임시 도형 대신 이 프리팹을 광맥으로 쓴다.")]
        public GameObject veinPrefab;

        sealed class Vein
        {
            public Transform Root;
            public Renderer[] Renderers;
            public Vector3 BaseScale;
        }

        readonly List<Vein> _veins = new List<Vein>();
        ParticleSystem _dust;
        int _activeIndex = -1;
        bool _mining;
        float _pulseTime;

        public int Count => _veins.Count;

        Vector3 Center => planetCenter != null ? planetCenter.position : Vector3.zero;

        /// <summary>index번 광맥이 놓일 표면 위 위치(월드).</summary>
        public Vector3 VeinPosition(Vector3 axis, Vector3 start, float angleDegrees)
        {
            var dir = Quaternion.AngleAxis(angleDegrees, axis) * start;
            // 옆으로 비키는 양(m)을 구면 각도로 바꿔서 축 쪽으로 기울인다.
            var lateral = sideOffsetMeters / Mathf.Max(radius, 0.01f);
            var placed = (dir * Mathf.Cos(lateral) + axis * Mathf.Sin(lateral)).normalized;
            return Center + placed * radius;
        }

        /// <summary>행성 하나치 광맥을 다시 만든다. 다시 불러도 같은 결과가 나온다(멱등).</summary>
        public void Build(CorePlanet planet)
        {
            Clear();
            if (planet == null) return;

            var axis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;
            var start = Vector3.ProjectOnPlane(startDirection, axis);
            if (start.sqrMagnitude < 0.0001f) start = Vector3.Cross(axis, Vector3.right);
            start.Normalize();

            var count = Mathf.Max(0, planet.VeinCount);
            for (var k = 0; k < count; k++)
            {
                var angle = VeinLayout.VeinAngleDegrees(planet, k);
                var pos = VeinPosition(axis, start, angle);
                _veins.Add(CreateVein(k, pos));
            }

            EnsureDust();
            SetActiveVein(-1, false);
        }

        public void Clear()
        {
            foreach (var v in _veins)
            {
                if (v.Root != null) DestroyObject(v.Root.gameObject);
            }
            _veins.Clear();
            _activeIndex = -1;
        }


        Vein CreateVein(int index, Vector3 worldPos)
        {
            GameObject root;
            if (veinPrefab != null)
            {
                root = Instantiate(veinPrefab, transform);
            }
            else
            {
                root = new GameObject();
                root.transform.SetParent(transform, false);
                // 임시 도형 — 크기와 기울기가 다른 결정 세 개를 모아 덩어리처럼 보이게 한다.
                AddShard(root.transform, new Vector3(0f, 0.55f, 0f), 1.0f, 0f);
                AddShard(root.transform, new Vector3(0.38f, 0.32f, 0.12f), 0.62f, 18f);
                AddShard(root.transform, new Vector3(-0.3f, 0.28f, -0.22f), 0.5f, -25f);
            }

            root.name = $"Vein_{index:00}";
            var normal = (worldPos - Center).normalized;
            var fwd = Vector3.ProjectOnPlane(Vector3.forward, normal);
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.Cross(normal, Vector3.right);
            root.transform.position = worldPos;
            root.transform.rotation = Quaternion.LookRotation(fwd.normalized, normal);
            root.transform.localScale = Vector3.one * Mathf.Max(0.01f, veinScale);

            var renderers = root.GetComponentsInChildren<Renderer>();
            if (veinMaterial != null)
                foreach (var r in renderers) r.sharedMaterial = veinMaterial;

            return new Vein { Root = root.transform, Renderers = renderers, BaseScale = root.transform.localScale };
        }

        void AddShard(Transform parent, Vector3 localPos, float scale, float tiltDegrees)
        {
            var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = "Shard";
            shard.transform.SetParent(parent, false);
            shard.transform.localPosition = localPos;
            shard.transform.localRotation = Quaternion.Euler(tiltDegrees, 45f, tiltDegrees * 0.5f);
            shard.transform.localScale = new Vector3(scale * 0.55f, scale * 1.3f, scale * 0.55f);

            // 부딪힐 일이 없다. 콜라이더는 빼는 쪽이 모바일에서 싸다(SurfaceScatter와 같은 이유).
            var col = shard.GetComponent<Collider>();
            if (col != null) DestroyObject(col);
        }

        static void DestroyObject(Object o)
        {
            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }

        /// <summary>채굴 먼지 파티클 하나를 만들어 두고, 캐는 광맥으로 옮겨 다니며 쓴다.
        /// 광맥마다 하나씩 두면 대부분 놀고 있어서 하나만 돌린다.</summary>
        void EnsureDust()
        {
            if (_dust != null) return;

            var go = new GameObject("MiningDust");
            go.transform.SetParent(transform, false);
            _dust = go.AddComponent<ParticleSystem>();

            var main = _dust.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = 0.9f;
            main.startSpeed = 2.4f;
            main.startSize = 0.22f;
            main.startColor = new Color(0.95f, 0.92f, 0.75f, 0.9f);
            main.gravityModifier = 0.55f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;

            var emission = _dust.emission;
            emission.rateOverTime = 22f;

            var shape = _dust.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 28f;
            shape.radius = 0.35f;

            var sizeOverLife = _dust.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.15f));

            var renderer = _dust.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && dustMaterial != null) renderer.sharedMaterial = dustMaterial;

            _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>지금 캐고 있는 광맥을 정한다. index가 범위 밖(-1 등)이면 아무것도 강조하지 않는다.</summary>
        public void SetActiveVein(int index, bool mining)
        {
            _mining = mining;
            var changed = index != _activeIndex;
            if (changed)
            {
                RestoreVein(_activeIndex);
                _activeIndex = index;
            }

            var active = Get(_activeIndex);
            if (active != null && mining && activeVeinMaterial != null)
                foreach (var r in active.Renderers) r.sharedMaterial = activeVeinMaterial;
            else if (active != null && !mining && veinMaterial != null)
                foreach (var r in active.Renderers) r.sharedMaterial = veinMaterial;

            if (_dust == null) return;
            if (mining && active != null)
            {
                // 먼지는 광맥 뿌리에서 위로 솟게 — 표면 법선이 그 광맥의 '위'다.
                var normal = (active.Root.position - Center).normalized;
                _dust.transform.position = active.Root.position + normal * 0.2f;
                _dust.transform.rotation = Quaternion.LookRotation(normal);
                if (!_dust.isPlaying) _dust.Play();
            }
            else if (_dust.isPlaying)
            {
                _dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        Vein Get(int index) => index >= 0 && index < _veins.Count ? _veins[index] : null;

        void RestoreVein(int index)
        {
            var v = Get(index);
            if (v == null) return;
            v.Root.localScale = v.BaseScale;
            if (veinMaterial != null)
                foreach (var r in v.Renderers) r.sharedMaterial = veinMaterial;
        }

        void Update()
        {
            var active = Get(_activeIndex);
            if (active == null) return;

            if (_mining)
            {
                // 캐는 동안 광맥이 숨 쉬듯 커졌다 작아진다. 프레임 수가 아니라 시간에 비례한다(CLAUDE.md 5번).
                _pulseTime += Time.deltaTime;
                var pulse = 1f + 0.12f * Mathf.Sin(_pulseTime * 7f);
                active.Root.localScale = active.BaseScale * pulse;
            }
            else if (active.Root.localScale != active.BaseScale)
            {
                active.Root.localScale = active.BaseScale;
                _pulseTime = 0f;
            }
        }
    }
}
