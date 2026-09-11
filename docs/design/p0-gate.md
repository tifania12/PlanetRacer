# P0 프리프로덕션 관문 확인 (D00–D03)

2026-09-11 밤 세션에서 작성. D03까지 끝나 P0을 마감하며 상태를 정리한다.

## 됐다고 보는 것

- **코어 골격** — `Packages/com.bax.gemracer.core`에 모델(Planet/Course/Part/Stats 등),
  MiningSimulator, RaceSimulator, DefaultData. `Core.Tests`가 `dotnet run`으로 오프라인 도는
  테스트 러너를 돌린다. 지금 14개 통과 / 0 실패(D00-N).
- **행성 표면 이동 확인** — `SurfaceMover`가 구체 표면을 도는 걸 Unity 실기로 확인했다.
  반지름 20m 유지, 극점 로직도 실제로 돌려서 문제없음 확인(D01-N/M).
- **레이스 카메라 방향 확정** — 2D/3D 실험 끝에 3D로 확정, 근거는
  `docs/design/art-and-presentation.md`(D01.5).
- **밸런스 데이터 파이프라인** — `docs/design/balance/*.csv` → `BalanceCsv`(코어) →
  `Assets/Editor/ImportBalance.cs` → `Assets/Data/Balance.asset`. CSV 값이 `DefaultData`와
  같은지 테스트로 고정해 뒀다(D02-N/M). 아직 아무 런타임 코드도 Balance.asset을 안 읽는다 —
  실제로 붙는 건 게임 상태 머신이 생기는 D04 이후.
- **세이브 라운드트립** — `Core/SaveData.cs`(순수 데이터 클래스) +
  `Assets/Scripts/Save/SaveService.cs`(JsonUtility, 임시 파일 교체로 원자적 쓰기). 직렬화→역직렬화가
  값을 그대로 보존하는지 `Core.Tests`로 확인(D03-N/M). Unity의 실제 파일 I/O(퍼시스턴트 경로 쓰기·
  읽기)는 에디터가 없어서 실기 검증은 못 했다 — 아침 확인 필요.

## 아직 못 하거나 다음으로 미룬 것

- **장비 비용 공식** — 곡괭이·화물칸 등 채굴 장비의 업그레이드 비용 공식이 아직 코어에 없다.
  그래서 D02 밸런스 CSV에서 장비 표는 뺐다. D05-N(채굴 장비 업그레이드 UI)에서 공식이 생기면
  그때 CSV와 ImportBalance에 추가한다.
- **Unity 실기 검증 3건** — `BalanceCsv`/`ImportBalance.cs`의 실제 컴파일·메뉴 실행,
  `SaveService`의 실제 파일 쓰기/읽기, UI Toolkit `PanelSettings`/`UIDocument` 설정(W-03)은
  전부 클라우드 세션이라 컴파일 확인을 못 했다. 다음 대화형(Unity 연결) 세션에서 콘솔 에러
  여부부터 봐야 한다.
- **코어 루프 개정 반영(L-01~L-05)** — backlog에 "P1 항목보다 먼저 확인할 것"으로 표시돼 있다.
  P0 자체는 끝났지만, D04(게임 상태 머신)로 넘어가기 전에 이 항목들을 먼저 정리해야
  코어 루프 v0.3(상호 강화 나선)에 맞는 설계로 P1을 시작할 수 있다.

## 결론

D00–D03 스코프는 끝났다고 본다. 남은 리스크는 전부 "Unity 실기로 확인 안 됨"이지,
설계가 막혔거나 값이 안 맞는 문제는 아니다. 다음 대화형 세션에서 위 3건 컴파일 확인부터
하고, 그다음 P1로 넘어가기 전에 L-01~L-05(코어 루프 재설계)를 먼저 짚고 가는 순서를 추천한다.
