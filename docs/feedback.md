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

- [ ] **(2026-09-14 16:50, 수정 배포 중) 웹에서 첫 프레임에 `RangeError: Maximum call stack size exceeded`.**
  `stripEngineCode`를 끄자 UI가 살아났고, 그러자마자 이 오류로 죽었다. UI Toolkit 레이아웃이
  비주얼 트리를 재귀로 훑는데 emscripten 기본 스택이 64KB라 그대로 넘친 것으로 본다.
  `-sSTACK_SIZE=5242880`(5MB)로 올리고 `exceptionSupport`를 `None` → `ExplicitlyThrownExceptionsOnly`로
  바꿔 다시 배포했다. **배포 후 실제로 열어서 확인할 것.** 그래도 나면 스택이 원인이 아니라
  진짜 무한 재귀이므로, 이제 예외 메시지에 관리 코드 스택이 찍히니 그걸로 지점을 잡는다.

<!-- 여기에 추가 -->

- [ ] **웹 빌드에서 URP 셰이더 세 개가 안 먹는다.** 브라우저 콘솔에 `Hidden/CoreSRP/CoreCopy`,
  `Hidden/Universal Render Pipeline/StencilDitherMaskSeed`, `Hidden/Universal/HDRDebugView`가
  "not supported on this GPU"로 찍힌다. 에디터에서는 흰 행성에 파란 하늘인데 웹에서는 전체가
  어두운 빨강으로 나온다 — 조명/톤매핑이 죽은 것으로 보인다. URP 에셋의 WebGL 품질 설정,
  HDR, 그리고 Always Included Shaders를 확인할 것. (2026-09-14 배포본에서 브라우저로 직접 확인)
- [ ] **웹 빌드에서 HUD가 안 보인다.** MainGame 씬에 UIDocument 여덟 개가 있고 에디터에서는
  HUD·튜토리얼이 Flex로 떠 있는데, 웹에서는 3D만 보이고 UI가 하나도 안 그려진다.
  PanelSettings의 레퍼런스 해상도/스케일 모드와, UI Toolkit이 WebGL에서 쓰는 셰이더가
  위 항목과 같은 이유로 죽은 건 아닌지 같이 볼 것. 위 셰이더 문제를 먼저 고치고 다시 확인.
- [?] **"Made with Unity" 스플래시는 Personal 라이선스에서는 못 끈다(확인됨).** 에디터에서
  `PlayerSettings.SplashScreen.show = false`가 먹고 ProjectSettings에도 0으로 들어갔지만,
  실제 배포본을 브라우저로 열면 여전히 나온다. 빌드 시점에 라이선스가 다시 켠다.
  로딩 화면의 유니티 큐브는 커스텀 템플릿으로 제거됐다 — 남은 건 이것뿐이다.
  없애려면 Unity Pro/Plus로 올리는 방법밖에 없다. **Tifania가 결정할 것** — 코드로 더
  할 수 있는 게 없어 2026-09-14 야간 세션에서 판단 대기로 옮긴다.

## 지난 것

<!-- 처리 끝난 항목 -->
