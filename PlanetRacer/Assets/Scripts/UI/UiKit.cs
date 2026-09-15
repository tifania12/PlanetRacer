using UnityEngine;

namespace GemRacer.UI
{
    /// <summary>
    /// D08(2026-09-15): UI Toolkit에서 일반 UI(uGUI)로 옮기면서 만든 작은 도우미.
    ///
    /// UI Toolkit에서는 `root.Q&lt;Label&gt;("planet-name")` 으로 이름을 찍어 찾았다.
    /// uGUI에는 그런 게 없어서 계층을 훑는 함수를 하나 둔다. 이렇게 해 두면 패널 스크립트가
    /// 인스펙터 연결에 의존하지 않아서, 부트스트랩 코드가 계층을 다시 만들어도 스크립트를
    /// 고칠 필요가 없다 — 이름만 맞으면 된다.
    ///
    /// 주의: 이름으로 찾는 건 시작할 때 한 번만 한다. 매 프레임 부르면 느리다.
    /// </summary>
    public static class UiKit
    {
        /// <summary>root 아래(자기 자신 포함)에서 이름이 name인 자식을 깊이 우선으로 찾아
        /// T 컴포넌트를 돌려준다. 못 찾으면 null이고 경고를 한 줄 남긴다 —
        /// 조용히 null이 흘러가면 나중에 엉뚱한 곳에서 NullReference로 터진다.</summary>
        public static T Find<T>(Transform root, string name, bool warnIfMissing = true) where T : Component
        {
            var t = FindTransform(root, name);
            if (t == null)
            {
                if (warnIfMissing)
                    Debug.LogWarning($"[GemRacer] UI에서 '{name}'을(를) 못 찾았다. (기준: {Path(root)})");
                return null;
            }
            var c = t.GetComponent<T>();
            if (c == null && warnIfMissing)
                Debug.LogWarning($"[GemRacer] '{name}'은(는) 찾았는데 {typeof(T).Name}이(가) 붙어 있지 않다.");
            return c;
        }

        /// <summary>컴포넌트 말고 GameObject 자체가 필요할 때. 패널을 켜고 끄는 데 쓴다.</summary>
        public static GameObject FindObject(Transform root, string name, bool warnIfMissing = true)
        {
            var t = FindTransform(root, name);
            if (t == null)
            {
                if (warnIfMissing)
                    Debug.LogWarning($"[GemRacer] UI에서 '{name}'을(를) 못 찾았다. (기준: {Path(root)})");
                return null;
            }
            return t.gameObject;
        }

        static Transform FindTransform(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindTransform(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        static string Path(Transform t)
        {
            if (t == null) return "(null)";
            var s = t.name;
            var p = t.parent;
            while (p != null) { s = p.name + "/" + s; p = p.parent; }
            return s;
        }
    }
}
