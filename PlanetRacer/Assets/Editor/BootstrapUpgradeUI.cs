using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.UI;
using GemRacer.Mining;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// D05-N 업그레이드 화면을 눈으로 확인하는 씬. MiningController 하나만 두고(3D 지면 없이)
    /// 원석이 저절로 쌓이는 걸 보면서 버튼이 켜지는 순간·업그레이드 후 다음 효과 문구가
    /// 바뀌는지 확인한다. 실제 게임에서는 채굴 화면(TestPlanet.unity)의 MiningController를
    /// 그대로 쓰면 된다 — 이 씬은 UI 확인 전용.
    ///
    /// GemRacer/6. 업그레이드 화면 테스트 씬 만들기
    /// </summary>
    public static class BootstrapUpgradeUI
    {
        const string ScenePath = "Assets/Scenes/UpgradeTest.unity";
        const string UIFolder = "Assets/UI";
        const string PanelSettingsPath = UIFolder + "/PanelSettings.asset";
        const string UxmlPath = UIFolder + "/Upgrade.uxml";

        [MenuItem("GemRacer/6. 업그레이드 화면 테스트 씬 만들기")]
        public static void CreateScene()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (uxml == null)
            {
                Debug.LogError($"[GemRacer] {UxmlPath}를 못 찾았다. Upgrade.uxml이 있는지 확인.");
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

            // 지면·SurfaceMover 없이 MiningController만 둔다 — 이동 없이도 광맥 앞에 멈춰서
            // 캐는 것처럼 시간이 흐르는 대로 원석이 쌓인다(Advance는 SurfaceMover가 없어도 동작).
            var rigGo = new GameObject("MiningRig");
            var miningController = rigGo.AddComponent<MiningController>();
            miningController.planetId = "quartz";
            miningController.showDebugGui = false; // 업그레이드 패널이 있으니 임시 OnGUI는 끈다

            var uiRoot = new GameObject("UI Root");
            var doc = uiRoot.AddComponent<UIDocument>();
            doc.panelSettings = panelSettings;
            doc.visualTreeAsset = uxml;
            uiRoot.AddComponent<ResponsiveLayout>();
            var panel = uiRoot.AddComponent<UpgradePanel>();
            panel.target = miningController;

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GemRacer] 업그레이드 화면 테스트 씬 생성 완료: " + ScenePath +
                "\nPlay하면 원석이 자동으로 쌓인다(MiningController가 배경에서 계속 돈다). " +
                "비용을 낼 만큼 모이면 버튼이 켜지고, 누르면 레벨과 '다음 효과' 문구가 바뀌는지 확인.");
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

            var themeGuid = AssetDatabase.FindAssets("t:ThemeStyleSheet").FirstOrDefault();
            if (!string.IsNullOrEmpty(themeGuid))
            {
                var themePath = AssetDatabase.GUIDToAssetPath(themeGuid);
                settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(themePath);
            }
            else
            {
                Debug.LogWarning("[GemRacer] 프로젝트에서 ThemeStyleSheet를 못 찾았다. PanelSettings에 테마가 " +
                    "없으면 버튼이 스타일 없이 나올 수 있다 — 아침 확인 필요.");
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
