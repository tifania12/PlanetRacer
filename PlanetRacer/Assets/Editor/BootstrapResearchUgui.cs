using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// E-06 ②(2026-10-04): 연구소 화면을 `UI Canvas/Overlays` 아래에 세운다.
    /// BootstrapDailyLoginRewardUgui와 같은 구조 — 루트 "Research"는 항상 켜 두고 스크립트를 붙이고,
    /// 실제 화면은 자식 "research-backdrop" 하나라 ResearchUgui.Open/Close가 그것만 여닫는다.
    /// 같은 메뉴를 다시 눌러도 옛 "Research"를 지우고 새로 세워서 결과가 같다(멱등).
    /// 여는 버튼은 만들지 않는다 — 강화 화면 안 탭 버튼의 onClick에 ResearchUgui.Open을 거는 건
    /// Unity 세션이 하고, 거기까지는 `GemRacer/40`을 누른 뒤 씬을 저장하면 된다.
    /// </summary>
    public static class BootstrapResearchUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.62f, 0.66f, 0.78f);
        static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color CardFace = new Color(0.094f, 0.102f, 0.141f, 1f);
        static readonly Color RowFace  = new Color(0.14f, 0.15f, 0.21f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color Gold     = new Color(0.824f, 0.784f, 0.353f);

        const float CardWidth = 440f;

        [MenuItem("GemRacer/40. 연구소 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Research");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("Research", overlays.transform);
            Stretch(root);
            root.gameObject.AddComponent<ResearchUgui>();

            var backdrop = NewRect("research-backdrop", root);
            Stretch(backdrop);
            var bg = backdrop.gameObject.AddComponent<Image>();
            bg.color = Backdrop;
            bg.raycastTarget = true;

            var card = NewRect("research-card", backdrop);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(CardWidth, 400f); // 높이는 아래 ContentSizeFitter가 다시 잡는다
            card.anchoredPosition = Vector2.zero;
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = CardFace;
            cardImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cardImg.type = Image.Type.Sliced;

            var col = card.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeText("research-title", "연구소", card, font, 20, Ink, 28f, TextAlignmentOptions.Center);
            MakeText("research-money-label", "—", card, font, 14, Gold, 22f, TextAlignmentOptions.MidlineLeft);

            for (var i = 0; i < ResearchUgui.RowCount; i++) BuildRow(i, card, font);

            var close = MakeButton("research-close-button", "닫기", card, font, BtnFace, 16);
            var le = close.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 44f; le.preferredHeight = 44f;

            backdrop.gameObject.SetActive(false);

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 연구소 화면(uGUI) 세움. ResearchUgui.Open()을 부를 버튼은 아직 없다.");
        }

        static void BuildRow(int index, RectTransform card, TMP_FontAsset font)
        {
            var row = NewRect($"research-row-{index}", card);
            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var rowLe = row.gameObject.AddComponent<LayoutElement>();
            rowLe.minHeight = 64f; rowLe.preferredHeight = 64f;

            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(10, 10, 6, 6);
            h.spacing = 10f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = true;
            h.childControlWidth = true;
            h.childControlHeight = true;

            var texts = NewRect("texts", row);
            var tle = texts.gameObject.AddComponent<LayoutElement>();
            tle.flexibleWidth = 1f;
            var v = texts.gameObject.AddComponent<VerticalLayoutGroup>();
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.childControlWidth = true;
            v.childControlHeight = true;
            MakeText("name", "—", texts, font, 15, Ink, 20f, TextAlignmentOptions.MidlineLeft);
            MakeText("level", "—", texts, font, 12, Dim, 16f, TextAlignmentOptions.MidlineLeft);
            MakeText("effect", "—", texts, font, 12, Dim, 16f, TextAlignmentOptions.MidlineLeft);

            var right = NewRect("right", row);
            var rle = right.gameObject.AddComponent<LayoutElement>();
            rle.minWidth = 120f; rle.preferredWidth = 120f;
            var rv = right.gameObject.AddComponent<VerticalLayoutGroup>();
            rv.spacing = 4f;
            rv.childForceExpandWidth = true;
            rv.childForceExpandHeight = false;
            rv.childControlWidth = true;
            rv.childControlHeight = true;
            MakeText("status", "—", right, font, 11, Gold, 28f, TextAlignmentOptions.Center);
            var btn = MakeButton("start-button", "연구", right, font, BtnFace, 13);
            var ble = btn.gameObject.AddComponent<LayoutElement>();
            ble.minHeight = 26f; ble.preferredHeight = 26f;
        }

        // --- 조각 만들기 (BootstrapDailyLoginRewardUgui와 같은 모양) ---------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static TMP_Text MakeText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                 float size, Color color, float height, TextAlignmentOptions align)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return t;
        }

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font, Color face, float fontSize)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = face;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelRt = NewRect("label", rt);
            Stretch(labelRt);
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return btn;
        }
    }
}
