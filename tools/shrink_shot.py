# 스크린샷을 세션이 볼 수 있게 절반 크기 사본으로 만든다 (2026-09-29, A-25)
#
#   python tools/shrink_shot.py <원본> <사본>
import sys

from PIL import Image

src, dst = sys.argv[1], sys.argv[2]
im = Image.open(src).convert("RGB")
print(src, im.size)
im.resize((im.width // 2, im.height // 2), Image.LANCZOS).save(dst, quality=88)
print(dst, "저장")
