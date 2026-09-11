using System.Collections.Generic;
using UnityEngine;

namespace GemRacer.Planet
{
    /// <summary>
    /// 구체 행성 표면에 지형 장식을 뿌린다.
    ///
    /// 속도감은 "카메라 가까이를 지나가는 것"에서 나온다. 지평선 근처 물체는 화면에서 거의
    /// 안 움직이기 때문에, 표면 전체에 고르게 뿌리는 것만으로는 빨라 보이지 않는다.
    /// 그래서 두 가지를 제공한다.
    ///   - ScatterEven: 표면 전체 균등 배치. 멀리서 행성을 보는 채굴 화면용.
    ///   - ScatterAlongTrack: 주행 대원 양옆 띠에만 배치. 레이스 화면용. 이쪽이 속도감을 만든다.
    ///
    /// 실험에서 배운 것(2026-09-11): 주행선 위에까지 깔면 카메라가 장식에 파묻힌다.
    /// 가운데는 반드시 비워야 한다.
    /// </summary>
    public static class SurfaceScatter
    {
        /// <summary>구 표면 균등 분포 방향들. 피보나치 구면.</summary>
        public static List<Vector3> FibonacciDirections(int count, int seed = 0)
        {
            var result = new List<Vector3>(Mathf.Max(0, count));
            if (count <= 0) return result;

            float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));
            var rng = new Jitter(seed);

            for (int i = 0; i < count; i++)
            {
                float y = 1f - (i / (float)Mathf.Max(1, count - 1)) * 2f;
                float radiusAtY = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float theta = goldenAngle * i + rng.Signed() * 0.15f;
                result.Add(new Vector3(Mathf.Cos(theta) * radiusAtY, y, Mathf.Sin(theta) * radiusAtY).normalized);
            }
            return result;
        }

        public sealed class DecoSettings
        {
            public PrimitiveType Shape = PrimitiveType.Cube;
            public Material Material;
            public float MinScale = 0.6f;
            public float MaxScale = 2.6f;
        }

        /// <summary>표면 전체에 고르게. 채굴 화면·먼 원경용.</summary>
        public static List<GameObject> ScatterEven(
            Transform parent, float radius, int count, DecoSettings deco, int seed = 12345)
        {
            var made = new List<GameObject>(Mathf.Max(0, count));
            var dirs = FibonacciDirections(count, seed);
            var rng = new Jitter(seed ^ 0x2545F491);
            foreach (var dir in dirs) made.Add(Place(parent, dir, radius, deco, rng));
            return made;
        }

        /// <summary>
        /// 주행 대원(orbitAxis에 수직인 큰 원) 양옆 띠에 배치한다. 가운데 주행로는 비운다.
        /// </summary>
        /// <param name="orbitAxis">SurfaceMover.orbitAxis와 같은 값</param>
        /// <param name="clearWidth">비워 둘 주행로 전체 폭(m)</param>
        /// <param name="bandWidth">주행로 바깥으로 장식이 깔릴 폭(m)</param>
        public static List<GameObject> ScatterAlongTrack(
            Transform parent, float radius, int count, Vector3 orbitAxis,
            float clearWidth, float bandWidth, DecoSettings deco, int seed = 777)
        {
            var made = new List<GameObject>(Mathf.Max(0, count));
            if (count <= 0 || radius <= 0.01f) return made;

            Vector3 axis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;
            Vector3 u = Vector3.Cross(axis, Vector3.right);
            if (u.sqrMagnitude < 0.001f) u = Vector3.Cross(axis, Vector3.forward);
            u.Normalize();
            Vector3 v = Vector3.Cross(axis, u).normalized;

            // 거리(m)를 구면 각도(rad)로. 반지름이 커지면 같은 m가 더 작은 각이 된다.
            float innerAngle = (clearWidth * 0.5f) / radius;
            float outerAngle = innerAngle + bandWidth / radius;

            var rng = new Jitter(seed);
            for (int i = 0; i < count; i++)
            {
                float theta = (i / (float)count) * Mathf.PI * 2f + rng.Signed() * 0.03f;
                Vector3 onCircle = (Mathf.Cos(theta) * u + Mathf.Sin(theta) * v).normalized;

                float side = rng.Unit() < 0.5f ? -1f : 1f;
                float lateral = side * Mathf.Lerp(innerAngle, outerAngle, rng.Unit());
                Vector3 dir = (onCircle * Mathf.Cos(lateral) + axis * Mathf.Sin(lateral)).normalized;

                made.Add(Place(parent, dir, radius, deco, rng));
            }
            return made;
        }

        static GameObject Place(Transform parent, Vector3 normal, float radius, DecoSettings deco, Jitter rng)
        {
            var go = GameObject.CreatePrimitive(deco.Shape);
            if (parent != null) go.transform.SetParent(parent, false);

            float s = Mathf.Lerp(deco.MinScale, deco.MaxScale, rng.Unit());
            go.transform.localScale = new Vector3(
                s * (0.7f + rng.Unit() * 0.6f),
                s * (0.9f + rng.Unit() * 1.4f),
                s * (0.7f + rng.Unit() * 0.6f));

            // 표면에 살짝 박히게. 안 그러면 떠 있는 것처럼 보인다.
            go.transform.position = normal * (radius + s * 0.2f);

            Vector3 fwd = Vector3.ProjectOnPlane(Vector3.forward, normal);
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.Cross(normal, Vector3.right);
            go.transform.rotation = Quaternion.LookRotation(fwd.normalized, normal)
                                    * Quaternion.Euler(0f, rng.Unit() * 360f, 0f);

            // 장식은 수백 개라 콜라이더를 빼는 게 낫다. 부딪힐 일도 없다.
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var r = go.GetComponent<Renderer>();
            if (r != null && deco.Material != null) r.sharedMaterial = deco.Material;

            return go;
        }

        /// <summary>같은 seed면 같은 배치가 나오는 xorshift 난수.</summary>
        sealed class Jitter
        {
            uint _s;
            public Jitter(int seed) { _s = (uint)seed == 0 ? 0x9E3779B9u : (uint)seed; }
            public float Unit()
            {
                var x = _s; x ^= x << 13; x ^= x >> 17; x ^= x << 5; _s = x;
                return (x >> 8) * (1f / 16777216f);
            }
            public float Signed() => Unit() * 2f - 1f;
        }
    }
}
