using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-17(2026-09-21, pet-gacha.md 9-2): 펫 도감 그리드를 uGUI로 세운다.
    /// BootstrapPetGachaPullUgui.cs(GemRacer/26)와 같은 패턴(NewRect/MakeText/MakeButton
    /// 로컬 헬퍼, UiPanel{hiddenOnStart=true}) + BootstrapArtViewer.cs(GemRacer/15)의
    /// GridLayoutGroup 3열 패턴을 재사용한다. 다만 ArtViewer는 Resources 폴더를 스캔해서
    /// 채우지만 여기는 PetSpeciesTable.All(124종) 전부를 등급별 섹션으로 미리 만들어 둔다 —
    /// PetDexUgui.Refresh가 보유 종만 스프라이트를 입힌다(design 9-2 "부트스트랩이 인덱스로
    /// 이름 붙인 고정 개수 줄을 만들고 스크립트가 채우는" 방식 그대로).
    ///
    /// 가로/태블릿에서 열 수를 5~6으로 늘리는 건(design 9-2) 여기서는 안 한다 — 다른 uGUI
    /// 화면들처럼 CanvasScaler 스케일에 맡긴다(BootstrapPetGachaPullUgui.cs의 같은 결정과
    /// 같은 이유, ugui-migration.md "ResponsiveLayout.cs를 uGUI로 옮기지 않는다").
    /// </summary>
    public static class BootstrapPetDexUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 1f);
        static readonly Color CardFace = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color RowFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color CellFace = new Color(1f, 1f, 1f, 0.08f); // 미보유 실루엣 자리.

        [MenuItem("GemRacer/27. 펫 도감 그리드 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("PetDex");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("PetDex", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true;

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true;
            root.gameObject.AddComponent<PetDexUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeText("dex-title", "펫 도감", root, font, 24, Ink, 32f, TextAlignmentOptions.MidlineLeft, false);

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
            contentCol.spacing = 14f;
            contentCol.childAlignment = TextAnchor.UpperLeft;
            contentCol.childForceExpandWidth = true;
            contentCol.childForceExpandHeight = false;
            contentCol.childControlWidth = true;
            contentCol.childControlHeight = true;
            var contentFit = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 등급 7개 섹션 — 124종 전부(PetSpeciesTable.All)를 등급별로 미리 셀로 만들어 둔다.
            for (var g = 0; g < PetGradeInfo.NameKo.Length; g++)
            {
                var grade = (PetGrade)g;
                MakeText($"dex-header-{g}", $"{PetGradeInfo.NameKoFor(grade)} 0/{PetGradeInfo.SpeciesCountFor(grade)}",
                    content, font, 16, Ink, 24f, TextAlignmentOptions.MidlineLeft, false);

                var grid = NewRect($"dex-grid-{g}", content);
                var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
                gridLayout.cellSize = new Vector2(150f, 172f);
                gridLayout.spacing = new Vector2(8f, 8f);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 3;
                var gridFit = grid.gameObject.AddComponent<ContentSizeFitter>();
                gridFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                foreach (var speciesId in PetSpeciesTable.InGrade(grade))
                    MakeCell(speciesId, grid);
            }

            MakeDetailPopup(root, font);

            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 펫 도감 그리드(uGUI) 세움. 124칸 전부 미리 만들어 뒀고 " +
                      "PetDexUgui가 패널을 열 때마다 보유 종만 스프라이트를 입힌다. MainHudUgui에 " +
                      "여는 버튼이 아직 없으니 Play 중 Hierarchy에서 직접 켜서 봐야 한다.");
        }

        static void MakeCell(int speciesId, RectTransform parent)
        {
            var cell = NewRect($"dex-cell-{speciesId}", parent);
            var img = cell.gameObject.AddComponent<Image>();
            img.color = CellFace; // 미보유 기본값 — 보유 확인되면 PetDexUgui가 스프라이트로 덮는다.

            var btn = cell.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
        }

        static void MakeDetailPopup(RectTransform root, TMP_FontAsset font)
        {
            var popup = NewRect("dex-detail-panel", root);
            popup.anchorMin = new Vector2(0.5f, 0.5f);
            popup.anchorMax = new Vector2(0.5f, 0.5f);
            popup.pivot     = new Vector2(0.5f, 0.5f);
            popup.sizeDelta = new Vector2(320f, 260f);

            var bg = popup.gameObject.AddComponent<Image>();
            bg.color = CardFace;
            bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bg.type = Image.Type.Sliced;

            var col = popup.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(16, 16, 16, 16);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperCenter;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            var portrait = NewRect("dex-detail-portrait", popup);
            var portraitImg = portrait.gameObject.AddComponent<Image>();
            portraitImg.color = CellFace;
            var portraitLayout = portrait.gameObject.AddComponent<LayoutElement>();
            portraitLayout.minHeight = 96f;
            portraitLayout.preferredHeight = 96f;

            MakeText("dex-detail-name", "—", popup, font, 16, Ink, 22f, TextAlignmentOptions.Midline, false);
            MakeText("dex-detail-grade", "—", popup, font, 12, Dim, 18f, TextAlignmentOptions.Midline, false);

            var equipBtn = MakeButton("dex-detail-equip", "장착", popup, font, RowFace);
            var equipLayout = equipBtn.gameObject.AddComponent<LayoutElement>();
            equipLayout.minHeight = 40f;
            equipLayout.preferredHeight = 40f;

            var closeBtn = MakeButton("dex-detail-close", "닫기", popup, font, CardFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 36f;
            closeLayout.preferredHeight = 36f;

            popup.gameObject.SetActive(false);
        }

        // --- 조각 만들기 (BootstrapPetGachaPullUgui.cs와 동일한 헬퍼) -------------

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
