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

MainGame 씬에서 옛 UI Toolkit 루트 여덟 개는 **껐다(지우지 않았다)**. 되돌릴 수 있게 남겨 둔 것이고,
일곱 화면이 다 옮겨지면 그때 지운다.

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

4. `MainHudUgui`의 해당 `UiPanel` 칸에 연결한다. 연결 안 하면 그 버튼은 **꺼진 채로 남는다** —
   일부러 그렇게 뒀다. 빠뜨린 걸 화면에서 바로 알 수 있다.
5. 에디터에서 Play로 열어 보고, 한글이 나오는지·버튼이 눌리는지 확인한 뒤 커밋한다.

## 하지 말 것

- **씬 YAML을 손으로 쓰지 않는다.** 계층은 반드시 `Assets/Editor`의 부트스트랩 코드로 만든다
  (CLAUDE.md 규칙). 그래야 다시 만들 수 있고 충돌이 안 난다.
- `ResponsiveLayout.cs`를 uGUI로 옮기지 않는다. `CanvasScaler`가 그 일을 한다
  (기준 540x960, MatchWidthOrHeight 0.5). 화면 크기를 직접 읽어 클래스를 갈아 끼우는 코드는 없앤다.
- 일곱 개를 한 번에 옮기지 않는다. 하나 옮기고 Play로 확인하고 커밋한다.
