#!/usr/bin/env python3
"""받은 그림이 쓸 만한 상태인지 검사한다.

두 가지 모드가 있다.

  투명 모드(기본) — 아이콘·행성처럼 **배경을 빼야 하는** 그림용.
  불투명 모드(`--opaque`) — 컷신처럼 **배경이 그림의 일부인** 것용.
      2026-09-17에 넣었다(T-11). 그 전에는 투명 검사밖에 없어서 컷신이 설계상
      통과할 수 없었고, 이미지 세션이 네 번 연속 같은 자리에서 멈췄다.
      `art-requests.md`의 참고가 "배경 제거 안 함"이면 이 모드로 검사한다.

쓰는 법
    python tools/check_alpha.py <파일>...                    # 투명 모드
    python tools/check_alpha.py --opaque <파일>...           # 불투명 모드
    python tools/check_alpha.py --opaque --min-width 1600 <파일>...

GPT에 `real alpha channel` / `no background color` 로 요청하면 진짜 RGBA PNG가 온다
(2026-09-16 확인). 그런데 가끔 알파 없이 흰 배경이나 체크무늬를 "그려서" 주기도 해서,
Resources/Art 에 넣기 전에 이 검사를 통과해야 한다.

투명 모드 통과 조건
  1. 모드가 RGBA (또는 LA)      — 알파 채널이 실제로 있다
  2. 네 모서리가 모두 알파 0    — 배경이 비어 있다
  3. 투명 영역이 10~95% 사이     — 다 비었거나(빈 파일) 거의 안 비었으면(배경이 붙음) 실패
  4. 마젠타 잔상이 없다          — 옛 키잉 방식으로 만든 파일은 테두리에 #FF00FF가 남는다

불투명 모드 통과 조건 (배경이 있어야 정상이므로 알파 관련 셋을 건너뛴다)
  1. 파일이 열린다              — 받다 만 파일·깨진 파일을 잡는다
  2. 폭이 --min-width 이상      — 기본 1600px. 요청보다 작게 오는 일이 잦다
  3. 마젠타 잔상이 없다          — 투명 모드와 같은 검사
  4. 단색이 아니다              — 전부 같은 색이면 생성이 실패한 것이다

종료 코드 0 = 통과, 1 = 실패. 이미지 세션이 이 코드를 보고 넣을지 정한다.
"""
import sys
from PIL import Image

# 한글 출력이 cp949 콘솔에서 죽지 않게 한다.
# 2026-09-18 이미지 세션에서 [실패] 줄의 em dash(—)가 UnicodeEncodeError를 내면서
# 검사 결과가 한 줄도 안 찍히고 종료 코드 1만 남았다. 실패 이유를 못 읽으면
# 세션이 daily에 무엇이 왜 실패했는지 적을 수가 없다.
for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8", errors="replace")
    except (AttributeError, ValueError):
        pass

MIN_RATIO, MAX_RATIO = 0.10, 0.95
DEFAULT_MIN_WIDTH = 1600


def magenta_fringe(im):
    """옛 마젠타 키잉으로 만든 파일은 테두리에 걸러지지 않은 #FF00FF가 남는다.
    게임에 넣으면 가장자리가 분홍빛으로 보인다. 두 모드가 같이 쓴다."""
    return sum(
        1 for r, g, b, al in im.convert("RGBA").getdata()
        if al > 32 and r > 150 and b > 150 and g < 100
    )


def check_opaque(path, min_width):
    """배경이 그림의 일부인 것(컷신 등)을 검사한다. 알파는 보지 않는다."""
    im = Image.open(path)
    notes = [f"{path}", f"  모드 {im.mode} · {im.width}x{im.height} (불투명 모드)"]
    ok = True

    if im.width < min_width:
        notes.append(f"  [실패] 폭이 {im.width}px — {min_width}px 미만이다. 다시 뽑는다")
        ok = False

    rgb = im.convert("RGB")
    colors = rgb.getcolors(maxcolors=256)
    if colors is not None and len(colors) <= 2:
        notes.append(f"  [실패] 사실상 단색이다(색 {len(colors)}종) — 생성이 실패한 파일이다")
        ok = False

    fringe = magenta_fringe(im)
    if fringe > 50:
        notes.append(f"  [실패] 마젠타 잔상 {fringe}픽셀 — 키잉으로 만든 파일이다. 다시 뽑는다")
        ok = False
    else:
        notes.append(f"  마젠타 잔상 {fringe}픽셀")

    if ok:
        notes.append("  [통과]")
    return ok, notes


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
    fringe = magenta_fringe(im)
    if fringe > 50:
        notes.append(f"  [실패] 마젠타 잔상 {fringe}픽셀 — 키잉으로 만든 파일이다. 다시 뽑는다")
        ok = False

    if ok:
        notes.append("  [통과]")
    return ok, notes


def main():
    args = sys.argv[1:]
    opaque = False
    min_width = DEFAULT_MIN_WIDTH
    paths = []
    i = 0
    while i < len(args):
        a = args[i]
        if a == "--opaque":
            opaque = True
        elif a == "--min-width":
            i += 1
            min_width = int(args[i])
        else:
            paths.append(a)
        i += 1

    if not paths:
        print("쓰는 법: python tools/check_alpha.py [--opaque] [--min-width N] <파일> [파일...]")
        return 2

    bad = 0
    for p in paths:
        try:
            ok, notes = check_opaque(p, min_width) if opaque else check(p)
        except Exception as e:
            ok, notes = False, [p, f"  [실패] 열 수 없다: {e}"]
        print("\n".join(notes))
        if not ok:
            bad += 1
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
