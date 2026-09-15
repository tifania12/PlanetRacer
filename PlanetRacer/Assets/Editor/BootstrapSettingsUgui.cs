using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-06(2026-09-16): 설정 화면(SettingsPanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD의 "설정" 버튼이 여닫는다(BootstrapHudUgui,
    /// `MainHudUgui.settingsPanel`에 이 패널을 물려야 실제로 열린다 — 씬 배선은 Unity 세션 몫).
    ///
    /// 다른 오버레이 화면(U-02~U-05)과 달리 줄마다 내용이 다르다(소리 켜짐/꺼짐 한 줄, 프레임
    /// 두 버튼 한 줄, 피드백은 여러 줄짜리 입력칸) — 그래서 GridLayoutGroup으로 카드를 맞추지
    /// 않고 VerticalLayoutGroup으로 세 줄을 그냥 쌓는다. 손계산으로 세로 한 칸 기준 내용 높이가
    /// 제목 32 + 줄 세 개(약 70+70+190) + 닫기 44 + 안팎 여백을 더해도 400대 초반이라, U-03이
    /// 실제로 넘쳤던 840대 가용 높이에 한참 못 미친다 — 그래서 이번엔 ScrollRect 없이 시작한다.
    /// 그래도 **Editor에서 세 기준점 다 실제로 봐야 확실하다** (U-03 전례).
    /// </summary>
    public static class BootstrapSettingsUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace  = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color FieldFace = new Color(0.10f, 0.11f, 0.16f, 1f);
        static readonly Color StatusInk = new Color(0.59f, 0.86f, 0.59f);

        [MenuItem("GemRacer/20. 설정 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Settings");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("Settings", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<SettingsUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("settings-title", "설정", root, font, 24, Ink, 32f);

            var rowList = NewRect("row-list", root);
            var rowListCol = rowList.gameObject.AddComponent<VerticalLayoutGroup>();
            rowListCol.spacing = 12f;
            rowListCol.childAlignment = TextAnchor.UpperLeft;
            rowListCol.childForceExpandWidth = true;
            rowListCol.childForceExpandHeight = false;
            rowListCol.childControlWidth = true;
            rowListCol.childControlHeight = true;
            var rowListFitter = rowList.gameObject.AddComponent<ContentSizeFitter>();
            rowListFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeSoundRow(rowList, font);
            MakeFrameRateRow(rowList, font);
            MakeFeedbackRow(rowList, font);

            // 닫기 버튼. 이 패널도 화면을 꽉 채우고 뒤로 클릭을 막기 때문에, 이게 없으면 한 번
            // 열었을 때 HUD의 "설정" 버튼까지 가려져서 빠져나올 길이 없다(ugui-migration.md 3-1번).
            // onClick은 인스펙터 OnClick 칸에 보이는 영구 리스너로 걸어 둔다.
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 설정 화면(uGUI) 세움. MainHudUgui.settingsPanel에 이 'Settings'를 물려야 " +
                      "HUD의 '설정' 버튼으로 실제로 열린다.");
        }

        static void MakeSoundRow(RectTransform parent, TMP_FontAsset font)
        {
            var row = MakeRowBase("row-sound", parent, 44f);

            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var nameLabel = MakeHeaderText("sound-name", "소리", row, font, 16, Ink, 0f);
            var nameLe = nameLabel.gameObject.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;

            var btn = MakeButton("sound-button", "켜짐", row, font, BtnFace);
            var btnLe = btn.gameObject.AddComponent<LayoutElement>();
            btnLe.minWidth = 70f;
            btnLe.preferredWidth = 70f;
        }

        static void MakeFrameRateRow(RectTransform parent, TMP_FontAsset font)
        {
            var row = MakeRowBase("row-framerate", parent, 44f);

            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var nameLabel = MakeHeaderText("framerate-name", "프레임", row, font, 16, Ink, 0f);
            var nameLe = nameLabel.gameObject.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;

            var fps30 = MakeButton("fps30-button", "30", row, font, BtnFace);
            var fps30Le = fps30.gameObject.AddComponent<LayoutElement>();
            fps30Le.minWidth = 60f;
            fps30Le.preferredWidth = 60f;

            var fps60 = MakeButton("fps60-button", "60", row, font, BtnFace);
            var fps60Le = fps60.gameObject.AddComponent<LayoutElement>();
            fps60Le.minWidth = 60f;
            fps60Le.preferredWidth = 60f;
        }

        static void MakeFeedbackRow(RectTransform parent, TMP_FontAsset font)
        {
            var row = MakeRowBase("row-feedback", parent, 190f);

            var col = row.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(14, 14, 12, 12);
            col.spacing = 6f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("feedback-name", "피드백 (자유롭게 적어 주세요)", row, font, 14, Ink, 20f);
            MakeMultilineField("feedback-field", row, font, 72f);

            var saveBtn = MakeButton("feedback-save-button", "저장", row, font, BtnFace);
            var saveLe = saveBtn.gameObject.AddComponent<LayoutElement>();
            saveLe.minHeight = 36f;
            saveLe.preferredHeight = 36f;

            MakeHeaderText("feedback-status", "", row, font, 12, StatusInk, 18f);
        }

        // --- 조각 만들기 ---------------------------------------------------

        static RectTransform MakeRowBase(string name, RectTransform parent, float minHeight)
        {
            var row = NewRect(name, parent);
            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = minHeight;
            return row;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static TMP_Text MakeHeaderText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                       float size, Color color, float height)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            if (height > 0f)
            {
                var le = rt.gameObject.AddComponent<LayoutElement>();
                le.minHeight = height;
                le.preferredHeight = height;
            }
            return t;
        }

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font, Color face)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = face;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;

            return btn;
        }

        /// <summary>여러 줄 입력칸. TMP_InputField는 textViewport/textComponent가 있어야 하고,
        /// placeholder는 없어도 동작하지만 빈 칸일 때 안내가 없으면 어색해서 같이 만든다.</summary>
        static TMP_InputField MakeMultilineField(string name, RectTransform parent, TMP_FontAsset font, float minHeight)
        {
            var rt = NewRect(name, parent);
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = FieldFace;
            bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bg.type = Image.Type.Sliced;

            var field = rt.gameObject.AddComponent<TMP_InputField>();
            field.targetGraphic = bg;
            field.lineType = TMP_InputField.LineType.MultiLineNewline;

            var viewport = NewRect("text-area", rt);
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(8f, 6f);
            viewport.offsetMax = new Vector2(-8f, -6f);
            viewport.gameObject.AddComponent<RectMask2D>();

            var textRt = NewRect("text", viewport);
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero; textRt.offsetMax = Vector2.zero;
            var text = textRt.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = 14;
            text.color = Ink;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.raycastTarget = false;

            var placeholderRt = NewRect("placeholder", viewport);
            placeholderRt.anchorMin = Vector2.zero; placeholderRt.anchorMax = Vector2.one;
            placeholderRt.offsetMin = Vector2.zero; placeholderRt.offsetMax = Vector2.zero;
            var placeholder = placeholderRt.gameObject.AddComponent<TextMeshProUGUI>();
            placeholder.font = font;
            placeholder.text = "생각을 적어 주세요";
            placeholder.fontSize = 14;
            placeholder.color = Dim;
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.alignment = TextAlignmentOptions.TopLeft;
            placeholder.enableWordWrapping = true;
            placeholder.raycastTarget = false;

            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = placeholder;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = minHeight;

            return field;
        }
    }
}
