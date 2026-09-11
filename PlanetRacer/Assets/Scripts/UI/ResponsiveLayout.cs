using UnityEngine;
using UnityEngine.UIElements;

namespace GemRacer.UI
{
    /// <summary>
    /// UI Toolkit엔 CSS 미디어 쿼리가 없어서, 화면 비율을 직접 보고 USS 클래스를 갈아 끼운다.
    /// 폭이 높이보다 좁으면(세로) "portrait", 아니면(가로) "landscape" 클래스를 루트에 붙인다.
    /// 세 기준점(세로 540x960 / 가로 960x540 / 태블릿 1280x800)이 전부 폭 vs 높이 비교
    /// 하나로 두 그룹으로 갈리기 때문에 이 정도로 충분하다(CLAUDE.md 6번 규칙).
    /// 실제 배치(위/아래 ↔ 왼쪽/오른쪽)는 Assets/UI/Root.uss가 그 클래스를 보고 정한다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ResponsiveLayout : MonoBehaviour
    {
        const string PortraitClass = "portrait";
        const string LandscapeClass = "landscape";

        VisualElement _root = null!;
        int _lastWidth = -1, _lastHeight = -1;

        void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Apply();
        }

        void OnDisable()
        {
            _root?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt) => Apply();

        void Apply()
        {
            int w = Screen.width, h = Screen.height;
            if (w == _lastWidth && h == _lastHeight) return;
            _lastWidth = w; _lastHeight = h;

            bool portrait = w < h;
            _root.EnableInClassList(PortraitClass, portrait);
            _root.EnableInClassList(LandscapeClass, !portrait);
        }
    }
}
