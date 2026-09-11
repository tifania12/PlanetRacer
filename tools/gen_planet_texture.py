#!/usr/bin/env python3
"""
보석 행성 지면 타일 텍스처 생성기.

AI 이미지 생성 대신 코드로 만든다. 이유:
  - 완벽하게 이어진다. 감싸도는 거리(toroidal distance)로 계산하므로 이음새가 원천적으로 없다.
    AI 생성물은 "seamless"라고 시켜도 이음새가 남아서 후처리를 해야 했다.
  - 공짜다. 행성을 몇 개로 늘리든 비용이 0이다.
  - 행성별 색과 밀도를 숫자로 조절한다. 여섯 행성이 같은 재질감으로 통일된다.
  - 워터마크 문제가 없다.

만드는 것: 갈라진 광물 표면(보로노이 세포) + 흩뿌려진 결정 조각.

쓰는 법:
    python3 gen_planet_texture.py ruby out.png
    python3 gen_planet_texture.py --all outdir/
"""

import sys
import os
import numpy as np
from PIL import Image, ImageDraw


# 행성별 팔레트와 밀도. (기본색, 어두운색, 밝은색, 결정색)
# 색은 sRGB 0~255. 결정 개수는 1024px 타일 기준.
PLANETS = {
    "quartz": dict(
        base=(206, 208, 214), dark=(158, 162, 172), light=(233, 235, 240),
        crystal=(244, 248, 252), crystal_count=150, crack=0.55, cells=170),
    "ruby": dict(
        base=(126, 28, 44), dark=(78, 16, 30), light=(168, 44, 56),
        crystal=(236, 44, 72), crystal_count=200, crack=0.70, cells=190),
    "sapphire": dict(
        base=(34, 58, 122), dark=(20, 34, 78), light=(58, 92, 166),
        crystal=(74, 150, 236), crystal_count=190, crack=0.65, cells=180),
    "aquamarine": dict(
        base=(96, 168, 166), dark=(58, 118, 122), light=(140, 206, 200),
        crystal=(168, 240, 232), crystal_count=170, crack=0.50, cells=160),
    "cinnabar": dict(
        base=(150, 62, 40), dark=(96, 38, 26), light=(196, 96, 56),
        crystal=(240, 128, 62), crystal_count=180, crack=0.75, cells=200),
    "lapis": dict(
        base=(32, 44, 104), dark=(18, 24, 66), light=(54, 72, 148),
        crystal=(226, 196, 96), crystal_count=140, crack=0.60, cells=175),  # 라피스는 금빛 점
}


def _rng(seed):
    return np.random.default_rng(seed)


def toroidal_voronoi(n, num_cells, seed):
    """감싸도는 보로노이. F1(가장 가까운 거리), F2(두 번째), 세포 번호를 돌려준다.

    감싸도는 거리를 쓰기 때문에 결과가 그대로 타일링된다. 후처리가 필요 없다."""
    rng = _rng(seed)
    pts = rng.random((num_cells, 2)) * n

    yy, xx = np.meshgrid(np.arange(n, dtype=np.float32),
                         np.arange(n, dtype=np.float32), indexing="ij")

    f1 = np.full((n, n), np.inf, dtype=np.float32)
    f2 = np.full((n, n), np.inf, dtype=np.float32)
    cid = np.zeros((n, n), dtype=np.int32)

    for i, (py, px) in enumerate(pts):
        dx = np.abs(xx - px)
        dx = np.minimum(dx, n - dx)          # 좌우로 감싸돎
        dy = np.abs(yy - py)
        dy = np.minimum(dy, n - dy)          # 상하로 감싸돎
        d = np.sqrt(dx * dx + dy * dy)

        closer = d < f1
        f2 = np.where(closer, f1, np.minimum(f2, d))
        cid = np.where(closer, i, cid)
        f1 = np.where(closer, d, f1)

    return f1, f2, cid


def tileable_noise(n, octaves, seed):
    """감싸도는 값 노이즈(value noise). 격자 번호를 주파수로 나눈 나머지로 참조하므로
    경계에서 반드시 이어진다.

    처음에는 작은 난수 이미지를 확대해서 쓰려고 했는데, 확대 보간이 경계에서 안 맞아
    이음새가 30배 넘게 벌어졌다. 격자 참조를 직접 감싸도는 방식으로 바꿔서 해결했다."""
    rng = _rng(seed)
    out = np.zeros((n, n), dtype=np.float32)
    amp_total = 0.0

    for o in range(octaves):
        freq = 2 ** (o + 2)          # 4, 8, 16, ... 정수 주파수
        amp = 0.5 ** o
        grid = rng.random((freq, freq)).astype(np.float32)

        t = np.arange(n, dtype=np.float32) * freq / n
        i0 = np.floor(t).astype(np.int64) % freq
        i1 = (i0 + 1) % freq          # 마지막 칸은 0번으로 돌아간다 = 이어짐
        f = t - np.floor(t)
        f = f * f * (3 - 2 * f)       # smoothstep

        fy = f[:, None]
        fx = f[None, :]
        g00 = grid[np.ix_(i0, i0)]
        g01 = grid[np.ix_(i0, i1)]
        g10 = grid[np.ix_(i1, i0)]
        g11 = grid[np.ix_(i1, i1)]

        top = g00 * (1 - fx) + g01 * fx
        bot = g10 * (1 - fx) + g11 * fx
        out += (top * (1 - fy) + bot * fy) * amp
        amp_total += amp

    return out / amp_total


def draw_crystals(n, count, color, seed):
    """각진 결정 조각을 흩뿌린다. 경계를 넘는 것은 반대편에도 그려서 이어지게 한다."""
    layer = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    rng = _rng(seed)

    for _ in range(count):
        cx, cy = rng.random(2) * n
        r = rng.uniform(n * 0.004, n * 0.016)
        sides = rng.integers(3, 6)
        rot = rng.random() * np.pi * 2
        # 불규칙한 다각형 하나
        pts = []
        for k in range(sides):
            a = rot + (k / sides) * np.pi * 2
            rr = r * rng.uniform(0.6, 1.4)
            pts.append((np.cos(a) * rr, np.sin(a) * rr))

        bright = rng.uniform(0.75, 1.0)
        col = tuple(int(min(255, c * bright)) for c in color) + (255,)
        edge = tuple(int(c * 0.55) for c in color) + (255,)

        # 3x3로 같은 모양을 그려 경계를 넘는 조각이 반대편에 이어지게 한다
        for ox in (-n, 0, n):
            for oy in (-n, 0, n):
                poly = [(cx + px + ox, cy + py + oy) for px, py in pts]
                if all(p[0] < -r * 2 or p[0] > n + r * 2 or
                       p[1] < -r * 2 or p[1] > n + r * 2 for p in poly):
                    continue
                d.polygon(poly, fill=col, outline=edge)
    return np.asarray(layer, dtype=np.float32) / 255.0


def generate(planet_id, n=1024, seed=None):
    if planet_id not in PLANETS:
        raise SystemExit(f"모르는 행성: {planet_id}. 가능한 값: {', '.join(PLANETS)}")
    cfg = PLANETS[planet_id]
    if seed is None:
        seed = abs(hash(planet_id)) % 100000

    base = np.array(cfg["base"], dtype=np.float32)
    dark = np.array(cfg["dark"], dtype=np.float32)
    light = np.array(cfg["light"], dtype=np.float32)

    f1, f2, cid = toroidal_voronoi(n, cfg["cells"], seed)

    # 세포마다 밝기를 조금씩 다르게. 광물 덩어리가 제각각으로 보인다.
    rng = _rng(seed + 1)
    cell_tone = rng.random(cfg["cells"]).astype(np.float32)[cid]          # 0~1
    tone = (cell_tone - 0.5) * 0.9

    # 세포 경계 = 균열. F2-F1이 작을수록 경계에 가깝다.
    border = np.clip((f2 - f1) / (n * 0.012), 0, 1)
    crack = (1.0 - border) ** 2 * cfg["crack"]

    # 미세한 얼룩
    grain = tileable_noise(n, 5, seed + 2) - 0.5

    img = np.empty((n, n, 3), dtype=np.float32)
    for c in range(3):
        v = base[c]
        v = v + tone * (light[c] - dark[c]) * 0.5     # 세포별 색조
        v = v + grain * 26.0                          # 얼룩
        v = v * (1.0 - crack * 0.62)                  # 균열은 어둡게
        img[:, :, c] = v

    # 결정 조각을 위에 올린다
    cr = draw_crystals(n, cfg["crystal_count"], cfg["crystal"], seed + 3)
    a = cr[:, :, 3:4]
    img = img * (1 - a) + cr[:, :, :3] * 255.0 * a

    return np.clip(img, 0, 255).astype(np.uint8)


def wrap_quality(a):
    """감싸도는 경계 차이 / 내부 인접 평균 차이. 1.0 근처면 이음새 없음."""
    x = a.astype(np.float64)
    v = np.abs(x[0] - x[-1]).mean() / np.abs(x[1:] - x[:-1]).mean()
    h = np.abs(x[:, 0] - x[:, -1]).mean() / np.abs(x[:, 1:] - x[:, :-1]).mean()
    return v, h


def main():
    args = sys.argv[1:]
    if not args:
        raise SystemExit(__doc__)

    if args[0] == "--all":
        outdir = args[1] if len(args) > 1 else "."
        os.makedirs(outdir, exist_ok=True)
        for pid in PLANETS:
            a = generate(pid)
            p = os.path.join(outdir, f"planet_{pid}.png")
            Image.fromarray(a).save(p)
            v, h = wrap_quality(a)
            print(f"{pid:12s} -> {p}  이음새 상하 {v:.2f} 좌우 {h:.2f}")
    else:
        pid = args[0]
        out = args[1] if len(args) > 1 else f"planet_{pid}.png"
        a = generate(pid)
        Image.fromarray(a).save(out)
        v, h = wrap_quality(a)
        print(f"{pid} -> {out}  이음새 상하 {v:.2f} 좌우 {h:.2f}")


if __name__ == "__main__":
    main()
