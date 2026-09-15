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
- [x] T-03 수익 모델 A안 확정(9/14). 상세 설계는 `docs/design/monetization.md`. 파는 범위는 시간·편의·꾸미기까지

## 아트 — GPT로 뽑는다 (2026-09-15 신설)

계획은 `docs/design/art-plan.md`, 요청 대기열은 `docs/design/art-requests.md`.
밤 20~08시 이미지 세션이 두 시간마다 한 장씩 뽑는다. PC가 놀고 있을 때만 돈다.

**세션이 할 일은 프롬프트를 쌓는 것이다.** 이미지를 직접 만들려 하지 않는다.

- [x] A-10 (2026-09-16 확인) 1순위 UI 아이콘 18종 전부 `docs/design/art-requests.md` 대기열에
      올라와 있다 — 부품·장비·재화 8종(9/15) + 열쇠·청사진·연료·상자 3종·등급 배지 4종 10종,
      합쳐서 18종 확인. 체크만 빠져 있던 것을 이번 세션이 바로잡음
- [ ] A-11 행성 구체 6종. 행성 선택/워프 화면(W1)이 생길 때
- [ ] A-12 채굴차 3티어. 한 장에 세 대를 나란히 요청해서 통일감을 잡고 잘라 쓴다
- [ ] A-13 컷신 일러스트 — 오프닝, 행성 도착 6장, 첫 레이스 우승. GPT의 힘이 제일 크게 나는 자리
- [ ] A-14 아이콘이 들어오면 `tools/strip_bg.py`로 마젠타 배경을 빼고 Unity 임포트 설정을
      Sprite(2D and UI)로 맞춘다. 이건 Unity 세션이 해야 한다
- [ ] A-15 스토어용(앱 아이콘·배너). 출시가 가까워지면. 지금은 하지 않는다

## UI 이사 — UI Toolkit에서 일반 UI(uGUI)로 (2026-09-15 신설, 최우선)

방법은 `docs/design/ugui-migration.md`에 다 적어 뒀다. 한 세션에 한 화면씩. 위에서부터.
HUD는 이미 끝났고 그게 본보기다(`MainHudUgui.cs` + `BootstrapHudUgui.cs`).

- [x] U-01 (2026-09-15) 튜토리얼 말풍선 uGUI로 옮김. `TutorialUgui.cs` + `BootstrapTutorialUgui.cs`
      (메뉴 `GemRacer/14`). 루트에 Image를 안 붙여서 말풍선 밖 클릭은 아래 HUD로 통과한다 —
      UI Toolkit판의 pickingMode를 대신한다. 에디터 Play에서 "다음" 클릭 시 단계 0→1,
      버튼 자동 잠김까지 확인. **웹에서 처음으로 눌리는 버튼이다**
- [x] U-02 (2026-09-15 야간 코드 + 같은 날 Unity 세션에서 배선·확인) `Assets/Scripts/UI/UpgradeUgui.cs`
      (`UpgradePanel.cs`와 조회·표시 로직 동일, `UiKit.Find`로 이름 조회만 바꿈) +
      `Assets/Editor/BootstrapUpgradeUgui.cs`(메뉴 `GemRacer/16`) 신규. 세 줄(곡괭이/화물칸/엔진)을
      `GridLayoutGroup`(Constraint=Flexible, 셀 400×168, 여백 12)에 담아서 CLAUDE.md 6번 반응형
      규칙을 화면 크기를 직접 읽는 코드 없이 만족시키려 했다 — CanvasScaler(참조 540×960,
      matchWidthOrHeight 0.5) 기준으로 계산하면 세로 폭 ~500에는 카드 하나(412)만 들어가고,
      가로 960×540 폭 ~920·태블릿 1280×800 폭 ~871에는 둘이 들어간다(계산 과정은
      `docs/daily/2026-09-15.md` 이 세션 기록에 남김) — **단 이 계산은 손으로 한 것이라 Editor에서
      세 기준점 다 실제로 봐야 확실하다.** Core는 안 건드려서 `Core.Tests` 그대로 140/실패 0.
      **배선 결과(2026-09-15 Unity 세션)**: `GemRacer/16` 실행 → `MainHudUgui.upgradePanel`에
      `Upgrade` 물림 → 씬 저장. Play로 세 기준점 다 봤고 손계산대로 **1칸/2칸/2칸**이 맞았다
      (목록 폭 500 / 920 / 871). 글자는 전부 Pretendard로 나오고 없는 글리프 0개, 콘솔 에러 0.
      곡괭이/화물칸/엔진 버튼은 정제 광물 0일 때 회색(살 수 없음)으로 정상 동작.
      **여기서 하나 걸렸다 — 닫기 버튼이 없었다.** 이 패널은 화면을 꽉 채우고 뒤로 클릭도 막아서,
      한 번 열면 HUD의 "업그레이드" 버튼까지 가려져 빠져나올 길이 없었다(UI Toolkit판도 같은 구조라
      원래 있던 구멍인데, 버튼이 실제로 눌리게 된 게 지금이라 이제야 드러났다). `BootstrapUpgradeUgui`에
      맨 아래 `close-button`("닫기")을 넣고 `UiPanel.Hide`를 **인스펙터에 보이는 영구 리스너**로 걸었다.
      세 기준점 모두에서 닫기가 맨 위로 잡히고(카드와 안 겹침), 누르면 닫히고 HUD가 다시 잡히는 것까지 확인.
- [x] U-03 부품 제작 화면 uGUI 이사. `Assets/Scripts/UI/CraftingUgui.cs`
      (`CraftingPanel.cs`와 조회·표시 로직 동일, `UiKit.Find`로 이름 조회만 바꿈) +
      `Assets/Editor/BootstrapCraftingUgui.cs`(메뉴 `GemRacer/17`) 신규. 다섯 줄(엔진/타이어/
      서스펜션/차체/부스터)을 U-02와 같은 `GridLayoutGroup`(Constraint=Flexible, 셀 400×190,
      여백 12)에 담았다 — 반응형 계산 자체는 U-02와 같다(셀 폭이 같아서). 한 줄에 담을 게
      업그레이드보다 많아서(이름+상태 한 줄, 강화 단계, 버튼 두 개) 줄 높이를 168→190으로 올렸다.
      **단 세로 화면에서 다섯 줄이 다 화면 안에 들어오는지는 손계산 안 함 — Editor에서 실제로
      봐야 한다.** 넘치면 `row-list`를 `BootstrapArtViewer.cs`처럼 `ScrollRect`로 감싸야 할 수도
      있음(아트 확인 화면이 그 패턴). Core는 안 건드려서 `Core.Tests` 그대로 140/실패 0.
      **남은 것(Unity 세션 몫)**: `GemRacer/17` 실행 → `MainHudUgui.craftPanel`에 생성된
      `Crafting` 오브젝트를 물리기(씬 저장 필요) → Play로 세 기준점(세로 540×960/가로 960×540/
      태블릿 1280×800)에서 1칸/2칸/2칸으로 나오는지, 다섯 줄이 세로 화면에서 잘리지 않는지,
      버튼 두 개(제작·강화)가 각각 눌리는지, 한글이 나오는지 확인.
      **거기에 하나 더 — 닫기 버튼.** U-02를 붙여 보니 화면을 꽉 채우는 패널은 HUD의 여는 버튼까지
      가려서 한 번 열면 빠져나올 길이 없었다(`ugui-migration.md` 3-1번으로 규칙을 박아 뒀다).
      `BootstrapCraftingUgui`에 `close-button`이 없으면 Unity 세션이 U-02와 같은 모양으로 넣는다.
      **배선 결과(2026-09-15 Unity 세션)**: `GemRacer/17` 실행 → `MainHudUgui.craftPanel`에
      `Crafting` 물림 → 씬 저장. 칸 수는 손계산대로 **1칸/2칸/2칸**(목록 폭 500 / 920 / 871,
      U-02와 같은 수치). 없는 글리프 0개, 콘솔 에러·예외 0.
      **걱정하던 대로 세로에서 넘쳤다.** 한 칸일 때 목록이 998px인데 쓸 수 있는 높이가 ~840px이라
      부스터 줄이 잘리고 닫기 버튼은 화면 **밖으로**(y=-182~-138) 밀려나 있었다. 그래서 예고대로
      `row-list`를 `ScrollRect`로 감쌌다(`scroll-view` 추가, 목록은 그 안의 content).
      닫기 버튼은 스크롤 **바깥**에 둬서 세 기준점 모두에서 항상 보인다. 스크롤 폭은 그대로라
      칸 수 계산도 그대로다. 버튼도 실제로 눌러 봤다 — 정제 광물 0일 때 회색, 광물을 주면
      제작 → 장착 → 강화(+1)까지 상태 글자가 따라 바뀌고 비용이 빠졌다(테스트로 바꾼 값은 되돌림).
      닫기 누르면 닫히고 HUD "제작" 버튼으로 다시 열리는 것까지 확인.
- [?] U-04 (2026-09-15 야간) 코드까지 완료, 씬 배선은 Unity 세션 필요. `Assets/Scripts/UI/RaceEntryUgui.cs`
      (`RaceEntryPanel.cs`와 로직 동일, 뷰 전환은 style.display 대신 SetActive, 진행 막대는
      Image.fillAmount) + `Assets/Editor/BootstrapRaceEntryUgui.cs`(메뉴 `GemRacer/18`) 신규.
      셋 중 제일 복잡했다 — entry-view(코스 3개, U-02·U-03과 같은 GridLayoutGroup Flexible)/
      anim-view(6대 도착 연출)/result-view(순위 6줄+보상) 세 개가 같은 자리를 차지하는 자식
      GameObject라 하나만 SetActive로 켠다.
      **닫기 버튼 구멍 하나 더 찾았다.** entry-view도 result-view처럼 화면을 꽉 채우고 뒤 클릭을
      막는데, 원래 UI Toolkit판 UXML(RaceEntry.uxml)에도 entry-view엔 닫기 버튼이 없었다 —
      U-02에서 발견된 구멍(ugui-migration.md 3-1번)과 같은 자리다. `entry-close-button`을 새로
      넣어서 막았다(`entry-close-button`은 패널을 완전히 닫고, 기존 `close-button`은 원래
      로직 그대로 result-view에서 entry-view로 돌아가는 버튼 — 이름이 겹치면 `UiKit.Find`가
      먼저 찾은 쪽만 집으므로 둘을 구분해 이름 붙였다).
      Core는 안 건드려서 `Core.Tests` 그대로 140/실패 0.
      **남은 것(Unity 세션 몫)**: `GemRacer/18` 실행 → `MainHudUgui.racePanel`에 생성된
      `RaceEntry` 오브젝트를 물리기(씬 저장 필요) → Play로 세 기준점에서 코스 3개가 1칸/2칸/2칸,
      출전 → 연출 6줄 채워짐 → 결과 화면 순서로 넘어가는지, 두 닫기 버튼이 각각 제 역할을
      하는지, 한글이 나오는지 확인.
- [?] U-05 (2026-09-16 야간) 코드까지 완료, 씬 배선은 Unity 세션 필요. `Assets/Scripts/UI/LootBoxUgui.cs`
      (`LootBoxPanel.cs`와 조회·표시 로직 동일, `UiKit.Find`로 이름 조회만 바꿈) +
      `Assets/Editor/BootstrapLootBoxUgui.cs`(메뉴 `GemRacer/19`) 신규. 세 줄(녹슨/강철/티타늄)을
      U-02·U-03과 같은 `GridLayoutGroup`(Constraint=Flexible, 셀 400×140, 여백 12)에 담았다 —
      한 줄에 이름+보유 개수+열기 버튼뿐이라 업그레이드 화면(레벨+효과 두 줄, 168px)보다 내용이
      적어서 셀 높이를 140으로 낮게 잡았다. 결과 카드(`result-card`/`result-label`, 줄바꿈 켬)를
      목록 아래 고정 72px로 추가 — LootBoxPanel.cs와 같은 자리. **손계산상 세로 한 칸일 때
      목록+결과 카드 높이가 업그레이드(넘치지 않았음)보다 작아 보이지만, U-03(부품 제작)이
      손계산으로는 괜찮아 보였다가 실제로는 넘쳤던 전례가 있어 확신할 수 없다 — Editor에서
      세 기준점 다 실제로 봐야 한다.** 닫기 버튼은 처음부터 넣었다(ugui-migration.md 3-1번 —
      U-02/U-03/U-04에서 반복 발견된 구멍을 이번엔 선제적으로 막음). `MainHudUgui.boxPanel` 필드는
      이미 있어서(`btn-box` 배선도 이미 있음) 코드 쪽엔 손댈 곳이 없었다 — 씬에서 `LootBox`
      오브젝트를 물리기만 하면 된다. Core는 안 건드려서 `Core.Tests` 그대로(마지막 확인 133/실패 0).
      A-10도 같이 확인 — 상자 3종 아이콘(`icon-box-rusty/steel/titanium`)이 이미
      `docs/design/art-requests.md` 대기열에 올라와 있어서 이번엔 새로 추가할 것 없음.
      **남은 것(Unity 세션 몫)**: `GemRacer/19` 실행 → `MainHudUgui.boxPanel`에 생성된 `LootBox`
      오브젝트를 물리기(씬 저장 필요) → Play로 세 기준점(세로 540×960/가로 960×540/태블릿
      1280×800)에서 1칸/2칸/2칸으로 나오는지, 세로 화면에서 결과 카드까지 포함해 잘리지 않는지
      (넘치면 U-03처럼 `row-list`를 `ScrollRect`로 감싸야 할 수도 있음), 상자 보유 0개일 때
      버튼이 회색인지, 상자를 실제로 열면 결과 문구가 뜨는지, 한글이 나오는지, 닫기 버튼이
      HUD "상자" 버튼으로 다시 열리게 하는지 확인.
- [?] U-06 (2026-09-16 야간) 코드까지 완료, 씬 배선은 Unity 세션 필요. `Assets/Scripts/UI/SettingsUgui.cs`
      (`SettingsPanel.cs`와 조회·표시 로직 동일, `UiKit.Find`로 이름 조회만 바꿈) +
      `Assets/Editor/BootstrapSettingsUgui.cs`(메뉴 `GemRacer/20`) 신규. 다른 오버레이 화면(U-02~U-05)과
      달리 줄마다 내용이 달라서(소리 켜짐/꺼짐 한 줄, 프레임 두 버튼 한 줄, 피드백은 여러 줄
      입력칸) `GridLayoutGroup`으로 카드를 맞추지 않고 `VerticalLayoutGroup`으로 세 줄을 그냥
      쌓았다 — 손계산상 세로 한 칸 기준 내용 높이가 400대 초반이라 U-03이 실제로 넘쳤던
      840대 가용 높이에 한참 못 미쳐서 이번엔 `ScrollRect` 없이 시작했다(그래도 Editor 확인 필요).
      `EnableInClassList("selected", ...)`는 ugui-migration.md 변환표대로 프레임 버튼의
      `Image.color`를 직접 바꾸는 것으로 옮겼다. 닫기 버튼은 처음부터 넣었다(ugui-migration.md
      3-1번). `MainHudUgui.settingsPanel` 필드는 이미 있어서(`btn-settings` 배선도 이미 있음)
      코드 쪽엔 손댈 곳이 없었다. 피드백 입력칸은 이 프로젝트에서 처음 쓰는 `TMP_InputField`라
      `textViewport`/`textComponent`/`placeholder`를 손으로 구성했다 — **Editor에서 실제로
      글자가 입력되고 여러 줄로 늘어나는지부터 확인할 것.** Core는 안 건드려서 `Core.Tests` 그대로
      (이번 세션 확인 140/실패 0).
      **남은 것(Unity 세션 몫)**: `GemRacer/20` 실행 → `MainHudUgui.settingsPanel`에 생성된
      `Settings` 오브젝트를 물리기(씬 저장 필요) → Play로 세 기준점에서 세 줄이 잘리지 않는지,
      소리 버튼이 켜짐/꺼짐을 토글하는지, 프레임 30/60 버튼이 눌리고 선택된 쪽 색이 바뀌는지,
      피드백 입력칸에 여러 줄을 적고 저장하면 문구가 바뀌고 칸이 비는지, 닫기 버튼이 HUD "설정"
      버튼으로 다시 열리게 하는지, 한글이 나오는지 확인.
- [ ] U-07 오프라인 보상 (`OfflineReward.uxml` → `OfflineRewardUgui`)
- [ ] U-10 상점 (`Shop.uxml` → `ShopUgui`). M-07이 2026-09-15 밤에 UI Toolkit으로 만든 화면이다.
      **이사 결정 전에 들어온 것이라 같이 옮긴다.** 앞으로 새 화면은 처음부터 uGUI로 만든다 —
      UI Toolkit으로 새로 만들면 옮길 것만 늘어난다
- [ ] U-08 일곱 개가 다 끝나면 — MainGame 씬에서 꺼 둔 UI Toolkit 루트 여덟 개를 지우고,
      옛 패널 스크립트·UXML·USS·PanelSettings·테마를 지운다. 그 전에는 지우지 않는다
- [x] U-09 (2026-09-15 확인) 이사 후 웹 빌드에서 **스택 오버플로가 사라졌다.** HUD만 옮긴 상태에서도
      깨끗하다 — UIDocument 여덟 개가 원인이었다는 가설이 맞았다. 콘솔 에러 0

## 수익화 구현 (2026-09-14 신설, 설계는 docs/design/monetization.md)

먼저 읽을 것: 원칙은 "시간은 팔고 힘은 팔지 않는다". 부품·상자·열쇠·청사진·연료·대전권은 어떤 경로로도 팔지 않는다.

- [x] N-01 노션 현황판 만들기(9/14). https://app.notion.com/p/3db7777608f8817b9f75ffc12200266f — Tifania가 매일 여는 페이지. 갱신 규칙은 CLAUDE.md "노션 현황판"
- [x] M-01 (9/14 야간) 화물칸 상한을 접속 중에도 적용. `Planet.BaseCargoHours` 필드 신규(쿼츠·루비 4h,
  사파이어·아쿠아마린 5h, 주사·라피스 6h — DefaultData에 반영). `MiningSimulator.CargoHours(rig, planet)`가
  이 행성 기본값에 CargoLevel 배율(1레벨 ×1 ~ 10레벨 ×3, 옛 수식과 쿼츠 기준으로 같은 4h~12h)을
  곱하도록 시그니처 변경 — 호출부 전부(`Offline`, `UpgradePanel.cs`, `Core.Tests`) 같이 고침.
  `CargoCapacityMinerals(rig, planet)`(원석 단위 상한)와 `ClampToCargoCapacity(raw, rig, planet)`
  신규 — `MiningController.Update()`가 매 프레임 이 함수로 `RawMinerals`를 잘라서 접속 중에도
  실제로 채굴이 멈춘다(연출은 그대로 돎, 상한 도달 화면은 M-04 몫). `decisions.md` T-06을
  A안(온라인에도 적용)으로 해결 처리. `Core.Tests`에 4개 추가 — 행성별 기본값, CargoLevel 배율이
  상한(원석)에도 그대로 곱해지는지, 온라인 클램프가 상한 아래/경계/초과에서 각각 맞게 동작하는지,
  `MiningRunState.Advance`를 여러 틱 몰아 굴려도 상한을 못 넘는지. `dotnet run` **통과 86 / 실패 0**.
  Unity 에디터가 없어 컴파일 확인은 다음 세션 몫 — `MiningController.cs`/`UpgradePanel.cs`의
  시그니처 변경분(둘 다 `Planet` 인자 추가)이 실제로 컴파일되는지 봐 줄 것.
- [x] M-02 (9/14 야간) 정제 광물을 화물칸과 분리해 실제 화폐로 만듦. 코어 `MiningSimulator`에
  `RefinePerHour(rig, planet)`(제련소 0~5레벨, 0레벨은 0, 5레벨은 원석 산출과 정확히 같아서
  그 이상은 화물칸이 사실상 다시는 안 참) + `Refine(rawMinerals, rig, planet, deltaSeconds)`
  (온라인 프레임 틱용) 신규. `Offline()`을 원석 유입 속도 R·정제 속도 F·화물칸 상한 Cap이
  경과 시간 동안 상수라는 점을 이용한 닫힌 형태 계산으로 다시 써서 `RefinedGained` 필드 추가 —
  수백 년짜리 오프라인도 프레임으로 안 쪼개고 한 번에 푼다(레벨 0은 기존 계산과 정확히 같은
  값이 나오는 걸로 회귀 확인). `MineralsPerHour` 주석이 예전에 "정제 광물"이라 잘못 적혀 있던
  것도 바로잡음(실제로는 원석). `MiningController`: `RefinedMinerals` 프로퍼티 신규, `Update()`가
  매 프레임 정제를 먼저 뗀 뒤 화물칸 클램프, `TrySpendRawMinerals` → `TrySpendRefinedMinerals`로
  바꿔 업그레이드·제작·강화 전부 정제 광물로 내게 함(코어 쪽 비용 함수 주석엔 이미 "정제 광물"
  이라 적혀 있었는데 실제 소비는 원석이었던 불일치를 해소), `ClaimOfflineReward`가 원석/정제
  광물/보물 환산치(정제 쪽으로 감, TreasureDef 주석대로)를 나눠서 지급 + 원석 쪽 재클램프(기존
  잠재 버그 — 온라인에 남아 있던 원석과 오프라인 보상 원석을 그냥 더하기만 해서 합이 상한을
  넘을 수 있었음). `UpgradePanel.cs`/`CraftingPanel.cs`가 `RefinedMinerals` 기준으로 표시·활성화,
  `MainHud.cs`는 한 줄에 원석·정제 광물을 같이 표시(새 UXML 라벨 없이), `OfflineRewardPanel.cs`/
  `OfflineReward.uxml`에 정제 광물 획득 줄 신규. `Core.Tests`에 3개 추가(제련소 레벨 클램프·단조
  증가, `Refine`의 원석 초과 방지, 레벨이 오르면 오프라인 상한 도달이 늦춰지고 5레벨은 아예 안
  닿는지) — `dotnet run` **통과 89 / 실패 0**. 알려진 근사(코드 주석에도 남김): 보물·희귀 광맥
  발견은 여전히 예전처럼 "화물칸 찬 뒤로는 안 는다"는 캡 시간만 인정하는데, 정제소가 생긴 지금은
  채굴 자체가 안 멈추는 게 더 정확한 모델이라 다음에 손볼 여지가 있음. Unity 에디터가 없어 컴파일
  확인은 다음 세션 몫 — UI 4개 파일과 `OfflineReward.uxml` 변경분이 실제로 붙는지 봐 줄 것.
- [x] M-03 (2026-09-14 밤) 첫 상한 도달까지 최소 90분 보장 — 봇 시뮬레이션(`Core.Tests`,
  `MiningRunState.Advance`를 1초 틱으로 실제로 돌림)으로 실측하니 쿼츠 첫 세션(전부 기본
  레벨, Detector/Refinery 0)은 **240.4분**에 상한에 닿는다. 90분보다 훨씬 여유 있어 쿼츠
  `BaseCargoHours`는 그대로 둔다 — RefineryLevel이 0이면 `Offline()`의 `hoursToCap`이 채굴
  속도(MineralsPerHour)와 무관하게 정확히 `CargoHours(rig,planet)`(= 여기선 BaseCargoHours ×
  1레벨 배율)이 되는 게 공식상 이유(속도 항이 분자·분모에서 상쇄됨). 이 관문을 회귀 테스트로
  고정해 뒀다 — 나중에 쿼츠 `BaseCargoHours`나 CargoLevel 배율을 낮추면 이 테스트가 먼저 걸린다.
- [x] M-04 (9/14 야간) 상한 도달 UI. `MiningController`에 `CargoJustFilled`(엣지 트리거 — 상한에
  막 닿은 프레임에만 켜짐) + `AcknowledgeCargoFull()` 신규. `CargoFullPanel.cs`(OfflineRewardPanel과
  같은 패턴 — 조건에 따라 스스로 접혔다 펴진다) + `CargoFull.uxml`/`.uss`(OfflineReward와 같은 톤의
  가운데 카드) 신규 — "정제로 돌리시겠어요?"로 시작하고 "멈췄습니다"는 안 쓴다. 버튼은 "레이스
  나가기"(1순위, 레이스 출전 패널을 연다) / "닫기" 둘뿐이다 — **"상점은 그다음"의 상점 다리는
  아직 없다**: 이 게임에서 화물칸의 진짜 무료 해법은 제련소인데, `UpgradeSlot`에 `Refinery`가
  없어서(RigUpgrade.cs 주석: "정제 기능 자체가 미구현" — 이제 미구현이 아닌데 이 주석이 안
  고쳐진 채 남아 있었다) 돈으로 사는 게 아니라 레이스 승리로 얻는 부품이다(docs/design/monetization.md
  "자동 제련소(레이스 보상)"). 그래서 1순위 버튼이 상점이 아니라 레이스다. 상점 화면 자체(M-07)가
  아직 없으니 그 버튼은 M-07이 붙을 때 같이 넣는다. `BootstrapMainGame.cs`에 GemRacer/7 메인
  게임 씬 생성 코드 추가(소트 오더 19, 오프라인 보상 20보다 한 단계 아래). 순수 UI/Mono 로직이라
  core 변경은 없음 — `Core.Tests` 새 항목 없음, 기존 90/실패 0 그대로 통과 확인.
  Unity 에디터가 없어 컴파일·실제 동작 확인은 다음 세션 몫 — 아침 가이드에 확인 포인트 남김.
  **→ 9/15 19:20 Unity 세션에서 컴파일 에러 0 확인 + `GemRacer/7` 실행으로 씬 반영 완료.**
  화물칸 가득 참 오버레이는 UI Toolkit판이라 꺼진 채로 들어갔다(U-10에서 uGUI로 옮길 때 켠다).
- [ ] M-05 화물칸 80% 푸시 알림. 재접속을 만드는 장치라 상품 하나보다 매출 기여가 크다. 로컬 알림으로 먼저 구현(서버 푸시는 P3).
  **(9/15 새벽 진행 중)** "언제 알릴지" 계산은 core로 끝냈다 — `MiningSimulator.HoursUntilCargoThreshold(rig,
  planet, currentRawMinerals, thresholdFraction)` 신규(Offline()과 같은 R/F 모델 재사용, 이미 도달했으면
  0, 제련소가 유입을 따라잡아 영원히 안 닿으면 null). `Core.Tests` 4개 추가(이미 도달/80%=100%의
  0.8배·선형/제련소 5레벨 null/경계값), **통과 94 / 실패 0**. **남은 것(에디터 있는 세션 몫)**: Unity
  Mobile Notifications 패키지를 `Packages/manifest.json`에 추가(이 클라우드 세션은 Package Manager로
  실제 resolve·컴파일 확인을 할 수 없어 손 안 댐 — 잘못 건드리면 지난 URP 셰이더 사고처럼 빌드가
  통째로 죽을 위험), `OnApplicationPause(true)`에서 이 함수로 예약 시각을 구해 실제 알림 API 호출,
  포그라운드 복귀 시 예약 취소. 하루 첫 접속 보상(같은 D18-N 범위)은 아직 안 건드림.
- [x] M-06 (9/15 새벽) 코어 `Entitlements.cs` 신규 — `PurchaseState`(영구 구매는 레벨/bool,
  기간제는 만료 시각 `long?`, 시간은 인자로만 받는다)를 `Entitlements.Effective(state, nowUnixSeconds)`
  하나로 계산한다. 화물칸 확장(0~3단계, ×1~×3)과 구독(×1.5)이 겹치면 monetization.md 2-5 "더 큰
  값 적용, 중복 차감 없음"대로 `Math.Max`(곱하지 않음) — 3단계 구매자가 구독까지 하면 ×4.5가 아니라
  ×3이어야 한다는 게 이 항목의 핵심 테스트. Steam 서포터 팩은 만료 없는 구독과 동일 취급, 광고
  제거는 개별 구매·구독 OR, 오프라인 상한 연장(4h→12h)은 구독과 무관하게 그 구매 하나로만 결정
  (구독 혜택 목록에 없어서 분리함). 채굴 가속 패스(×2 산출)는 화물칸·구독과 완전히 독립.
  "매일 정제 광물 지급"은 하루 한 번이라는 청구 타이밍이 상태값(마지막 지급 날짜)이 필요해
  순수 계산엔 안 맞아서 `DailyRefinedMineralsGrant`(구독 여부만) 플래그로만 남기고 실제 청구
  로직은 안 건드림(코드 TODO). `OfflineCapHours`/`BonusFuelCapacity`도 계산만 있고 실제
  `MiningController`/`RaceFuel` 배선은 아직 없음(TODO 주석) — 구매 UI(M-07 상점)가 없으니
  당장 연결할 곳이 없다, 다음은 그쪽. `Core.Tests`에 9개 추가(기본값 전부 꺼짐, 화물칸 단계별
  배율·범위 밖 클램프, 구독 켜짐/만료 경계값, 화물칸 중복 방지 양방향, Steam 영구 취급, 광고
  제거 OR, 오프라인 연장 구독 무관, 가속 패스 독립) — **통과 103 / 실패 0**. Unity 에디터가
  없어 컴파일 확인은 다음 세션 몫 — UnityEngine 참조 없는 순수 C#이라 위험은 낮음.
- [x] M-07 상점 화면(UI Toolkit으로 만들어짐 — U-10에서 uGUI로 옮긴다). 스타터 팩·화물칸 확장 3단계·오프라인 연장·가속 패스·구독·스킨. 가격은 CSV에서 읽는다.
  **(9/15 새벽 진행 중, 2세션째)** core 뼈대(1세션째) + 화면·배선(2세션째)까지 끝냈다.
  1세션째: `ShopSkuId`/`ShopItem`(스킨·시즌 패스는 종류가 안 정해져서 빠짐), `BalanceCsv.ParseShopItems`
  + `docs/design/balance/shop.csv`(가격표, DefaultData.ShopItems()와 값 일치를 Core.Tests가 검사),
  `ShopPurchase.Apply(state, skuId, now)`, `SaveData` PurchaseState 저장 6필드.
  2세션째: `Assets/UI/Shop.uxml`/`.uss`(LootBoxPanel과 같은 고정 아홉 줄 구조) + `ShopPanel.cs`
  신규 — DefaultData.ShopItems() 순서로 이름·가격을 채우고, 버튼을 누르면 `MiningController.
  DebugPurchase(skuId)`(신규, 실제 결제 SDK 전이라 바로 ShopPurchase.Apply 적용)를 부른다.
  `MiningController`에 `_purchases`(PurchaseState, 세이브 왕복) + `Entitlements`/`Purchases`
  프로퍼티 추가, `CargoCapacityMinerals`에 `Entitlements.CargoMultiplier`를 곱해서 화물칸 확장·구독이
  실제로 HUD 게이지·온라인 클램프·오프라인 보상 전부에 먹게 했다(코어 `ClampToCargoCapacity`는
  이 배율을 몰라서 세 곳 다 `Mathf.Min(.., CargoCapacityMinerals)`로 직접 자르는 방식으로 바꿈).
  `Entitlements.MiningYieldMultiplier`(가속 패스)는 접속 중 원석 산출에만 곱함(오프라인은 아직,
  core `MiningSimulator.Offline` 시그니처를 같이 바꿔야 해서 다음으로 미룸). HUD(`Root.uxml`)에
  여섯 번째 "상점" 버튼 + `MainHud.cs` 배선(다른 다섯 버튼과 같은 패턴), `CargoFullPanel`에도
  "상점 보기" 보조 버튼 추가(레이스 나가기가 여전히 1순위). `BootstrapMainGame.cs`에 상점
  UIDocument 생성 코드 추가(소트 오더 21, 상점이 항상 맨 위). Core 파일은 하나도 안 건드려서
  `Core.Tests` 그대로 109/실패 0.
  **남은 것**: `Entitlements.OfflineCapHours`는 M-01 이후 의미가 애매해져서 판단 대기로 넘김
  (`docs/decisions.md` 2026-09-15 항목), `BonusFuelCapacity`(RaceFuel 시그니처 변경 필요)·
  `AutoRefineryAlwaysOn`·`DailyRefinedMineralsGrant`는 아직 미배선 — 전부 decisions.md에 정리해 둠.
  M-08(스타터 팩 노출 로직)이 이 상점 화면을 전제로 하니 다음 순서로 자연스럽다.
- [x] M-08 (9/15 새벽) 스타터 팩 노출 로직. core `StarterPackOffer.ShouldShow(hasReachedCargoCapBefore,
  declined, cargoExpansionLevel)` 신규 — 셋 다 맞을 때만 true(상한에 한 번이라도 닿았고, 거절한 적
  없고, 화물칸 확장을 아직 아무 경로로도 안 가짐). `SaveData`에 `HasReachedCargoCapBefore`(엣지
  트리거인 `CargoJustFilled`와 달리 영구 보존)·`StarterPackOfferDeclined` 두 필드 추가.
  `MiningController.ShouldShowStarterPackOffer`/`DeclineStarterPackOffer()` 배선, `CargoFullPanel`에
  "스타터 팩" 강조 칸 추가(`CargoFull.uxml`/`.uss`) — 첫 상한 도달 화면 안에서 "스타터 팩 보기"
  (상점 열기)·"괜찮아요"(거절, 이 칸만 접힘) 두 버튼. 이름·가격은 `DefaultData.ShopItems()[0]`에서
  읽어 하드코딩 안 함. `Core.Tests` 5개 추가(안 닿음/셋 다 맞음/거절함/이미 보유/음수 경계) —
  **통과 114 / 실패 0**. Unity 에디터가 없어 컴파일·UXML 바인딩 확인은 다음 세션 몫 —
  `CargoFullPanel.cs`에 `GemRacer.Core` using 추가했으니 특히 확인. `GemRacer/7` 씬 반영도 필요.
  **→ 9/15 19:20 Unity 세션: 컴파일 에러 0(`CargoFullPanel.cs` 포함), `GemRacer/7` 실행으로
  씬 반영 완료. M-07 상점 오버레이도 같은 실행에서 씬에 들어왔다.** 그 과정에서 `GemRacer/7`이
  uGUI로 옮긴 것(HUD·튜토리얼·아트 확인 화면·`?fast=` 배속)을 지워 버리는 문제를 먼저 고쳤다 —
  자세한 건 `docs/daily/2026-09-15.md` 19:20 세션.
- [x] M-09 (9/15 야간, 두 세션) 보상형 광고 자리 4곳(오프라인 2배 3회 / 상자 1개 더 3회 / 상한 2배
  1시간 2회 / 연료 +3 2회). 하루 한도 카운터는 코어에. SDK 연동은 P3, 자리와 카운터·지급 로직까지
  끝. `RewardAdBoost.CargoCapMultiplier`/`ExtendCargoCapDoubleHour`로 "상한 2배 1시간"의 만료
  시각을 이어 붙이고(선결제 손해 없음), `MiningController.WatchAdForExtraLootBox`/
  `WatchAdForCargoCapDouble`/`WatchAdForFuelRefill`/`ClaimOfflineRewardDoubled`로 네 자리 다 실제
  지급까지 배선, 화면 네 곳(오프라인 보상/화물칸 가득 참/레이스 출전·결과)에 버튼도 붙었다.
  `Core.Tests` **통과 123 / 실패 0**. 아래는 진행 중이던 첫 세션(카운터만) 기록.
  **(9/15 야간 진행 중)** 하루 한도 카운터까지 끝났다. core `RewardAd.cs` 신규 — `RewardAdSlot`
  4종(OfflineRewardDouble/ExtraLootBox/CargoCapDoubleHour/FuelRefill) + `RewardAdState`(자리별
  오늘 시청 횟수 + 마지막 리셋 날짜) + `RewardAdTracker`(`DayIndex`/`CanWatch`/`RemainingToday`/
  `RecordWatch`). "하루"의 경계는 UTC 자정이 아니라 `timeZoneOffsetSeconds`만큼 민 자정 — 시간은
  전부 인자로 받는다(CLAUDE.md 1번)는 원칙을 시간대까지 포함해서 지켰다. `SaveData`에 필드 5개
  (`RewardAdLastResetDayIndex` + 자리별 카운트 4개) + `ToRewardAdState()`/`ApplyRewardAdState()`
  추가(전부 non-nullable이라 M-06 PurchaseState처럼 0↔null 변환은 필요 없지만, 화면이 세이브 필드를
  직접 안 만지게 하려고 같은 패턴을 맞췄다). `MiningController`에 `_rewardAds` 필드(로드·세이브
  왕복) + `CanWatchRewardAd`/`RemainingRewardAdsToday`/`RecordRewardAdWatched` 세 메서드 — 시간대
  오프셋은 `KstOffsetSeconds`(9시간) 상수로 고정(TimeZoneInfo로 기기 시간대를 읽는 방법도 있지만
  WebGL에서 IANA 시간대 DB 가용성이 플랫폼마다 갈려 위험, 이 게임은 한국 유저 기준이라 고정값으로
  충분). `Core.Tests`에 8개 추가(초기 상태 한도, 한 자리만 올라가고 다른 자리는 그대로, 한도 초과
  시 카운트 안 넘음, 날짜 바뀌면 전부 리셋, 같은 KST 하루 안에서는 UTC 날짜가 갈려도 리셋 안 됨,
  서쪽 시간대 UTC 자정 넘나듦, `DayIndex`가 로컬 시각 음수(floor 나눗셈 경계)에서도 맞는지 —
  **통과 121 / 실패 0**. **남은 것**: `RecordRewardAdWatched`는 카운터만 올린다 — 실제 보상(2배
  지급/상자 1개 더/상한 1시간 2배/연료 +3)을 각 화면이 어떻게 적용할지는 아직 안 정함(연료·상자는
  단순 가산이라 쉽지만, "1시간 동안 상한 2배"는 만료 시각을 어딘가에 들고 있어야 해서 Entitlements
  패턴처럼 상태값이 하나 더 필요할 수 있다). 네 화면에 실제 버튼을 놓는 UI(자리) 자체도 아직 없다 —
  오프라인 보상·레이스 결과·화물칸 가득 참·연료 부족 화면이 전부 이미 있으니(D07-N/D09-N/M-04)
  버튼 하나씩 추가하는 정도면 될 것. Unity 에디터가 없어 `MiningController.cs` 컴파일 확인은 다음
  세션 몫 — 새 core 파일(`RewardAd.cs`)에 `.meta`도 아직 없다(에디터가 여는 다음 세션에서 생기는
  대로 커밋).
- [x] M-10 (9/15 야간) 시즌 패스 데이터 구조. core `SeasonPass.cs` 신규 — `SeasonPassTier`(레벨·
  필요 누적 XP·무료/유료 보상), `SeasonPassState`(XP·유료 트랙 보유 여부·수령 완료 비트마스크
  두 개), `SeasonPassProgress`(`LevelForXp`/`CanClaim`/`Claim`/`AddXp`, 전부 순수 함수). 이름이
  기존 "행성 통행증 구독"(Entitlements.PurchaseState.SeasonPassSubscription...)과 겹쳐 보이는데
  다른 상품이다 — monetization.md 2-5(월 구독, 이미 M-06에서 구현됨)와 2-6(4주 배틀패스, 이번
  항목)은 기획 문서가 먼저 같은 이름을 썼을 뿐, 타입 이름으로만 구분해 뒀다(파일 상단 주석에
  적음). `DefaultData.SeasonPassTiers()`에 10티어 초안 — 무료 트랙엔 채굴차 부품(RigPart)·공구
  상자·소량 원석만(힘), 유료 트랙엔 정제 광물·화물칸 임시 확장 시간·스킨 id만(꾸미기·시간
  단축) 넣어서 monetization.md "절대 팔지 않는 것"을 코드로도 강제했다(테스트로 이 규칙 자체를
  검증 — 유료 트랙에 RigPart/LootBox가 섞이면 실패). XP 필요량은 레벨×100(등차)으로 우선
  잡음 — 4주 시즌 길이·실제 XP 획득원(레이스 승리 등)은 아직 안 정해서 P4 봇 시뮬레이션에서
  재조정 필요(코드 주석에 남김). `SaveData`에 필드 4개(`SeasonPassXp`/`SeasonPassOwnsPaidTrack`/
  `SeasonPassClaimedFreeTierMask`/`SeasonPassClaimedPaidTierMask`) + `ToSeasonPassState()`/
  `ApplySeasonPassState()`. `Core.Tests`에 10개 추가(레벨 경계값 3종, 레벨 미도달 시 무료/유료 둘 다
  막힘, 무료는 유료 미보유와 무관하게 받을 수 있음, 중복 수령 방지, 방어적 이중 확인, 비트마스크
  레벨별 독립, 범위 밖 레벨, XP 누적·음수 예외, 무료/유료 보상 종류 규칙, SaveData 왕복) —
  **통과 133 / 실패 0**. **남은 것**: 화면(진행 막대·보상 목록 UI)과 `MiningController` 배선(XP를
  언제·얼마나 주는지 — 레이스 승리마다? 채굴 시간마다? 아직 안 정함), 유료 트랙 구매 SKU를
  ShopCatalog에 추가하는 것, 시즌 시작/종료(4주 경계) 스케줄링은 전부 다음 세션 몫. Unity 에디터가
  없어 컴파일 확인은 다음 세션 몫 — 새 core 파일(`SeasonPass.cs`)에 아직 `.meta`가 없다.
- [ ] M-11 Steam 판 분기: 광고 항목 제거, 구독 대신 서포터 팩, 화물칸 기본 상한 1.5배. 플랫폼 플래그 하나로 갈린다
- [ ] M-12 스토어 문구 초안. 파는 것 전부와 안 파는 것 전부를 첫 문단에 나열. Steam 리뷰 방어의 핵심
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
- [x] D11-N (주말 매시간 세션, 03:02 마무리) 공구 상자. `Core/LootTable.cs`(확률표+천장)에 이어
  T-07을 "결정 안 나면 A안 기본 진행"으로 매듭짓고, `Core/LootReward.cs`+`Core/LootBoxOpener.cs`
  (확률표 뽑기+천장 카운터 갱신+부품 매핑 조립)까지 끝났다. 그다음 세션이 "상자를 실제로 얻는
  경로"를 채웠다 — `Course.Tier`(RaceTier: Local/Circuit/Challenge/GrandPrix) 필드 추가,
  `Core/RaceBoxReward.cs`(등급→상자 매핑, GDD 그대로 로컬=녹슨/서킷=강철/챌린지=티타늄), 쿼츠
  로컬 레이스 3개가 우승 시(`MiningController.TryEnterRace`) 기존 확정 슬롯 보상에 더해 녹슨
  상자를 1개 `_save`에 직접 더하도록 배선. 이번 세션(03:02)이 마지막 조각인 **개봉 화면**을
  마저 채웠다 — `MiningController.TryOpenBox(type, out result)` 신규(보유 0개면 false, 등급·슬롯
  seed는 `TryEnterRace`와 같은 이유로 `UnityEngine.Random`, `LootBoxOpener.Open` → `RigPartApply.Apply`
  로 즉시 채굴차 부품 반영, 상자 개수 차감 + 강철/티타늄 천장 카운터 갱신 + 저장까지 한 번에).
  `Assets/UI/LootBox.uxml`+`.uss`(Crafting.uxml과 같은 패턴, 상자 세 종류 한 줄씩 + 결과 카드) +
  `Assets/Scripts/UI/LootBoxPanel.cs`. `Root.uxml`의 action-row에 "상자" 버튼 추가(4번째 칸,
  기존 flex-basis 25%라 그대로 맞음), `MainHud.cs`에 `boxDocument` 배선(업그레이드/제작/레이스와
  같은 토글 패턴), `BootstrapMainGame.cs`(`GemRacer/7`)에 오버레이(sortingOrder 13, 레이스보다
  위) 추가. 서킷·챌린지 코스 자체(트랙 데이터, 해금 구조)는 `docs/backlog.md` W2 몫으로 남겨
  둔다 — 이번 세션도 침범하지 않았다, 그래서 강철·티타늄 열기 버튼은 실전에서 보유 0개로
  계속 비활성 상태인 게 정상(코드 확인용으로만 존재). 코어를 고치지 않아서(Assets 글루 코드만)
  `Core.Tests`는 그대로(통과 75 / 실패 0, 이전 세션 수치). Unity 에디터가 없어 컴파일 확인은
  다음 세션 몫 — 특히 `LootBoxPanel.cs`의 `UIDocument`/`Button.clicked` 클로저 캡처와
  `MiningController.TryOpenBox`의 `out LootBoxOpenResult` 문법을 봐 줄 것.
- [x] D11-M (주말 매시간 세션) 확률표 합 1.0 및 10만 회 시뮬레이션 분포 테스트. `Core.Tests`에 6개
  추가 — 세 상자 가중치 합 1.0, seed 재현성, 10만 회 분포가 표와 1%p 안쪽, 천장이 정확히
  pityCount번째에만 확정(그 전엔 확률대로), 천장 없는 상자는 안 확정, 빈 표·가중치 합 0은 예외.
  `dotnet run` **통과 66 / 실패 0**.
- [x] D12-N (주말 매시간 세션) 부품 강화(+10, 실패 없음, 비용 가파름) UI·코어. `Part.Enhance`/
  `Effective()`(강화 1당 +6%)는 D08-N 때 이미 있었다("강화는 D12에서 부품 위에 따로 붙는다"는
  주석까지 남겨 둔 상태) — 여기서 비용 곡선과 실제 반영만 채웠다. 코어에 `PartEnhance.cs`
  신규 — `Cost(part)`(등급 제작 비용의 절반에서 시작해 단계마다 1.9배, +10이면
  `PositiveInfinity`), `AtMax`, `Apply`(실패 롤 없음 — 비용만 내면 무조건 성공, `RigUpgrade`와
  같은 패턴). **부품 정체성 문제를 하나 발견해 같이 고쳤다**: `MiningController.AvailableParts`가
  `DefaultData.QuartzStarterParts()`를 호출 때마다 새 `Part` 인스턴스로 새로 만들어서, 강화
  단계를 인스턴스 필드(Enhance)에만 두면 다음 프레임에 그냥 사라진다 — 그래서 진짜 값은
  `MiningController`의 `_partEnhanceLevels`(부품 id→강화 단계 딕셔너리)와 `SaveData`
  (`OwnedPartEnhanceLevels`, `OwnedPartIds`와 같은 인덱스의 병렬 리스트 — JsonUtility가
  Dictionary를 못 다뤄서 `EquippedPartIds`와 같은 방식)에 두고, `AvailableParts`가 새
  인스턴스를 만들 때마다 저장된 값을 다시 입혀 준다. `MiningController.TryEnhancePart(part)`
  신규 — 비용 계산→차감→`PartEnhance.Apply`→`_partEnhanceLevels` 갱신까지 하고, 지금 장착
  중인 슬롯이 같은 부품이면 그 인스턴스(Awake 때 만들어진 별개 참조)도 같이 맞춰서 레이스
  스탯 계산에 바로 반영되게 했다. `Crafting.uxml`/`.uss`에 기존 다섯 줄 각각 강화 라벨(+N)과
  강화 버튼을 추가(같은 반응형 패턴 재사용, 새 레이아웃 없음), `CraftingPanel.cs`가 미보유면
  버튼 비활성, +10이면 "MAX", 그 외엔 "강화 (비용)"을 보여주고 클릭 시 `TryEnhancePart` 호출.
  `Core.Tests`에 5개 추가(비용 단조 증가, +10 무한대/AtMax, Apply 클램프, +0/+10 스탯 배율
  경계값, 미정의 등급 예외) — **통과 80 / 실패 0**. Unity 에디터가 없어 컴파일 확인은 다음
  세션 몫 — 특히 `CraftingPanel.cs`의 새 `_enhanceLabels`/`_enhanceButtons` 배열과 UXML 이름이
  정확히 맞는지, `MiningController.LoadParts`가 `_partEnhanceLevels`를 `OwnedPartIds`보다
  먼저 채우는 순서를 지키는지 봐 줄 것.
- [x] D12-M (주말 매시간 세션, D12-N과 같은 세션) 강화 비용 곡선 테스트. D12-N 안에 같이 넣었다 —
  단조 증가·최대치 클램프·+0/+10 스탯 경계값·미정의 등급 예외 4종.
- [x] D13-N (주말 매시간 세션) 튜토리얼 첫 5분: 첫 접속 → 채굴 시작 → 첫 부품 제작 → 첫 레이스,
  안내 말풍선 4개. 다른 오버레이(Upgrade/Crafting 등)처럼 화면을 막는 모달로 만들지 않았다 —
  3·4번째 말풍선이 "아래 '제작'/'레이스' 버튼을 눌러 보라"는 안내라서 배너가 클릭을 가로채면
  안 된다. `Assets/UI/Tutorial.uxml`+`.uss`(화면 맨 위 좁은 배너 하나) +
  `Assets/Scripts/UI/TutorialController.cs` — root의 `pickingMode`를 `Ignore`로 두고
  말풍선(bubble) 자체만 `Position`으로 되돌려서 배너 밖 클릭은 HUD로 그대로 통과시킨다.
  진행 상태는 `SaveData.TutorialStep`(0~4, 코어) + `MiningController.TutorialStep`(읽기 전용)/
  `AdvanceTutorial()`(쓰기 전용 — 화면은 이 함수 하나만 불러서 정확히 한 단계씩만 올린다,
  D13-M "단계 건너뛰기 방지"의 전부). `BootstrapMainGame.cs`(`GemRacer/7`)에 소트 오더 5로
  추가(HUD 위, 다른 모달 오버레이 10 이상보다는 아래 — 모달을 열면 그 뒤로 자연스레 가려진다).
  코어 변경은 필드 하나뿐이라(SaveData.TutorialStep) `Core.Tests`는 그대로(통과 80 / 실패 0,
  직전 세션과 동일 — 실제로 `dotnet run` 다시 돌려 확인). Unity 에디터가 없어 컴파일 확인은
  다음 세션 몫 — 특히 `TutorialController.cs`의 `PickingMode` 사용, `bubble.pickingMode`
  대입 문법.
- [x] D13-M (D13-N과 같은 세션) 문구 다듬기, 단계 건너뛰기 방지 점검. 문구 4개는
  `TutorialController.Messages`에 모아 뒀다(다음에 고칠 땐 그 배열만). 단계 건너뛰기 방지는
  화면이 `TutorialStep`을 직접 못 건드리게(읽기 전용 프로퍼티) `AdvanceTutorial()` 하나만
  통로로 남기고, 버튼도 클릭 즉시 비활성화해 다음 프레임 전 중복 클릭으로 두 단계가 한 번에
  넘어가는 것도 막았다 — 자동화 테스트가 아니라 코드 리뷰로 점검(Assets 쪽 글루 코드라
  Core.Tests 대상이 아님, MainHud·ResponsiveLayout 등 다른 UI 코드와 같은 이유).
- [x] D14-N (주말 매시간 세션) 사운드 자리 + 설정 화면. `Assets/Scripts/Audio/AudioHub.cs` 신규 —
  AudioSource 네 개(엔진·채굴·UI 탭·상자)를 한 오브젝트에 배선했다. 클립을 하나도 안 채워서(에셋
  팩이 아직 없다, W3 몫) 지금은 전부 무음 플레이스홀더다 — `PlayUiTap`/`PlayBoxOpen`/
  `SetMovementLoop` 전부 `clip == null`이면 조용히 아무 일도 안 하게 방어해 뒀다. 나중에
  인스펙터에서 클립만 채우면 코드 수정 없이 그대로 소리가 난다. `MiningController`가 이동/채굴
  전환마다 `audioHub.SetMovementLoop`를 부르고, `MainHud`의 action-row 버튼 다섯 개(업그레이드·
  제작·레이스·상자·설정)와 `LootBoxPanel`의 상자 열기가 각각 탭/개봉 효과음을 내도록 배선했다.
  설정 화면(코어 `GameSettings.cs`의 `NormalizeFrameRate`(30/60만 허용, 그 외엔 45 기준으로
  가까운 쪽 — 동률이면 60) + `SaveData.SoundEnabled`/`TargetFrameRate` 필드) —
  `Assets/UI/Settings.uxml`+`.uss`(LootBox.uxml과 같은 반응형 패턴) +
  `Assets/Scripts/UI/SettingsPanel.cs`. `MiningController.SetSoundEnabled`가 `AudioListener.volume`을
  0/1로 전역 음소거하고, `SetTargetFrameRate`가 `Application.targetFrameRate`에 즉시 반영한다.
  `Root.uxml`의 action-row에 "설정" 버튼 추가(다섯 번째 칸 — 기존 네 칸이 flex-basis 25%라 5번째는
  줄바꿈되어 혼자 한 줄을 차지한다, 어색하면 다음 세션이 flex-basis를 20%로 낮출 것),
  `BootstrapMainGame.cs`(`GemRacer/7`)에 AudioHub 오브젝트 + 설정 오버레이(sortingOrder 14, 상자보다
  위) 추가. `Core.Tests`에 3개 추가(허용값 그대로 반환, 45 경계, 0·음수·아주 큰 값 방어) —
  **통과 83 / 실패 0**. Unity 에디터가 없어 컴파일 확인은 다음 세션 몫 — 특히 `SettingsPanel.cs`의
  `Button.EnableInClassList` 사용과 action-row 다섯 번째 버튼의 줄바꿈 모양을 봐 줄 것.
- [x] D14-M (D14-N과 같은 세션) 설정 저장 확인. `MiningController.Save()`가 이미 `_save` 전체를
  쓰므로 `SoundEnabled`/`TargetFrameRate`도 자동으로 같이 저장된다(별도 코드 불필요) — 세이브
  라운드트립 테스트(`Core.Tests`의 "세이브: 직렬화→역직렬화..." 항목)가 이미 SaveData 전체를
  검증하니 이 두 필드가 새로 깨질 위험은 낮다고 보고 별도 테스트는 추가하지 않았다. 실제로 값이
  남는지(설정 바꾸고 Play 재시작)는 에디터가 있는 다음 세션이 눈으로 확인.
- [x] D15-N (주말 매시간 세션) 안드로이드 빌드 준비. `docs/design/android-build-checklist.md` 신규
  — 지금 값과 목표를 표로 대조(최소 API 25/목표 API Auto/IL2CPP/ARM64는 이미 적정값이라 스크립트로
  안 건드림). `Assets/Editor/BuildSettingsMobilePC.cs` 신규 — `GemRacer/9. 안드로이드 세로 고정
  적용`(방향 관련 필드만, `defaultInterfaceOrientation`=Portrait + 나머지 세 방향 끔),
  `GemRacer/10. PC 세로 창 설정 적용`(Standalone 기본 창 540×960 + 리사이즈 허용 +
  Windowed) — 둘 다 `ProjectSettings.asset`의 실제 YAML 필드명을 먼저 확인하고 썼다(defaultScreenWidth/
  Height, resizableWindow, fullscreenMode 등 전부 지금 파일에 이미 있는 키). 키스토어는 비밀번호가
  같이 도는 절차라 코드화하지 않고 문서 절차로만 남겼고, `.gitignore`에 `*.keystore`/`*.jks` 추가.
  패키지명(Application ID)이 아직 URP 템플릿 placeholder라 `docs/decisions.md` T-09에 선택지 올림
  (A안 제안, 결정 전이라 스크립트는 안 건드림). 코어(Packages)는 안 건드려서 Core.Tests 그대로
  (직전 세션 통과 83 / 실패 0).
- [x] D15-M 빌드 체크리스트 대조 → 2026-09-14 Unity MCP로 에디터에 붙어 `GemRacer/9`·`GemRacer/10`을 실행하고 값을 되읽어 확인. 방향 세로 고정·Min API 25·Target Auto·IL2CPP·ARM64·PC 창 540x960 전부 목표대로. 남은 건 패키지명(T-09 결정)과 키스토어(수동)뿐. 상세는 `docs/design/android-build-checklist.md`
- [x] D16-N (주말 매시간 세션) 안정화 1: `docs/feedback.md`가 이번 세션 시작 시점에 이미
  비어 있어서(밀린 `- [?]`도 없음) 처리할 게 없었다 — 그대로 통과 처리.
- [x] D16-M (주말 매시간 세션) 테스트 전수 통과 확인. `cd Core.Tests && dotnet run` **통과
  83 / 실패 0**(직전 07:02 세션 수치 그대로 — 이번 세션도 코어를 안 고쳐서 개수는 안 늘었다).
  이 클라우드 환경도 `dotnet`이 없어서 다시 설치했다(매 세션 반복되는 패턴).
- [x] D17-N (주말 매시간 세션) 지인 테스트 준비. `Assets/Scripts/Diagnostics/FeedbackLog.cs`
  신규 — 텍스트를 `persistentDataPath/feedback.txt`에 이어 쓰고 클립보드에도 복사(공유는
  네이티브 공유 시트 대신 클립보드 복사로 대신함, 이유는 `docs/design/session-log-format.md`
  참고). `Assets/Scripts/Diagnostics/SessionLogger.cs` 신규 — 포그라운드 진입/이탈을 세션
  경계로 봐서 `session_log.csv`에 시작 시각·플레이 시간을 한 줄씩 남긴다. 설정 화면
  (`Settings.uxml`/`.uss`/`SettingsPanel.cs`)에 피드백 입력칸+저장 버튼 추가, `BootstrapMainGame.cs`
  (`GemRacer/7`)의 GameFlow 오브젝트에 `SessionLogger` 배선. 둘 다 순수 파일 IO/로그 코드라
  게임 규칙이 아니라고 보고 Core는 안 건드림(CLAUDE.md 1번 — 코어는 게임 규칙·수식만).
- [x] D17-M (D17-N과 같은 세션) 로그 포맷 문서화. `docs/design/session-log-format.md` 신규 —
  두 파일의 컬럼/형식, D23-N이 나중에 이 로그를 읽을 때 참고할 점(포그라운드 경계, UTC/KST
  날짜 변환), 알려진 한계(서버 업로드 없음, WebGL 새로고침 시 지속 여부 불확실)까지 정리.
- [ ] D18-N (9/29 화) 리텐션 훅: 화물칸이 다 찼을 때 로컬 알림(Android/iOS Mobile Notifications 패키지), 하루 첫 접속 보상.
  **(9/15 저녁 매시간 세션 진행 중)** 두 축 중 "하루 첫 접속 보상" 쪽만 core로 끝냈다 — 화물칸
  알림 쪽은 M-05와 완전히 같은 이유(Unity Mobile Notifications 패키지, Package Manager를 클라우드
  세션이 건드리면 URP 셰이더 사고처럼 빌드가 죽을 위험)로 손 안 댐, 에디터 세션 몫으로 그대로 둠.
  `Core/DailyLoginReward.cs` 신규 — `DailyLoginState`(마지막으로 받은 날 + 연속 접속 일수)를
  `RewardAdTracker.DayIndex`(이미 검증된 KST 자정 경계 계산, 새로 안 만들고 재사용)로 판정한다.
  `CanClaim`/`Claim`(오늘 이미 받았으면 방어적으로 상태 그대로, RewardAdTracker.RecordWatch와 같은
  패턴), 어제 받았으면 스트릭 +1, 하루라도 건너뛰면 스트릭이 1로(완전 초기화는 아님). 보상은
  `RawMineralsFor(streakDays)` — 7일 주기표(5/8/10/12/15/18/30 원석), 7일차가 1일차의 6배라
  "일주일 채우면 크게 온다"는 감을 준다. 8일차부터는 다시 1일차로 순환(월 단위 초기화 없음).
  실제 지급은 코어가 하지 않고 호출부(MiningController 몫)가 `Claim` 결과의 `StreakDays`로
  `RawMineralsFor`를 불러 직접 준다 — RewardAdTracker와 같은 역할 분리(코어는 "얼마 줄지"만,
  "준다"는 글루 레이어). `SaveData`에 `DailyLoginLastClaimedDayIndex`/`DailyLoginStreakDays`
  2필드 + `ToDailyLoginState()`/`ApplyDailyLoginState()` 왕복 함수(다른 상태들과 같은 패턴).
  `Core.Tests`에 7개 추가(첫 접속/같은 날 재접속 방어/연속 접속 스트릭 증가/하루 건너뛰면 스트릭
  리셋/7일 순환·경계값 방어/보상표 단조 증가/세이브 왕복) — **통과 140 / 실패 0**.
  **남은 것(에디터 있는 세션 몫)**: `MiningController`(또는 새 컴포넌트)에서 앱 시작 시
  `DailyLoginReward.CanClaim`을 확인해 화면(간단한 팝업 하나, OfflineReward/CargoFull과 같은
  중앙 카드 톤)을 띄우고 "받기"를 누르면 `Claim`+`TrySpendRawMinerals`의 반대(지급이라 덧셈)로
  원석을 준다. 새 core 파일(`DailyLoginReward.cs`)에 아직 `.meta`가 없다(에디터가 여는 다음
  세션에서 생기는 대로 커밋할 것, 계속 반복되는 패턴). 이미지 요청은 없음 — 팝업은 기존 카드
  스타일 재사용이라 새 아이콘이 굳이 필요 없어 보임(필요해지면 다음 세션이 판단).
- [x] D18-M 알림 예약 시각 계산 테스트. → M-05에서 이미 끝남(`HoursUntilCargoThreshold` 4개 테스트,
  9/15 새벽 세션) — 화물칸 몇 %에서 알릴지 계산이라 이 항목과 같은 것이었다. 체크만 누락돼 있었다.
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
