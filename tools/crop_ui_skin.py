# A-25 UI 스킨 — 9-slice로 쓸 수 있게 여백을 잘라낸다 (2026-09-29)
#
# GPT가 준 `Resources/Art/UI/ui-*.png`는 1254x1254 캔버스 가운데에 모양이 작게 들어 있고
# 나머지는 전부 투명이다(`tools/ui_alpha_map.py`로 확인). 이 상태로는 9-slice가 성립하지 않는다 —
# spriteBorder의 모서리 칸이 "투명 여백 + 실제 테두리"가 되어 버려서, 96x48 같은 작은 버튼에
# 얹으면 모서리끼리 겹치고 테두리가 뭉개진다.
#
# 그래서 **모양의 경계 상자로 자른다.** 알파 문턱값을 두고 자르는 것이 중요하다 —
# `Image.getbbox()`는 알파 1짜리 유령 픽셀 하나에도 걸려서 상자가 엉뚱하게 커진다
# (ui-button이 실제로 그랬다: 모양은 y 470~783인데 getbbox는 y 474~1150을 줬다).
#
# 원본은 git 이력에 남는다(자르기 전 커밋). 되돌리려면 `git show <이전 커밋>:<경로>`.
#
#   python tools/crop_ui_skin.py            # 잰 값만 보여준다
#   python tools/crop_ui_skin.py --write    # 실제로 자른다
import glob
import os
import sys

from PIL import Image

THRESH = 24   # 이 알파 미만은 여백으로 본다
MARGIN = 2    # 자른 뒤 네 변에 남기는 투명 여백(안티에일리어싱이 잘리지 않게)


def content_box(im, thresh=THRESH):
    a = im.split()[3]
    return a.point(lambda v: 255 if v >= thresh else 0).getbbox()


def main():
    write = "--write" in sys.argv
    root = "PlanetRacer/Assets/Resources/Art/UI"
    for path in sorted(glob.glob(os.path.join(root, "*.png"))):
        im = Image.open(path).convert("RGBA")
        box = content_box(im)
        if box is None:
            print(os.path.basename(path) + ": 내용이 없다 — 건너뜀")
            continue
        x0, y0, x1, y1 = box
        x0, y0 = max(0, x0 - MARGIN), max(0, y0 - MARGIN)
        x1, y1 = min(im.width, x1 + MARGIN), min(im.height, y1 + MARGIN)
        out = im.crop((x0, y0, x1, y1))
        print("{0:24s} {1} -> {2}  box={3}".format(
            os.path.basename(path), im.size, out.size, (x0, y0, x1, y1)))
        if write:
            out.save(path)
    if not write:
        print("\n--write 를 붙이면 실제로 자른다.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
