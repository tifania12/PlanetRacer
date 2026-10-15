using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>A-28-2 꼬리(2026-10-16 07시): 화면 제목 글자 뒤에 **리본**(`ui-popup-title`)을 깐다.
    ///
    /// 지금 화면들의 제목은 글자 노드 하나뿐이다. 글자와 같은 GameObject에 Image를 더하면 둘이
    /// 한 CanvasRenderer를 다퉈 글자가 가려지므로, 리본을 깔려면 **글자 뒤에 배경 노드를 한 겹**
    /// 넣어야 한다. 계층을 바꾸는 일이라 런타임 함수(`UiSkin`)가 아니라 부트스트랩 몫이다 —
    /// 재화 칩(`GemRacer/44`)과 똑같은 분업이고, 그림을 얹는 일만 `UiSkin.ApplyTitleRibbons`가 한다.
    ///
    /// **각 화면의 `GemRacer/N`을 다시 누르지 않는다.** 그 메뉴들은 화면을 통째로 다시 만들어
    /// `MainHudUgui`의 패널 연결을 끊는다(ugui-migration.md 3-7). 그래서 이 메뉴도
    /// `GemRacer/23`·`44`와 같은 애처블(additive) 메뉴로, 제목 글자 하나하나만 감싼다.
    ///
    /// **화면 부트스트랩을 다시 누르면 그 화면의 리본은 사라진다**(계층을 새로 세우니까).
    /// 그때는 이 메뉴를 한 번 더 누르면 된다. 여러 번 눌러도 결과가 같다(멱등) —
    /// 아래 `Unwrap`이 이미 깔린 리본을 먼저 풀고 시작한다.
    ///
    /// 감싸는 대상은 **화면 제목**뿐이다. 이름이 `-title`로 끝나거나 `race-title-`로 시작하고
    /// 글자 크기가 20pt 이상인 것 — 24pt 전면 제목 열넷과 20pt 카드 제목 다섯, 모두 열아홉이다.
    /// 16pt 이하의 속제목(`advanced-title`·`tier3-title`·`result-title` 등)은 걸리지 않는다.
    /// 그것들은 줄·칸 안의 작은 글씨라 리본을 깔 자리가 아니다.</summary>
    public static class BootstrapPopupTitles
    {
        const string OverlaysPath = "UI Canvas/Overlays";

        /// <summary>제목 글자인지. 이름 규칙 + 크기 둘 다 봐야 한다 —
        /// 이름만 보면 `gacha-odds` 화면의 16pt `fusion-title`(속제목)까지 걸린다.</summary>
        static bool IsScreenTitle(TMP_Text t)
        {
            if (t == null) return false;
            var n = t.name;
            if (!n.EndsWith("-title") && !n.StartsWith("race-title-")) return false;
            return t.fontSize >= 20f;
        }

        [MenuItem("GemRacer/45. 팝업 제목 리본 (A-28-2, 안전 - 제목 글자만 감싼다)")]
        public static void WrapTitlesWithRibbon()
        {
            var overlaysGo = GameObject.Find(OverlaysPath);
            if (overlaysGo == null)
            {
                Debug.LogError($"[GemRacer] '{OverlaysPath}'가 없다. " +
                               "MainGame 씬을 열고 화면들을 먼저 세워라.");
                return;
            }
            var overlays = overlaysGo.transform;

            Unwrap(overlays);

            var made = 0;
            var names = new List<string>();
            foreach (var t in overlays.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!IsScreenTitle(t)) continue;
                Wrap(t);
                names.Add(t.name);
                made++;
            }

            // 그림을 얹는 일은 런타임과 같은 함수가 한다 — 한쪽만 고쳐서 어긋나는 일을 막는다
            // (HUD 칩·강화·상점과 같은 방식). 각 화면의 `Awake`도 같은 함수를 부른다.
            var skinned = UiSkin.ApplyTitleRibbons(overlays);

            EditorUtility.SetDirty(overlaysGo);
            Debug.Log($"[GemRacer] 제목 리본 {made}개 세움. 키트 스킨 {skinned}곳. " +
                      $"대상: {string.Join(", ", names)}. 그림이 아직 없으면 조용히 넘어간다.");
        }

        /// <summary>제목 글자 하나를 리본으로 감싼다. 글자의 자리(형제 순서)와 부모는 그대로 두고
        /// 그 자리에 리본을 끼운 뒤 글자를 안으로 넣는다.</summary>
        static void Wrap(TMP_Text title)
        {
            var titleRt = (RectTransform)title.transform;
            var parent = titleRt.parent;
            var at = titleRt.GetSiblingIndex();

            var ribbon = NewRect(title.name + "-ribbon", parent);
            ribbon.SetSiblingIndex(at);

            // **그림은 리본 자신이 아니라 뒤에 깔린 자식이 쓴다.** 리본에 직접 Image를 붙이면
            // Image도 레이아웃 요소라서 `preferredWidth`로 "테두리 둘의 합"(249)을 보고한다.
            // 칩에서 그 값 때문에 행성 이름이 찌그러졌던 것과 같은 함정이다(`BootstrapHudChips` 주석).
            // 배경을 레이아웃에서 빼면 리본 폭은 바깥 세로 줄이 주는 폭 그대로가 된다.
            var bg = NewRect(title.name + "-ribbon-bg", ribbon);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
            var img = bg.gameObject.AddComponent<Image>();
            img.raycastTarget = false;   // 제목이라 클릭을 먹을 이유가 없다
            img.color = Color.white;     // 그림에 색이 들어 있다. 흰색이라야 안 물든다
            bg.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var layout = ribbon.gameObject.AddComponent<HorizontalLayoutGroup>();
            // 좌우 여백은 보석 끝을 피하는 값이다. 테두리 125px이 46 높이에서 약 19.6단위가 되니
            // 26이면 글자가 끝에 올라타지 않는다(강화 화면 `currency-line`에 14를 준 것과 같은 이유).
            layout.padding = new RectOffset(26, 26, 4, 4);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            // 높이를 못 박고 flexible을 0으로 눌러 둔다(ugui-migration.md 3-2).
            // 글자 자신의 LayoutElement(32/28)는 안쪽에서 그대로 산다 — 줄이 늘어나지 않는다.
            var le = ribbon.gameObject.AddComponent<LayoutElement>();
            le.minHeight = UiSkin.TitleRibbonHeight;
            le.preferredHeight = UiSkin.TitleRibbonHeight;
            le.flexibleHeight = 0f;

            titleRt.SetParent(ribbon, false);
        }

        /// <summary>이미 깔린 리본을 풀어 제목 글자를 바깥으로 도로 꺼낸다. 멱등의 핵심이다.
        /// 배경 노드(`*-ribbon-bg`)는 리본과 함께 사라진다.</summary>
        static void Unwrap(Transform overlays)
        {
            var ribbons = new List<Transform>();
            foreach (var rt in overlays.GetComponentsInChildren<RectTransform>(true))
                if (rt.name.EndsWith("-ribbon")) ribbons.Add(rt);

            foreach (var ribbon in ribbons)
            {
                if (ribbon == null) continue;
                var parent = ribbon.parent;
                var at = ribbon.GetSiblingIndex();
                for (var i = ribbon.childCount - 1; i >= 0; i--)
                {
                    var child = ribbon.GetChild(i);
                    if (child.name.EndsWith("-ribbon-bg")) continue;   // 배경은 같이 사라진다
                    child.SetParent(parent, false);
                    child.SetSiblingIndex(at);
                }
                Object.DestroyImmediate(ribbon.gameObject);
            }
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
