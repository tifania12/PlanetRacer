#!/usr/bin/env python3
"""펫 골격 한 장을 행성 색 변종으로 바꾼다.

1~4등급 64종을 전부 GPT로 뽑으면 64장인데, 그중 48장은 **같은 그림의 색만 다른 것**이다.
다시 뽑으면 색만 바뀌는 게 아니라 생김새까지 달라져서 "같은 종의 다른 색"으로 안 보인다.
그래서 골격 16장만 뽑고(쿼츠 색 원본) 나머지는 이 스크립트로 만든다.

방식: HSV에서 **색상(H)을 목표 색의 색상으로 돌리고, 채도(S)를 목표 색 쪽으로 당긴다.**
명도(V)는 건드리지 않는다 — 명암이 유지돼야 입체감이 남는다. 알파는 그대로 통과시킨다.
회색에 가까운 픽셀(채도가 아주 낮은 곳)은 금속·흰빛이라 그대로 둔다.

쓰는 법
    python tools/recolor_pet.py <골격.png> --out-dir <폴더>          # 5색 전부
    python tools/recolor_pet.py <골격.png> --planets ruby,sapphire   # 일부만
"""
import argparse
import colorsys
import os
import sys
from PIL import Image

# 행성 대표색 — GemColors와 같은 값을 쓴다
PLANETS = {
    "quartz":     "#E8EDFF",
    "ruby":       "#D94D4D",
    "sapphire":   "#4D6ED9",
    "aquamarine": "#59D9CC",
    "cinnabar":   "#D94D0F",
    "lapis":      "#2D4DA6",
}

GRAY_KEEP = 0.12   # 채도가 이보다 낮으면 금속·흰빛으로 보고 건드리지 않는다


def hex_to_hsv(h):
    h = h.lstrip("#")
    r, g, b = (int(h[i:i + 2], 16) / 255 for i in (0, 2, 4))
    return colorsys.rgb_to_hsv(r, g, b)


def recolor(im, target_hex, sat_pull=0.75):
    th, ts, _ = hex_to_hsv(target_hex)
    im = im.convert("RGBA")
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            hh, ss, vv = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if ss < GRAY_KEEP:      # 회색·흰빛은 그대로
                continue
            ns = ss + (ts - ss) * sat_pull
            nr, ng, nb = colorsys.hsv_to_rgb(th, max(0.0, min(1.0, ns)), vv)
            px[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
    return im


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("source")
    ap.add_argument("--out-dir", default=None, help="기본값: 원본과 같은 폴더")
    ap.add_argument("--planets", default="ruby,sapphire,aquamarine,cinnabar",
                    help="쉼표로 구분. 기본은 쿼츠 원본을 뺀 4색")
    ap.add_argument("--sat-pull", type=float, default=0.75)
    a = ap.parse_args()

    src = Image.open(a.source)
    if src.mode != "RGBA":
        print(f"[실패] {a.source} 가 RGBA가 아니다(모드 {src.mode}). 투명 PNG여야 한다.")
        return 1

    out_dir = a.out_dir or os.path.dirname(a.source) or "."
    os.makedirs(out_dir, exist_ok=True)
    stem = os.path.splitext(os.path.basename(a.source))[0]
    # 골격 이름이 'wheel' 이면 결과는 'wheel-ruby.png'
    stem = stem.replace("-quartz", "")

    made = 0
    for name in [p.strip() for p in a.planets.split(",") if p.strip()]:
        if name not in PLANETS:
            print(f"[건너뜀] 모르는 행성: {name}")
            continue
        out = os.path.join(out_dir, f"{stem}-{name}.png")
        recolor(src.copy(), PLANETS[name], a.sat_pull).save(out)
        print(f"  만듦: {out}")
        made += 1
    print(f"{made}장 생성")
    return 0 if made else 1


if __name__ == "__main__":
    sys.exit(main())
