using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-26(2026-10-17): 타이틀 화면을 세운다. `Title/title-keyart`·`title-emblem` 두 장이
    /// 여기 붙고, 게임 이름 「보석 행성 레이서」는 폰트로 얹는다(그림에 글자가 없다).
    ///
    /// 화물칸 가득(메뉴 24)과 같은 구조다 — 루트 "Title"은 항상 켜 두고 자식 "title-veil"만
    /// 스크립트가 여닫는다. UiPanel은 달지 않는다(HUD 버튼으로 여는 화면이 아니다).
    ///
    /// **형제 순서를 맨 뒤로 보낸다.** uGUI는 형제 순서가 그리는 순서라, 가림막이 다른 화면들보다
    /// 뒤에 있어야 위에 그려진다 — 화물칸 가득이 상점보다 **앞**으로 간 것과 정확히 반대 이유다.
    /// 오프라인 보상 팝업은 타이틀 아래에서 뜨고, 넘어가면 그 자리에 그대로 보인다.
    ///
    /// 키아트는 941x1672(세로 9:16)라 세로 화면을 꽉 채운다. 가로·태블릿에서 찌그러지지 않게
    /// `AspectRatioFitter(EnvelopeParent)`로 덮는다 — 비율을 지키면서 칸을 넘치게 키우는 모드라
    /// 넘치는 쪽이 잘린다. `preserveAspect`만으로는 남는 쪽에 빈 띠가 생긴다.
    /// 뒤에 깔린 `title-veil` 자체가 어두운 면이라 혹시 모자라도 검은 바탕이 받친다.
    ///
    /// 멱등 — 다시 눌러도 기존 "Title"을 지우고 같은 모양으로 다시 만든다.
    /// </summary>
    public static class BootstrapTitleUgui
    {
        static readonly Color Ink      = new Color(0.91f, 0.93f, 1f);
        static readonly Color Dim      = new Color(0.72f, 0.75f, 0.86f);
        static readonly Color VeilFace = new Color(0.043f, 0.047f, 0.078f, 1f); // 키아트 뒤를 받치는 어두운 면
        static readonly Color Scrim    = new Color(0f, 0f, 0f, 0.35f);          // 글자가 읽히게 한 겹

        [MenuItem("GemRacer/46. 타이틀 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("Title");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // 루트는 항상 켜 둔다 — 넘어간 뒤에도 Update가 돌아야 오프라인 복귀에 다시 뜬다.
            var root = NewRect("Title", overlays.transform);
            Stretch(root);
            root.gameObject.AddComponent<TitleUgui>();

            // 가림막. 이 노드 하나가 화면 전체를 덮는 버튼이기도 하다.
            var veil = NewRect("title-veil", root);
            Stretch(veil);
            var veilImg = veil.gameObject.AddComponent<Image>();
            veilImg.color = VeilFace;          // 그림 없이 면만 — 회색 상자(UISprite.psd)를 쓰지 않는다
            veilImg.raycastTarget = true;
            veil.gameObject.AddComponent<CanvasGroup>();
            var tap = veil.gameObject.AddComponent<Button>();
            tap.targetGraphic = veilImg;
            tap.transition = Selectable.Transition.None;  // 누를 때 화면 전체가 번쩍이면 안 된다

            // 키아트. 비율을 지키면서 칸을 덮는다(넘치는 쪽이 잘린다).
            var keyart = NewRect("title-keyart", veil);
            Stretch(keyart);
            var keyImg = keyart.gameObject.AddComponent<Image>();
            keyImg.sprite = LoadSprite("Assets/Resources/Art/Title/title-keyart.png");
            keyImg.raycastTarget = false;
            keyImg.preserveAspect = false;     // 비율은 아래 Fitter가 맡는다
            var fit = keyart.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 941f / 1672f;

            // 글자가 읽히게 한 겹 어둡게. 키아트가 밝은 쪽이어도 이름이 묻히지 않는다.
            var scrim = NewRect("title-scrim", veil);
            Stretch(scrim);
            var scrimImg = scrim.gameObject.AddComponent<Image>();
            scrimImg.color = Scrim;
            scrimImg.raycastTarget = false;

            // 엠블럼 — 위쪽 가운데. 1254 정사각에 그림이 거의 꽉 차 있어 자를 것이 없다.
            var emblem = NewRect("title-emblem", veil);
            emblem.anchorMin = new Vector2(0.5f, 1f);
            emblem.anchorMax = new Vector2(0.5f, 1f);
            emblem.pivot     = new Vector2(0.5f, 1f);
            emblem.sizeDelta = new Vector2(260f, 260f);
            emblem.anchoredPosition = new Vector2(0f, -150f);
            var emblemImg = emblem.gameObject.AddComponent<Image>();
            emblemImg.sprite = LoadSprite("Assets/Resources/Art/Title/title-emblem.png");
            emblemImg.preserveAspect = true;
            emblemImg.raycastTarget = false;

            // 게임 이름. 그림에 글자가 없으니 여기서 얹는다.
            var name = MakeText("title-name", "보석 행성 레이서", veil, font, 40f, Ink);
            name.anchorMin = new Vector2(0.5f, 1f);
            name.anchorMax = new Vector2(0.5f, 1f);
            name.pivot     = new Vector2(0.5f, 1f);
            name.sizeDelta = new Vector2(460f, 72f);
            name.anchoredPosition = new Vector2(0f, -436f);

            // 누르라는 안내는 아래쪽에. 스크립트가 같은 문구를 다시 넣는다(다시 떴을 때를 위해).
            var hint = MakeText("title-hint", "아무 곳이나 누르세요", veil, font, 16f, Dim);
            hint.anchorMin = new Vector2(0.5f, 0f);
            hint.anchorMax = new Vector2(0.5f, 0f);
            hint.pivot     = new Vector2(0.5f, 0f);
            hint.sizeDelta = new Vector2(460f, 36f);
            hint.anchoredPosition = new Vector2(0f, 120f);

            // 맨 뒤 형제 = 맨 위에 그려진다. 다른 화면들 위를 덮어야 가림막 구실을 한다.
            root.SetAsLastSibling();

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log($"[GemRacer] 타이틀 화면(uGUI) 세움. 형제 순서 {root.GetSiblingIndex()}(맨 뒤 = 맨 위). " +
                      "키아트·엠블럼 두 장 물렸고 이름은 폰트로 얹었다. 아무 곳이나 누르면 넘어간다.");
        }

        // --- 조각 만들기 ---------------------------------------------------

        static Sprite LoadSprite(string path)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp == null) Debug.LogWarning($"[GemRacer] '{path}'를 Sprite로 못 읽었다. 'GemRacer/90' 먼저 눌러 임포트 설정을 맞출 것.");
            return sp;
        }

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

        static RectTransform MakeText(string name, string text, RectTransform parent, TMP_FontAsset font,
                                      float size, Color color)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            t.enableAutoSizing = true;      // 가로 화면에서 칸이 좁아지면 잘리는 대신 줄어든다
            t.fontSizeMin = 12f;
            t.fontSizeMax = size;
            return rt;
        }
    }
}
