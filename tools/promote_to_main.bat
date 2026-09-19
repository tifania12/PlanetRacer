@echo off
chcp 65001 >nul
REM 밤새 쌓인 claude/dev 결과를 배포 주소(main)로 올린다.
REM
REM 왜 이 파일이 있나: main으로 미는 것은 Claude 세션의 실행 환경 정책에 막혀 있다.
REM "배포는 사람이 한 번 보고 나간다"가 그 제한의 이유라서 우회하지 않는다.
REM 대신 Tifania가 누르는 한 번을 최대한 쉽게 만든 것이 이 파일이다.
REM
REM 쓰는 법: 바탕화면 바로가기를 만들어 두고 아침에 더블클릭.
REM         먼저 https://dev.planetracer-daz.pages.dev 를 열어 밤새 결과를 만져 보고 나서.

cd /d E:\Unity\PlanetRacer
if errorlevel 1 (
  echo [실패] E:\Unity\PlanetRacer 로 이동하지 못했다.
  pause & exit /b 1
)

echo.
echo === 최신 상태를 받아온다 ===
git fetch origin
if errorlevel 1 ( echo [실패] fetch 실패. & pause & exit /b 1 )

for /f %%i in ('git rev-parse --short origin/main') do set MAIN=%%i
for /f %%i in ('git rev-parse --short origin/claude/dev') do set DEV=%%i
for /f %%i in ('git rev-list --count origin/main..origin/claude/dev') do set AHEAD=%%i

echo.
echo   배포 주소(main)  : %MAIN%
echo   밤새 작업(dev)   : %DEV%
echo   밀린 커밋        : %AHEAD%개
echo.

if "%MAIN%"=="%DEV%" (
  echo 이미 같다. 올릴 것이 없다.
  pause & exit /b 0
)

REM main이 dev의 조상일 때만 올린다. 아니면 누가 main에 직접 올린 것이라 사람이 봐야 한다.
git merge-base --is-ancestor origin/main origin/claude/dev
if errorlevel 1 (
  echo [중단] main이 claude/dev의 조상이 아니라 그냥 올릴 수 없다.
  echo        main에 직접 올라간 커밋이 있는지 확인할 것.
  pause & exit /b 1
)

echo 무엇이 올라가는지:
git log --oneline origin/main..origin/claude/dev
echo.
set /p OK="이대로 배포 주소에 올릴까요? (y 를 누르고 엔터): "
if /i not "%OK%"=="y" (
  echo 취소했다. 아무것도 안 올라갔다.
  pause & exit /b 0
)

echo.
git push origin %DEV%:main
if errorlevel 1 (
  echo [실패] push 실패.
  pause & exit /b 1
)

echo.
echo 올렸다. 빌드와 배포가 10~16분 걸린다.
echo 끝나면 https://planetracer-daz.pages.dev 에서 보인다.
echo.
pause
