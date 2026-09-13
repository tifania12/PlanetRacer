# 안드로이드 빌드 체크리스트

D15-N. 실제 APK를 처음 뽑기 전에 PC에서 Unity 에디터로 한 번씩 확인·적용할 항목들.
클라우드 세션은 에디터가 없어 `Edit > Project Settings > Player`를 직접 열어 볼 수 없다 —
그래서 코드로 확실히 되는 부분(`GemRacer/9`, `GemRacer/10` 메뉴)은 스크립트로 만들어 두고,
사람이 판단하거나 비밀 정보가 들어가는 부분(키스토어)만 절차 문서로 남긴다.

## Player Settings

| 항목 | 지금 값 (`ProjectSettings.asset` 확인) | 목표 | 상태 |
|---|---|---|---|
| Company/Product Name | `DefaultCompany` / `PlanetRacer` | Company는 실제 이름으로(스토어 등록 전 확인) | **PC 세션 확인 필요** |
| Package Name (Application ID, Android) | `com.UnityTechnologies.com.unity.template.urpblank` — URP 템플릿이 남긴 placeholder | 실제 값으로 교체 | **결정 필요 → `docs/decisions.md` T-09** |
| Minimum API Level | 25 (Android 7.1) | 그대로 유지해도 됨 — 지금 목표 기기 범위에서 낮출 이유가 없다 | 확인만 |
| Target API Level | Auto(최신) | 그대로 유지 | 확인만 |
| Scripting Backend (Android) | IL2CPP | 이미 맞음 | 확인만 |
| Target Architecture | ARM64 | 이미 맞음(`AndroidTargetArchitectures: 2`) | 확인만 |
| 화면 방향 | 자동 회전(4방향 전부) | **세로 고정** — CLAUDE.md 6번 규칙대로 모바일은 세로(9:16)가 기준 화면이다 | `GemRacer/9` 메뉴로 적용 |
| 키스토어 | 없음(`AndroidKeystoreName` 비어 있음) | 아래 절차대로 PC에서 직접 생성 | **PC 세션에서 수동 진행**, 저장소에 커밋 금지 |

`GemRacer/9. 안드로이드 세로 고정 적용` 메뉴가 하는 일 — 방향 관련 필드만 건드린다(최소/목표
API, 스크립팅 백엔드, 아키텍처는 이미 적정값이라 스크립트가 손대지 않는다):

- `defaultInterfaceOrientation` → Portrait
- `allowedAutorotateToPortrait` → 켬, 나머지 세 방향(뒤집힌 세로·좌우 가로) → 끔

## 키스토어 절차 (PC에서만, 절대 저장소에 커밋하지 않는다)

1. Unity 메뉴 `Edit > Project Settings > Player > Publishing Settings` (또는 `Window > Package
   Manager`가 아니라 상단 메뉴의 Keystore Manager)에서 새 키스토어를 만든다.
2. 키스토어 파일(`.keystore`/`.jks`)과 비밀번호·별칭은 **저장소 밖**에 둔다 — `PlanetRacer/`
   바깥의 안전한 폴더나 비밀번호 관리자. `.gitignore`에 `*.keystore`/`*.jks` 패턴이 아직 없으면
   이번에 추가해 둔다(아래 반영).
3. 키스토어를 한 번 잃으면 같은 패키지명으로는 업데이트를 낼 방법이 없다(구글 정책) — 반드시
   저장소 바깥의 다른 곳(클라우드 드라이브 등)에도 백업 하나를 더 만들어 둔다.
4. Play Console에 앱을 처음 등록할 때 서명 키를 Play App Signing에 맡기면 이후 키 분실 위험이
   줄어든다 — 처음 업로드할 때 선택할 것.

## PC(Steam) 창 설정

`GemRacer/10. PC 세로 창 설정 적용` 메뉴 — Standalone 기본 창을 540×960 세로로 잡고 리사이즈를
허용한다(CLAUDE.md 6번 규칙의 세 기준점 중 세로 기준을 PC에서도 그대로 열어 볼 수 있게, 창을
넓히면 가로 960×540/태블릿 1280×800 배치로 반응형이 바뀌는지 확인하는 용도).

- `defaultScreenWidth` 540, `defaultScreenHeight` 960
- `resizableWindow` 켬
- `fullScreenMode` → Windowed (지금은 FullScreenWindow로 창 모드가 아니었다)

## 다음에 할 것 (이 문서가 커버 못 하는 부분)

- 패키지명(T-09)이 정해지면 `GemRacer/9` 메뉴에 `PlayerSettings.SetApplicationIdentifier` 호출을
  추가해 같이 적용되게 한다 — 지금은 값이 아직 없어서 스크립트가 건드리지 않는다.
- 이 체크리스트 자체도 PC에서 Unity를 열어 실제 가로 안에서 표에 있는 "확인만"이라고 적은
  줄들이 진짜 그런지 한 번 대조해야 한다(D15-M).
