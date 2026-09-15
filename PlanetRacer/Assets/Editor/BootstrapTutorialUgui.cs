using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-01(2026-09-15): 튜토리얼 말풍선을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD 위에 뜬다.
    ///
    /// 화면을 막지 않는다 — 루트에는 Image를 붙이지 않아서 레이캐스트 대상이 아니다.
    /// 그래서 말풍선 밖 클릭은 그대로 아래 HUD 버튼으로 통과한다.
    /// UI Toolkit판이 pickingMode로 하던 일을 여기서는 "Image를 안 붙인다"로 대신한다.
    /// </summary>
    public static class BootstrapTutorialUgui
    {
        static readonly Color Ink    = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim    = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bubble = new Color(0.07f, 0.08f, 0.14f, 0.94f);
        static readonly Color BtnFace= new Color(0.28f, 0.34f, 0.62f, 1f);

        [MenuItem("GemRacer/14. 튜토리얼 말풍선 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Tutorial");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // 루트 — 화면 전체에 깔리지만 Image가 없어서 클릭을 막지 않는다.
            var root = NewRect("Tutorial", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = false;      // 튜토리얼은 처음부터 보인다
            root.gameObject.AddComponent<TutorialUgui>();

            // 말풍선 — 화면 위쪽, 상태바/게이지 아래. 여기만 Image가 있어 클릭을 받는다.
            var bubble = NewRect("tutorial-bubble", root);
            bubble.anchorMin = new Vector2(0f, 1f);
            bubble.anchorMax = new Vector2(1f, 1f);
            bubble.pivot     = new Vector2(0.5f, 1f);
            bubble.offsetMin = new Vector2(16f, -280f);
            bubble.offsetMax = new Vector2(-16f, -108f);

            var bg = bubble.gameObject.AddComponent<Image>();
            bg.color = Bubble;
            bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bg.type = Image.Type.Sliced;
            bg.raycastTarget = true;   // 말풍선 위 클릭은 여기서 멈춘다

            var col = bubble.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(18, 18, 14, 14);
            col.spacing = 10f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeText("tutorial-step-label", "1 / 4", bubble, font, 18, Dim, 24f);
            var msg = MakeText("tutorial-message-label", "—", bubble, font, 21, Ink, 84f);
            msg.enableWordWrapping = true;
            msg.alignment = TextAlignmentOptions.TopLeft;

            MakeButton("tutorial-next-button", "다음", bubble, font);

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 튜토리얼 말풍선 세움. 루트에 Image가 없어서 밖은 클릭이 통과한다.");
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static TMP_Text MakeText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                 float size, Color color, float height)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font; t.text = text; t.fontSize = size; t.color = color;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;
            return t;
        }

        static void MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = BtnFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 48f; le.preferredHeight = 48f;

            var lrt = NewRect("label", rt);
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            var t = lrt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font; t.text = label; t.fontSize = 20; t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
        }
    }
}
