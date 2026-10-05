using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// E-08(2026-10-06 Unity 배선 세션): 일일 던전 입구 화면을 `UI Canvas/Overlays` 아래에 세운다.
    /// BootstrapResearchUgui와 같은 구조 — 루트 "Dungeon"은 항상 켜 두고 스크립트를 붙이고,
    /// 실제 화면은 자식 "dungeon-backdrop" 하나라 DungeonUgui.Open/Close가 그것만 여닫는다.
    /// 같은 메뉴를 다시 눌러도 옛 "Dungeon"을 지우고 새로 세워서 결과가 같다(멱등).
    ///
    /// `GemRacer/7`은 절대 누르지 않는다 — 지금 MainGame 씬에 쌓여 있는 uGUI 화면들이
    /// 통째로 날아간다(CLAUDE.md "GemRacer/7 함정").
    /// </summary>
    public static class BootstrapDungeonUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.62f, 0.66f, 0.78f);
        static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color CardFace = new Color(0.094f, 0.102f, 0.141f, 1f);
        static readonly Color RowFace  = new Color(0.14f, 0.15f, 0.21f, 1f);
        static readonly Color BtnFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color Gold     = new Color(0.824f, 0.784f, 0.353f);

        const float CardWidth = 420f;

        [MenuItem("GemRacer/42. 일일 던전 입구 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Dungeon");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("Dungeon", overlays.transform);
            Stretch(root);
            root.gameObject.AddComponent<DungeonUgui>();

            var backdrop = NewRect("dungeon-backdrop", root);
            Stretch(backdrop);
            var bg = backdrop.gameObject.AddComponent<Image>();
            bg.color = Backdrop;
            bg.raycastTarget = true;

            var card = NewRect("dungeon-card", backdrop);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(CardWidth, 380f); // 높이는 아래 ContentSizeFitter가 다시 잡는다
            card.anchoredPosition = Vector2.zero;
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = CardFace;
            cardImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cardImg.type = Image.Type.Sliced;

            var col = card.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeText("dungeon-title", "일일 던전", card, font, 20, Ink, 28f, TextAlignmentOptions.Center);
            MakeText("dungeon-planet-label", "—", card, font, 15, Ink, 22f, TextAlignmentOptions.MidlineLeft);
            MakeText("dungeon-entries-label", "—", card, font, 13, Gold, 20f, TextAlignmentOptions.MidlineLeft);
            MakeText("dungeon-stones-label", "—", card, font, 13, Gold, 20f, TextAlignmentOptions.MidlineLeft);

            // 보상표 — 등급 네 줄. 값은 DungeonUgui가 연구 배율까지 곱해서 채운다.
            var table = NewRect("dungeon-reward-table", card);
            var tableImg = table.gameObject.AddComponent<Image>();
            tableImg.color = RowFace;
            tableImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            tableImg.type = Image.Type.Sliced;
            var tv = table.gameObject.AddComponent<VerticalLayoutGroup>();
            tv.padding = new RectOffset(12, 12, 8, 8);
            tv.spacing = 2f;
            tv.childForceExpandWidth = true;
            tv.childForceExpandHeight = false;
            tv.childControlWidth = true;
            tv.childControlHeight = true;
            table.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            for (var i = 0; i < DungeonUgui.GradeCount; i++)
                MakeText($"dungeon-grade-{i}", "—", table, font, 12, Ink, 18f, TextAlignmentOptions.MidlineLeft);

            var note = MakeText("dungeon-note-label", "—", card, font, 11, Dim, 34f, TextAlignmentOptions.MidlineLeft);
            note.enableWordWrapping = true;

            var enter = MakeButton("dungeon-enter-button", "준비 중", card, font, BtnFace, 16);
            var ele = enter.gameObject.AddComponent<LayoutElement>();
            ele.minHeight = 44f; ele.preferredHeight = 44f;
            enter.interactable = false; // 한 판이 아직 없다 — DungeonUgui.Refresh도 매번 끈다

            var close = MakeButton("dungeon-close-button", "닫기", card, font, BtnFace, 16);
            var cle = close.gameObject.AddComponent<LayoutElement>();
            cle.minHeight = 44f; cle.preferredHeight = 44f;

            backdrop.gameObject.SetActive(false);

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 일일 던전 입구 화면(uGUI) 세움. 여는 버튼은 'GemRacer/43'이 단다.");
        }

        /// <summary>
        /// 던전 입구를 여는 길. `GemRacer/41`(강화 화면에 연구소 버튼)과 똑같은 꼴이다 —
        /// 이미 서 있는 레이스 출전 화면에 버튼 하나만 더하고, 그 화면을 통째로 다시 세우는
        /// `GemRacer/19`류는 누르지 않는다(손으로 물린 연결이 끊긴다).
        ///
        /// 왜 레이스 출전 화면인가: 던전 한 판이 "60초 채굴 + 레이스 1판"이라 출전 화면이
        /// 제자리다. HUD 액션 줄은 이미 열 칸이고 라벨 셋이 잘려서 feedback.md에 Tifania
        /// 판단 대기로 남아 있으니 열한 번째 칸을 만들지 않는다(GemRacer/41과 같은 이유).
        /// onClick은 인스펙터에 보이는 영구 리스너로 걸어 Tifania가 눈으로 보고 바꿀 수 있게 한다.
        /// 같은 메뉴를 다시 눌러도 옛 버튼을 지우고 새로 달아서 결과가 같다(멱등).
        /// </summary>
        [MenuItem("GemRacer/43. 레이스 출전 화면에 일일 던전 버튼 추가 (uGUI, 안전 — 출전 화면만 건드림)")]
        public static void AddDungeonButtonToRaceEntry()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var entryGo = GameObject.Find("UI Canvas/Overlays/RaceEntry/entry-view");
            if (entryGo == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays/RaceEntry/entry-view'가 없다. 레이스 출전 화면을 먼저 세울 것."); return; }
            var entry = (RectTransform)entryGo.transform;

            var dungeon = Object.FindAnyObjectByType<DungeonUgui>();
            if (dungeon == null) { Debug.LogError("[GemRacer] 씬에 DungeonUgui가 없다. 'GemRacer/42' 먼저."); return; }

            var old = entry.Find("dungeon-open-button");
            if (old != null) Object.DestroyImmediate(old.gameObject); // 멱등

            var btn = MakeButton("dungeon-open-button", "일일 던전", entry, font, BtnFace, 16);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 44f; le.preferredHeight = 44f;

            // 닫기 버튼 바로 위에 놓는다 — 닫기는 늘 맨 아래여야 빠져나올 길이 눈에 띈다.
            var close = entry.Find("entry-close-button");
            btn.transform.SetSiblingIndex(close != null ? close.GetSiblingIndex() : entry.childCount - 1);

            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                btn.onClick, new UnityEngine.Events.UnityAction(dungeon.Open));

            EditorUtility.SetDirty(entryGo);
            Debug.Log("[GemRacer] 레이스 출전 화면에 'dungeon-open-button'(일일 던전)을 달고 onClick을 DungeonUgui.Open에 걸었다.");
        }

        // --- 조각 만들기 (BootstrapResearchUgui와 같은 모양) -----------------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static TMP_Text MakeText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                 float size, Color color, float height, TextAlignmentOptions align)
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
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return t;
        }

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font, Color face, float fontSize)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = face;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelRt = NewRect("label", rt);
            Stretch(labelRt);
            var t = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = label;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return btn;
        }
    }
}
