# 그림의 바탕색을 원하는 색으로 끌어올린다 (2026-10-16, A-28-2 `ui-dot` 때문에 만들었다)
#
# GPT가 준 키트 그림 중에 속 색이 의도보다 어둡게 나온 장이 있다(`ui-dot`: #731F04,
# 원하는 건 #D94D0F). backlog에는 "`Image.color`로 곱하라"고 적혀 있었지만 **uGUI의
# `Image.color`는 곱셈이라 색을 밝게 만들 수 없다** — 어둡게만 된다. 그래서 그림 자체를 고친다.
#
#   python tools/relight_sprite.py <png> --from 731F04 --to D94D0F [--write]
#
# 채널별 배수(목표/현재)를 곱한다. 바탕색은 정확히 목표색이 되고, 이미 밝은 하이라이트는
# 255에서 잘려 흰색으로 남는다(점 하나짜리 그림이라 이게 더 자연스럽다). 알파는 건드리지 않는다.
import argparse

from PIL import Image


def hex2rgb(s):
    s = s.lstrip("#")
    return tuple(int(s[i:i + 2], 16) for i in (0, 2, 4))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("path")
    ap.add_argument("--from", dest="src", required=True, help="지금 바탕색 (RRGGBB)")
    ap.add_argument("--to", dest="dst", required=True, help="원하는 바탕색 (RRGGBB)")
    ap.add_argument("--write", action="store_true", help="실제로 덮어쓴다. 없으면 미리보기만")
    a = ap.parse_args()

    src, dst = hex2rgb(a.src), hex2rgb(a.dst)
    k = [dst[i] / max(1, src[i]) for i in range(3)]
    print("배수 R×%.3f G×%.3f B×%.3f" % tuple(k))

    im = Image.open(a.path).convert("RGBA")
    r, g, b, al = im.split()
    out = Image.merge("RGBA", (
        r.point(lambda v: min(255, int(v * k[0] + 0.5))),
        g.point(lambda v: min(255, int(v * k[1] + 0.5))),
        b.point(lambda v: min(255, int(v * k[2] + 0.5))),
        al))

    w, h = im.size
    px = out.load()
    tot, n = [0, 0, 0], 0
    for y in range(h // 2 - h // 8, h // 2 + h // 8):
        for x in range(w // 2 - w // 8, w // 2 + w // 8):
            pr, pg, pb, pa = px[x, y]
            if pa > 200:
                tot[0] += pr; tot[1] += pg; tot[2] += pb; n += 1
    print("고친 뒤 속 평균 #%02X%02X%02X (원하던 것 #%02X%02X%02X)"
          % (tot[0] // n, tot[1] // n, tot[2] // n, dst[0], dst[1], dst[2]))

    if a.write:
        out.save(a.path)
        print("덮어썼다:", a.path)
    else:
        print("미리보기만 했다. 실제로 고치려면 --write")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
