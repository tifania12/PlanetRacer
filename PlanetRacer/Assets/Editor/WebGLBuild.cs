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
            PlayerSettings.stripEngineCode = true;

            AssetDatabase.SaveAssets();
            Debug.Log("[GemRacer] WebGL 플레이어 설정 적용: Brotli, 대체 해제 끔, 캐싱 켬.");
        }
    }
}
