using UnityEngine;
using UnityEngine.UI;

namespace GemRacer.UI
{
    /// <summary>
    /// U-11(2026-09-26): GridLayoutGroup을 쓰는 화면들이 가로에서 한 칸짜리로 남아
    /// 오른쪽 절반이 비던 문제를 공통 규칙으로 잡는다.
    ///
    /// 지금까지 부트스트랩들이 `cellSize`를 400~420으로 고정해 둬서, 폭 800짜리 가로
    /// 화면에서도 두 칸(400×2 + 간격 12 = 812)이 안 들어가 Flexible이 한 칸만 놓았다.
    /// 그래서 칸 폭을 고정값으로 두지 않고 **부모 폭에서 계산한다** — 들어갈 수 있는
    /// 칸 수를 먼저 정하고(minCellWidth 기준, maxColumns까지), 남은 폭을 그 칸 수로
    /// 정확히 나눈다. 세로(540)에서는 한 칸이 그대로 나오니 지금 모습이 안 바뀌고,
    /// 가로(960)·태블릿(1280)에서는 두 칸으로 재배치된다.
    /// CLAUDE.md 6번("화면 구성과 정보는 같고 배치만 바뀐다")이 말하는 그 재배치다.
    ///
    /// 칸 높이는 건드리지 않는다 — 카드 안의 글자 크기가 그대로여야 세 기준점에서
    /// 같은 화면으로 읽힌다.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    [DisallowMultipleComponent]
    public sealed class ResponsiveGridCells : MonoBehaviour
    {
        [Tooltip("이 폭보다 좁아지면 칸을 하나 줄인다. 카드 안 글자가 읽히는 최소 폭.")]
        public float minCellWidth = 330f;

        [Tooltip("아무리 넓어도 이 칸 수를 넘기지 않는다. 태블릿에서 세 칸이 되면 글자가 너무 작아진다.")]
        public int maxColumns = 2;

        GridLayoutGroup _grid = null!;
        RectTransform _rect = null!;
        float _lastWidth = -1f;

        void Awake()
        {
            _grid = GetComponent<GridLayoutGroup>();
            _rect = (RectTransform)transform;
        }

        void OnEnable()
        {
            _lastWidth = -1f;
            Apply();
        }

        /// <summary>부모가 리사이즈되면(회전, 창 크기 변경, 기준점 전환) 여기로 들어온다.</summary>
        void OnRectTransformDimensionsChange()
        {
            if (_grid == null) return;
            Apply();
        }

        void Apply()
        {
            float width = _rect.rect.width;
            if (width <= 1f) return; // 레이아웃이 아직 한 번도 안 돈 프레임

            // 같은 폭으로 또 부르면 아무것도 하지 않는다 — cellSize를 건드리면 레이아웃이
            // 다시 돌고, 그게 또 여기로 들어와서 무한히 오갈 수 있다.
            if (Mathf.Abs(width - _lastWidth) < 0.5f) return;
            _lastWidth = width;

            float inner = width - _grid.padding.left - _grid.padding.right;
            if (inner <= 0f) return;

            float spacing = _grid.spacing.x;
            int columns = Mathf.FloorToInt((inner + spacing) / Mathf.Max(1f, minCellWidth + spacing));
            columns = Mathf.Clamp(columns, 1, Mathf.Max(1, maxColumns));

            float cellWidth = (inner - spacing * (columns - 1)) / columns;
            if (cellWidth <= 0f) return;

            // Flexible에 맡기지 않고 칸 수를 못 박는다 — 칸 폭을 딱 맞게 계산해 놓아도
            // 소수점 때문에 한 칸이 밀려 내려가는 일이 있다.
            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = columns;
            _grid.cellSize = new Vector2(cellWidth, _grid.cellSize.y);
        }
    }
}
