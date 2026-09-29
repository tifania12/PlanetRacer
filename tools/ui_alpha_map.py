# A-25 진단 — `Resources/Art/UI/ui-*.png`의 알파 분포를 글자 지도로 찍는다 (2026-09-29)
#
# 9-slice로 쓰려면 "그림이 캔버스를 꽉 채우고, 가운데가 불투명 단색"이어야 한다.
# measure_ui_border.py가 장마다 엉뚱한 값을 내서(가운데 픽셀이 투명한 장이 있다) 먼저 눈으로 본다.
#
#   python tools/ui_alpha_map.py
#
# 칸: '#'=거의 불투명 / '+'=반투명 / '.'=거의 투명. 24x24 칸으로 줄여 찍는다.
import glob
import os
import sys

from PIL import Image

N = 24


def main():
    root = sys.argv[1] if len(sys.argv) > 1 else "PlanetRacer/Assets/Resources/Art/UI"
    for path in sorted(glob.glob(os.path.join(root, "*.png"))):
        im = Image.open(path).convert("RGBA")
        a = im.split()[3].resize((N, N), Image.BOX).load()
        rgb = im.resize((N, N), Image.BOX).load()
        print("== " + os.path.basename(path) + " " + str(im.size))
        for y in range(N):
            row = ""
            for x in range(N):
                v = a[x, y]
                row += "#" if v > 200 else ("+" if v > 40 else ".")
            print("   " + row)
        c = rgb[N // 2, N // 2]
        print("   가운데 색 " + str(c))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
