# UI Toolkit → 일반 UI(uGUI) 이사

2026-09-15 결정. Tifania가 UI Toolkit에 익숙하지 않아 **나중에 직접 고치기 어렵다**고 해서 옮긴다.
인스펙터에서 눈으로 보며 고칠 수 있는 게 이 이사의 목적이다. 그 목적을 해치는 방향으로는 가지 않는다.

## 덤으로 따라오는 것

- **한글.** UI Toolkit 기본 폰트에 한글 글리프가 없어서 빌드에서 글자가 통째로 빈칸이었다.
  uGUI는 TMP 폰트 에셋 하나 물리면 끝난다. `Assets/Fonts/Pretendard-Regular SDF.asset`
  (Dynamic, Pretendard SIL OFL). TMP 기본 폰트로도 지정해 뒀다.
- **코드가 준다.** `MainHud.cs` 253줄 → `MainHudUgui.cs` 105줄. 줄어든 대부분이
  `EnsureXxxHiddenOnce` 다섯 벌이었다. `UIDocument.rootVisualElement`가 늦게 만들어져서
  필요했던 방어 코드인데 uGUI에는 그 문제가 없다.
- **웹 스택 오버플로.** 2026-09-14에 웹 빌드가 첫 프레임에 죽었다. UIDocument 여덟 개가
  한 패널에 붙어 레이아웃 재귀가 깊어진 것이 의심 지점이었다. uGUI로 옮기면 이 경로 자체가 없어진다.
  **확인 필요** — 이사 후 웹 빌드를 열어 봐야 안다.

## 이미 된 것

| 것 | 파일 |
|---|---|
| 한글 폰트 에셋 만들기 | `Assets/Editor/BootstrapKoreanFont.cs` (메뉴 11·12) |
| 이름으로 엘리먼트 찾기 | `Assets/Scripts/UI/UiKit.cs` |
| 패널 여닫기 (UIDocument 대체) | `Assets/Scripts/UI/UiPanel.cs` |
| HUD | `Assets/Scripts/UI/MainHudUgui.cs` + `Assets/Editor/BootstrapHudUgui.cs` (메뉴 13) |
| U-02 업그레이드 | `UpgradeUgui.cs` + `BootstrapUpgradeUgui.cs` (메뉴 16) — 씬 배선까지 완료 |
| U-03 부품 제작 | `CraftingUgui.cs` + `BootstrapCraftingUgui.cs` (메뉴 17) — 씬 배선까지 완료 |
| U-04 레이스 출전 | `RaceEntryUgui.cs` + `BootstrapRaceEntryUgui.cs` (메뉴 18) — 씬 배선까지 완료 |
| U-05 공구 상자 | `LootBoxUgui.cs` + `BootstrapLootBoxUgui.cs` (메뉴 19) — 씬 배선까지 완료 |
| U-06 설정 | `SettingsUgui.cs` + `BootstrapSettingsUgui.cs` (메뉴 20) — 씬 배선까지 완료 |
| U-07 오프라인 보상 | `OfflineRewardUgui.cs` + `BootstrapOfflineRewardUgui.cs` (메뉴 21) — 씬 배선까지 완료 |
| U-10 상점 | `ShopUgui.cs` + `BootstrapShopUgui.cs` (메뉴 22, HUD 버튼은 메뉴 23) — 씬 배선까지 완료 |
| U-11 화물칸 가득 | `CargoFullUgui.cs` + `BootstrapCargoFullUgui.cs` (메뉴 24) — 씬 배선까지 완료 |
| A-17 펫 뽑기 실행 | `PetGachaPullUgui.cs` + `BootstrapPetGachaPullUgui.cs` (메뉴 26) — 씬 배선까지 완료, HUD에 여는 버튼은 아직 없음 |
| A-17 펫 도감 | `PetDexUgui.cs` + `BootstrapPetDexUgui.cs` (메뉴 27) — 씬 배선까지 완료, HUD에 여는 버튼은 아직 없음 |

MainGame 씬에서 옛 UI Toolkit 루트는 **껐다(지우지 않았다)**. 되돌릴 수 있게 남겨 둔 것이고,
다 옮겨지면 그때 지운다.

> **2026-09-17 정정 — 꺼 둔 루트는 여덟 개가 아니라 열 개였다.** 이 문서(와 backlog U-08)가
> "여덟 개"라고 적어 둔 탓에, "일곱 화면이 다 끝났으니 이제 지우면 된다"고 읽히는 상태였다.
> 실제로 꺼져 있던 건 열 개고, 그중 **화물칸 가득 화면(Cargo Full)은 옮긴 적이 없었다** —
> U-01~U-10 어디에도 없다. 꺼진 채로 잊혀서 M-04·M-08·M-09 후속이 빌드에서 안 뜨고 있었다.
> U-11로 옮겼다(`CargoFullUgui.cs` + `BootstrapCargoFullUgui.cs`, 메뉴 24).
> 교훈 한 줄: **지우기 전에 "옛 루트 개수"와 "옮긴 화면 개수"를 직접 세어 맞춰 본다.**
> 씬에서 세는 법 — `FindObjectsByType<UIDocument>(FindObjectsInactive.Include, ...)`.

## 남은 일곱 화면 — 옮기는 방법

순서는 **화면에 먼저 보이는 것부터**. 튜토리얼 → 업그레이드 → 제작 → 레이스 → 상자 → 설정 → 오프라인 보상.

한 화면씩 이렇게 한다. 한 세션에 하나면 충분하다.

1. `Assets/UI/<이름>.uxml`을 열어 **엘리먼트 이름과 계층만** 본다. 색·여백은 `.uss`에 있지만
   그대로 옮기려 애쓰지 않는다 — `BootstrapHudUgui.cs` 위쪽의 색 상수와 간격을 그대로 쓴다.
   화면마다 다른 톤을 쓰면 나중에 손볼 곳이 늘어난다.
2. `Assets/Editor/BootstrapXxxUgui.cs`를 만든다. `BootstrapHudUgui.cs`를 그대로 베끼면 된다.
   `NewRect` / `Stretch` / `MakeText` / `MakeButton` 헬퍼가 거기 있다.
   만든 루트는 `UI Canvas/Overlays` 아래에 붙이고 `UiPanel`을 단다(`hiddenOnStart = true`).
3. 패널 스크립트를 `Assets/Scripts/UI/<이름>Ugui.cs`로 새로 쓴다. **로직은 건드리지 않는다.**
   바뀌는 것은 조회와 표시뿐이다.

   | UI Toolkit | uGUI |
   |---|---|
   | `_root.Q<Label>("x")` | `UiKit.Find<TMP_Text>(transform, "x")` |
   | `_root.Q<Button>("x")` | `UiKit.Find<Button>(transform, "x")` |
   | `_root.Q<VisualElement>("x")` | `UiKit.Find<Image>(transform, "x")` 또는 `UiKit.FindObject(...)` |
   | `button.clicked += f` | `button.onClick.AddListener(f)` |
   | `button.SetEnabled(b)` | `button.interactable = b` |
   | `el.style.display = None/Flex` | `go.SetActive(false/true)` |
   | `el.style.width = Length.Percent(p)` | `image.fillAmount = p / 100f` (Image.Type.Filled) |
   | `el.EnableInClassList("selected", b)` | 색을 직접 바꾸거나 Toggle 사용 |
   | `label.text = s` | `tmpText.text = s` (그대로) |

3-1. **화면을 꽉 채우는 패널에는 닫기 버튼을 반드시 넣는다.** (2026-09-15 U-02에서 걸렸다)
   루트에 `Image`를 붙여 뒤로 클릭이 새는 걸 막으면 HUD의 여는 버튼까지 같이 가려진다.
   그러면 한 번 열었을 때 빠져나올 길이 없다. 맨 아래에 `close-button`("닫기")을 만들고
   `UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(panel.Hide))`로
   묶는다 — 영구 리스너라 인스펙터 OnClick 칸에 보인다(그게 이 이사의 목적이다).
   튜토리얼 말풍선처럼 루트에 `Image`가 없어 클릭이 통과하는 패널은 필요 없다.
   **U-04(레이스 출전)에서 같은 구멍이 하위 화면 하나에만 또 있었다** — entry-view/anim-view/
   result-view처럼 한 패널 안에 화면을 꽉 채우는 뷰가 여럿이면(SetActive로 갈아 끼우는 구조)
   **뷰마다 각각** 확인해야 한다. result-view엔 원래 UXML에 닫기가 있었지만 entry-view엔
   없어서 새로 넣었다(`entry-close-button`). 다음 화면을 옮길 때도 뷰가 하나가 아니면 전부 훑는다.

3-2. **`LayoutElement`로 높이를 고정할 때는 `flexibleHeight = 0f`도 같이 못 박는다.**
   (2026-09-16 U-04 배선에서 걸렸다 — 24px 막대가 130px로 부풀었다)
   `LayoutElement`의 min/preferred/flexible 기본값은 **-1이고 그건 "무시"라는 뜻**이다.
   `LayoutUtility`는 값이 음수인 항목을 우선순위 비교 **전에** 건너뛰기 때문에,
   `flexibleHeight`를 안 정해 두면 우선순위가 낮은 `HorizontalLayoutGroup`/`VerticalLayoutGroup`이
   보고하는 값이 대신 쓰인다. 그 그룹에 `childForceExpandHeight = true`가 켜져 있으면
   flexibleHeight가 1 이상으로 보고되고, **부모 세로 그룹이 남은 높이를 그 줄들에 나눠 준다.**
   그래서 `preferredHeight = 24`라고 써 놨는데도 줄이 화면을 가득 메운다.
   줄 안의 자식이 제 높이를 지켜야 하면 그 줄의 `childForceExpandHeight`도 false로 둔다.
   지금 `childForceExpandHeight = true`가 남아 있는 곳: `BootstrapHudUgui.cs:101`·`149`,
   `BootstrapSettingsUgui.cs:106`·`129` — U-06 배선할 때 확인할 것.
   **U-05에서 실제로 또 터졌다(2026-09-16 배선)** — 72px로 잡은 `result-card`가 224px가 됐다.
   `BootstrapLootBoxUgui.cs`는 `result-card`·`close-button`뿐 아니라 `MakeHeaderText`/`MakeButton`
   **헬퍼 자체**에 `flexibleHeight = 0f`를 못 박아서 고쳤다. 앞으로 부트스트랩을 새로 쓸 때도
   높이를 정한 `LayoutElement`를 만드는 헬퍼에 아예 못 박아 두는 편이 낫다 — 부르는 자리마다
   기억하는 것보다 새지 않는다. 카드 안쪽 그룹의 `childForceExpandHeight`는 라벨을 세로 가운데로
   두려고 true로 남겨도 된다. 바깥쪽(카드 자신)의 `flexibleHeight`만 못 박으면 그룹이 보고하는
   값은 더 이상 쓰이지 않기 때문이다.

3-3. **뷰를 SetActive로 갈아 끼우는 패널은 전환 함수마다 "나머지 전부"를 꺼야 한다.**
   (2026-09-16 U-04 배선에서 걸렸다) `RaceEntryUgui.ShowResultView`가 `entry-view`만 끄고
   `anim-view`를 안 꺼서 결과 글자 위에 연출 막대가 그대로 겹쳐 보였다. 뷰가 둘일 때는
   "하나 켜고 하나 끄기"로 넘어가지만 셋이 되면 바로 새는 자리다. 뷰가 셋 이상이면
   전환 함수를 하나씩 **다 눌러 보고** 켜진 뷰가 하나인지 확인한다.

3-4. **HUD 자체(`BootstrapHudUgui.Build`, `GemRacer/13`)는 화면을 하나라도 배선한 뒤에는
   다시 누르지 않는다.** (2026-09-16 U-10에서 발견 — 옛 UI Toolkit HUD에 있던 `btn-shop`이
   uGUI로 옮기며 빠져 있었다) 그 메뉴는 "UI Canvas"가 이미 있으면 통째로 지우고 다시 만든다.
   `Overlays` 밑에는 각 화면이 배선되면서 자식으로 붙는데, HUD를 다시 세우면 그 화면들과
   `MainHudUgui`의 패널 연결(craftPanel 등, 씬에만 있는 인스펙터 데이터)이 전부 같이 사라진다.
   HUD에 버튼 하나를 더 끼워 넣어야 하면(`BootstrapShopUgui.AddShopButtonToActionRow`,
   `GemRacer/23`처럼) 대상 노드(`action-row`)만 찾아 그 자식 하나만 지우고 다시 만드는
   애처블(additive) 메뉴를 새로 만든다 — 나머지는 손대지 않는다.

3-5. **`Refresh()`를 `Update()`에서 부르는 패널은 라벨·색이 "다음 프레임"에 바뀐다.**
   (2026-09-16 U-06 배선에서 헷갈렸다) `Crafting`·`LootBox`·`Settings`가 다 이 패턴이다.
   `button.onClick.Invoke()`로 눌러 놓고 **같은 호출 안에서** 라벨을 읽으면 아직 옛 값이라
   "안 바뀐다"고 오판하게 된다. 값 자체(`AudioListener.volume`, `Application.targetFrameRate`)는
   그 자리에서 바뀌어 있으니 그쪽을 보거나, 표시를 보려면 한 프레임 뒤에 다시 읽는다.

3-6. **가로로 등분하는 줄(action-row)에 버튼을 하나 더 끼우면 옆 라벨이 잘린다.**
   (2026-09-17 U-10 배선에서 실제로 봤다) `HorizontalLayoutGroup`이 폭을 등분하므로
   다섯 칸 96.8px이 여섯 칸 79.3px이 됐고, 20pt "업그레이드"는 86.4px가 필요해서
   `TextOverflowModes.Ellipsis`로 "업그레이…"가 됐다. **버튼을 더할 때는 그 줄의 다른 라벨도
   같이 본다.** 폭을 하나씩 재서 맞추는 것보다 TMP 자동 축소(`enableAutoSizing = true`,
   `fontSizeMin = 14`, `fontSizeMax = 20`)를 켜 두는 편이 안 샌다 — 잘리는 대신 줄어든다.
   지금 `BootstrapHudUgui.MakeButton`과 `BootstrapShopUgui.AddShopButtonToActionRow`
   양쪽에 켜 뒀고, 메뉴 23은 이미 있던 라벨도 같은 설정으로 맞춰 준다.
   `preferredWidth`가 칸 폭보다 크면 잘린다 — 새 버튼을 넣었으면 그 줄 전체를 한 번 찍어 본다.

3-7. **이미 배선된 화면의 부트스트랩을 다시 누르면 `MainHudUgui` 연결이 끊긴다 — 누른 뒤 반드시
   다시 물린다.** (2026-09-18 E-02 배선에서 실제로 끊겼다) 3-4는 HUD(`GemRacer/13`) 이야기지만
   화면 쪽 부트스트랩도 같은 구조다 — `BootstrapCraftingUgui.Build`는 맨 앞에서
   `Overlays/Crafting`을 찾아 `DestroyImmediate`하고 새로 만든다. 그러면 `MainHudUgui.craftPanel`이
   가리키던 `UiPanel`이 사라져 **칸이 비고, HUD의 그 버튼은 조용히 꺼진 채로 남는다**(4번 항목의
   "연결 안 하면 꺼진 채로 남는다"가 그대로 재현된다). 밤 세션이 부트스트랩에 요소를 하나
   더한 뒤 "다시 눌러야 반영된다"고 남겨 두는 경우가 이 자리다. **순서**: 메뉴 실행 → 해당
   `UiPanel` 다시 대입 → 씬 저장 → Play로 그 버튼이 실제로 열리는지 확인.

4. `MainHudUgui`의 해당 `UiPanel` 칸에 연결한다. 연결 안 하면 그 버튼은 **꺼진 채로 남는다** —
   일부러 그렇게 뒀다. 빠뜨린 걸 화면에서 바로 알 수 있다.
5. 에디터에서 Play로 열어 보고, 한글이 나오는지·버튼이 눌리는지 확인한 뒤 커밋한다.

## 하지 말 것

- **씬 YAML을 손으로 쓰지 않는다.** 계층은 반드시 `Assets/Editor`의 부트스트랩 코드로 만든다
  (CLAUDE.md 규칙). 그래야 다시 만들 수 있고 충돌이 안 난다.
- `ResponsiveLayout.cs`를 uGUI로 옮기지 않는다. `CanvasScaler`가 그 일을 한다
  (기준 540x960, MatchWidthOrHeight 0.5). 화면 크기를 직접 읽어 클래스를 갈아 끼우는 코드는 없앤다.
- 일곱 개를 한 번에 옮기지 않는다. 하나 옮기고 Play로 확인하고 커밋한다.

## 에디터 없는 세션(밤·새벽)이 어디까지 할 수 있나 — 꼭 읽을 것

밤 세션에는 Unity 에디터가 없다. 그래서 **코드는 다 쓸 수 있지만 계층은 못 세운다.**
메뉴(`GemRacer/13` 같은 것)를 실행할 방법이 없기 때문이다. 경계를 분명히 해 둔다.

**에디터 없이 되는 것 — 여기까지 하고 커밋한다.**

- `Assets/Editor/BootstrapXxxUgui.cs` 작성. `BootstrapHudUgui.cs`를 베껴 쓰면 된다
- `Assets/Scripts/UI/XxxUgui.cs` 작성. 변환표대로 조회·표시만 바꾸고 로직은 그대로
- 코어 수식을 건드렸으면 `Core.Tests`로 확인

**에디터가 있어야 되는 것 — 하지 말고 남겨 둔다.**

- 메뉴 실행해서 씬에 계층 세우기
- `MainHudUgui`의 `UiPanel` 칸 연결 (씬 저장이 필요하다)
- Play로 한글·버튼 확인

그래서 밤 세션은 한 화면을 옮겼으면 **daily 파일 맨 위 "오늘 웹에서 확인할 것"에
"Unity 세션에서 `GemRacer/N` 실행 필요"라고 한 줄 남긴다.** 그리고 backlog의 해당 U 항목은
`- [x]`가 아니라 **`- [?] 코드까지 완료, 씬 배선은 Unity 세션 필요`** 로 바꾼다.
그래야 다음에 Tifania가 Unity를 켤 때 무엇을 눌러야 하는지 한눈에 보인다.

씬 YAML을 손으로 써서 이 경계를 넘으려 하지 않는다. 그렇게 만든 씬은 반드시 어긋난다.

## 테스트용 시간 배속 (2026-09-15)

방치형이라 정상 속도로는 화면을 들여다봐도 아무 일도 안 일어나는 것처럼 보인다
(첫 화물칸이 차는 데 90분을 잡아 뒀다). 그래서 주소 뒤에 배속을 붙일 수 있게 했다.

    https://planetracer-daz.pages.dev/?fast=10     ← 10배속
    https://planetracer-daz.pages.dev/             ← 정상 속도

`Assets/Scripts/UI/DebugTimeScale.cs`가 `Application.absoluteURL`에서 `fast=` 를 읽어
`Time.timeScale`에 넣는다. 1~100 사이만 받고, 값이 없으면 아무 일도 안 한다.
MainGame 씬의 `GameFlow`에 붙어 있다.

**출시 전에 빼야 한다.** 지금은 테스트가 급해서 항상 켜 뒀다.
파일 안의 `#if UNITY_WEBGL && !UNITY_EDITOR` 블록을 `GEMRACER_DEBUG` 심볼로 바꾸면 된다.
