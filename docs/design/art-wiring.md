# 아트 배선 — 뽑아 둔 그림을 실제 화면에 붙이는 일 (2026-09-20 신설)

Tifania: "이미지 애셋 만든 거 이런 건 하나도 적용이 안 되어 있는 것 같아서 물어봤어."

맞는 지적이었다. 아래가 그날 확인한 것이다.

## 0. 지금 상태 — 181장 중 0장이 붙어 있다

`PlanetRacer/Assets/Resources/Art` 아래에 **PNG 181장**이 들어가 있다.
그런데 게임 코드에서 그림을 불러오는 곳은 프로젝트 전체에 **딱 한 군데**다.

    Scripts/UI/ArtViewer.cs:78
        var sprites = Resources.LoadAll<Sprite>("Art")...

이건 주소에 `?art=1` 을 붙였을 때만 열리는 **확인용 갤러리**다. 평소에는 아예 안 뜬다.
실제 화면 열세 개(업그레이드·제작·상자·상점·HUD·튜토리얼·설정·레이스·오프라인 보상·
화물칸 가득·뽑기 확률…)는 전부 유니티 기본 회색 박스를 쓴다.

    img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

즉 **그림은 빌드 용량만 차지하고 플레이어에게는 한 장도 닿지 않았다.**
T-13에서 169MB를 압축하느라 고생한 게 바로 이 181장이다. 비용은 다 냈고 효과는 없었다.

### 왜 이렇게 됐나

밤 세션이 두 갈래인데 서로 안 만났다. **이미지 세션**은 매일 그림을 뽑아 폴더에 넣었고,
**배선은 유니티 에디터가 필요한 일**이라 별도 세션 몫이었는데 그쪽은 UI 이사 → 빌드 용량(T-13)
→ 배포 막힘에 계속 잡아먹혔다. backlog에도 "실제 아트 적용"이 **P2 W3** 한 줄로만 있었고
P2는 아직 시작 전이다. 그래서 아무도 집어 가지 않았다.

**이 문서가 그 한 줄을 대신한다.** 아래 표만 보고 집어 가면 판단할 게 없도록 적는다.

## 1. 붙이는 방법 — 규칙 하나만 지킨다

그림은 `Resources` 아래에 있으니 **런타임에 경로로 부른다.** 확장자는 빼고, `Resources/` 다음부터 쓴다.

    var sp = Resources.Load<Sprite>("Art/Icons/icon-raw-mineral");
    if (sp != null) image.sprite = sp;     // 없으면 회색 박스 그대로 둔다

**`null` 검사를 반드시 넣는다.** 그림이 아직 안 들어온 자리에서 화면이 통째로 죽으면 안 된다.

화면 구조는 **부트스트랩(Editor)이 만들고, 패널 스크립트(Scripts/UI)가 이름으로 찾는다**
(`UiKit.Find<T>(transform, "이름")`). 그래서 배선은 늘 두 군데를 같이 고친다.

1. `Assets/Editor/Bootstrap*Ugui.cs` — 그 줄에 `Image` 를 하나 더 만들고 **이름을 붙인다**
2. `Assets/Scripts/UI/*Ugui.cs` — `UiKit.Find<Image>(transform, "그 이름")` 으로 잡고 스프라이트를 넣는다

아이콘 자리 이름은 **기존 이름 규칙을 따른다** — 라벨이 `tool-level` 이면 아이콘은 `tool-icon`.

> 부트스트랩을 고친 뒤에는 유니티에서 해당 메뉴(`GemRacer/...`)를 다시 실행해야 씬에 반영된다.
> 그림 임포트 설정은 `Editor/ArtImportSettings.cs`가 자동으로 잡는다(Sprite·투명·crunch).

## 2. 아이콘 18종 — 붙을 자리가 이미 다 있다 (A-16, 제일 먼저)

화면을 새로 만들 필요가 없다. 기존 줄에 아이콘 자리만 하나씩 더 만들면 된다.

| 그림 (`Art/Icons/`) | 화면 | 붙일 자리 | 옆에 있는 기존 엘리먼트 |
|---|---|---|---|
| `icon-raw-mineral` | 업그레이드 · HUD | `currency-raw-icon` · `mineral-icon` | `currency-label` · `mineral-count` |
| `icon-refined-mineral` | 업그레이드 | `currency-refined-icon` | `currency-label` |
| `icon-gear-tool` | 업그레이드 | `tool-icon` | `tool-level` |
| `icon-gear-cargo` | 업그레이드 | `cargo-icon` | `cargo-level` |
| `icon-gear-engine` | 업그레이드 | `engine-icon` | `engine-level` |
| `icon-blueprint` | 업그레이드 (제련소 줄) | `refinery-icon` | `refinery-level` |
| `icon-box-rusty` | 상자 | `rusty-icon` | `rusty-count` |
| `icon-box-steel` | 상자 | `steel-icon` | `steel-count` |
| `icon-box-titanium` | 상자 | `titanium-icon` | `titanium-count` |
| `icon-part-engine` | 제작 | `engine-icon` | `engine-name` |
| `icon-part-tire` | 제작 | `tire-icon` | `tire-name` |
| `icon-part-suspension` | 제작 | `suspension-icon` | `suspension-name` |
| `icon-grade-c/b/a/s` | 제작 | `<부품>-grade-badge` | `<부품>-state` |
| `icon-fuel` | HUD · 레이스 출전 | `fuel-icon` | 연료 게이지 |
| `icon-key` | 상자 · 상점 | `key-icon` | 열쇠 수량 |

**제련소 줄에 `icon-blueprint`를 쓰는 건 임시다.** 제련소 전용 그림이 아직 없다.
전용 아이콘(`icon-refinery`)을 `art-requests.md` 대기열에 올려 두고, 들어오면 갈아 끼운다.

**제작 화면의 `body` · `booster` 두 줄은 아이콘이 없다.** `icon-part-body` ·
`icon-part-booster` 도 대기열에 올린다. 그때까지 그 두 줄은 회색 박스로 둔다.

**등급 뱃지 넷은 부품 등급에 따라 골라 넣는다** — `PartGrade`를 문자로 바꿔
`Art/Icons/icon-grade-{c|b|a|s}` 를 만들어 부르면 된다. 네 장을 미리 캐시해 둘 것.

### 업그레이드 화면이 본보기다 (2026-09-20에 먼저 끝냈다)

여섯 자리(`currency-raw-icon` · `currency-refined-icon` · `refinery-icon` · `tool-icon` ·
`cargo-icon` · `engine-icon`)를 실제로 붙여 봤다. 남은 화면도 **똑같은 모양으로 간다.**

1. 부트스트랩에 `MakeLine(...)` 으로 **가로 한 줄**을 만들고 그 안에 `MakeIcon("{prefix}-icon", ...)`
   과 기존 글자를 나란히 넣는다. 세로로 쌓던 줄을 한 겹 감싸는 것뿐이라 나머지 배치는 안 건드린다.
   `MakeIcon`은 유니티 기본 스프라이트를 자리 표시자로 넣어 둔다 — 그림이 없어도 간격이 안 흔들린다.
2. 패널 스크립트에 `SetIcon("자리 이름", "그림 이름")` 한 줄씩. 자리가 없거나 그림이 아직
   없으면 조용히 넘어간다(`UiKit.Find(..., warnIfMissing: false)` + `Resources.Load` null 검사).

**카드 높이를 먼저 계산하고 아이콘 크기를 정한다.** 업그레이드 카드는 `CellHeight = 168`이라
머리줄 28(아이콘 24) + 효과 60 + 버튼 44 + 여백 24 + 줄 사이 12 = 168로 딱 맞췄다.
아이콘을 키우면 카드가 넘친다 — 화면마다 이 계산을 다시 한다.

**글자를 쪼개야 하는 자리가 있다.** 업그레이드 머리글은 "원석 0.0 · 정제 광물 0.0" 한 덩어리라
아이콘 둘을 끼울 수가 없었다. `currency-raw-label` · `currency-refined-label` 둘로 나누고,
옛 `currency-label`도 찾으면 그대로 채우게 남겨 뒀다(아직 안 고친 씬에서 빈 화면이 되지 않게).

**메뉴를 다시 실행하면 그 화면의 `UiPanel` 참조가 끊긴다.** `GemRacer/16`은 옛 `Upgrade`를
지우고 새로 만들기 때문에 `MainHudUgui.upgradePanel`이 `null`이 된다 — 다시 물리지 않으면
HUD의 그 버튼이 아무 반응도 안 한다. **배선 후 반드시 해당 칸을 다시 연결하고 씬을 저장한다.**

## 3. 화면 자체가 아직 없는 것들

여기 그림들은 **붙일 자리가 없어서** 못 붙였다. 화면을 만드는 일이 먼저다.

| 그림 | 장수 | 필요한 화면 | backlog |
|---|---|---|---|
| `Art/Pets/**` | 123 | 펫 뽑기 화면 · 펫 도감 | A-17 |
| `Art/Planets/planet-*` | 6 | 행성 선택 / 워프 흐름 | A-18 (P2 W1과 겹침) |
| `Art/Cutscenes/arrive-*` | 5 | 행성 도착 컷신 재생 | A-19 |
| `Art/Cutscenes/opening` · `first-race-win` | 2 | 오프닝 · 첫 승리 연출 | A-19 |
| `Art/Rigs/rig-tiers-sheet` | 1 | 채굴차 티어 외형 (스프라이트 **시트**라 잘라야 한다) | A-20 |

펫 쪽은 **코드는 이미 있다** — `PetGachaController` · `PetGachaTable` · `PetCollection` ·
`PetFusion` · `PetGrade`. 화면만 없다. 확률 공개 화면(`PetGachaOddsUgui`)은 있으니
그 옆에 뽑기 화면을 붙이는 모양이 자연스럽다.

## 4. 펫 폴더에 남아 있던 옛 파일 26장 — 지웠다 (2026-09-20)

`Art/Pets/` **바로 아래**에 `pet-t1-wheel.png` · `pet-t7-wing-prism.png` 같은 옛 이름 규칙
파일이 26장 있었다. 등급 폴더(`1-common` … `7-transcend`)의 123장과 별개라서 `find`가
181장으로 셌고, `?art=1` 갤러리에도 두 벌로 나왔다.

**지우기 전에 확인한 것 — 이건 복사본이 아니었다.** 26장을 등급 폴더 123장과 md5로 맞춰
봤더니 **한 장도 같지 않았다.** 같은 자리를 나중에 다시 그린 것이지 같은 파일이 아니다.
예: 옛 `pet-t7-wing-herald` ↔ 새 `7-transcend/beacon-herald`,
옛 `pet-t2-wheel` ↔ 새 `2-base/wheel`.

그럼에도 지운 근거:

- **코드에서 이름으로 부르는 곳이 한 군데도 없다.** `Scripts` · `Editor` · `Packages` ·
  `Core.Tests` 전부 `pet-t` 문자열이 없다.
- **T-12에서 구조가 4계열 × N색으로 정해지면서** 등급 폴더 쪽이 살아 있는 이름이 됐다.
  장부(`art-requests.md`)에도 새 경로 항목이 50개(7-transcend 18 + 1~4등급 32) 들어와 있다.
- 26장 26.2MB가 빌드 용량을 그대로 먹고 있었다 — T-13에서 줄이려던 바로 그 용량이다.

**`git rm`으로 지웠다.** 작업 폴더에서만 사라지고 커밋 이력에는 그대로 남으니, 나중에
옛 그림이 필요하면 `git show <커밋>:<경로> > 파일` 로 언제든 꺼낼 수 있다.

장부의 옛 항목(`### [x] Resources/Art/Pets/pet-t*.png`)은 **기록이라 지우지 않았다.**
대신 그 절 머리에 "이 경로의 파일은 2026-09-20에 지웠고 살아 있는 건 등급 폴더 쪽"이라고 적어 뒀다.

## 5. 순서

1. **A-16 아이콘 18종** — 화면 안 만들어도 되고, 열어 보면 바로 다른 게임으로 보인다
2. ~~A-21 펫 폴더 옛 파일 26장 정리~~ — **2026-09-20 완료** (26.2MB 감소)
3. **A-17 펫 뽑기 화면** — 123장이 한꺼번에 살아난다. 수익 화면이기도 하다
4. **A-18 행성 선택** — P2 W1과 같은 일이라 P2 시작과 함께
5. **A-19 컷신 · A-20 채굴차 티어** — 연출이라 뒤로
