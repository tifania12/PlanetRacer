# 아트 계획 — GPT로 뽑을 것들

2026-09-15. Tifania가 ChatGPT Plus의 이미지 생성을 주력으로 쓰기로 했다.
이 문서는 **무엇을 어떤 순서로 뽑을지**와 **어떻게 해야 50장이 한 게임처럼 보이는지**를 정한다.

실제 요청은 `art-requests.md` 대기열에 쌓고, 밤 20~08시 이미지 세션이 하나씩 뽑는다.

---

## 0. 제일 중요한 것 — 스타일 고정문

AI 아트로 게임을 만들 때 실패하는 지점은 퀄리티가 아니라 **통일감**이다.
한 장씩 보면 다 괜찮은데 모아 놓으면 다른 게임 다섯 개처럼 보인다.

그래서 **모든 프롬프트 맨 앞에 아래 문단을 그대로 붙인다.** 예외 없이.

```
Style: clean stylized 3D game art, soft matte surfaces, gentle rim light from upper left,
low-poly-inspired faceted forms, muted deep-navy background (#121424), restrained palette,
no text, no watermark, no UI chrome, centered composition, even lighting, crisp silhouette
readable at small size.
```

이 문단은 **고치지 않는다.** 바꾸고 싶어지면 이미 뽑은 것들과 어긋나므로,
바꾸려면 `decisions.md`에 기록하고 기존 에셋을 다시 뽑을 각오를 한다.

## 1. 색 — 게임 안에서 이미 정해진 값

행성 색은 `GemColors.cs`에 박혀 있다. 프롬프트에 **헥사 값을 직접 적는다.**
"루비색"이라고 쓰면 GPT가 매번 다른 빨강을 준다.

| 행성 | 헥사 | 성격 (DefaultData) |
|---|---|---|
| 쿼츠 quartz | `#E6E6F0` | 기본. 흰빛에 살짝 푸른 기 |
| 루비 ruby | `#BF0F29` | 뜨겁다 (Heat 0.9), 거칠다 |
| 사파이어 sapphire | `#1238A8` | 춥다 (Cold 0.9) |
| 아쿠아마린 aquamarine | `#59D9CC` | 액체가 있다 (Liquid 0.5) |
| 주사 cinnabar | `#D94D0F` | 유독하다 (Toxic 0.7), 매우 거칠다 |
| 라피스 라줄리 lapis | `#1C298C` | 저중력(0.25), 희박한 대기(0.15) |

UI 색은 `BootstrapHudUgui.cs` 상수를 따른다.

| 쓰임 | 헥사 |
|---|---|
| 글자 (밝은) | `#E8EDFF` |
| 글자 (흐린) | `#8C94B8` |
| 패널 바탕 | `#121424` |
| 버튼 면 | `#292E4D` |
| 게이지 채움 | `#708CFF` |

## 2. 무엇을 뽑나 — 우선순위 순

**지금 만들고 있는 화면이 필요로 하는 것부터.** 예쁜 것보다 막힌 것을 먼저 뚫는다.

### 1순위 — UI 아이콘 (U-02 업그레이드 화면이 기다린다)

작고 많다. 한 장에 하나씩, 512x512, 정사각.

- 부품 슬롯 3종: 엔진 / 타이어 / 서스펜션
- 채굴 장비 3종: 도구(곡괭이·드릴·레이저 티어별) / 화물칸 / 엔진
- 재화 5종: 원석, 정제 광물, 상자 열쇠, 청사진, 연료
- 공구 상자 3종: 녹슨 / 강철 / 티타늄
- 등급 배지 4종: C / B / A / S

합 18장. 이게 다 들어오면 업그레이드·제작·상자 화면이 한꺼번에 살아난다.

### 2순위 — 행성 구체 6종

행성 선택·워프 화면과 타이틀에 쓴다. 1024x1024.
표면 질감이 보이는 구체 하나. 배경은 우주가 아니라 **단색**으로 —
게임 안에서 배경 위에 얹을 것이기 때문이다.

### 3순위 — 채굴차 3티어

곡괭이 시절 / 드릴 시절 / 레이저 시절. 같은 차가 자라는 느낌이어야 한다.
**한 장에 세 대를 나란히** 그려 달라고 하면 통일감이 훨씬 좋다. 그다음 잘라 쓴다.

### 4순위 — 컷신 일러스트

여기서 GPT의 힘이 제일 크게 난다. 16:9, 1536x1024 이상.

- 오프닝: 채굴선이 쿼츠 행성에 내려앉는 장면
- 행성 도착 6장: 각 행성의 첫인상 (루비=용암빛 절벽, 사파이어=얼음 결정 평원,
  아쿠아마린=청록 액체 호수, 주사=붉은 유독 안개, 라피스=저중력에 떠오른 파편들)
- 첫 레이스 우승
- 엔딩 후보 (P2에서)

### 5순위 — 스토어용

앱 아이콘, Steam 라이브러리 아트, 배너. **출시가 가까워지면.** 지금은 아니다.

## 3. 실무에서 걸리는 것들

**배경 투명이 안 나온다.** GPT는 투명 PNG를 잘 안 준다. 그래서 아이콘은
`solid flat background of pure magenta #FF00FF, no shadow, no gradient` 로 요청하고,
`tools/strip_bg.py` 로 그 색을 빼서 투명하게 만든다. 그림자를 넣으면 경계가 지저분해지니
그림자는 빼 달라고 명시한다.

**정확한 크기가 안 나온다.** 요청한 비율 근처로만 준다. Unity에서 임포트할 때
Sprite로 잡고 Pixels Per Unit으로 맞춘다. 픽셀 단위 정확도가 필요한 것
(9-slice 프레임, 타일링 텍스처, 폰트)은 **GPT로 뽑지 않는다.** 코드나 Unity 기본 에셋으로 만든다.

**같은 걸 다시 못 만든다.** 마음에 든 그림이 나오면 그 프롬프트를 `art-requests.md`의
"들어온 것"에 **그대로 남긴다.** 나중에 변형이 필요할 때 그 프롬프트에서 출발한다.

**한 대화에 한 장.** 앞 대화가 남아 있으면 엉뚱하게 섞인다. 이미지 세션도 그렇게 하게 해 뒀다.

## 4. 프롬프트 형판

아이콘:

```
<스타일 고정문>
A single game UI icon of <무엇>, rendered in <행성색 헥사> as the dominant accent,
on a solid flat background of pure magenta #FF00FF, no shadow, no gradient,
centered, square composition, simple bold shapes readable at 64x64 pixels.
```

컷신:

```
<스타일 고정문>
Wide cinematic illustration, 16:9. <장면 설명>. Dominant color <헥사>.
No characters' faces in close-up, no text overlay. Atmospheric depth,
strong silhouette of the foreground element against the sky.
```

행성 구체:

```
<스타일 고정문>
A single spherical planet floating centered, surface of <보석 이름> crystal formations,
dominant color <헥사>, <성격 한 줄: 예 "cracked lava veins glowing faintly">,
on a solid flat background of pure magenta #FF00FF, no stars, no space background.
```

## 5. 들어온 뒤

`Assets/Art/` 아래에 쓰임새별로 둔다 — `Icons/`, `Planets/`, `Rigs/`, `Cutscenes/`.
Unity 임포트 설정은 아이콘·UI는 Sprite (2D and UI), 컷신은 Sprite 또는 Texture.
용량이 커지면 압축을 조인다 — 웹 빌드 용량은 이미 backlog W-09에서 보고 있는 문제다.

**아트가 없다고 기다리지 않는다.** 자리 표시자(단색 사각형, 유니티 기본 스프라이트)로 먼저
붙여서 동작을 확인하고, 그림이 들어오면 갈아 끼운다. 지금 HUD가 그렇게 돌아가고 있다.
