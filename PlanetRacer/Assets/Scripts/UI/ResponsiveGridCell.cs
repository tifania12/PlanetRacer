using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.UI
{
    /// <summary>
    /// U-11(2026-09-26 19시 Unity 배선 세션 발견): GridLayoutGroup.cellSize를 고정값으로 두면
    /// constraint가 Flexible이어도 가로가 넓어질 때 2열이 안 들어간다 — cellSize.x 자체가
    /// 커서 폭 나누기 셈이 항상 1로 나오기 때문이다(예: 420짜리 칸 두 개는 800짜리 화면에 안 들어감).
    /// 그래서 parent 폭을 보고 폭이 충분할 때만 cellSize를 절반 크기로 줄여 2열이 나오게 한다.
    /// grid.constraint는 그대로 Flexible로 둬야 한다 — 실제 열 개수는 Unity가 cellSize와 폭으로 계산한다.
    /// 이 컴포넌트는 열 개수를 직접 정하지 않고 칸 크기만 조절한다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class ResponsiveGridCell : MonoBehaviour
    {
        [Tooltip("세로(1열) 기준 칸 크기. Bootstrap의 CellWidth/CellHeight와 같은 값을 넣는다.")]
        public Vector2 baseCellSize = new Vector2(400f, 140f);

        [Tooltip("이 폭 이상이면 2열로 바꾼다. 태블릿 1280 폭에서도 2열까지만 허용한다(칸이 너무 작아지지 않게).")]
        public float twoColumnMinWidth = 700f;

        RectTransform _rect;
        GridLayoutGroup _grid;
        float _lastWidth = float.NaN;

        void OnEnable()
        {
            _rect = (RectTransform)transform;
            _grid = GetComponent<GridLayoutGroup>();
            _lastWidth = float.NaN;
            Apply();
        }

        void Update()
        {
            var width = _rect.rect.width;
            if (Mathf.Approximately(width, _lastWidth)) return;
            _lastWidth = width;
            Apply();
        }

        void Apply()
        {
            if (_grid == null || _rect == null) return;
            var width = _rect.rect.width;
            if (width <= 0f) return;

            if (width >= twoColumnMinWidth)
            {
                var spacing = _grid.spacing.x;
                var cellWidth = (width - spacing) / 2f;
                var scale = cellWidth / baseCellSize.x;
                _grid.cellSize = new Vector2(cellWidth, baseCellSize.y * scale);
            }
            else
            {
                _grid.cellSize = baseCellSize;
            }
        }
    }
}
