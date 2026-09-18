using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>구체 행성 표면을 따라 자동으로 움직이는 오브젝트. 방치형 채굴차용.
    /// 표면 법선(행성 중심 → 자기 위치 방향)을 up으로, 진행 방향을 forward로 정렬한다.
    ///
    /// 이동 방식: 고정된 회전축(orbitAxis) 둘레로 자기 방향 벡터를 계속 회전시키는 대원(great circle) 궤도.
    /// 회전축이 고정돼 있으면 방향 벡터와 축 사이의 각도는 항상 일정하게 유지되므로,
    /// 시작 위치가 축과 거의 평행(=축의 극점)이 아닌 한 실제로 극점 근처를 지나가지는 않는다.
    /// 다만 나중에 플레이어가 직접 방향을 바꾸거나 표면을 자유롭게 돌아다니게 하면
    /// 이 가정이 깨지고 LookRotation이 극점 근처에서 뒤집힐 수 있다 — D01-M에서 점검할 것.</summary>
    [DisallowMultipleComponent]
    public class SurfaceMover : MonoBehaviour
    {
        [Tooltip("행성 중심 트랜스폼. 비워두면 월드 원점(0,0,0)을 중심으로 쓴다.")]
        public Transform planetCenter;

        [Tooltip("행성 반지름(m). 부트스트랩에서 만든 구체 스케일과 맞춰야 한다.")]
        public float radius = 20f;

        [Tooltip("표면 이동 속도(m/s)")]
        public float speed = 3f;

        [Tooltip("궤도 회전축. 정규화해서 쓴다. (0,1,0)이면 세계 좌표 적도를 도는 셈이다.")]
        public Vector3 orbitAxis = new Vector3(0.2f, 1f, 0f);

        [Tooltip("D04-N: 이동 정지 여부. MiningController가 채굴 단계(광맥 앞에 서서 캐는 동안) 이걸 꺼서 " +
                 "채굴차를 세운다. 꺼도 speed 값 자체는 그대로 유지되고, 다시 켜면 그 속도로 이어서 돈다.")]
        public bool isMoving = true;

        [Tooltip("D06-N: 각도를 바깥에서 정해 주는 모드. MiningController가 코어의 광맥 진행도를 각도로 " +
                 "바꿔 SetOrbitAngle을 부른다. 켜져 있으면 여기서 스스로 돌지 않는다 — 속도를 화면에서 " +
                 "따로 적분하면 몇 시간 뒤엔 코어가 말하는 광맥 위치와 어긋나기 때문이다.")]
        public bool externallyDriven;

        [Tooltip("D06-N: 채굴 중 흔들림 같은 연출용 위치 보정(m). 표면 위 최종 위치에 그대로 더한다. " +
                 "누적되지 않는다 — 매 프레임 표면 위치를 새로 구한 뒤 더하기 때문이다.")]
        public Vector3 positionOffset;

        /// <summary>행성 중심 기준, 현재 표면 위치의 방향(단위 벡터).</summary>
        Vector3 _direction;

        /// <summary>Start 시점의 방향. 각도 0도의 기준점이다(=채굴차 출발 지점, 코어 VeinLayout의 0도).</summary>
        Vector3 _startDirection;

        /// <summary>출발 지점에서 지금까지 돈 각도(도). externallyDriven일 때만 의미가 있다.</summary>
        public float OrbitAngleDegrees { get; private set; }

        void Start()
        {
            Vector3 center = CenterPosition();
            Vector3 fromCenter = transform.position - center;

            _direction = fromCenter.sqrMagnitude > 0.0001f ? fromCenter.normalized : Vector3.up;
            orbitAxis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;
            _startDirection = _direction;

            ApplyTransform();
        }

        /// <summary>D06-N: 출발 지점 기준 각도(도)로 위치를 직접 정한다. externallyDriven 모드에서 쓴다.
        /// 각도는 누적값이라 360을 넘어도 되고, 접을 필요도 없다.</summary>
        public void SetOrbitAngle(float degrees)
        {
            if (_startDirection.sqrMagnitude < 0.0001f) return; // Start 전이면 아직 기준이 없다
            OrbitAngleDegrees = degrees;
            _direction = (Quaternion.AngleAxis(degrees, orbitAxis) * _startDirection).normalized;
            ApplyTransform();
        }

        void Update()
        {
            if (externallyDriven || !isMoving) return;

            // 각속도(rad/s) = 선속도 / 반지름. 프레임마다 이만큼 축 둘레로 돌린다.
            float angularSpeedDeg = (speed / Mathf.Max(radius, 0.01f)) * Mathf.Rad2Deg;
            _direction = Quaternion.AngleAxis(angularSpeedDeg * Time.deltaTime, orbitAxis) * _direction;
            _direction.Normalize();

            ApplyTransform();
        }

        Vector3 CenterPosition() => planetCenter != null ? planetCenter.position : Vector3.zero;

        void ApplyTransform()
        {
            Vector3 center = CenterPosition();
            Vector3 normal = _direction;
            Vector3 tangent = Vector3.Cross(orbitAxis, normal);

            // 방향 벡터가 회전축과 거의 평행할 때(=축의 극점 바로 위)는 접선이 0에 가까워진다.
            // 지금 궤도 방식에서는 거의 발생하지 않지만, 방어적으로 이전 forward를 표면에 투영해 대체한다.
            if (tangent.sqrMagnitude < 0.0001f)
            {
                tangent = Vector3.ProjectOnPlane(transform.forward, normal);
                if (tangent.sqrMagnitude < 0.0001f)
                    tangent = Vector3.ProjectOnPlane(Vector3.forward, normal);
            }
            tangent.Normalize();

            transform.position = center + normal * radius + positionOffset;
            transform.rotation = Quaternion.LookRotation(tangent, normal);
        }
    }
}
