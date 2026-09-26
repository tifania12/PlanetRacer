using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-17(2026-09-21, pet-gacha.md 9-1): 펫 뽑기 실행 화면을 uGUI로 세운다.
    /// BootstrapPetGachaOddsUgui.cs(GemRacer/25)와 같은 패턴(NewRect/MakeText/MakeButton
    /// 로컬 헬퍼, 카드 = VerticalLayoutGroup+ContentSizeFitter, UiPanel{hiddenOnStart=true},
    /// ScrollRect로 감싸기) — 확률 공개 화면 바로 옆에 붙는 화면이라 그대로 따랐다.
    ///
    /// 다른 uGUI 화면들(PetGachaOddsUgui·ShopUgui·CraftingUgui)처럼 세로 한 칸 + ScrollRect로만
    /// 만든다 — CanvasScaler(기준 540x960, MatchWidthOrHeight 0.5)가 세 기준점(세로/가로/태블릿)
    /// 전부에서 읽히게 스케일을 맞춰 준다(ugui-migration.md "ResponsiveLayout.cs를 uGUI로
    /// 옮기지 않는다" 그대로) — 가로에서 2열로 직접 재배치하는 코드는 안 짠다.
    /// </summary>
    public static class BootstrapPetGachaPullUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 1f);
        static readonly Color CardFace = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color RowFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color AccentFace = new Color(0.36f, 0.28f, 0.62f, 1f);

        [MenuItem("GemRacer/26. 펫 뽑기 실행 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("PetGachaPull");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("PetGachaPull", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true;

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true;
            root.gameObject.AddComponent<PetGachaPullUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeText("pull-title", "펫 뽑기", root, font, 24, Ink, 32f, TextAlignmentOptions.MidlineLeft, false);

            // 재화 줄 — 원석 + 인장. 캡슐 카드·결과 패널과 같이 스크롤 안에 둔다(odds 화면과 같은
            // 이유, 화면 낮은 기기에서 전부 안 들어간다).
            var scrollView = NewRect("scroll-view", root);
            var scrollLayout = scrollView.gameObject.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 120f;
            scrollLayout.preferredHeight = 120f;
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

            var content = NewRect("content", scrollView);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot     = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            scroll.content = content;

            var contentCol = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentCol.spacing = 12f;
            contentCol.childAlignment = TextAnchor.UpperLeft;
            contentCol.childForceExpandWidth = true;
            contentCol.childForceExpandHeight = false;
            contentCol.childControlWidth = true;
            contentCol.childControlHeight = true;
            var contentFit = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeCurrencyRow(content, font);
            MakeCapsuleCard(content, font, "free", "무료 뽑기", "광고 시청 · 하루 10회", "free-limit");
            MakeCapsuleCard(content, font, "normal", "일반 뽑기", "레이싱 재화 · 천장 없음", null);
            MakeAdvancedCard(content, font);
            MakeCapsuleCard(content, font, "special", "특수 뽑기", "초월의 인장 전용", "special-pity");
            MakeSealPurchaseCard(content, font);
            MakeResultPanel(content, font);
            MakeFusionEntry(content, font);

            // 닫기 버튼. 스크롤 바깥에 고정(ugui-migration.md 3-1번, PetGachaOdds와 같은 자리).
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 펫 뽑기 실행 화면(uGUI) 세움. " +
                      "MainHudUgui에 여는 버튼을 아직 안 달았다 — PetGachaOddsUgui와 같은 처지라 " +
                      "지금은 Play 중 Hierarchy에서 직접 켜서 봐야 한다.");
        }

        static void MakeCurrencyRow(RectTransform parent, TMP_FontAsset font)
        {
            var card = MakeCardContainer("currency-card", parent);
            MakeText("currency-minerals", "원석 0.0", card, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, false);
            MakeText("currency-seals", "초월의 인장 0개", card, font, 14, Dim, 20f, TextAlignmentOptions.MidlineLeft, false);
        }

        static void MakeCapsuleCard(RectTransform parent, TMP_FontAsset font, string prefix, string title,
                                     string subtitle, string progressRowName)
        {
            var card = MakeCardContainer($"{prefix}-card", parent);
            MakeText($"{prefix}-title", title, card, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, false);
            MakeText($"{prefix}-subtitle", subtitle, card, font, 12, Dim, 18f, TextAlignmentOptions.MidlineLeft, false);

            var btn = MakeButton($"btn-pull-{prefix}", "뽑기", card, font, RowFace);
            var btnLayout = btn.gameObject.AddComponent<LayoutElement>();
            btnLayout.minHeight = 44f;
            btnLayout.preferredHeight = 44f;

            if (progressRowName != null)
                MakeText(progressRowName, "—", card, font, 12, Dim, 18f, TextAlignmentOptions.MidlineLeft, false);
        }

        static void MakeAdvancedCard(RectTransform parent, TMP_FontAsset font)
        {
            var card = MakeCardContainer("advanced-card", parent);
            MakeText("advanced-title", "고급 뽑기", card, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, false);
            MakeText("advanced-subtitle", "하루 1회 무료 + 유료", card, font, 12, Dim, 18f, TextAlignmentOptions.MidlineLeft, false);

            var row = NewRect("advanced-button-row", card);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            var rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.minHeight = 44f;
            rowElement.preferredHeight = 44f;

            MakeButton("btn-pull-advanced", "뽑기 1회", row, font, RowFace);
            MakeButton("btn-pull-advanced-ten", "10연차", row, font, RowFace);

            MakeText("advanced-pity", "—", card, font, 12, Dim, 18f, TextAlignmentOptions.MidlineLeft, false);
        }

        // P-16(2026-09-26): 인장은 SeasonPassUgui가 아니라 이 화면 쪽이 자연스럽다고
        // ShopUgui.cs 주석이 남겨 둔 자리 — 특수 뽑기 바로 아래에 둬서 "인장이 없으면
        // 여기서 산다"는 흐름이 눈에 보이게 했다. 가격표는 PetGachaPullUgui.Awake가
        // DefaultData.ShopItems()에서 읽어 채운다(여기 문구는 자리표시자).
        static void MakeSealPurchaseCard(RectTransform parent, TMP_FontAsset font)
        {
            var card = MakeCardContainer("seal-purchase-card", parent);
            MakeText("seal-purchase-title", "인장 구매", card, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, false);
            MakeText("seal-purchase-subtitle", "특수 뽑기의 유일한 유료 경로", card, font, 12, Dim, 18f, TextAlignmentOptions.MidlineLeft, false);

            var row = NewRect("seal-purchase-button-row", card);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            var rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.minHeight = 44f;
            rowElement.preferredHeight = 44f;

            MakeButton("btn-buy-seal-1", "1장", row, font, AccentFace);
            MakeButton("btn-buy-seal-10", "10장", row, font, AccentFace);
        }

        static void MakeResultPanel(RectTransform parent, TMP_FontAsset font)
        {
            var panel = MakeCardContainer("result-panel", parent);
            MakeText("result-title", "결과", panel, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, false);

            // 단일 뽑기 — 초상화 하나 + 이름 + 신규/중복 문구.
            var single = NewRect("result-single", panel);
            var singleCol = single.gameObject.AddComponent<VerticalLayoutGroup>();
            singleCol.spacing = 4f;
            singleCol.childAlignment = TextAnchor.UpperCenter;
            singleCol.childForceExpandWidth = true;
            singleCol.childForceExpandHeight = false;
            singleCol.childControlWidth = true;
            singleCol.childControlHeight = true;
            var singleFit = single.gameObject.AddComponent<ContentSizeFitter>();
            singleFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var portrait = NewRect("result-portrait", single);
            var portraitImg = portrait.gameObject.AddComponent<Image>();
            portraitImg.color = new Color(1f, 1f, 1f, 0.08f); // A-17: 그림이 없으면 흐린 자리표시자로 남는다.
            var portraitLayout = portrait.gameObject.AddComponent<LayoutElement>();
            portraitLayout.minHeight = 150f;
            portraitLayout.preferredHeight = 150f;
            portraitLayout.preferredWidth = 150f;

            // 10연차 — 5x2 그리드(BootstrapArtViewer.cs 패턴 재사용).
            var grid = NewRect("result-grid", panel);
            var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(72f, 82f);
            gridLayout.spacing = new Vector2(6f, 6f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;
            var gridFit = grid.gameObject.AddComponent<ContentSizeFitter>();
            gridFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            for (var i = 0; i < 10; i++)
            {
                var cell = NewRect($"result-portrait-{i}", grid);
                var cellImg = cell.gameObject.AddComponent<Image>();
                cellImg.color = new Color(1f, 1f, 1f, 0.08f);
            }

            MakeText("result-name", "—", panel, font, 14, Ink, 20f, TextAlignmentOptions.Midline, false);
            MakeText("result-note", "", panel, font, 12, Dim, 18f, TextAlignmentOptions.Midline, true);
        }

        static void MakeFusionEntry(RectTransform parent, TMP_FontAsset font)
        {
            var card = MakeCardContainer("fusion-entry-card", parent);
            // A-17(2026-09-22): 조각 합성 화면(PetFusionUgui, GemRacer/29)이 이 버튼을 연다 —
            // PetGachaPullUgui.Awake가 fusionPanel 필드를 보고 물려 있을 때만 켠다. 부트스트랩은
            // 자리만 만들고 "준비 중" 문구로 시작한다(MainHudUgui.Wire의 안 물린 버튼과 같은 모양).
            var btn = MakeButton("btn-fusion", "조각 합성 (준비 중)", card, font, AccentFace);
            btn.interactable = false;
            var btnLayout = btn.gameObject.AddComponent<LayoutElement>();
            btnLayout.minHeight = 40f;
            btnLayout.preferredHeight = 40f;
        }

        static RectTransform MakeCardContainer(string name, RectTransform parent)
        {
            var card = NewRect(name, parent);
            var img = card.gameObject.AddComponent<Image>();
            img.color = CardFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var col = card.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(14, 14, 12, 12);
            col.spacing = 4f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return card;
        }

        // --- 조각 만들기 ---------------------------------------------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

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
            if (!wrap) t.overflowMode = TextOverflowModes.Ellipsis;
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
