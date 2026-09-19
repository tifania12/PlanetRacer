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

**펫 아트 76장 (2026-09-19에 채움).** `docs/design/pet-gacha.md` 7절의 순서 그대로다.
위에서부터 뽑으면 된다 — 7등급 10장이 먼저 나오게 해 뒀다(뽑기 화면에 제일 크게 나온다).

**초월 10종 중 5장이 마젠타 잔상으로 막혀 있다 (2026-09-19 밤 세션).**
`drill-sovereign`(3946px) · `comet-racer`(136px) · `burst-phoenix`(60px) ·
`shard-weaver`(547px) · `ember-heart`(269px). 파일은 받아서 자리에 그대로 두었지만
`check_alpha.py`가 실패시켜서 커밋하지 않았다(`drill-sovereign`은 다른 세션의 `git add -A`에
휩쓸려 들어갔다가 e0120fa에서 추적을 뺐다).

**키잉 때문이 아니다.** 프롬프트의 `iridescent prismatic material`이 분홍·보라 픽셀을 만드는데,
`check_alpha.py`의 마젠타 검사가 그걸 `r>150 and b>150 and g<100`로 잡는다. 같은 프롬프트의
나머지 5장은 우연히 50px 문턱을 넘지 않아 통과했다 — 그림의 좋고 나쁨과는 무관하다.
**Tifania가 정할 일:** (1) 초월 프롬프트에서 `iridescent prismatic`을 덜 분홍인 말로 바꾸거나,
(2) 마젠타 검사를 순수 #FF00FF에 가깝게 좁히거나(지금은 보라 전체를 잡는다),
(3) 이 5장은 눈으로 보고 통과시키거나. 정해지기 전에는 밤 세션이 다시 뽑아도 같은 자리에서 막힌다.

**같은 일이 신화 날개족에서도 났다 (2026-09-20 새벽 세션).** `wing-03`(마젠타 123px) ·
`wing-04`(89px)가 같은 검사에서 막혔다. 초월이 아닌데도 막힌 이유는 날개족 프롬프트의
`rich jewel tones`가 보라·남색 픽셀을 만들고, 그걸 마젠타 검사(`r>150 and b>150 and g<100`)가
잡기 때문이다. 같은 프롬프트로 뽑은 `wing-02` · `wing-05~08` 다섯 장은 통과했다 —
**그림의 좋고 나쁨이 아니라 우연히 50px 문턱을 넘었는지의 문제다.** 두 파일은 자리에 그대로
두었고 커밋하지 않았다.

**광석족(ore)은 거의 전멸했다 (2026-09-20 새벽 세션).** 일곱 장 중 통과는 `ore-01` 하나뿐이다.
`ore-02`(4209px) · `ore-03`(2203px) · `ore-04`(574px) · `ore-05`(178px) · `ore-07`(1742px)이
마젠타 검사에 걸렸고, `ore-06`은 ChatGPT 앱이 "미리 보기" 상태로 멈춰 아예 내려받지 못했다.
광석족 프롬프트는 `a faceted crystal cluster` + `rich jewel tones`라서 모델이 거의 항상
보라·자수정 결정을 그린다 — 날개족처럼 운에 맡길 문제가 아니라 **이 계열은 구조적으로 막힌다.**
반면 **짐꾼족(haul)은 일곱 장 전부 통과했다**(따뜻한 갈색·남색 위주라 마젠타 검사에 안 걸린다).

**한줄로: 위 (2)번으로 정하면 한꺼번에 풀린다.** 마젠타 검사를 순수 #FF00FF 쪽으로 좁히면
초월 5장 · 날개 2장 · 광석 5장, 모두 열두 장이 같이 풀린다. 정해지기 전까지는 다시 뽑아도 같은 자리에서 막힌다.


**신화 30종은 계열별로 프롬프트가 똑같다.** 바퀴 8장이 같은 문장 하나, 날개 8장이 같은 문장 하나다.
종 이름(톱니 순례자·궤도 방랑자…)이 프롬프트에 들어가 있지 않아서, 어느 파일이 어느 종이 되는지는
뽑는 순서가 정할 뿐이다. 그림 자체는 매번 다르게 나오니 못 쓸 것은 아니지만,
이름과 그림을 맞추려면 종마다 한 줄씩 더 적어야 한다.

1~4등급의 **색 변종 48장은 여기 없다.** 그림을 다시 뽑지 않고 아래 "골격" 항목을
`tools/recolor_pet.py`로 색만 바꿔 만든다(backlog P-18). 골격만 뽑으면 된다.

### Resources/Art/Pets/7-transcend/drill-sovereign.png — 초월 1/10 — 굴착의 군주 (채굴 산출)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a colossal crowned wheel ringed with rotating drill bits, molten gold light in the gaps, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/7-transcend/comet-racer.png — 초월 4/10 — 혜성 질주자 (레이스 속도)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a streamlined twin-wheeled creature trailing a comet tail of white fire, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/7-transcend/burst-phoenix.png — 초월 5/10 — 폭발의 불새 (부스트)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a bird-like creature with thruster wings, exhaust blooming into feathers of blue flame, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/7-transcend/shard-weaver.png — 초월 9/10 — 조각의 직조자 (조각 획득)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a many-armed crystal weaver spinning floating shards into a lattice, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/7-transcend/ember-heart.png — 초월 10/10 — 불씨의 심장 (연료 회복)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a crystal creature with an open chest cavity holding a burning ember core, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/wing-03.png — 신화 — 안개 사냥꾼 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/wing-04.png — 신화 — 쌍익 도굴꾼 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-02.png — 신화 — 정맥 탐색자 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-03.png — 신화 — 용암 조각가 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-04.png — 신화 — 서릿결 현자 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-05.png — 신화 — 원석 수도사 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-06.png — 신화 — 심층 광부 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/6-myth/ore-07.png — 신화 — 공명하는 정동 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/ore-aquamarine.png — 전설 — 광석족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/ore-cinnabar.png — 전설 — 광석족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/haul-quartz.png — 전설 — 짐꾼족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/haul-ruby.png — 전설 — 짐꾼족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/haul-sapphire.png — 전설 — 짐꾼족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/haul-aquamarine.png — 전설 — 짐꾼족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/5-legend/haul-cinnabar.png — 전설 — 짐꾼족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/2-base/wheel.png — 고급 골격 — 바퀴족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/2-base/wing.png — 고급 골격 — 날개족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/2-base/ore.png — 고급 골격 — 광석족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/2-base/haul.png — 고급 골격 — 짐꾼족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 고급 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, a thin metal trim and one small glowing dot, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/3-base/wheel.png — 희귀 골격 — 바퀴족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/3-base/wing.png — 희귀 골격 — 날개족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/3-base/ore.png — 희귀 골격 — 광석족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/3-base/haul.png — 희귀 골격 — 짐꾼족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 희귀 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, layered plating with two glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/4-base/wheel.png — 영웅 골격 — 바퀴족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/4-base/wing.png — 영웅 골격 — 날개족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/4-base/ore.png — 영웅 골격 — 광석족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

### Resources/Art/Pets/4-base/haul.png — 영웅 골격 — 짐꾼족 (쿼츠 색 원본)
- 크기: 512x512 정사각
- 용도: 영웅 등급 골격. 이 한 장을 `tools/recolor_pet.py`로 색만 바꿔 4색 변종을 만든다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, ornate plating, a floating ring, several glowing accents, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다

## 들어온 것

<!-- 프로젝트에 반영된 것 -->

### [x] Resources/Art/Pets/6-myth/wheel-04.png — 신화 — 폭주 기수 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-04.png (커밋 fc22cab)

### [x] Resources/Art/Pets/6-myth/wheel-05.png — 신화 — 황혼 바퀴 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-05.png (커밋 fc22cab)

### [x] Resources/Art/Pets/6-myth/wheel-06.png — 신화 — 이중륜 술사 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-06.png (커밋 fc22cab)

### [x] Resources/Art/Pets/6-myth/wheel-07.png — 신화 — 먼지 폭군 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-07.png (커밋 e2f6b4f)

### [x] Resources/Art/Pets/6-myth/wheel-08.png — 신화 — 광륜 기사 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-08.png (커밋 e2f6b4f)

### [x] Resources/Art/Pets/6-myth/wing-01.png — 신화 — 성층권 파수꾼 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-01.png (커밋 47583b2)

### [x] Resources/Art/Pets/7-transcend/refinery-sage.png — 초월 2/10 — 제련의 현자 (정제 속도)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a tall crystal sage with a furnace glowing inside its chest, rings of molten light orbiting it, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/refinery-sage.png (커밋 408a59c)

### [x] Resources/Art/Pets/7-transcend/vault-titan.png — 초월 3/10 — 화물의 거인 (화물칸)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a broad armored beast carrying a vast glowing container that folds open like petals, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/vault-titan.png (커밋 def81a2)

### [x] Resources/Art/Pets/7-transcend/fortune-key.png — 초월 6/10 — 행운의 열쇠 (상자 등급)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a floating creature shaped like an ornate key with wings, keyholes glowing across its body, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/fortune-key.png (커밋 46c0099)

### [x] Resources/Art/Pets/7-transcend/beacon-herald.png — 초월 7/10 — 신호의 전령 (광고 보상)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a lantern-bodied herald with a broadcasting horn, concentric light rings pulsing outward, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/beacon-herald.png (커밋 46c0099)

### [x] Resources/Art/Pets/7-transcend/dream-keeper.png — 초월 8/10 — 잠의 수호자 (오프라인 상한)
- 크기: 1024x1024 정사각 (최고 등급이라 크게)
- 용도: 펫 뽑기 — 최고 등급. 뽑기 화면에 제일 크게 나온다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  a sleepy moon-faced guardian curled around a glowing hourglass, made of iridescent prismatic material, majestic and imposing,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/7-transcend/dream-keeper.png (커밋 46c0099)

### [x] Resources/Art/Pets/1-common/wheel-quartz.png — 일반 — 바퀴족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/wheel-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/1-common/wing-quartz.png — 일반 — 날개족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/wing-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/1-common/ore-quartz.png — 일반 — 광석족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/ore-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/1-common/haul-quartz.png — 일반 — 짐꾼족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 무료 뽑기에서 가장 자주 나온다. 계열의 기본형
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, plain untextured surface, no decoration, very simple, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/1-common/haul-quartz.png (커밋 54f6972)

### [x] Resources/Art/Pets/6-myth/wheel-01.png — 신화 — 톱니 순례자 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-01.png (커밋 4e8caa8)

### [x] Resources/Art/Pets/6-myth/wheel-02.png — 신화 — 궤도 방랑자 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-02.png (커밋 4e8caa8)

> **2·3·4등급 골격 12장이다.** `pet-gacha.md` 7절 "1~4등급 52종"의 나머지 —
> 계열 4 × 등급 4 = 16장 중 1등급 4장은 들어왔고 이 12장이 남은 것이다.
> 1등급과 같은 이유로 **행성 색을 안 쓰고 중립 회청색(#8C94B8)**으로 받는다.
> 계열 문장은 초월 10종·1등급 4장과 똑같고, 등급 문장만 갈아 끼웠다 —
> 7절 예시(1등급 "plain rubber tread" / 4등급 "twin wheels with a metal rim" /
> 7등급 "glowing ring, ornate plating") 사이를 메우는 방향이다.
>
> **올리는 순서 3·4번(6등급 30종·5등급 18종)을 건너뛰고 5번을 먼저 집었다.**
> 3·4번은 종 이름과 고유 효과 48개를 새로 정해야 하는데, 그건 밤 세션이 혼자
> 정할 일이 아니라 Tifania에게 물을 일이다(초월 10종은 2절이 "서로 다른 축 10개"를
> 이미 고정해 둬서 나눠 붙이기만 하면 됐다). 골격 12장은 7절이 이미 정해 둔 것이라 바로 뽑을 수 있다.

### [x] Resources/Art/Pets/pet-t3-carrier.png — 희귀 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a wooden crate strapped to its back, no plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-carrier.png (커밋 33f5471)

### [x] Resources/Art/Pets/pet-t4-carrier.png — 영웅 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a reinforced metal crate with a latch on its back, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-carrier.png (커밋 33f5471)

### [x] Resources/Art/Pets/pet-t2-wheel.png — 고급 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a slightly larger wheel with a studded tread and a thin metal band, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-wheel.png (커밋 4f89a35)

### [x] Resources/Art/Pets/pet-t3-wheel.png — 희귀 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a reinforced wheel with a patterned tread and small bolted plates, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-wheel.png (커밋 4f89a35)

### [x] Resources/Art/Pets/pet-t4-wheel.png — 영웅 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  twin wheels with a metal rim and reinforced plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-wheel.png (커밋 4f89a35)

### [x] Resources/Art/Pets/pet-t2-wing.png — 고급 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  slightly longer wings with soft feather tips, no plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-wing.png (커밋 6fd71f7)

### [x] Resources/Art/Pets/pet-t3-wing.png — 희귀 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  wings with small metal cuffs at the joints, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-wing.png (커밋 6fd71f7)

### [x] Resources/Art/Pets/pet-t4-wing.png — 영웅 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  layered wings with reinforced plating along the leading edge, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-wing.png (커밋 6fd71f7)

### [x] Resources/Art/Pets/pet-t2-ore.png — 고급 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a slightly larger lump with a few cleanly cut facets, dull surface, no glowing core,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere.
- 참고: 투명 PNG로 받는다. 결정은 보랏빛으로 새기 쉬워서 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-ore.png (커밋 14f4d2f)

### [x] Resources/Art/Pets/pet-t3-ore.png — 희귀 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a clustered body with sharper faceted shards, no glowing core,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere.
- 참고: 투명 PNG로 받는다. 결정은 보랏빛으로 새기 쉬워서 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t3-ore.png (커밋 14f4d2f)

### [x] Resources/Art/Pets/pet-t4-ore.png — 영웅 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a layered crystal body with a metal band around its middle, no glowing core,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere.
- 참고: 투명 PNG로 받는다. 결정은 보랏빛으로 새기 쉬워서 금지 줄을 붙였다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t4-ore.png (커밋 14f4d2f)

### [x] Resources/Art/Pets/pet-t2-carrier.png — 고급 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a slightly larger cloth saddlebag with buckled straps, no plating, no glow,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 같은 계열 1등급·초월과 나란히 놓았을 때 같은 생물로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t2-carrier.png (커밋 0062fee)

> **계열 넷의 1등급(일반) 골격 4장이다.** `pet-gacha.md` 7절 "올리는 순서" 2번 —
> 무료 뽑기에서 제일 자주 보인다. 7절 "1~4등급 52종"대로 **이 4장은 색 변종의 바탕**이라
> 행성 색을 안 쓰고 중립 회청색(#8C94B8)으로 받는다. 나중에 행성 색으로 갈아 끼운다.
> 계열 문장은 초월 10종과 **똑같은 문장**을 쓰고 등급 문장만 1등급용으로 바꿨다.

### [x] Resources/Art/Pets/pet-t1-wheel.png — 일반 바퀴족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub,
  a small plain wheel with a rubber tread, no decoration, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 초월 바퀴족 셋과 나란히 놓았을 때 같은 생물의 어린 모습으로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-wheel.png (커밋 bbe60ee)

### [x] Resources/Art/Pets/pet-t1-wing.png — 일반 날개족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with short stubby wings hovering in place, big friendly eyes,
  plain bare wings, no plating, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 초월 날개족 셋과 같은 생물의 어린 모습으로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-wing.png (커밋 bbe60ee)

### [x] Resources/Art/Pets/pet-t1-ore.png — 일반 광석족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature that is a cluster of raw crystal with two big friendly eyes set into the stone,
  a small rough uncut lump, dull surface, no glowing core, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
  Keep the stone strictly neutral grey with a faint blue tint — no pink, no magenta, no violet, no purple tints anywhere, including highlights and edges.
- 참고: 투명 PNG로 받는다. 지오드코어처럼 결정은 보랏빛으로 새기 쉬워서 금지 줄을 처음부터 붙였다
- 참고 추가: 첫 판은 응답이 아예 안 왔다(그림이 나쁜 게 아니라 빈 응답). 같은 프롬프트로 한 번 더 보내니 바로 나왔다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-ore.png (커밋 bbe60ee)

### [x] Resources/Art/Pets/pet-t1-carrier.png — 일반 짐꾼족 골격
- 크기: 1024x1024 정사각
- 용도: 펫 뽑기·도감. 행성 색 변종 12종의 바탕
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A plump four-legged creature with a cargo pannier on its back, big friendly eyes,
  a simple empty cloth saddlebag, no plating, no glow, no ornament,
  rendered with #8C94B8 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient, centered, readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 초월 짐꾼족 둘과 같은 생물의 어린 모습으로 읽혀야 한다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/pet-t1-carrier.png (커밋 bbe60ee)

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

### [x] Resources/Art/Pets/6-myth/ore-01.png — 신화 — 결정 대장장이 (광석족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/ore-01.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-01.png — 신화 — 강철 등짐꾼 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-01.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-02.png — 신화 — 심해 운반자 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-02.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-03.png — 신화 — 중력 포터 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-03.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-04.png — 신화 — 보급의 어머니 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-04.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-05.png — 신화 — 이동 창고 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-05.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-06.png — 신화 — 마지막 짐꾼 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-06.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/haul-07.png — 신화 — 느린 거인 (짐꾼족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small stout creature with a cargo box strapped on its back, short sturdy legs, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/haul-07.png (커밋 47f9ba8)

### [x] Resources/Art/Pets/6-myth/wheel-03.png — 신화 — 분쇄의 무희 (바퀴족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wheel-03.png (커밋 d295363)

### [x] Resources/Art/Pets/6-myth/wing-02.png — 신화 — 유성 전령 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-02.png (커밋 d295363)

### [x] Resources/Art/Pets/6-myth/wing-05.png — 신화 — 월광 활공자 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-05.png (커밋 d295363)

### [x] Resources/Art/Pets/6-myth/wing-06.png — 신화 — 전파 나그네 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-06.png (커밋 d295363)

### [x] Resources/Art/Pets/6-myth/wing-07.png — 신화 — 섬광 매 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-07.png (커밋 d295363)

### [x] Resources/Art/Pets/6-myth/wing-08.png — 신화 — 고요의 감시자 (날개족)
- 크기: 768x768 정사각
- 용도: 펫 뽑기 — 고급·특수 뽑기 주력 등급. 고유 효과를 가진다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, heavily ornamented ceremonial form with layered armor and multiple glowing runes, a distinct silhouette that reads apart from its family siblings, rendered with rich jewel tones,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/6-myth/wing-08.png (커밋 d295363)

### [x] Resources/Art/Pets/5-legend/wheel-quartz.png — 전설 — 바퀴족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-quartz.png (커밋 8b576c6)

### [x] Resources/Art/Pets/5-legend/wheel-ruby.png — 전설 — 바퀴족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-ruby.png (커밋 8b576c6)

### [x] Resources/Art/Pets/5-legend/wheel-sapphire.png — 전설 — 바퀴족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-sapphire.png (커밋 8b576c6)

### [x] Resources/Art/Pets/5-legend/wheel-aquamarine.png — 전설 — 바퀴족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-aquamarine.png (커밋 8b576c6)

### [x] Resources/Art/Pets/5-legend/wheel-cinnabar.png — 전설 — 바퀴족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small round creature whose body is a single wheel, big friendly eyes on the hub, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wheel-cinnabar.png (커밋 8b576c6)

### [x] Resources/Art/Pets/5-legend/wing-quartz.png — 전설 — 날개족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-quartz.png (커밋 4bf7d2e)

### [x] Resources/Art/Pets/5-legend/wing-ruby.png — 전설 — 날개족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-ruby.png (커밋 4bf7d2e)

### [x] Resources/Art/Pets/5-legend/wing-sapphire.png — 전설 — 날개족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-sapphire.png (커밋 4bf7d2e)

### [x] Resources/Art/Pets/5-legend/wing-aquamarine.png — 전설 — 날개족 (아쿠아마린 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #59D9CC as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-aquamarine.png (커밋 4bf7d2e)

### [x] Resources/Art/Pets/5-legend/wing-cinnabar.png — 전설 — 날개족 (주사 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small floating creature with two short stubby wings and a rounded body, big friendly eyes, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D0F as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/wing-cinnabar.png (커밋 4bf7d2e)

### [x] Resources/Art/Pets/5-legend/ore-quartz.png — 전설 — 광석족 (쿼츠 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #E8EDFF as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-quartz.png (커밋 1b839fa)

### [x] Resources/Art/Pets/5-legend/ore-ruby.png — 전설 — 광석족 (루비 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #D94D4D as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-ruby.png (커밋 1b839fa)

### [x] Resources/Art/Pets/5-legend/ore-sapphire.png — 전설 — 광석족 (사파이어 색)
- 크기: 512x512 정사각
- 용도: 펫 뽑기 — 전설 등급
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, fully transparent background, restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A small creature made of a faceted crystal cluster with two big friendly eyes set into the front face, elaborate armored shell, glowing seams, a small hovering halo, rendered with #4D6ED9 as the dominant accent,
  on a fully transparent background — real alpha channel, no background color, no checkerboard,
  no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 투명 PNG로 받는다. 세션이 `check_alpha.py`(투명 모드)로 RGBA·모서리·잔상을 검사한 뒤 넣는다
- 들어간 곳: PlanetRacer/Assets/Resources/Art/Pets/5-legend/ore-sapphire.png (커밋 1b839fa)
