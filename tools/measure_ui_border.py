# A-25 UI 스킨 — 9-slice 테두리(spriteBorder)를 그림에서 직접 잰다 (2026-09-29)
#
# 눈대중으로 넣으면 늘렸을 때 모서리가 뭉개지거나 겹친다. 그래서 잰다.
#
# **두께가 아니라 "모서리가 끝나는 지점"을 재야 한다.** 처음엔 가운데 가로선을 따라 걸으며
# 테두리 선 두께(4px 같은 값)를 재 봤는데, 알약형 버튼에서는 그게 답이 아니다 —
# 9-slice의 모서리 칸은 **둥근 부분을 전부 덮어야** 하므로 알약이면 좌우가 반높이(약 150px)다.
#
# 그래서 이렇게 잰다. 왼쪽을 예로 들면: 각 열의 (불투명 시작 행, 끝 행)과 색 표본이
# **가운데 열과 같아지는 첫 열**을 찾는다. 거기까지가 왼쪽 테두리다. 위/아래도 같은 식으로 행을 본다.
# 둥근 모서리·바깥 광선·위쪽 하이라이트 띠가 전부 이 판정에 걸린다.
#
#   python tools/measure_ui_border.py
import glob
import os
import sys

from PIL import Image

AT = 24    # 이 알파 이상을 "불투명"으로 본다
TOL = 14   # 같은 색으로 볼 채널 차이
PAD = 2    # 여유


def main():
    root = sys.argv[1] if len(sys.argv) > 1 else "PlanetRacer/Assets/Resources/Art/UI"
    for path in sorted(glob.glob(os.path.join(root, "*.png"))):
        im = Image.open(path).convert("RGBA")
        w, h = im.size
        px = im.load()

        def col(x):
            ys = [y for y in range(h) if px[x, y][3] >= AT]
            if not ys:
                return None
            t, b = ys[0], ys[-1]
            s = [px[x, t + (b - t) * k // 8] for k in range(1, 8)]
            return t, b, s

        def row(y):
            xs = [x for x in range(w) if px[x, y][3] >= AT]
            if not xs:
                return None
            l, r = xs[0], xs[-1]
            s = [px[l + (r - l) * k // 8, y] for k in range(1, 8)]
            return l, r, s

        def same(a, b):
            if a is None or b is None:
                return False
            if a[0] != b[0] or a[1] != b[1]:
                return False
            return all(all(abs(p - q) <= TOL for p, q in zip(u, v)) for u, v in zip(a[2], b[2]))

        cmid, rmid = col(w // 2), row(h // 2)
        left = right = top = bottom = 0
        for x in range(w // 2):
            if same(col(x), cmid):
                left = x
                break
        for x in range(w // 2):
            if same(col(w - 1 - x), cmid):
                right = x
                break
        for y in range(h // 2):
            if same(row(y), rmid):
                top = y
                break
        for y in range(h // 2):
            if same(row(h - 1 - y), rmid):
                bottom = y
                break
        b = (left + PAD, bottom + PAD, right + PAD, top + PAD)
        print("{0:24s} {1}  spriteBorder(L,B,R,T)={2}".format(os.path.basename(path), im.size, b))
    print()
    print("원본 픽셀 기준이다. maxTextureSize로 줄여도 Unity가 비율에 맞춰 같이 줄인다.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
