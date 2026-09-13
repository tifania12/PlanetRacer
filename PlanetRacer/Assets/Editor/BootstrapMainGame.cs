using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using GemRacer.Audio;
using GemRacer.Planet;
using GemRacer.Mining;
using GemRacer.Game;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>W-04: 지금까지 따로 있던 세 씬 — 테스트 씬(GemRacer/1, 3D 채굴만·임시 OnGUI),
    /// 반응형 UI 테스트 씬(GemRacer/5, 자리 표시자뿐), 업그레이드 화면 테스트 씬(GemRacer/6, 지면
    /// 없이 UI만) — 을 실제로 웹에서 열어 볼 수 있는 하나의 게임 화면으로 합친다.
    ///
    /// 3D 채굴(행성+채굴차+카메라)은 GemRacer/1과 같은 구성이고, 그 위에 Root.uxml HUD와
    /// 업그레이드 패널을 얹는다. HUD의 "채굴" 버튼을 누르면 업그레이드 패널이 열리고 닫힌다
    /// (MainHud.cs). 이 씬을 Build Settings 맨 앞(0번)에 넣어서 — main/claude/dev에 푸시되면
    /// GitHub Actions가 만드는 웹 빌드가 이제 이 화면으로 시작한다. 지금까지는 RaceCameraSpike
    /// 실험 씬이 그 자리였다(레이스 카메라·아트 방향을 눈으로 보는 실험용이라 실제 게임이 아니었다).
    ///
    /// GemRacer/7. 메인 게임 씬 만들기
    /// </summary>
    public static class BootstrapMainGame
    {
        const string ScenePath = "Assets/Scenes/MainGame.unity";
        const string MaterialFolder = "Assets/Art/Materials";
        const string UIFolder = "Assets/UI";
        const string PanelSettingsPath = UIFolder + "/PanelSettings.asset";
        const string RootUxmlPath = UIFolder + "/Root.uxml";
        const string UpgradeUxmlPath = UIFolder + "/Upgrade.uxml";
        const string CraftingUxmlPath = UIFolder + "/Crafting.uxml";
        const string OfflineRewardUxmlPath = UIFolder + "/OfflineReward.uxml";
        const string RaceEntryUxmlPath = UIFolder + "/RaceEntry.uxml";
        const string LootBoxUxmlPath = UIFolder + "/LootBox.uxml";
        const string TutorialUxmlPath = UIFolder + "/Tutorial.uxml";
        const string SettingsUxmlPath = UIFolder + "/Settings.uxml";
        const float PlanetRadius = 20f;

        [MenuItem("GemRacer/7. 메인 게임 씬 만들기")]
        public static void CreateScene()
        {
            var rootUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootUxmlPath);
            var upgradeUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UpgradeUxmlPath);
            var craftingUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CraftingUxmlPath);
            var offlineRewardUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(OfflineRewardUxmlPath);
            var raceEntryUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RaceEntryUxmlPath);
            var lootBoxUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LootBoxUxmlPath);
            var tutorialUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TutorialUxmlPath);
            var settingsUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingsUxmlPath);
            if (rootUxml == null || upgradeUxml == null || craftingUxml == null || offlineRewardUxml == null || raceEntryUxml == null || lootBoxUxml == null || tutorialUxml == null || settingsUxml == null)
            {
                Debug.LogError($"[GemRacer] UXML을 못 찾았다. {RootUxmlPath}, {UpgradeUxmlPath}, {CraftingUxmlPath}, {OfflineRewardUxmlPath}, {RaceEntryUxmlPath}, {LootBoxUxmlPath}, {TutorialUxmlPath}, {SettingsUxmlPath}가 있는지 확인.");
                return;
            }

            var quartzMaterial = CreateOrUpdatePlanetMaterial("quartz");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "Planet_Quartz";
            planet.transform.position = Vector3.zero;
            planet.transform.localScale = Vector3.one * (PlanetRadius * 2f);
            var planetRenderer = planet.GetComponent<Renderer>();
            if (planetRenderer != null && quartzMaterial != null) planetRenderer.sharedMaterial = quartzMaterial;

            var rig = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rig.name = "MiningRig";
            rig.transform.localScale = new Vector3(1.2f, 0.9f, 1.8f);
            rig.transform.position = new Vector3(0f, 0f, PlanetRadius);

            var mover = rig.AddComponent<SurfaceMover>();
            mover.planetCenter = planet.transform;
            mover.radius = PlanetRadius;
            mover.speed = 3f; // MiningController가 매 프레임 코어 RigSpeed로 덮어쓴다. 첫 프레임 전 기본값일 뿐.
            mover.orbitAxis = new Vector3(0.2f, 1f, 0f);

            var miningController = rig.AddComponent<MiningController>();
            miningController.planetId = "quartz";
            miningController.surfaceMover = mover;
            miningController.showDebugGui = false; // 이제 실제 HUD가 있으니 임시 OnGUI는 끈다

            var flow = new GameObject("GameFlow").AddComponent<GameFlowController>();
            flow.miningController = miningController;

            // D14-N: 사운드 자리. 소스 네 개(엔진·채굴·UI 탭·상자)를 한 오브젝트에 묶어 둔다 —
            // 지금은 클립을 하나도 안 채워서(에셋 팩이 없다, W3 몫) 전부 무음 플레이스홀더다.
            // AudioHub.cs가 클립 없으면 조용히 아무 일도 안 하니, 나중에 인스펙터에서 클립만
            // 채워 넣으면 코드를 안 고쳐도 그대로 소리가 난다.
            var audioHubGo = new GameObject("AudioHub");
            var audioHub = audioHubGo.AddComponent<AudioHub>();
            audioHub.engineSource = audioHubGo.AddComponent<AudioSource>();
            audioHub.engineSource.playOnAwake = false;
            audioHub.engineSource.loop = true;
            audioHub.miningSource = audioHubGo.AddComponent<AudioSource>();
            audioHub.miningSource.playOnAwake = false;
            audioHub.miningSource.loop = true;
            audioHub.uiTapSource = audioHubGo.AddComponent<AudioSource>();
            audioHub.uiTapSource.playOnAwake = false;
            audioHub.uiTapSource.loop = false;
            audioHub.boxSource = audioHubGo.AddComponent<AudioSource>();
            audioHub.boxSource.playOnAwake = false;
            audioHub.boxSource.loop = false;
            miningController.audioHub = audioHub;

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var camGo = new GameObject("Main Camera");
                mainCamera = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            var follow = mainCamera.GetComponent<CameraFollow>();
            if (follow == null) follow = mainCamera.gameObject.AddComponent<CameraFollow>();
            follow.target = rig.transform;
            follow.distance = 8f;
            follow.height = 4f;

            var panelSettings = GetOrCreatePanelSettings();

            // HUD(Root.uxml). MainHud.cs가 viewport-area를 투명하게 만들어 뒤의 3D 카메라가 보이게 한다.
            var hudRoot = new GameObject("UI Root (HUD)");
            var hudDoc = hudRoot.AddComponent<UIDocument>();
            hudDoc.panelSettings = panelSettings;
            hudDoc.visualTreeAsset = rootUxml;
            hudRoot.AddComponent<ResponsiveLayout>();

            // 업그레이드 패널. 같은 PanelSettings를 쓰되 소트 오더를 HUD보다 높여서 위에 뜨게 한다.
            // 기본은 화면에 보이되, MainHud가 Play 시작 직후(첫 프레임 이후) 닫아 둔다.
            var upgradeRoot = new GameObject("UI Root (Upgrade Overlay)");
            var upgradeDoc = upgradeRoot.AddComponent<UIDocument>();
            upgradeDoc.panelSettings = panelSettings;
            upgradeDoc.visualTreeAsset = upgradeUxml;
            upgradeDoc.sortingOrder = 10;
            upgradeRoot.AddComponent<ResponsiveLayout>();
            var upgradePanel = upgradeRoot.AddComponent<UpgradePanel>();
            upgradePanel.target = miningController;

            // D08-N: 부품 제작 패널. 업그레이드 오버레이와 같은 구성 — 소트 오더만 그 위(11)로
            // 둬서 둘을 동시에 열어도(원래는 안 그러겠지만) 제작 패널이 위에 보이게 했다.
            var craftRoot = new GameObject("UI Root (Crafting Overlay)");
            var craftDoc = craftRoot.AddComponent<UIDocument>();
            craftDoc.panelSettings = panelSettings;
            craftDoc.visualTreeAsset = craftingUxml;
            craftDoc.sortingOrder = 11;
            craftRoot.AddComponent<ResponsiveLayout>();
            var craftingPanel = craftRoot.AddComponent<CraftingPanel>();
            craftingPanel.target = miningController;

            // D09-N: 레이스 출전 패널. 업그레이드·제작 오버레이와 같은 구성 — 소트 오더는 제작보다
            // 위(12)로 둬서 셋을 동시에 열어도(원래는 안 그러겠지만) 레이스 패널이 제일 위에 보이게 했다.
            var raceRoot = new GameObject("UI Root (Race Overlay)");
            var raceDoc = raceRoot.AddComponent<UIDocument>();
            raceDoc.panelSettings = panelSettings;
            raceDoc.visualTreeAsset = raceEntryUxml;
            raceDoc.sortingOrder = 12;
            raceRoot.AddComponent<ResponsiveLayout>();
            var raceEntryPanel = raceRoot.AddComponent<RaceEntryPanel>();
            raceEntryPanel.target = miningController;

            // D11-N 후속: 공구 상자 개봉 패널. 업그레이드·제작·레이스 오버레이와 같은 구성 —
            // 소트 오더는 레이스보다 위(13)로 둬서 레이스 우승 직후 상자를 확인하러 열어도 가장 위에 보이게 했다.
            var boxRoot = new GameObject("UI Root (Loot Box Overlay)");
            var boxDoc = boxRoot.AddComponent<UIDocument>();
            boxDoc.panelSettings = panelSettings;
            boxDoc.visualTreeAsset = lootBoxUxml;
            boxDoc.sortingOrder = 13;
            boxRoot.AddComponent<ResponsiveLayout>();
            var lootBoxPanel = boxRoot.AddComponent<LootBoxPanel>();
            lootBoxPanel.target = miningController;
            lootBoxPanel.audioHub = audioHub;

            // D14-N: 설정 패널. 업그레이드·제작·레이스·상자 오버레이와 같은 구성 — 소트 오더는
            // 상자보다 위(14)로 둬서 동시에 열려도 설정 패널이 제일 위에 보이게 했다.
            var settingsRoot = new GameObject("UI Root (Settings Overlay)");
            var settingsDoc = settingsRoot.AddComponent<UIDocument>();
            settingsDoc.panelSettings = panelSettings;
            settingsDoc.visualTreeAsset = settingsUxml;
            settingsDoc.sortingOrder = 14;
            settingsRoot.AddComponent<ResponsiveLayout>();
            var settingsPanel = settingsRoot.AddComponent<SettingsPanel>();
            settingsPanel.target = miningController;
            settingsPanel.audioHub = audioHub;

            var hud = hudRoot.AddComponent<MainHud>();
            hud.target = miningController;
            hud.upgradeDocument = upgradeDoc;
            hud.craftDocument = craftDoc;
            hud.raceDocument = raceDoc;
            hud.boxDocument = boxDoc;
            hud.settingsDocument = settingsDoc;
            hud.audioHub = audioHub;

            // D13-N: 튜토리얼 배너. 소트 오더는 HUD(기본 0)보다 위, 다른 모달 오버레이(10 이상)보다
            // 아래로 둬서 — 평소엔 HUD 위에 보이다가 업그레이드/제작/레이스/상자 화면을 열면 그
            // 화면 뒤로 자연스레 가려진다. HUD와 달리 화면을 막지 않아서(TutorialController.cs)
            // Ensure~HiddenOnce 같은 처리도 필요 없다 — 스스로 TutorialStep을 보고 숨는다.
            var tutorialRoot = new GameObject("UI Root (Tutorial Banner)");
            var tutorialDoc = tutorialRoot.AddComponent<UIDocument>();
            tutorialDoc.panelSettings = panelSettings;
            tutorialDoc.visualTreeAsset = tutorialUxml;
            tutorialDoc.sortingOrder = 5;
            tutorialRoot.AddComponent<ResponsiveLayout>();
            var tutorialController = tutorialRoot.AddComponent<TutorialController>();
            tutorialController.target = miningController;

            // 오프라인 보상 화면(D07-N). 업그레이드 오버레이보다 더 위에 뜨게 소트 오더를 더 높인다 —
            // 돌아왔을 때 제일 먼저 봐야 하는 화면이라서다. OfflineRewardPanel.cs가 스스로
            // MiningController.PendingOfflineReward 유무로 보이고 숨는 걸 판단하니, Upgrade
            // 오버레이처럼 여기서 강제로 숨겨 둘 필요는 없다.
            var offlineRewardRoot = new GameObject("UI Root (Offline Reward Overlay)");
            var offlineRewardDoc = offlineRewardRoot.AddComponent<UIDocument>();
            offlineRewardDoc.panelSettings = panelSettings;
            offlineRewardDoc.visualTreeAsset = offlineRewardUxml;
            offlineRewardDoc.sortingOrder = 20;
            offlineRewardRoot.AddComponent<ResponsiveLayout>();
            var offlineRewardPanel = offlineRewardRoot.AddComponent<OfflineRewardPanel>();
            offlineRewardPanel.target = miningController;

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterAsFirstBuildScene(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GemRacer] 메인 게임 씬 생성 완료: " + ScenePath +
                "\nBuild Settings 맨 앞에 등록했다 — 다음 웹 배포부터 이 화면이 시작 화면이 된다.\n" +
                "Play하면 채굴차가 행성을 돌며 원석을 캔다. 화면 아래 '업그레이드' 버튼을 누르면 " +
                "곡괭이/화물칸/엔진 패널이 열리고 닫힌다. 엔진을 올리면 채굴차가 실제로 더 빨리 도는지 " +
                "확인해 줄 것(이번 세션에서 SurfaceMover 속도를 코어 RigSpeed에 맞추는 버그를 고쳤다).\n" +
                "D07-N: 이번 세션부터 세이브를 실제로 읽고 쓴다(30초마다 자동 저장 + 일시정지/종료 시). " +
                "저장된 상태로 Play를 다시 시작하면 그사이 지난 시간만큼 오프라인 보상 화면이 뜨는지 " +
                "확인해 줄 것 — Stop 후 30초 넘게 기다렸다가 다시 Play하면 재현된다.\n" +
                "D08-N: '제작' 버튼을 누르면 부품 제작 패널이 열린다. 원석 15개를 모으면 부품 5개 중 " +
                "하나를 제작할 수 있고, 제작한 부품은 '장착' 버튼으로 끼우거나 '해제'로 뺄 수 있다 — " +
                "장착 상태는 세이브에 남아서 다시 Play해도 그대로여야 한다.\n" +
                "D09-N: '레이스' 버튼을 누르면 출전 화면이 열린다. 연료는 10분마다 1개씩 차서 최대 " +
                "10개(새 세이브는 꽉 찬 채로 시작) — 코스 하나를 골라 '출전'을 누르면 연료 1개를 쓰고 " +
                "바로 결과(순위·기록)가 뜬다. 1위면 그 코스의 채굴차 부품 보상(곡괭이날/화물칸/엔진 " +
                "부스터 중 하나, 레벨 +1)이 적용되는지 확인해 줄 것 — 부품을 하나도 안 갖춘 채로는 " +
                "AI가 살짝 더 세서(임시 밸런스) 지기 쉬우니, 제작 화면에서 부품을 갖춘 뒤 도전해 볼 것.\n" +
                "D11-N 후속: '상자' 버튼을 누르면 개봉 화면이 열린다. 로컬 레이스에서 우승하면 녹슨 상자가 " +
                "하나씩 쌓이는데(RaceEntryPanel의 우승 문구에도 표시), 여기서 '열기'를 누르면 등급을 뽑아 " +
                "채굴차 부품(곡괭이/화물칸/엔진/탐지기/제련기 중 하나) 보상으로 즉시 바뀌어 적용된다 — " +
                "결과 카드에 등급과 어느 슬롯이 얼마나 올랐는지 뜨는지 확인해 줄 것. 강철·티타늄 상자는 " +
                "아직 코스가 없어 실전에서는 못 얻지만(W2 몫), 버튼 자체는 보유 0개로 비활성 상태인 게 맞다.\n" +
                "D13-N: 화면 맨 위에 노란 테두리 말풍선(튜토리얼)이 뜬다. 첫 세이브(완전히 새로 시작)일 " +
                "때만 보이고, '다음'을 누르면 4개(환영 → 화물칸 → 제작 유도 → 레이스 유도)를 순서대로 " +
                "지나간 뒤 저절로 사라지고 다시 Play해도 안 뜬다 — 계속 뜨거나 순서를 건너뛰면 버그다. " +
                "3번째·4번째 말풍선이 떠 있는 동안 배너 밖(화면 아래 '제작'/'레이스' 버튼)을 눌러도 " +
                "실제로 그 버튼이 눌리는지 확인해 줄 것(배너가 클릭을 가로채면 안 된다).\n" +
                "D14-N: 화면 아래 다섯 번째 '설정' 버튼을 누르면 설정 패널이 열린다. '소리' 줄의 " +
                "버튼을 누르면 켜짐/꺼짐이 바뀌고(지금은 클립이 없어 어차피 무음이지만 값은 세이브에 " +
                "남아야 함), '프레임' 줄의 30/60을 누르면 선택된 쪽이 파랗게 표시되고 실제로 " +
                "Application.targetFrameRate가 바뀌는지 확인해 줄 것. action-row가 이제 버튼 다섯 개라 " +
                "세로 화면에서 넷+하나(둘째 줄)로 자연스럽게 줄바꿈되는지도 봐 줄 것 — 어색하면 " +
                "Root.uss의 .action-button flex-basis를 20%로 낮추는 것도 방법.");
        }

        static Material CreateOrUpdatePlanetMaterial(string planetId)
        {
            string path = $"{MaterialFolder}/Planet_{planetId}.mat";
            Color color = GemColors.For(planetId);

            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                ApplyColor(existing, color);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("[GemRacer] URP Lit 셰이더를 못 찾았다 — 아침 확인 필요.");
                return null;
            }

            EnsureFolder(MaterialFolder);
            var mat = new Material(shader) { name = $"Planet_{planetId}" };
            ApplyColor(mat, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void ApplyColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
        }

        static PanelSettings GetOrCreatePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null) return existing;

            EnsureFolder(UIFolder);
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(540, 960);
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
                Debug.LogWarning("[GemRacer] 프로젝트에서 ThemeStyleSheet를 못 찾았다 — 버튼이 스타일 없이 나올 수 있다.");
            }

            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            return settings;
        }

        /// <summary>이 씬을 Build Settings 0번으로 넣는다 — WebGL 빌드가 시작할 때 여는 씬이 이걸로
        /// 바뀐다는 뜻. 이미 목록에 있으면 맨 앞으로 옮기기만 하고, 나머지 씬은 순서만 밀린다
        /// (지우지 않는다 — 테스트 씬들은 그대로 확인용으로 남는다).</summary>
        static void RegisterAsFirstBuildScene(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != scenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
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
