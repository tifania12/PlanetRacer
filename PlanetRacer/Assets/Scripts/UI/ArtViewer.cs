using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.UI
{
    /// <summary>
    /// 2026-09-15: 아트 확인 화면.
    ///
    /// Tifania가 이미지를 폴더에서 한 장씩 열어 보는 대신 **게임을 열었을 때** 보고
    /// "이건 고쳐야겠다"를 판단하고 싶다고 했다. 그런데 아이콘 대부분은 아직 들어갈 화면이
    /// 없다(업그레이드·제작·상자 화면이 아직 안 옮겨졌다). 그래서 들어갈 자리가 생기기 전까지
    /// **게임 안에서 한꺼번에 볼 수 있는 화면**을 따로 둔다.
    ///
    /// 여는 법 — 주소에 `?art=1` 을 붙인다.
    ///
    ///     https://planetracer-daz.pages.dev/?art=1
    ///     https://planetracer-daz.pages.dev/?art=1&amp;fast=20
    ///
    /// `Resources/Art` 아래의 스프라이트를 전부 긁어서 이름과 함께 격자로 뿌린다.
    /// 새 이미지가 들어와도 코드를 고칠 필요가 없다 — 폴더에 넣기만 하면 여기 나온다.
    ///
    /// 실제 화면들이 다 옮겨지면 이 화면은 없애도 된다. 그때까지는 이게 아트 확인 창구다.
    /// </summary>
    public sealed class ArtViewer : MonoBehaviour
    {
        [Tooltip("한 줄에 몇 개씩 놓을지. 세로 화면 기준.")]
        public int columns = 3;

        [Tooltip("칸 하나의 크기(px, 기준 해상도 540x960에서).")]
        public float cellSize = 150f;

        UiPanel _panel;
        RectTransform _grid;
        TMP_Text _title;
        bool _built;

        void Awake()
        {
            _panel = GetComponent<UiPanel>();
            _grid  = UiKit.Find<RectTransform>(transform, "art-grid", warnIfMissing: false);
            _title = UiKit.Find<TMP_Text>(transform, "art-title", warnIfMissing: false);
        }

        void Start()
        {
            // 주소에 ?art=1 이 있을 때만 연다. 평소에는 아예 안 보인다.
            bool wanted = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            wanted = WantsArtViewer(Application.absoluteURL);
#endif
            if (!wanted) { _panel?.Hide(); return; }

            Build();
            _panel?.Show();
        }

        public static bool WantsArtViewer(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            int i = url.IndexOf("art=", System.StringComparison.OrdinalIgnoreCase);
            if (i < 0) return false;
            int v = i + 4;
            return v < url.Length && url[v] != '0';
        }

        /// <summary>에디터에서 버튼 하나로 열어 보고 싶을 때 쓰라고 남겨 둔다.</summary>
        public void OpenFromEditor() { Build(); _panel?.Show(); }

        void Build()
        {
            if (_built || _grid == null) return;
            _built = true;

            // Resources/Art 아래 전부. 이름순으로 정렬해야 매번 같은 자리에 나온다.
            var sprites = Resources.LoadAll<Sprite>("Art").OrderBy(s => s.name).ToList();

            if (_title != null)
                _title.text = sprites.Count > 0
                    ? $"아트 확인 — {sprites.Count}개"
                    : "아트 확인 — Resources/Art 가 비어 있다";

            foreach (var sp in sprites) AddCell(sp);
            Debug.Log($"[GemRacer] 아트 확인 화면: 스프라이트 {sprites.Count}개 표시");
        }

        void AddCell(Sprite sp)
        {
            var cell = new GameObject(sp.name, typeof(RectTransform));
            cell.transform.SetParent(_grid, false);
            var crt = (RectTransform)cell.transform;
            crt.sizeDelta = new Vector2(cellSize, cellSize + 22f);

            var imgGo = new GameObject("img", typeof(RectTransform));
            imgGo.transform.SetParent(cell.transform, false);
            var irt = (RectTransform)imgGo.transform;
            irt.anchorMin = new Vector2(0f, 0f);
            irt.anchorMax = new Vector2(1f, 1f);
            irt.offsetMin = new Vector2(4f, 26f);
            irt.offsetMax = new Vector2(-4f, -4f);
            var img = imgGo.AddComponent<Image>();
            img.sprite = sp;
            img.preserveAspect = true;
            img.raycastTarget = false;

            var labGo = new GameObject("name", typeof(RectTransform));
            labGo.transform.SetParent(cell.transform, false);
            var lrt = (RectTransform)labGo.transform;
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(1f, 0f);
            lrt.offsetMin = new Vector2(2f, 2f);
            lrt.offsetMax = new Vector2(-2f, 24f);
            var lab = labGo.AddComponent<TextMeshProUGUI>();
            lab.text = sp.name;
            lab.fontSize = 13;
            lab.color = new Color(0.55f, 0.58f, 0.72f);
            lab.alignment = TextAlignmentOptions.Center;
            lab.raycastTarget = false;
            lab.enableWordWrapping = false;
            lab.overflowMode = TextOverflowModes.Ellipsis;
        }
    }
}
