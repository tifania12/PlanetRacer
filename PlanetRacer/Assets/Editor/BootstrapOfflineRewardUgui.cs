using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// U-07(2026-09-16): 오프라인 보상 화면(OfflineRewardPanel, UI Toolkit)을 일반 UI(uGUI)로
    /// 세운다. `UI Canvas/Overlays` 아래에 붙지만, 다른 오버레이(U-02~U-06)와 달리 HUD 버튼이
    /// 없다 — 보상이 있을 때 스스로 뜬다(OfflineRewardUgui.Update가 매 프레임 확인).
    ///
    /// 그래서 구조가 한 겹 다르다. 루트 "OfflineReward"는 항상 켜 둔 채로 스크립트만 붙이고
    /// (꺼 버리면 Update가 안 돈다), 실제 화면(반투명 배경 + 가운데 카드)은 자식 "offline-reward-backdrop"
    /// 하나로 묶어서 그 GameObject만 스크립트가 여닫는다 — TutorialUgui의 bubble과 같은 요령이다.
    /// 닫기 버튼이 없다 — "받기"를 누르는 것 자체가 닫는 동작이라 U-02~U-06과 같은 구멍이 없다.
    ///
    /// 카드 폭은 원래 UXML의 "80%, 최대 420px"를 고정폭 380px로 단순화했다(다른 화면들도 카드
    /// 종류는 폭을 고정 픽셀로 잡는다 — 예: LootBox 400px). 세로/가로/태블릿 세 기준점 모두
    /// 화면 폭(500/920/871)보다 훨씬 좁아서 가운데 정렬만 하면 되고 재배치가 필요 없다 —
    /// 원래 UXML 주석("한 칸짜리 중앙 카드라 재배치할 게 없다")과 같은 이유.
    /// 줄 높이는 손계산이라 **Editor에서 실제로 봐야 한다** — 특히 wasted-label/treasure-label처럼
    /// 길어질 수 있는 문장이 카드 폭 안에서 두 줄까지 늘어날 수 있어 줄바꿈을 켜 두고 넉넉한
    /// 높이(40px)를 줬다.
    /// </summary>
    public static class BootstrapOfflineRewardUgui
    {
        static readonly Color Ink        = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim        = new Color(0.55f, 0.58f, 0.72f);
        static readonly Color Backdrop   = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color CardFace   = new Color(0.094f, 0.102f, 0.141f, 1f);
        static readonly Color BtnFace    = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color AdBtnFace  = new Color(0.157f, 0.173f, 0.227f, 1f);
        static readonly Color HighlightInk = new Color(0.824f, 0.784f, 0.353f);

        const float CardWidth = 380f;

        [MenuItem("GemRacer/21. 오프라인 보상 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("OfflineReward");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // 루트는 항상 켜 둔다 — Update가 계속 돌아야 보상이 생겼을 때 스스로 뜬다.
            var root = NewRect("OfflineReward", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;
            root.gameObject.AddComponent<OfflineRewardUgui>();

            // backdrop만 여닫는다 — 반투명 배경이 뒤(3D 뷰·HUD)로 클릭이 새지 않게 막는다.
            var backdrop = NewRect("offline-reward-backdrop", root);
            backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = Vector2.zero; backdrop.offsetMax = Vector2.zero;
            var bg = backdrop.gameObject.AddComponent<Image>();
            bg.color = Backdrop;
            bg.raycastTarget = true;

            // 카드는 화면 크기와 무관하게 가운데 고정폭(380px)으로 뜬다 — anchor를 스트레치하지
            // 않고 중앙 한 점(0.5,0.5)에 고정해서, 폭은 sizeDelta.x로 직접 주고 높이는
            // ContentSizeFitter가 내용에 맞춰 계산하게 한다. 세 기준점 폭(500/920/871) 모두
            // 380px보다 넉넉히 넓어서 재배치가 필요 없다(원래 UXML의 "한 칸짜리 중앙 카드"와 같은 이유).
            var card = NewRect("offline-reward-card", backdrop);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(CardWidth, 200f); // 높이는 초기값일 뿐, 아래 ContentSizeFitter가 다시 잡는다
            card.anchoredPosition = Vector2.zero;
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = CardFace;
            cardImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            cardImg.type = Image.Type.Sliced;

            var col = card.gameObject.AddComponent<VerticalLayoutGroup>();
            col.padding = new RectOffset(24, 24, 24, 24);
            col.spacing = 8f;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;
            col.childControlWidth = true;
            col.childControlHeight = true;

            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeText("offline-reward-title", "돌아온 것을 환영한다", card, font, 20, Ink, 28f, TextAlignmentOptions.Center, false);
            MakeText("elapsed-label", "자리를 비운 시간: —", card, font, 14, Ink, 22f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("counted-label", "인정된 시간: —", card, font, 14, Ink, 22f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("wasted-label", "—", card, font, 14, Dim, 40f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("minerals-label", "획득 원석: —", card, font, 14, Ink, 22f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("refined-label", "획득 정제 광물: —", card, font, 16, HighlightInk, 26f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("treasure-label", "—", card, font, 14, Ink, 40f, TextAlignmentOptions.MidlineLeft, true);

            var claimBtn = MakeButton("claim-button", "받기", card, font, BtnFace, 16);
            var claimLe = claimBtn.gameObject.AddComponent<LayoutElement>();
            claimLe.minHeight = 46f;
            claimLe.preferredHeight = 46f;

            // M-09 후속: 오늘 한도가 남아 있을 때만 OfflineRewardUgui가 이 둘을 보여준다.
            MakeText("double-ad-label", "—", card, font, 12, Dim, 34f, TextAlignmentOptions.MidlineLeft, true);
            var doubleBtn = MakeButton("double-claim-button", "광고 보고 2배 받기", card, font, AdBtnFace, 14);
            var doubleLe = doubleBtn.gameObject.AddComponent<LayoutElement>();
            doubleLe.minHeight = 40f;
            doubleLe.preferredHeight = 40f;

            backdrop.gameObject.SetActive(false); // 보상이 확정되기 전까지 숨겨 둔다(스크립트 Awake와 이중 안전장치)

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 오프라인 보상 화면(uGUI) 세움. HUD 버튼 없이 보상이 있을 때 스스로 뜬다.");
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
            labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero; labelRt.offsetMax = Vector2.zero;
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
