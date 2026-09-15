#!/usr/bin/env python3
"""GPT가 준 이미지에서 단색 배경(기본: 마젠타 #FF00FF)을 빼서 투명하게 만든다.

GPT는 투명 PNG를 잘 안 준다. 그래서 아이콘을 요청할 때 배경을 순수 마젠타로 달라고 하고,
여기서 그 색을 알파 0으로 바꾼다. 마젠타를 쓰는 이유는 게임 아트에 거의 안 쓰이는 색이라
그림 본체와 겹칠 일이 없기 때문이다.

쓰는 법:
    python tools/strip_bg.py Assets/Art/Icons/engine.png
    python tools/strip_bg.py Assets/Art/Icons/*.png --tolerance 40

경계가 지저분하면 tolerance를 올린다. 너무 올리면 그림 본체가 파인다 —
결과를 눈으로 보고 정한다.
"""
import argparse
import glob
import sys

try:
    from PIL import Image
except ImportError:
    sys.exit("Pillow가 필요하다:  pip install Pillow")


def strip(path: str, key=(255, 0, 255), tolerance: int = 30, feather: bool = True) -> str:
    img = Image.open(path).convert("RGBA")
    px = img.load()
    w, h = img.size
    kr, kg, kb = key
    removed = 0

    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            # 채널별 거리의 합으로 본다. 제곱근까지 갈 필요 없다.
            d = abs(r - kr) + abs(g - kg) + abs(b - kb)
            if d <= tolerance:
                px[x, y] = (r, g, b, 0)
                removed += 1
            elif feather and d <= tolerance * 3:
                # 경계 한 겹을 반투명으로 — 계단 현상을 줄인다
                t = (d - tolerance) / float(tolerance * 2)
                px[x, y] = (r, g, b, int(a * min(1.0, t)))

    out = path
    img.save(out)
    pct = 100.0 * removed / (w * h)
    print(f"{path}: {w}x{h}, 배경 {pct:.1f}% 제거")
    if pct < 5:
        print("  주의: 제거된 픽셀이 너무 적다. 배경이 마젠타가 아닐 수 있다.")
    if pct > 90:
        print("  주의: 거의 다 지워졌다. tolerance가 너무 크거나 빈 이미지다.")
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("paths", nargs="+")
    ap.add_argument("--tolerance", type=int, default=30)
    ap.add_argument("--key", default="FF00FF", help="뺄 배경색 헥사 (기본 FF00FF)")
    ap.add_argument("--no-feather", action="store_true")
    a = ap.parse_args()

    k = a.key.lstrip("#")
    key = (int(k[0:2], 16), int(k[2:4], 16), int(k[4:6], 16))

    files = []
    for p in a.paths:
        files.extend(glob.glob(p))
    if not files:
        sys.exit("처리할 파일이 없다.")

    for f in files:
        strip(f, key=key, tolerance=a.tolerance, feather=not a.no_feather)


if __name__ == "__main__":
    main()
