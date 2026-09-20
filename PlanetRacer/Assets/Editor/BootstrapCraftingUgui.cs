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
    /// 더 크게 잡았다. 2026-09-15 Unity 세션에서 Play로 확인해 보니 세로 한 칸일 때 다섯 줄이
    /// 화면을 넘겨서(998px > 쓸 수 있는 ~840px) 목록을 ScrollRect로 감쌌다. 닫기 버튼은
    /// 스크롤 바깥이라 항상 보인다.
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

        // A-16: 다섯 부품 전부 전용 아이콘이 있다(차체·부스터는 2026-09-20에 들어왔다).
        static readonly (string prefix, string label, string icon)[] Rows =
        {
            ("engine", "엔진", "icon-part-engine"),
            ("tire", "타이어", "icon-part-tire"),
            ("suspension", "서스펜션", "icon-part-suspension"),
            ("body", "차체", "icon-part-body"),
            ("booster", "부스터", "icon-part-booster"),
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

            // E-02(2026-09-18): 정제 광물이 모자라 회색인 버튼이 있을 때만 CraftingUgui가 이
            // 줄에 안내를 채운다(비어 있으면 안 보이지만 자리는 늘 잡혀 있다 — 채워질 때 목록이
            // 밀리지 않게). scroll-view는 flexibleHeight로 남는 높이를 다 받으니, 이 줄이 차지하는
            // 28px(높이 20 + spacing 8)만큼 스크롤 뷰포트가 줄어들 뿐 — 이미 스크롤이라 잘리지 않는다.
            var hint = MakeHeaderText("hint-label", "", root, font, 13, Dim, 20f);
            hint.enableWordWrapping = true;

            // 다섯 줄은 세로 화면(540×960)에 다 안 들어간다. 한 칸일 때 목록 높이가
            // 5×190 + 4×12 = 998px인데 제목·재화·닫기를 빼면 쓸 수 있는 높이가 ~840px이라,
            // 부스터 줄이 잘리고 닫기 버튼이 화면 밖으로 밀려났다(2026-09-15 Unity 세션 Play로 확인).
            // 그래서 목록만 스크롤로 감싼다. 닫기 버튼은 스크롤 바깥에 둬서 어느 화면에서든
            // 항상 보이게 한다(ugui-migration.md 3-1). 칸 수 계산은 그대로다 — 스크롤바를
            // 띄우지 않아서 목록 폭이 전과 같기 때문이다.
            var scrollView = NewRect("scroll-view", root);
            var scrollLayout = scrollView.gameObject.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 120f;
            scrollLayout.preferredHeight = 120f; // 남는 높이는 flexibleHeight로 받는다
            scrollLayout.flexibleHeight = 1f;
            var viewImg = scrollView.gameObject.AddComponent<Image>();
            viewImg.color = new Color(1f, 1f, 1f, 0.02f);
            scrollView.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            var scroll = scrollView.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = scrollView;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            var rowList = NewRect("row-list", scrollView);
            rowList.anchorMin = new Vector2(0f, 1f);
            rowList.anchorMax = new Vector2(1f, 1f);
            rowList.pivot     = new Vector2(0.5f, 1f);
            rowList.offsetMin = Vector2.zero;
            rowList.offsetMax = Vector2.zero;
            scroll.content = rowList;

            var grid = rowList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            var rowListFit = rowList.gameObject.AddComponent<ContentSizeFitter>();
            rowListFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var (prefix, label, icon) in Rows)
                MakeRow(rowList, font, prefix, label, icon);

            // 닫기 버튼. 이 패널도 업그레이드 화면과 똑같이 화면을 꽉 채우고 뒤로 클릭을 막기 때문에,
            // 이게 없으면 한 번 열었을 때 HUD의 "제작" 버튼까지 가려져서 빠져나올 길이 없다
            // (ugui-migration.md 3-1번 규칙). onClick은 인스펙터 OnClick 칸에 보이는
            // 영구 리스너로 걸어 둔다 — Tifania가 눈으로 보고 바꿀 수 있어야 한다.
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 부품 제작 화면(uGUI) 세움. MainHudUgui.craftPanel에 이 'Crafting'을 물려야 " +
                      "HUD의 '제작' 버튼으로 실제로 열린다.");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, string prefix, string nameText, string iconName)
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

            // A-16: 부품 아이콘(있으면)을 이름 앞에, 등급 뱃지를 상태 뒤에 둔다.
            if (iconName != null) MakeIcon($"{prefix}-icon", head, 20f);
            var nameLabel = MakeInlineText($"{prefix}-name", nameText, head, font, 18, Ink);
            nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var stateLabel = MakeInlineText($"{prefix}-state", "미보유", head, font, 14, Dim);
            stateLabel.alignment = TextAlignmentOptions.MidlineRight;
            MakeIcon($"{prefix}-grade-badge", head, 18f);

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

        // A-16: 부품 아이콘·등급 뱃지 자리. 그림이 없는 동안은 투명(회색 박스 대신) —
        // CraftingUgui.cs가 Awake/매 프레임 UiKit.SetIcon으로 실제 스프라이트를 입힌다.
        static Image MakeIcon(string name, RectTransform parent, float size)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.color = new Color(1f, 1f, 1f, 0f);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minWidth = size; le.preferredWidth = size;
            le.minHeight = size; le.preferredHeight = size;
            return img;
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
    }
}
