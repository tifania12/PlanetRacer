#!/usr/bin/env python3
"""`check_alpha.py`의 마젠타 판정에 걸린 픽셀이 **어떤 색인지** 보여 준다.

왜 필요한가 (2026-09-21 04시 이미지 세션).
    `check_alpha.py`의 마젠타 검사는 `r>150 and b>150 and g<100`이다.
    이건 순수 #FF00FF만이 아니라 **보라 전체**를 잡는다. 광석족(`ore-*`)은
    프롬프트가 `faceted crystal cluster` + `rich jewel tones`라서 자수정 보라가
    몸통 전체를 덮는다 - 그래서 34연속으로 이 검사에 막혔다.
    이 스크립트는 "키잉 잔상이라서 막힌 것인지, 그냥 보라 생물이라 막힌 것인지"를
    숫자로 가른다. 검사 기준을 바꾸지 않고 **보기만** 한다.

읽는 법.
    - `#FF00FF 근처`가 걸린 픽셀의 극히 일부다  -> 키잉 잔상이 아니다(오탐)
    - `반투명(alpha<250)`이 극히 일부다          -> 테두리 잔상이 아니라 몸통 색이다
    - `가장 많은 색`이 rgb(170,65,240)쯤이다     -> 자수정 보라, 즉 그림 그 자체다

쓰는 법
    python tools/check_magenta_kind.py <파일>...
"""
import sys
from PIL import Image


def probe(path):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    flagged = []
    for y in range(h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al > 32 and r > 150 and b > 150 and g < 100:
                flagged.append((x, y, r, g, b, al))
    n = len(flagged)
    print(f"{path}: 걸린 픽셀 {n}")
    if not n:
        return
    pure = [p for p in flagged if p[2] > 240 and p[4] > 240 and p[3] < 40]
    near = [p for p in flagged if p[2] > 220 and p[4] > 220 and p[3] < 60]
    print(f"  #FF00FF 근처(r>240,b>240,g<40): {len(pure)}")
    print(f"  느슨하게(r>220,b>220,g<60): {len(near)}")
    edge = [p for p in flagged if p[5] < 250]
    print(f"  반투명(alpha<250) 픽셀: {len(edge)}")
    pure_edge = [p for p in pure if p[5] < 250]
    print(f"  #FF00FF 근처 + 반투명 (= 진짜 키잉 잔상의 모습): {len(pure_edge)}")
    from collections import Counter
    c = Counter((p[2] // 16 * 16, p[3] // 16 * 16, p[4] // 16 * 16) for p in flagged)
    print("  가장 많은 색 상위 8 (16단위 양자화):")
    for col, cnt in c.most_common(8):
        print(f"    rgb{col}  {cnt}  ({cnt * 100 / n:.1f}%)")


def main():
    paths = sys.argv[1:]
    if not paths:
        print("쓰는 법: python tools/check_magenta_kind.py <파일> [파일...]")
        return 2
    for p in paths:
        probe(p)
        print()
    return 0


if __name__ == "__main__":
    sys.exit(main())
