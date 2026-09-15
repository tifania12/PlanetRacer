using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-03(2026-09-15): 부품 제작 화면(CraftingPanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD의 "제작" 버튼이 여닫는다(BootstrapHudUgui,
    /// `MainHudUgui.craftPanel`에 이 패널을 물려야 실제로 열린다 — 씬 배선은 Unity 세션 몫).
    ///
    /// 다섯 줄(엔진/타이어/서스펜션/차체/부스터)을 BootstrapUpgradeUgui와 같은 방식으로
    /// GridLayoutGroup(Flexible)에 담아 CLAUDE.md 6번 반응형 규칙("세로 기준, 가로가 넓어지면
    /// 한 칸을 두 칸으로")을 화면 크기를 직접 읽지 않고 만족한다. 한 줄에 담을 내용이 업그레이드
    /// 화면보다 많다(이름+상태, 강화 단계, 제작/장착/해제 버튼, 강화 버튼) — 그래서 줄 높이를
    /// 더 크게 잡았다. 씬에서 실제로 다섯 줄이 세로 한 칸일 때 화면 안에 다 들어오는지는
    /// Unity 세션이 Play로 확인해야 한다(에디터가 없어 손계산만 했다, docs/design/ugui-migration.md
    /// 옆 daily 메모의 U-02 계산과 같은 CanvasScaler 기준).
    /// </summary>
    public static class BootstrapCraftingUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace  = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color EnhFace  = new Color(0.34f, 0.24f, 0.5f, 1f);
        static readonly Color Currency = new Color(0.78f, 0.78f, 0.31f);

        const float CellWidth = 400f;
        const float CellHeight = 190f;
        const float CellSpacing = 12f;

        static readonly (string prefix, string label)[] Rows =
        {
            ("engine", "엔진"), ("tire", "타이어"), ("suspension", "서스펜션"),
            ("body", "차체"), ("booster", "부스터"),
        };

        [MenuItem("GemRacer/17. 부품 제작 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Crafting");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("Crafting", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<CraftingUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("crafting-title", "레이싱카 부품 제작", root, font, 24, Ink, 32f);
            MakeHeaderText("currency-label", "정제 광물 0.0", root, font, 17, Currency, 24f);

            var rowList = NewRect("row-list", root);
            var rowListLayout = rowList.gameObject.AddComponent<LayoutElement>();
            rowListLayout.flexibleHeight = 1f;
            var grid = rowList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            foreach (var (prefix, label) in Rows)
                MakeRow(rowList, font, prefix, label);

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 부품 제작 화면(uGUI) 세움. MainHudUgui.craftPanel에 이 'Crafting'을 물려야 " +
                      "HUD의 '제작' 버튼으로 실제로 열린다.");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, string prefix, string nameText)
        {
            var row = NewRect($"row-{prefix}", parent);

            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var col = row.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(14, 14, 10, 10);
            col.spacing = 5f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            // 첫 줄 — 이름(왼쪽) + 상태(오른쪽)
            var head = NewRect($"{prefix}-head", row);
            var headLayout = head.gameObject.AddComponent<LayoutElement>();
            headLayout.minHeight = 24f; headLayout.preferredHeight = 24f;
            var headRow = head.gameObject.AddComponent<HorizontalLayoutGroup>();
            headRow.childAlignment = TextAnchor.MiddleLeft;
            headRow.childForceExpandWidth = true;
            headRow.childForceExpandHeight = true;
            headRow.childControlWidth = true;
            headRow.childControlHeight = true;

            var nameLabel = MakeInlineText($"{prefix}-name", nameText, head, font, 18, Ink);
            nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var stateLabel = MakeInlineText($"{prefix}-state", "미보유", head, font, 14, Dim);
            stateLabel.alignment = TextAlignmentOptions.MidlineRight;

            MakeHeaderText($"{prefix}-enhance-level", "+0", row, font, 13, Dim, 20f);

            // 둘째 줄 — 제작/장착/해제 버튼(넓게) + 강화 버튼(좁게)
            var buttonRow = NewRect($"{prefix}-buttons", row);
            var buttonRowLayout = buttonRow.gameObject.AddComponent<LayoutElement>();
            buttonRowLayout.minHeight = 44f; buttonRowLayout.preferredHeight = 44f;
            var buttonsLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 8f;
            buttonsLayout.childAlignment = TextAnchor.MiddleLeft;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;

            MakeButton($"{prefix}-button", "제작", buttonRow, font, BtnFace);
            MakeButton($"{prefix}-enhance-button", "강화", buttonRow, font, EnhFace);
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

        // MakeHeaderText와 달리 LayoutElement를 안 단다 — HorizontalLayoutGroup 자식으로
        // 너비를 나눠 가지게 두기 위해서다(이름 칸은 넓게, 상태 칸은 좁게 자동으로 맞춰진다).
        static TMP_Text MakeInlineText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                       float size, Color color)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return t;
        }

        static void MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font, Color face)
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
        }
    }
}
