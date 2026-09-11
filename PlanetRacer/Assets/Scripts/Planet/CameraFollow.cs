using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>타깃(채굴차 등)을 따라가는 3인칭 카메라. 타깃의 위(표면 법선) 기준으로
    /// 뒤쪽·위쪽에서 내려다본다. 구체 표면을 도는 대상이라 타깃의 up이 계속 바뀌는 걸 전제로 한다.
    ///
    /// 따라붙는 속도는 프레임 수와 무관하게 유지한다. 모바일 30fps 옵션과 PC 고주사율에서
    /// 카메라 감이 달라지면 안 되기 때문에, 프레임당 고정 계수 대신 시간 기반 감쇠를 쓴다.</summary>
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("따라갈 대상(채굴차)")]
        public Transform target;

        [Tooltip("타깃 뒤쪽으로 떨어지는 거리(m)")]
        public float distance = 8f;

        [Tooltip("타깃 위쪽으로 떨어지는 거리(m)")]
        public float height = 4f;

        [Range(0.5f, 20f)]
        [Tooltip("따라붙는 빠르기. 클수록 빨리 붙는다. 목표까지 남은 거리의 63%를 좁히는 데 1/이 값 초가 걸린다.")]
        public float followSpeed = 6f;

        void Start()
        {
            // 첫 프레임은 보간 없이 제자리로. 그래야 씬을 켜자마자 차량 뒤에서 시작한다.
            // (기본 카메라 위치가 행성 반대편이라, 보간으로만 오면 한참 헤맨다.)
            SnapToTarget();
        }

        void LateUpdate()
        {
            if (target == null) return;

            // 프레임 독립 감쇠: dt가 커지든 작아지든 같은 시간에 같은 만큼 따라붙는다.
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, DesiredPosition(), t);

            Vector3 lookDir = LookPoint() - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                var desiredRot = Quaternion.LookRotation(lookDir.normalized, target.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, t);
            }
        }

        /// <summary>보간 없이 목표 위치·회전으로 즉시 이동. 시작할 때와 행성을 옮겼을 때 쓴다.</summary>
        public void SnapToTarget()
        {
            if (target == null) return;

            transform.position = DesiredPosition();

            Vector3 lookDir = LookPoint() - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDir.normalized, target.up);
        }

        Vector3 DesiredPosition() => target.position - target.forward * distance + target.up * height;

        Vector3 LookPoint() => target.position + target.up * (height * 0.3f);
    }
}
