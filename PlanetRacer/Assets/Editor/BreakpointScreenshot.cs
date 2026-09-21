using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// 2026-09-22 (W-05): 반응형 세 기준점(세로 540x960 / 가로 960x540 / 태블릿 1280x800)을
    /// Play 모드에서 자동으로 돌며 스크린샷을 찍고 오늘 daily 파일에 경로를 남긴다.
    /// 그동안 새 uGUI 화면을 배선할 때마다 이 세 크기를 손으로 하나씩 바꿔 가며 확인해 온 것을
    /// 메뉴 한 번으로 줄인다.
    ///
    /// 2026-09-13 세션이 막혔던 지점(비공개 GameViewSizes API)은 2026-09-16 03:20 Unity 세션이
    /// 실제로 풀었다 — `PlayModeWindow.SetCustomRenderingResolution`은 공개 API고 Play 중에도
    /// 먹는다. 그때 확인된 두 가지 대기 시간을 그대로 썼다:
    /// 크기를 바꾼 직후 `Screen.width`가 곧바로 갱신되지 않아 재는 것보다 먼저 몇 초 기다려야
    /// 하고, `ScreenCapture.CaptureScreenshot`은 프레임 끝에 파일을 비동기로 쓰므로 그 뒤에도
    /// 몇 초 더 기다려야 한다. 촬영은 반드시 `ScreenCapture.CaptureScreenshot`을 쓴다 — Unity MCP의
    /// `manage_camera(screenshot)`는 Main Camera 경로만 찍어서 Screen Space - Overlay UI(=HUD 전부)가
    /// 빠진다.
    ///
    /// Edit 모드에서는 UI가 실제로 그려지지 않아 의미가 없으므로 Play 모드에서만 실행된다.
    /// </summary>
    public static class BreakpointScreenshot
    {
        static readonly (uint w, uint h, string name)[] Breakpoints =
        {
            (540, 960, "portrait"),
            (960, 540, "landscape"),
            (1280, 800, "tablet"),
        };

        const float MeasureDelaySeconds = 5f; // Screen.width가 곧바로 갱신되지 않는다
        const float WriteDelaySeconds = 4f;   // CaptureScreenshot은 비동기로 파일을 쓴다

        static bool _running;
        static int _step;
        static bool _waitingForWrite;
        static double _stepStartedAt;
        static string[] _paths;

        [MenuItem("GemRacer/기준점 스크린샷 찍기 (Play 중)")]
        public static void Run()
        {
            if (_running) { Debug.LogWarning("[GemRacer] 이미 도는 중이다."); return; }
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[GemRacer] Play 모드에서 실행할 것 — 지금은 Edit 모드다.");
                return;
            }

            Directory.CreateDirectory("Assets/Screenshots");
            _paths = new string[Breakpoints.Length];
            _step = 0;
            _waitingForWrite = false;
            _running = true;
            EditorApplication.update += Tick;
            BeginStep();
        }

        [MenuItem("GemRacer/기준점 스크린샷 찍기 (Play 중)", true)]
        public static bool RunValidate() => EditorApplication.isPlaying && !_running;

        static void BeginStep()
        {
            var bp = Breakpoints[_step];
            PlayModeWindow.SetCustomRenderingResolution(bp.w, bp.h, bp.name);
            _stepStartedAt = EditorApplication.timeSinceStartup;
            _waitingForWrite = false;
            Debug.Log($"[GemRacer] {_step + 1}/{Breakpoints.Length} — {bp.name} {bp.w}x{bp.h} 대기 중...");
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("[GemRacer] Play 모드가 도중에 꺼져서 중단한다.");
                Finish();
                return;
            }

            var elapsed = EditorApplication.timeSinceStartup - _stepStartedAt;

            if (!_waitingForWrite)
            {
                if (elapsed < MeasureDelaySeconds) return;
                var bp = Breakpoints[_step];
                var stamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
                var path = $"Assets/Screenshots/breakpoint-{bp.name}-{stamp}.png";
                _paths[_step] = path;
                ScreenCapture.CaptureScreenshot(path);
                _stepStartedAt = EditorApplication.timeSinceStartup;
                _waitingForWrite = true;
                return;
            }

            if (elapsed < WriteDelaySeconds) return;

            _step++;
            if (_step < Breakpoints.Length) { BeginStep(); return; }

            // 세 기준점 다 끝났다 — 기본값(세로)으로 되돌리고 daily에 기록한 뒤 멈춘다.
            PlayModeWindow.SetCustomRenderingResolution(540, 960, "portrait");
            AppendToDaily(_paths);
            Debug.Log("[GemRacer] 세 기준점 스크린샷 완료:\n" + string.Join("\n", _paths));
            Finish();
        }

        static void Finish()
        {
            EditorApplication.update -= Tick;
            _running = false;
        }

        static void AppendToDaily(string[] paths)
        {
            var now = DateTime.Now;
            // 에디터의 작업 디렉터리는 저장소 루트가 아니라 **Unity 프로젝트 루트**
            // (E:\Unity\PlanetRacer\PlanetRacer)다. 그래서 "docs/daily/..."로 적으면
            // 저장소 밖의 없는 폴더를 가리켜 조용히 실패한다(2026-09-22 배선 세션에서 실제로 겪었다).
            // Application.dataPath(=<프로젝트>/Assets)에서 두 칸 올라가야 저장소 루트다.
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var dailyPath = Path.Combine(repoRoot, "docs", "daily", $"{now:yyyy-MM-dd}.md");
            var entry =
                $"\n### {now:HH:mm} 스크린샷 자동 촬영 (`GemRacer/기준점 스크린샷 찍기`)\n" +
                $"- 세로 540x960 / 가로 960x540 / 태블릿 1280x800 — 경로: " +
                $"{paths[0]} / {paths[1]} / {paths[2]}\n" +
                $"- Screenshots는 .gitignore라 커밋 안 됨. 눈으로 세 장 다 확인하고 이 줄은 지워도 된다.\n";
            try
            {
                if (!File.Exists(dailyPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dailyPath));
                    Debug.LogWarning($"[GemRacer] {dailyPath}가 없어 새로 만든다.");
                    File.WriteAllText(dailyPath, $"# {now:yyyy-MM-dd}\n");
                }
                File.AppendAllText(dailyPath, entry);
                Debug.Log($"[GemRacer] {dailyPath}에 한 줄 남김.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GemRacer] daily 파일에 못 남겼다({dailyPath}): {e.Message} " +
                                  "— 스크린샷 경로를 손으로 옮겨 적을 것.");
            }
        }
    }
}
