using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// WebGL 빌드 진입점. 매일 아침 휴대폰으로 확인하는 웹 빌드가 여기서 나온다.
    ///
    /// Brotli로 미리 압축하고 압축 해제 대체(decompression fallback)를 끄는 이유는
    /// 첫 로딩 시간 때문이다. 대체 방식은 브라우저가 자바스크립트로 풀기 때문에
    /// 모바일에서 눈에 띄게 느리다. 대신 서버가 Content-Encoding을 제대로 내려 줘야 하고,
    /// 그건 web/_headers가 맡는다. (Cloudflare Pages는 이 파일을 읽는다)
    ///
    /// GitHub Actions는 game-ci 기본 빌더를 쓰므로 이 메서드를 거치지 않는다.
    /// 그래서 플레이어 설정은 ProjectSettings에도 저장해 둔다. 여기서 한 번 더 잡는 것은
    /// PC에서 tools/deploy_web.ps1로 직접 빌드할 때를 위한 안전장치다.
    /// </summary>
    public static class WebGLBuild
    {
        [MenuItem("GemRacer/웹 빌드 (WebGL)")]
        public static void BuildFromMenu()
        {
            var path = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).Parent.FullName,
                "build", "WebGL", "PlanetRacer");
            Run(path);
        }

        /// <summary>배치 모드용. -customBuildPath 로 출력 경로를 받는다.</summary>
        public static void Build()
        {
            var args = Environment.GetCommandLineArgs();
            string path = null;
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-customBuildPath") path = args[i + 1];

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[GemRacer] -customBuildPath 가 없다.");
                EditorApplication.Exit(1);
                return;
            }
            EditorApplication.Exit(Run(path) ? 0 : 1);
        }

        static bool Run(string outputPath)
        {
            ApplySettings();

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[GemRacer] Build Settings에 켜진 씬이 없다. 씬을 하나 이상 추가할 것.");
                return false;
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            var s = report.summary;
            Debug.Log($"[GemRacer] WebGL 빌드 {s.result}. {s.totalSize / (1024 * 1024)}MB, {s.totalTime.TotalMinutes:F1}분, 씬 {scenes.Length}개.");
            return s.result == BuildResult.Succeeded;
        }

        /// <summary>웹 빌드에 필요한 플레이어 설정. 메뉴에서 따로 실행할 수도 있다.</summary>
        [MenuItem("GemRacer/웹 빌드 설정만 적용")]
        public static void ApplySettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = true;           // 두 번째 방문부터 즉시 뜬다
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;  // 용량과 속도
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetIl2CppCompilerConfiguration(
                NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Master);
            // 2026-09-14: true에서 false로. true일 때 웹 빌드에서 UI Toolkit이 통째로 사라지고
            // (UIDocument 여덟 개가 하나도 안 그려짐) 스카이박스도 죽어서 화면 전체가 어두워졌다.
            // 에디터 Play에서는 HUD·튜토리얼이 멀쩡히 뜨는데 빌드에서만 사라져서 오래 헤맸다.
            // 네이티브 엔진 모듈을 떼어내는 옵션이라 UIElements 모듈까지 같이 날아간 것으로 본다.
            // 용량이 늘지만 화면이 안 보이는 것보다 낫다. 용량은 backlog W-09에서 따로 잡는다.
            PlayerSettings.stripEngineCode = false;

            // 로딩 화면에서 유니티 큐브 로고를 없앤다.
            // 기본 템플릿(APPLICATION:Default)의 index.html에 로고가 박혀 있어서
            // Assets/WebGLTemplates/GemRacer 로 갈아끼운다. 라이선스와 무관하게 되는 부분.
            PlayerSettings.WebGL.template = "PROJECT:GemRacer";

            // 실행 직후 "Made with Unity" 스플래시. Personal 라이선스에서는
            // 에디터가 다시 켤 수 있어서 예외를 삼키고 결과만 남긴다.
            try
            {
                PlayerSettings.SplashScreen.show = false;
                PlayerSettings.SplashScreen.showUnityLogo = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[GemRacer] 스플래시를 끄지 못했다: " + e.Message);
            }
            Debug.Log($"[GemRacer] 스플래시 show={PlayerSettings.SplashScreen.show}, " +
                      $"유니티로고={PlayerSettings.SplashScreen.showUnityLogo}, " +
                      $"템플릿={PlayerSettings.WebGL.template}");

            AssetDatabase.SaveAssets();
            Debug.Log("[GemRacer] WebGL 플레이어 설정 적용: Brotli, 대체 해제 끔, 캐싱 켬.");
        }
    }
}
