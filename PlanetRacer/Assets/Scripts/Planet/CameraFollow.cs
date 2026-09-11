using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>타깃(채굴차 등)을 따라가는 3인칭 카메라. 타깃의 위(표면 법선) 기준으로
    /// 뒤쪽·위쪽에서 내려다본다. 구체 표면을 도는 대상이라 타깃의 up이 계속 바뀌는 걸 전제로 한다.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("따라갈 대상(채굴차)")]
        public Transform target;

        [Tooltip("타깃 뒤쪽으로 떨어지는 거리(m)")]
        public float distance = 8f;

        [Tooltip("타깃 위쪽으로 떨어지는 거리(m)")]
        public float height = 4f;

        [Range(0.01f, 1f)]
        [Tooltip("한 프레임에 목표 위치로 얼마나 따라붙는지(보간 계수)")]
        public float followLerp = 0.15f;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPos = target.position - target.forward * distance + target.up * height;
            transform.position = Vector3.Lerp(transform.position, desiredPos, followLerp);

            Vector3 lookPoint = target.position + target.up * (height * 0.3f);
            Vector3 lookDir = lookPoint - transform.position;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRot = Quaternion.LookRotation(lookDir.normalized, target.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, followLerp);
            }
        }
    }
}
