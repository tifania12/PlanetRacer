# 밤새 쌓인 claude/dev 결과를 배포 주소(main)로 올린다.
#
# 왜 이 파일이 있나: main으로 미는 것은 Claude 세션의 실행 환경 정책에 막혀 있다.
# "배포는 사람이 한 번 보고 나간다"가 그 제한의 이유라서 우회하지 않는다.
# 대신 Tifania가 누르는 한 번을 최대한 쉽게 만든 것이 이것이다.
#
# .bat 이 아니라 .ps1 인 이유: cmd.exe 는 .bat 을 시스템 코드페이지(949)로 읽는데
# git 출력은 UTF-8 이라 커밋 메시지 한글이 전부 깨진다. PowerShell 은 UTF-8 을 그대로 다룬다.
#
# 쓰는 법: 바탕화면 "게임 배포하기" 더블클릭.
#         먼저 https://dev.planetracer-daz.pages.dev 를 열어 밤새 결과를 만져 보고 나서.

[Console]::OutputEncoding = [Text.Encoding]::UTF8
$OutputEncoding = [Text.Encoding]::UTF8
$env:LC_ALL = "C.UTF-8"

# --- 2026-09-20: 창이 바로 꺼지던 이유 -------------------------------------
# 전에는 여기에 $ErrorActionPreference = "Stop" 이 있었다.
# PowerShell 5.1 은 `git ... 2>&1 | Out-Null` 처럼 네이티브 명령의 stderr 를
# 파이프로 넘기면 그 줄들을 오류 레코드로 바꾼다. Stop 이면 그게 곧바로
# 치명적 오류가 되어 스크립트가 그 자리에서 죽고, -File 로 띄운 창은
# 아무것도 못 보여 준 채 닫힌다.
# git fetch 는 받아올 게 있으면 진행 상황을 늘 stderr 에 쓴다. 그래서
# "올릴 게 있을 때만" 창이 꺼졌다. 받아올 게 없던 날은 멀쩡히 돌았다.
# 고치는 법은 Stop 을 쓰지 않는 것. 실패는 아래처럼 $LASTEXITCODE 로 본다.
$ErrorActionPreference = "Continue"

function Pause-Exit($code) {
    Write-Host ""
    Read-Host "엔터를 누르면 닫힙니다" | Out-Null
    exit $code
}

try {

Set-Location "E:\Unity\PlanetRacer"

Write-Host ""
Write-Host "=== 최신 상태를 받아온다 ===" -ForegroundColor Cyan
git fetch origin 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[실패] git fetch 가 안 된다. 인터넷 연결을 확인할 것." -ForegroundColor Red
    Pause-Exit 1
}

$main  = (git rev-parse --short origin/main).Trim()
$dev   = (git rev-parse --short origin/claude/dev).Trim()
$devFull = (git rev-parse origin/claude/dev).Trim()
$ahead = (git rev-list --count origin/main..origin/claude/dev).Trim()

Write-Host ""
Write-Host "  배포 주소(main) : $main"
Write-Host "  밤새 작업(dev)  : $dev"
Write-Host "  밀린 커밋       : ${ahead}개"
Write-Host ""

if ($main -eq $dev) {
    Write-Host "이미 같다. 올릴 것이 없다." -ForegroundColor Yellow
    Pause-Exit 0
}

# main 이 dev 의 조상일 때만 올린다. 아니면 누가 main 에 직접 올린 것이라 사람이 봐야 한다.
git merge-base --is-ancestor origin/main origin/claude/dev
if ($LASTEXITCODE -ne 0) {
    Write-Host "[중단] main 이 claude/dev 의 조상이 아니라 그냥 올릴 수 없다." -ForegroundColor Red
    Write-Host "       main 에 직접 올라간 커밋이 있는지 확인할 것."
    Pause-Exit 1
}

Write-Host "무엇이 올라가는지:" -ForegroundColor Cyan
git log --oneline --no-decorate origin/main..origin/claude/dev | ForEach-Object { Write-Host "  $_" }
Write-Host ""
Write-Host "먼저 https://dev.planetracer-daz.pages.dev 에서 확인하셨나요?" -ForegroundColor DarkGray
$ok = Read-Host "이대로 배포 주소에 올릴까요? (y 를 입력하고 엔터)"

if ($ok -ne "y" -and $ok -ne "Y") {
    Write-Host "취소했다. 아무것도 안 올라갔다." -ForegroundColor Yellow
    Pause-Exit 0
}

Write-Host ""
git push origin "${devFull}:main"
if ($LASTEXITCODE -ne 0) {
    Write-Host "[실패] push 가 거절됐다." -ForegroundColor Red
    Pause-Exit 1
}

Write-Host ""
Write-Host "올렸다. 빌드와 배포가 10~16분 걸린다." -ForegroundColor Green
Write-Host "끝나면 https://planetracer-daz.pages.dev 에서 보인다."
Pause-Exit 0

}
catch {
    # 여기까지 오면 예상 못 한 오류다. 창이 그냥 닫히지 않도록 보여 주고 멈춘다.
    Write-Host ""
    Write-Host "[예상 못 한 오류] $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  위치: $($_.InvocationInfo.ScriptLineNumber)번째 줄" -ForegroundColor DarkGray
    Pause-Exit 1
}
