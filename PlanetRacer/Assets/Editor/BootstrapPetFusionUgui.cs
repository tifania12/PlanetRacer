using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Core;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-17 조각 합성(2026-09-22, pet-gacha.md 9절): 조각 합성 실행 화면을 uGUI로 세운다.
    /// BootstrapPetGachaPullUgui.cs(GemRacer/26)와 같은 패턴(NewRect/MakeText/MakeButton
    /// 로컬 헬퍼, 카드 = VerticalLayoutGroup+ContentSizeFitter, UiPanel{hiddenOnStart=true},
    /// ScrollRect로 감싸기) — PetGachaPullUgui의 btn-fusion이 여는 화면이라 그 옆에 형제로 둔다.
    ///
    /// 7등급 각각에 카드 하나 — 조각 보유 수 + "합성"(같은 등급 다른 펫) + "승급"(위 등급 조각)
    /// 버튼 두 개. Transcendent(7등급)는 승급 버튼이 없다(더 위 등급이 없어서, PetFusionUgui가
    /// Awake에서 그 버튼을 꺼 둔다 — 여기서는 만들어만 둔다).
    /// </summary>
    public static class BootstrapPetFusionUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg       = new Color(0.07f, 0.07f, 0.09f, 1f);
        static readonly Color CardFace = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color RowFace  = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color AccentFace = new Color(0.36f, 0.28f, 0.62f, 1f);

        static readonly PetGrade[] Grades =
        {
            PetGrade.Common, PetGrade.Advanced, PetGrade.Rare, PetGrade.Epic,
            PetGrade.Legendary, PetGrade.Mythic, PetGrade.Transcendent,
        };

        [MenuItem("GemRacer/29. 펫 조각 합성 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("PetFusion");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("PetFusion", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true;

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true;
            root.gameObject.AddComponent<PetFusionUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeText("fusion-title", "조각 합성", root, font, 24, Ink, 32f, TextAlignmentOptions.MidlineLeft, false);

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
            contentCol.spacing = 10f;
            contentCol.childAlignment = TextAnchor.UpperLeft;
            contentCol.childForceExpandWidth = true;
            contentCol.childForceExpandHeight = false;
            contentCol.childControlWidth = true;
            contentCol.childControlHeight = true;
            var contentFit = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (var i = 0; i < Grades.Length; i++)
                MakeGradeCard(content, font, i, Grades[i]);

            MakeResultPanel(content, font);

            var closeBtn = MakeButton("close-button", "닫기", root, font, RowFace);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 펫 조각 합성 화면(uGUI) 세움. " +
                      "PetGachaPullUgui의 fusionPanel 필드에 이 오브젝트(PetFusion)를 물려야 " +
                      "btn-fusion이 열 수 있다 — 안 물리면 그 버튼은 계속 꺼진 채로 남는다.");
        }

        static void MakeGradeCard(RectTransform parent, TMP_FontAsset font, int index, PetGrade grade)
        {
            var card = MakeCardContainer($"grade-{index}-card", parent);
            MakeText($"fusion-shards-{index}", $"{PetGradeInfo.NameKoFor(grade)} 조각 0개",
                card, font, 15, Ink, 20f, TextAlignmentOptions.MidlineLeft, false);

            var row = NewRect($"grade-{index}-button-row", card);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            var rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.minHeight = 40f;
            rowElement.preferredHeight = 40f;

            MakeButton($"btn-fuse-{index}", "합성 (3개)", row, font, RowFace);

            // Transcendent는 승급 대상이 없다 — 자리는 만들어 두되 PetFusionUgui.Awake가 꺼 둔다
            // (설계 판단 그대로: "화면이 숨기는 것으로 막아야 한다", PetFusionController.cs 주석).
            if (grade != PetGrade.Transcendent)
                MakeButton($"btn-promote-{index}", $"승급 ({PetFusion.PromotionCost(grade)}개)", row, font, AccentFace);
            else
                MakeButton($"btn-promote-{index}", "승급 (없음)", row, font, AccentFace);
        }

        static void MakeResultPanel(RectTransform parent, TMP_FontAsset font)
        {
            var panel = MakeCardContainer("fusion-result-panel", parent);
            MakeText("fusion-result-title", "조각을 골라 합성하거나 승급한다", panel, font, 15, Ink, 20f,
                TextAlignmentOptions.MidlineLeft, true);
            MakeText("fusion-result-note", "", panel, font, 12, Dim, 36f, TextAlignmentOptions.MidlineLeft, true);
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
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;

            return btn;
        }
    }
}
