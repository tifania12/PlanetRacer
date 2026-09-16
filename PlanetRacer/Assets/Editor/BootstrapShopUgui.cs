using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-10(2026-09-16): 상점 화면(ShopPanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD의 "상점" 버튼이 여닫는다(BootstrapHudUgui,
    /// `MainHudUgui.shopPanel`에 이 패널을 물려야 실제로 열린다 — 씬 배선은 Unity 세션 몫).
    ///
    /// 아홉 줄(스타터 팩/화물칸 확장 3단계/오프라인 상한 연장/채굴 가속 패스/행성 통행증 구독/
    /// Steam 서포터 팩/광고 제거)을 BootstrapLootBoxUgui와 같은 줄 구조(이름/상태/버튼 하나)로
    /// 담는다. 다섯 줄짜리 부품 제작 화면도 세로 한 칸에서 넘쳐서(998px > ~840px) 목록을
    /// ScrollRect로 감싸야 했다(U-03) — 아홉 줄은 그보다 훨씬 크니 처음부터
    /// BootstrapCraftingUgui와 같은 ScrollRect 구조로 만든다. 닫기 버튼은 스크롤 바깥이라
    /// 항상 보인다.
    /// </summary>
    public static class BootstrapShopUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace  = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);

        // BootstrapHudUgui.cs의 BtnFace와 같은 값 — HUD 버튼 다섯 개와 톤을 맞추려고 따로 둔다
        // (Shop 화면 안의 줄 버튼(BtnFace, 위)과는 쓰임이 달라 섞으면 안 된다).
        static readonly Color HudBtnFace = new Color(0.16f, 0.18f, 0.30f, 0.95f);

        const float CellWidth = 400f;
        const float CellHeight = 140f;
        const float CellSpacing = 12f;

        static readonly (string prefix, string label)[] Rows =
        {
            ("starter", "스타터 팩"), ("cargo1", "화물칸 확장 1단계"), ("cargo2", "화물칸 확장 2단계"),
            ("cargo3", "화물칸 확장 3단계"), ("offlinecap", "오프라인 상한 연장"), ("accel", "채굴 가속 패스"),
            ("season", "행성 통행증 구독"), ("steam", "Steam 서포터 팩"), ("adremoval", "광고 제거"),
        };

        [MenuItem("GemRacer/22. 상점 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Shop");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("Shop", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<ShopUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("shop-title", "상점 (테스트 구매)", root, font, 24, Ink, 32f);

            // 아홉 줄은 세로 화면에 다 안 들어간다(U-03에서 다섯 줄도 넘쳤다). 목록만
            // 스크롤로 감싼다 — 닫기 버튼은 스크롤 바깥에 둬서 항상 보이게 한다
            // (ugui-migration.md 3-1). 칸 수 계산은 스크롤바를 띄우지 않아 목록 폭이 그대로다.
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

            foreach (var (prefix, label) in Rows)
                MakeRow(rowList, font, prefix, label);

            // 닫기 버튼. 화면을 꽉 채우고 뒤로 클릭을 막기 때문에, 이게 없으면 한 번 열었을 때
            // HUD의 "상점" 버튼까지 가려져서 빠져나올 길이 없다(ugui-migration.md 3-1번).
            // onClick은 인스펙터 OnClick 칸에 보이는 영구 리스너로 걸어 둔다.
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 상점 화면(uGUI) 세움. MainHudUgui.shopPanel에 이 'Shop'을 물려야 " +
                      "HUD의 '상점' 버튼으로 실제로 열린다.");
        }

        /// <summary>
        /// U-10: HUD 액션 줄(BootstrapHudUgui가 만든 다섯 버튼)에 "상점" 버튼을 하나 더 끼워 넣는다.
        /// **일부러 GemRacer/13을 다시 실행하지 않는다** — 그 메뉴는 "UI Canvas" 전체를
        /// 통째로 지우고 다시 세우는데(BootstrapHudUgui.Build, "이미 있으면 통째로 지우고 다시
        /// 만든다"), 그 밑의 `Overlays`에는 U-02~U-07이 각 세션에서 하나씩 배선해 둔 화면들이
        /// 자식으로 들어 있다. UI Canvas를 지우면 그 화면들도 같이 사라지고, MainHudUgui의
        /// craftPanel/racePanel/boxPanel/settingsPanel 연결(인스펙터에만 있는 씬 데이터)도
        /// 전부 다시 해야 한다 — 지금까지 배선한 세션 다섯 번어치가 날아간다. 그래서 이 메뉴는
        /// `action-row`만 찾아서 그 밑에 "btn-shop" 하나만 추가한다. 같은 메뉴를 다시 눌러도
        /// 기존 btn-shop을 지우고 새로 만들 뿐이라(CLAUDE.md 3번, 멱등) 안전하다.
        /// </summary>
        [MenuItem("GemRacer/23. HUD에 상점 버튼 추가 (uGUI, 안전 — HUD만 건드림)")]
        public static void AddShopButtonToActionRow()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var actionRowGo = GameObject.Find("UI Canvas/HUD/action-row");
            if (actionRowGo == null) { Debug.LogError("[GemRacer] 'UI Canvas/HUD/action-row'가 없다. 'GemRacer/13' 먼저."); return; }
            var actionRow = (RectTransform)actionRowGo.transform;

            var old = actionRow.Find("btn-shop");
            if (old != null) Object.DestroyImmediate(old.gameObject); // 멱등 — 다시 눌러도 결과가 같다

            // BootstrapHudUgui.MakeButton과 같은 모양(LayoutElement 없이 HorizontalLayoutGroup의
            // childControlWidth/Height에 맡긴다) — 다섯 버튼과 같은 방식으로 여섯 등분되게 한다.
            var rt = NewRect("btn-shop", actionRow);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = HudBtnFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = "상점";
            t.fontSize = 20;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            // BootstrapHudUgui.MakeButton과 같은 자동 축소. 여섯 번째 버튼이 붙으면 한 칸이
            // 79.3px로 줄어서 "업그레이드"가 잘린다 — 그래서 여기서 옆 다섯 개도 같이 손본다.
            t.enableAutoSizing = true;
            t.fontSizeMin = 14f;
            t.fontSizeMax = 20f;

            // 이미 씬에 있던 다섯 버튼도 같은 설정으로 맞춘다(멱등, 라벨만 건드린다).
            foreach (Transform sibling in actionRow)
            {
                var sl = sibling.Find("label");
                if (sl == null) continue;
                var st = sl.GetComponent<TextMeshProUGUI>();
                if (st == null || st == t) continue;
                st.enableAutoSizing = true;
                st.fontSizeMin = 14f;
                st.fontSizeMax = 20f;
            }

            EditorUtility.SetDirty(actionRowGo);
            Debug.Log("[GemRacer] HUD action-row에 'btn-shop' 추가함. MainHudUgui.shopPanel에 'Shop'을 " +
                      "물려야 버튼이 켜진다(MainHudUgui가 Awake에서 패널 없으면 자동으로 끈다).");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, string prefix, string nameText)
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

            MakeHeaderText($"{prefix}-name", nameText, row, font, 17, Ink, 24f);
            MakeHeaderText($"{prefix}-state", "미보유", row, font, 14, Dim, 20f);
            MakeButton($"{prefix}-button", "구매", row, font, BtnFace);
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
            t.overflowMode = TextOverflowModes.Ellipsis;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
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

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.preferredHeight = 44f;

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
