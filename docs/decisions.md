# 결정 기록

| 날짜 | 결정 | 이유 |
|---|---|---|
| 2026-09-10 | 대전 부품 획득은 청사진 복제(B안). 패자 손실 없음 | 패배 이탈 방지, 서버 로직 절반 |
| 2026-09-10 | 유료 뽑기 없음. 공구 상자는 게임 안에서만 | 가챠 위 PvP 밸런스 붕괴(이전 기획 검토 지적) 회피 |
| 2026-09-10 | 코어 로직은 순수 C#, Unity 참조 금지 | 서버 검증 코드 공유, 에디터 없는 환경에서 테스트 |
| (미정) | 수익 모델 A(무료+인앱) / B(유료) | P0 안에 Tifania 결정 |
| (미정) | UI 세로 단일 레이아웃 | 22주 일정의 전제. P0 안에 결정 |
| (미정) | 백엔드 Nakama / PlayFab | P3 전 |
| 2026-09-11 | Run In Background 켬 (PlayerSettings) | 방치형이라 창이 뒤에 있어도 채굴이 돌아야 한다. 에디터 검증도 가능해짐 |
| 2026-09-11 | 카메라·이동 보간은 프레임 독립(1-exp(-k*dt)) | 모바일 30fps와 PC 고주사율에서 감이 달라지면 안 됨 |
| 2026-09-11 | 3D로 확정 (2D 사이드뷰 검토했으나 기각) | 실제로 만들어 확인. 속도감은 2D/3D가 아니라 지면 텍스처와 카메라가 결정. docs/design/art-and-presentation.md |
| 2026-09-11 | 카메라 두 벌: 채굴(멀리·60도) / 레이스(뒤 4.5m·높이 2.2m·62~88도) | 같은 3D 씬에서 카메라만 바꾸면 되므로 아트 비용 없음 |
| 2026-09-11 | AI 생성 이미지는 평면만 담당(지면 타일·부품 아이콘·UI), 움직이는 것은 3D 메시 | AI 이미지는 다각도 일관성이 약함 |
| 2026-09-11 | 코어 루프를 자원 배분(줄다리기)에서 상호 강화 나선으로 변경 | 레이스 보상을 채굴차 부품으로 돌려 두 축이 서로를 먹이게 함. 어느 쪽에 투자해도 헛되지 않다. docs/design/core-loop.md |
| 2026-09-11 | 탐험·보물 요소 추가 | 발견은 자동(오프라인 포함), 캘지 선택은 수동. 방치형을 깨지 않으면서 능동적 목표를 준다 |
| 2026-09-11 | 지면 텍스처는 AI 대신 절차적 생성(tools/gen_planet_texture.py) | 감싸도는 거리로 계산해 이음새가 원천적으로 없고 비용 0. AI는 이음새가 남아 후처리 필요했고 무료 대안(Pollinations)은 워터마크가 박힘 |

## 2026-09-11 웹 테스트 배포 경로 확정

주소: https://planetracer-daz.pages.dev (Cloudflare Pages, main 브랜치)

main이나 claude/dev에 푸시되면 GitHub Actions가 game-ci로 WebGL을 빌드해 여기에 올린다.
Tifania는 Unity를 켜지 않고 휴대폰으로 이 주소만 연다.

확인한 것 (9/11)
- Unity 6000.3.10f1 도커 이미지가 game-ci에 있다. 첫 빌드 28분, 캐시 이후는 더 짧을 것
- 빌드 결과 14MB. Cloudflare Pages의 파일당 25MiB 제한에 여유가 있다
- Brotli 헤더가 제대로 내려간다. wasm/framework/data 전부 Content-Encoding: br,
  wasm은 Content-Type: application/wasm. web/_headers가 동작한다는 뜻이다

막혔던 곳 두 가지. 같은 실수를 반복하지 않게 적어 둔다.
- wrangler pages deploy는 프로젝트가 없으면 거부한다. 28분 빌드 뒤 마지막 줄에서 막히므로
  생성 단계를 앞에 뒀다 (이미 있으면 에러가 나므로 continue-on-error)
- game-ci는 도커 안에서 root로 빌드해 build/ 가 root 소유로 남는다.
  다음 단계에서 파일 복사조차 Permission denied가 난다. sudo chown 단계가 필요하다

Unity 계정이 구글 연동이어도 id.unity.com에서 비밀번호를 따로 만들면 game-ci가 쓸 수 있다.
비밀번호를 만들어도 구글 로그인은 그대로 된다.

### _headers 규칙은 절대 겹치면 안 된다 (2026-09-11, 첫 배포가 이것 때문에 죽었다)

증상: 화면에 빨간 줄로 "Unable to load file Build/PlanetRacer.framework.js.br".

원인: web/_headers 에 포괄 규칙 `/Build/*.br` 과 확장자별 규칙 `/Build/*.js.br` 을 같이 뒀다.
framework.js.br 에 양쪽이 걸렸고 Cloudflare가 둘을 합쳐 `Content-Encoding: br, br` 로 내려보냈다.
파일은 한 번만 압축돼 있는데 헤더는 두 번이라 말하니, 브라우저가 두 번 풀려다 죽는다.
wasm 과 data 도 똑같이 걸려 있었다.

고친 방법: 포괄 규칙을 없애고 확장자별로 딱 하나씩만 걸리게 했다. no-transform 도 넣었다.

확인 방법: `node tools/check_web_deploy.js`. 브라우저가 하는 일을 그대로 한다 —
br 로 받아 한 번 풀고, 나온 것이 진짜 자바스크립트인지 wasm 매직(\0asm)인지 본다.
워크플로의 "배포 확인" 단계가 이걸 돌리고, 실패하면 빌드를 실패로 만든다.

헤더가 200 인 것만 보고 넘어가면 이 문제를 못 잡는다. 실제로 한 번 놓쳤다.

압축 후 크기 (참고)
- wasm  8.4MB → 푼 것 48MB
- data  5.8MB → 푼 것 15MB
압축을 끄면 wasm 하나가 Cloudflare Pages 의 파일당 25MiB 제한을 훌쩍 넘는다. Brotli는 선택이 아니다.

