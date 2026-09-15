# 이미지 요청 대기열 — GPT로 만들 것

Tifania가 ChatGPT Plus를 쓰고, **이미지 생성은 거기서 한다**(2026-09-15 결정).
예약 세션들은 GPT에 접근할 수 없다. 그래서 **이미지가 필요해지면 직접 만들려 하지 말고
여기에 프롬프트를 적어 둔다.** Tifania가 GPT에서 뽑아 `PlanetRacer/Assets/Art/` 에 넣는다.

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
UI 아이콘은 `BootstrapHudUgui.cs` 위쪽의 색 상수와 어울리게 — 남색 바탕에 밝은 회백색 선.
사실적인 렌더링보다 단순한 형태가 낫다. 작은 화면에서 알아볼 수 있어야 한다.

## 대기 중

<!-- 여기에 추가 -->

### icon-part-engine.png — 부품: 엔진
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a compact futuristic vehicle engine block with glowing intake vents, rendered with #708CFF as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-part-tire.png — 부품: 타이어
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a chunky off-road vehicle tire seen at a three-quarter angle, rendered with #8C94B8 as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-part-suspension.png — 부품: 서스펜션
- 크기: 512x512 정사각
- 용도: 업그레이드·제작 화면 부품 슬롯
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a coil-over suspension strut with a spring, rendered with #8C94B8 as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-gear-tool.png — 채굴 장비: 도구
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a mining drill bit with a faceted crystal tip, rendered with #59D9CC as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-gear-cargo.png — 채굴 장비: 화물칸
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비 + 화물칸 게이지 옆
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of an open cargo container half filled with rough crystal ore, rendered with #E6E6F0 as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-gear-engine.png — 채굴 장비: 엔진
- 크기: 512x512 정사각
- 용도: 업그레이드 화면 채굴 장비
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a rugged tracked-vehicle drive unit with a single piston, rendered with #708CFF as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-raw-mineral.png — 재화: 원석
- 크기: 512x512 정사각
- 용도: HUD 상단 원석 숫자 옆, 모든 보상 표시
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a cluster of three rough uncut crystal shards, rendered with #E6E6F0 as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다

### icon-refined-mineral.png — 재화: 정제 광물
- 크기: 512x512 정사각
- 용도: 제련소·상점. 원석과 한눈에 구분되어야 한다
- 프롬프트:
  Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left, low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette, no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette readable at small size.
  A single game UI icon of a single polished faceted gem cut into a clean hexagon, rendered with #708CFF as the dominant accent,
  on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
  centered, square composition, simple bold shapes readable at 64x64 pixels.
- 참고: 받은 뒤 `python tools/strip_bg.py <경로>` 로 마젠타 배경을 뺀다


## 들어온 것

<!-- 프로젝트에 반영된 것 -->
