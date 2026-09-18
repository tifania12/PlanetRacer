using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// P-15(2026-09-19): 펫 뽑기 확률 공개 화면을 uGUI로 세운다. `UI Canvas/Overlays` 아래에
    /// 붙는다 — HUD 버튼은 아직 없다(액션 줄에 이미 여섯 개가 있고, 아직 펫 뽑기 자체를
    /// 돌리는 화면이 없어서 지금 일곱 번째를 끼워 넣을 자리인지도 불확실하다). 대신
    /// `MainHudUgui.gachaOddsPanel` 필드와 Wire 호출을 미리 넣어 뒀다 — U-10(상점)이 그랬듯,
    /// 나중에 `btn-gacha-odds` 버튼만 액션 줄에 추가하면(BootstrapShopUgui.AddShopButtonToActionRow
    /// 와 같은 모양의 애처블 메뉴 하나면 된다) 바로 열린다. 그때까지는 `Selection.activeObject`로
    /// 잡아 둔 이 GameObject를 Play 모드에서 직접 켜서 확인할 수 있다.
    ///
    /// 줄 수는 확률표 길이 그대로다 — 무료 5 / 일반 6 / 고급 4 / 특수 3 / 승급 6단계.
    /// 표 값이 나중에 바뀌어도 이 부트스트랩은 줄 "칸"만 만들고 글자는
    /// PetGachaOddsUgui.Awake가 PetGachaTable/PetFusion에서 직접 읽어 채운다.
    /// </summary>
    public static class BootstrapPetGachaOddsUgui
    {
        static readonly Color Ink     = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim     = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Bg      = new Color(0.07f, 0.07f, 0.09f, 0.97f);
        static readonly Color CardFace = new Color(0.12f, 0.13f, 0.19f, 1f);
        static readonly Color RowFace  = new Color(0.28f, 0.34f, 0.62f, 1f);

        const int FreeRows = 5;
        const int NormalRows = 6;
        const int AdvancedRows = 4;
        const int SpecialRows = 3;
        const int PromotionRows = 6; // Common→Advanced ... Mythic→Transcendent

        [MenuItem("GemRacer/25. 펫 뽑기 확률 공개 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("PetGachaOdds");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("PetGachaOdds", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Bg;
            bg.raycastTarget = true;

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true;
            root.gameObject.AddComponent<PetGachaOddsUgui>();

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(20, 20, 20, 20);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            MakeText("gacha-odds-title", "펫 뽑기 확률 공개", root, font, 24, Ink, 32f, TextAlignmentOptions.MidlineLeft, false);

            // 카드 다섯 장(무료/일반/고급/특수/합성)이 세로 한 칸에 다 안 들어간다 —
            // BootstrapShopUgui·BootstrapCraftingUgui와 같은 이유로 처음부터 ScrollRect로 감싼다.
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

            var cardList = NewRect("card-list", scrollView);
            cardList.anchorMin = new Vector2(0f, 1f);
            cardList.anchorMax = new Vector2(1f, 1f);
            cardList.pivot     = new Vector2(0.5f, 1f);
            cardList.offsetMin = Vector2.zero;
            cardList.offsetMax = Vector2.zero;
            scroll.content = cardList;

            var listCol = cardList.gameObject.AddComponent<VerticalLayoutGroup>();
            listCol.spacing = 12f;
            listCol.childAlignment = TextAnchor.UpperLeft;
            listCol.childForceExpandWidth = true;
            listCol.childForceExpandHeight = false;
            listCol.childControlWidth = true;
            listCol.childControlHeight = true;
            var listFit = cardList.gameObject.AddComponent<ContentSizeFitter>();
            listFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeOddsCard(cardList, font, "free", "무료 뽑기 (광고 시청, 하루 10회 · 천장 없음)", FreeRows);
            MakeOddsCard(cardList, font, "normal", "일반 뽑기 (레이싱 재화 · 천장 없음)", NormalRows);
            MakeOddsCard(cardList, font, "advanced", "고급 뽑기 (하루 1회 무료 + 유료)", AdvancedRows);
            MakeOddsCard(cardList, font, "special", "특수 뽑기 (초월의 인장 전용)", SpecialRows);
            MakeFusionCard(cardList, font);

            // 닫기 버튼. 화면을 꽉 채우고 뒤로 클릭을 막기 때문에 스크롤 바깥에 고정한다
            // (ugui-migration.md 3-1번, U-02~U-10과 같은 구멍).
            var closeBtn = MakeButton("close-button", "닫기", root, font);
            var closeLayout = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLayout.minHeight = 44f;
            closeLayout.preferredHeight = 44f;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                closeBtn.onClick, new UnityEngine.Events.UnityAction(panel.Hide));

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 펫 뽑기 확률 공개 화면(uGUI) 세움. " +
                      "MainHudUgui.gachaOddsPanel에 이 'PetGachaOdds'를 물려야 코드에서 여닫을 수 있다 — " +
                      "액션 줄에 여는 버튼은 아직 없어서 지금은 Play 중 Hierarchy에서 직접 켜서 봐야 한다.");
        }

        static void MakeOddsCard(RectTransform parent, TMP_FontAsset font, string prefix, string title, int rowCount)
        {
            var card = MakeCardContainer($"{prefix}-card", parent);
            MakeText($"{prefix}-title", title, card, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, true);
            for (var i = 0; i < rowCount; i++)
                MakeText($"{prefix}-row-{i}", "—", card, font, 14, Dim, 20f, TextAlignmentOptions.MidlineLeft, false);
            MakeText($"{prefix}-note", "", card, font, 12, Dim, 18f, TextAlignmentOptions.MidlineLeft, true);
        }

        static void MakeFusionCard(RectTransform parent, TMP_FontAsset font)
        {
            var card = MakeCardContainer("fusion-card", parent);
            MakeText("fusion-title", "합성 (중복 조각 → 다른 펫/등급)", card, font, 16, Ink, 22f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("fusion-same", "—", card, font, 14, Dim, 20f, TextAlignmentOptions.MidlineLeft, true);
            for (var i = 0; i < PromotionRows; i++)
                MakeText($"fusion-promo-{i}", "—", card, font, 14, Dim, 20f, TextAlignmentOptions.MidlineLeft, false);
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

        static Button MakeButton(string name, string label, RectTransform parent, TMP_FontAsset font)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = RowFace;
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
