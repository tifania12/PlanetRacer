# 작업 목록 (일 단위)

규칙: 위에서부터 체크 안 된 항목을 집는다. 항목 뒤의 `-N`/`-M`은 옛 표기이고 지금은 구분하지 않는다.
`docs/feedback.md`에 `- [ ]` 줄이 있으면 여기보다 먼저 처리한다.

일과 (2026-09-13 개정. 자세한 것은 CLAUDE.md)
- 평일 23:00 / 03:00 — 크게. 한 번에 2~3항목
- 평일 06:30 — 작게. 검증·배포 확인·아침 가이드만. 새 기능 금지
- 주말 매시간(하루 24번) — 한 번에 1~2항목. 예전엔 하루 네 번(09/14/19/23시)에 3~4항목씩
  크게 갔지만, 지금은 매시간 도니 그렇게 하면 세션끼리 겹친다.

날짜는 이제 맞지 않으므로 순서만 본다.

## 🔥 최우선 — 아트 배선 (2026-09-20 신설, 설계는 docs/design/art-wiring.md)

Tifania: "이미지 애셋 만든 거 이런 건 하나도 적용이 안 되어 있는 것 같아서 물어봤어."

확인해 보니 맞다. `Resources/Art`에 **PNG 181장**이 들어가 있는데 게임 코드에서
그림을 부르는 곳은 `Scripts/UI/ArtViewer.cs:78` **한 군데**뿐이고, 그건 `?art=1` 로만
열리는 확인용 갤러리다. 실제 화면 열세 개는 전부 유니티 기본 회색 박스를 쓴다.
**빌드 용량은 다 내고 플레이어에게는 한 장도 안 닿았다**(T-13에서 압축하느라 고생한 게 이것들이다).

원래 이 일은 P2 W3 "실제 아트 적용" 한 줄로만 있었다. P2는 아직 시작 전이라 아무도
집어 가지 않았다. **그 한 줄을 여기로 끌어올린다.** 판단할 게 없도록 화면별 매핑을
`docs/design/art-wiring.md`에 표로 적어 뒀다 — 집어 갈 때 그 문서만 보면 된다.

- [x] **A-16 아이콘 18종을 실제 화면에 붙인다 (2026-09-20 완료).** 두 세션이 같은 시간대에 겹쳐 각자 다른 화면을
      집었다 — 둘 다 남는다(코드 충돌 없이 리베이스됨).
      **끝나고 눈으로 확인됨 — 업그레이드 화면(2026-09-20 19시 Unity 배선 세션).**
      `currency-raw-icon` · `currency-refined-icon` · `refinery-icon`(임시 `icon-blueprint`) ·
      `tool-icon` · `cargo-icon` · `engine-icon` 여섯 자리를 만들고 실제 그림이 뜨는 것까지
      플레이 모드에서 확인했다(예외 0). `BootstrapUpgradeUgui`가 이름 붙인 빈 `Image`를 만들고
      `UpgradeUgui.SetIcon`이 `Resources.Load`로 넣는 식이다.
      **끝나고 눈으로 확인됨 — 제작·상자·레이스 출전 세 화면(2026-09-20 21시 Unity 배선
      세션). 코드는 같은 날 19시대 클라우드 코딩 세션이 썼다.**
      `BootstrapCraftingUgui.cs`(엔진/타이어/서스펜션 아이콘 + 등급 뱃지 자리
      다섯 칸, 차체·부스터는 그림 없어 아이콘 자리 자체를 생략) · `BootstrapLootBoxUgui.cs`
      (녹슨/강철/티타늄 아이콘) · `BootstrapRaceEntryUgui.cs`(연료 아이콘) — 셋 다 자기
      서브트리(`Crafting`/`LootBox`/`RaceEntry`)만 지우고 다시 만드는 기존 방식이라 해당 메뉴
      (`GemRacer/17·19·18`)를 다시 눌러도 안전하다(다른 화면은 안 건드림). 대응하는 패널
      스크립트(`CraftingUgui`·`LootBoxUgui`·`RaceEntryUgui`)에 `UiKit.SetIcon`/`UiKit.LoadIcon`
      (신규, `UiKit.cs`)으로 그림을 입혔다 — 그림이 아직 없으면 투명 자리로 조용히 남는다.
      등급 뱃지(`icon-grade-c/b/a/s`)는 `part.Grade`가 바뀔 수 있어 매 프레임 다시 확인하지만
      `UiKit.LoadIcon`이 캐시해서 디스크는 한 번만 읽는다.
      세 메뉴(`GemRacer/17·19·18`)를 다시 눌러 씬에 반영하고 `MainHudUgui`의
      `craftPanel`·`boxPanel`·`racePanel`을 다시 물렸다. 컴파일 에러 0, 플레이 12초 예외 0,
      게임 뷰 스크린샷으로 엔진·타이어·서스펜션 / 녹슨·강철·티타늄 / 연료 아이콘이 실제로
      뜨는 것을 눈으로 확인했다. 등급 뱃지는 아직 승급한 부품이 없어 빈 자리로 남는다(정상).
      확인하다 레이스 출전 화면의 연료 줄이 164px까지 늘어나 아이콘만 크게 뜨는 것을 발견해
      같이 고쳤다 — `HorizontalLayoutGroup`이 `childForceExpandHeight=true`면 스스로
      `flexibleHeight=1`을 내놓아 바깥 세로 그룹이 남는 공간을 그 줄에 준다. 그 줄의
      `LayoutElement.flexibleHeight = 0`, `childControlWidth = true`로 잡았다.
      **끝나고 눈으로 확인됨 — HUD 원석 아이콘(2026-09-20 21시 Unity 배선 세션).**
      `BootstrapHudUgui.Build()`(GemRacer/13)를 다시 누르면 이미 배선된 일곱 화면이 날아가니
      (`BootstrapShopUgui.AddShopButtonToActionRow`(`GemRacer/23`)와 같은 이유), status-bar
      밑만 건드리는 `AddMineralIconToStatusBar`(**GemRacer/24**)를 새로 만들었다. status-bar의
      바깥 줄은 반반(planet-name / mineral-count)인데 아이콘을 세 번째 자식으로 그냥 끼우면
      셋으로 쪼개져 지금 확인된 배치가 흔들린다 — 그래서 mineral-count를 "mineral-group"으로
      감싸고 그 안에서만 아이콘+글자를 나란히 놓아, 바깥에서 보면 여전히 자식 둘이라 반반은
      그대로다. 몇 번을 다시 눌러도 결과가 같다(mineral-group이 있으면 도로 풀고 새로 만듦).
      `MainHudUgui.Awake()`에 `UiKit.SetIcon("mineral-icon", "icon-raw-mineral")` 한 줄을
      추가해 실제 그림을 넣는다(자리가 없는 옛 씬에서는 조용히 넘어감).
      `GemRacer/24`를 실행해 씬에 반영했다. 컴파일 에러 0, 플레이 12초 예외 0,
      `mineral-icon`에 `icon-raw-mineral`이 실제로 들어가 원석 글자 옆에 붙는 것을 게임 뷰
      스크린샷으로 확인했다. 바깥 반반 배치도 그대로다(status-bar 508 안에서 planet-name 228 /
      mineral-group 248).
      메뉴 번호 24는 `BootstrapCargoFullUgui`(화물칸 가득)와 겹친다 — 이름이 달라 유니티는 둘 다
      보여 주지만, 다음에 번호를 새로 붙이는 세션은 25부터 쓰는 게 좋다.
      **아직 안 한 것 — 상점(열쇠).**
      상점의 `icon-key`는 게임에 "열쇠" 재화 자체가 아직 코드 어디에도 없어(design 문서에만
      있음) 붙일 자리가 없다 — A-16이 아니라 그 재화가 실제로 생길 때 같이 할 일.
      곁들여 넣기로 했던 `icon-refinery`·`icon-part-body`·`icon-part-booster`는 이미
      `art-requests.md` 대기열에 있다(9/20 낮 세션이 올려 둠).
      **남은 세 장, 코드까지는 끝났다 (2026-09-21 코딩 세션).** 셋 다 `Resources/Art/Icons`에
      들어온 것을 확인하고 `UpgradeUgui.cs`(임시 `icon-blueprint` → `icon-refinery`)·
      `CraftingUgui.cs`(차체·부스터 `null` → `icon-part-body`/`icon-part-booster`)와 짝이 되는
      `BootstrapUpgradeUgui.cs`·`BootstrapCraftingUgui.cs`를 같이 고쳤다. 업그레이드 화면은
      `refinery-icon` 자리가 이미 씬에 있어(9/20 19시 Unity 세션이 만듦) 다음 dev 배포에
      바로 반영된다. **제작 화면도 씬 반영까지 끝났다 — 눈으로 확인됨(2026-09-21 03시 Unity
      배선 세션).** `GemRacer/17`을 재실행해 `Crafting` 서브트리를 다시 세웠다. 한 번에 되지는
      않았다 — **처음 누른 건 옛 어셈블리였다.** 유니티가 9/16부터 계속 떠 있어 9/21 코딩 세션이
      고친 `BootstrapCraftingUgui.cs`를 아직 컴파일하지 않은 상태였고, 그래서 `row-body`·
      `row-booster` 줄은 생기는데 아이콘 자리만 안 생겼다(옛 코드의 `iconName == null` 가지).
      `refresh_unity`(force/compile)로 임포트·컴파일한 뒤 다시 누르니 다섯 자리가 전부 생겼다.
      **오래 떠 있는 에디터에 부트스트랩을 누를 때는 먼저 `refresh_unity`를 한 번 돌릴 것** —
      메뉴는 성공으로 돌아오고 씬도 실제로 바뀌기 때문에 결과를 따로 확인하지 않으면 속는다.
      끊긴 `MainHudUgui.craftPanel`을 다시 물렸고(나머지 일곱 칸은 그대로), 씬 저장 → 플레이
      12초 예외 0 → 게임 뷰 스크린샷으로 엔진·타이어·서스펜션·**차체·부스터** 다섯 줄에 실제
      그림이 뜨는 것을 확인했다(스크롤을 내려 부스터 줄까지 봤다). 이걸로 A-16 아이콘 배선은
      열쇠 한 장만 남는다.
      곁다리로 본 것: 등급 뱃지가 다섯 줄 모두 `icon-grade-c`로 떠 있다. 아직 아무것도 제작하지
      않아 전부 `미보유`인데 C 뱃지가 보이는 건 조금 이상하다 — 이번 변경과 무관하고(기존 세
      줄도 같다) 손대지 않았다. 다음 코딩 세션이 볼 것.
      **확인함(2026-09-21 04시 코딩 세션) — 버그 아님.** `CraftingUgui.SetRow`(`Assets/Scripts/UI/CraftingUgui.cs:129`)는
      `owned` 여부와 무관하게 `part.Grade`로 뱃지를 고른다. `MiningController.AvailableParts`가
      지금 돌려주는 다섯 개는 전부 `DefaultData.QuartzStarterParts()`(C등급 고정)뿐이고, B/A/S
      등급 부품(`QuartzEpicParts` 등, 02시 세션에서 코어에 추가됨)은 아직 어느 화면에도 안
      노출된다 — 그러니 지금 화면에 있는 부품 다섯 개가 전부 C인 게 사실이고, 뱃지가 C만
      보이는 건 그 사실을 그대로 보여 주는 것뿐이다. "미보유인데 등급이 보인다"는 것도
      의도로 봐도 된다 — 제작하면 어떤 등급이 나오는지 미리 알려 주는 편이 자연스럽다(다른
      게임의 "미리보기" 관행과 같다). B/A/S가 화면에 실제로 붙을 때(위 "이어서 할 것 (2)")
      뱃지가 그때 가서 B/A/S도 제대로 보여 주는지만 다시 확인하면 된다 — 지금은 손댈 것 없음.
- [x] **A-21 펫 폴더에 남은 옛 파일 26장 정리 (2026-09-20 완료).** Tifania 확인 후 `git rm`.
      지우기 전에 md5로 맞춰 보니 **등급 폴더 123장과 한 장도 같지 않았다** — 복사본이 아니라
      같은 자리를 나중에 다시 그린 옛 세대였다. 코드에서 `pet-t` 이름을 부르는 곳이 한 군데도
      없고, T-12 이후 등급 폴더 쪽이 살아 있는 이름이라 지웠다. **26.2MB 감소.**
      커밋 이력에는 남아 있으니 필요하면 꺼낼 수 있다. 자세한 것은 `art-wiring.md` 4절.
- [ ] **A-17 펫 뽑기 화면 + 도감.** 펫 그림 123장이 한꺼번에 살아난다. 수익 화면이기도 하다.
      코어는 이미 있다(`PetGachaController` · `PetGachaTable` · `PetCollection` · `PetFusion`).
      확률 공개 화면(`PetGachaOddsUgui`)이 있으니 그 옆에 붙이는 모양이 자연스럽다.
      **화면 붙이기 전에 준비 하나 끝냄(2026-09-21 04시 코딩 세션) — 종 → 그림 파일 경로 매핑.**
      화면을 실제로 짜기 전에 조사해 보니(`Assets/Resources/Art/Pets` 폴더 구조 vs
      `PetSpeciesTable`의 종 정의) 등급마다 파일명 규칙이 제각각이라 이 변환이 없으면 도감·뽑기
      결과 화면 둘 다 각자 다시 만들다 어긋나기 쉬웠다 — 그래서 화면보다 먼저 코어에
      `PetArt.ResourcePath(PetSpeciesDef) → string`(새 파일, `Packages/com.bax.gemracer.core/Runtime/PetArt.cs`)
      을 만들었다. 규칙 넷:
      1~5등급은 `{계열}-{색}`(1·5등급은 쿼츠도 접미사 붙임, 2~4등급은 쿼츠만 접미사 없음) ·
      6등급(신화)은 `{계열}-{계열 안 순번 01~08}` · 7등급(초월)은 고유 이름 10개인데
      `art-requests.md`에 이미 "초월 N/10 — 이름 (축)"으로 `TranscendentAxisKo` 순서와 나란히
      적혀 있어 그 순서를 그대로 배열로 옮겼다(임의로 정하지 않아도 됐다).
      `Core.Tests`에 5개 추가(등급별 규칙 + **124종 전부를 실제 디스크 파일과 대조하는 테스트** —
      `6-myth/ore-06.png` 누락 하나만 예외로 맞고 나머지 123장은 전부 일치 확인, 335→340, 실패 0).
      Unity 참조 없는 순수 C#, 새 파일이라 `.meta`도 새로 만들었다(GUID 중복 없음 확인).
      **장착 필드도 채움(2026-09-21 06시 코딩 세션)** — `SaveData.PetGachaSave.EquippedSpeciesId`
      (기본 -1 = 미장착, 마이그레이션 불필요)와 `EquipSpecies`/`UnequipSpecies`/`HasEquippedSpecies`를
      추가했다. 한 번에 한 마리만(`PetCollection.EquipBonus(PetGrade)`가 값 하나를 돌려주는 구조와
      맞춤). 도감에 없는 종은 장착 시도 시 예외 — 등급 제한은 안 걸었다(6·7등급 고유 효과가
      아직 없어도 미리 장착해 둘 수 있어야 나중에 효과가 생겼을 때 다시 장착할 필요가 없다).
      `Core.Tests` 3개 추가(기본값 -1 · 미보유 종 장착 시 예외 · 장착 교체와 해제), 340→**343, 실패 0**.
      **스프라이트 로드 헬퍼 끝냄(2026-09-21 18시 코딩 세션).** `UiKit.LoadIcon`이
      `Art/Icons/` 고정이라 못 쓰던 문제를 `UiKit.LoadSpriteAtPath(string resourcePath)`(전체
      경로를 그대로 받음)와 `UiKit.SetSpriteAtPath(root, imageName, resourcePath)`로 풀었다.
      `LoadIcon`/`SetIcon`은 내부에서 이 둘을 부르는 얇은 래퍼로 바뀌었다(동작 그대로, 캐시도
      경로 기준 하나로 합침). 화면 쪽에서는 `UiKit.SetSpriteAtPath(root, "portrait",
      PetArt.ResourcePath(def))` 한 줄이면 된다. Unity 참조 코드(`Assets/Scripts/UI/UiKit.cs`)라
      `Core.Tests`엔 안 걸림 — 컴파일 확인은 다음 Unity 세션 몫.
      **레이아웃 설계 끝냄(2026-09-21 18시 코딩 세션) — `pet-gacha.md` 9절.** 뽑기 실행 화면
      (`PetGachaPullUgui`, `GemRacer/26`, 확률 공개 화면 옆)과 도감 그리드(`PetDexUgui`,
      `BootstrapArtViewer.cs`의 그리드 패턴 재사용 + 124종 등급별 섹션)의 구성·오브젝트 이름·
      가로세로 재배치까지 정해 뒀다. **막힌 지점 둘**을 같이 적어 뒀다 — (1) 뽑기 중복 시
      자동 조각 전환 함수가 코어에 아직 없음(설계 결정 필요), (2) 6·7등급 종 이름
      (`MechanicalDisplayNameKo`가 예외를 던짐 — 도감 상세 팝업에 이름을 못 씀).
      **9-1(뽑기 실행 화면) 코드로 끝냄(2026-09-21 19시 코딩 세션).** 위 막힌 지점 둘 다
      실제로는 더 쉽게 풀렸다 — (1)은 `PetGachaController.ResolveAndRecordSpecies`가 이미
      "중복이면 조각 1개 자동 지급"을 하고 있었다(설계 문서 작성 시점에 놓친 부분, 코드가
      더 앞서 있었다). (2)는 `PetSpeciesTable.DisplayNameKo(def)`를 새로 추가해 풀었다 —
      1~5등급은 `MechanicalDisplayNameKo` 그대로, 6등급은 "{계열} 신화 #NN", 7등급은
      "초월 · {TranscendentAxisKo}"로 정식 이름이 없어도 화면이 안 죽는다(정식 이름이
      정해지면 이 함수 안만 고치면 됨). `Core.Tests` 5개 추가(343→**347, 실패 0**).
      `MiningController`에 `PetGacha` 프로퍼티 + `PullFreePet`/`PullNormalPet`/
      `PullAdvancedPet`/`PullAdvancedTenPet`/`PullSpecialPet` 다섯 래퍼(TryEnterRace와 같은
      패턴, seed는 UnityEngine.Random으로 글루 레이어가 뽑아 코어에 넘김)를 추가하고 Awake에
      `PetGacha.ResetDailyIfNewDay` 호출을 끼워 넣었다. 화면은 `Assets/Scripts/UI/
      PetGachaPullUgui.cs`(신규) + `Assets/Editor/BootstrapPetGachaPullUgui.cs`(신규,
      `GemRacer/26`) — `PetGachaOddsUgui`/`BootstrapPetGachaOddsUgui.cs`와 같은 패턴(카드+
      ScrollRect, CanvasScaler가 반응형을 맡음, 손으로 가로/세로 재배치 코드 안 씀).
      **일반/고급/특수 뽑기의 인게임 재화 비용은 아직 안 정해져서(design 4절엔 고급·특수의
      실물결제 단가만 있음) 이 화면은 비용 확인·차감 없이 바로 뽑는다** — `MiningController`
      쪽 메서드 주석에 TODO로 남겨 뒀다, 값이 정해지면 그 메서드들 앞에 확인만 끼우면 된다.
      조각 합성(`btn-fusion`) 실행은 여전히 범위 밖 — 버튼 자리만 잡고 비활성으로 뒀다.
      Unity 참조 코드라 컴파일 확인은 못 했다 — **다음 Unity 세션이 `GemRacer/26` 실행 +
      MainHudUgui 연결(여는 버튼 없음, 확률 공개 화면처럼 Play 중 Hierarchy에서 직접 켜서
      우선 확인) 필요.**
      **9-2(도감 그리드) 코드로 끝냄(2026-09-21 20시 코딩 세션).** `Assets/Scripts/UI/
      PetDexUgui.cs`(신규) + `Assets/Editor/BootstrapPetDexUgui.cs`(신규, `GemRacer/27`).
      부트스트랩이 `PetSpeciesTable.All`(124종) 전부를 등급 7섹션으로 나눠 `dex-cell-{id}` 칸을
      미리 만들어 두고(`BootstrapArtViewer.cs`의 GridLayoutGroup 3열 패턴 재사용), 화면 스크립트는
      `PetDexUgui.OnEnable`(패널이 열릴 때만, PetGachaPullUgui의 Update()와 달리 도감은 안에서
      보유 상태가 스스로 안 바뀐다)에서 `OwnsSpecies`를 보고 보유 종만 스프라이트를 입힌다 —
      미보유는 부트스트랩이 만든 회색 실루엣 그대로(2026-09-15 원칙). 셀을 탭하면 상세 팝업
      (`dex-detail-panel`)이 뜨고 이름(`DisplayNameKo`, 6·7등급도 예외 없이 이름을 준다) + 등급 +
      "장착" 버튼을 보여준다. `MiningController.EquipPetSpecies(speciesId)` 래퍼를 추가해
      `_save.PetGacha.EquipSpecies` + `Save()`를 감쌌다(호출 전 보유 확인은 화면 몫, Pull*Pet
      래퍼들과 같은 책임 분리). 가로/태블릿에서 열 수를 5~6으로 늘리는 건(design 9-2) 여기서는
      안 함 — 9-1과 같은 이유로 CanvasScaler에 맡긴다.
      `Core.Tests`는 안 건드렸다(코어 API를 그대로 재사용, 새 코어 함수 없음) — `dotnet run` **347
      통과 / 실패 0** 그대로(회귀 확인만).
      Unity 참조 코드라 컴파일 확인은 못 했다 — **다음 Unity 세션이 `GemRacer/27` 실행 + Play 중
      Hierarchy에서 `PetDex` 오브젝트를 직접 켜서 확인 필요** (9-1과 같은 처지, MainHudUgui에
      여는 버튼 둘 다 아직 없음).
      **아직 안 한 것** — 두 화면을 여는 버튼을 MainHudUgui에 달기, 씬 배선(`GemRacer/26`·`27`
      실행 후 저장), 일반/고급/특수 뽑기 재화 비용 확정, 조각 합성(`PetFusion`) 실행 화면.
- [ ] **A-18 행성 선택 / 워프 흐름** — `planet-*` 6장이 여기 붙는다. P2 W1과 같은 일이라
      P2 시작과 함께 간다.
- [ ] **A-19 컷신 재생** — `opening` · `first-race-win` · `arrive-*` 5장, 모두 7장.
- [ ] **A-20 채굴차 티어 외형** — `rig-tiers-sheet` 한 장인데 **스프라이트 시트**라 잘라야 한다.

## ⛔ 지금 막혀 있는 것

(아래 T-14는 저절로 풀렸다 — 2026-09-20 20시 세션이 API로 확인)

- [x] **T-14 WebGL 빌드가 네 번 연속 죽었다 → 다섯 번째 시도(run #345)에서 저절로 성공했다.**
      #339 성공 → #340~#342 세 번 연속 실패(exit 255·255·139) → **#343은 A-16 나머지 절반
      커밋(`6943e3b`)인데 cancelled**(다른 세션이 겹쳐서 취소된 것으로 보임, 실패 아님) →
      **#344(문서만, `c9038c9`) 성공** → **#345(T-14 기록 자체, `1020272`) 성공**.
      #344가 이미 성공이었다는 게 핵심 — T-14를 backlog에 올린 19시 세션 시점엔 #344가
      아직 안 끝나 있었을 것이다. `mcp__github__actions_list`(`list_workflow_jobs`)로
      #345의 14단계를 전부 확인: `unity-builder` 빌드 → 파일 크기 확인 → Cloudflare 배포 →
      배포 확인 → **별칭 주소 확인까지 14단계 전부 success**. 코드를 하나도 안 고쳤는데
      풀렸다 — T-13 때의 "세 번 연속 exit 139"와 같은 결로, **깃허브 러너의 일시적 문제였지
      이 저장소 코드의 지속적인 버그가 아니었다**로 결론. 원인(메모리/캐시/라이선스 중 무엇)은
      재현이 안 돼 특정하지 못했다 — 다시 나면 그때 스왑을 키우거나 캐시 키를 갈아 본다.
      **dev 주소가 다시 살아 있다** — A-21(펫 정리)·A-16(아이콘 배선)이 이제 웹에 보인다.

(아래 T-13은 풀렸다 — 2026-09-20 06시 세션이 확인·정리)

- [x] **T-13 웹 배포가 이틀째 안 됐던 것 — Tifania가 A+C안으로 정해서 04:43 세션이 고쳤고,
      06시 세션이 실제 빌드 통과를 확인했다.**
      Actions run #301부터 연달아 실패고 마지막 성공은 #300(`2438800`, P-03)이다. 그래서
      planetracer-daz.pages.dev는 그 시점에 멈춰 있다 — P-03·P-04·A-04는 웹에서 안 보인다.
      **Unity 빌드는 성공한다**(`unity-builder` 단계 success). 죽는 자리는 그다음
      `파일 크기 확인` 단계다 — Cloudflare Pages가 25MiB 넘는 파일을 거부해서 배포 단계가
      전부 skipped 된다. `Assets/Resources/` 아래는 참조 여부와 상관없이 전부 빌드에
      들어가는데 그게 막힐 당시 **98MB**(그 뒤 이미지 세션이 더 넣어 커밋 시점엔 169MB,
      펫만 101장)까지 불었다. 선택지 세 개(A 임포트 해상도 낮추기 / B 아직 안 쓰는 그림을
      Resources 밖으로 / C 원본을 줄이는 파이프라인) 중 **Tifania가 A+C안**("좋아 진행하자")을
      골랐고, 04:43 세션이 `Assets/Editor/ArtImportSettings.cs`(AssetPostprocessor)로
      Pets·Icons는 512, Cutscenes·Planets·Rigs는 1024로 최대 해상도를 낮추고 압축+crunch를
      걸었다(원본 PNG는 안 건드리고 `.meta`만 변경, 앞으로 들어오는 그림에도 자동 적용).
      커밋 `07db953`. **06시 세션이 GitHub Actions MCP로 직접 확인**: run #320
      (`35465252016`)이 `success`로 완료 — Cloudflare Pages 배포까지 끝까지 갔다.
      dev 주소(`https://dev.planetracer-daz.pages.dev`)가 다시 살아 있을 것이다 —
      **Tifania가 열어서 실제로 보이는지, 펫 화질(512 해상도)이 괜찮은지 확인할 것.**
      **정해지기 전엔 어느 세션도 아트 파이프라인을 건드리지 않는다.**
      이것 때문에 main 승격도 멈춰 있다(승격 조건 "직전 WebGL 빌드 성공" 불충족, main은 d704632).

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
- [x] A-11 (2026-09-16 확인) 행성 구체 6종(쿼츠·루비·사파이어·아쿠아마린·주사·라피스) 프롬프트
      전부 `art-requests.md`에 이미 올라가 있다. W1 화면이 아직 없어도 이미지 자체는 미리
      받아 둘 수 있어 체크만 바로잡음
- [x] A-12 (2026-09-16 확인) `rig-tiers-sheet.png`(세 티어 한 장) 프롬프트가 `art-requests.md`에
      올라가 있다. 체크만 바로잡음
- [x] A-13 (2026-09-16 확인) 컷신 7장(오프닝=쿼츠 도착 + 루비·사파이어·아쿠아마린·주사·라피스
      도착 5장 + 첫 레이스 우승) 전부 `art-requests.md`에 올라가 있다. 체크만 바로잡음
- [x] A-14 (2026-09-18 03시 Unity 배선 세션) 아이콘 임포트 설정을 Sprite(2D and UI)로 맞췄다.
      (배경 제거는 더 이상 필요 없다 — 2026-09-16부터 투명 PNG로 받는다. 이미지 세션이
      `python tools/check_alpha.py <파일>`로 RGBA·모서리·잔상을 검사한 뒤에만 넣는다)
      Icons 18 + Planets 6 + Rigs 1 = **25장 전부** `textureType=Sprite`, `Single`,
      `alphaIsTransparency`, mipmap 끔, wrap Clamp. 전부 `Resources.Load<Sprite>`로 뜨는 것까지 확인.
      컷신 7장은 **일부러 건드리지 않았다**(T-11 판단 대기, 아직 untracked).
      씬은 안 건드렸다 — 아직 이 그림들을 참조하는 코드·씬·프리팹이 0건이다(guid 검색으로 확인).
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
- [x] U-04 (2026-09-15 야간 코드 + 2026-09-16 03:20 Unity 세션에서 배선·확인) `Assets/Scripts/UI/RaceEntryUgui.cs`
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
      **배선 결과(2026-09-16 03:20 Unity 세션)**: `GemRacer/18` 실행 → `MainHudUgui.racePanel`에
      `RaceEntry` 물림 → 씬 저장. 칸 수는 요구대로 **1칸/2칸/2칸**(목록 폭 500 / 920 / 871 —
      U-02·U-03과 같은 수치). 세로에서 안 넘쳤다(카드 3장이라 U-03보다 짧다). 없는 글리프 0개,
      콘솔 에러·예외 0. 출전 → 연출 → 결과 → 닫기 흐름을 실제로 다 눌러 봤다: 코스1 출전 시
      6줄 이름이 "나/상대 1~5"로 채워지고 막대가 서로 다른 속도로 차고, 결과에 "1위 나 — 29.2초"
      부터 6위까지와 우승 보상 문구가 뜬다. result-view의 `close-button`은 entry-view로 돌아가고,
      `entry-close-button`은 패널을 완전히 닫고, HUD "레이스"로 다시 열린다 — 셋 다 확인.
      **눈으로 봐야 잡히는 버그 두 개를 찾아서 고쳤다(이 커밋에 포함).**
      1. `RaceEntryUgui.ShowResultView`가 `_animView`를 끄지 않아서 **결과 글자 위에 연출 막대가
         그대로 겹쳐** 보였다(제목도 "레이스 결과"와 "레이스 진행 중"이 겹쳐 "레이스 결함 중"으로
         읽혔다). `ShowAnimView`/`ShowEntryView`는 나머지 둘을 다 끄는데 여기만 빠져 있었다.
         뷰 세 개가 같은 자리를 겹쳐 쓰는 구조에서는 전환 함수마다 **나머지 전부**를 꺼야 한다.
      2. 연출 막대가 24px가 아니라 **130px로 부풀어** 여섯 줄이 화면을 가득 메웠다. 원인은
         `LayoutElement.flexibleHeight`의 기본값 -1이 "무시"라서, `LayoutUtility`가 우선순위가
         높은 `LayoutElement`를 건너뛰고 `HorizontalLayoutGroup`이 보고하는 flexibleHeight
         (`childForceExpandHeight = true`면 1 이상)를 쓰기 때문이다 — 그러면 부모
         `VerticalLayoutGroup`이 남은 높이를 여섯 줄에 나눠 준다. `flexibleHeight = 0f`를 못 박고
         행의 `childForceExpandHeight`를 false로 바꿔서 24px/트랙 14px로 되돌렸다.
         **이건 이 화면만의 문제가 아닐 수 있다** — `BootstrapLootBoxUgui.cs:102`,
         `BootstrapSettingsUgui.cs:106`·`129`, `BootstrapHudUgui.cs:101`·`149`에 같은
         `childForceExpandHeight = true`가 있다. U-05·U-06 배선하는 세션이 막대·줄 높이를
         꼭 눈으로 확인할 것.
- [x] U-05 (2026-09-16 야간 코드 → 2026-09-16 저녁 Unity 세션에서 씬 배선까지 완료). `Assets/Scripts/UI/LootBoxUgui.cs`
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
      **배선 결과(2026-09-16 저녁 Unity 세션)**: `GemRacer/19` 실행 → `MainHudUgui.boxPanel`에
      `LootBox` 연결 → 씬 저장까지 마쳤다. 세 기준점 모두 Play로 직접 봤다 — 세로 540×960은
      1칸, 가로 960×540과 태블릿 1280×800은 2칸(셋째 줄이 아래로 내려감), **어디서도 결과
      카드·닫기 버튼이 잘리지 않아 `ScrollRect`는 필요 없었다**(세로에서 목록 칸 748px에 내용
      444px). 한글 다 나오고(TMP Pretendard), 보유 0개인 강철·티타늄은 `interactable = false`로
      회색, 녹슨 상자(보유 2개)를 열었더니 "녹슨 상자 개봉: C등급 → C등급 곡괭이 부품 +1"이
      결과 카드에 뜨고 보유 표시가 1개로 줄었다. 닫기 버튼(영구 리스너 1개)으로 닫히고 HUD
      "상자" 버튼으로 다시 열린다. Play 중 예외 0.
      **걸린 것 하나**: 예고된 대로 `result-card`가 72px가 아니라 **224px로 부풀었다**. U-04와
      같은 원인(`LayoutElement.flexibleHeight` 기본값 -1) — `BootstrapLootBoxUgui.cs`의
      `result-card`·`close-button`과 `MakeHeaderText`/`MakeButton` 헬퍼에 `flexibleHeight = 0f`를
      못 박아 고쳤다. 헬퍼에 못 박았으니 이 파일로 만드는 줄은 앞으로 다 안전하다.
      `resultPad.childForceExpandHeight`는 라벨을 카드 안에서 세로 가운데로 두려고 true로 남겼다 —
      카드 쪽 `flexibleHeight`를 못 박았으면 그룹이 보고하는 값은 더 이상 쓰이지 않는다.
- [x] U-06 (2026-09-16 야간 코드 → 저녁 Unity 세션 배선) `Assets/Scripts/UI/SettingsUgui.cs`
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
      **배선 결과(2026-09-16 21:20 Unity 세션)**: `GemRacer/20` 실행 → `MainHudUgui.settingsPanel`에
      `Settings` 연결 → 씬 저장까지 마쳤다. 세 기준점 모두 Play로 직접 봤고 **어디서도 잘리지 않았다** —
      내용 높이는 세 곳 다 302px(제목 32 + 세 줄 44/44/190)이고, 세로 540×960은 아래로 440px,
      가로 960×540도 닫기 버튼(y 20~64)까지 여유, 태블릿 1280×800은 내용 폭이 871px로 늘어날 뿐
      줄 수는 그대로다. **`ScrollRect` 없이 시작한 판단이 맞았다.** 예고됐던
      `childForceExpandHeight`(`BootstrapSettingsUgui.cs:106`·`129`) 문제도 이번엔 안 터졌다 —
      U-04·U-05와 달리 세로 그룹이 남은 높이를 나눠 줄 자리가 없었기 때문이다(줄 높이 합이
      가용 높이보다 훨씬 작다). 손댈 것 없어서 그대로 뒀다.
      동작도 다 확인했다: 소리 버튼 → 라벨이 켜짐↔꺼짐으로 바뀌고 `AudioListener.volume`이 1↔0,
      프레임 30/60 → `Application.targetFrameRate`가 실제로 바뀌고 선택된 쪽만 밝은 파랑,
      피드백 칸에 세 줄을 넣고 "저장" → 칸이 비고 "저장됐어요 (클립보드에도 복사됨)"가 초록으로 뜬다
      (`TMP_InputField`는 `MultiLineNewline`, viewport/textComponent/placeholder 다 물려 있다),
      닫기(영구 리스너 `UiPanel.Hide` 1개) → 닫히고 HUD "설정" 버튼으로 다시 열린다. 한글 다 나오고
      Play 중 예외 0. 테스트로 남은 피드백 파일(`persistentDataPath/feedback.txt`)은 지웠다.
      **`Refresh()`가 `Update()`에 있어서 라벨·색은 누른 다음 프레임에 바뀐다** — 같은 프레임에
      읽으면 안 바뀐 것처럼 보인다. 다음에 이 화면을 검사할 때 헷갈리지 말 것(버그 아니다).
- [x] U-07 (2026-09-16 야간 코드 / 같은 날 23시 Unity 세션에서 배선·확인 완료) `Assets/Scripts/UI/OfflineRewardUgui.cs`
      (`OfflineRewardPanel.cs`와 조회·표시 로직 동일, `UiKit.Find`로 이름 조회만 바꿈) +
      `Assets/Editor/BootstrapOfflineRewardUgui.cs`(메뉴 `GemRacer/21`) 신규. 다른 오버레이(U-02~U-06)와
      다른 점 하나 — 이 화면은 HUD 버튼이 아니라 보상 유무로 스스로 열리고 닫혀서 `MainHudUgui`에
      물릴 필드가 없다. 그래서 구조도 한 겹 다르다: 루트 `OfflineReward`는 항상 켜 둔 채로
      스크립트만 붙이고(꺼 버리면 Update가 멈춘다), 실제 화면(반투명 배경+가운데 카드)은 자식
      `offline-reward-backdrop` 하나로 묶어 그 GameObject만 스크립트가 여닫는다 — `TutorialUgui`의
      말풍선과 같은 요령. 카드는 원래 UXML의 "80%, 최대 420px"를 고정폭 380px로 단순화(anchor를
      중앙 한 점에 고정 + `ContentSizeFitter`로 높이만 내용에 맞춤) — 세 기준점 폭(500/920/871)
      모두 380px보다 넉넉히 넓어 재배치가 필요 없다. 닫기 버튼은 안 넣었다 — "받기"가 곧 닫는
      동작이라 U-02~U-06에서 반복된 구멍(ugui-migration.md 3-1번)이 애초에 없다.
      **줄 높이는 손계산 — Editor에서 실제로 봐야 한다.** `wasted-label`/`treasure-label`처럼
      길어질 수 있는 문장은 줄바꿈을 켜고 40px로 넉넉히 잡았지만, U-03이 손계산으로는 괜찮아
      보였다가 실제로는 넘쳤던 전례가 있다. Core는 안 건드려서 `dotnet run` 생략(코어 변경 없음).
      **배선 결과(2026-09-16 23시 Unity 세션)**: `GemRacer/21` 실행 → `UI Canvas/Overlays/OfflineReward`
      생성(HUD 필드에 물릴 것 없음, 예상대로) → 씬 저장. 세이브(`persistentDataPath/save.json`)의
      `LastSeenUnixSeconds`를 6시간 전으로, `RawMinerals`를 0으로 돌려 자리 비움을 흉내 내고 Play로 확인했다.
      카드가 스스로 떴고 Play 중 예외 0. 라벨 다 나온다 — "자리를 비운 시간: 6.0시간 / 인정된 시간:
      4.0시간 / 화물칸이 넘쳐 버린 시간: 2.0시간 / 획득 원석: 1290.4 / 획득 정제 광물: 48.0 /
      발견한 보물 16개(그중 지금 캘 수 있는 것 6개)", 한글 다 보인다. **손계산으로 걱정했던 줄 높이는
      실제로 안 넘쳤다** — `wasted-label`·`treasure-label` 둘 다 잡아 둔 40px 안에서 한 줄(실제
      필요 높이 17px)로 끝났고, 카드 전체가 380×440으로 잡혔다. 세 기준점 모두 여유가 있다
      (세로 540×960, 가로는 CanvasScaler 배율 1.0이라 960×540 안에 440 → 위아래 50px씩,
      태블릿 1280×800은 논리 크기 911×569 → 440이 들어간다). "받기"를 눌렀더니 원석 9.1→1290.4,
      정제 0.0→48.0으로 실제로 들어오고 `PendingOfflineReward`가 null이 되면서 다음 프레임에
      카드가 스스로 닫혔다. "광고 보고 2배 받기"는 오늘 한도가 3회 남아 있어서 보였다
      (한도 0일 때 접히는 건 이번에 못 봄 — 코드상 `RemainingRewardAdsToday`로 갈리고
      U-05·U-06에서 같은 패턴을 확인했다). 확인 뒤 세이브는 원래 파일로 되돌려 놨다.
- [x] U-10 (2026-09-16 야간 코드 → 2026-09-17 01시 Unity 세션 배선) `Assets/Scripts/UI/ShopUgui.cs`
      (`ShopPanel.cs`와 조회·표시·디버그 구매 로직 동일, `UiKit.Find`로 이름 조회만 바꿈) +
      `Assets/Editor/BootstrapShopUgui.cs`(메뉴 `GemRacer/22`) 신규. 아홉 줄(스타터 팩·화물칸
      확장 3단계·오프라인 상한 연장·채굴 가속 패스·행성 통행증 구독·Steam 서포터 팩·광고 제거)이라
      U-03(다섯 줄도 넘쳤다)보다 훨씬 크다 — 처음부터 `BootstrapCraftingUgui`와 같은 ScrollRect
      구조로 만들었다. 줄 자체는 `BootstrapLootBoxUgui`처럼 이름/상태/버튼 하나 구조(Crafting처럼
      버튼 두 개가 아니다). 닫기 버튼은 스크롤 바깥.
      **덤으로 발견한 것 — HUD에 "상점" 버튼이 아예 없었다.** 옛 `MainHud.cs`(UI Toolkit)에는
      `btn-shop`이 있었는데 `BootstrapHudUgui`(uGUI 이사, D08)로 옮기면서 다섯 개(업그레이드·
      제작·레이스·상자·설정)만 옮겨지고 빠졌다. `MainHudUgui.cs`에 `shopPanel` 필드 + `Wire`
      호출을 추가했지만(새 필드 추가라 기존 배선은 안 건드린다), 실제 버튼은 씬에 아직 없다 —
      **`BootstrapHudUgui.Build`(`GemRacer/13`)를 다시 누르면 절대 안 된다.** 그 메뉴는
      "UI Canvas"를 통째로 지우고 다시 만드는데, 그 밑 `Overlays`에 U-02~U-09가 배선해 둔 화면
      전부와 `MainHudUgui`의 패널 연결(craftPanel 등, 씬에만 있는 데이터)이 같이 날아간다.
      대신 `action-row`만 찾아 `btn-shop` 하나를 추가하는 애처블 메뉴
      `BootstrapShopUgui.AddShopButtonToActionRow`(`GemRacer/23`)를 새로 만들었다 — 이건
      다시 눌러도 그 버튼만 지우고 새로 만들어서 안전하다(멱등). `BootstrapHudUgui.BuildActionRow`
      코드 자체에도 btn-shop을 추가해 뒀다(나중에 씬을 완전히 새로 세울 일이 생기면 그때는
      한 번에 여섯 개가 나오게). Core는 안 건드려서 `dotnet run` 생략(코어 변경 없음).
      **배선 결과(2026-09-17 01시 Unity 세션)**: `GemRacer/22` 실행 → `UI Canvas/Overlays/Shop`
      생성(shop-title / scroll-view / close-button) → `GemRacer/23` 실행 → `action-row`에
      `btn-shop`이 여섯 번째로 붙음(앞의 다섯 개와 `Overlays` 밑 일곱 화면 그대로, 3-4의 걱정대로
      되지 않았다) → `MainHudUgui.shopPanel`에 `Shop` 연결 → 씬 저장. Play 중 예외 0.
      "상점" 버튼이 켜진 채로 나왔고 눌러서 열렸다. **아홉 줄 다 한글이 나오고 줄 높이도 안 넘쳤다**
      — 이름 24px(실제 20.3), 상태 20px(16.7), 버튼 라벨 44px(16.7)로 셋 다 여유가 있다.
      U-03에서 넘쳤던 것과 달리 처음부터 ScrollRect라 줄이 아홉이어도 문제가 안 된다
      (540×960에 여섯 줄이 보이고 나머지는 스크롤, "닫기"는 스크롤 바깥에 고정).
      "스타터 팩" 구매를 눌렀더니 "미보유"→"보유 중"으로 바뀌고 **"화물칸 확장 1단계"도 같이
      "보유 중"이 됐다** — 버그가 아니라 `ShopPurchase.Apply`가 `StarterPack`에서
      `CargoExpansionLevel`을 1로 올리기 때문이다(monetization.md 2-1 그대로). "닫기"도 닫힌다.
      **한 가지 눈에 걸린 것** — Play 중 뒤에 오프라인 보상 카드가 떠 있었는데 상점 스크림 사이로
      그 글자가 비쳐 보였다. 읽는 데 지장은 없지만 다른 패널보다 스크림이 옅은지 한 번 볼 만하다.
      **아홉 개 버튼을 다 눌러 봤다** — 전부 `DebugPurchase`가 먹는다. 스타터 팩·화물칸 1~3단계·
      Steam 서포터 팩은 "보유 중", 오프라인 상한 연장은 "구매함", 가속 패스와 통행증 구독은
      "활성 (30일 남음)", 광고 제거는 "제거됨"으로 바뀐다. 아홉 번 누르는 동안 예외 0.
      스크롤은 viewport 828 / content 1356이라 확실히 스크롤되고, "닫기"(44px)와 제목(32px)은
      스크롤 바깥이라 항상 보인다. 가로 960×540이면 viewport가 408로 줄 뿐 구조는 그대로다.
      **여기서 하나 고쳤다 — 여섯 번째 버튼이 붙으면서 "업그레이드"가 잘렸다.** action-row는
      HorizontalLayoutGroup으로 폭을 등분하는데 다섯 칸(96.8px)이 여섯 칸(79.3px)이 되면서
      20pt "업그레이드"(86.4px 필요)가 말줄임으로 "업그레이…"가 됐다. 라벨마다 폭을 재서
      맞추는 대신 `BootstrapHudUgui.MakeButton`과 `BootstrapShopUgui.AddShopButtonToActionRow`
      양쪽에 TMP 자동 축소(`enableAutoSizing`, 14~20pt)를 켰다 — 버튼이 더 늘거나 라벨이
      길어져도 잘리는 대신 줄어든다. `GemRacer/23`은 이미 있던 다섯 라벨도 같은 설정으로
      맞춰 주니 다시 눌러도 안전하다(멱등). 지금 "업그레이드"는 18.4pt로 자동으로 줄어 다 보이고
      나머지 다섯은 20pt 그대로다.
- [ ] U-08 다 옮기고 나면 — MainGame 씬에서 꺼 둔 UI Toolkit 루트를 지우고,
      옛 패널 스크립트·UXML·USS·PanelSettings·테마를 지운다. 그 전에는 지우지 않는다
      **→ 2026-09-17 19시 Unity 세션 정정: 꺼 둔 루트는 여덟 개가 아니라 열 개다**
      (Cargo Full / Settings / Tutorial / HUD / Upgrade / Shop / Crafting / Offline Reward /
      Race / Loot Box). 그중 화물칸 가득 화면만 옮긴 적이 없어서, "일곱 개가 다 끝났으니
      이제 지우면 된다"고 판단하고 지웠다면 **M-04 화면이 그대로 사라질 뻔했다.**
      U-11로 옮겼으니 이제 열 개 다 대체본이 있다 — 다만 **dev 주소에서 화물칸 화면을
      한 번 보고 나서** 지운다. 지우는 순서: ① 씬의 `UI Root (*)` 열 개 ②
      `Assets/Scripts/UI/*Panel.cs` 아홉 개(+`MainHud.cs`) ③ `Assets/UI/*.uxml`/`*.uss` 열 벌
      ④ `BootstrapMainGame.cs`의 UIDocument 생성부 ⑤ PanelSettings·UnityDefaultRuntimeTheme.
      ④를 빼먹으면 `GemRacer/7`이 없는 UXML을 찾다 에러를 낸다
      **→ 2026-09-18 07시 배선 세션: 관문 풀렸다.** U-11이 요구한 "dev 주소에서 화물칸 화면을
      한 번 보고" 오는 것을 이번 세션이 해 왔다(위 U-11 참고). 이제 U-08은 지우는 일만 남았다 —
      다만 다섯 단계를 한 세션에 다 하지 말고 ①~⑤ 순서대로, 지울 때마다 `GemRacer/7`을 다시
      돌려 에러 0을 보고 나서 다음으로 갈 것
      **→ 2026-09-20 08시 코딩 세션 — 위 "지울 때마다 GemRacer/7을 다시 돌려"는 이제 틀린
      지시다.** CLAUDE.md의 "`GemRacer/7`을 다시 누르면 손으로 배선한 uGUI 화면이 전부
      날아간다"(2026-09-18 저녁)가 이 문단보다 나중에 적혔고, `GemRacer/7`은 빈 씬에서
      새로 만드는 메뉴라 지금 배선된 MainGame 씬에 다시 돌리면 이 U-08이 지우려는 열 개는
      물론 이미 배선된 uGUI 화면 일곱 개까지 통째로 사라진다. **①~⑤를 진행할 때 에러
      확인은 `GemRacer/7` 재실행이 아니라, 씬을 연 채로 지우고 저장한 뒤 `refresh_unity`+
      `read_console`로 컴파일 에러만 본다.** (위 B-01 항목이 이 열 개 UIDocument를 원인으로
      지목했으니, U-08을 마치면서 B-01도 같이 없어졌는지 확인할 것.)
- [x] U-11 (2026-09-17 19시 Unity 세션 이사 + 2026-09-18 07시 Unity 배선 세션 웹 확인) **화물칸 가득 화면(M-04)을 uGUI로 옮겼다 — 이 화면만
      U-01~U-10에서 빠져 있었다.** 옛 루트는 꺼져 있고 uGUI 대체본은 없어서, 지금 배포된
      빌드에서는 M-04("정제로 돌리시겠어요?")도, 그 안의 M-08 스타터 팩 제안도,
      M-09 후속 "광고 보고 1시간 상한 2배"도 **아무것도 안 떴다.** 셋 다 코드는 멀쩡히
      살아 있었고(`MiningController.CargoJustFilled`/`ShouldShowStarterPackOffer`/
      `CargoCapDoubleHourRemainingSeconds`) 그걸 읽는 화면만 없었다.
      `Assets/Scripts/UI/CargoFullUgui.cs` + `Assets/Editor/BootstrapCargoFullUgui.cs`
      (메뉴 `GemRacer/24`) 신규. 로직은 `CargoFullPanel.cs` 그대로 옮겼고 바뀐 건 조회·표시뿐이다.
      구조는 OfflineReward(메뉴 21)와 같다 — 루트는 항상 켜 두고 `cargo-full-backdrop`만 여닫는다.
      **형제 순서 주의**: uGUI는 형제 순서가 곧 그리는 순서라 맨 뒤에 붙이면 상점 위에 그려져서
      "상점 보기"를 눌러도 상점이 뒤에 가린다 — 부트스트랩이 Shop 바로 앞(인덱스 7)에 넣는다.
      옛 UI Toolkit 시절 `BootstrapMainGame`이 상점 sortingOrder를 21로 올려 둔 것과 같은 이유다.
      Play로 확인: 긴 문구(제련소 0)·스타터 팩 칸까지 카드가 늘어나고, "상점 보기"가 상점을
      위에 띄운다. 콘솔 예외 0, `Core.Tests` 200/200.
      **웹 확인 완료(2026-09-18 07시 배선 세션)**: `dev.planetracer-daz.pages.dev/?fast=100`으로
      새 세이브부터 화물칸을 실제로 채웠다 — `DebugTimeScale`의 100배속이면 4시간치가 2분 반이라
      한 세션 안에 끝난다(이 방법은 앞으로 웹에서 방치형 화면을 확인할 때 계속 쓸 것).
      원석 760.8에서 채굴이 멈추고 M-04 카드가 떴다: "정제로 돌리시겠어요?" 본문 + 레이스 나가기·
      닫기·상점 보기 + M-09 "광고 보고 1시간 상한 2배(오늘 2회 남음)" + M-08 스타터 팩 제안까지
      **다섯 덩어리 전부** 나왔다. 한글 다 나오고(없는 글리프 0) 잘린 글자·넘친 칸 없음.
      **형제 순서도 실제로 확인됐다** — "상점 보기"를 누르면 상점이 카드 **위에** 뜨고,
      상점을 닫으면 카드로 돌아오고, 카드의 "닫기"로 HUD까지 돌아온다.
      덤: 카드가 뜬 뒤에도 원석 숫자가 760.8에서 **한 자리도 안 늘었다** — M-01의 온라인 상한
      클램프가 배포판에서 실제로 동작한다는 뜻이다(지금까지 테스트로만 확인했던 것).
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
  (`docs/decisions.md` 2026-09-15 항목), `BonusFuelCapacity`는 core 시그니처 쪽만 2026-09-19
  야간 세션이 먼저 열어 뒀다(`RaceFuel.Recover`에 `maxFuel` 4인자 오버로드 추가, 기존 3인자
  호출은 동작 그대로) — `MiningController.cs`/`RaceEntryUgui.cs`가 실제로 그 오버로드를 불러
  유효 최대치를 쓰게 바꾸는 건 여전히 Unity 세션 몫. `AutoRefineryAlwaysOn`·`DailyRefinedMineralsGrant`는
  아직 미배선 — 전부 decisions.md에 정리해 둠.
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
- [x] M-11 (2026-09-16 야간 코어 → 2026-09-18 05시 Unity 세션에서 화면 적용까지 완료) Steam 판 분기. 코어 `PlatformConfig.cs`
      신규 — `StorePlatform`(Mobile/Steam) enum + `CargoBaseMultiplier(platform)`(Steam만 ×1.5,
      monetization.md 4장) + `IsShopItemAvailable(skuId, platform)`(Steam엔 AdRemoval·
      SeasonPassSubscription 안 보임, SteamSupporterPack은 Steam에만 보임, 나머지 SKU는 둘 다 판매).
      `Entitlements`(구매·구독)와는 완전히 독립된 배율이라 함께 안 건드렸다 — 판 자체가 다른 것이지
      "무엇을 샀는지"가 아니다. `Core.Tests`에 3개 추가, **통과 143 / 실패 0**.
      **왜 여기서 멈췄나** — 실제로 화면에 적용하려면 두 곳을 건드려야 하는데 둘 다 지금은 위험하다.
      1) `MiningController.CargoCapacityMinerals`에 `PlatformConfig.CargoBaseMultiplier`를 곱하려면
      `StorePlatform` 필드(빌드 타깃에 따라 정해질 값)를 어디서 받을지부터 정해야 한다.
      2) 상점 화면(`ShopPanel.cs`/`ShopUgui.cs`)은 `DefaultData.ShopItems()`가 돌려주는 목록의
      **순서(인덱스)와 씬에 미리 만들어 둔 이름 배열("starter-name"~"adremoval-name")이 1:1로
      고정**돼 있다 — SKU를 판별로 걸러 목록에서 빼면 인덱스가 밀려서 엉뚱한 줄에 엉뚱한 값이
      찍힌다. 게다가 `ShopUgui.cs`(U-10)는 아직 씬 배선조차 안 끝난 상태라 지금 손대면 두 가지
      미완성이 겹친다. **남은 것(Unity 세션 몫)**: U-10 씬 배선이 먼저 끝난 뒤, 이름 기반으로
      해당 줄을 감추는 방식(인덱스 재배열이 아니라 `row.gameObject.SetActive(false)` 같은)으로
      두 화면에 적용하고, `MiningController`에 플랫폼 필드를 추가해 `CargoCapacityMinerals` 계산에
      곱한다. 판별 자체(빌드 타깃 → Mobile/Steam)는 아직 안 정해서 그것도 같이 정할 것.
      **배선 결과(2026-09-18 05시 Unity 세션)**: 위 세 가지를 그대로 했다. 막았던 이유가 둘 다
      풀렸다 — U-10 씬 배선은 9/17 01시 세션에서 이미 끝났고, 판별은 아래처럼 정했다.
      1) **판별은 빌드 타깃이 아니라 전용 정의 `GEMRACER_STEAM`으로 정한다**
      (`Assets/Scripts/Mining/GamePlatform.cs` 신규). 빌드 타깃(`UNITY_STANDALONE`)으로 정하면
      에디터는 늘 Standalone이라 Play만 눌러도 Steam 판 화면이 떠서 확인하려던 것과 다른 것을
      보게 된다. Steam 판은 스토어 SDK가 같이 들어가야 성립하는 의도적인 빌드이기도 하다.
      Steam 빌드를 낼 때 Scripting Define Symbols에 `GEMRACER_STEAM`을 넣는다 — 지금 CI에도
      모바일에도 안 넣었으니 **전부 Mobile이고, 이 변경으로 기존 빌드 동작은 하나도 안 바뀐다.**
      2) `MiningController`에 `overrideStorePlatform`/`storePlatformOverride`(인스펙터 확인용) +
      `Platform` 프로퍼티. `CargoCapacityMinerals`에 `PlatformConfig.CargoBaseMultiplier(Platform)`를
      곱했다 — `CargoHours`가 `BaseCargoHours`에 정비례하니 결과에 곱하는 것과 같다.
      3) 두 상점 화면에 `ApplyPlatform` 추가. **인덱스는 하나도 안 건드렸다** — 아홉 줄 배열을
      그대로 두고 `row-{prefix}` 오브젝트만 껐다(uGUI는 `SetActive`, UI Toolkit은 `display:None`).
      끄는 쪽뿐 아니라 켜는 쪽도 같이 써서 몇 번을 돌려도 결과가 같다.
      **에디터에서 실제로 확인**: 컴파일 에러 0, Play 중 예외 0. Mobile일 때 `row-steam`만 숨고
      여덟 줄이 보이며 화물칸 상한 4604.6, override로 Steam으로 바꾸면 `row-season`·`row-adremoval`이
      숨고 `row-steam`이 뜨며 상한 6906.9(**정확히 ×1.500**). 씬은 안 건드렸다(`git status`에
      `.unity` 변경 0건) — 새 필드는 기본값이라 YAML에 쓸 것이 없다.
- [x] M-12 (2026-09-16 야간) 스토어 문구 초안. `docs/design/store-listing.md` 신규 — Steam
  페이지(짧은 설명·상세 설명 첫 문단에 파는 것/안 파는 것 나열·본문 골자·태그 후보)와 모바일
  스토어(짧은 설명·긴 설명·키워드) 둘 다 초안을 썼다. monetization.md 6장의 "첫 문단에 사양
  나열이 최선의 리뷰 방어" 원칙대로 두 문구 모두 첫 문단을 나열로 시작한다. 스크린샷 자리·
  영문 번역·최종 가격은 비워 두고 무엇이 남았는지 문서 끝에 적어 뒀다 — 이 세션이 최종본을
  올리지 않는다. 문서 작업이라 Core.Tests 영향 없음(기존 143/실패 0 그대로).
- [ ] M-13 (2026-09-20 07시 주말 세션 발견) **오프라인 상한 연장(2-3, ₩5,500)이 상점에서 사지는데
      효과가 없다.** `ShopUgui`·`ShopPanel`이 "구매함"으로 표시하고, `ShopPurchase.Apply`가
      `PurchaseState.OfflineCapExtensionPurchased`를 실제로 true로 저장하고, `Entitlements.Effective`가
      `OfflineCapHours`(4h→12h)를 정확히 계산까지 한다 — 그런데 `OfflineCapHours`를 읽는 곳이
      코드 전체에 **단 한 군데도 없다**(`grep -rn "OfflineCapHours" Assets/Scripts`가 선언·계산부
      셋뿐, 소비하는 곳 0). `Entitlements.cs`의 `OfflineCapHours` 필드 주석 자신이 "TODO —
      MiningController 배선은 아직 안 됨"이라고 이미 적어 뒀던 것 — 이번 세션이 실제로 grep으로
      확인해 살아있는 gap임을 검증했다. 지금 상태로 이 상품이 출시되면 **돈 받고 아무 효과도
      안 주는 상품**이 되니 상점을 실제로 붙이기 전에 반드시 막아야 한다.
      **배선을 미룬 이유(설계 판단이 필요해서 이번 세션은 손대지 않았다)**: `MiningController.
      ComputeOfflineReward`는 지금 `elapsedSeconds`(오프라인 경과)에 아무 상한도 안 건다 —
      원석은 `MiningSimulator.Offline`의 화물칸 계산이 이미 자연스럽게 막고, 정제 광물
      (`RefinedGained`)은 M-02 설계대로 시간에 정비례해서 **경과 시간과 무관하게 무한정** 늘어난다
      (`refined = refineRate * hours`, hours가 얼마든 안 잘림). `OfflineCapHours`를 문자 그대로
      "그 시간 넘게 지나면 나머지는 안 쳐준다"로 구현하면 지금은 무제한인 정제 광물 오프라인
      수익에 새 상한이 생겨 **버프 상품이 아니라 기존 무료 유저 혜택을 깎는 상품**이 될 위험이
      있다 — `Entitlements.cs` 주석의 "Math.Max(화물칸 상한, 이 값)"도 "이 값이 기존 상한보다
      낮아도 안 깎는다"는 방향이라 같은 우려를 가리키고 있다. 반대로 그대로 두면(정제는 원래
      무제한이니 캡을 안 걸어도 된다로 결론) 이 상품은 원석·보물 발견 쪽에만 의미가 있는데,
      그 둘은 이미 화물칸 상한이나 `HoursCounted`로 사실상 막혀 있어 실제 체감 효과가 거의
      없을 수 있다 — **이 상품이 정확히 무엇을 몇 시간까지 더 인정해 줘야 하는지"를 Tifania가
      한 줄로 정해 주면(예: "정제 광물도 그 시간 넘으면 그만 쌓는다" vs "지금 무제한 그대로 두고
      이 상품은 보물 발견 쪽만 늘린다") 코어 함수 하나 고치고 MiningController에서 부르는
      선에서 끝나는 작은 일이다.** `docs/decisions.md`에 선택지로 올려 둘 것.
- [x] T-05 (9/12 오전 확인) GitHub Actions 실행 기록으로 확인 — main 브랜치 W-02 커밋들의 빌드+Cloudflare 배포가 실제로 성공했다(9/11, run #6·#8·#9). 다섯 비밀값과 Pages 프로젝트가 전부 정상 등록돼 있다는 뜻. 에디터로 직접 열어 본 건 아니라서 이상 있으면 다시 `- [ ]`로

## 반응형 레이아웃·웹 배포 (2026-09-11 추가)

- [x] W-01 GitHub Actions WebGL 빌드 + Cloudflare Pages 배포 구성, web/_headers, tools/deploy_web.ps1, WebGLBuild.cs (9/11)
- [x] W-02 첫 배포 성공 (9/11). https://planetracer-daz.pages.dev — 빌드 28분, 결과 14MB. 막혔던 두 곳은 Pages 프로젝트 부재와 root 소유 폴더 권한이었고 둘 다 워크플로에 단계를 추가해 해결
- [x] W-03 UI Toolkit 반응형 골격. `Assets/UI/Root.uxml`+`Root.uss`(3D 뷰 자리 + HUD 자리, 상태바, 버튼 3개) + `Assets/Scripts/UI/ResponsiveLayout.cs`(폭<높이면 "portrait", 아니면 "landscape" 클래스를 루트에 붙임 — 미디어 쿼리 대신). 태블릿(1280x800)도 가로라 landscape 규칙을 그대로 탄다, 즉 두 클래스로 세 기준점 다 커버. `GemRacer/5. 반응형 UI 테스트 씬 만들기`로 확인.
- [x] W-04 (9/13 오후) 가로 화면 3D 뷰 비율 조정 — 사실상 "진짜 게임 화면이 생기면 마무리" 하기로 미뤄 둔 항목이었는데, D04(MiningController)·D05(업그레이드 패널)가 각자 다른 안 만들어진 씬(TestPlanet/ResponsiveUITest/UpgradeTest — 셋 다 Unity 에디터가 있어야 부트스트랩이 돌아서 실제로는 하나도 저장된 적이 없었다)에 흩어져 있던 걸 발견했다. `Assets/Editor/BootstrapMainGame.cs`(`GemRacer/7. 메인 게임 씬 만들기`)로 하나로 합침 — 3D 채굴(행성+채굴차+카메라) 위에 Root.uxml HUD와 업그레이드 패널을 얹는다. `Assets/Scripts/UI/MainHud.cs`가 viewport-area에 `.live` 클래스를 붙여 자리 표시자 배경/문구를 지우면 뒤의 실제 카메라가 그대로 보인다(Root.uss에 `.viewport-area.live` 추가) — Root.uxml/Root.uss 자체는 그대로 둬서 ResponsiveUITest 씬은 여전히 자리 표시자를 쓴다. HUD의 "채굴" 버튼은 실제 채굴은 이미 자동이라 할 일이 없어서 "업그레이드" 패널을 여닫는 용도로 재활용, "제작"/"레이스"는 화면이 없어(D08/D09) 비활성화. 이 씬을 Build Settings 0번으로 등록해서 다음 웹 배포부터 시작 화면이 RaceCameraSpike(실험용)에서 이걸로 바뀐다. **덤으로 버그 발견·수정**: `MiningController`가 `SurfaceMover.speed`를 고정값(3)에 묶어 놔서 엔진을 업그레이드해도(코어 `RigSpeed`는 실제로 올라감) 화면상 채굴차는 그대로 느리게 돌고 있었다 — 매 프레임 코어 `RigSpeed`로 덮어쓰게 고침. 화물칸 게이지(Root.uxml에 `cargo-gauge-track`/`-fill` 추가, `MiningController.CargoCapacityMinerals` 신규)도 같이 붙였다 — 단 이건 표시용일 뿐 실시간 채굴 자체를 상한에서 멈추진 않는다(오프라인 캐치업에만 상한 적용 중), 접속 중에도 막을지는 미정이라 아래 "막힌 것"에 남김. **컴파일 확인 완료** (run #20, 9/13 오후): 이 커밋의 webgl 빌드가 실제로 성공했다 — `BootstrapMainGame.cs`/`MainHud.cs`/`MiningController.cs` 변경분 전부 Unity가 실제로 컴파일했다는 뜻. 다만 Build Settings는 커밋된 `EditorBuildSettings.asset`을 CI가 그대로 쓸 뿐이라(내가 손으로 안 건드림), 이 부트스트랩 메뉴를 실제로 눌러 씬을 만들고 커밋하기 전까지는 웹 시작 화면이 여전히 RaceCameraSpike 그대로다 — Play 모드 동작(버튼 눌림·게이지 채워짐 등)도 여전히 눈으로 봐야 한다.
- [ ] W-05 세 기준점 스크린샷을 자동으로 찍어 daily 파일에 붙이는 에디터 스크립트. 매번 눈으로 세 번 확인하지 않게. (9/13 오후: `GameViewSizes` 등 관련 API가 비공개/불확실해서 이번 세션엔 손 안 댐 — Unity 에디터로 실제 확인하면서 짜는 게 나을 것 같다)
      **막힌 부분이 풀렸다(2026-09-16 03:20 Unity 세션에서 실제로 써 봄).** 비공개 `GameViewSizes`를
      건드릴 필요가 없다 — `UnityEditor.PlayModeWindow.SetCustomRenderingResolution(uint w, uint h, string 이름)`이
      **공개 API**이고, Play 중에 불러도 먹는다. 이번에 이걸로 540×960 → 960×540 → 1280×800을
      차례로 바꿔 가며 U-04를 세 기준점 다 확인했다. 바꾼 뒤 `Screen.width`가 곧바로 갱신되지는
      않으니 **4~5초 기다린 다음** 측정·촬영해야 한다. 끝나면 540×960으로 되돌려 둘 것.
      **촬영은 `ScreenCapture.CaptureScreenshot(절대경로)`를 써야 한다** — Unity MCP의
      `manage_camera(screenshot)`는 카메라를 지정하지 않아도 Main Camera 경로로 찍어서
      Screen Space - Overlay 캔버스(= 우리 UI 전부)가 **안 찍힌다**(3D 배경만 나온다).
      `CaptureScreenshot`은 프레임 끝에 비동기로 파일을 쓰니 호출 후 3~4초 기다렸다가 읽는다.
- [x] W-06 (9/13 오후) WebGL 첫 로딩 시간 측정. `tools/measure_web_load.js`(신규, 외부 의존성 없음) — 배포된 Build 파일들의 실제 Content-Length를 재서 대역폭 구간별(LTE 약함 3Mbps/보통 8Mbps/좋음 25Mbps) 다운로드 시간을 계산하고 10초 예산과 비교한다. `.github/workflows/webgl.yml`의 "배포 확인" 다음 단계로 넣어서 **이제 매 배포마다 자동으로 잰다**(continue-on-error — 지금은 예산 초과가 빌드를 막진 않음). 이 클라우드 세션 자체는 아웃바운드 네트워크 정책상 `*.pages.dev`에 못 나가서(403) 직접 실행해 확인은 못 했지만, **push 직후 run #20 Actions 로그로 실측 확인 완료**: 실제 배포(`https://51b6c2f6.planetracer-daz.pages.dev`)에서 wasm 8.09MB + data 5.62MB + framework 0.07MB, 합계 **13.78MB**. 대역폭별 다운로드 시간 — **LTE 약함(3Mbps) 36.8초, LTE 보통(8Mbps) 13.8초로 10초 예산 초과, LTE/5G 좋음(25Mbps)만 4.4초로 통과**(9/11 기록으로 미리 해 둔 손계산 38초/14초/4.5초와 거의 일치). 다운로드 시간만 잰 것이라 파싱·초기화까지 더하면 실제 체감은 더 나쁠 것. 예산을 계속 넘기면 에셋을 줄이는 작업이 필요해 별도 항목으로 남김(아래 W-09).
- [ ] W-09 (9/13 오후 신설) 에셋 크기 줄이기. W-06 실측 결과 LTE 약함·보통 구간(국내 LTE 이용자 상당수가 해당할 대역)에서 10초 예산을 이미 넘긴다(wasm 8.09MB + data 5.62MB, 압축 후로 이미 이 정도). Unity WebGL 압축 레벨·텍스처 포맷·Code Stripping(IL2CPP) 옵션부터 볼 것. 급하진 않지만(지금 볼 화면 자체가 아직 적어서 실제 wasm/data가 더 커질 여지도 있다) 화면이 늘어나기 전에 예산을 벌어 두는 게 나을 것
  - (주말 매시간 세션 검토만) `WebGLBuild.cs`를 보니 압축(Brotli)·예외 지원 끔·IL2CPP Master는 이미 되어 있다.
    남은 손잡이는 Managed Stripping Level과 텍스처 포맷인데, 전자는 `SaveService`가 JsonUtility로
    리플렉션 직렬화를 쓰고 있어 레벨을 올렸을 때 필드가 잘려 세이브가 깨질 위험이 있고, 후자는
    브라우저/기기별 압축 텍스처 지원이 갈려서(모바일 실제 지원 포맷이 데스크톱과 다름) 잘못 고르면
    Tifania가 아침에 여는 화면에서 바로 티가 나는 정도의 회귀(텍스처 깨짐)가 될 수 있다 — 둘 다
    Unity 에디터로 켜 보고 확인해야 안전해서 이번 클라우드 세션은 손 안 대고 넘김. Unity 켤 때 먼저
    Managed Stripping Level=Medium으로 시험 빌드 → 세이브/불러오기 되는지 확인부터 하는 게 안전할 듯.
  - **(2026-09-18 03시 Unity 배선 세션) 텍스처 쪽 큰 구멍 하나를 A-14 하다가 발견해서 같이 막았다.**
    아이콘·행성 원본이 **1254x1254**인데 1254가 4로 안 나눠떨어져서(NPOT) 블록 압축이 아예 안 걸리고
    전부 `RGBA32` **무압축**으로 들어가고 있었다 — 한 장 6.1MB, 24장이면 145MB다. 게다가 이 그림들은
    `Assets/Resources/` 밑이라 **참조가 하나도 없어도 빌드에 통째로 들어간다**(Resources 폴더 규칙).
    `Resources/Art` 전체 런타임 크기 **187MB → 27MB**. 아이콘 max 256, 행성 max 512로 내리고
    압축을 켠 결과다(둘 다 4의 배수라 이제 `DXT5`로 압축된다). 기준 해상도가 540x960이라
    아이콘이 화면에 100px 남짓으로 뜨는 걸 생각하면 256도 넉넉하다 — 눈으로 나빠질 자리가 아니다.
    지금은 쓰는 화면이 없어서 회귀 위험 0(참조 0건 확인하고 바꿨다).
    **남은 것**: (1) 이게 실제 `.data` 크기를 얼마나 줄였는지는 다음 WebGL 빌드에서 재 볼 것 —
    위 숫자는 에디터 플랫폼(Standalone) 기준이라 WebGL 실측과 다를 수 있다.
    (2) `rig-tiers-sheet.png`(1536x1024, DXT5 3MB)는 세 티어가 한 장에 든 시트라 나중에 잘라 쓸 때
    크기를 정하는 게 맞을 것 같아 그대로 뒀다. (3) 컷신 7장은 T-11 대기라 안 건드렸다.
    (4) 위에 적힌 Managed Stripping Level 건은 여전히 안 건드렸다.
- [x] W-12 (2026-09-18 07시 배선 세션 발견 → 2026-09-18 21시 Unity 배선 세션 종결)
      **웹에서 켜질 때 URP가 렌더 에러를 108번 토한다.** `dev` 주소로 게임을 열면 로딩 직후
      `Render Graph Execution error` + `ArgumentException: RenderTextureDesc width must be
      greater than zero (desc.width)`가 108번 몰아서 찍혔다.
  → **결론: 게임 버그가 아니다. 자동화 창이 폭 0으로 뜬 탓이다 — 항목을 닫는다.**
    21시 세션이 PC 브라우저로 세 가지를 나눠서 확인했다.
    ① **폭 0으로 시작하면 재현된다.** 창이 숨겨진 채로 열면 `innerWidth/innerHeight`가 `0, 0`이고
      캔버스가 `1x1`(clientWidth 0)인데, 이 상태로 로딩하면 에러가 그대로 쏟아진다. 07시 세션이
      본 108번이 정확히 이 경우다. 원인 추정("캔버스 폭이 0인 순간")은 맞았지만, 그 순간을
      만든 것이 게임이 아니라 자동화 창이었다.
    ② **제대로 된 크기로 시작하면 0번이다.** 로딩 *전에* 뷰포트를 1024x768로 고정해 두고 열면
      `Render Graph Execution error`가 **한 번도 안 찍힌다**(로딩 10초 뒤 콘솔 전수 확인).
      게임은 정상으로 뜨고 `?fast=10`도 먹는다.
    ③ **창을 실제로 리사이즈해도 안 난다.** 로딩 뒤 `console.error`를 후킹해 세어 가며
      1024x768 → 800x600 → 900x400, 이어서 세로 540x960 → 가로 960x540(CLAUDE.md의 기준점 둘,
      폰을 돌리는 것과 같은 변화)까지 바꿔 봤지만 **새 에러 0건**이고 캔버스도 매번 정확히
      따라 커졌다.
    즉 07시 세션이 걱정한 "폰에서 돌릴 때마다 108번씩 돈다"는 **일어나지 않는다.** 폭 0은
    자동화 창이 뜨는 순간에만 있는 상태이고 사람이 여는 브라우저·폰에는 그 순간이 없다.
    코드는 고치지 않았다 — 고칠 것이 없다.
    (곁다리로 같이 본 것: 콘솔에 `Hidden/CoreSRP/CoreCopy`·`StencilDitherMaskSeed`·
    `HDRDebugView` 세 셰이더가 "not supported on this GPU"로 남아 있다. 폭 0이든 아니든
    똑같이 찍히고 화면에는 영향이 없어 보이지만, URP 쪽을 다시 볼 일이 있으면 같이 볼 것.)
- [x] W-11 (2026-09-18 05시 Unity 세션 발견) **확인 주소(dev)가 게임을 못 띄운다.**
  → **해결됨 (2026-09-18 06:38 06시 세션 확인, run #239 `489f9ef`).** "별칭 주소 확인" 스텝이
    `dev.planetracer-daz.pages.dev`를 열어 loader.js·framework.js.br·wasm.br·data.br·index.html
    다섯 파일을 전부 정상으로 읽었다("전부 통과. 브라우저에서 열린다."). Cloudflare 쪽 별칭이
    최신 배포를 다시 가리키게 된 것으로 보인다(누가 언제 바꿨는지는 저장소 밖이라 알 수 없음).
    아침 확인 경로가 다시 살아났다.
  CLAUDE.md가 2026-09-17에 "아침에 여기서 직접 만져 보라"고 정해 둔 그 주소다. 실제로 재 보면
  이렇다(PC에서 `node`로 몸통을 받아 바이트를 셌다, 2026-09-18 05:2x).

  | 파일 | `dev.planetracer-daz.pages.dev` | `planetracer-daz.pages.dev` (main) |
  |---|---|---|
  | `Build/PlanetRacer.loader.js` | **2177B, `text/html`** | 9547B, `application/javascript` |
  | `Build/PlanetRacer.framework.js.br` | **5085B** | 76179B |
  | `Build/PlanetRacer.data.br` | **5085B** | 6007127B |
  | `Build/PlanetRacer.wasm.br` | **5085B** | 8406666B |

  두 가지가 동시에 이상하다. (1) `loader.js`가 자바스크립트가 아니라 **HTML**로 내려온다 —
  그 파일이 없어서 대체 페이지가 나가는 것이고, loader.js가 없으면 유니티는 **아예 시작을 못 한다.**
  (2) 나머지 세 파일이 크기가 **전부 정확히 5085B로 똑같다** — 서로 다른 세 파일이 같은 크기일 리
  없으니 진짜 빌드 산출물이 아니다. main 쪽은 넷 다 정상이다.
  **왜 CI가 못 잡았나**: "배포 확인"·"로딩 시간 예산 확인" 두 스텝은 `steps.deploy.outputs`가 주는
  **그 배포 고유 주소**를 열어 본다. `dev.` 별칭 주소는 한 번도 안 열어 본다. 그래서 배포는
  성공으로 뜨는데 Tifania가 여는 주소는 딴것일 수 있다 — W-08·W-10과 정확히 같은 계열이다
  ("확인했습니다"가 초록불로 뜨는데 실제로는 그 대상을 확인하지 않은 것).
  **먼저 볼 것**: Cloudflare Pages에서 `dev` 브랜치 별칭이 어느 배포를 가리키고 있는지. 오래된
  실패 배포에 고정돼 있을 가능성이 크다. 고친 뒤에는 CI에 **별칭 주소 자체를 여는 스텝**을 하나
  더 넣어야 같은 일이 다시 안 생긴다.
  **(2026-09-18 06시 세션)** CI 쪽 몫만 먼저 함 — `webgl.yml`에 "별칭 주소 확인" 스텝 추가.
  `steps.deploy.outputs.pages-deployment-alias-url`(비면 `dev.planetracer-daz.pages.dev`
  기본값, main 푸시는 `planetracer-daz.pages.dev`)을 `check_web_deploy.js`로 그대로 열어 본다.
  `continue-on-error: true`로 둬서 빌드를 죽이지는 않는다 — 원인이 저장소 밖(Cloudflare
  대시보드)이라 재시도로 고쳐지지 않으니, 대신 로그에 `::error::`로 크게 남겨서 다음 세션이
  "배포 확인 성공"만 보고 지나치지 않게 하는 것까지가 목적. **Cloudflare 대시보드에서 별칭이
  가리키는 배포를 실제로 바꾸는 것은 여전히 Tifania 몫** — 이 스텝은 문제가 계속되는지
  알려 줄 뿐 고치지는 못한다. 다음 클라우드 세션이 할 일: 이번 푸시의 Actions 로그에서
  "별칭 주소 확인" 스텝이 실제로 뭐라고 찍혔는지 볼 것(YAML 문법은 `python3 -c "import yaml..."`,
  `node tools/check_web_deploy.js` 로직 자체는 이미 W-10에서 검증됨 — 여기선 새 스텝을
  기존 파일에 끼워 넣기만 했다).
  **그때까지 아침 확인은 `https://planetracer-daz.pages.dev`(main)에서 한다** — 다만 이쪽은
  승격이 막혀 있어 내용이 오래됐다(`origin/main`은 아직 `1814b22`). 즉 지금 **밤 세션들의 작업을
  웹에서 볼 수 있는 경로가 사실상 없다.** daily의 "오늘 웹에서 확인할 것"이 9/17부터 죽은 주소를
  가리켜 왔다는 뜻이기도 하다.
- [x] W-10 (2026-09-18 04시 코딩 세션에서 고침) `tools/measure_web_load.js`가 조용히 0을
      보고하고 있다 — 로딩 예산 감시가 사실상 꺼져 있다. W-08과 같은 계열의 구멍이다.
      **증상**: PC에서 `node tools/measure_web_load.js https://dev.planetracer-daz.pages.dev`를 돌리면
      다섯 파일 전부 `0.00 MB`, 합계 `0.01 MB`, 그리고 **"측정한 대역폭 구간 전부 10초 예산 안쪽"**
      이라고 통과 판정을 낸다. 빌드 로그의 "로딩 시간 예산 확인" 스텝도 그래서 계속 success다.
      **원인**: 37~40행이 `Accept-Encoding: br`로 헤더만 받고 `res.headers['content-length']`만 읽는데
      (`|| 0`), Cloudflare가 이 자산들을 이제 Content-Length 없이 chunked로 내려준다. 없으면 0이 되고,
      0은 "작다"로 읽혀 통과가 된다. 9/13(W-06)엔 값이 나왔으니 그 사이에 Cloudflare 쪽이 바뀐 것 같다.
      **고칠 방향**: Content-Length가 없으면 몸통을 실제로 읽어 바이트를 세거나(지금 `res.resume()`으로
      버리는 자리), 최소한 **0이면 통과가 아니라 실패**로 보고할 것. 0을 통과로 읽는 게 제일 나쁘다.
      **실측(이번 세션이 직접 세어 봤다, 스트림 바이트 수)**: run #229(`d96f8d8`) 배포 기준
      wasm 8.24MB + data 8.57MB + framework 0.07MB = **16.88MB**.
      → LTE 약함 3Mbps **47.2초**, LTE 보통 8Mbps **17.7초**, 좋음 25Mbps 5.7초.
      **W-06 때(9/13) 13.78MB / 36.8·13.8·4.4초보다 더 나빠졌다.** 늘어난 3.1MB는 9/18에 들어온
      아트다(A-14에서 압축을 태운 뒤의 숫자가 이것이다 — 안 태웠으면 훨씬 컸다).
      남은 큰 덩어리는 `rig-tiers-sheet.png`(DXT5 3MB)다. W-09와 같이 볼 것.
      **고침(04시)**: `headLength`가 헤더의 `content-length`를 읽던 걸 그만두고, 응답 몸통을
      실제로 받아 `data` 이벤트 바이트 수를 직접 합산하게 바꿨다 — Content-Length가 없어도
      실제 전송 바이트 수는 그대로 잡힌다. 덤으로 0바이트 응답은 이제 "통과"가 아니라 명시적
      실패로 센다(다섯 파일 다 빈 파일일 수 없으니). 이 클라우드 세션은 `*.pages.dev`에 못
      나가서(정책 403, CLAUDE.md에 적힌 그대로) 실제 배포 주소로 재현·재확인은 못 했다 —
      `node --check`로 문법만 확인했고, 스트림 바이트 합산 로직 자체는 `api.github.com`으로
      별도 검증(헤더 Content-Length 278 = 직접 합산 278, 일치 확인). **다음 GitHub Actions
      실행 로그의 "로딩 시간 예산 확인" 스텝에서 실제 MB 값이 찍히는지 볼 것** — 여전히
      0.00MB가 나오면 이 수정이 원인을 잘못 짚은 것이다.
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
- [x] D06-N (9/17 목 → 9/18 저녁 Unity 세션에서 마무리) 광맥 비주얼: 행성 표면에 광맥 프리팹 N개 배치(부트스트랩), 채굴 중 파티클·흔들림, 화물칸 게이지.
  - (주말 매시간 세션 검토만) `MiningRunState`/`SurfaceMover`를 보니 지금 "광맥"은 순전히 시간 기반
    추상 개념이다 — 채굴차는 표면을 계속 돌다가 `isMoving=false`가 되면 "그 자리"에서 멈출 뿐, 실제
    좌표를 가진 광맥 오브젝트가 하나도 없다. 그래서 이 항목은 단순히 장식 배치가 아니라 "채굴차가
    실제로 광맥 위치를 향해 이동하다 도착해서 멈춘다"는 이동 로직 자체를 건드려야 앞뒤가 맞는다
    (`planet.Circumference / VeinCount` 간격과 실제 배치 간격을 맞춰야 함). 구면 위 각도 계산이라
    실수하면 채굴차가 표면을 벗어나거나 엉뚱하게 도는 등 폰으로 열자마자 티 나는 회귀가 될 수 있어서,
    Unity 에디터로 직접 보면서 하는 게 안전하다고 판단해 이번 세션은 손 안 대고 다음(D07-M)으로 넘어감.
  - **(9/18 새벽 재검토, 결론 동일)** 다시 조사했지만 같은 결론이다 — `Core/Models.cs`의 `Planet`은
    `VeinCount`(밀도)·`Circumference`만 갖고 광맥 좌표·각도·인덱스가 없고, `MiningRunState`도
    "몇 번째 광맥으로 가는지"를 전혀 추적하지 않는다. 코어에 광맥 각도 배열을 순수 함수로 먼저
    추가하는 절충안도 생각해 봤지만, 결국 "이동 로직 자체를 건드려야 앞뒤가 맞는다"는 원래 메모대로
    반쪽짜리 작업이 되어 다음 세션에 혼란만 더할 것 같아 역시 손 안 댔다.
    **덤으로 확인**: 이 항목에 같이 적힌 "화물칸 게이지"는 **이미 있다** — uGUI 이사 후에도
    `MainHudUgui._cargoFill`(`BootstrapHudUgui.BuildCargoGauge`)로 살아 있으니, 다음에 이 항목을
    집을 때는 광맥 배치·이동 로직만 남은 것으로 보면 된다. **Unity 세션 필요**(광맥 위치 기반
    이동 로직 리팩터 포함이라 에디터로 직접 보면서 할 것).
  - **(9/18 19시 Unity 세션에서 완료)** 두 번 미룬 이유가 "각도 계산을 눈으로 봐야 한다"였는데,
    막상 붙어 보니 리팩터가 걱정한 것보다 작았다. 채굴차는 원래도 고정 축 둘레의 대원을 일정한
    각속도로 돌고 있었고, 광맥을 **그 대원 위에 같은 간격으로** 놓으면 "한 칸 지나는 시간"이
    코어의 이동 단계 길이(`Circumference / VeinCount / RigSpeed`)와 저절로 같아진다. 그래서
    이동 공식을 새로 만들 필요가 없었다.
    - 코어에 `VeinLayout.cs`(광맥 각도·번호 접기)와 `MiningRunState.VeinProgress`(지나온 광맥
      칸 수, 소수부가 다음 광맥까지의 진행률)를 추가했다. 화면은 속도를 따로 적분하지 않고
      이 진행도를 각도로 바꿔 그대로 찍는다 — 몇 시간을 돌려도 "코어는 도착했다는데 화면은
      아직 가는 중"이 안 생긴다. `SurfaceMover`에 `externallyDriven`/`SetOrbitAngle`을 더했고,
      옛 자유 주행 모드는 그대로 남아 있다(레이스·실험 씬이 쓴다).
    - `Assets/Scripts/Planet/VeinField.cs`가 표면에 광맥을 놓는다. 주행선 위에 그대로 놓으면
      채굴차가 파묻혀서 축 방향으로 2.2m 비켜 놨다 — 채굴차가 광맥 옆에 나란히 서는 그림이다.
      캐는 동안 그 광맥만 노랗게 바뀌고 크기가 맥동하며, 먼지 파티클이 뿜어져 나오고 차체가
      잘게 떨린다(`SurfaceMover.positionOffset`).
    - 에디터에서 실제로 확인: 광맥 14개 생성, 채굴 단계의 채굴차 각도 257.142853도 = 9번 광맥
      각도 257.142853도(정확히 일치), 광맥까지 거리 2.25m. `Core.Tests` **통과 228 / 실패 0**
      (광맥 관련 5개 추가). 광맥 모양은 임시 도형이라 실제 아트가 오면 `VeinField.veinPrefab`에
      꽂으면 된다.
- [x] D06-M (9/18 밤 세션) 극점 근처 광맥 배치 균등성 점검.
  → **문제 없음, 수학적으로 확인.** 걱정은 "광맥을 위도·경도 격자로 놓으면 극점 근처에서 촘촘해진다"는
    흔한 함정이었는데, D06-N의 실제 구현(`VeinLayout`)은 그 방식이 아니다 — 채굴차가 도는 대원 위에
    각도로만 놓고, 3D 위치는 `SurfaceMover`가 고정축 둘레로 회전시켜 얻는다. 회전은 거리를 보존하는
    등거리 변환이라 축이 어디를 향하든, 원 위의 등각도 간격은 항상 등거리 간격이다 — 위도·경도 격자가
    아니라서 애초에 왜곡될 자리가 없다.
    `Core.Tests`에 Rodrigues 회전 공식으로 `Quaternion.AngleAxis`를 UnityEngine 없이 재현해
    검증하는 테스트 4개 추가: 궤도축이 세계 위(0,1,0)와 수직이라 대원이 남북극을 그대로 지나가는
    경우, 시작점을 극점 바로 위(최악의 경우)에 두는 경우, 광맥 개수가 적거나(8개) 홀수(13개)인
    경우까지 전부 인접 광맥 간 3D 거리가 정확히 같음을 확인(`dotnet run` 238/0, 실패 0).
    `SurfaceMover.ApplyTransform`의 "접선이 0에 가까울 때" 방어 코드는 방향(회전, LookRotation)에만
    관여하고 위치(광맥 간격)에는 영향이 없다는 것도 코드를 보고 확인했다 — 그래서 그 방어 코드가
    있든 없든 이 결과는 그대로다. Unity 에디터 없이 순수 수학으로 끝난 항목이라 "확인 필요" 없이 닫는다.
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
- [x] A-04 레이스 결과 화면에 랩타임과 이전 기록 대비 차이 표시
      → **2026-09-19 23:0x Unity 배선 세션에서 UI까지 붙여 끝냈다.** 아래 야간 세션 기록 뒤에
      이어지는 내용이다. `MiningController.RecordRaceTime(courseId, timeSeconds)` 신규 —
      `_save.RaceRecordCourseIds`/`RaceRecordBestSeconds`를 `RaceRecordBook.Update`에 그대로
      넘기고 `Save()`까지 부른다(`RustyBoxCount` 등과 같이 `_save`를 직접 다루는 자리).
      `RaceEntryUgui.PlayerRecordSuffix(course, time)` 신규 — `ShowResultView`의 `player` 줄
      뒤에만 붙는다. 첫 완주 `(첫 기록)` / 갱신 `(최고 기록! 3.8초 단축)` / 미갱신
      `(내 최고 42.0초, 3.5초 느림)` / 동률 `(최고 기록과 같음 38.2초)` 네 갈래.
      `DeltaSeconds`가 "이번 - 이전"이라 음수가 단축인데 그대로 쓰면 "-1.3초"로 읽히므로
      부호는 말로 옮기고 숫자는 절댓값만 쓴다. **씬은 안 건드렸다** — 결과 줄은 이미
      `result-row-0~5`로 배선돼 있어서 붙일 칸이 따로 없다.
      Play 모드에서 네 갈래를 전부 실제로 불러 문자열·세이브 반영까지 확인했고(확인 뒤
      기록 리스트는 원래대로 되돌렸다), 게임 예외 0.
      → **2026-09-19 20:0x 야간 세션(코드만, Unity 없음)**: `Core/RaceRecordBook.cs` 신규.
      코스별 자기 최고 기록(초)을 관리하는 순수 함수 두 개 — `Update(courseIds, bestSeconds,
      courseId, timeSeconds)`가 처음 완주면 기록을 추가하고, 더 빠르면 갱신하고, 더 느리거나
      같으면 그대로 둔 채 `PreviousBestSeconds`·`DeltaSeconds`(음수=단축)를 돌려준다.
      `BestOf`는 갱신 없이 조회만. `SaveData`가 JsonUtility라 Dictionary를 못 써서(파일 상단
      주석) `RaceRecordCourseIds`/`RaceRecordBestSeconds` 병렬 리스트로 저장한다
      (`OwnedPartIds`/`OwnedPartEnhanceLevels`와 같은 패턴). Core.Tests 7개 추가
      (256 → 263, 실패 0) — 첫 완주, 갱신, 미갱신, 동률(< 비교라 갱신 안 됨), 코스별 분리,
      없는 코스 조회, 경계값(0초 이하·빈 id·리스트 길이 불일치) 전부 확인.
      **UI는 아직 안 붙였다** — `RaceEntryUgui.ShowResultView`(`Assets/Scripts/UI/
      RaceEntryUgui.cs:219`)가 결과 줄을 `"{순위}위 {이름} — {시간}초"`로 그리는 자리이니,
      `player` id 줄에 `RaceRecordBook.Update`를 부르고 `HasPreviousRecord`면 델타를 이어
      붙이면 된다(`target`이 `MiningController`라 SaveData 접근은 이미 있다). MonoBehaviour를
      건드리는 구조 변경이라 Unity 컴파일 확인이 필요해 다음 Unity 세션 몫으로 남긴다.
- [ ] A-05 레이스 행성 반지름 결정(채굴 20m / 레이스 60m를 유지할지, 코스를 따로 둘지)

## P2 버티컬 슬라이스 (10/6–11/13) — 주 단위, P1 끝나면 일 단위로 쪼갠다

- [ ] W1 루비·사파이어 행성 파라미터·머티리얼, 행성 선택/워프 흐름, 행성 배지.
- [ ] W2 레이스 4등급(로컬·서킷·챌린지·그랑프리) 해금 구조, 그랑프리 조건(세트 3개), 강철·티타늄 상자.
- [ ] W3 실제 아트 적용(에셋 팩), 채굴차 티어 외형 3종, 부품 아이콘.
      → **이 줄은 위 「🔥 최우선 — 아트 배선」의 A-16~A-20으로 쪼개서 올렸다 (2026-09-20).**
      여기 남은 건 에셋 팩 구입분뿐이다.
- [ ] W4 일일 광맥 1종(월요일 고온), 입장권 초기화(로컬 자정).
- [ ] W5 UI 전체 폴리시, 튜토리얼 10분으로 확장, 데모 뼈대 빌드.

## P3 이후

- P3 온라인·대전·계정 (11/16–12/11), P4 콘텐츠·밸런스 (12/14–1/1), P5 스토어·데모 (1/4–1/22), P6 폴리시·QA·심사 (1/25–2/12). 상세는 기획서 로드맵. P2 중반에 일 단위로 쪼갠다.

## 2026-09-17 경제 회귀에서 나온 것

- [x] E-01 (2026-09-17 야간) 제작·강화 버튼 글자에 화폐 이름을 넣었다. `제작 (15)`/`강화 (15)` →
      `제작 (정제 15)`/`강화 (정제 15)`. `CraftingUgui.SetRow` 두 줄만 고쳤다 — 제작·강화 둘 다
      정제 광물 하나만 쓰기 때문에(업그레이드 화면처럼 원석과 갈리지 않는다) 유닛 접두어만
      붙이면 됐다. 버튼 라벨이 최대 4글자 늘어나는데(`제작 (15)`→`제작 (정제 15)`)
      `BootstrapCraftingUgui.MakeButton`은 고정 14pt·줄바꿈 없음·자동 축소 없음이라 셀 폭
      400px을 반씩 나눈 버튼(~190px)에서 넘칠지 에디터 없이는 확신할 수 없다 —
      **Unity 세션이 세 기준점에서 실제로 안 잘리는지 볼 것.** Core 변경 없어 `dotnet run` 생략.
      **확인 완료(2026-09-18 01시 Unity 세션)**: 안 잘린다. `제작 (정제 15)` 실측
      `preferredWidth = 79.5px`, 버튼 폭 `182px` — 두 배 넘게 여유가 있다. 세 기준점을 따로
      돌려 볼 필요가 없었다: `row-list`의 `GridLayoutGroup`이 `cellSize = 400x190` **고정**이라
      화면이 넓어져도 칸이 넓어지지 않고 칸 수만 늘어난다(Flexible). 즉 182px이 모든 기준점에서
      같은 값이다. `강화`도 24.2px로 여유.
- [x] E-02 (2026-09-18 야간) 화폐가 모자라 못 누르는 버튼에 안내 문구를 붙였다 — 최대 레벨(MAX)과
      "돈만 더 모으면 되는 것"을 글자로 구분되게 했다. 두 화면이 상황이 달라서 방식을 다르게 했다.
      **`CraftingUgui`**: 새 `hint-label` 한 줄을 헤더(재화 표시 아래)에 추가 — 다섯 줄 중 하나라도
      정제 광물 부족으로 막혀 있으면 "정제 광물이 부족해요 — 제련소를 올리면 더 빨리 쌓여요."가 뜨고,
      아니면 빈 문자열(자리는 항상 잡혀 있어 목록이 밀리지 않음). 목록이 이미 `ScrollRect`라
      (U-03 결론) 헤더가 28px(줄 20 + spacing 8) 늘어도 스크롤 뷰포트만 줄 뿐 안 잘린다.
      `BootstrapCraftingUgui.cs`(`GemRacer/17`)에 `hint-label` 생성 코드 추가 — 다시 눌러야 반영된다.
      **`UpgradeUgui`**: 새 UI 요소를 안 만들고 이미 있던 `{prefix}-effect` 라벨(word wrap 켜짐,
      60px로 여유 있게 잡힌 자리) 끝에 한 줄을 덧붙이는 쪽을 택했다 — 이 화면은 `ScrollRect`가
      없어서(2026-09-17 제련소 추가로 네 줄이 됐는데 세로 한 칸에서 안 넘치는지 아직 실제로
      확인된 적이 없다) 헤더에 새 고정 높이 줄을 넣으면 그 확인 안 된 예산을 더 깎는 셈이라
      부트스트랩은 손대지 않았다. 화폐가 둘이라(제련소만 원석) 문구도 둘 — 정제 광물 부족이면
      크래프팅과 같은 문구, 원석 부족(제련소 줄)이면 "원석이 부족해요 — 채굴이 좀 더 쌓일 때까지
      기다려 보세요.". 둘 다 최대 레벨(AtMax)일 때는 기존대로 "최대 레벨"만 보여준다(돈 문제가
      아니라서 안내 대상이 아님). Core는 안 건드려서 `dotnet run` 생략(197/실패 0 그대로일 것).
      Unity 에디터가 없어 컴파일·실제 줄바꿈 확인은 다음 세션 몫 — **`GemRacer/17` 재실행 필요**
      (hint-label이 새 UXML 요소라 씬에 없으면 `CraftingUgui`가 경고만 찍고 조용히 안 보임,
      기능은 안 죽음). `UpgradeUgui` 쪽은 부트스트랩 변경이 없어 씬 재실행이 필요 없다 — 다만
      세 기준점(세로/가로/태블릿)에서 두 줄짜리 안내 문구가 60px 안에 잘리지 않는지, 특히
      제련소 줄(원석 부족 문구)에서 실제로 볼 것.
      **확인 완료(2026-09-18 01시 Unity 세션)** — 셋 다 통과.
      1) `GemRacer/17` 재실행해서 `hint-label`을 씬에 넣었다. **다시 세우면 `Crafting` 노드를
      통째로 지우고 만들기 때문에 `MainHudUgui.craftPanel` 연결이 끊긴다**(ugui-migration 3-4와
      같은 함정, 이번엔 HUD가 아니라 화면 쪽) — 실제로 끊겨 있었고 다시 물렸다. 안 물렸으면
      "제작" 버튼이 조용히 꺼진 채로 남았을 것이다.
      2) Play에서 실측: `hint-label`이 정제 광물 부족 상태에서 한 줄로 뜨고(`preferredWidth`
      289px < 폭 500px) 한글도 정상(Pretendard 물려 있음). 콘솔 예외 0.
      3) `UpgradeUgui` 쪽 60px 예산: 네 줄 전부 최악값(효과 문구 + 안내 문구)으로 재 보니
      **두 줄 31px** — 60px의 절반. `refinery-effect`(원석 부족 문구, 가장 긴 것)도 같다.
      여기도 칸 폭이 `cellSize = 400x168` 고정이라 기준점마다 달라지지 않는다.
      **덤**: 업그레이드 화면 네 줄이 세로 한 칸(540x960)에서 안 넘치는지도 이번에 확인했다 —
      닫기 버튼까지 다 보인다. 2026-09-17 제련소 추가 이후 확인된 적 없던 항목이다.
- [ ] E-03 새 세이브로 첫 30분을 실제로 눌러 보는 점검을 정기적으로 한다. 이번 회귀(사흘)는
      코어 테스트 197개가 전부 통과하는 동안 아무도 못 잡았다 — 순수 함수는 다 맞았고
      **연결된 진행 경로만 끊겨 있었다.**


## 2026-09-17 설계 확정분 — 심야 세션이 집어 갈 것

설계 문서를 먼저 읽고 시작한다. 셋 다 2026-09-17에 Tifania와 정한 것이다.
- `docs/design/balance/idle-research.md` — 다른 방치형 게임 조사. 비용/생산 비율이 핵심
- `docs/design/planet-progression.md` — 행성별 곡괭이, 레벨 상한, 클리어·이동
- `docs/design/amplifier.md` — 증폭기
- `docs/design/pet-gacha.md` — 펫 뽑기(캐시). **법적 표시 의무가 있다. 0절 먼저 읽을 것**

순서대로 한다. 앞이 끝나야 뒤가 된다. **에디터 없이 되는 것은 코어 + 테스트까지 하고,
씬 배선은 Unity 세션 몫으로 남긴다.**

### 레벨 상한 · 비용 곡선

- [x] P-01 (2026-09-17 밤 매시간 세션) 화물칸·엔진 상한을 10 → 30으로 올렸다(`UpgradeCost.CargoMaxLevel`·
      `EngineMaxLevel`). 엔진은 `RigSpeed`가 이미 레벨당 ×1.12 지수식이라 상한만 늘리면 비율이
      그대로 유지됐다. **화물칸이 문제였다** — `CargoHours`가 "1레벨 ×1 ~ 10레벨 ×3"을 10에서
      멈추는 걸 전제로 한 선형식이었다. 그대로 30까지 늘리면(끝값 3배를 30레벨에 맞게 나눠서든,
      같은 기울기로 늘려서든) 비용(지수 1.25)과 생산(선형)의 성장 방식 자체가 달라서 비율이 뒤로
      갈수록 나빠졌다 — idle-research.md가 경고한 "선형 생산 + 지수 비용" 조합.
      그래서 `CargoHours` 배율도 지수식(레벨당 ×1.12, 엔진과 같은 기울기)으로 바꾸고, 비용 성장률을
      1.25 → 1.18로 낮췄다. 비율 1.18/1.12 = **1.054**, 목표(1.04~1.06) 안. 레벨 1 비용(9)·화물칸
      기본 배율(×1)은 그대로라 초반 체감은 안 바뀐다.
      `RigPartApply.Apply`(레이스 보상 적용)가 Cargo/Engine 상한을 `Math.Min(10, ...)`으로 박아
      뒀던 것도 `UpgradeCost.*MaxLevel` 참조로 바꿔서, 이번처럼 상한이 다시 바뀔 때 두 곳이
      어긋나는 걸 막았다(안 고쳤으면 상점에서는 30까지 사지는데 레이스 무료 보상은 10에서 막히는
      버그가 났을 것이다).
      Core.Tests: 기존 5개(정확한 배율·상한 값을 가정하던 테스트) 새 수식에 맞게 갱신 +
      "비용/생산 비율이 30레벨 내내 1.04~1.06"과 "20레벨 뒤 누적 구매 간격 3배 미만"을 확인하는
      신규 테스트 1개. **201 → 202, 실패 0.** `dotnet run sim`(L-05 봇)으로도 확인 —
      Tool/Cargo/Engine이 전부 1.7시간에 30/30/30(84회 구매)에 닿고, 구매 간격이 로그 뒤로
      갈수록 매끄럽게 벌어진다(전에는 10레벨에서 몇 분 만에 막혀 2~3시간이 완전히 비었다,
      idle-research.md 2절). Core 변경만 있어 Unity 컴파일 확인은 필요 없음 — UI(`UpgradeUgui.cs`)는
      레벨/상한 숫자를 하드코딩하지 않고 그대로 읽어서 손 안 댐. **웹에서 볼 것**: 화물칸·엔진
      업그레이드가 10에서 안 멈추고 30까지 계속 눌리는지(수치 자체는 눈으로 검증하기 어려우니
      "버튼이 안 사라진다" 정도만 확인해도 충분).
- [x] P-02 (2026-09-17 밤 매시간 세션) `YieldPerVein`의 10레벨×1.5 티어 점프 두 번을
      5레벨×1.176 점프 다섯 번으로 늘렸다(`MiningSimulator.TierSpanLevels`/`TierJumpMultiplier`
      신규 상수). 점프 배율(1.5^(2/5))은 **레벨 30 지점의 최종 누적 배율(×2.25)이 예전과 똑같아지게**
      역산한 값이다 — 상한·비용 곡선(P-01에서 막 맞춘 비율)은 그대로 두고 중간(레벨 6·11·16·21·26)만
      더 자주 튀게 하려는 목적이라, 끝값이 흔들리면 그 튜닝이 도로 어긋난다.
      **부딪힌 것**: `docs/design/planet-progression.md` 1절에 이미 "행성별 곡괭이 천장 레벨" 표가
      있는데, 이게 정확히 `YieldPerVein`의 옛 티어 구조로 계산된 값이었다(직접 재계산해 보니
      문서 표 자체도 당시 살짝 부정확했다 — 15/17/19/21/23/25였는데 정확히는 15/17/18/20/21/21).
      티어 구조를 바꾸면 이 표가 통째로 움직인다 — 그래서 **끝값을 보존하는 배율을 역산**해서
      표가 크게 어긋나지 않게(±1~2레벨) 막고, 새 표(16/16/18/19/21/22)로 두 문서
      (`planet-progression.md`, `idle-research.md`) 다 갱신해 뒀다. 그래도 숫자가 움직인 것 자체는
      사실이니 **다음에 P-06(행성별 곡괭이 분리)을 다룰 세션은 이 갱신된 표를 기준으로 볼 것.**
      `Core.Tests`에 2개 추가(5레벨 경계에서만 점프하고 그 사이는 순수 지수 성장인지, 레벨 30
      최종값이 옛 방식과 사실상 같은지) — **202 → 204, 실패 0**. `dotnet run sim`(L-05 봇)으로도
      페이싱 확인 — Tool/Cargo/Engine 30/30/30 도달 시각(1.6~1.7h)이 P-01 때와 그대로라 회귀 없음.
      Core만 고쳐서 Unity 컴파일 확인 불필요, 웹에서 눈으로 볼 변화도 없음(내부 수치 튜닝).

### 증폭기 (amplifier.md)

- [x] P-03 (2026-09-19 21시 주말 세션) `Core/Amplifier.cs` 신규 — 등급(PartGrade C/B/A/S =
      일반/고급/에픽/전설) → 증폭률 구간 표 + `Roll(grade, seed)`(구간 안 균등분포, 재현 가능) +
      `Apply(basePerformance, totalBonus)`(합산 방식: 최종 = 기본 × (1 + 합)). 새 등급 enum을
      만들지 않고 기존 `PartGrade`를 그대로 썼다 — 상자가 이미 그 등급 확률표(`LootTable`)로
      뽑고 있어서 증폭기도 같은 등급을 물려받으면 P-04에서 `LootTable` 결과를 바로 넘길 수 있다.
      상한(칸당 누적을 어디까지 열어 둘지)은 amplifier.md에 아직 안 정해져 있어 이 파일은
      강제하지 않는다 — P-05(SaveData)에서 정해지면 자르기로 함.
      `Core.Tests`에 6개 추가(등급별 구간이 표와 일치·Roll 재현성·구간 안쪽 1000표본·등급 간
      구간이 안 겹침·Apply 합산 검증·경계값(기본 0/증폭률 0)) — **263 → 269, 실패 0**.
      Unity 참조 없는 순수 C#이라 컴파일 위험 낮음. **부딪힌 것**: 지난 20시 세션이 커밋한
      `RaceRecordBook.cs`에 `.meta`가 빠져 있었다(CLAUDE.md 4번 위반 — Unity가 열면 참조가
      끊겼을 것) — 이번 세션이 새 GUID로 `.meta`를 만들어 같이 커밋했다.
- [x] P-04 (2026-09-19 23시 주말 세션) `LootTable`/`LootBoxOpener`에 증폭기·광물 결과 추가.
      `LootReward.RollKind(seed)`(부품 55%/증폭기 30%/광물 15%, 상자 종류와 무관한 첫 값 —
      코어 루프 원칙대로 부품 위주는 유지) + `AmplifierFor(grade, seed)`(`Amplifier.Roll`을
      감싸 등급까지 들고 다님) + `MineralsFor(grade, seed)`(등급별 구간 균등분포, C 10~20 ~
      S 150~300, 첫 값). `LootBoxOpener.OpenAny(...)`가 새로 등급→종류→해당 페이로드까지
      한 번에 조립한다(천장 Guaranteed는 등급만 확정할 뿐 종류를 부품으로 고정하지 않는다 —
      확정 등급의 증폭기·원석도 그대로 나온다).
      **기존 `LootBoxOpener.Open(...)`은 시그니처·동작 전부 그대로 뒀다** — `MiningController.
      TryOpenBox`/`LootBoxPanel`/`LootBoxUgui`가 여전히 `result.Reward`를 무조건 읽는 MonoBehaviour/
      UI 코드라(컴파일 확인이 안 되는 이 세션에서 건드리면 위험), `LootBoxOpenResult`에 필드
      (`Kind`/`AmplifierBonus`/`Minerals`)만 더했다 — `Kind`의 기본값(enum 0번)이 `RigPart`라
      옛 `Open()`을 그대로 불러도 `Kind`가 자동으로 `RigPart`로 읽히고 `Reward`도 그대로 채워진다
      (03:11 세션의 `RaceFuel.Recover` 4인자 오버로드, 08:05 세션의 `MiningSimulator.Offline`
      `forceFullRefine` 오버로드와 같은 패턴 — 새 진입점을 나란히 추가해 기존 호출부는 안 건드림).
      `Amplifier.Roll`이 어느 칸(곡괭이·화물칸·엔진·제련소/레이싱카 다섯 칸)에 꽂히는지는
      아직 안 정했다 — amplifier.md가 "소모품이냐 끼우는 것이냐"를 열어 둔 것과 같은 자리라
      P-05(SaveData) 몫으로 남긴다.
      `Core.Tests`에 7개 추가(RollKind 정의값·재현성, 2000표본 분포가 가중치와 5%p 안,
      MineralsFor 구간·등급별 단조 증가, Minerals/AmplifierFor 재현성, 옛 `Open()` 회귀,
      `OpenAny` 필드 배타성, 천장에서도 종류별 값이 맞는지) — **269 → 276, 실패 0**.
      Unity 참조 없는 순수 C#이라 컴파일 위험 낮음(기존 파일만 수정, 새 파일 없어 `.meta` 불필요).
- [x] P-05 `SaveData`에 칸별 증폭률 + `MiningSimulator`·레이스 스탯에 반영. 세이브 왕복 테스트.
      P-04가 남긴 것 — 증폭기가 상자에서 나오긴 하는데(`AmplifierReward{Grade,Bonus}`) 아직
      어느 칸에 쌓이는지 정하는 자리가 없다. amplifier.md "누적 상한"도 여기서 같이 정할 것.
      **(2026-09-20 00시 주말 세션, 채굴 쪽 절반)** `SaveData.Amplifiers`(`RigAmplifierSave` 신규,
      RigSlot 다섯 칸 Tool/Cargo/Engine/Detector/Refinery, `SaveData.AddAmplifier(slot, bonus)`로
      더함 — 상한 없음, amplifier.md가 아직 안 정한 그대로 안 자른다) + `MiningSimulator`에 amp-aware
      오버로드(`RigSpeed`/`YieldPerVein`/`MineralsPerHour`/`CargoHours`/`CargoCapacityMinerals`/
      `GemsPerHour`/`RefinePerHour`/`Offline`, 전부 기존 시그니처는 그대로 두고 `RigAmplifierSave amp`
      인자 하나를 더 받는 오버로드로 추가 — 기존 호출부 회귀 없음). 칸→지표 매핑은 새로 정한 게
      아니라 `UpgradeCost.Cost` 주석이 이미 써 둔 것(Tool→YieldPerVein, Cargo→CargoHours,
      Engine→RigSpeed, Refinery→RefinePerHour, Detector→GemsPerHour)을 증폭기에도 그대로 적용했다.
      Tool 증폭은 광맥 매장량(VeinYield)을 못 넘도록 그대로 클램프, Detector는 레벨 0이면 증폭해도
      0(장비 없이 증폭기만으론 안 켜짐), Refinery는 비율이 아니라 최종 정제량에 곱해서 채굴 속도를
      넘어서면(`Offline`의 `rate<=refineRate` 분기) 화물칸이 아예 안 찰 수 있다 — 의도된 동작.
      `Core.Tests`에 8개 추가(칸별 누적·0 이하 무시, SaveData 연결, amp=null 회귀 없음, Tool 클램프,
      Engine이 MineralsPerHour까지 정확히 전파, Cargo+Tool+Engine 합성, Refinery가 화물칸을 비우는
      경계 케이스, Detector 레벨 0 무효) — **276 → 284, 실패 0**. `dotnet run sim` 페이싱도 그대로
      (증폭기는 아직 봇 시뮬레이션에 안 붙었다 — 붙이는 건 amplifier.md 6단계, 다음 몫).
      **안 한 것(채굴 쪽 세션 시점)** — ① 레이싱카 다섯 칸(엔진·타이어·서스펜션·차체·부스터)은
      그대로 남았다(RigSlot과 다른 enum이 필요, `PartSlot` 쪽엔 손 안 댐). ② 상자에서 나온
      증폭기(`AmplifierReward`)를 실제로 어느 칸에 넣을지 고르는 자리(무작위/플레이어 선택)는
      여전히 없다 — `SaveData.AddAmplifier(slot, bonus)`까지만 만들어 뒀고, slot을 누가
      정하는지는 UI 쪽 Unity 세션 몫으로 남긴다. ③ 누적 상한은 amplifier.md에 여전히 미정이라
      이 세션도 안 잘랐다.
      Unity 참조 없는 순수 C#이라 컴파일 위험 낮음(기존 파일 수정 + `SaveData.cs` 안에 클래스
      추가라 새 파일 없음, `.meta` 불필요).

      **(2026-09-20 01시 주말 세션, 레이싱카 쪽 나머지 절반 — P-05 전체 완료)**
      `RaceSimulator.cs`를 먼저 읽어 보니 이 코드는 `Stats` 값만 받아 랩타임을 계산할 뿐
      슬롯·부품 개념이 아예 없다 — 슬롯별 증폭이 실제로 붙는 자리는 `RaceSimulator`가 아니라
      `RacingCar.TotalStats()`(각 슬롯의 `Part.Effective()`를 합산하는 지점, `Models.cs`)였다.
      그래서 백로그 메모의 "RaceSimulator에 오버로드"는 읽고 나서 더 정확한 자리로 옮겼다.
      `SaveData.PartAmplifiers`(`PartAmplifierSave` 신규, `PartSlot` 중 amplifier.md가 명시한
      다섯 칸 Engine/Tire/Suspension/Body/Booster만 필드로 가짐 — `Module`은 설계 문서에
      없는 칸이라 애초에 필드가 없고 `Bonus`는 0, `Add`는 조용히 무시) + `SaveData.AddAmplifier
      (PartSlot, float)` 오버로드(기존 `RigSlot` 버전과 나란히) + `RacingCar.TotalStats
      (PartAmplifierSave? amp)` — 슬롯에 꽂힌 `Part.Effective()`(강화 반영 스탯)에만
      `(1+그 칸 증폭률)`을 곱한다. 빈 슬롯·차량 최소 기본치(부품 없어도 주는 Power10/Grip10 등)는
      증폭 대상이 아니다(채굴 쪽 "장비 없이 증폭기만으론 안 켜짐"과 같은 결의 규칙). 기존
      `TotalStats()`는 `TotalStats(null)`을 부르는 것으로 바뀌었을 뿐 동작은 완전히 같다.
      `Core.Tests`에 6개 추가(칸별 누적·Module과 0 이하 무시, SaveData 연결, amp=null 회귀 없음,
      빈 슬롯 비증폭, 칸끼리 안 섞임, 강화×증폭기 결합법칙) — **284 → 290, 실패 0**.
      **P-05가 이제 전부 끝났다.** 열린 채로 남은 건 amplifier.md 자체가 미정으로 남긴 두 가지뿐
      — (a) 상자에서 나온 증폭기를 실제로 어느 칸에 넣을지 고르는 화면(무작위/직접 선택,
      Unity 세션 몫), (b) 누적 상한(정해지면 `RigAmplifierSave.Add`/`PartAmplifierSave.Add`에서
      한 줄씩 자르면 됨). Unity 참조 없는 순수 C#(기존 파일만 수정, 새 파일 없어 `.meta` 불필요).

### 행성 진행 (planet-progression.md)

- [ ] P-06 `MiningRig.ToolLevel` 하나를 행성별로 나눈다(2절 (나)안).
      **세이브 마이그레이션 필요** — 기존 세이브의 ToolLevel을 현재 행성 값으로 옮긴다.
      그리고 **물려주기**를 반드시 같이 넣는다(이전 최고 레벨의 40%를 새 행성 시작값으로).
      이게 없으면 행성 이동이 좌절 구간이 된다
      **(2026-09-18 23시 세션 확인) T-10과 무관하게 착수 가능해 보여서 검토했지만 오늘 밤엔 안 댔다** —
      `int` → `Dictionary<planetId, int>`로 타입 자체가 바뀌는 구조 변경이라 `UpgradeUgui.cs`/
      `UpgradePanel.cs`/`MiningController`/`SaveData` 쪽이 새 시그니처에 맞는지 컴파일로
      확인해야 하고, "물려주기" 공식은 실제 세이브 파일로 왕복해 봐야 의미가 있다 —
      둘 다 Unity 에디터가 있어야 검증되는 부분이라 Core.Tests만으로 끝내기엔 위험이 크다.
      **Unity 세션 몫으로 남긴다.**
- [ ] P-07 행성별 광물 종류. 상위 행성 광물이 상위 부품 제작에 쓰이게 해서 되돌아갈 이유를 만든다
      **(2026-09-20 02시 주말 세션, 구조 확정 + 메커니즘 부분 완료)** planet-progression.md
      2.1절에 구체적인 구조를 적었다 — 광물 종류는 별도 테이블 없이 `Planet.Id`를 그대로 쓰고,
      표시 이름만 새 필드 `Planet.MineralNameKo`(6행성 전부 채움, "석영 원석" 등)로 둔다.
      정제 광물을 행성별로 나눠 보관하는 창고를 새로 만들었다 — `SaveData.PlanetMineralIds`/
      `PlanetMineralAmounts`(병렬 리스트, JsonUtility가 Dictionary를 못 다뤄서 RaceRecordBook과
      같은 패턴) + `PlanetMineralBank.cs`(조회·추가·차감 순수 함수) + `MineralCost`/
      `PlanetMineralRecipe`(여러 행성 광물을 섞은 제작 비용을 all-or-nothing으로 검사·소비).
      기존 `RawMinerals`/`RefinedMinerals`(지금 캐는 행성 진행값)는 손대지 않았다 — 새 필드라
      세이브 마이그레이션도 필요 없다. **아직 안 된 것**: 정제 광물을 언제 이 창고로
      옮겨 담을지(행성 이동 시점? 실시간?)는 UI 흐름과 같이 정해야 해서 Unity 세션 몫으로
      남겼다. `Core.Tests` 7개 추가 — 290 → 296, 실패 0. Unity 참조 없는 순수 C#, 새 파일이라
      `.meta` 손으로 만듦(GUID 중복 없음 확인).
      **(2026-09-20 12시 주말 세션) B등급 쿼츠 부품 5종 추가 — (b) 절반 해결.**
      `DefaultData.QuartzAdvancedParts()`(엔진/타이어/서스펜션/차체/부스터, id `q_*_b`) —
      스탯은 C의 2배, `PartCraft.Cost`도 B등급을 지원(`PartCostB` = C의 4배 = 60). 배수는
      기존 `QuartzTreasureDefs`의 등급별 배수(약 2~2.5배)와 `RaceSimulator.LapTime`이 Power 등을
      선형 가산으로 쓰는 것(2배가 성능을 과하게 안 키움)을 참고해 잡은 첫 값 — 실제 레이스
      체감은 P-11(달리는 장면) 붙은 뒤 조정 여지 있음. `PartEnhance.Cost`는 `PartCraft.Cost`를
      그대로 따라가는 구조라 코드 변경 없이 B등급 강화 비용도 자동으로 풀렸다.
      **여전히 안 된 것**: (a) 위 창고 연결(Unity 세션 몫, 그대로). A/S 등급 부품·
      `MineralCost` 레시피(다른 행성 광물을 섞는 쪽)는 아직 손 안 댔다 — A/S는 쿼츠 하나만으로는
      의미가 약하고 다음 행성(루비) 광물이 있어야 "섞어서 만드는" 레시피가 뜻이 생긴다.
      **UI에는 아직 안 보인다** — `MiningController.AvailableParts`가 지금도 `QuartzStarterParts()`
      5종 고정이라 B등급을 화면에서 고를 방법이 없다(C를 다 만들면 자동 승급? 별도 탭? 정해야
      함, Unity 세션 몫). 기존 테스트 2개(B/A/S가 전부 예외인지 보던 것)를 새 기댓값에 맞춰
      갱신 — 삭제 아님, T-12와 같은 방식. `Core.Tests` 3개 추가 — 296 → **308, 실패 0**.
      Unity 참조 없는 순수 C#, 기존 파일만 고침(새 파일 없음, `.meta` 불필요).
- [ ] P-08 행성 클리어 조건과 해금. **조건은 Tifania 결정 대기**(planet-progression.md 4절).
      해금은 되돌릴 수 없고, 이동은 되돌아갈 수 있어야 한다
- [ ] P-09 행성 선택·이동 화면(uGUI). 씬 배선이라 Unity 세션 몫

### 레이스 코스 (planet-progression.md 6절)

- [x] P-10 (2026-09-19 새벽 매시간 세션) `Core/CourseGenerator.cs` 신규 — `Generate(planetId, tier,
      index, seed)`가 길이·랩·평지/험지/부스트 비율 네 값을 시드에서 뽑는다. 비율 세 개는 [0,1)
      안에서 절단점 두 개를 뽑아 정렬하는 방식으로 합이 항상 정확히 1이 되게 했다(디리클레 근사).
      등급별 길이·랩 범위(로컬 400~900m·1~2랩 ~ 그랑프리 1800~3000m·3~4랩)는 손으로 만든 쿼츠
      로컬 3개(500~800m, 1~2랩)를 기준으로 등급이 오를수록 늘어나게 잡은 **첫 값**이다 — 실제로
      달려 보고(P-11 붙은 뒤) 조정될 여지가 있다. `GenerateMany(planetId, tier, count, baseSeed)`로
      한 번에 여러 개(각기 다른 시드) 뽑는다. **아직 아무 데도 안 붙였다** — `DefaultData`나
      레이스 진입 화면이 이 생성기를 부르게 하는 건 W2(4등급 해금 구조)가 먼저 정해져야 의미가
      있어서 손 안 댔다(이번 항목은 "시드에서 그 네 값을 뽑는 순수 함수부터"까지). `Core.Tests`에
      4개 추가(같은 입력 재현성, 비율 합이 항상 1·음수 없음, 등급별 범위를 벗어나지 않는지 400
      개 표본, GenerateMany 개수·Id 중복 없음) — **244 → 248, 실패 0**. Unity 참조 없는 순수
      C#이라 컴파일 위험은 낮지만 `.meta`는 이 세션이 기존 Core 파일들과 같은 최소 2줄 형식으로
      손으로 만들었다(GUID 중복 없음 확인) — **다음 Unity 세션이 정상 임포트되는지 한 번 봐 줄 것.**
      → **2026-09-19 09시 세션에서 발견된 문제 수정.** 03:10 Unity 배선 세션이 등급이 달라져도
      노면 비율이 똑같다고 적어 둔 것의 원인을 찾아 고쳤다 — `NextInt`가 범위와 무관하게
      항상 draw 1회만 쓰는 구조라, 랩·길이를 뽑은 뒤 이어지는 두 `NextFloat()`(비율용)가 등급과
      무관하게 같은 시드에서는 항상 같은 값이었다. `RoughnessBias(tier)`를 추가해 등급이 오를수록
      평지에서 부스트로 비중을 옮기게 했다(Local 0 / Circuit 0.05 / Challenge 0.10 / GrandPrix
      0.15, 험지 비율은 그대로 두고 평지→부스트로만 이동 — 합 1 유지, 음수 안 남). 손으로 정한
      **첫 값**이라 P-11 붙어서 실제로 달려 보면 조정될 여지가 있다. `Core.Tests` 3개 추가(네
      등급 전부에서 합=1·비음수 재확인, 등급이 오를수록 500표본 평균 평지↓·부스트↑가
      단조로운지, 클램프 경계에서 음수/1 초과가 안 나는지) — **248 → 256** (도중 다른 세션이
      먼저 250→254로 올려 둔 것과 합쳐짐), 실패 0.
- [ ] P-11 달리는 장면. 지금은 결과 목록만 나온다. 지형 조각(직선/코너/경사/터널)을 Blender로 만들고
      코스 데이터에 따라 런타임에 이어 붙인다. **코스 하나하나를 모델링하지 않는다.**
      Blender MCP는 붙어 있지만 Blender가 켜져 있어야 쓸 수 있다

### 펫 뽑기 (pet-gacha.md) — 법적 제약 확인 후 시작

- [x] P-12 `Core/PetGrade.cs` + `Core/PetGachaTable.cs` — **등급 7단계**(일반~초월),
      **뽑기 4종의 확률표와 천장을 전부 한 파일에.** 무료(광고 10회/일) / 일반(레이싱 재화) /
      고급(하루 1 무료+유료, 메인) / 특수(초월 전용). 순수 함수, seed 재현.
      **뽑기마다 천장 카운터가 별개여야 한다** — 합치면 무료로 유료 천장이 돌아 설계가 무너진다
      → 2026-09-18 야간 세션. `PetGrade`(7등급 enum + 종 수·도감 보너스), `PetGachaTable`
      (Open/OpenTen, 고급·특수 천장 80, 10연차 전설 이상 보장). 문서 8절의 "특수 150"은
      3·4절과 어긋난 오기라 80으로 고쳤다. Core.Tests 15개 추가(210→223, 실패 0).
      SaveData 연결·PetFusion·PetCollection(장착·고유 효과)은 아직 — P-13/P-14 몫.
- [x] P-13 `Core/PetFusion.cs` — 조각 환산(같은 등급 3 / 1~4등급 5 / 5등급 8 / 6등급 15).
      **중복이 버려지면 안 된다**
      → 2026-09-18 야간 세션. `ExchangeForSameGrade(fragments)`(3개마다 다른 펫 1마리, 나머지는
      그대로 돌려줌 — 버려지지 않는다) + `PromotionCost(grade)`/`PromotedGrade(grade)`/
      `ExchangeForPromotion(grade, fragments)`(1~4등급 5 / 전설 8 / 신화 15, 초월은 승급 대상이
      아니라 예외). 조각 개수만 계산하는 순수 함수라 SaveData 연결 없이도 시작 가능했다 —
      실제로 몇 개를 갖고 있는지·바꾼 뒤 어떤 펫을 줄지는 여전히 P-14(PetCollection·SaveData) 몫.
      Core.Tests 6개 추가(223→229, 실패 0).
- [x] P-14 `Core/PetCollection.cs` — 도감 보너스(가진 전부) + 장착 보너스(한 마리)
      → 2026-09-19 야간 세션. `CollectionBonus(int[7])`(등급별 "가진 종 수" × 등급별 마리당
      보너스를 합산, 길이·음수·최대치 초과는 예외)와 `EquipBonus(grade)`(1~5등급, `PetGradeInfo`에
      새로 넣은 `EquipBonusFor`를 그대로 읽음 — 표 +8/+14/+22/+35/+55% 그대로). **6·7등급(신화·초월)
      고유 효과는 여기 없다** — `EquipBonus(Mythic/Transcendent)`는 예외를 던진다. 종 48개(신화 30+
      초월 10)의 이름과 효과 값이 아직 안 정해져서다(art-requests.md가 이미 "밤 세션이 혼자 정할
      일이 아니다"로 판단해 둔 그 항목과 같은 블로커). 정해지면 종 ID로 찾는 고유 효과 조회를
      이 클래스에 더하면 된다. Core.Tests 6개 추가(238→244, 실패 0).
      **(2026-09-20 16시 주말 세션) ② 종 ID 목록을 별도로 만들었다 — `Core/PetSpecies.cs`
      신규.** "다음에 할 만한 것" 문단이 제안한 대로 이름·아트 없이 id+등급+계열만으로 124종
      전부의 자리를 잡았다. 1~4등급(16종=4계열×4색)·5등급(20종=4계열×5색+주사)은 계열×행성
      색으로 기계적으로 만들고(`PlanetSpeciesTable.Build()`), 6등급(신화, 계열별 8·8·7·7)·
      7등급(초월, 능력 축 10개)은 PlanetId 없이 자리만(이름·고유 효과는 여전히 Tifania 결정
      대기, 48종). `InGrade(grade)`/`Get(id)`/`PickInGrade(grade, seed)`(그 등급 안에서 종
      하나를 균등 확률로 고름, seed 재현 가능)/`MechanicalDisplayNameKo`(1~5등급만, 6·7등급은
      예외) 제공. **아직 안 한 것 — 이 표를 실제 뽑기 결과에 연결하는 일.** `PetGachaController`가
      `PetGachaResult`에 종 id를 채우고, `SaveData.PetGachaSave`가 등급별 카운트 대신 종별
      보유 여부(중복이면 `PetFusion` 조각으로 자동 전환)를 담아야 진짜 마지막 조각이 된다 —
      셋을 같이 고쳐야 하는 구조 변경이라 다음 세션 몫으로 남긴다. Core.Tests 9개 추가
      (319→328, 실패 0). Unity 참조 없는 순수 C#, 새 파일이라 `.meta`도 새로 만들었다(GUID
      중복 없음 확인).
      **(2026-09-20 17시 주말 세션) 이 연결 작업을 끝냈다 — P-14가 이제 전부 끝났다.**
      `SaveData.PetGachaSave`에 `OwnedSpeciesIds`(종 124개 보유 여부, 새 세이브는 전부 false로
      시작해 마이그레이션 불필요)와 `MarkSpeciesOwned(speciesId)`를 신규 추가 — 처음 얻은
      종이면 도감(`OwnedSpeciesIds`)과 등급별 카운트(`OwnedSpeciesCountByGrade`)를 같이 채우고
      true, 이미 가진 종(중복)이면 false만 돌려준다(등급 안 종이 전부 서로 다른 id라 기존
      `AddOwnedSpecies`와 달리 별도 상한 클램프가 필요 없다 — 그 등급 종 수를 넘게 부를 수가
      없는 구조). 기존 `AddOwnedSpecies(PetGrade)`는 그대로 남겨 뒀다(직접 등급 카운트만
      건드리는 용도로 계속 쓸 수 있음, 종 단위 경로와 안 섞임).
      `PetGachaController`의 네 Pull* 메서드 전부 `PetSpeciesTable.PickInGrade`로 등급 안의
      실제 종을 뽑고, 신규면 도감에 반영·중복이면 `AddShards(grade, 1)`로 조각 1개를 대신
      지급하도록 고쳤다(pet-gacha.md 2절 "중복 → 조각" 그대로, 조각 3개 = 같은 등급 다른 펫).
      등급 확률표 seed와 종을 고르는 seed는 `seed ^ 0x5bd1e995`로 다르게 파생했다
      (`RaceSimulator.cs`가 이미 쓰던 패턴 재사용 — 같은 seed를 그대로 재사용하면 두 추첨이
      상관관계를 가질 수 있어서). 각 Pull* 결과 타입에 `PetPullOutcome`(등급/천장/종
      id/신규 여부)을 새로 만들어 기존 `PetGachaResult`(순수 확률 결과, 종 개념 없음) 자리를
      대신하게 했다 — 화면에서 부르는 곳이 아직 없어서(P-15는 확률표만 직접 읽지 이 컨트롤러를
      안 부름) 호환성 걱정 없이 시그니처를 자유롭게 바꿀 수 있었다.
      `Core.Tests` 3개 추가·2개 재작성 — **328 → 331, 실패 0.** 재작성한 둘은 원래
      "정확히 N마리 늘어난다"를 가정했는데, 종 단위로 바뀌면서 10연차·다수 뽑기 안에서
      같은 종이 두 번 나올 수 있어(생일 문제 — 한 등급에 종이 16~20개뿐) 이제
      "새 종 + 중복(조각)을 합치면 N"으로 확인하도록 고쳐 어떤 seed에도 안전하게 만들었다.
      Unity 참조 없는 순수 C#, 새 파일 없이 기존 파일 2개만 수정 — 컴파일 위험 낮음. **웹에서
      볼 변화 없음**(화면이 아직 이 컨트롤러를 안 부른다, P-15는 확률표만 직접 읽어서 무관).
- [x] P-15 확률 공개 화면. **`PetGachaTable`을 그대로 읽어서 뽑기 4종을 각각 표로 그린다.
      사람이 옮겨 적지 않는다.** 캡슐형(뽑기)과 합성형(`PetFusion`)을 나눠 표시.
      Core.Tests가 "표시값 == 표 값"과 "확률 합 1.0"을 검사한다
      → 2026-09-19 야간 세션(코드만, Unity 없음). `Assets/Scripts/UI/PetGachaOddsUgui.cs` +
      `Assets/Editor/BootstrapPetGachaOddsUgui.cs`(메뉴 `GemRacer/25`) 신규. 카드 다섯 장(무료·
      일반·고급·특수·합성)을 `BootstrapShopUgui`와 같은 ScrollRect 구조로 쌓았다 — 각 카드 줄
      수는 확률표 길이 그대로(무료 5/일반 6/고급 4/특수 3, 합성은 승급 6단계+동급 교환 1줄)
      부트스트랩이 빈 줄만 만들고, `PetGachaOddsUgui.Awake`가 `PetGachaTable.Free()` 등과
      `PetFusion.PromotionCost`를 직접 읽어 채운다 — 숫자를 옮겨 적은 곳이 없어서 표가 바뀌면
      화면도 같이 바뀐다. `MainHudUgui.gachaOddsPanel` 필드 + `Wire("btn-gacha-odds", ...)` 호출도
      추가해 뒀다(U-10 상점과 같은 순서 — 버튼 없는 채로 필드부터 넣고 나중에 버튼만 끼워 넣는다).
      **아직 액션 줄에 여는 버튼이 없다** — 실제 뽑기를 돌리는 화면 자체가 아직 없어서
      (SaveData 연결 전이라 P-12~14는 core만 있다) 지금 일곱 번째 버튼을 끼워 넣는 게 맞는지
      판단하지 않고 남겨 뒀다. **Unity 세션 몫**: `GemRacer/25` 실행 → 카드 다섯 장이 세로
      화면에서 스크롤되는지, 카드마다 줄이 안 잘리는지 확인 → 버튼을 넣기로 하면
      `BootstrapShopUgui.AddShopButtonToActionRow`와 같은 모양으로 `btn-gacha-odds`를
      action-row에 추가하고 `gachaOddsPanel`에 `PetGachaOdds`를 물린다. Core는 안 건드려서
      `Core.Tests` 그대로(244/실패 0, 확률 합 검사는 이미 있었다).
      → **2026-09-19 03:10 Unity 배선 세션에서 씬 배선 완료.** `GemRacer/25` 실행(에러 0) →
      `UI Canvas/Overlays/PetGachaOdds` 생성 → `MainHudUgui.gachaOddsPanel`에 물림 → 저장.
      Play 12초 예외 0, TMP 36칸 중 34칸이 표에서 채워졌고 폰트는 `Pretendard-Regular SDF`라
      한글이 다 읽힌다. 스크롤 content 966 > viewport 828이라 실제로 스크롤되고, `close-button`에
      영구 리스너 1개가 붙어 있어 빠져나올 길도 있다. 세로 540×960 캡처로 눈으로도 확인했다.
      배선 뒤에도 Overlays 자식 11개와 나머지 패널 6칸이 그대로다(`GemRacer/7` 함정 안 건드림).
      남은 자잘한 것 둘: (1) `free-note`·`normal-note`가 빈 문자열이라 두 카드 아래에 빈 칸이
      남는다 — 천장이 없는 두 종이라 적을 말이 없는 것이니 줄 자체를 `SetActive(false)` 하는 편이
      낫다. (2) 루트 배경 알파가 0.97이라 카드 사이 틈으로 뒤 HUD 글자가 희미하게 비친다.
      둘 다 다음 코딩 세션 몫(화면 동작에는 지장 없음). 여는 버튼은 여전히 없다.
      → **2026-09-19 04:10 코딩 세션에서 둘 다 고쳤다.** (1) `PetGachaOddsUgui.FillCapsule`이
      `note == null`이면 `noteLabel.gameObject.SetActive(false)`로 줄 자체를 끈다(런타임 코드라
      씬을 다시 안 세워도 다음 Play부터 바로 적용된다). (2) `BootstrapPetGachaOddsUgui.Bg`의
      알파를 0.97 → 1(완전 불투명)로. **다만 이건 부트스트랩이 만드는 값이라 이미 세워진 씬에는
      바로 안 먹는다** — 다음 Unity 세션이 `GemRacer/25`를 다시 눌러야 반영된다. 그 메뉴는
      `Overlays` 아래 자기 자신(`PetGachaOdds`)만 지우고 새로 만드는 멱등 구조라 다른 화면
      6개는 안 건드리지만, 새로 만들어진 `PetGachaOdds`는 새 인스턴스라 **`MainHudUgui.gachaOddsPanel`
      필드가 끊긴다 — 다시 물려야 한다**(재실행 시 03-10 세션 순서 그대로 반복). Core 변경 없음,
      Unity 컴파일 확인 못 함(API는 `SetActive`·`Color`뿐이라 위험 낮음).
      → **2026-09-19 05:10 Unity 배선 세션에서 반영 완료.** `GemRacer/25` 재실행(콘솔 에러 0) →
      `PetGachaOdds`가 새 인스턴스로 다시 서면서 `MainHudUgui.gachaOddsPanel`이 예상대로 `NULL`이
      됐고, `execute_code`로 다시 물렸다(다른 여섯 패널 참조는 그대로 살아 있는 것 확인). 씬은
      50,719줄 — `GemRacer/7` 함정(5,691줄로 줄어드는 것)은 일어나지 않았다. Play 12초 동안 예외 0,
      게임 뷰 스크린샷으로 눈으로 확인: 배경이 완전 불투명이라 뒤 HUD 글자가 더 이상 안 비치고,
      `free-note`·`normal-note` 두 줄은 `activeSelf=False`로 꺼져 빈 칸이 사라졌다. 한글은
      Pretendard로 정상 출력(TMP 텍스트 36개 중 빈 것은 꺼 둔 노트 2개뿐). 여는 버튼은 여전히 없다 —
      Play 중 Hierarchy에서 `UI Canvas/Overlays/PetGachaOdds`를 켜서 본다.
- [ ] P-16 **초월의 인장** — 티타늄 상자 희귀 드롭 / 행성 클리어 / 시즌 패스 / 유료.
      이게 특수 뽑기의 유일한 입장권이고, 초월이 나오는 유일한 경로다.
      무과금이 주 6장 모아 약 3개월에 첫 초월에 닿는 것을 목표로 잡았다(pet-gacha.md 3·4절).
      → **2026-09-20 15시 주말 세션 — "쓰는 쪽"은 끝냈다. "얻는 쪽"은 하나만 남았다.**
      `SaveData.PetGachaSave`에 이미 있던 `TranscendentSealCount`(전 세션이 미리 만들어 둔 자리)에
      `AddSeal(amount)`를 추가했다 — 0 이하는 무시(`AddShards`와 같은 규칙). 네 경로 중
      **세 개는 전부 고정 수량이라 결정이 필요 없었다**(행성 클리어 5장 / 시즌 패스 주 2장 /
      유료 구매 1장·10장) — 각자의 시스템(P-08 행성 클리어 판정, `SeasonPass.cs`,
      `ShopPurchase.cs`)이 생기면 그 값 그대로 `AddSeal`을 부르면 된다.
      `PetGachaController`에 `PullAdvanced(save, seed, useFreeDaily)`/`PullAdvancedTen(save, baseSeed)`/
      `PullSpecial(save, seed)`를 추가했다 — 인장 소비(1인장=특수 뽑기 1회, pet-gacha.md 3절 그대로)와
      고급 뽑기 하루 무료분·80천장 전부 design에 이미 있는 숫자라 임의로 정한 게 없다.
      **테스트를 쓰다가 실제 버그를 하나 찾아 고쳤다** — `PetGachaTable.OpenTen`의 `Guaranteed`는
      "80천장 도달"과 "10연차 최소 전설 보장" 둘 다 true를 주는데(그 자체는 의도된 동작,
      확정 연출 표시용), 그걸 그대로 믿고 세이브의 피티 카운터를 리셋하면 **최소 등급 보장으로
      확정된 칸에서도 진짜 80천장 진행도가 사라져 버린다**(원래 80번째여야 할 확정이 부당하게
      늦춰짐). `PullAdvancedTen`은 `Guaranteed`를 안 믿고 OpenTen과 같은 식(시작 피티 + 회차
      순번)으로 진짜 피티 도달 여부를 직접 다시 계산한다.
      **여전히 안 정해진 것 — 티타늄 상자 희귀 드롭 확률 하나뿐.** "주 2~3장"이 목표인데, 그
      상자를 주는 챌린지 레이스가 주당 몇 번 열리는지(W2, 레이스 4등급 해금 구조가 아직 설계
      전)를 모르면 상자 하나당 몇 %인지 역산이 안 된다 — 분모(주당 개봉 횟수)를 모르는 채
      분자만 정하면 숫자 하나가 아니라 두 겹의 추측이 된다. **W2가 정해지거나, Tifania가
      "상자 하나당 대략 이 정도"로 직접 정해 주면 그때 `LootReward.RollSeal(type, seed)` 같은
      함수 하나로 바로 끝난다**(다른 세 경로처럼 `LootBoxOpener.OpenAny`가 티타늄 상자를 열 때
      `AddSeal(1)`을 부르는 자리만 추가하면 되는 작은 일이다) — 그 전까지는 인장이 실제로
      안 늘어나니 `PullSpecial`을 써 볼 방법이 없다는 뜻이고, 이건 급할 것 없다(다른 세 경로도
      아직 아무 시스템에서 안 부른다).
      `docs/design/pet-gacha.md`가 말한 "드롭률을 넣고 나면 시뮬레이션으로 확인"은 위 확률이
      정해진 뒤의 다음 단계로 남긴다.
      `Core.Tests`에 10개 추가(AddSeal 누적·0 이하 무시, PullAdvanced 하루 무료 게이트·도감
      반영·80천장, PullAdvancedTen 10연차 보장·단일 10회와 카운터 일치·위 버그의 회귀 테스트,
      PullSpecial 인장 부족·소비·도감 반영·80천장) — **309 → 319, 실패 0**(컨테이너에 dotnet이
      없어서 `apt-get install -y dotnet-sdk-8.0`부터 설치, 매 세션 반복).
      Unity 참조 없는 순수 C#, 기존 파일 2개만 수정(새 파일 없음) — 컴파일 위험 낮음.
      **웹에서 볼 변화 없음**(어느 화면도 아직 이 메서드들을 안 부른다).
- [ ] P-17 펫 아트 124종(등급별 종 수가 16/20/30/10으로 다시 잡히면서 총량도 바뀜 — 아래 참고).
      **7등급 10종이 1순위** — 뽑기 화면에 제일 크게 나온다.
      계열 넷(바퀴족·날개족·광석족·짐꾼족)으로 묶고 등급은 같은 생물이 자란 모습으로 그린다.
      **T-12 해결됨(2026-09-19)** — 1~4등급 각 16종(4계열×4색: 쿼츠·루비·사파이어·아쿠아마린),
      5등급 20종(+주사), 6등급 30종·7등급 10종은 Tifania 지정값. `pet-gacha.md` 2절·7절에
      대응표 대신 이 나눠떨어지는 규칙으로 적혀 있다. **프롬프트 76건은 이미
      `art-requests.md` 대기 중에 다 올라가 있다**(7등급10/1등급4/6등급30/5등급20/2~4등급
      골격12). 1~4등급 색 변종 48장은 새 프롬프트가 아니라 P-18(`recolor_pet.py`)로
      골격에서 뽑는다 — 남은 건 이미지 세션이 대기열을 처리하는 것과, 6·7등급 종 이름·고유
      효과(48종)를 Tifania가 정하는 것뿐이다. 코딩 세션이 할 일은 지금 없다.

## 다음에 할 만한 것 (2026-09-20 09시 주말 세션 갱신 — PetGrade 종 수 불일치 버그 수정)

- **(2026-09-20 09시 세션) `PetGrade.cs`가 T-12 이전 값을 그대로 갖고 있던 버그를 고쳤다.**
  `docs/design/pet-gacha.md` 2절은 2026-09-19에 "계열 4로 나눠떨어지게" 등급별 종 수를
  12/12/14/16/18(합 112)에서 16/16/16/16/20(합 124)으로 고쳤다고 적혀 있는데(T-12 해결),
  실제 코드 `PetGradeInfo.SpeciesCount`/`TotalSpeciesCount`는 여전히 옛 값(112)이었다 —
  09-18 밤에 쓴 코드가 09-19 낮의 설계 수정을 반영 못 받은 것. `PetCollection.CollectionBonus`가
  이 배열로 "보유 종 수가 등급 최대치를 넘는지" 검사하므로, 실제로 펫이 SaveData에 연결되는
  순간(아직 안 됨, 아래 참고) 1~4등급에서 13~16종째를 도감에 채우면 정당한 값인데도 예외가
  터졌을 잠복 버그다. `SpeciesCount`를 새 값으로, `TotalSpeciesCount`를 124로 고치고
  `Core.Tests`의 대응 값(합계 검사·최대치 초과 테스트의 "13개는 예외"→"17개는 예외")도 맞춰
  고쳤다 — **297/실패 0 그대로**(기존 테스트 값만 고쳤을 뿐 새 테스트는 없어 개수 변화 없음).
  `pet-gacha.md` 7절 제목·본문에 남아 있던 "112종" 두 곳도 124로 맞췄다(본문의 실제 나눗셈
  64+20+30+10=124는 이미 맞았고 헤드라인 숫자만 낡아 있었다).
- **펫을 실제로 뽑아서 SaveData에 저장하는 곳이 없던 빈 자리 — ①(SaveData 필드)은
  2026-09-20 10시 세션에서 끝났다.** `PetGachaTable`/`PetFusion`/`PetCollection`은 전부
  순수 함수라 뽑기·합성·도감 계산은 다 되는데, 이걸 실제로 부르고 결과를 `SaveData`에
  쌓는 컨트롤러는 여전히 어디에도 없다(`grep -rn "PetGachaTable\." Assets` 0건 — P-15
  확률 공개 화면도 표만 읽지 뽑기는 안 돌린다). P-03~P-05처럼 여러 세션에 걸쳐 쪼개는
  계획이었고(SaveData 필드 → 무료/일반 뽑기 컨트롤러 → 고급/특수는 P-16 이후), 이번
  세션이 그 첫 조각을 만들었다.
  - **① 끝남 — `SaveData.PetGacha`(`PetGachaSave` 신규, `SaveData.cs`).** 종 ID 데이터가
    아직 없어서(②, 아래 그대로 남음) "보유 펫"을 종 단위로는 못 담는다 — 대신
    `PetCollection.CollectionBonus`·`PetFusion.ExchangeFor*`가 실제로 받는 값(등급별
    "가진 종 수" `OwnedSpeciesCountByGrade`, 조각 수 `ShardsByGrade`, 둘 다 7칸 고정)과
    고급/특수 뽑기 천장 카운터(`AdvancedOpenedSincePity`/`SpecialOpenedSincePity`), 무료
    뽑기 하루 10회 + 고급 뽑기 하루 1회 무료의 리셋 상태(`ResetDailyIfNewDay`,
    `RewardAdTracker.DayIndex` 재사용), 특수 뽑기 입장권(`TranscendentSealCount`)까지
    담았다. `AddOwnedSpecies`는 등급 최대 종 수에서 멈춰서 `CollectionBonus`가 절대
    예외를 못 던지게 막고, `RecordAdvancedPull`/`RecordSpecialPull`은 Guaranteed 여부로
    천장을 올리거나 0으로 되돌린다. "장착 중인 펫"은 이번에 안 넣었다 — 그것도 종
    ID가 있어야 뜻이 있어서 ②와 묶인다. `Core.Tests` 5개 추가, 297 → 302, 실패 0.
  - **② 여전히 열려 있다 — 등급이 나온 뒤 "그 등급 안에서 어느 종이 나오는지" 고르는
    로직.** 지금 `PetGachaResult`는 `Grade`만 있고 종 ID가 없다 — 종 목록 자체도 아직
    데이터로 없다(P-17 아트 요청은 나갔지만 종 "이름·ID" 데이터 테이블은 아직 없음).
  - **③ 임시 형태로 끝남 (2026-09-20 02시 주말 세션) — `Core/PetGachaController.cs`
    신규.** `PullFree(save, seed)`(오늘 무료 한도 확인 → 뽑고 성공하면 등급 도감에
    반영 + 뽑은 횟수 증가, 한도 초과면 안 뽑고 `Success=false`)와 `PullNormal(save, seed)`
    (하루 한도 없음, 항상 뽑힘). 재화 소모(광고 시청 여부·레이싱 재화 비용 확인)는 이
    클래스 책임이 아니다 — 호출하는 쪽이 먼저 확인하고 부른다. **여전히 임시 형태다**
    (②가 없어서) — 등급 도감이 이미 가득 찬 칸에 같은 등급이 또 나오면 `AddOwnedSpecies`가
    조용히 무시해서 그 뽑기는 버려진다. 종 ID가 생기면 "그 등급 안에서 어느 종인지 뽑고,
    이미 있으면 조각으로"(PetFusion 참고)로 바꿔야 한다. 고급/특수 뽑기는 여전히 없음
    (P-16 결정 대기). `Core.Tests` 5개 추가(302 → 307, 실패 0) — 성공 시 도감·카운트
    반영, 한도 초과 시 미반영, 재현성(같은 seed), 도감이 가득 찬 칸의 알려진 한계,
    일반 뽑기는 무료 한도와 무관. Unity 참조 없는 순수 C#, 새 파일이라 `.meta`도
    새로 만들었다(GUID 중복 없음 확인). 실제로 화면에서 부르는 곳은 아직 없다(P-15와
    같은 자리 — 화면 배선은 Unity 세션 몫, 여기 없어도 컴파일에는 지장 없음).


T-10~T-12는 전부 정해져 커밋됐고, P-03~P-05(증폭기 뼈대 → 상자 결과 → 채굴/레이싱카 반영)도
이제 전부 끝났다. A-04(레이스 결과 화면 UI 배선)도 09-19 23시 Unity 세션에서 이미 끝났다.
P-07(행성별 광물 종류)도 구조 확정 + 창고·레시피 메커니즘까지 이번 세션에서 만들었다
(자세한 내용은 위 P-07 항목 참고) — **실제 레시피 값은 B/A/S 부품 정의가 생겨야 채울 수 있어서
남아 있다.**

- **(2026-09-20 02시 세션) P-14 ③(무료/일반 뽑기 컨트롤러)을 임시 형태로 끝냈다** — 위 P-14
  항목 참고. 종 ID(②)가 여전히 없어서 이게 진짜 마지막 조각은 아니다.
- **코드만으로 되는 다음 후보** — P-07의 남은 절반(레시피 값)은 부품 데이터가 없어서 못
  채우고, P-08(행성 클리어 조건)은 Tifania 결정 대기다. 다음 세션은 아래 "바닥났을 때" 후보
  (Core.Tests 경계값 보강, 밸런스 시뮬레이션, 문서 정리)에서 고르거나, B/A/S 등급 부품(루비
  행성 부품 등) 자체를 새로 설계해 DefaultData에 추가하는 걸 검토할 만하다 — 그러면 P-07
  레시피도 같이 채울 수 있다. 종 ID 목록(P-14 ②)을 먼저 설계하는 것도 방법이다 — 이름·아트
  없이 숫자 id + 등급만으로 시작해도 뽑기 결과를 진짜 종 단위로 담을 수 있게 된다.
- **(2026-09-20 03시 세션) `PlanetMineralRecipe`에 부분 차감 버그가 있어 고쳤다.** costs에 같은
  planetId가 두 줄이면(레시피 작성 실수로도 생길 수 있다) `CanAfford`가 각 줄을 보유량과
  따로 비교해서 실제로는 합이 모자란데도 감당 가능하다고 잘못 판정하고, `TrySpend`는 그 상태로
  첫 줄만 깎고 둘째 줄에서 실패하는데도 **호출부에는 성공(`true`)을 돌려줬다** — all-or-nothing
  문서화와 반대로 절반만 깎이는데 성공으로 보인 것. `Merge()`로 같은 planetId를 합쳐서 판정·
  차감하도록 고쳤다. 지금 실제 레시피는 아직 하나도 없어서(위 P-07 항목대로 B/A/S 부품 데이터가
  없다) 플레이에는 영향 없었지만, 레시피 값이 채워지는 순간 터졌을 잠복 버그였다.
  회귀 테스트 2개 추가, **296 → 297, 실패 0**.
- **누적 상한(amplifier.md "정해야 하는 것")은 여전히 미정** — 정해지면 `RigAmplifierSave.Add`/
  `PartAmplifierSave.Add`에서 한 줄씩 자르면 끝나는 작은 일이다.
- **상자에서 나온 증폭기를 어느 칸에 넣을지 고르는 화면**(무작위 배정 / 플레이어 직접 선택)은
  `SaveData.AddAmplifier(slot, bonus)`까지 만들어 뒀으니(채굴·레이싱카 두 칸 다 있음) 슬롯을
  고르는 화면만 있으면 된다 — Unity 세션 몫.
- Unity 세션이 있으면: P-09(행성 선택·이동 화면 uGUI) / U-08(옛 UI Toolkit 루트 정리, 일곱
  화면이 uGUI로 다 옮겨진 뒤가 조건) / P-06(행성별 ToolLevel 분리, 구조 변경이라 컴파일 확인
  필요, 2026-09-18 23시 세션 확인) 중 하나. 위 "상자 개봉 화면 slot 선택"도 여기서 같이 볼 만하다.
- 사람이 직접 볼 시간이 있으면: E-03(새 세이브로 첫 30분 점검, 이미 배송된 화면들이 실제로
  이어져서 동작하는지는 아직 사람이 한 번도 끝까지 눌러본 적이 없다).
- ~~**T-13(웹 배포 막힘)이 여전히 최우선 블로커다**~~ → **풀렸다(2026-09-20).** A+C안 커밋
  `07db953` 이후 run #320·#321·#322가 연달아 `success`고, 07시 Unity 세션이 dev 주소를
  브라우저로 직접 열어 게임이 실제로 뜨는 것까지 확인했다(한글 정상, 튜토리얼 1/4→2/4 클릭
  동작). 남은 확인은 펫 512 해상도 화질 하나 — 그건 Tifania가 봐야 한다.
- **(2026-09-20 15시 세션) P-16(초월의 인장) "쓰는 쪽"을 끝냈다** — 위 P-16 항목 참고.
  `PetGachaController.PullAdvanced`/`PullAdvancedTen`/`PullSpecial` 신규 + 그 과정에서
  10연차 최소 등급 보장이 진짜 80천장 카운터를 잘못 리셋하는 버그를 찾아 고쳤다. **이 세션이
  시작할 때 처음엔 하네스 기본 브랜치(`claude/serene-cannon-8yktil`, 강제 갱신된 옛 `main`
  기준)에서 P-05를 통째로 새로 구현하고 있었다** — 11:0x 세션이 남긴 바로 그 함정을 이번에도
  겪었다(0단계를 하기 전에 `claude/dev`로 안 옮겨 탐). `dotnet run` 293/실패 0까지 확인한
  뒤에야 `git checkout -B claude/dev origin/claude/dev`로 알아챘고, 이미 `claude/dev`에
  더 완성된 형태(레이싱카 다섯 칸까지, "체인 전부 완료")로 들어가 있는 걸 보고 `git stash drop`
  으로 그 작업을 통째로 버렸다(다행히 커밋 전). **이 함정이 세 번째다** — CLAUDE.md와 이
  프롬프트 양쪽에 이미 적혀 있는데도 매번 새로 겪는다. 다음 세션은 fetch 다음 줄에 바로
  `git checkout -B claude/dev origin/claude/dev`부터 칠 것.
  - **다음 후보**: (1) P-16의 진짜 남은 조각(티타늄 상자 드롭률)은 W2(레이스 등급 해금
    구조) 또는 Tifania의 직접 지정이 필요 — 위 P-16 항목 참고. (2) 종 ID 목록(P-14 ②)을
    설계하는 것도 여전히 방법이다. (3) B/A/S 등급 부품(루비 행성용 등) 설계로 P-07 레시피를
    같이 채우는 것. (4) Unity 세션이 있으면 U-08(+B-01) 우선, 그다음 P-06/P-09.
- **(2026-09-20 17시 주말 세션) P-14가 완전히 끝났다** — 종 목록(②)을 실제 뽑기(③)에
  연결하는 마지막 조각까지 끝났다(위 P-14 항목 참고). `PetGachaController`가 이제
  `PetSpeciesTable.PickInGrade`로 실제 종을 뽑고, 중복이면 조각(`AddShards`)으로 대신
  지급한다. **다음 후보**: (1) B/A/S 등급 부품(루비 행성용 등) 설계로 P-07 레시피를
  채우는 것 — 코드만으로 되는 항목 중 가장 뚜렷하다. (2) P-16 티타늄 상자 드롭률은 여전히
  W2 또는 Tifania 결정 대기. (3) Unity 세션이 있으면 U-08(+B-01) 우선, 그다음 P-06/P-09,
  그리고 이제 P-14가 끝났으니 실제 뽑기 화면(재화 소모 → PullFree/PullNormal 호출 →
  결과 연출)도 후보에 들어간다 — `Bootstrap*.cs`부터 코딩 세션이 먼저 써야 한다.
- **(2026-09-21 02시 주말 세션) 위 (1)번 — A/S 등급 쿼츠 부품을 설계해 P-07 레시피를 채웠다.**
  `DefaultData.QuartzEpicParts()`(A등급 5종, `q_*_a`)·`QuartzLegendaryParts()`(S등급 5종,
  `q_*_s`) 신규 — 스탯은 C 대비 A는 4배·S는 8배(B가 C의 2배였던 배수를 그대로 이었다,
  선형 가산이라 2배씩은 과하지 않다는 QuartzAdvancedParts의 판단 재사용). 이름은 A "융합
  석영"(섞어 만든다는 뜻), S는 `QuartzTreasureDefs`의 S등급 보물 이름 "쿼츠의 심장"을
  그대로 가져와 플레이버를 이었다.
  **여기서부터 제작 비용이 단일 정제 광물이 아니다** — `PartCraft.Cost(A/S)`는 여전히
  예외를 던지고(단일 자원 개념 자체가 안 맞아서), 대신 `PartCraft.Recipe(PartGrade)` 신규
  메서드가 `PlanetMineralRecipe`로 쿼츠+루비를 섞어 요구한다(C/B는 반대로 Recipe가 예외 —
  Cost와 Recipe가 서로 배타적인 두 등급 구간을 나눈다). `DefaultData.QuartzEpicRecipe()`
  (쿼츠 180 + 루비 60, 25% 루비)·`QuartzLegendaryRecipe()`(쿼츠 480 + 루비 480, 50%
  루비) — A→S로 갈수록 루비 의존도를 25%→50%로 올려서 "상위 행성 광물이 상위 부품
  제작에 쓰이게 해서 되돌아갈 이유를 만든다"(P-07 원래 취지)를 처음으로 실제 값에
  반영했다. 총량은 PartCostC(15)→PartCostB(60, ×4)가 쓰던 성장 폭(×4)을 그대로 이어
  A 240·S 960으로 잡았다. 전부 첫 값(P4 봇 시뮬레이션에서 재조정 여지 있음, 기존
  B등급 값과 같은 성격).
  `Core.Tests` 5개 추가 — A/S 부품 5종씩 슬롯·등급·스탯 배수·id 안 겹침, `PartCraft.Recipe`
  값과 C/B의 예외 경계, `PlanetMineralRecipe`로 실제 검사·소비하는 통합 테스트(모자란 자원
  1개 차이로 all-or-nothing 확인). **331 → 335, 실패 0.**
  Unity 참조 없는 순수 C#, 기존 파일 2개(`DefaultData.cs`·`PartCraft.cs`)만 수정 — 새
  파일 없음, `.meta` 불필요, 컴파일 위험 낮음.
  **여전히 안 된 것 — Unity 세션 몫.** `MiningController.AvailableParts`가 지금도
  `QuartzStarterParts()` 5종 고정이라 B/A/S 전부 화면에서 여전히 못 고른다(B등급 때부터
  이미 있던 제약, 이번 추가로 새로 생긴 문제 아님) — 등급을 어떻게 노출할지(자동 승급?
  별도 탭?)와 A/S의 `PlanetMineralBank` 연결(정제 광물을 언제 이 창고로 옮겨 담을지)은
  여전히 UI 흐름과 같이 정할 일. `PartCraft.Recipe`를 실제로 부르는 화면도 아직 없다 —
  이번 세션은 메커니즘과 데이터만 채웠다(CourseGenerator·PlanetMineralBank가 먼저
  만들어지고 나중에 UI가 붙은 것과 같은 순서). **웹에서 볼 변화 없음.**

- [x] P-18 `tools/recolor_pet.py`로 1~4등급 색 변종 48장 만들기. **골격 16장이 다 들어온 뒤에.**
      그림을 다시 뽑지 않는다 — 다시 뽑으면 생김새까지 달라져서 "같은 종의 다른 색"으로 안 보인다.
      스크립트는 이미 있고 동작 확인까지 했다(`docs/design/recolor-sample.jpg`).
      HSV에서 색상만 돌리고 명도는 그대로 둬서 입체감이 남는다
      → **2026-09-20 08시 코딩 세션.** 골격 16장(1-common 4 + 2/3/4-base 12) 전부에
      `--planets ruby,sapphire,aquamarine`로 돌렸다(pet-gacha.md가 1~4등급은 쿼츠 포함 4색뿐이라고
      정해서 cinnabar는 뺐다 — 그건 5등급부터). 48장 전부 `python tools/check_alpha.py`
      통과(RGBA·모서리 투명·마젠타 잔상 0). **`.meta`는 이번에 만들지 않았다** — CLAUDE.md 4번
      "`.meta`는 손으로 쓰지 않는다"를 그대로 지켰고, `Assets/Editor/ArtImportSettings.cs`가
      `AssetPostprocessor`라 Resources/Art 아래 `.meta` 없는 새 파일은 Unity가 열 때 자동으로
      Pets 512px·Sprite·압축 설정을 걸어 준다(T-13 그 스크립트 그대로) — 손으로 만들면 오히려
      그 자동 적용을 막을 뻔했다(코드가 "기존 meta가 있으면 안 건드린다"로 판단하기 때문).
      **다음 Unity 세션이 할 일**: 프로젝트를 열어(또는 `refresh_unity`) 48장이 자동 임포트되는지,
      `.meta`가 생기고 `userData: art-import-v1`(512px)로 맞게 걸리는지 확인. 코드·씬은 안 건드림,
      `Core.Tests` 무관(297 그대로).

- [ ] B-01 웹 빌드가 뜨는 순간 콘솔에 `Render Graph Execution error` + `ArgumentException:
      RenderTextureDesc width must be greater than zero`가 46쌍(92줄) 쏟아진다. 2026-09-20
      07시 Unity 세션이 dev 주소에서 확인했다. **로드가 끝나면 더 늘지 않고 화면도 정상**이라
      급한 건 아니지만, 렌더 그래프가 폭 0짜리 RenderTexture를 만들려다 매번 실패하는 것이니
      원인이 있다. 캔버스가 크기를 잡기 전에 생성되는 RT(카메라 타깃이나 RawImage용)부터 본다
      — 확인 시점 캔버스 자체는 1280x720으로 정상이었다.
      → **2026-09-20 08시 코딩 세션 — 유력한 원인을 찾았다(고치지는 않았다).**
      `MainGame.unity`를 grep해 보니 "UI Root (HUD)" 등 **UIDocument 컴포넌트가 열 개 그대로
      살아 있다**(`m_Enabled: 1`) — HUD·업그레이드·제작·레이스·상자·설정·상점·튜토리얼 배너·
      오프라인 보상·화물칸 가득, 전부 옛 UI Toolkit 화면이고 지금은 대응하는 uGUI 화면으로
      대체돼 실제로는 아무것도 안 그린다(U-08이 "지우는 일만 남았다"고 이미 적어 둔 바로 그
      열 개). Unity 6 URP는 `ScreenSpaceOverlay` UIDocument마다 RenderGraph 패스에서
      `Screen.width/height` 크기의 RT를 매 프레임 만드는데, WebGL은 시작 시 JS 쪽이 캔버스를
      아직 못 잡아 `matchWebGLToCanvasSize`가 적용되기 전 몇 프레임 동안 크기가 0일 수 있다 —
      UIDocument가 없으면 이 패스 자체가 없으니 에러도 없다. 46쌍 ≈ 10개 × 4~5프레임과도
      자릿수가 맞는다. **코드에는 RenderTexture를 직접 만드는 곳이 한 군데도 없어서**
      (`grep -rn RenderTexture Assets/Scripts Assets/Editor` 0건) 우리 스크립트 버그가 아니라
      이 열 개가 원인일 가능성이 높다. **고치려면 U-08의 ①~⑤ 순서(씬의 UI Root 열 개부터)를
      Unity 세션이 그대로 밟으면 된다** — 지우고 나서 `GemRacer/7`은 절대 다시 누르지 않는다
      (CLAUDE.md 함정 문서 그대로) 대신 씬을 연 채로 `execute_code`로 열 개만 지우고 저장한다.
      **B-01은 U-08과 같은 작업으로 묶는다** — U-08을 마치면 B-01도 같이 확인될 것으로 본다.
