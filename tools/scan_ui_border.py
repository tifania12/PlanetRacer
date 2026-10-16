# 9-slice 테두리를 알파 띠로 찾는다 (2026-10-16, A-28-2 「테두리 못 정한 그림 다섯」)
#
# measure_ui_border.py는 가운데 장식(말풍선 꼬리·등급 돌기·리본 매듭)에서 판정이 멈춰서
# 좌우 합이 그림 크기를 넘는 값을 내놓는 장이 있다. 2026-10-16 07시 세션이 `ui-popup-title`을
# 정할 때 쓴 방법이 더 잘 들어서 그걸 스크립트로 옮겼다 — **알파를 열별로 훑어
# 띠 높이(위·아래 끝)가 일정해지는 구간**을 찾고, 그 바깥을 테두리로 잡는다.
# 늘어나도 되는 곳은 "모양이 변하지 않는 평평한 구간"이라는 뜻이다.
#
#   python tools/scan_ui_border.py PlanetRacer/Assets/Resources/Art/UI/Kit/ui-tooltip.png ...
#
# 찍어 주는 것: 가장 긴 평평 구간, 거기서 나온 Vector4(left, bottom, right, top),
# 그리고 열·행별 끝값을 20칸으로 줄인 표(눈으로 한 번 더 보라고).
import sys

from PIL import Image

A_MIN = 40          # 이 위면 "있다"로 본다 (ui_alpha_map.py의 '+' 기준과 같게)


def band(alpha, w, h, by_column):
    """열(또는 행)마다 불투명 구간의 시작·끝을 돌려준다. 빈 줄은 None."""
    out = []
    n, m = (w, h) if by_column else (h, w)
    for i in range(n):
        lo, hi = None, None
        for j in range(m):
            v = alpha[i, j] if by_column else alpha[j, i]
            if v > A_MIN:
                if lo is None:
                    lo = j
                hi = j
        out.append(None if lo is None else (lo, hi))
    return out


def longest_flat(bands, tol=1):
    """끝값이 거의 그대로인 가장 긴 구간 (start, end) 을 찾는다. tol은 허용 흔들림(px)."""
    best = (0, -1)
    i = 0
    n = len(bands)
    while i < n:
        if bands[i] is None:
            i += 1
            continue
        lo, hi = bands[i]
        j = i + 1
        while j < n and bands[j] is not None \
                and abs(bands[j][0] - lo) <= tol and abs(bands[j][1] - hi) <= tol:
            j += 1
        if j - 1 - i > best[1] - best[0]:
            best = (i, j - 1)
        i = j
    return best


def digest(bands, cells=20):
    """끝값 표를 cells칸으로 줄여 찍는다."""
    n = len(bands)
    rows = []
    for k in range(cells):
        i = min(n - 1, k * n // cells)
        b = bands[i]
        rows.append("%4d:%s" % (i, "----" if b is None else "%d-%d" % b))
    return "  ".join(rows)


def main():
    if len(sys.argv) < 2:
        print("쓰는 법: python tools/scan_ui_border.py <png> [<png> ...]")
        return 2
    for path in sys.argv[1:]:
        im = Image.open(path).convert("RGBA")
        w, h = im.size
        alpha = im.split()[3].load()

        cols = band(alpha, w, h, True)
        rows = band(alpha, w, h, False)
        cl, cr = longest_flat(cols)      # x 방향 평평 구간 → 좌·우 테두리
        rt, rb = longest_flat(rows)      # y 방향 평평 구간 → 위·아래 테두리

        left, right = cl, w - 1 - cr
        top, bottom = rt, h - 1 - rb     # PNG는 위가 y=0, Unity 테두리는 (왼,아래,오른,위)

        print("== %s  %dx%d" % (path.split("/")[-1], w, h))
        print("   열 평평 구간 x=%d~%d (%d칸)  → 왼 %d · 오른 %d" % (cl, cr, cr - cl + 1, left, right))
        print("   행 평평 구간 y=%d~%d (%d칸)  → 위 %d · 아래 %d" % (rt, rb, rb - rt + 1, top, bottom))
        ok = (left + right) < w and (top + bottom) < h
        print("   Vector4(%df, %df, %df, %df)   %s"
              % (left, bottom, right, top, "" if ok else "← 합이 크기를 넘는다. 눈으로 볼 것"))
        print("   열 끝값: " + digest(cols))
        print("   행 끝값: " + digest(rows))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
