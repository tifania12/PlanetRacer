# 보석 행성 레이서 (Gem Planet Racer) — 작업 규칙

방치형 레이싱 게임. 보석 이름의 행성이 스테이지. 채굴차가 방치 채굴 → 광물로 레이싱카 부품 제작 → 행성 그랑프리 1등 → 다음 행성.
모바일(iOS/Android) + Steam 동시 출시. 목표 2027-02-16. 기획 요약은 `docs/GDD.md`, 전체는 Claude 아티팩트 "보석 행성 레이서 기획서".

## 저장소 구조

```
PlanetRacer/                Unity 6 프로젝트 (6000.3.10f1, URP). PC에서 저장소 루트는 E:\Unity\PlanetRacer
  Packages/com.bax.gemracer.core/Runtime/   순수 C# 코어. UnityEngine 참조 금지 (asmdef noEngineReferences)
  Assets/Scripts/           MonoBehaviour, UI, 연출. 코어를 호출만 한다
  Assets/Editor/            에디터 부트스트랩 (씬·프리팹·머티리얼을 코드로 생성하는 MenuItem)
  Assets/Screenshots/       검증용. .gitignore에 있으니 커밋되지 않는다
Core.Tests/                 dotnet 콘솔 테스트 러너. `cd Core.Tests && dotnet run`. NuGet 사용 금지(오프라인)
docs/GDD.md                 기획 요약
docs/backlog.md             일 단위 작업 목록. 야간·새벽 세션은 여기서 다음 항목을 집는다
docs/daily/YYYY-MM-DD.md    그날 작업 보고 + 아침 테스트 가이드
docs/decisions.md           결정 기록
```

## 반드시 지킬 것

1. 게임 규칙·수식은 전부 `PlanetRacer/Packages/com.bax.gemracer.core/Runtime`에 순수 C#으로 쓴다. 서버가 같은 코드로 대전 결과를 검증하므로 코어에서 `UnityEngine`, `Random`, `DateTime.Now`를 쓰지 않는다. 난수는 `DeterministicRandom`, 시간은 인자로 받는다.
2. 코어를 고치면 `Core.Tests/Program.cs`에 테스트를 추가하고 `dotnet run`이 `실패 0`으로 끝나는지 확인한다. 실패한 채로 커밋하지 않는다.
3. 씬·프리팹·머티리얼은 `.unity`/`.prefab` YAML을 손으로 쓰지 말고, `Assets/Editor/Bootstrap*.cs`에 `[MenuItem("GemRacer/...")]`로 생성 코드를 쓴다. 같은 메뉴를 다시 눌러도 같은 결과가 나오게(멱등) 만든다.
4. `.meta` 파일은 손으로 쓰지 않는다. Unity가 생성한 뒤에는 반드시 커밋한다 — `.meta`가 빠지면 다른 사람이 열 때 참조가 전부 끊긴다. 이미 있는 `.meta`는 지우거나 GUID를 바꾸지 않는다.
5. 시간에 비례해야 하는 것(카메라 추적, 이동, 보간)은 프레임 수에 의존하면 안 된다. 프레임당 고정 계수 `Lerp(a, b, 0.15f)` 대신 `1 - Mathf.Exp(-k * Time.deltaTime)`를 쓴다. 모바일 30fps와 PC 고주사율에서 감이 달라지면 안 된다.
6. Unity 6 LTS, URP, UI Toolkit, Input System. 입력은 탭/클릭만. UI는 세로(9:16) 단일 레이아웃, PC는 세로 창.
7. 한국어로 쓴다. 코드 주석·문서·커밋 메시지 모두. 문장은 사람이 말하듯 자연스럽게.
8. 하나의 작업 = 하나의 커밋. 메시지 첫 줄은 `[D07-N] 채굴차 자동 주행 연출` 형식 (backlog 항목 ID).

## Unity 에디터에 닿을 수 있는지는 세션마다 다르다

- **예약 세션(야간 23:00 / 새벽 06:00)**: 클라우드에서만 돈다. Unity 에디터가 없다. 그래서 씬은 부트스트랩 코드로만 만들고, 컴파일 여부를 확인할 수 없으니 확실하지 않은 API는 쓰지 말고 daily 파일에 "아침 확인 필요"로 남긴다. 검증은 `Core.Tests`의 `dotnet run`까지다.
- **Tifania와 같이 하는 대화형 세션**: PC가 연결돼 있으면 Unity MCP(`unityMCP__*`)로 에디터를 직접 쓸 수 있다. 이때는 실제로 확인하고 나서 보고한다 — `refresh_unity`로 컴파일 → `read_console`로 에러 확인 → `execute_menu_item`으로 부트스트랩 실행 → `manage_editor play` + `manage_camera screenshot`으로 눈으로 확인.
- 주의: `manage_scene get_hierarchy`는 플레이 중 값이 갱신되지 않을 때가 있다. 실행 중 좌표를 정확히 볼 때는 `execute_code`로 직접 읽는다.

## 야간(23:00 KST) 세션이 하는 일

1. `git pull` 후 `docs/backlog.md`에서 체크 안 된 첫 `-N` 항목을 집는다. 그 전날 `-M` 항목이 남아 있으면 그것부터.
2. 구현한다. 코어 변경이면 테스트까지. Unity 스크립트면 부트스트랩·테스트 절차까지.
3. `docs/daily/YYYY-MM-DD.md`를 만든다(날짜는 KST 기준 다음 날 아침). 항목: 한 일 / 아침에 Tifania가 할 것(단계별, 5분 안) / 확인 포인트 / 막힌 것·결정 필요 / 아침 피드백(빈칸).
4. backlog 항목을 `[x]`로, 커밋, push.

## 새벽(06:00 KST) 세션이 하는 일

1. `git pull`. 야간 세션이 남긴 것을 이어서 마무리하거나, 다음 `-M` 항목을 한다. 새 큰 기능을 시작하지 않는다.
2. `dotnet run` 통과 확인. `docs/daily` 파일의 아침 테스트 가이드를 다시 읽고 실제 파일 경로·메뉴 이름이 맞는지 대조한다.
3. 커밋·push 후, 아침 테스트 가이드 요약(5줄 이내)을 마지막 메시지로 남긴다. 알림으로 나간다.

## Tifania가 아침에 하는 일 (5~10분)

1. Unity 프로젝트 열기 → `git pull` (예약 세션은 `claude/dev` 브랜치에 올리므로 `git checkout claude/dev` 후 pull).
2. 컴파일 에러가 있으면 콘솔 첫 에러 메시지를 복사해 그날 `docs/daily` 파일 맨 아래 "아침 피드백"에 붙인다.
3. 가이드의 메뉴(`GemRacer/...`)를 눌러 씬을 만들고 Play. 확인 포인트를 보고 한 줄씩 O/X를 적는다.
4. 커밋·push. 다음 야간 세션이 이 피드백부터 읽는다.
