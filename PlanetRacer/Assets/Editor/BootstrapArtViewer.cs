using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// 2026-09-15: 아트 확인 화면을 씬에 세운다. `UI Canvas/Overlays` 아래에 붙는다.
    /// 평소에는 꺼져 있고 주소에 `?art=1` 이 있을 때만 열린다(ArtViewer.cs).
    ///
    /// 왜 필요한가 — Tifania가 이미지를 폴더에서 보는 대신 게임을 열어서 판단하고 싶어 한다.
    /// 그런데 아이콘 대부분은 아직 들어갈 화면이 없다. 그 사이를 이 화면이 메운다.
    /// </summary>
    public static class BootstrapArtViewer
    {
        static readonly Color Ink   = new Color(0.91f, 0.93f, 1f);
        static readonly Color Panel = new Color(0.05f, 0.06f, 0.11f, 0.97f);

        [MenuItem("GemRacer/15. 아트 확인 화면 세우기 (uGUI)")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Pretendard-Regular SDF.asset");
            if (font == null) { Debug.LogError("[GemRacer] 한글 폰트 에셋이 없다. 'GemRacer/11' 먼저."); return; }

            var overlays = GameObject.Find("UI Canvas/Overlays");
            if (overlays == null) { Debug.LogError("[GemRacer] 'UI Canvas/Overlays'가 없다. 'GemRacer/13' 먼저."); return; }

            var old = overlays.transform.Find("ArtViewer");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = New("ArtViewer", overlays.transform);
            Stretch(root);
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = Panel;                 // 확인 화면은 뒤를 다 가린다. 그림만 보게.

            var panel = root.gameObject.AddComponent<UiPanel>();
            panel.hiddenOnStart = true;
            root.gameObject.AddComponent<ArtViewer>();

            var title = New("art-title", root);
            title.anchorMin = new Vector2(0f, 1f);
            title.anchorMax = new Vector2(1f, 1f);
            title.pivot     = new Vector2(0.5f, 1f);
            title.offsetMin = new Vector2(16f, -60f);
            title.offsetMax = new Vector2(-16f, -16f);
            var t = title.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font; t.text = "아트 확인"; t.fontSize = 24; t.color = Ink;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;

            // 스크롤 영역 — 이미지가 32장이면 한 화면에 안 들어간다
            var viewport = New("viewport", root);
            viewport.anchorMin = new Vector2(0f, 0f);
            viewport.anchorMax = new Vector2(1f, 1f);
            viewport.offsetMin = new Vector2(12f, 12f);
            viewport.offsetMax = new Vector2(-12f, -68f);
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(1f, 1f, 1f, 0.02f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            var grid = New("art-grid", viewport);
            grid.anchorMin = new Vector2(0f, 1f);
            grid.anchorMax = new Vector2(1f, 1f);
            grid.pivot     = new Vector2(0.5f, 1f);
            grid.offsetMin = new Vector2(0f, 0f);
            grid.offsetMax = new Vector2(0f, 0f);
            scroll.content = grid;

            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(150f, 172f);
            g.spacing = new Vector2(8f, 8f);
            g.padding = new RectOffset(8, 8, 8, 8);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 3;
            var fit = grid.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Selection.activeObject = root.gameObject;
            EditorUtility.SetDirty(root.gameObject);
            Debug.Log("[GemRacer] 아트 확인 화면 세움. 주소에 ?art=1 을 붙이면 열린다. " +
                      "Resources/Art 아래 스프라이트를 전부 보여준다.");
        }

        static RectTransform New(string name, Transform parent)
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
    }
}
