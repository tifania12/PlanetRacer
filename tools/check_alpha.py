#!/usr/bin/env python3
"""받은 그림이 정말 투명 PNG인지 검사한다.

GPT에 `real alpha channel` / `no background color` 로 요청하면 진짜 RGBA PNG가 온다
(2026-09-16 확인). 그런데 가끔 알파 없이 흰 배경이나 체크무늬를 "그려서" 주기도 해서,
Resources/Art 에 넣기 전에 이 검사를 통과해야 한다.

통과 조건 세 가지
  1. 모드가 RGBA (또는 LA)      — 알파 채널이 실제로 있다
  2. 네 모서리가 모두 알파 0    — 배경이 비어 있다
  3. 투명 영역이 10~95% 사이     — 다 비었거나(빈 파일) 거의 안 비었으면(배경이 붙음) 실패
  4. 마젠타 잔상이 없다          — 옛 키잉 방식으로 만든 파일은 테두리에 #FF00FF가 남는다

종료 코드 0 = 통과, 1 = 실패. 이미지 세션이 이 코드를 보고 넣을지 정한다.
"""
import sys
from PIL import Image

MIN_RATIO, MAX_RATIO = 0.10, 0.95


def check(path):
    im = Image.open(path)
    notes = [f"{path}", f"  모드 {im.mode} · {im.width}x{im.height}"]

    if im.mode not in ("RGBA", "LA", "PA"):
        notes.append(f"  [실패] 알파 채널이 없다 (모드 {im.mode})")
        return False, notes

    im = im.convert("RGBA")
    a = im.getchannel("A")
    w, h = im.size

    # 한 점만 보면 안티에일리어싱 한 픽셀에 흔들린다. 모서리마다 8x8 조각의 평균을 본다
    k = max(4, min(w, h) // 64)
    boxes = [(0, 0, k, k), (w - k, 0, w, k), (0, h - k, k, h), (w - k, h - k, w, h)]
    vals = [round(sum(a.crop(b).tobytes()) / (k * k)) for b in boxes]
    notes.append(f"  모서리 알파 {vals} (각 {k}x{k} 평균)")

    px = a.tobytes()
    ratio = sum(1 for v in px if v < 16) / len(px)
    notes.append(f"  투명 영역 {ratio * 100:.0f}%")

    ok = True
    if max(vals) > 16:
        notes.append("  [실패] 모서리가 불투명하다 — 배경이 그려져 있다")
        ok = False
    if ratio < MIN_RATIO:
        notes.append(f"  [실패] 투명 영역이 {MIN_RATIO * 100:.0f}% 미만 — 배경이 붙어 왔다")
        ok = False
    if ratio > MAX_RATIO:
        notes.append(f"  [실패] 투명 영역이 {MAX_RATIO * 100:.0f}% 초과 — 그림이 거의 비었다")
        ok = False
    # 옛 마젠타 키잉으로 만든 파일은 테두리에 걸러지지 않은 #FF00FF가 남는다.
    # 게임에 넣으면 아이콘 가장자리가 분홍빛으로 보인다
    fringe = sum(
        1 for r, g, b, al in im.getdata()
        if al > 32 and r > 150 and b > 150 and g < 100
    )
    if fringe > 50:
        notes.append(f"  [실패] 마젠타 잔상 {fringe}픽셀 — 키잉으로 만든 파일이다. 다시 뽑는다")
        ok = False

    if ok:
        notes.append("  [통과]")
    return ok, notes


def main():
    if len(sys.argv) < 2:
        print("쓰는 법: python tools/check_alpha.py <파일> [파일...]")
        return 2
    bad = 0
    for p in sys.argv[1:]:
        try:
            ok, notes = check(p)
        except Exception as e:
            ok, notes = False, [p, f"  [실패] 열 수 없다: {e}"]
        print("\n".join(notes))
        if not ok:
            bad += 1
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
