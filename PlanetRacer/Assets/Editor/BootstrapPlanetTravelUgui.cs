using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// P-09(2026-09-25 야간 세션): 행성 선택·이동 화면을 uGUI로 세운다. `UI Canvas/Overlays`
    /// 아래에 붙는다 — HUD에 아직 여는 버튼이 없으니 MainHudUgui.planetPanel에 이 'PlanetTravel'을
    /// 물리고 HUD 씬에 "btn-planet" 버튼을 추가해야 실제로 열린다(둘 다 씬 배선, Unity 세션 몫).
    ///
    /// 줄 개수는 DefaultData.Planets().Count를 그대로 따라간다 — 나중에 행성이 늘어나도
    /// 이 부트스트랩과 PlanetTravelUgui 둘 다 코드를 안 고치고 다시 눌러 주기만 하면 된다
    /// (CLAUDE.md 3번, 멱등). 이름은 course1/course2처럼 고정 접두사 대신 "planet-{순서}-*"로
    /// 인덱스를 쓴다 — RaceEntryUgui의 result-row-{i}와 같은 방식.
    /// </summary>
    public static class BootstrapPlanetTravelUgui
    {
        static readonly Color Ink     = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim     = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg      = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color RowFace = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color BtnFace = new Color(0.28f, 0.34f, 0.62f, 1f);

        const float RowHeight = 64f;

        [MenuItem("GemRacer/32. 행성 이동 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("PlanetTravel");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("PlanetTravel", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true; // 열려 있는 동안 뒤로 클릭이 새지 않게 막는다

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true; // 오버레이 화면이라 기본은 닫힌 채로 시작
            root.gameObject.AddComponent<PlanetTravelUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeHeaderText("planet-travel-title", "행성 이동", root, font, 24, Ink, 32f);

            var rowList = NewRect("row-list", root);
            var rowListLayout = rowList.gameObject.AddComponent<LayoutElement>();
            rowListLayout.flexibleHeight = 1f;
            var rowCol = rowList.gameObject.AddComponent<VerticalLayoutGroup>();
            rowCol.spacing = 8f;
            rowCol.childAlignment = TextAnchor.UpperLeft;
            rowCol.childForceExpandWidth = true;
            rowCol.childForceExpandHeight = false;
            rowCol.childControlWidth = true;
            rowCol.childControlHeight = true;

            var planets = DefaultData.Planets();
            for (int i = 0; i < planets.Count; i++)
                MakePlanetRow(rowList, font, i, planets[i].NameKo);

            // ugui-migration.md 3-1번: 화면을 꽉 채우는 패널이라 자체 닫기 버튼이 없으면
            // 빠져나올 길이 없다.
            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace, 40f);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log($"[GemRacer] 행성 이동 화면(uGUI) 세움. 줄 {planets.Count}개. " +
                      "MainHudUgui.planetPanel에 이 'PlanetTravel'을 물리고, HUD에 여는 버튼을 " +
                      "추가해야 실제로 열린다(둘 다 씬 배선).");
        }

        static void MakePlanetRow(RectTransform parent, TMP_FontAsset font, int index, string nameKo)
        {
            var row = NewRect($"planet-{index}-row", parent);
            var img = row.gameObject.AddComponent<Image>();
            img.color = RowFace;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.minHeight = RowHeight; rowLayout.preferredHeight = RowHeight;
            rowLayout.flexibleHeight = 0f;

            var horiz = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            horiz.padding = new RectOffset(14, 14, 8, 8);
            horiz.spacing = 10f;
            horiz.childAlignment = TextAnchor.MiddleLeft;
            horiz.childForceExpandWidth = false;
            horiz.childForceExpandHeight = false;
            horiz.childControlWidth = true;
            horiz.childControlHeight = true;

            var textCol = NewRect($"planet-{index}-text", row);
            var textColLayout = textCol.gameObject.AddComponent<LayoutElement>();
            textColLayout.flexibleWidth = 1f;
            var textColV = textCol.gameObject.AddComponent<VerticalLayoutGroup>();
            textColV.spacing = 2f;
            textColV.childAlignment = TextAnchor.MiddleLeft;
            textColV.childForceExpandWidth = true;
            textColV.childForceExpandHeight = false;
            textColV.childControlWidth = true;
            textColV.childControlHeight = true;

            var nameLabel = MakeHeaderText($"planet-{index}-name", nameKo, textCol, font, 17, Ink, 22f);
            nameLabel.enableWordWrapping = false;
            var infoLabel = MakeHeaderText($"planet-{index}-info", "", textCol, font, 12, Dim, 18f);
            infoLabel.enableWordWrapping = false;

            var btn = MakeButton($"planet-{index}-button", "이동", row, font, BtnFace, 40f);
            var btnLayout = btn.gameObject.GetComponent<LayoutElement>();
            btnLayout.minWidth = 90f; btnLayout.preferredWidth = 90f; btnLayout.flexibleWidth = 0f;
        }

        // --- 조각 만들기 (BootstrapRaceEntryUgui.cs와 같은 형식) ---------------------------------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
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
