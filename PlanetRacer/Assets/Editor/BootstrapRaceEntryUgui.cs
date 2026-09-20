using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-04(2026-09-15): 레이스 출전 화면(RaceEntryPanel, UI Toolkit)을 일반 UI(uGUI)로 세운다.
    /// `UI Canvas/Overlays` 아래에 붙고, HUD의 "레이스" 버튼이 여닫는다(BootstrapHudUgui,
    /// `MainHudUgui.racePanel`에 이 패널을 물려야 실제로 열린다 — 씬 배선은 Unity 세션 몫).
    ///
    /// 이 화면은 지금까지 옮긴 것 중 유일하게 하위 화면이 셋이다 — entry-view(코스 3개) →
    /// anim-view(6대 도착 연출) → result-view(순위·보상). 셋 다 같은 자리를 차지하는 자식
    /// GameObject라 RaceEntryUgui가 SetActive로 갈아 끼운다(UI Toolkit 시절엔 style.display였다).
    /// 진행 막대(anim-fill)는 docs/design/ugui-migration.md 변환표대로 Image.fillAmount로 채운다.
    ///
    /// entry-view도 result-view처럼 화면을 꽉 채우고 루트 Image가 뒤 클릭을 막는다 —
    /// ugui-migration.md 3-1번 규칙대로 자체 닫기 버튼(`entry-close-button`)을 넣었다.
    /// result-view의 `close-button`은 원래 로직 그대로 entry-view로 돌아가는 버튼이다
    /// (RaceEntryUgui.ShowEntryView) — 완전히 닫는 것과 이름이 겹치지 않게 구분했다.
    /// </summary>
    public static class BootstrapRaceEntryUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace  = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color FuelInk  = new Color(0.47f, 0.78f, 0.92f);
        static readonly Color RewardInk = new Color(0.78f, 0.78f, 0.31f);
        static readonly Color AdInk    = new Color(0.59f, 0.78f, 0.63f);
        static readonly Color TrackBg  = new Color(0.16f, 0.16f, 0.22f, 1f);
        static readonly Color FillFace = new Color(0.47f, 0.78f, 0.55f, 1f);

        const float CellWidth = 400f;
        const float CellHeight = 140f;
        const float CellSpacing = 12f;

        static readonly (string prefix, string name, string info)[] Courses =
        {
            ("course1", "석영 평원 스프린트", "600m x1 · 평지70 험지20 부스트10"),
            ("course2", "결정 능선 루프", "800m x1 · 평지40 험지50 부스트10"),
            ("course3", "반사면 직선로", "500m x2 · 평지50 험지10 부스트40"),
        };

        [MenuItem("GemRacer/18. 레이스 출전 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("RaceEntry");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("RaceEntry", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<RaceEntryUgui>();

            BuildEntryView(root, font, panel);
            BuildAnimView(root, font);
            BuildResultView(root, font);

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 레이스 출전 화면(uGUI) 세움. MainHudUgui.racePanel에 이 'RaceEntry'를 물려야 " +
                      "HUD의 '레이스' 버튼으로 실제로 열린다.");
        }

        static void BuildEntryView(RectTransform root, TMP_FontAsset font, UiPanel panel)
        {
            var view = FullStretchChild("entry-view", root);
            var col = view.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("race-title-entry", "레이스 출전", view, font, 24, Ink, 32f);

            // A-16: 연료 아이콘 + 텍스트를 한 줄에.
            var fuelRow = NewRect("fuel-row", view);
            var fuelRowLayout = fuelRow.gameObject.AddComponent<LayoutElement>();
            fuelRowLayout.minHeight = 24f; fuelRowLayout.preferredHeight = 24f;
            // HorizontalLayoutGroup이 childForceExpandHeight=true면 스스로 flexibleHeight=1을 내놓아
            // 바깥 세로 그룹이 남는 세로 공간을 이 줄에 나눠 준다(연료 줄이 164px까지 늘어났다).
            fuelRowLayout.flexibleHeight = 0f;
            var fuelRowH = fuelRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            fuelRowH.spacing = 6f;
            fuelRowH.childAlignment = TextAnchor.MiddleLeft;
            fuelRowH.childForceExpandWidth = false;
            fuelRowH.childForceExpandHeight = false;
            fuelRowH.childControlWidth = true;
            fuelRowH.childControlHeight = true;
            MakeIcon("fuel-icon", fuelRow, 20f);
            MakeInlineText("fuel-label", "연료 0/10", fuelRow, font, 16, FuelInk);

            var rowList = NewRect("row-list", view);
            var rowListLayout = rowList.gameObject.AddComponent<LayoutElement>();
            rowListLayout.flexibleHeight = 1f;
            var grid = rowList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(CellSpacing, CellSpacing);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            foreach (var (prefix, name, info) in Courses)
                MakeCourseRow(rowList, font, prefix, name, info);

            // M-09 후속: 연료 부족일 때만 RaceEntryUgui.Refresh가 보여준다.
            var adLabel = MakeHeaderText("fuel-ad-label", "—", view, font, 13, AdInk, 20f);
            adLabel.enableWordWrapping = true;
            MakeButton("fuel-ad-button", "광고 보고 연료 +3", view, font, RowFace, 40f);

            // ugui-migration.md 3-1번: 화면을 꽉 채우는 패널이라 자체 닫기 버튼이 없으면
            // 빠져나올 길이 없다. onClick은 인스펙터에 보이는 영구 리스너로 걸어 둔다.
            var closeBtn = MakeButton("entry-close-button", "닫기", view, font, RowFace, 40f);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));
        }

        static void BuildAnimView(RectTransform root, TMP_FontAsset font)
        {
            var view = FullStretchChild("anim-view", root);
            view.gameObject.SetActive(false); // 출전 전에는 숨어 있다 — RaceEntryUgui가 연출 시작할 때 켠다

            var col = view.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("race-title-anim", "레이스 진행 중", view, font, 24, Ink, 32f);

            var animList = NewRect("anim-list", view);
            var animListLayout = animList.gameObject.AddComponent<LayoutElement>();
            animListLayout.flexibleHeight = 1f;
            var animCol = animList.gameObject.AddComponent<VerticalLayoutGroup>();
            animCol.spacing = 10f;
            animCol.childAlignment = TextAnchor.UpperLeft;
            animCol.childForceExpandWidth = true;
            animCol.childForceExpandHeight = false;
            animCol.childControlWidth = true;
            animCol.childControlHeight = true;

            for (int i = 0; i < 6; i++) MakeAnimRow(animList, font, i);

            MakeButton("skip-button", "건너뛰기", view, font, RowFace, 40f);
        }

        static void BuildResultView(RectTransform root, TMP_FontAsset font)
        {
            var view = FullStretchChild("result-view", root);
            view.gameObject.SetActive(false); // 결과가 나오기 전엔 숨어 있다 — 연출이 끝나면 RaceEntryUgui가 켠다

            var col = view.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("race-title-result", "레이스 결과", view, font, 24, Ink, 32f);

            var resultList = NewRect("result-list", view);
            var resultListLayout = resultList.gameObject.AddComponent<LayoutElement>();
            resultListLayout.flexibleHeight = 1f;
            var resultCol = resultList.gameObject.AddComponent<VerticalLayoutGroup>();
            resultCol.spacing = 6f;
            resultCol.childAlignment = TextAnchor.UpperLeft;
            resultCol.childForceExpandWidth = true;
            resultCol.childForceExpandHeight = false;
            resultCol.childControlWidth = true;
            resultCol.childControlHeight = true;

            for (int i = 0; i < 6; i++) MakeResultRow(resultList, font, i);

            var rewardLabel = MakeHeaderText("reward-label", "", view, font, 15, RewardInk, 44f);
            rewardLabel.enableWordWrapping = true;

            var boxAdLabel = MakeHeaderText("box-ad-label", "—", view, font, 13, AdInk, 20f);
            boxAdLabel.enableWordWrapping = true;
            MakeButton("box-ad-button", "광고 보고 상자 1개 더", view, font, RowFace, 40f);

            // RaceEntryUgui.Awake가 "close-button"을 찾아 ShowEntryView에 물린다 — entry-view로
            // 돌아가는 버튼이지 패널 전체를 닫는 버튼이 아니다(완전히 닫는 건 entry-view의
            // entry-close-button).
            MakeButton("close-button", "닫기", view, font, BtnFace, 44f);
        }

        static void MakeCourseRow(RectTransform parent, TMP_FontAsset font, string prefix, string name, string info)
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

            var nameLabel = MakeHeaderText($"{prefix}-name", name, row, font, 17, Ink, 22f);
            nameLabel.enableWordWrapping = false;
            var infoLabel = MakeHeaderText($"{prefix}-info", info, row, font, 12, Dim, 32f);
            infoLabel.enableWordWrapping = true;

            MakeButton($"{prefix}-button", "출전", row, font, BtnFace, 36f);
        }

        // 배경(Image)과 텍스트를 같은 오브젝트에 같이 달면 나중에 추가한 컴포넌트가 그림 순서상
        // 위로 올라와 글자를 덮어 버린다(MakeButton이 배경 오브젝트 + 텍스트 자식으로 나눈 이유와
        // 같다) — 그래서 결과 줄도 배경 오브젝트 하나에 텍스트 자식을 붙이는 구조로 만든다.
        static TMP_Text MakeResultRow(RectTransform parent, TMP_FontAsset font, int index)
        {
            var row = NewRect($"result-row-{index}-bg", parent);
            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 30f; le.preferredHeight = 30f;

            var labelRt = NewRect($"result-row-{index}", row);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(12f, 0f); labelRt.offsetMax = new Vector2(-12f, 0f);
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = 15;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return t;
        }

        static void MakeAnimRow(RectTransform parent, TMP_FontAsset font, int index)
        {
            var row = NewRect($"anim-row-{index}", parent);
            var layout = row.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 24f; layout.preferredHeight = 24f;
            // flexibleHeight를 0으로 못 박아 둔다. 기본값 -1은 "무시"라서
            // LayoutUtility가 LayoutElement를 건너뛰고 HorizontalLayoutGroup이
            // 보고하는 flexibleHeight(childForceExpandHeight가 켜져 있으면 1 이상)를
            // 쓴다 — 그러면 부모 VerticalLayoutGroup이 남은 높이를 여섯 줄에
            // 나눠 줘서 24px 막대가 130px로 부풀었다(2026-09-15 배선 세션에서 확인).
            layout.flexibleHeight = 0f;
            var horiz = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            horiz.spacing = 8f;
            horiz.childAlignment = TextAnchor.MiddleLeft;
            horiz.childForceExpandWidth = false;
            // 트랙을 줄 높이(24)로 늘리지 않고 제 높이(14)를 지키게 한다.
            horiz.childForceExpandHeight = false;
            horiz.childControlWidth = true;
            horiz.childControlHeight = true;

            var nameLabel = MakeHeaderText($"anim-name-{index}", "", row, font, 14, Ink, 24f);
            nameLabel.enableWordWrapping = false;
            var nameLayout = nameLabel.GetComponent<LayoutElement>();
            nameLayout.minWidth = 70f; nameLayout.preferredWidth = 70f;
            nameLayout.flexibleWidth = 0f;

            var track = NewRect($"anim-track-{index}", row);
            var trackImg = track.gameObject.AddComponent<Image>();
            trackImg.color = TrackBg;
            var trackLayout = track.gameObject.AddComponent<LayoutElement>();
            trackLayout.flexibleWidth = 1f;
            trackLayout.minHeight = 14f; trackLayout.preferredHeight = 14f;

            var fill = NewRect($"anim-fill-{index}", track);
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = FillFace;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
        }

        // --- 조각 만들기 ---------------------------------------------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        // entry-view/anim-view/result-view처럼 같은 자리를 겹쳐 차지하는(그중 하나만 SetActive) 자식.
        static RectTransform FullStretchChild(string name, RectTransform parent)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        static TMP_Text MakeHeaderText(string name, string text, RectTransform root, TMP_FontAsset font,
                                       float size, Color color, float height)
        {
            var rt = NewRect(name, root);
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

        // A-16: 연료 아이콘 자리. 그림이 없는 동안은 투명 — RaceEntryUgui.cs가 Awake에서
        // UiKit.SetIcon으로 실제 스프라이트를 입힌다.
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

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font, Color face, float height)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = face;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var labelRt = NewRect("label", rt);
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 15;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;

            return btn;
        }
    }
}
