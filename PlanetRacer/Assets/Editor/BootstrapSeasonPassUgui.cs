using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// M-14 ③(2026-09-26): 시즌 패스 화면(SeasonPassUgui)을 uGUI로 세운다. BootstrapShopUgui
    /// (GemRacer/22)와 같은 자리 — `UI Canvas/Overlays` 아래에 붙고, HUD의 "시즌패스" 버튼이
    /// 여닫는다(MainHudUgui.seasonPassPanel에 이 패널을 물려야 실제로 열린다 — 씬 배선은
    /// Unity 세션 몫).
    ///
    /// 상점(아홉 줄, 줄마다 이름/상태/버튼 하나)과 달리 여기는 줄마다 무료·유료 두 칸이 있어서
    /// (레벨/XP 헤더 + 무료 보상·버튼 + 유료 보상·버튼) 한 줄이 더 크다 — CellHeight를 상점의
    /// 140 대신 168로 잡았다. 열 줄(DefaultData.SeasonPassTiers 길이)이라 아홉 줄이던 상점보다도
    /// 커서 처음부터 ScrollRect로 감싼다(ugui-migration.md 3-1과 같은 이유).
    ///
    /// 이 화면은 에디터 없는 세션이 코드까지만 준비한 것이다(CLAUDE.md "UI는 uGUI" 절 —
    /// "새 화면은 코드까지만, 씬 배선은 Unity 세션"). 이 메뉴를 실제로 눌러 씬에 반영하는 것과
    /// MainHudUgui.seasonPassPanel 연결, HUD 액션 줄에 버튼 추가(아래 두 번째 메뉴)는 다음
    /// Unity 세션이 할 일이다.
    /// </summary>
    public static class BootstrapSeasonPassUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace  = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color HeaderFace = new Color(0.16f, 0.18f, 0.30f, 0.95f);
        static readonly Color FreeBtnFace = new Color(0.24f, 0.42f, 0.30f, 1f); // 무료 트랙 — 초록 계열
        static readonly Color PaidBtnFace = new Color(0.42f, 0.34f, 0.20f, 1f); // 유료 트랙 — 금색 계열

        // BootstrapHudUgui.cs의 BtnFace와 같은 값 — HUD 버튼들과 톤을 맞춘다.
        static readonly Color HudBtnFace = new Color(0.16f, 0.18f, 0.30f, 0.95f);

        const float CellWidth = 420f;
        const float CellHeight = 168f;
        const float CellSpacing = 12f;

        [MenuItem("GemRacer/33. 시즌 패스 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("SeasonPass");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("SeasonPass", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤로 클릭이 새지 않게 막는다(ShopUgui와 같은 이유)

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true;
            root.gameObject.AddComponent<SeasonPassUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("season-title", "시즌 패스", root, font, 24, Ink, 32f);
            MakeHeaderText("level-label", "레벨 0 · 0/100 XP", root, font, 16, Dim, 24f);

            // 유료 트랙 구매 줄. ShopUgui의 열 번째 줄(SeasonPassPaidTrack, M-14 ②)이 아직 상점
            // 화면엔 없어서(ShopUgui.cs 주석) 여기서 대신 산다 — 배틀패스 화면이 자기 유료 트랙을
            // 직접 파는 건 다른 게임에서도 흔한 자리라 화면 밖으로 뺄 이유가 없다.
            var buyRow = NewRect("paidtrack-row", root);
            var buyLayout = buyRow.gameObject.AddComponent<LayoutElement>();
            buyLayout.minHeight = 44f; buyLayout.preferredHeight = 44f;
            // flexibleHeight를 0으로 못 박아 둔다. 바로 아래 HorizontalLayoutGroup이
            // childForceExpandHeight = true라 자기 자신의 flexibleHeight를 1로 보고하는데, 이
            // LayoutElement가 flexibleHeight를 안 건드리면(기본 -1 = 미설정) 그 1이 그대로 먹혀서
            // 바깥 VerticalLayoutGroup이 남는 세로 공간을 scroll-view와 이 줄에 반씩 나눠 준다.
            // 그래서 44픽셀이어야 할 구매 줄이 356픽셀이 되고 구매 버튼이 세로로 길쭉해졌다
            // (2026-09-26 19시 Unity 배선 세션에서 씬을 세우고 눈으로 확인).
            buyLayout.flexibleHeight = 0f;
            var buyImg = buyRow.gameObject.AddComponent<Image>();
            buyImg.color = RowFace;
            buyImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            buyImg.type = Image.Type.Sliced;
            var buyRowLayout = buyRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buyRowLayout.padding = new RectOffset(14, 14, 8, 8);
            buyRowLayout.spacing = 10f;
            buyRowLayout.childAlignment = TextAnchor.MiddleLeft;
            buyRowLayout.childForceExpandWidth = false;
            buyRowLayout.childForceExpandHeight = true;
            buyRowLayout.childControlWidth = false;
            buyRowLayout.childControlHeight = true;

            // MakeHeaderText와 MakeButton이 이미 LayoutElement를 하나씩 붙여 준다. 여기서
            // AddComponent를 또 부르면 같은 오브젝트에 LayoutElement가 둘이 되는데, 유니티는 우선순위가
            // 같은 것끼리는 큰 값을 택하므로 아래 preferredHeight 32가 헬퍼의 40에 조용히 먹힌다 —
            // 값이 왜 안 먹는지 알기 어려운 자리라 GetComponent로 이미 있는 것을 고쳐 쓴다.
            var stateText = MakeHeaderText("paidtrack-state", "미보유", buyRow, font, 15, Dim, 28f);
            var stateLe = stateText.gameObject.GetComponent<LayoutElement>();
            stateLe.preferredWidth = 200f;
            var buyBtn = MakeButton("paidtrack-button", "유료 트랙 구매", buyRow, font, PaidBtnFace);
            var buyBtnLe = buyBtn.gameObject.GetComponent<LayoutElement>();
            buyBtnLe.preferredWidth = 180f; buyBtnLe.minHeight = 32f; buyBtnLe.preferredHeight = 32f;

            // 열 줄은 세로 화면에 다 안 들어간다(BootstrapShopUgui의 아홉 줄과 같은 이유) — 스크롤로 감싼다.
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
            // U-11: 가로가 넓어지면 2열로 재배치한다(CLAUDE.md 6번).
            var rowListResp = rowList.gameObject.AddComponent<ResponsiveGridCell>();
            rowListResp.baseCellSize = new Vector2(CellWidth, CellHeight);

            foreach (var tier in DefaultData.SeasonPassTiers())
                MakeRow(rowList, font, tier);

            // 닫기 버튼. ShopUgui와 같은 이유(뒤 클릭을 막기 때문에 빠져나올 길이 필요)로 스크롤 바깥,
            // 항상 보이는 자리에 둔다.
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 시즌 패스 화면(uGUI) 세움. MainHudUgui.seasonPassPanel에 이 'SeasonPass'를 " +
                      "물려야 HUD 버튼으로 실제로 열린다. HUD에 여는 버튼이 아직 없으면 'GemRacer/34' 먼저.");
        }

        /// <summary>
        /// BootstrapShopUgui.AddShopButtonToActionRow(GemRacer/23)와 완전히 같은 자리 — action-row만
        /// 찾아서 "btn-seasonpass" 하나만 추가한다. GemRacer/7(메인 씬 통째로 새로 만들기)을 다시
        /// 누르지 않는 이유는 그 파일 주석 그대로다.
        ///
        /// 주의: action-row는 이미 열 개 버튼(mine/craft/race/box/settings/shop/gacha-odds/
        /// pet-gacha/pet-dex/planet)이 붙어 있다 — feedback.md에 "HUD 액션 줄이 9칸이 되면서
        /// 긴 라벨이 잘린다"가 Tifania 판단 대기로 남아 있는 채로, 이 버튼을 더하면 열한 칸이
        /// 된다. 그 판단(라벨 줄이기/두 줄로 접기/가로 스크롤)이 나기 전까지는 라벨이 더 잘릴
        /// 수 있다는 뜻이다 — 새 기능이 없다고 화면 접근 경로 자체를 안 만들 수는 없어서 버튼은
        /// 추가하되, 이 사실을 feedback.md 판단 대기 항목에 같이 적어 둔다(이 커밋과 같은 세션).
        /// </summary>
        [MenuItem("GemRacer/34. HUD에 시즌 패스 버튼 추가 (uGUI, 안전 — HUD만 건드림)")]
        public static void AddSeasonPassButtonToActionRow()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var actionRowGo = GameObject.Find("UI Canvas/HUD/action-row");
            if (actionRowGo == null) { Debug.LogError("[GemRacer] 'UI Canvas/HUD/action-row'가 없다. 'GemRacer/13' 먼저."); return; }
            var actionRow = (RectTransform)actionRowGo.transform;

            var old = actionRow.Find("btn-seasonpass");
            if (old != null) Object.DestroyImmediate(old.gameObject); // 멱등

            var rt = NewRect("btn-seasonpass", actionRow);
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
            t.text = "시즌패스";
            t.fontSize = 20;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.enableAutoSizing = true; // BootstrapShopUgui.MakeButton과 같은 자동 축소
            t.fontSizeMin = 12f;
            t.fontSizeMax = 20f;

            EditorUtility.SetDirty(actionRowGo);
            Debug.Log("[GemRacer] HUD action-row에 'btn-seasonpass' 추가함. MainHudUgui.seasonPassPanel에 " +
                      "'SeasonPass'를 물려야 버튼이 켜진다.");
        }

        static void MakeRow(RectTransform parent, TMP_FontAsset font, SeasonPassTier tier)
        {
            var row = NewRect($"tier{tier.Level}", parent);

            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var col = row.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(14, 14, 10, 10);
            col.spacing = 6f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            var header = NewRect("header", row);
            var headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 8f;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            var headerLe = header.gameObject.AddComponent<LayoutElement>();
            headerLe.minHeight = 22f; headerLe.preferredHeight = 22f;
            MakeHeaderText($"tier{tier.Level}-title", $"레벨 {tier.Level}", header, font, 16, Ink, 22f);
            MakeHeaderText($"tier{tier.Level}-xp", $"{tier.RequiredXp} XP", header, font, 13, Dim, 22f);

            var tracks = NewRect("tracks", row);
            var tracksLayout = tracks.gameObject.AddComponent<HorizontalLayoutGroup>();
            tracksLayout.spacing = 10f;
            tracksLayout.childAlignment = TextAnchor.UpperLeft;
            tracksLayout.childForceExpandWidth = true;
            tracksLayout.childForceExpandHeight = true;
            tracksLayout.childControlWidth = true;
            tracksLayout.childControlHeight = true;

            MakeTrackColumn(tracks, font, $"tier{tier.Level}-free", "무료", FreeBtnFace);
            MakeTrackColumn(tracks, font, $"tier{tier.Level}-paid", "유료", PaidBtnFace);
        }

        static void MakeTrackColumn(RectTransform parent, TMP_FontAsset font, string prefix, string label, Color btnFace)
        {
            var colRt = NewRect(prefix, parent);
            var col = colRt.gameObject.AddComponent<VerticalLayoutGroup>();
            col.spacing = 4f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText($"{prefix}-label", label, colRt, font, 12, Dim, 16f);
            MakeHeaderText($"{prefix}-text", "-", colRt, font, 14, Ink, 20f);
            MakeButton($"{prefix}-button", "받기", colRt, font, btnFace);
        }

        // --- 조각 만들기 (BootstrapShopUgui와 동일) ---------------------------------------------------

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
            le.minHeight = 40f;
            le.preferredHeight = 40f;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 13;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;

            return btn;
        }
    }
}
