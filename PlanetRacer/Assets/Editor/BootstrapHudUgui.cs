using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// D08(2026-09-15): 일반 UI(uGUI)로 만든 HUD를 씬에 세운다.
    /// UI Toolkit(UIDocument + UXML/USS)에서 옮겨오는 첫 화면이다.
    ///
    /// 왜 옮기나 — Tifania가 UI Toolkit에 익숙하지 않아 나중에 직접 고치기 어렵다고 했다.
    /// uGUI는 인스펙터에서 눈으로 보며 고칠 수 있고, 한글 폰트도 TMP 에셋 하나로 끝난다.
    ///
    /// 기준 해상도는 540x960(세로). CLAUDE.md 6번 규칙대로 모바일 세로가 기준 화면이다.
    /// CanvasScaler가 알아서 늘리고 줄이므로 UI Toolkit 시절의 ResponsiveLayout처럼
    /// 화면 크기를 직접 보고 클래스를 갈아 끼우는 코드가 필요 없다.
    ///
    /// **경고(2026-09-16, U-10에서 발견)**: 이 메뉴는 "UI Canvas"가 이미 있으면 통째로 지우고
    /// 다시 만든다. 문제는 U-02~U-07이 배선하면서 그 밑 `Overlays`에 화면들을 자식으로
    /// 붙여 뒀다는 것 — 이 메뉴를 다시 누르면 그 화면들이 전부 같이 사라지고, MainHudUgui의
    /// craftPanel/racePanel/boxPanel/settingsPanel/shopPanel 연결(씬에만 있는 데이터)도
    /// 전부 다시 해야 한다. **이미 화면이 하나라도 배선된 뒤에는 이 메뉴를 다시 누르지 않는다.**
    /// HUD 버튼만 추가해야 하면(예: btn-shop) `BootstrapShopUgui.AddShopButtonToActionRow`
    /// (GemRacer/23)처럼 대상 노드만 찾아 지우고 다시 만드는 애처블(additive) 메뉴를 새로 만든다.
    /// </summary>
    public static class BootstrapHudUgui
    {
        const string CanvasName = "UI Canvas";
        static readonly Vector2 Reference = new Vector2(540f, 960f);

        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Panel    = new Color(0.07f, 0.08f, 0.14f, 0.82f);
        static readonly Color BtnFace  = new Color(0.16f, 0.18f, 0.30f, 0.95f);
        static readonly Color GaugeBg  = new Color(1f, 1f, 1f, 0.12f);
        static readonly Color GaugeFg  = new Color(0.44f, 0.55f, 1f);

        [MenuItem("GemRacer/13. HUD를 일반 UI로 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null)
            {
                Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 먼저 'GemRacer/11. 한글 폰트 에셋 만들기'를 실행할 것.");
                return;
            }

            EnsureEventSystem();

            // 이미 있으면 통째로 지우고 다시 만든다. 부분 수정은 어긋나기 쉽다.
            var old = GameObject.Find(CanvasName);
            if (old != null) Object.DestroyImmediate(old);

            var canvasGo = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Reference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // 0.5 = 가로·세로 변화를 반반씩 반영. 세로로 길쭉한 폰과 넓은 PC 창 양쪽에서
            // 글자 크기가 덜 튄다. 0이면 가로만, 1이면 세로만 본다.
            scaler.matchWidthOrHeight = 0.5f;

            var hud = NewRect("HUD", canvasGo.transform);
            Stretch(hud, Vector2.zero, Vector2.one);
            var hudPanel = hud.gameObject.AddComponent<UiPanel>();
            hudPanel.hiddenOnStart = false;          // HUD는 항상 보인다
            var hudScript = hud.gameObject.AddComponent<MainHudUgui>();

            BuildStatusBar(hud, font);
            BuildCargoGauge(hud);
            BuildActionRow(hud, font);

            // 오버레이 패널들이 들어갈 자리. 지금은 빈 껍데기고, 나머지 일곱 화면을
            // 옮길 때 여기 아래에 하나씩 붙인다(docs/design/ugui-migration.md).
            var overlays = NewRect("Overlays", canvasGo.transform);
            Stretch(overlays, Vector2.zero, Vector2.one);

            Selection.activeObject = canvasGo;
            EditorUtility.SetDirty(canvasGo);
            Debug.Log($"[GemRacer] uGUI HUD 세움. 기준 해상도 {Reference.x}x{Reference.y}, 폰트 {font.name}. " +
                      $"오버레이 패널은 'Overlays' 아래에 붙인다.");
        }

        static void BuildStatusBar(RectTransform parent, TMP_FontAsset font)
        {
            var bar = NewRect("status-bar", parent);
            // 위쪽에 가로로 꽉 차게, 높이 64
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot     = new Vector2(0.5f, 1f);
            bar.offsetMin = new Vector2(16f, -80f);
            bar.offsetMax = new Vector2(-16f, -16f);

            var bg = bar.gameObject.AddComponent<Image>();
            bg.color = Panel;
            bg.raycastTarget = false;

            var row = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleLeft;
            row.padding = new RectOffset(16, 16, 0, 0);
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            row.childControlWidth = true;
            row.childControlHeight = true;

            MakeText("planet-name",   "쿼츠 행성", bar, font, 26, Ink, TextAlignmentOptions.MidlineLeft);
            MakeText("mineral-count", "원석 0.0",  bar, font, 26, Dim, TextAlignmentOptions.MidlineRight);
        }

        static void BuildCargoGauge(RectTransform parent)
        {
            var track = NewRect("cargo-gauge-track", parent);
            track.anchorMin = new Vector2(0f, 1f);
            track.anchorMax = new Vector2(1f, 1f);
            track.pivot     = new Vector2(0.5f, 1f);
            track.offsetMin = new Vector2(16f, -96f);
            track.offsetMax = new Vector2(-16f, -86f);

            var trackImg = track.gameObject.AddComponent<Image>();
            trackImg.color = GaugeBg;
            trackImg.raycastTarget = false;

            var fill = NewRect("cargo-gauge-fill", track);
            Stretch(fill, Vector2.zero, Vector2.one);
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = GaugeFg;
            fillImg.raycastTarget = false;
            // MainHudUgui가 fillAmount로 채운다. 스프라이트가 없으면 Filled가 안 먹으므로
            // 유니티 기본 UI 스프라이트를 물린다.
            fillImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0f;
        }

        static void BuildActionRow(RectTransform parent, TMP_FontAsset font)
        {
            var row = NewRect("action-row", parent);
            row.anchorMin = new Vector2(0f, 0f);
            row.anchorMax = new Vector2(1f, 0f);
            row.pivot     = new Vector2(0.5f, 0f);
            row.offsetMin = new Vector2(12f, 16f);
            row.offsetMax = new Vector2(-12f, 96f);

            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            MakeButton("btn-mine",     "업그레이드", row, font);
            MakeButton("btn-craft",    "제작",       row, font);
            MakeButton("btn-race",     "레이스",     row, font);
            MakeButton("btn-box",      "상자",       row, font);
            MakeButton("btn-settings", "설정",       row, font);
            MakeButton("btn-shop",     "상점",       row, font);
            MakeButton("btn-pet-gacha", "뽑기",      row, font);
            MakeButton("btn-pet-dex",  "도감",       row, font);
        }

        /// <summary>
        /// A-16(2026-09-20): status-bar의 "원석 0.0" 옆에 아이콘 자리를 하나 끼워 넣는다.
        /// `BootstrapShopUgui.AddShopButtonToActionRow`(GemRacer/23)와 같은 이유로 additive
        /// 메뉴다 — `Build()`(GemRacer/13)를 다시 누르면 이미 배선된 일곱 화면이 통째로 날아가니
        /// (위 클래스 설명 참고), status-bar 밑만 건드린다.
        ///
        /// status-bar의 바깥 HorizontalLayoutGroup은 childForceExpandWidth=true라 자식 두 개
        /// (planet-name / mineral-count)가 정확히 반반씩 나뉜다. 아이콘을 세 번째 자식으로 그냥
        /// 끼워 넣으면 반반이 셋으로 쪼개져 지금 확인된 배치가 흔들린다 — 그래서 mineral-count를
        /// "mineral-group"이라는 안쪽 줄로 감싸고, 그 안에서만 아이콘+글자를 나란히 놓는다.
        /// 바깥에서 보면 여전히 자식 둘(planet-name, mineral-group)이라 반반 배치는 그대로다.
        /// 실제 그림은 여기서 넣지 않는다 — 자리 표시자만 두고, `MainHudUgui.Awake()`가
        /// `UiKit.SetIcon("mineral-icon", "icon-raw-mineral")`로 런타임에 입힌다(art-wiring.md 1절).
        /// </summary>
        [MenuItem("GemRacer/24. HUD 상태바에 원석 아이콘 추가 (uGUI, 안전 — HUD만 건드림)")]
        public static void AddMineralIconToStatusBar()
        {
            var statusBarGo = GameObject.Find("UI Canvas/HUD/status-bar");
            if (statusBarGo == null) { Debug.LogError("[GemRacer] 'UI Canvas/HUD/status-bar'가 없다. 'GemRacer/13' 먼저."); return; }
            var statusBar = (RectTransform)statusBarGo.transform;

            // 멱등 — 이미 이 메뉴로 만든 mineral-group이 있으면 mineral-count를 도로 꺼내고
            // 그룹만 지운 뒤 처음부터 다시 만든다. 그래야 몇 번을 눌러도 결과가 같다.
            RectTransform mineralCount;
            int insertIndex;
            var existingGroup = statusBar.Find("mineral-group");
            if (existingGroup != null)
            {
                insertIndex = existingGroup.GetSiblingIndex();
                var mc = existingGroup.Find("mineral-count");
                mineralCount = mc != null ? (RectTransform)mc : null;
                if (mineralCount != null) mineralCount.SetParent(statusBar, false);
                Object.DestroyImmediate(existingGroup.gameObject);
                if (mineralCount == null)
                {
                    Debug.LogError("[GemRacer] 'mineral-group' 안에 'mineral-count'가 없다. 씬이 예상과 달라 멈춘다.");
                    return;
                }
            }
            else
            {
                var mc = statusBar.Find("mineral-count");
                if (mc == null) { Debug.LogError("[GemRacer] 'mineral-count'가 없다. 'GemRacer/13' 먼저."); return; }
                mineralCount = (RectTransform)mc;
                insertIndex = mineralCount.GetSiblingIndex();
            }

            var group = NewRect("mineral-group", statusBar);
            group.SetSiblingIndex(insertIndex); // 원래 mineral-count가 있던 자리(오른쪽 칸)에 그대로 넣는다

            var groupLayout = group.gameObject.AddComponent<HorizontalLayoutGroup>();
            groupLayout.spacing = 6f;
            groupLayout.childAlignment = TextAnchor.MiddleRight; // mineral-count가 원래 MidlineRight였던 것과 같은 자리
            groupLayout.childForceExpandWidth = false; // 안쪽은 아이콘+글자가 딱 붙어야 하니 바깥과 다르게 false
            groupLayout.childForceExpandHeight = true;
            groupLayout.childControlWidth = true;
            groupLayout.childControlHeight = true;

            var icon = NewRect("mineral-icon", group);
            var img = icon.gameObject.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            // 실제 그림은 MainHudUgui.Awake()가 UiKit.SetIcon으로 넣는다. 여기서는 자리 표시자만.
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var iconLayout = icon.gameObject.AddComponent<LayoutElement>();
            iconLayout.minWidth = 22f;  iconLayout.preferredWidth = 22f;  iconLayout.flexibleWidth = 0f;
            iconLayout.minHeight = 22f; iconLayout.preferredHeight = 22f; iconLayout.flexibleHeight = 0f;

            mineralCount.SetParent(group, false);
            mineralCount.SetSiblingIndex(1); // 아이콘 다음

            EditorUtility.SetDirty(statusBarGo);
            Debug.Log("[GemRacer] HUD 상태바에 'mineral-icon' 자리 추가함. MainHudUgui.Awake()가 " +
                      "UiKit.SetIcon(\"mineral-icon\", \"icon-raw-mineral\")로 실제 그림을 입힌다.");
        }

        // --- 조각 만들기 ---------------------------------------------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static TMP_Text MakeText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                 float size, Color color, TextAlignmentOptions align)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
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

            var labelRt = NewRect("label", rt);
            Stretch(labelRt, Vector2.zero, Vector2.one);
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = 20;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            // 글자 수가 많은 라벨은 20pt로는 칸을 넘는다. action-row에 여섯 번째 버튼(btn-shop)이
            // 붙으면서 한 칸이 96.8px → 79.3px로 줄었고 "업그레이드"(20pt에서 86.4px 필요)가
            // 말줄임으로 잘렸다(2026-09-17 U-10 배선에서 실제로 봤다). 자동 축소를 켜 두면
            // 앞으로 버튼이 하나 더 늘거나 라벨이 길어져도 잘리는 대신 줄어든다.
            t.enableAutoSizing = true;
            t.fontSizeMin = 14f;
            t.fontSizeMax = 20f;

            return btn;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            // 이 프로젝트는 새 인풋 시스템을 쓴다. 옛 StandaloneInputModule을 붙이면
            // "이 프로젝트는 새 입력 시스템을 쓰도록 설정되어 있다"며 UI 입력이 안 먹는다.
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) go.AddComponent(t);
            else go.AddComponent<StandaloneInputModule>();
            Debug.Log("[GemRacer] EventSystem을 새로 만들었다. 없으면 버튼이 눌리지 않는다.");
        }
    }
}
