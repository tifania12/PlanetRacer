using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// 반응형 UI Toolkit 골격을 눈으로 확인하는 씬(W-03/W-04). 실제 게임 화면이 아니라
    /// 3D 뷰 자리와 HUD 자리가 세로/가로에서 어떻게 재배치되는지 보는 자리 표시자다.
    /// 실제 게임 카메라를 이 안에 넣는 작업은 진짜 게임 화면(D04 이후)이 생겼을 때 한다 —
    /// 지금 넣으면 이 씬만을 위한 가짜 카메라 연출이 되어 버린다.
    ///
    /// GemRacer/5. 반응형 UI 테스트 씬 만들기
    /// </summary>
    public static class BootstrapResponsiveUI
    {
        const string ScenePath = "Assets/Scenes/ResponsiveUITest.unity";
        const string UIFolder = "Assets/UI";
        const string PanelSettingsPath = UIFolder + "/PanelSettings.asset";
        const string UxmlPath = UIFolder + "/Root.uxml";

        [MenuItem("GemRacer/5. 반응형 UI 테스트 씬 만들기")]
        public static void CreateScene()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (uxml == null)
            {
                Debug.LogError($"[GemRacer] {UxmlPath}를 못 찾았다. Root.uxml이 있는지 확인.");
                return;
            }

            var panelSettings = GetOrCreatePanelSettings();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var camGo = new GameObject("Main Camera");
                mainCamera = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.07f, 0.07f, 0.09f);

            var uiRoot = new GameObject("UI Root");
            var doc = uiRoot.AddComponent<UIDocument>();
            doc.panelSettings = panelSettings;
            doc.visualTreeAsset = uxml;
            uiRoot.AddComponent<ResponsiveLayout>();

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GemRacer] 반응형 UI 테스트 씬 생성 완료: " + ScenePath +
                "\nGame 뷰 크기를 세로 540x960 → 가로 960x540 → 태블릿 1280x800 순으로 바꿔 가며 " +
                "Play — 3D 뷰 자리(어두운 큰 칸)와 HUD 자리(버튼 3개)가 위/아래에서 왼쪽/오른쪽으로 " +
                "바뀌는지, 버튼이 셋 다 안 잘리고 보이는지 확인.");
        }

        static PanelSettings GetOrCreatePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null) return existing;

            EnsureFolder(UIFolder);
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(540, 960); // CLAUDE.md 6번: 세로가 기준
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;

            // 런타임 테마가 없으면 버튼 등이 스타일 없이 나온다. 프로젝트에 있는 걸 하나 찾아 붙인다.
            var themeGuid = AssetDatabase.FindAssets("t:ThemeStyleSheet").FirstOrDefault();
            if (!string.IsNullOrEmpty(themeGuid))
            {
                var themePath = AssetDatabase.GUIDToAssetPath(themeGuid);
                settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(themePath);
            }
            else
            {
                Debug.LogWarning("[GemRacer] 프로젝트에서 ThemeStyleSheet를 못 찾았다. PanelSettings에 테마가 " +
                    "없으면 버튼이 스타일 없이 나올 수 있다 — 아침 확인 필요. Project 창에서 기본 런타임 " +
                    "테마를 만들어(우클릭 > Create > UI Toolkit > Theme Style Sheet) PanelSettings.asset에 붙여줄 것.");
            }

            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            return settings;
        }

        static void RegisterInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Any(s => s.path == scenePath)) return;

            var list = scenes.ToList();
            list.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
