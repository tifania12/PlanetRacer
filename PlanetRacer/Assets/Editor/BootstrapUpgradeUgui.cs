using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-02(2026-09-15): 업그레이드 화면(UpgradePanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD의 "업그레이드" 버튼이 여닫는다(BootstrapHudUgui,
    /// `MainHudUgui.upgradePanel`에 이 패널을 물려야 실제로 열린다 — 씬 배선은 Unity 세션 몫).
    ///
    /// 세 줄(곡괭이/화물칸/엔진)을 GridLayoutGroup(Flexible)에 담아서 CLAUDE.md 6번 반응형
    /// 규칙("세로 기준, 가로가 넓어지면 한 칸을 두 칸으로")을 화면 크기를 직접 읽지 않고 만족한다 —
    /// CanvasScaler(참조 540x960, matchWidthOrHeight 0.5)가 만드는 캔버스 로컬 좌표 기준으로
    /// 세로 500폭에는 카드(400)+여백(12)=412가 하나만 들어가고, 가로 920·태블릿 871폭에는
    /// 둘이 들어간다(계산은 docs/design/ugui-migration.md 옆에 남긴 daily 메모 참고).
    /// </summary>
    public static class BootstrapUpgradeUgui
    {
        static readonly Color Ink       = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim       = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg        = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace   = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace   = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color Currency  = new Color(0.78f, 0.78f, 0.31f);

        const float CellWidth = 400f;
        const float CellHeight = 168f;
        const float CellSpacing = 12f;

        [MenuItem("GemRacer/16. 업그레이드 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Upgrade");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("Upgrade", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<UpgradeUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("upgrade-title", "채굴 장비 업그레이드", root, font, 24, Ink, 32f);
            MakeHeaderText("currency-label", "정제 광물 0.0", root, font, 17, Currency, 24f);

            var rowList = NewRect("row-list", root);
            var rowListLayout = rowList.gameObject.AddComponent<LayoutElement>();
            rowListLayout.flexibleHeight = 1f;
            var grid = rowList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            MakeRow(rowList, font, "tool", "곡괭이 Lv.1");
            MakeRow(rowList, font, "cargo", "화물칸 Lv.1");
            MakeRow(rowList, font, "engine", "엔진 Lv.1");

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 업그레이드 화면(uGUI) 세움. MainHudUgui.upgradePanel에 이 'Upgrade'를 물려야 " +
                      "HUD의 '업그레이드' 버튼으로 실제로 열린다.");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, string prefix, string levelText)
        {
            var row = NewRect($"row-{prefix}", parent);

            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var col = row.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(14, 14, 12, 12);
            col.spacing = 6f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText($"{prefix}-level", levelText, row, font, 18, Ink, 26f);
            var effect = MakeHeaderText($"{prefix}-effect", "다음: —", row, font, 13, Dim, 60f);
            effect.enableWordWrapping = true;
            effect.alignment = TextAlignmentOptions.TopLeft;

            MakeButton($"{prefix}-button", "업그레이드", row, font);
        }

        // --- 조각 만들기 ---------------------------------------------------

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
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
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
            le.minHeight = 44f;
            le.preferredHeight = 44f;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 16;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
        }
    }
}
