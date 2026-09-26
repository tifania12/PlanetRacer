# 피드백

여기에 적힌 것을 밤·새벽 작업이 **다른 무엇보다 먼저** 읽고 처리한다.
아침에 웹에서 확인하고 이상한 점이 있으면 여기 한 줄 적어 두면 된다.
대화창에 말해도 되고, 그러면 내가 여기로 옮겨 적는다.

## 규칙

- `- [ ]` 로 시작하는 줄이 처리 대상이다.
- 처리한 줄은 `- [x]` 로 바꾸고 뒤에 `→ 커밋해시 한 줄 설명` 을 붙인다.
- 판단이 필요해서 처리하지 못한 줄은 `- [?]` 로 바꾸고 무엇이 막혔는지 적는다.
- 다 처리된 항목은 두 주가 지나면 아래 "지난 것"으로 내린다.

## 처리할 것

- [x] **(2026-09-14 16:50, 수정 배포 중 → 20:22 빌드/배포 자체는 성공 확인) 웹에서 첫 프레임에 `RangeError: Maximum call stack size exceeded`.**
  → **해결됨 (2026-09-15 01:50 배포본 확인).** UI를 uGUI로 옮기면서 사라졌다. UIDocument 여덟 개가
    한 패널에 붙어 레이아웃 재귀가 깊어진 것이 원인이었다는 뜻이다. 스택 5MB와 예외 표시도 같이 켜 뒀지만
    결정적인 건 UI 이사였다. 콘솔 에러 0.
  `stripEngineCode`를 끄자 UI가 살아났고, 그러자마자 이 오류로 죽었다. UI Toolkit 레이아웃이
  비주얼 트리를 재귀로 훑는데 emscripten 기본 스택이 64KB라 그대로 넘친 것으로 본다.
  `-sSTACK_SIZE=5242880`(5MB)로 올리고 `exceptionSupport`를 `None` → `ExplicitlyThrownExceptionsOnly`로
  바꿔 다시 배포했다. **배포 후 실제로 열어서 확인할 것.** 그래도 나면 스택이 원인이 아니라
  진짜 무한 재귀이므로, 이제 예외 메시지에 관리 코드 스택이 찍히니 그걸로 지점을 잡는다.
  — **20:22 갱신**: 그 사이 세 번 연속 exit 139로 빌드 자체가 안 나오고 있었다(D08-15가 원인 수정).
  이번 세션에서 `main`(D08-17, `1f3c10e`) 빌드+Cloudflare 배포+"배포 확인" 스텝까지 **전부 성공**한
  것을 GitHub Actions run #65로 확인했다 — 스택 크기 수정이 실제로 production에 나간 것은 이번이
  처음이다. 다만 이 클라우드 세션은 조직 egress 정책으로 `*.pages.dev`에 직접 못 나가서(403,
  CLAUDE.md에 새로 적어 둠) 화면을 눈으로 보는 확인은 못 했다 — **아침에 열어서 실제로 살아있는지
  볼 것.**

<!-- 여기에 추가 -->

- [x] **(2026-09-14 22:xx 수정 → 배포 후 확인 필요) 웹 빌드에서 URP 셰이더 세 개가 안 먹는다.**
  → **해결됨 (2026-09-15).** `Skybox/Procedural`을 Always Included 셰이더에 넣으니 스카이박스와
    앰비언트가 살아났다. 배포본에서 행성이 밝은 흰색, 하늘도 정상.
  브라우저 콘솔에 `Hidden/CoreSRP/CoreCopy`, `Hidden/Universal Render Pipeline/StencilDitherMaskSeed`,
  `Hidden/Universal/HDRDebugView`가 "not supported on this GPU"로 찍히고, 전체 화면이 어두운
  빨강으로 나오는 문제. 세 셰이더 전부 `UniversalRenderPipelineGlobalSettings.asset`에
  `HdrDebugViewPS`/`StencilDitherMaskSeedPS`/`RenderGraphUtilsResources.CoreCopyPS`로 등록된
  URP 전역 리소스였다 — `SampleSceneProfile.asset`의 Tonemapping 오버라이드에 `paperWhite`/
  `minNits`/`maxNits` 같은 실물 HDR 디스플레이 전용 값이 들어 있는 것과 맞춰 보면, WebGL이
  올릴 수 없는 "HDR Output" 경로가 켜져 있던 게 원인으로 보인다(브라우저는 진짜 HDR 디스플레이
  출력을 지원하지 않음). `Assets/Settings/Mobile_RPAsset.asset`(WebGL·모바일 기본 품질
  Mobile이 쓰는 URP 에셋, `QualitySettings.asset`의 WebGL 기본값 0번과 연결)에서
  `m_SupportsHDR: 1→0`, `m_PrefilterHDROutput: 1→0`으로 HDR Output 자체를 꺼서 HDRDebugView·
  관련 CoreCopy 블릿 변형이 빌드에서 빠지게 했다. StencilDitherMaskSeed는 LOD 크로스페이드의
  스텐실 디더링 패스가 원인으로 보여 같은 파일에서 `m_EnableLODCrossFade: 1→0`도 같이 껐다
  (LOD 전환 시 살짝 팝핑이 생길 수 있음 — 화면이 안 뜨는 것보다는 나은 트레이드오프).
  PC_RPAsset(Standalone/Steam 기본값)은 데스크톱 GPU라 문제가 없을 걸로 보고 그대로 뒀다.
  Unity 에디터가 없어 컴파일·실제 렌더링 확인은 못 했다 — **배포 후 실제로 열어서 화면이
  밝게 뜨는지, 콘솔에 저 셰이더 에러가 사라졌는지 볼 것.**
- [x] **웹 빌드에서 HUD가 안 보인다.** MainGame 씬에 UIDocument 여덟 개가 있고 에디터에서는
  → **해결됨 (2026-09-15).** uGUI로 옮기고 TMP 셰이더를 Always Included에 넣었다. 한글이 안 나오던 것
    (유니티 기본 폰트에 한글 글리프 없음)도 Pretendard TMP 폰트로 같이 해결. 배포본에서
    '쿼츠 행성 / 원석 0.0 / 업그레이드·제작·레이스·상자·설정' 전부 글자로 확인.
  HUD·튜토리얼이 Flex로 떠 있는데, 웹에서는 3D만 보이고 UI가 하나도 안 그려진다.
  `PanelSettings.asset`을 직접 읽어 봤는데 RenderMode(ScreenSpaceOverlay)·스케일 모드·
  기본 셰이더 참조(UIR-Default 등, GraphicsSettings의 Always Included Shaders에 이미 포함됨)는
  전부 정상으로 보인다 — PanelSettings 자체의 설정 문제는 아닐 가능성이 높다. 위 URP 셰이더
  항목이 원인이라 3D 렌더링이 깨지면서 같이 죽었을 가능성이 있으니, 위 수정 배포 후 HUD가
  같이 살아났는지부터 확인. 그래도 안 뜨면 PanelSettings이 아니라 UIDocument들의 소팅 오더나
  MainGame 씬의 카메라 스택(오버레이 카메라 누락 등)을 다음으로 볼 것.
- [?] **"Made with Unity" 스플래시는 Personal 라이선스에서는 못 끈다(확인됨).** 에디터에서
  `PlayerSettings.SplashScreen.show = false`가 먹고 ProjectSettings에도 0으로 들어갔지만,
  실제 배포본을 브라우저로 열면 여전히 나온다. 빌드 시점에 라이선스가 다시 켠다.
  로딩 화면의 유니티 큐브는 커스텀 템플릿으로 제거됐다 — 남은 건 이것뿐이다.
  없애려면 Unity Pro/Plus로 올리는 방법밖에 없다. **Tifania가 결정할 것** — 코드로 더
  할 수 있는 게 없어 2026-09-14 야간 세션에서 판단 대기로 옮긴다.

- [?] **HUD 액션 줄이 9칸이 되면서 긴 라벨이 잘린다.** (2026-09-25 07시 Unity 배선 세션에서
  `btn-planet`을 넣다가 확인) action-row는 HorizontalLayoutGroup(childControlWidth +
  forceExpandWidth)이라 버튼이 늘 때마다 칸 너비가 같이 줄어든다 — 8칸 57px → 9칸 50px.
  `btn-mine`("업그레이드", 5글자)은 최소 크기 14pt에서도 50px에 안 들어가 끝이 잘린다.
  **다만 8칸이던 때에도 이미 잘리고 있었다**(btn-planet을 껐다 켜며 A/B로 확인) — 이번
  추가로 생긴 회귀는 아니다. 남은 화면(P-15 확률 공개 등)이 버튼을 더 달면 짧은 라벨까지
  위험해진다. 고치는 길은 세 가지인데 어느 쪽인지는 취향 문제라 **Tifania 결정 대기**:
  (1) 라벨을 줄인다("업그레이드"→"강화"), (2) 액션 줄을 두 줄로 접는다,
  (3) 가로 스크롤 줄로 바꾼다.
  — **2026-09-26 10시 갱신**: M-14(시즌 패스 화면) 코드를 준비하면서 `GemRacer/34`로 action-row에
  "btn-seasonpass"를 하나 더 추가하는 메뉴를 만들어 뒀다 — 다음 Unity 세션이 그 메뉴를 누르면
  9칸이 10칸이 된다. 새 화면의 접근 경로 자체를 안 만들 수는 없어서 버튼은 그대로 뒀지만,
  판단이 나기 전까지는 라벨이 지금보다 더 잘릴 수 있다는 뜻이다.

## 지난 것

<!-- 처리 끝난 항목 -->
