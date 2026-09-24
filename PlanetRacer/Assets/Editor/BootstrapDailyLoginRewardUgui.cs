using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// D18-N 남은 절반(2026-09-25): 하루 첫 접속 보상 + 구독 매일 지급 확인 팝업을
    /// `UI Canvas/Overlays` 아래에 세운다. BootstrapOfflineRewardUgui와 같은 구조 —
    /// HUD 버튼이 없고 보상이 있을 때 스스로 뜬다(DailyLoginRewardUgui.Update가 매 프레임 확인).
    /// 루트 "DailyLoginReward"는 항상 켜 둔 채로 스크립트만 붙이고, 실제 화면은 자식
    /// "daily-reward-backdrop" 하나로 묶어서 그 GameObject만 스크립트가 여닫는다.
    ///
    /// 카드 폭은 OfflineReward와 같은 380px 고정 — 세 기준점(500/920/871) 모두 넉넉히 넓다.
    /// </summary>
    public static class BootstrapDailyLoginRewardUgui
    {
        static readonly Color Ink          = new Color(0.91f, 0.93f, 1f);
        static readonly Color Backdrop     = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color CardFace     = new Color(0.094f, 0.102f, 0.141f, 1f);
        static readonly Color BtnFace      = new Color(0.28f, 0.34f, 0.62f, 1f);
        static readonly Color HighlightInk = new Color(0.824f, 0.784f, 0.353f);

        const float CardWidth = 380f;

        [MenuItem("GemRacer/31. 접속·구독 보상 팝업 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("DailyLoginReward");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = NewRect("DailyLoginReward", overlays.transform);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;
            root.gameObject.AddComponent<DailyLoginRewardUgui>();

            var backdrop = NewRect("daily-reward-backdrop", root);
            backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = Vector2.zero; backdrop.offsetMax = Vector2.zero;
            var bg = backdrop.gameObject.AddComponent<Image>();
            bg.color = Backdrop;
            bg.raycastTarget = true;

            var card = NewRect("daily-reward-card", backdrop);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(CardWidth, 200f); // 높이는 초기값, 아래 ContentSizeFitter가 다시 잡는다
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

            MakeText("daily-reward-title", "오늘의 보상", card, font, 20, Ink, 28f, TextAlignmentOptions.Center, false);
            MakeText("streak-label", "—", card, font, 14, Ink, 22f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("login-reward-label", "—", card, font, 16, HighlightInk, 26f, TextAlignmentOptions.MidlineLeft, true);
            MakeText("subscription-reward-label", "—", card, font, 16, HighlightInk, 26f, TextAlignmentOptions.MidlineLeft, true);

            var confirmBtn = MakeButton("confirm-button", "확인", card, font, BtnFace, 16);
            var confirmLe = confirmBtn.gameObject.AddComponent<LayoutElement>();
            confirmLe.minHeight = 46f;
            confirmLe.preferredHeight = 46f;

            backdrop.gameObject.SetActive(false); // 지급이 확정되기 전까지 숨겨 둔다(스크립트 Awake와 이중 안전장치)

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 접속·구독 보상 팝업(uGUI) 세움. HUD 버튼 없이 보상이 있을 때 스스로 뜬다.");
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
