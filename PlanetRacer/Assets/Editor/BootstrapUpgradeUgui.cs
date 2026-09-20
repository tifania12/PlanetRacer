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

        // A-16: 아이콘 한 변과 그 아이콘이 들어가는 줄 높이.
        // 카드 안쪽 높이가 딱 맞아떨어지게 잡았다 — 머리줄 28 + 효과 60 + 버튼 44 +
        // 위아래 여백 24 + 줄 사이 12 = 168 = CellHeight. 여기를 키우면 카드가 넘친다.
        const float IconSize = 24f;
        const float HeadHeight = 28f;

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
            MakeCurrencyLine(root, font);

            var rowList = NewRect("row-list", root);
            var rowListLayout = rowList.gameObject.AddComponent<LayoutElement>();
            rowListLayout.flexibleHeight = 1f;
            var grid = rowList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            // 제련소가 맨 앞이다. 새 채굴차는 정제 광물이 0이라 아래 세 줄을 아예 못 누르고,
            // 제련소를 사야 정제가 흐르기 시작해서 나머지가 열린다(RigUpgrade.cs UpgradeSlot 주석).
            // 실제로 눌러야 하는 순서대로 놓는다 — 뒤에 두면 화면을 열었을 때 회색 버튼 셋이
            // 먼저 보이고 유일하게 누를 수 있는 것이 오른쪽 아래에 숨는다.
            // 시작 레벨이 0이라 다른 줄과 달리 Lv.0으로 적는다.
            // A-16: 제련소 전용 아이콘(icon-refinery)이 2026-09-20에 들어왔다(art-wiring.md 2절).
            MakeRow(rowList, font, "refinery", "제련소 Lv.0", "icon-refinery");
            MakeRow(rowList, font, "tool", "곡괭이 Lv.1", "icon-gear-tool");
            MakeRow(rowList, font, "cargo", "화물칸 Lv.1", "icon-gear-cargo");
            MakeRow(rowList, font, "engine", "엔진 Lv.1", "icon-gear-engine");

            // 닫기 버튼. 이 패널은 화면을 꽉 채우고 뒤로 클릭이 새지 않게 막기 때문에,
            // 이게 없으면 한 번 열었을 때 HUD의 "업그레이드" 버튼도 가려져서 빠져나올 길이 없다
            // (2026-09-15 Unity 세션에서 실제로 막혔다). onClick은 인스펙터에 보이는
            // 영구 리스너로 걸어 둔다 — Tifania가 눈으로 보고 바꿀 수 있어야 한다.
            var closeBtn = MakeButton("close-button", "닫기", root, font);
            closeBtn.GetComponent<Image>().color = RowFace;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 업그레이드 화면(uGUI) 세움. MainHudUgui.upgradePanel에 이 'Upgrade'를 물려야 " +
                      "HUD의 '업그레이드' 버튼으로 실제로 열린다.");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, string prefix, string levelText, string iconName)
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

            // A-16(2026-09-20): 레벨 글자 앞에 아이콘 자리를 하나 둔다. 이름은 기존 규칙을 따라
            // `tool-level` 옆이면 `tool-icon`이다(docs/design/art-wiring.md 2절 표).
            // 그림을 넣는 건 UpgradeUgui가 Awake에서 한다 — 여기서는 자리와 이름만 만든다.
            var head = MakeLine($"{prefix}-head", row, HeadHeight, 8f);
            MakeIcon($"{prefix}-icon", head, IconSize);
            MakeHeaderText($"{prefix}-level", levelText, head, font, 18, Ink, 26f);

            var effect = MakeHeaderText($"{prefix}-effect", "다음: —", row, font, 13, Dim, 60f);
            effect.enableWordWrapping = true;
            effect.alignment = TextAlignmentOptions.TopLeft;

            MakeButton($"{prefix}-button", "업그레이드", row, font);
        }

        // A-16(2026-09-20): 머리글의 화폐 줄. 전에는 "원석 0.0 · 정제 광물 0.0" 한 덩어리였는데,
        // 화폐가 둘이라 아이콘도 둘이어야 해서 [그림][글자] 두 쌍으로 나눈다.
        // 옛 이름 `currency-label`을 찾던 빌드도 그대로 돌게 UpgradeUgui 쪽에 대비를 남겨 뒀다.
        static void MakeCurrencyLine(RectTransform parent, TMP_FontAsset font)
        {
            var line = MakeLine("currency-line", parent, 26f, 6f);
            MakeIcon("currency-raw-icon", line, 22f);
            MakeHeaderText("currency-raw-label", "원석 0.0", line, font, 17, Currency, 24f);
            MakeSpacer(line);
            MakeIcon("currency-refined-icon", line, 22f);
            MakeHeaderText("currency-refined-label", "정제 광물 0.0", line, font, 17, Currency, 24f);
        }

        // --- 조각 만들기 ---------------------------------------------------

        /// <summary>가로로 늘어놓는 한 줄. 아이콘과 글자를 나란히 놓을 때 쓴다.</summary>
        static RectTransform MakeLine(string name, RectTransform parent, float height, float spacing)
        {
            var rt = NewRect(name, parent);
            var row = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = spacing;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            row.childControlWidth = true;
            row.childControlHeight = true;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return rt;
        }

        /// <summary>아이콘 자리. 그림은 런타임에 패널 스크립트가 넣는다(art-wiring.md 1절).
        /// 여기서는 유니티 기본 스프라이트를 자리 표시자로 두어, 그림이 아직 없어도
        /// 줄 간격이 흔들리지 않게 한다.</summary>
        static Image MakeIcon(string name, RectTransform parent, float size)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minWidth = size;  le.preferredWidth = size;  le.flexibleWidth = 0f;
            le.minHeight = size; le.preferredHeight = size; le.flexibleHeight = 0f;
            return img;
        }

        /// <summary>가로 줄에서 남는 자리를 밀어내는 빈 칸.</summary>
        static void MakeSpacer(RectTransform parent)
        {
            var rt = NewRect("spacer", parent);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 8f;
            le.flexibleWidth = 1f;
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
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return t;
        }

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font)
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

            return btn;
        }
    }
}
