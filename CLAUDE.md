# 보석 행성 레이서 (Gem Planet Racer) — 작업 규칙

방치형 레이싱 게임. 보석 이름의 행성이 스테이지. 채굴차가 방치 채굴 → 광물로 레이싱카 부품 제작 → 행성 그랑프리 1등 → 다음 행성.
모바일(iOS/Android) + Steam 동시 출시. 목표 2027-02-16. 기획서 요약은 `docs/GDD.md`, 전체는 Claude 아티팩트 "보석 행성 레이서 기획서".

## 저장소 구조

```
Unity/                      Unity 6 프로젝트 (Tifania가 Unity Hub에서 생성, URP 2D/3D Core 템플릿)
  Packages/com.bax.gemracer.core/Runtime/   순수 C# 코어. UnityEngine 참조 금지 (asmdef noEngineReferences)
  Assets/Scripts/           MonoBehaviour, UI, 연출. 코어를 호출만 한다
  Assets/Editor/            에디터 부트스트랩 (씬·프리팹·머티리얼을 코드로 생성하는 MenuItem)
Core.Tests/                 dotnet 콘솔 테스트 러너. `cd Core.Tests && dotnet run`. NuGet 사용 금지(오프라인)
docs/GDD.md                 기획 요약
docs/backlog.md             일 단위 작업 목록. 야간·새벽 세션은 여기서 다음 항목을 집는다
docs/daily/YYYY-MM-DD.md    그날 작업 보고 + 아침 테스트 가이드
docs/decisions.md           결정 기록
```

## 반드시 지킬 것

1. 게임 규칙·수식은 전부 `Packages/com.bax.gemracer.core/Runtime`에 순수 C#으로 쓴다. 서버가 같은 코드로 대전 결과를 검증하므로 `UnityEngine`, `Random`, `DateTime.Now`를 코어에서 쓰지 않는다. 난수는 `DeterministicRandom`, 시간은 인자로 받는다.
2. 코어를 고치면 `Core.Tests/Program.cs`에 테스트를 추가하고 `dotnet run`이 `실패 0`으로 끝나는지 확인한다. 실패한 채로 커밋하지 않는다.
3. Unity 에디터는 이 환경에 없다. 씬·프리팹·머티리얼은 `.unity`/`.prefab` YAML을 손으로 쓰지 말고, `Assets/Editor/Bootstrap*.cs`에 `[MenuItem("GemRacer/...")]`로 생성 코드를 쓴다. Tifania가 아침에 메뉴를 눌러 만든다.
4. `.meta` 파일은 만들지 않는다. Unity가 열 때 생성한다. 이미 있는 `.meta`는 지우지 않는다.
5. Unity 스크립트는 컴파일을 확인할 수 없으니 API를 보수적으로 쓴다. Unity 6 LTS, URP, UI Toolkit, Input System. 확실치 않은 API는 쓰지 않고 `docs/daily`에 "아침 확인 필요"로 적는다.
6. 입력은 탭/클릭만. UI는 세로(9:16) 단일 레이아웃. PC는 세로 창.
7. 한국어로 쓴다. 코드 주석·문서·커밋 메시지 모두. 문장은 사람이 말하듯 자연스럽게.
8. 하나의 작업 = 하나의 커밋. 메시지 첫 줄은 `[D07-N] 채굴차 자동 주행 연출` 형식 (backlog 항목 ID).

## 야간(23:00 KST) 세션이 하는 일

1. `git pull` 후 `docs/backlog.md`에서 체크 안 된 첫 `-N` 항목을 집는다. 그 전날 `-M` 항목이 남아 있으면 그것부터.
2. 구현한다. 코어 변경이면 테스트까지. Unity 스크립트면 부트스트랩·테스트 절차까지.
3. `docs/daily/YYYY-MM-DD.md`를 만든다(날짜는 KST 기준 다음 날 아침). 항목: 한 일 / 아침에 Tifania가 할 것(단계별, 5분 안) / 확인 포인트 / 막힌 것·결정 필요.
4. backlog 항목을 `[x]`로, 커밋, push.

## 새벽(06:00 KST) 세션이 하는 일

1. `git pull`. 야간 세션이 남긴 것을 이어서 마무리하거나, 다음 `-M` 항목을 한다. 새 큰 기능을 시작하지 않는다.
2. `dotnet run` 통과 확인. `docs/daily` 파일의 아침 테스트 가이드를 다시 읽고 실제 파일 경로·메뉴 이름이 맞는지 대조한다.
3. 커밋·push 후, 아침 테스트 가이드 요약(5줄 이내)을 마지막 메시지로 남긴다. 알림으로 나간다.

## Tifania가 아침에 하는 일 (5~10분)

1. Unity 프로젝트 열기 → 자동 pull(또는 수동 `git pull`).
2. 컴파일 에러가 있으면 콘솔 첫 에러 메시지를 복사해 그날 `docs/daily` 파일 맨 아래 "아침 피드백"에 붙인다.
3. 가이드의 메뉴(`GemRacer/...`)를 눌러 씬을 만들고 Play. 확인 포인트를 보고 한 줄씩 O/X를 적는다.
4. 커밋·push. 다음 야간 세션이 이 피드백부터 읽는다.
