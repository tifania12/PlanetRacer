using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        // A-16(2026-09-20): art-wiring.md 1절 규칙 — 그림은 Resources 아래에 있으니 런타임에
        // 경로로 부르고, 없으면 조용히 회색/투명 자리로 둔다(화면이 죽으면 안 된다). Resources.Load
        // 결과를 캐시해서 Update()에서 매 프레임 부르는 곳(등급 뱃지 등)이 있어도 매번 디스크·
        // 애셋 테이블을 다시 뒤지지 않게 한다. null도 같이 캐싱한다 — 없는 아이콘을 매번 다시
        // 찾지 않는다(그림이 나중에 들어와도 이 세션 재시작 전까지는 안 뜨지만, 이 프로젝트는
        // 씬을 다시 열 때마다 Awake가 새로 도니 실질적인 문제가 아니다).
        static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        /// <summary>Resources.Load에 그대로 넘길 수 있는 전체 경로(확장자 없음, "Art/..." 부터)로
        /// 스프라이트를 읽는다. A-17(2026-09-21): <c>PetArt.ResourcePath</c>처럼 폴더가 아이콘
        /// 밑이 아닌 곳(펫 등)도 있어 <see cref="LoadIcon"/>만으로는 못 부른다.</summary>
        public static Sprite LoadSpriteAtPath(string resourcePath)
        {
            if (SpriteCache.TryGetValue(resourcePath, out var cached)) return cached;
            var sp = Resources.Load<Sprite>(resourcePath);
            SpriteCache[resourcePath] = sp;
            return sp;
        }

        public static Sprite LoadIcon(string name) => LoadSpriteAtPath($"Art/Icons/{name}");

        /// <summary>root 아래 imageName인 Image를 찾아 아이콘을 입힌다. 이름을 못 찾거나
        /// 그림이 아직 안 들어왔으면 조용히 넘어간다(경고 없음 — 둘 다 정상 상태다).</summary>
        public static void SetIcon(Transform root, string imageName, string iconName)
        {
            SetSpriteAtPath(root, imageName, $"Art/Icons/{iconName}");
        }

        /// <summary>SetIcon과 같지만 "Art/Icons/" 밑으로 고정하지 않고 전체 경로를 받는다.
        /// A-17: 펫 뽑기 결과·도감 그리드가 <c>PetArt.ResourcePath(def)</c>를 그대로 넘겨 쓴다.</summary>
        public static void SetSpriteAtPath(Transform root, string imageName, string resourcePath)
        {
            var img = Find<Image>(root, imageName, false);
            if (img == null) return;
            var sp = LoadSpriteAtPath(resourcePath);
            if (sp == null) return;
            img.sprite = sp;
            img.color = Color.white;
        }
    }
}
