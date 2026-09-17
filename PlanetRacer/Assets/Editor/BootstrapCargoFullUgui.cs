using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-11(2026-09-17): 화물칸 상한 도달 화면(CargoFullPanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// 구성은 CargoFull.uxml 그대로다 — 제목 / 문구 / [레이스 나가기 · 닫기] / 상점 보기 /
    /// 광고 안내 · 광고 버튼 / 스타터 팩 칸.
    ///
    /// OfflineReward(메뉴 21)와 같은 구조를 쓴다. HUD 버튼으로 여닫는 화면이 아니라 상한에 닿으면
    /// 스스로 뜨는 화면이라, 루트 "CargoFull"은 항상 켜 두고(꺼 두면 Update가 안 돈다)
    /// 자식 "cargo-full-backdrop"만 스크립트가 여닫는다. UiPanel을 안 다는 이유도 같다.
    ///
    /// 버튼 순서는 monetization.md를 따른다 — 무료 해법(레이스 나가기)이 1순위고 상점은 그다음이다.
    /// 문구가 제련소 레벨에 따라 두세 줄로 길어지므로 message 라벨에는 LayoutElement를 달지 않고
    /// TMP가 계산한 높이를 그대로 쓴다(카드는 ContentSizeFitter로 같이 늘어난다).
    /// 멱등 — 다시 눌러도 기존 "CargoFull"을 지우고 같은 모양으로 다시 만든다.
    /// </summary>
    public static class BootstrapCargoFullUgui
    {
        static readonly Color Ink          = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim          = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Backdrop     = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color CardFace     = new Color(0.094f, 0.102f, 0.141f, 1f);
        static readonly Color PrimaryFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color PlainFace    = new Color(0.157f, 0.173f, 0.227f, 1f);
        static readonly Color CalloutFace  = new Color(0.129f, 0.141f, 0.196f, 1f);
        static readonly Color HighlightInk = new Color(0.824f, 0.784f, 0.353f);

        const float CardWidth = 380f;

        [MenuItem("GemRacer/24. 화물칸 가득 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("CargoFull");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // 루트는 항상 켜 둔다 — Update가 계속 돌아야 상한에 닿았을 때 스스로 뜬다.
            var root = NewRect("CargoFull", overlays.transform);
            Stretch(root);
            var ugui = root.gameObject.AddComponent<CargoFullUgui>();

            var backdrop = NewRect("cargo-full-backdrop", root);
            Stretch(backdrop);
            var bg = backdrop.gameObject.AddComponent<Image>();
            bg.color = Backdrop;
            bg.raycastTarget = true;

            var card = NewRect("cargo-full-card", backdrop);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot     = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(CardWidth, 240f); // 초기값. 아래 ContentSizeFitter가 다시 잡는다
            card.anchoredPosition = Vector2.zero;
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = CardFace;
            cardImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cardImg.type = Image.Type.Sliced;
            Column(card.gameObject, 24, 8f);
            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeText("cargo-full-title", "정제로 돌리시겠어요?", card, font, 20, Ink, 28f, TextAlignmentOptions.Center, false);
            // 높이를 고정하지 않는다 — 제련소 레벨에 따라 두 줄에서 네 줄까지 오간다.
            MakeText("cargo-full-message", "—", card, font, 14, Ink, 0f, TextAlignmentOptions.TopLeft, true);

            // 1순위 줄: 레이스 나가기(무료 해법) + 닫기.
            var buttonRow = NewRect("button-row", card);
            Row(buttonRow.gameObject, 8f);
            SetHeight(buttonRow.gameObject, 46f);
            MakeButton("race-button", "레이스 나가기", buttonRow, font, PrimaryFace, 16);
            MakeButton("close-button", "닫기", buttonRow, font, PlainFace, 16);

            // 그다음이 상점 — 강조하지 않는다(monetization.md: 무료 해법이 먼저다).
            var shopBtn = MakeButton("shop-button", "상점 보기", card, font, PlainFace, 14);
            SetHeight(shopBtn.gameObject, 40f);

            // M-09 후속: 오늘 한도가 남아 있을 때만 스크립트가 이 둘을 켠다.
            MakeText("ad-label", "—", card, font, 12, Dim, 34f, TextAlignmentOptions.MidlineLeft, true);
            var adBtn = MakeButton("ad-button", "광고 보고 1시간 상한 2배", card, font, PlainFace, 14);
            SetHeight(adBtn.gameObject, 40f);

            // M-08: 첫 상한 도달 직후에만 스크립트가 켜는 칸. 테두리 대신 한 톤 밝은 면으로 구분한다.
            var callout = NewRect("starter-pack-callout", card);
            var calloutImg = callout.gameObject.AddComponent<Image>();
            calloutImg.color = CalloutFace;
            calloutImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            calloutImg.type = Image.Type.Sliced;
            Column(callout.gameObject, 12, 8f);

            MakeText("starter-pack-message", "—", callout, font, 13, HighlightInk, 0f, TextAlignmentOptions.TopLeft, true);
            var packRow = NewRect("starter-pack-button-row", callout);
            Row(packRow.gameObject, 8f);
            SetHeight(packRow.gameObject, 42f);
            MakeButton("starter-pack-button", "스타터 팩 보기", packRow, font, PrimaryFace, 14);
            MakeButton("starter-pack-decline-button", "괜찮아요", packRow, font, PlainFace, 14);

            callout.gameObject.SetActive(false);  // 스크립트가 조건을 보고 켠다
            backdrop.gameObject.SetActive(false); // 상한에 닿기 전까지 숨겨 둔다(스크립트 Awake와 이중 안전장치)

            // 레이스·상점 패널은 같은 Overlays 아래에 이미 있다 — 있으면 미리 물려 준다.
            ugui.racePanel = FindPanel(overlays.transform, "RaceEntry");
            ugui.shopPanel = FindPanel(overlays.transform, "Shop");
            if (ugui.racePanel == null) Debug.LogWarning("[GemRacer] Overlays/RaceEntry를 못 찾아 '레이스 나가기'가 꺼진 채로 남는다. 'GemRacer/18' 먼저.");
            if (ugui.shopPanel == null) Debug.LogWarning("[GemRacer] Overlays/Shop을 못 찾아 '상점 보기'가 꺼진 채로 남는다. 'GemRacer/22' 먼저.");

            // 상점보다 **아래**에 둔다. uGUI는 형제 순서가 그리는 순서라, 맨 뒤에 그냥 붙이면
            // 이 화면이 상점 위에 그려져서 "상점 보기"를 눌러도 상점이 뒤에 가려 안 보인다
            // (옛 UI Toolkit 시절 BootstrapMainGame이 상점 sortingOrder를 21로 올려 둔 것과 같은 이유).
            if (ugui.shopPanel != null) root.SetSiblingIndex(ugui.shopPanel.transform.GetSiblingIndex());

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 화물칸 가득 화면(uGUI) 세움. HUD 버튼 없이 상한에 닿으면 스스로 뜬다.");
        }

        // --- 조각 만들기 ---------------------------------------------------

        static UiPanel FindPanel(Transform overlays, string name)
        {
            var t = overlays.Find(name);
            return t != null ? t.GetComponent<UiPanel>() : null;
        }

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

        static void Column(GameObject go, int pad, float spacing)
        {
            var col = go.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(pad, pad, pad, pad);
            col.spacing = spacing;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;
        }

        static void Row(GameObject go, float spacing)
        {
            var row = go.AddComponent<HorizontalLayoutGroup>();
            row.spacing = spacing;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            row.childControlWidth = true;
            row.childControlHeight = true;
        }

        static void SetHeight(GameObject go, float height)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
        }

        /// <summary>height가 0 이하면 LayoutElement를 달지 않는다 — TMP가 계산한 높이를 그대로 쓴다.</summary>
        static TMP_Text MakeText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                  float size, Color color, float height, TextAlignmentOptions align, bool wrap)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.enableWordWrapping = wrap;
            if (height > 0f) SetHeight(rt.gameObject, height);
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
            t.enableAutoSizing = true;
            t.fontSizeMin = 11f;
            t.fontSizeMax = fontSize;

            return btn;
        }
    }
}
