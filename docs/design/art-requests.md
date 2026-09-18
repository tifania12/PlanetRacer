# 이미지 요청 대기열 — GPT로 만들 것

Tifania가 ChatGPT Plus를 쓰고, **이미지 생성은 거기서 한다**(2026-09-15 결정).
밤 20~08시에 도는 이미지 세션이 PC의 ChatGPT 데스크탑 앱을 조작해 여기 쌓인 것을 뽑는다.
그러니 **이미지가 필요해지면 직접 만들려 하지 말고 여기에 프롬프트를 적어 둔다.**

받은 이미지는 `PlanetRacer/Assets/Resources/Art/` 아래로 들어간다 — `Assets/Art/`가 아니다.
`Resources`에 있어야 아트 확인 화면(`?art=1`)이 런타임에 긁어서 보여줄 수 있다.
배경을 빼야 하는 그림은 **처음부터 투명 PNG로 요청한다**(마젠타 키잉은 더 쓰지 않는다).
GPT는 프롬프트에 `real alpha channel` / `no background color`를 넣으면 진짜 RGBA PNG를 준다 — 2026-09-16 확인.
이미지 세션은 받은 파일이 정말 RGBA이고 네 모서리가 투명한지 검사한 뒤 넣는다.

## 적는 법

한 항목에 이만큼만 있으면 된다. 길게 쓰지 않는다.

    ### (파일명) 무엇에 쓰는 이미지인지
    - 크기: 512x512 (또는 필요한 비율)
    - 용도: 어디에 들어가는지 (예: 업그레이드 화면 도구 아이콘)
    - 프롬프트: (영문. GPT에 그대로 붙여 넣을 수 있게)
    - 참고: 색·톤에서 지켜야 할 것 (예: 쿼츠 행성 팔레트에 맞출 것)

만들어져서 프로젝트에 들어가면 `- [x]`로 바꾸고 파일 경로를 적는다.

## 톤 기준

행성 색은 `GemColors`가 정해 둔 값을 따른다(쿼츠는 흰빛, 루비는 붉은빛).

**톤이 두 갈래다. 2026-09-18에 Tifania가 컷신 7장을 보고 확정했다.**

- **작은 것 — 아이콘·펫·행성 구체.** UI 아이콘은 `BootstrapHudUgui.cs` 위쪽의 색 상수와
  어울리게(남색 바탕에 밝은 회백색 선). **사실적인 렌더링보다 단순한 형태가 낫다.**
  64x64에서도 실루엣이 읽혀야 하기 때문이다.
- **큰 것 — 컷신.** 전체 화면에 깔리는 자리라 **사실적으로 가도 된다.**
  기존 7장(오프닝·행성 도착 5·첫 우승)이 그 기준이다 —
  저폴리 결정 지형 + 실사에 가까운 차량·조명 + 하늘에 행성 실루엣.
  **컷신을 새로 뽑을 때는 그 7장과 나란히 놓고 튀지 않는지 본다.**

두 갈래가 섞여 있는 게 의도다. 작은 건 알아보는 게 목적이고, 큰 건 분위기가 목적이다.

## 대기 중

<!-- 여기에 추가 -->

## 들어온 것

<!-- 프로젝트에 반영된 것 -->

> **7등급(초월) 10종이다.** `docs/design/pet-gacha.md` 7절 "올리는 순서" 1번.
> 종 이름과 계열·맡는 축은 같은 문서 2절(10마리가 서로 다른 축을 하나씩 맡는다)과
> 7절(계열 넷)에서 갈라 놓은 것이다. 계열 문장은 고정하고 등급 문장만 7등급용으로 썼다.
> 들어가는 곳은 새 하위 폴더 `Resources/Art/Pets/`다.

### [x] Resources/Art/Pets/pet-t7-wheel-aurora.png — 초월 바퀴족: 오로라휠 (레이스 속도)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a large wheel with a glowing ring spinning around it, ornate plating, trailing light ribbons,
  rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(바퀴 몸통 + 허브의 큰 눈)은 바퀴족 넷이 똑같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wheel-aurora.png (커밋 deb2fe6)

### [x] Resources/Art/Pets/pet-t7-wheel-blaze.png — 초월 바퀴족: 블레이즈휠 (부스트)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a large wheel with a glowing ring spinning around it, ornate plating, twin exhaust vents flaring at the sides,
  rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep every warm tone strictly in the orange/vermilion range — no pink, no magenta, no purple tints anywhere, including highlights and edges.
- 참고: 투명 PNG로 받는다. 위 마지막 줄이 붉은 계열용 금지 줄이다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wheel-blaze.png (커밋 deb2fe6)

### [x] Resources/Art/Pets/pet-t7-wheel-ember.png — 초월 바퀴족: 엠버휠 (연료 회복)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a large wheel with a glowing ring spinning around it, ornate plating, a small fuel cell glowing inside the hub,
  rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wheel-ember.png (커밋 deb2fe6)

### [x] Resources/Art/Pets/pet-t7-wing-lantern.png — 초월 날개족: 랜턴윙 (상자 등급)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  ornate plated wings with a glowing ring behind it, carrying a small lantern that lights its face,
  rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(짧은 날개 + 떠 있는 자세)은 날개족 넷이 똑같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wing-lantern.png (커밋 0116d3f)

### [x] Resources/Art/Pets/pet-t7-wing-prism.png — 초월 날개족: 프리즘윙 (조각 획득)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  ornate plated wings with a glowing ring behind it, a faceted prism shard orbiting its body,
  rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wing-prism.png (커밋 0116d3f)

### [x] Resources/Art/Pets/pet-t7-wing-herald.png — 초월 날개족: 헤럴드윙 (광고 보상)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  ornate plated wings with a glowing ring behind it, a long ribbon banner streaming from its tail,
  rendered with #1238A8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-wing-herald.png (커밋 0116d3f)

### [x] Resources/Art/Pets/pet-t7-ore-geode.png — 초월 광석족: 지오드코어 (채굴 산출)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a tall ornate geode split open to show a glowing core, small shards orbiting it,
  rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(결정 덩어리에 눈)은 광석족 셋이 똑같이 쓴다
- 참고 추가: 첫 판이 마젠타 잔상 886픽셀로 걸렸다(결정이 보랏빛으로 나왔다). 프롬프트 끝에
  `Keep the crystal strictly pale white and cool grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere, including the inner glow, highlights and edges.`
  를 붙여 다시 뽑으니 통과했다. 흰빛 결정도 붉은 계열과 똑같이 이 줄이 필요하다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-ore-geode.png (커밋 caa8562)

### [x] Resources/Art/Pets/pet-t7-ore-crucible.png — 초월 광석족: 크루시블코어 (정제 속도)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a tall ornate crystal body with a molten refining chamber glowing in its chest,
  rendered with #BF0F29 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep every red strictly in the crimson/scarlet range — no pink, no magenta, no purple tints anywhere, including highlights and edges.
- 참고: 투명 PNG로 받는다. 붉은 계열이라 planet-ruby에서 쓴 분홍·마젠타 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-ore-crucible.png (커밋 268dbaa)

### [x] Resources/Art/Pets/pet-t7-carrier-vault.png — 초월 짐꾼족: 볼트캐리어 (화물칸)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  an ornate reinforced vault container on its back with a glowing seal, heavy plated legs,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 계열 문장(통통한 몸 + 등의 짐칸)은 짐꾼족 셋이 똑같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-carrier-vault.png (커밋 268dbaa)

### [x] Resources/Art/Pets/pet-t7-carrier-hearth.png — 초월 짐꾼족: 하스캐리어 (오프라인 상한)
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감·장착 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  an ornate domed shelter on its back with a warm glow inside, curled up as if resting,
  rendered with #1C298C as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t7-carrier-hearth.png (커밋 268dbaa)

### [x] Resources/Art/Cutscenes/opening.png — 오프닝 — 쿼츠 행성 착륙
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A small mining ship descending toward a pale crystalline planet surface at dawn, landing struts extended, dust kicked up below. Dominant color #E6E6F0.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/opening.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-ruby.png — 도착 — 루비
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A lone mining rig cresting a ridge above glowing lava-veined red crystal cliffs. Dominant color #BF0F29.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-ruby.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-sapphire.png — 도착 — 사파이어
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig on a vast frozen plain of blue crystal spires, frost drifting low across the ground. Dominant color #1238A8.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-sapphire.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-aquamarine.png — 도착 — 아쿠아마린
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig at the edge of a wide teal liquid lake reflecting rounded crystal reefs. Dominant color #59D9CC.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-aquamarine.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-cinnabar.png — 도착 — 주사
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig pushing through drifting orange toxic mist over rough vermilion terrain. Dominant color #D94D0F.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-cinnabar.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/arrive-lapis.png — 도착 — 라피스 라줄리
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A mining rig anchored to deep indigo rock while broken fragments drift upward in low gravity. Dominant color #1C298C.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/arrive-lapis.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Cutscenes/first-race-win.png — 첫 레이스 우승
- 크기: 16:9 (1600x900 이상)
- 용도: 컷신 일러스트
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Wide cinematic illustration, 16:9. A racing buggy crossing a finish marker on a crystal plain, dust trail behind, other racers distant. Dominant color #E6E6F0.
  No close-up faces, no text overlay. Atmospheric depth, strong silhouette of the
  foreground element against the sky.
- 참고: 배경 제거 안 함. 그대로 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Cutscenes/first-race-win.png
- 검사: `check_alpha.py --opaque` 통과 (1672x941, 마젠타 잔상 0픽셀)

### [x] Resources/Art/Icons/icon-key.png — 재화: 상자 열쇠
- 크기: 512x512 정사각
- 용도: 상자 개봉 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a small ornate key with a crystal bow, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-key.png (커밋 72daba5, 재생성 2032cbb)

### [x] Resources/Art/Icons/icon-blueprint.png — 재화: 청사진
- 크기: 512x512 정사각
- 용도: 제작 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a rolled technical blueprint scroll with faint grid lines, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-blueprint.png (커밋 72daba5)

### [x] Resources/Art/Icons/icon-fuel.png — 재화: 연료
- 크기: 512x512 정사각
- 용도: 레이스 출전 화면
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a sealed fuel canister with a glowing level window, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-fuel.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-box-rusty.png — 공구 상자: 녹슨
- 크기: 512x512 정사각
- 용도: 상자 개봉·레이스 보상
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a battered rusty metal toolbox, closed, worn edges, rendered with #8C6A4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-box-rusty.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-box-steel.png — 공구 상자: 강철
- 크기: 512x512 정사각
- 용도: 상자 개봉·레이스 보상
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a clean steel toolbox, closed, riveted panels, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-box-steel.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-box-titanium.png — 공구 상자: 티타늄
- 크기: 512x512 정사각
- 용도: 상자 개봉·레이스 보상
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a sleek titanium case, closed, subtle blue sheen, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-box-titanium.png (커밋 2032cbb)

### [x] Resources/Art/Icons/icon-grade-c.png — 등급 배지: C
- 크기: 512x512 정사각
- 용도: 부품 등급 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a simple shield badge with one notch, plain finish, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-c.png (커밋 a5d9ee4)

### [x] Resources/Art/Icons/icon-grade-b.png — 등급 배지: B
- 크기: 512x512 정사각
- 용도: 부품 등급 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a shield badge with two notches, polished finish, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-b.png (커밋 a5d9ee4)

### [x] Resources/Art/Icons/icon-grade-a.png — 등급 배지: A
- 크기: 512x512 정사각
- 용도: 부품 등급 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a shield badge with three notches and a small gem inset, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-a.png (커밋 a5d9ee4)

### [x] Resources/Art/Icons/icon-grade-s.png — 등급 배지: S
- 크기: 512x512 정사각
- 용도: 부품 등급 표시. 네 개 중 가장 화려하게
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of an ornate shield badge with a radiant gem centerpiece, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-grade-s.png (커밋 a5d9ee4)

### [x] Resources/Art/Planets/planet-quartz.png — 쿼츠 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 쿼츠 crystal formations,
  dominant color #E6E6F0, smooth pale surface with scattered clear prismatic shards,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-quartz.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-sapphire.png — 사파이어 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 사파이어 crystal formations,
  dominant color #1238A8, frozen blue crystal plains with frost haze,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-sapphire.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-aquamarine.png — 아쿠아마린 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 아쿠아마린 crystal formations,
  dominant color #59D9CC, shallow teal liquid pools between rounded crystal reefs,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-aquamarine.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-cinnabar.png — 주사 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 주사 crystal formations,
  dominant color #D94D0F, rough vermilion terrain with drifting toxic orange mist,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-cinnabar.png (커밋 7fddc92)

### [x] Resources/Art/Planets/planet-lapis.png — 라피스 라줄리 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 라피스 라줄리 crystal formations,
  dominant color #1C298C, deep indigo rock with fragments floating off the surface in low gravity,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-lapis.png (커밋 7fddc92)

### [x] Resources/Art/Rigs/rig-tiers-sheet.png — 채굴차 3티어 (한 장에 세 대)
- 크기: 1536x1024 가로
- 용도: 채굴차 티어 외형. 받은 뒤 셋으로 잘라 쓴다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  Three versions of the same small six-wheeled mining rover shown side by side in a row,
  left to right: (1) basic, with a simple pickaxe arm, worn grey panels;
  (2) upgraded, with a rotary drill arm and reinforced plating;
  (3) advanced, with a sleek laser cutter arm and glowing #708CFF accents.
  Same scale, same angle, same lighting for all three. Fully transparent background — real alpha channel, no background color, no checkerboard.
- 참고: 한 장에 세 대를 그려야 통일감이 산다. 따로 뽑으면 셋이 다른 차가 된다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Rigs/rig-tiers-sheet.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-part-engine.png — 부품: 엔진
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a compact futuristic vehicle engine block with glowing intake vents, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-engine.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-part-tire.png — 부품: 타이어
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a chunky off-road vehicle tire seen at a three-quarter angle, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-tire.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-part-suspension.png — 부품: 서스펜션
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a coil-over suspension strut with a spring, rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-part-suspension.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-gear-tool.png — 채굴 장비: 도구
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a mining drill bit with a faceted crystal tip, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-gear-tool.png (커밋 0badb06)

### [x] Resources/Art/Icons/icon-gear-cargo.png — 채굴 장비: 화물칸
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비 + 화물칸 게이지 옆
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of an open cargo container half filled with rough crystal ore, rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-gear-cargo.png (커밋 94625d6)

### [x] Resources/Art/Icons/icon-gear-engine.png — 채굴 장비: 엔진
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a rugged tracked-vehicle drive unit with a single piston, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-gear-engine.png (커밋 94625d6)

### [x] Resources/Art/Icons/icon-raw-mineral.png — 재화: 원석
- 크기: 512x512 정사각
- 용도: HUD 상단 원석 숫자 옆, 모든 보상 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a cluster of three rough uncut crystal shards, rendered with #E6E6F0 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-raw-mineral.png (커밋 94625d6)

### [x] Resources/Art/Icons/icon-refined-mineral.png — 재화: 정제 광물
- 크기: 512x512 정사각
- 용도: 제련소·상점. 원석과 한눈에 구분되어야 한다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a single polished faceted gem cut into a clean hexagon, rendered with #708CFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 RGBA·모서리 투명을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Icons/icon-refined-mineral.png (커밋 94625d6)

### [x] Resources/Art/Planets/planet-ruby.png — 루비 행성 구체
- 크기: 1024x1024 정사각
- 용도: 행성 선택·워프 화면, 타이틀
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single spherical planet floating centered, surface covered in 루비 crystal formations,
  dominant color #BF0F29, cracked lava veins glowing faintly between jagged red crystals,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no stars, no space background, no atmosphere glow.
- 참고: 투명 PNG로 받아 UI에 얹어 쓴다
- 참고 추가: 첫 판이 마젠타 잔상 59픽셀로 검사에 걸렸다. 프롬프트 끝에
  `Keep every red strictly in the crimson/scarlet range — no pink, no magenta, no purple tints anywhere, including highlights and edges.`
  를 붙여 다시 뽑으니 통과했다. 붉은 계열 행성을 다시 뽑을 땐 이 줄을 같이 쓴다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Planets/planet-ruby.png (커밋 246dacc)

### 다시 뽑아 달라고 하는 법

마음에 안 드는 그림이 있으면, 여기 그 항목 아래에 이렇게 한 줄만 붙이면 된다.

    - [!] 다시: 색이 너무 어둡다. 더 밝게, 그리고 드릴 날을 더 크게

다음 이미지 세션이 **대기 중인 새 요청보다 이걸 먼저** 처리한다.
원래 프롬프트 뒤에 그 지시를 덧붙여 새로 뽑고 파일을 덮어쓴 뒤, `- [!]` 줄을 지운다.
여러 번 반복해도 된다. 마음에 들 때까지 붙이면 된다.
