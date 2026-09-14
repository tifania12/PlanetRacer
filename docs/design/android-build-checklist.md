# 안드로이드 빌드 체크리스트

D15-N. 실제 APK를 처음 뽑기 전에 PC에서 Unity 에디터로 한 번씩 확인·적용할 항목들.
클라우드 세션은 에디터가 없어 `Edit > Project Settings > Player`를 직접 열어 볼 수 없다 —
그래서 코드로 확실히 되는 부분(`GemRacer/9`, `GemRacer/10` 메뉴)은 스크립트로 만들어 두고,
사람이 판단하거나 비밀 정보가 들어가는 부분(키스토어)만 절차 문서로 남긴다.

## Player Settings

| 항목 | 지금 값 (`ProjectSettings.asset` 확인) | 목표 | 상태 |
|---|---|---|---|
| Company/Product Name | `Wheel` / `PlanetRacer` | — | **완료** (2026-09-14, Tifania 결정) |
| Package Name (Application ID, Android) | `com.wheel.gemracer` | — | **완료** (2026-09-14 T-09 결정. `BuildSettingsMobilePC.AndroidPackageName` 상수) |
| Minimum API Level | 25 (Android 7.1) | 그대로 유지 | **확인됨** (에디터 실측 `AndroidApiLevel25`) |
| Target API Level | Auto(최신) | 그대로 유지 | **확인됨** (`AndroidApiLevelAuto`) |
| Scripting Backend (Android) | IL2CPP | 이미 맞음 | **확인됨** |
| Target Architecture | ARM64 | 이미 맞음 | **확인됨** (`ARM64`) |
| 화면 방향 | **세로 고정 완료** | — | **적용됨** (2026-09-14 `GemRacer/9` 실행. 기본 Portrait, 세로만 허용·나머지 3방향 꺼짐) |
| 키스토어 | `gemracer.keystore` (별칭 `gemracer`) | — | **완료** (2026-09-14 Tifania가 생성, 구글 드라이브에 백업) |

> **2026-09-14 실측** — Unity MCP로 에디터에 붙어 `GemRacer/9`·`GemRacer/10`을 실제로 실행하고
> 값을 되읽었다(backlog D15-M 완료). 아래가 지금 프로젝트의 실제 값이다.
>
> ```
> 회사/제품        Wheel / PlanetRacer
> Android 패키지명  com.wheel.gemracer
> Min API / Target  AndroidApiLevel25 / AndroidApiLevelAuto
> 백엔드 / 아키텍처  IL2CPP / ARM64
> 기본 방향         Portrait (세로만 허용, 나머지 3방향 꺼짐)
> 키스토어          gemracer.keystore (별칭 gemracer, Unity 전용 보관소)
> PC 창            540 x 960, Windowed, 크기조절 허용
> ```
>
> **체크리스트는 전부 닫혔다.** (2026-09-14: 패키지명 `com.wheel.gemracer` 확정, 키스토어 생성 완료.)
> `.gitignore`에 `*.keystore` / `*.jks`는 이미 들어가 있다(140~141행).

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


## 키스토어 — 만든 뒤 확인한 것 (2026-09-14)

Tifania가 `C:/Users/BaxXR/gemracer.keystore`로 만들고 구글 드라이브에 백업했다.
에디터에서 되읽어 확인한 값:

```
useCustomKeystore  True
keystoreName       {dedicated}: gemracer.keystore
keyaliasName       gemracer
```

**`{dedicated}`가 뭔가** — Unity 6이 키스토어를 프로젝트 밖 전용 보관소에 등록해 두고 쓰는 방식이다.
그래서 `ProjectSettings.asset`에는 파일 이름과 별칭만 들어가고 **비밀번호는 들어가지 않는다.**
실제로 커밋 전에 파일 전체를 훑어 `AndroidKeystorePass` / `AndroidKeyaliasPass` 필드가
아예 없는 것을 확인했다. 이 파일을 커밋해도 비밀이 새지 않는다.

**대신 따라오는 제약이 하나 있다.** 전용 보관소는 이 PC 안에만 있다. 그래서
**GitHub Actions는 안드로이드 APK/AAB에 서명할 수 없다** — 지금 CI는 WebGL만 빌드하니 당장은
문제가 아니지만, 나중에 CI에서 안드로이드를 뽑으려면 키스토어 파일과 비밀번호를 GitHub Secrets로
넣고 워크플로에서 복원하는 단계를 따로 만들어야 한다. 그 전까지 **APK는 이 PC에서만 나온다.**

Unity를 다시 깔거나 PC를 옮기면 전용 보관소가 비므로, 백업해 둔 `.keystore` 파일과 비밀번호로
다시 등록해야 한다. 그래서 백업이 중요하다.
