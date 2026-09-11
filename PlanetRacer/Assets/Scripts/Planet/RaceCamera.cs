using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>
    /// 레이스 전용 카메라. 채굴 카메라(CameraFollow)와 목적이 다르다.
    ///
    /// 채굴은 켜 두고 보는 화면이라 멀리서 행성 전체를 느긋하게 보여 준다.
    /// 레이스는 짧고 강해야 하므로, 지면에 바짝 붙이고 시야각을 키워서
    /// 지형이 화면 가장자리로 쏟아지게 만든다. 속도감은 화면 가장자리에서 나온다.
    ///
    /// 속도가 오를수록 시야각을 조금 더 벌리고 카메라를 낮춘다. 같은 실제 속도라도
    /// 훨씬 빠르게 느껴지는, 레이싱 게임에서 오래 쓰인 방법이다.
    /// </summary>
    [DisallowMultipleComponent]
    public class RaceCamera : MonoBehaviour
    {
        [Tooltip("따라갈 레이싱카")]
        public Transform target;

        [Tooltip("행성 중심. 비우면 월드 원점")]
        public Transform planetCenter;

        [Header("자리")]
        [Tooltip("차 뒤로 떨어지는 거리(m). 짧을수록 빠르게 느껴진다")]
        public float distance = 4.5f;

        [Tooltip("표면 기준 카메라 높이(m). 낮을수록 빠르게 느껴진다")]
        public float height = 1.4f;

        [Tooltip("차보다 얼마나 앞을 보는지(m). 지평선이 화면에 들어오게 한다")]
        public float lookAhead = 9f;

        [Header("시야각")]
        [Tooltip("정지 상태 시야각")]
        public float baseFov = 62f;

        [Tooltip("최고 속도에서의 시야각. 넓을수록 주변부가 빨리 흐른다")]
        public float maxFov = 88f;

        [Tooltip("maxFov에 도달하는 속도(m/s)")]
        public float fovFullSpeed = 25f;

        [Header("따라붙기")]
        [Range(0.5f, 30f)]
        [Tooltip("클수록 빨리 따라붙는다. 프레임 수와 무관하게 동작한다")]
        public float followSpeed = 9f;

        Camera _cam;
        Vector3 _lastTargetPos;
        float _speed;

        void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        void Start()
        {
            if (target != null) _lastTargetPos = target.position;
            SnapToTarget();
        }

        void LateUpdate()
        {
            if (target == null) return;

            // 실제 이동량으로 속도를 잰다. SurfaceMover의 speed 값을 몰라도 되고,
            // 나중에 어떤 이동 방식으로 바뀌어도 카메라는 그대로 쓸 수 있다.
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float instant = Vector3.Distance(target.position, _lastTargetPos) / dt;
            _lastTargetPos = target.position;
            // 속도 값 자체도 부드럽게. 안 그러면 시야각이 떨린다.
            _speed = Mathf.Lerp(_speed, instant, 1f - Mathf.Exp(-6f * dt));

            float t = 1f - Mathf.Exp(-followSpeed * dt);
            transform.position = Vector3.Lerp(transform.position, DesiredPosition(), t);

            Vector3 lookDir = LookPoint() - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                var desiredRot = Quaternion.LookRotation(lookDir.normalized, target.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, t);
            }

            if (_cam != null)
            {
                float k = Mathf.Clamp01(_speed / Mathf.Max(0.01f, fovFullSpeed));
                _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, Mathf.Lerp(baseFov, maxFov, k), t);
            }
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = DesiredPosition();
            Vector3 lookDir = LookPoint() - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDir.normalized, target.up);
            if (_cam == null) _cam = GetComponent<Camera>();
            if (_cam != null) _cam.fieldOfView = baseFov;
        }

        Vector3 DesiredPosition() => target.position - target.forward * distance + target.up * height;

        /// <summary>차 앞쪽 지면을 본다. 구체라서 앞을 보면 자연스럽게 지평선이 걸린다.</summary>
        Vector3 LookPoint() => target.position + target.forward * lookAhead + target.up * 0.6f;
    }
}
