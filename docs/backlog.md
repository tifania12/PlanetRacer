# 작업 목록 (일 단위)

규칙: 위에서부터 체크 안 된 항목을 집는다. 항목 뒤의 `-N`/`-M`은 옛 표기이고 지금은 구분하지 않는다.
`docs/feedback.md`에 `- [ ]` 줄이 있으면 여기보다 먼저 처리한다.

일과 (2026-09-13 개정. 자세한 것은 CLAUDE.md)
- 평일 23:00 / 03:00 — 크게. 한 번에 2~3항목
- 평일 06:30 — 작게. 검증·배포 확인·아침 가이드만. 새 기능 금지
- 주말 매시간(하루 24번) — 한 번에 1~2항목. 예전엔 하루 네 번(09/14/19/23시)에 3~4항목씩
  크게 갔지만, 지금은 매시간 도니 그렇게 하면 세션끼리 겹친다.

날짜는 이제 맞지 않으므로 순서만 본다.

## Tifania가 먼저 해 둘 것 (D01 전)

- [x] T-01 GitHub 저장소(github.com/tifania12/PlanetRacer) 생성·골격 push 완료 (9/11). 로컬 클론: C:\Users\BaxXR\source\repos\PlanetRacer. Claude 웹 GitHub 연결은 아직
- [x] T-02 Unity Hub에서 Unity 6 LTS, URP(Universal 3D) 템플릿으로 `PlanetRacer/` 폴더에 프로젝트 생성. `Packages/com.bax.gemracer.core`가 이미 있으니 Package Manager에 "GemRacer Core"가 뜨는지 확인. 커밋·push.
- [ ] T-03 수익 모델 결정(A 무료+인앱 3종만 / B 유료 단품)을 `docs/decisions.md`에 한 줄로. UI 레이아웃은 반응형 한 벌로 정해졌다(9/11).
- [x] T-05 (9/12 오전 확인) GitHub Actions 실행 기록으로 확인 — main 브랜치 W-02 커밋들의 빌드+Cloudflare 배포가 실제로 성공했다(9/11, run #6·#8·#9). 다섯 비밀값과 Pages 프로젝트가 전부 정상 등록돼 있다는 뜻. 에디터로 직접 열어 본 건 아니라서 이상 있으면 다시 `- [ ]`로

## 반응형 레이아웃·웹 배포 (2026-09-11 추가)

- [x] W-01 GitHub Actions WebGL 빌드 + Cloudflare Pages 배포 구성, web/_headers, tools/deploy_web.ps1, WebGLBuild.cs (9/11)
- [x] W-02 첫 배포 성공 (9/11). https://planetracer-daz.pages.dev — 빌드 28분, 결과 14MB. 막혔던 두 곳은 Pages 프로젝트 부재와 root 소유 폴더 권한이었고 둘 다 워크플로에 단계를 추가해 해결
- [x] W-03 UI Toolkit 반응형 골격. `Assets/UI/Root.uxml`+`Root.uss`(3D 뷰 자리 + HUD 자리, 상태바, 버튼 3개) + `Assets/Scripts/UI/ResponsiveLayout.cs`(폭<높이면 "portrait", 아니면 "landscape" 클래스를 루트에 붙임 — 미디어 쿼리 대신). 태블릿(1280x800)도 가로라 landscape 규칙을 그대로 탄다, 즉 두 클래스로 세 기준점 다 커버. `GemRacer/5. 반응형 UI 테스트 씬 만들기`로 확인.
- [x] W-04 (9/13 오후) 가로 화면 3D 뷰 비율 조정 — 사실상 "진짜 게임 화면이 생기면 마무리" 하기로 미뤄 둔 항목이었는데, D04(MiningController)·D05(업그레이드 패널)가 각자 다른 안 만들어진 씬(TestPlanet/ResponsiveUITest/UpgradeTest — 셋 다 Unity 에디터가 있어야 부트스트랩이 돌아서 실제로는 하나도 저장된 적이 없었다)에 흩어져 있던 걸 발견했다. `Assets/Editor/BootstrapMainGame.cs`(`GemRacer/7. 메인 게임 씬 만들기`)로 하나로 합침 — 3D 채굴(행성+채굴차+카메라) 위에 Root.uxml HUD와 업그레이드 패널을 얹는다. `Assets/Scripts/UI/MainHud.cs`가 viewport-area에 `.live` 클래스를 붙여 자리 표시자 배경/문구를 지우면 뒤의 실제 카메라가 그대로 보인다(Root.uss에 `.viewport-area.live` 추가) — Root.uxml/Root.uss 자체는 그대로 둬서 ResponsiveUITest 씬은 여전히 자리 표시자를 쓴다. HUD의 "채굴" 버튼은 실제 채굴은 이미 자동이라 할 일이 없어서 "업그레이드" 패널을 여닫는 용도로 재활용, "제작"/"레이스"는 화면이 없어(D08/D09) 비활성화. 이 씬을 Build Settings 0번으로 등록해서 다음 웹 배포부터 시작 화면이 RaceCameraSpike(실험용)에서 이걸로 바뀐다. **덤으로 버그 발견·수정**: `MiningController`가 `SurfaceMover.speed`를 고정값(3)에 묶어 놔서 엔진을 업그레이드해도(코어 `RigSpeed`는 실제로 올라감) 화면상 채굴차는 그대로 느리게 돌고 있었다 — 매 프레임 코어 `RigSpeed`로 덮어쓰게 고침. 화물칸 게이지(Root.uxml에 `cargo-gauge-track`/`-fill` 추가, `MiningController.CargoCapacityMinerals` 신규)도 같이 붙였다 — 단 이건 표시용일 뿐 실시간 채굴 자체를 상한에서 멈추진 않는다(오프라인 캐치업에만 상한 적용 중), 접속 중에도 막을지는 미정이라 아래 "막힌 것"에 남김. **컴파일 확인 완료** (run #20, 9/13 오후): 이 커밋의 webgl 빌드가 실제로 성공했다 — `BootstrapMainGame.cs`/`MainHud.cs`/`MiningController.cs` 변경분 전부 Unity가 실제로 컴파일했다는 뜻. 다만 Build Settings는 커밋된 `EditorBuildSettings.asset`을 CI가 그대로 쓸 뿐이라(내가 손으로 안 건드림), 이 부트스트랩 메뉴를 실제로 눌러 씬을 만들고 커밋하기 전까지는 웹 시작 화면이 여전히 RaceCameraSpike 그대로다 — Play 모드 동작(버튼 눌림·게이지 채워짐 등)도 여전히 눈으로 봐야 한다.
- [ ] W-05 세 기준점 스크린샷을 자동으로 찍어 daily 파일에 붙이는 에디터 스크립트. 매번 눈으로 세 번 확인하지 않게. (9/13 오후: `GameViewSizes` 등 관련 API가 비공개/불확실해서 이번 세션엔 손 안 댐 — Unity 에디터로 실제 확인하면서 짜는 게 나을 것 같다)
- [x] W-06 (9/13 오후) WebGL 첫 로딩 시간 측정. `tools/measure_web_load.js`(신규, 외부 의존성 없음) — 배포된 Build 파일들의 실제 Content-Length를 재서 대역폭 구간별(LTE 약함 3Mbps/보통 8Mbps/좋음 25Mbps) 다운로드 시간을 계산하고 10초 예산과 비교한다. `.github/workflows/webgl.yml`의 "배포 확인" 다음 단계로 넣어서 **이제 매 배포마다 자동으로 잰다**(continue-on-error — 지금은 예산 초과가 빌드를 막진 않음). 이 클라우드 세션 자체는 아웃바운드 네트워크 정책상 `*.pages.dev`에 못 나가서(403) 직접 실행해 확인은 못 했지만, **push 직후 run #20 Actions 로그로 실측 확인 완료**: 실제 배포(`https://51b6c2f6.planetracer-daz.pages.dev`)에서 wasm 8.09MB + data 5.62MB + framework 0.07MB, 합계 **13.78MB**. 대역폭별 다운로드 시간 — **LTE 약함(3Mbps) 36.8초, LTE 보통(8Mbps) 13.8초로 10초 예산 초과, LTE/5G 좋음(25Mbps)만 4.4초로 통과**(9/11 기록으로 미리 해 둔 손계산 38초/14초/4.5초와 거의 일치). 다운로드 시간만 잰 것이라 파싱·초기화까지 더하면 실제 체감은 더 나쁠 것. 예산을 계속 넘기면 에셋을 줄이는 작업이 필요해 별도 항목으로 남김(아래 W-09).
- [ ] W-09 (9/13 오후 신설) 에셋 크기 줄이기. W-06 실측 결과 LTE 약함·보통 구간(국내 LTE 이용자 상당수가 해당할 대역)에서 10초 예산을 이미 넘긴다(wasm 8.09MB + data 5.62MB, 압축 후로 이미 이 정도). Unity WebGL 압축 레벨·텍스처 포맷·Code Stripping(IL2CPP) 옵션부터 볼 것. 급하진 않지만(지금 볼 화면 자체가 아직 적어서 실제 wasm/data가 더 커질 여지도 있다) 화면이 늘어나기 전에 예산을 벌어 두는 게 나을 것
  - (주말 매시간 세션 검토만) `WebGLBuild.cs`를 보니 압축(Brotli)·예외 지원 끔·IL2CPP Master는 이미 되어 있다.
    남은 손잡이는 Managed Stripping Level과 텍스처 포맷인데, 전자는 `SaveService`가 JsonUtility로
    리플렉션 직렬화를 쓰고 있어 레벨을 올렸을 때 필드가 잘려 세이브가 깨질 위험이 있고, 후자는
    브라우저/기기별 압축 텍스처 지원이 갈려서(모바일 실제 지원 포맷이 데스크톱과 다름) 잘못 고르면
    Tifania가 아침에 여는 화면에서 바로 티가 나는 정도의 회귀(텍스처 깨짐)가 될 수 있다 — 둘 다
    Unity 에디터로 켜 보고 확인해야 안전해서 이번 클라우드 세션은 손 안 대고 넘김. Unity 켤 때 먼저
    Managed Stripping Level=Medium으로 시험 빌드 → 세이브/불러오기 되는지 확인부터 하는 게 안전할 듯.
- [x] W-07 (9/12 오전) claude/dev의 webgl 빌드가 D02-N 커밋부터 이틀 연속 실패하고 있던 것을 GitHub Actions 로그로 찾아 고침. `BalanceTable.cs(48,21) error CS0118: 'Planet' is a namespace but is used like a type` — `Assets/Scripts/Planet/`이 네임스페이스를 `GemRacer.Planet`으로 쓰는데 `BalanceTable.cs`가 `using GemRacer.Core;`만 걸어 두고 bare `Planet`을 썼더니, 같은 이름의 형제 네임스페이스가 코어 타입을 가려 버렸다(Core.Tests는 이 네임스페이스가 없는 별도 프로젝트라 안 걸렸다 — 그래서 `dotnet run`은 계속 통과였다). `using CorePlanet = GemRacer.Core.Planet;` 별칭으로 고침. Unity 에디터가 없어 실제 재빌드 확인은 다음 푸시 결과로 봐야 함
- [x] W-08 (9/12 오후) `.github/workflows/webgl.yml`의 "배포 확인" 스텝이 URL 인자를 안 넘겨서 **claude/dev로 push한 날도 항상 main 기준 프로덕션 주소(`check_web_deploy.js` 기본값)만 확인하고 있었던 것**을 발견해 고침. `pages deploy --branch=dev`는 main의 프로덕션 별칭을 안 바꾸니, 지금까지 claude/dev push에서 뜬 "배포 확인 성공"은 사실 이전에 성공했던 main 내용을 다시 확인한 것뿐이었다 — 그날 새로 올라간 dev 프리뷰가 실제로 열리는지는 한 번도 검증된 적이 없었다는 뜻(W-07의 "빌드는 됐지만 확인 안 됨"과는 또 다른, 더 근본적인 구멍). 배포 스텝에 `id: deploy`를 주고 그 출력(`deployment-url`/`pages-deployment-alias-url`)을 `check_web_deploy.js`에 넘기게 고쳤다 — 출력 이름이 실제와 다르면 빈 문자열이 되어 기존 기본값으로 조용히 넘어가니 최소한 하위 호환은 깨지지 않는다. `check_web_deploy.js`에 "대상: URL" 로그 줄도 추가해서, 다음 세션이 이번 push의 Actions 로그에서 실제로 어느 주소를 확인했는지 볼 수 있게 했다. **확인 완료** (run #15, 9/12 오후): `deployment-url` 출력이 실제로 존재했고 `check_web_deploy.js`가
`대상: https://0bbd485c.planetracer-daz.pages.dev`(그날의 새 dev 프리뷰, 프로덕션 주소가 아니다)를
받아 5개 항목 전부 통과했다. 같은 빌드에서 이번 세션이 추가한 `MiningController`/`GameFlowController`/
`GameState`/`MiningRun.cs` 등도 Unity가 실제로 컴파일해 성공했다는 뜻이라(D04-N도 함께 검증됨),
"Unity 에디터 없어 컴파일 확인 못 함" 걱정은 이 커밋들에 한해 해소됐다 — 다만 Play 모드 동작(광맥 앞에
멈추는지, 카운터가 도는지)은 여전히 눈으로 봐야 한다.

## P0 프리프로덕션 (D01–D03)

- [x] D00-N 코어 골격: 모델, MiningSimulator, RaceSimulator, DefaultData, 테스트 러너 9개 통과. (2026-09-11 세션에서 완료)
- [x] D01-N (9/12 토) `Assets/Editor/BootstrapScene.cs`: 메뉴 `GemRacer/1. 테스트 씬 만들기` — 구체 행성(반지름 20) + 표면을 도는 채굴차 큐브 + 카메라 팔로우 + URP 기본 머티리얼 6색(보석 컬러 스크립트). `Assets/Scripts/Planet/SurfaceMover.cs`(표면 법선 따라 이동). 테스트 절차 작성.
- [x] D01-M SurfaceMover 극점 로직 점검(대원 궤도라 극점 안 지남, 방어 코드 있음). Unity에서 실제 확인 완료: 컴파일 OK, 씬 생성 OK, Play에서 반지름 20 표면 유지·한 바퀴 41.9초. 카메라 프레임 독립 보간으로 수정, Run In Background 켬.
- [x] D01.5 레이스 카메라 스파이크: 2D/3D 판단용 실험 씬(GemRacer/3. 레이스 카메라 실험). 3D 확정. 결과는 docs/design/art-and-presentation.md
- [x] D02-N 밸런스 CSV → 코드 파이프라인. `docs/design/balance.csv` 하나 대신 `docs/design/balance/{planets,courses,parts}.csv` 세 개로 나눴다(표마다 열이 달라서 한 파일에 못 담는다). 장비(곡괭이·화물칸 등) 비용 공식은 아직 코어에 없어서 이번엔 뺐다 — D05-N에서 공식이 생기면 그때 추가. 파싱은 코어 `BalanceCsv`(서버·에디터·테스트가 같은 코드로 읽음) + `Assets/Editor/ImportBalance.cs`(`GemRacer/2. 밸런스 CSV 가져오기`, `Assets/Data/Balance.asset`로 저장, 멱등) + `Assets/Scripts/Data/BalanceTable.cs`(ScriptableObject). `DefaultData`는 그대로 폴백 유지, 아직 아무도 Balance.asset을 안 씀(런타임 연결은 실제 소비처가 생기는 D04+ 때).
- [x] D02-M CSV 값과 DefaultData 값 일치 테스트 추가. `Core.Tests`에 3개 추가(행성·코스·부품), `dotnet run` 통과 12 / 실패 0.
- [x] D03-N 세이브: `Core/SaveData.cs`(순수 클래스, 버전·마지막 저장 시각(UTC epoch초, TODO 서버시각 교체 지점 주석)·행성·채굴차·부품·광물) + `MiningRigSave`(MiningRig ↔ 변환) + `Assets/Scripts/Save/SaveService.cs`(JsonUtility, `Application.persistentDataPath`에 임시 파일→교체로 원자적 쓰기, 읽기 실패 시 새 세이브로 폴백). 오프라인 누적 계산은 이미 있는 `MiningSimulator.Offline`를 그대로 쓴다 — D03에서 새로 만들 게 없었다.
- [x] D03-M 세이브 라운드트립 테스트 2개 추가(`Core.Tests`에서 System.Text.Json으로 직렬화 확인 — Unity JsonUtility는 에디터 없이는 못 돌려서 대신 검증, 필드 기반 직렬화라 구조는 같음). `dotnet run` 통과 14 / 실패 0. P0 관문 확인 문서: `docs/design/p0-gate.md`.

## 코어 루프 개정 반영 (docs/design/core-loop.md, P1 항목들보다 먼저 확인할 것)

- [x] L-01 (9/14 새벽) D04(MiningController)를 탐험+발견+선택 채굴 구조로 다시 설계 → 코어 모델 완료: `Core/Exploration.cs`의 `ExplorationSimulator.Discover`(seed 재현 가능, 경과 시간 동안 보물 발견 목록, 광맥은 기존 MiningSimulator가 그대로 자동 산출). MonoBehaviour 배선은 D04-N 몫으로 남김
- [x] L-02 (9/14 새벽) 보물 데이터 모델(등급 C~S, 요구 채굴 도구 등급) 코어에 추가 + 테스트 → `TreasureGrade`/`TreasureDef`(Models.cs) + `DefaultData.QuartzTreasureDefs()` 4종, 테스트 3개
- [x] L-03 (9/14 새벽) 채굴차 부품 슬롯 구성 결정, 레이스 보상 테이블을 채굴차 부품 중심으로 재작성 → 슬롯 5개(Tool/Cargo/Engine/Detector/Refinery)=MiningRig 레벨 필드와 1:1, `RigPartReward`+`RigPartApply`(RigParts.cs), `DefaultData.QuartzLocalRaceRewards()`. docs/decisions.md, docs/design/core-loop.md 갱신
- [x] L-04 (9/12 오후) 오프라인 발견 목록: `Core/Exploration.cs`의 `ExplorationSimulator.DiscoverOffline`이 `MiningSimulator.Offline`(광물)과 `Discover`(보물)를 한 번에 계산해 `OfflineDiscoveries`로 묶는다. 탐험도 화물칸 상한(`Offline.HoursCounted`)만큼만 인정하게 만들었다 — 원래 `Discover`는 상한 없이 elapsedSeconds를 그대로 썼는데, 화물칸이 찬 뒤에도 발견이 계속 쌓이면 광물 쪽과 앞뒤가 안 맞아서 여기서 맞췄다. 테스트 2개 추가(상한 안쪽이면 기존 Discover와 동일 / 상한 넘기면 광물처럼 발견도 잘림). D07-N 오프라인 보상 화면이 이 구조체 하나만 받으면 되도록 설계.
- [x] L-05 (9/12 밤) 봇 시뮬레이션(`Core.Tests/BalanceSim.cs`, `dotnet run -- sim`)으로 확인 — "제일 싼 업그레이드를 산다" 봇 + 30분마다 로컬 레이스 승리(무료 +1 레벨) 가정. D05-N에서 처음 잡은 상수(성장률 1.22~1.35)로는 **쿼츠 Tool/Cargo/Engine 전부가 3.1시간 만에 최대치**에 도달해 버렸다 — 나선이 도는 게 아니라 순식간에 터지는 그림이었다. 성장률을 1.28~1.48로, 기본 비용도 조금 올려서(10~15 → 15~25) 다시 돌리니 8시간으로 늘었고, 뒷부분 구매 간격이 0.05h→1.55h로 완만히 벌어져 체감 효과가 자연스럽게 생겼다. 정확한 목표 시간(하루? 며칠?)은 안 정해서 이 정도가 최종은 아니다 — 구체 수치는 여전히 P4 봇 시뮬레이션에서 재조정. 덤으로 시뮬레이션 도중 **버그 발견**: `RigPartApply.Apply`(레이스 무료 보상)가 슬롯 상한을 안 지켜서 Cargo/Engine이 10을 넘어 12까지 올라가고 있었다 — Models.cs 필드 주석에 있던 상한(Tool 30 / Cargo·Engine 10 / Detector·Refinery 5)을 실제로 클램프하도록 고치고 회귀 테스트 추가.

## P1 코어 루프 프로토타입 (D04–D24, 3주)

- [x] D04-N (9/12 오후) 게임 상태 머신 `GameState`(Mining/Racing/Result, `Assets/Scripts/Game/GameState.cs`) + `GameFlowController`(상태에 따라 다른 컴포넌트를 켜고 끄는 자리, 지금은 MiningController 하나) + `MiningController`: 코어에 새로 만든 `Core/MiningRun.cs`의 `MiningRunState`(이동→광맥 도착→SecondsPerVein만큼 채굴→YieldPerVein 획득, 반복)를 매 프레임 `Advance`시키는 실시간 루프. `SurfaceMover`에 `isMoving` 플래그를 추가해 채굴 단계 동안 채굴차가 광맥 앞에 멈추게 했다(기본값 true라 기존 씬 동작엔 영향 없음). `BootstrapScene.cs`가 테스트 씬에 자동으로 연결. 실제 게이지 UI는 없고 임시 OnGUI 텍스트(원석 누적·이동/채굴 상태)로만 확인 가능 — 진짜 HUD는 D05-N 이후.
- [x] D04-M (9/12 오후) 실시간 산출 ≈ MineralsPerHour 검증 테스트 추가(20시간 적분 결과가 MineralsPerHour×20의 ±5% 안). 추가로 "이동 중엔 원석이 안 나온다", "델타를 잘게 나눠도/한 번에 몰아줘도 누적 결과가 같다"(오프라인 캐치업에서 큰 델타를 써도 안전하다는 뜻) 2개 더. `Core.Tests` 통과 25 / 실패 0.
- [x] D05-N (9/12 밤) 채굴 장비 업그레이드. 코어 `RigUpgrade.cs`(`UpgradeSlot` Tool/Cargo/Engine, `UpgradeCost.Cost`/`Apply`/`AtMax` — 지수 증가, 상한 30/10/10). `Assets/UI/Upgrade.uxml`+`.uss`(세로 540×960 기준, `.landscape`에서 세 줄이 두 칸으로 재배치) + `Assets/Scripts/UI/UpgradePanel.cs`(레벨·다음 효과·비용 표시, 탭으로 업그레이드) + `Assets/Editor/BootstrapUpgradeUI.cs`(`GemRacer/6. 업그레이드 화면 테스트 씬 만들기`). `MiningController`에 `TryUpgrade`/`TrySpendRawMinerals` 추가 — 정제 광물 단계가 아직 없어서 원석(RawMinerals)을 그대로 쓴다(제련 로직이 생기면 바꿀 지점, 코드에 TODO 주석). Unity 에디터 없어 실제 컴파일은 다음 세션 확인 필요.
- [x] D05-M (9/12 밤) `Core.Tests`에 비용 단조 증가·최대 레벨 클램프·슬롯 독립성·실제 산출 개선 테스트 4개 추가. UXML/USS `name`은 `Assets/Scripts/UI/UpgradePanel.cs`의 `Q<>()` 호출과 눈으로 대조 완료(에디터가 없어 실제 바인딩 실행은 못 함).
- [ ] D06-N (9/17 목) 광맥 비주얼: 행성 표면에 광맥 프리팹 N개 배치(부트스트랩), 채굴 중 파티클·흔들림, 화물칸 게이지.
  - (주말 매시간 세션 검토만) `MiningRunState`/`SurfaceMover`를 보니 지금 "광맥"은 순전히 시간 기반
    추상 개념이다 — 채굴차는 표면을 계속 돌다가 `isMoving=false`가 되면 "그 자리"에서 멈출 뿐, 실제
    좌표를 가진 광맥 오브젝트가 하나도 없다. 그래서 이 항목은 단순히 장식 배치가 아니라 "채굴차가
    실제로 광맥 위치를 향해 이동하다 도착해서 멈춘다"는 이동 로직 자체를 건드려야 앞뒤가 맞는다
    (`planet.Circumference / VeinCount` 간격과 실제 배치 간격을 맞춰야 함). 구면 위 각도 계산이라
    실수하면 채굴차가 표면을 벗어나거나 엉뚱하게 도는 등 폰으로 열자마자 티 나는 회귀가 될 수 있어서,
    Unity 에디터로 직접 보면서 하는 게 안전하다고 판단해 이번 세션은 손 안 대고 다음(D07-M)으로 넘어감.
- [ ] D06-M 극점 근처 광맥 배치 균등성 점검.
- [x] D07-N (9/13 밤 매시간 세션) 오프라인 보상 화면. 이번 세션 전까지는 `MiningController`가 세이브를
  아예 안 읽고 안 썼다(매번 레벨 1·원석 0으로 시작 — 작업 도중 발견). 이걸 먼저 고쳤다: `Awake`에서
  `SaveService.Load()`로 채굴차 레벨·원석·행성을 복원하고, 30초마다 + 일시정지/종료 시 `Save()`로
  저장한다(로드한 `SaveData` 객체를 그대로 들고 있다가 이 컨트롤러가 다루는 필드만 갱신 — 나중에
  D08-N이 보유/장착 부품 필드를 쓰기 시작해도 여기서 덮어써서 날리지 않는다). 그 위에 오프라인 보상:
  마지막 저장 시각과 지금 UTC 시각 차를 경과로 보고(30초 미만이면 화면 자체를 안 띄운다 — 에디터에서
  Play 재시작하는 정도로는 안 뜸) 이미 있는 `ExplorationSimulator.DiscoverOffline`(광물+보물)을 그대로
  쓴다. 코어에 작은 조각 2개 추가 — `TreasureDiscovery.MineralValue`(발견 시점에 def 값을 복사해 둬서
  화면이 defs를 다시 안 찾아도 됨), `ExplorationSimulator.MineableValue`(지금 캘 수 있는 보물만 합산,
  D07-N이 실제로 지급하는 값). `Assets/UI/OfflineReward.uxml`+`.uss`(중앙 카드, 세로/가로 구분 없음)
  + `Assets/Scripts/UI/OfflineRewardPanel.cs`(경과·인정·버린 시간, 획득 원석, 발견한 보물 요약, 받기
  버튼 — 보상 없으면 스스로 숨음) — `BootstrapMainGame.cs`(`GemRacer/7`)에 업그레이드 오버레이보다
  더 위(sortingOrder 20)로 얹었다. **알려진 한계**: 안 받은 보상은 세이브 파일이 아니라 메모리에만
  있어서, 화면을 안 보고 앱을 끄면(그 사이 자동 저장이 있었다면) 다음 실행 때 그 보상은 사라진다 —
  지금은 첫 구현이라 범위를 좁혔고, 실제로 문제되면(플레이테스트 피드백 등) SaveData에 pending 필드를
  추가할 것(코드 주석에도 남겨 둠). `Core.Tests`에 3개 추가, **통과 37 / 실패 0**. Unity 에디터 없어
  컴파일 확인은 다음 세션 몫 — 특히 `MiningController`가 이제 `Awake`에서 파일 I/O를 하니 첫 실행(세이브
  파일 없음) 경로를 꼭 봐 줄 것.
- [x] D07-M (주말 세션) 시계 되감기(과거 시각) 시 0 처리 테스트 — 화면(D07-N)보다 먼저 됨. 확인해 보니
  `MiningSimulator.Offline`이 이미 `Math.Max(0, elapsedSeconds)`로 막고 있어서 코드는 손 안 댔고(구현은
  그대로), 그 동작과 `ExplorationSimulator.DiscoverOffline`까지 사슬로 이어지는지를 회귀 테스트로
  고정했다 — 음수 경과, 경계값(정확히 0), 아주 큰 경과(300년치, 오프라인 캐치업 버그로 실제 가능한
  시나리오) 4개 추가. `Core.Tests` 통과 34 / 실패 0. D07-N 화면이 생기면 이 테스트들이 이미 지켜 주는
  범위(음수·0·초대형 델타)는 신경 안 써도 된다.
- [x] D08-N (주말 매시간 세션) 레이싱카 부품 제작 UI. 코어에 `PartCraft.cs` 추가 —
  `PartCraft.Cost(grade)`(지금은 C등급만 정의, DefaultData.PartCostC)·`CanCraft`(중복 제작 방지),
  `PartEquip.TryEquip`/`Unequip`(Part.Slot이 제작 시점에 고정돼 있어서 엉뚱한 슬롯에 못 끼운다 —
  구조적으로 중복 장착이 안 생긴다). `MiningController`에 `OwnedPartIds`·`Car`(RacingCar)·
  `TryCraftPart`/`TryEquipPart`/`UnequipPart` 추가, `Save()`/`Awake()`가 이미 있던
  `SaveData.OwnedPartIds`/`EquippedPartIds` 필드(D03-N 때 미리 만들어 둔 것)를 실제로 읽고 쓴다 —
  6칸 순서는 `SlotOrder`(PartSlot enum 순서)로 고정. `Assets/UI/Crafting.uxml`+`.uss`(쿼츠 C등급
  5종 — 엔진/타이어/서스펜션/차체/부스터 — 한 줄씩, Upgrade.uxml과 같은 반응형 패턴) +
  `Assets/Scripts/UI/CraftingPanel.cs`(버튼 하나가 상태별로 제작/장착/해제를 겸한다). `MainHud.cs`의
  "제작" 버튼을 실제로 연결(그동안 비활성화였다), `BootstrapMainGame.cs`(`GemRacer/7`)에 제작
  오버레이(sortingOrder 11, 업그레이드보다 위·오프라인 보상보다 아래)를 추가로 얹었다. `Core.Tests`에
  6개 추가(제작 성공/중복 방지, 미정의 등급 예외, 미보유 장착 실패, 장착 슬롯 배타성, 슬롯 교체 시
  보유 목록 유지, 해제) — **통과 43 / 실패 0**. Unity 에디터가 없어 컴파일 확인은 다음 세션 몫 —
  특히 `MiningController.LoadParts`/`EquippedIdsInSlotOrder`(Dictionary 순회)와
  `CraftingPanel.cs`의 `UIDocument`/`Button.clicked` 클로저 캡처를 봐 줄 것.
- [x] D08-M (주말 매시간 세션) 제작 비용 차감 테스트 + 세이브 라운드트립 통합 테스트.
  `Core.Tests`에 2개 추가 — ① 비용만큼 정확히 차감되는지·부족하면 값이 안 바뀌는지(부분 차감 없음).
  ② `MiningController.TryCraftPart`/`Save`/`LoadParts`/`EquippedIdsInSlotOrder`가 하는 일(제작→
  장착→SlotOrder로 직렬화→SaveData JSON 왕복→id로 되찾아 슬롯 복원)을 코어 조각만으로 그대로
  재현 — 장착 안 한 부품(보유는 하지만)이 복원 후에도 계속 빈 슬롯인지까지 확인. Assets/Scripts는
  UnityEngine을 참조해서 Core.Tests가 직접 못 불러 재현하는 방식을 택했다 — 테스트 안의 slotOrder
  배열이 `MiningController.cs`의 `SlotOrder`와 반드시 같은 순서여야 한다는 주석을 남겨 뒀다(어긋나면
  이 테스트가 그걸 못 잡는다는 뜻이므로 MiningController.cs를 고칠 때 같이 봐야 함). `dotnet run`
  **통과 45 / 실패 0**.
- [x] D09-N (주말 매시간 세션) 레이스 출전 화면. 코어에 `RaceFuel.cs` 신규 —
  `Recover(currentFuel, baselineUnixSeconds, nowUnixSeconds)`(순수 함수, 시간은 전부 인자로 받는다 —
  CLAUDE.md 1번). 10분(`RecoverySeconds`)마다 1개, 최대 10개. 화물칸 오프라인 캐치업과 같은 정책 —
  이미 꽉 찬 상태에서 흐른 시간은 버린다(캐리 없음), 그래서 기준 시각을 매번 "정확히 회복된 만큼만"
  앞으로 밀거나(잘게 나눠 불러도 결과가 같다) 꽉 찼을 땐 그냥 지금으로 당긴다. `SaveData`에
  `Fuel`(기본값 `RaceFuel.MaxFuel`)·`FuelBaselineUnixSeconds` 필드 추가. `MiningController`가
  `Update`마다 `RecoverFuel()`을 불러 실시간으로 채우고(정수 나눗셈 하나뿐이라 매 프레임 불러도
  싸다), `TryEnterRace(course, out results, out won)`로 연료 1개(`RaceFuel.EntryCost`)를 내고
  `RaceSimulator.Run`을 돌린다 — 상대는 `MakeOpponents`로 5명, 강도는 플레이어 평균 스탯의 90%
  (임시 밸런스, 첫 레이스를 이길 수 있게 — TODO 표시해 둠). 1등이면 그 코스의 `RigPartReward`(L-03)를
  적용한다. `Assets/UI/RaceEntry.uxml`+`.uss`(Crafting.uxml과 같은 반응형 패턴 — 세로 기준,
  `.landscape`에서 두 칸) + `Assets/Scripts/UI/RaceEntryPanel.cs`(목록 뷰 ↔ 결과 뷰 전환, 연료
  게이지·다음 회복까지 남은 시간 표시). `MainHud.cs`의 "레이스" 버튼을 실제로 연결(그동안
  비활성화였다), `BootstrapMainGame.cs`(`GemRacer/7`)에 레이스 오버레이(sortingOrder 12, 제작보다
  위)를 추가로 얹었다. **결과 연출은 아직 없다** — 출전 버튼을 누르면 바로 순위·기록이 뜬다,
  6대가 달리는 연출은 D10-N 몫. Unity 에디터가 없어 컴파일 확인은 다음 세션 몫 — 특히
  `RaceEntryPanel.cs`의 `UIDocument`/`Button.clicked` 클로저 캡처와, `MiningController.TryEnterRace`가
  `List<RaceSimulator.Result>.Find`로 플레이어 결과를 찾는 부분을 봐 줄 것.
- [x] D09-M (주말 매시간 세션) 연료 회복 계산 테스트(경과 시간 기반, 상한) — D09-N과 같은 세션에서
  코어부터 먼저 짬. `Core.Tests`에 7개 추가: 경과 0, 음수(시계 되감기), 정확히 한 주기, 이미
  최대치(오래 기다려도 그대로 + 기준 시각만 당겨짐), 아주 큰 경과(300년치 — 오버플로 없이 최대치),
  음수 연료 방어적 처리, 잘게 나눠 불러도/한 번에 몰아 불러도 결과가 같음(오프라인 채굴 델타
  테스트와 같은 성질). `dotnet run` **통과 52 / 실패 0**.
- [x] D10-N (주말 매시간 세션) 레이스 연출. 코어에 `RaceAnimation.cs` 신규 — `BuildSchedule(results,
  durationSeconds)`(순수 함수)가 이미 정해진 순위(`RaceSimulator.Run` 결과)를 절대 안 바꾸면서
  화면에서 보기 좋게 도착 시각표를 만든다. 실제 기록은 ±3% 지터뿐이라 격차가 거의 없는 경우가
  많아서, 그대로 연출 속도로 쓰면 6대가 거의 동시에 들어와 순위가 안 보인다 — 그래서 격차 비율은
  유지한 채(치열했던 순위는 연출에서도 붙어서, 크게 벌어졌던 순위는 벌어져서) 최소 도착 간격
  (`MinGapSeconds` 0.8초)을 강제하고, 1등은 연출의 55%(`WinnerArrivalRatio`) 지점에서 들어오게
  했다. `Assets/Scripts/UI/RaceEntryPanel.cs`가 출전 버튼 클릭 시 바로 결과를 안 띄우고
  `StartAnimation`으로 전환 — 연출 길이(20~30초)는 `UnityEngine.Random.Range`로 여기(글루
  레이어)에서 뽑아 코어에 인자로 넘긴다(CLAUDE.md 1번). `RaceEntry.uxml`/`.uss`에 `anim-view`
  추가(6줄 진행 막대, cargo-gauge-track/fill과 같은 패턴) + "건너뛰기" 버튼. Unity 에디터가 없어
  컴파일 확인은 다음 세션 몫 — 특히 `Length.Percent` 사용(MainHud.cs의 기존 패턴을 그대로 따름)과
  `UIDocument` 쿼리 부분을 봐 줄 것.
- [x] D10-M (주말 매시간 세션) 연출 도착 순서 = 결과 순위 검증 테스트. `Core.Tests`에 8개 추가:
  실제 접전(지터뿐인 레이스)에서 도착 순서 == 순위 && 엄격히 증가(동시 도착 없음), 입력이 뒤섞여
  있어도 Rank 기준 재정렬, 모든 도착 시각이 (0, duration] 안, 전원 기록이 완전히 같아도(격차 0)
  균등 배분되며 동시 도착 없음, 기록 격차가 극단적으로 커도 순서 유지, 출전자 1명/0명 경계값,
  duration이 Min/MaxDurationSeconds 범위를 벗어나면 방어적으로 잘림. `dotnet run` **통과 60 / 실패 0**.
- [ ] D11-N (주말 매시간 세션, 계속 진행 중) 공구 상자. `Core/LootTable.cs`(확률표+천장)에 이어
  T-07을 "결정 안 나면 A안 기본 진행"으로 매듭짓고, `Core/LootReward.cs`+`Core/LootBoxOpener.cs`
  (확률표 뽑기+천장 카운터 갱신+부품 매핑 조립)까지 끝났다. 이번 세션이 "상자를 실제로 얻는
  경로"를 마저 채웠다 — `Course.Tier`(RaceTier: Local/Circuit/Challenge/GrandPrix) 필드 추가,
  `Core/RaceBoxReward.cs`(등급→상자 매핑, GDD 그대로 로컬=녹슨/서킷=강철/챌린지=티타늄), 쿼츠
  로컬 레이스 3개가 우승 시(`MiningController.TryEnterRace`) 기존 확정 슬롯 보상에 더해 녹슨
  상자를 1개 `_save`에 직접 더하도록 배선(서킷·챌린지 코스는 아직 없어 지금은 로컬=녹슨만 실제로
  나온다), `RaceEntryPanel`의 우승 문구에도 상자 반영. **아직 안 한 것**: 개봉 화면
  (`Assets/UI/LootBox.uxml` 등, 다른 오버레이와 같은 패턴, `LootBoxOpener.Open` 하나만 부르면
  됨 — 이제 `MiningController.RustyBoxCount` 등으로 실제 보유 개수를 읽을 수 있으니 테스트 가능).
  서킷·챌린지 코스 자체(트랙 데이터, 해금 구조)는 `docs/backlog.md` W2 몫으로 남겨 둔다 — 이번
  세션이 침범하지 않았다. Unity 에디터가 없어 이번 세션도 화면 쪽은 손 안 댐 — 다음 Unity
  세션이나 다음 크게 도는 세션이 이어서.
- [x] D11-M (주말 매시간 세션) 확률표 합 1.0 및 10만 회 시뮬레이션 분포 테스트. `Core.Tests`에 6개
  추가 — 세 상자 가중치 합 1.0, seed 재현성, 10만 회 분포가 표와 1%p 안쪽, 천장이 정확히
  pityCount번째에만 확정(그 전엔 확률대로), 천장 없는 상자는 안 확정, 빈 표·가중치 합 0은 예외.
  `dotnet run` **통과 66 / 실패 0**.
- [ ] D12-N (9/23 수) 부품 강화(+10, 실패 없음, 비용 가파름) UI·코어.
- [ ] D12-M 강화 비용 곡선 테스트.
- [ ] D13-N (9/24 목) 튜토리얼 첫 5분: 첫 접속 → 채굴 시작 → 첫 부품 제작 → 첫 레이스까지 안내 말풍선 4개.
- [ ] D13-M 문구 다듬기, 단계 건너뛰기 방지 점검.
- [ ] D14-N (9/25 금) 사운드 자리(엔진·채굴·UI 탭·상자) AudioSource 배선 + 무음 플레이스홀더, 설정 화면(소리·프레임 30/60).
- [ ] D14-M 설정 저장 확인.
- [ ] D15-N (9/26 토) 안드로이드 빌드 준비: Player Settings 체크리스트 문서, 세로 고정, 최소 API, 키스토어 절차. PC 세로 창(540×960, 리사이즈 허용) 설정 스크립트.
- [ ] D15-M 빌드 체크리스트 대조.
- [ ] D16-N (9/27 일) 안정화 1: 아침 피드백 밀린 것 전부 처리.
- [ ] D16-M 테스트 전수 통과 확인.
- [ ] D17-N (9/28 월) 지인 테스트 준비: 게임 안 피드백 버튼(텍스트 → 로컬 파일 저장 → 공유), 세션 로그(접속 시각·플레이 시간) 기록.
- [ ] D17-M 로그 포맷 문서화.
- [ ] D18-N (9/29 화) 리텐션 훅: 화물칸이 다 찼을 때 로컬 알림(Android/iOS Mobile Notifications 패키지), 하루 첫 접속 보상.
- [ ] D18-M 알림 예약 시각 계산 테스트.
- [ ] D19-N (9/30 수) 안정화 2 + 프로토타입 빌드용 태그 `proto-1`. Tifania가 APK 빌드해 지인 5명 배포.
- [ ] D19-M 배포 안내문 작성.
- [ ] D20–D22 (10/1–10/3) 지인 테스트 3일. 야간 세션은 피드백·로그 정리와 버그만. 새 기능 금지.
- [ ] D23-N (10/4 일) 관문 판정 문서: 5명 중 3일 연속 접속 인원, 이탈 지점, 다음 단계(P2 진입 / 루프 재설계) 제안.
- [ ] D24 (10/5 월) 관문 결정. Tifania가 `docs/decisions.md`에 기록.

## 아트·연출 (P1 중 끼워 넣기, docs/design/art-and-presentation.md 참고)

- [x] A-01 보석 행성 6종 지면 타일 텍스처 완료. AI 대신 절차적 생성(tools/gen_planet_texture.py)으로 전환 — 이음새 0, 비용 0. PlanetLook에 6종 다 연결됨
- [x] A-02 행성별 하늘색. 스카이박스 대신 카메라 단색 + 환경광으로 처리(PlanetLook). 저폴리에 더 맞고 행성별로 바꾸기 쉽다
- [ ] A-06 원경 깊이감(안개). URP에서 RenderSettings.fog Linear를 켜면 화면 전체가 안개색이 되어 꺼 둔 상태. URP 방식으로 다시 넣을 것
- [ ] A-03 고스트 카: 코스별 이전 최고 기록 주행을 반투명으로 재생. 성장 체감의 1순위 장치
- [ ] A-04 레이스 결과 화면에 랩타임과 이전 기록 대비 차이 표시
- [ ] A-05 레이스 행성 반지름 결정(채굴 20m / 레이스 60m를 유지할지, 코스를 따로 둘지)

## P2 버티컬 슬라이스 (10/6–11/13) — 주 단위, P1 끝나면 일 단위로 쪼갠다

- [ ] W1 루비·사파이어 행성 파라미터·머티리얼, 행성 선택/워프 흐름, 행성 배지.
- [ ] W2 레이스 4등급(로컬·서킷·챌린지·그랑프리) 해금 구조, 그랑프리 조건(세트 3개), 강철·티타늄 상자.
- [ ] W3 실제 아트 적용(에셋 팩), 채굴차 티어 외형 3종, 부품 아이콘.
- [ ] W4 일일 광맥 1종(월요일 고온), 입장권 초기화(로컬 자정).
- [ ] W5 UI 전체 폴리시, 튜토리얼 10분으로 확장, 데모 뼈대 빌드.

## P3 이후

- P3 온라인·대전·계정 (11/16–12/11), P4 콘텐츠·밸런스 (12/14–1/1), P5 스토어·데모 (1/4–1/22), P6 폴리시·QA·심사 (1/25–2/12). 상세는 기획서 로드맵. P2 중반에 일 단위로 쪼갠다.
