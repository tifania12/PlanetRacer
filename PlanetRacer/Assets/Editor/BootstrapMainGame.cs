using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GemRacer.Audio;
using GemRacer.Diagnostics;
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
        const float PlanetRadius = 20f;

        [MenuItem("GemRacer/7. 메인 게임 씬 만들기")]
        public static void CreateScene()
        {
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

            // D06-N: 광맥. 개수는 행성(코어 VeinCount)이 정하므로 여기서 오브젝트를 만들지 않고,
            // 자리와 재질만 준비해 둔다 — 실제 배치는 MiningController.Awake가 VeinField.Build로 한다.
            // 채굴차가 도는 대원과 같은 축·같은 출발 방향을 줘야 광맥 위에 정확히 멈춘다.
            var veinFieldGo = new GameObject("VeinField");
            var veinField = veinFieldGo.AddComponent<VeinField>();
            veinField.planetCenter = planet.transform;
            veinField.radius = PlanetRadius;
            veinField.orbitAxis = mover.orbitAxis;
            veinField.startDirection = rig.transform.position.normalized;
            veinField.veinMaterial = CreateOrUpdateSimpleMaterial("Vein_quartz", new Color(0.62f, 0.82f, 0.88f));
            veinField.activeVeinMaterial = CreateOrUpdateSimpleMaterial("Vein_quartz_active", new Color(1f, 0.93f, 0.45f));
            veinField.dustMaterial = CreateOrUpdateParticleMaterial("MiningDust", new Color(0.95f, 0.9f, 0.72f, 0.85f));

            var miningController = rig.AddComponent<MiningController>();
            miningController.planetId = "quartz";
            miningController.surfaceMover = mover;
            miningController.veinField = veinField;
            miningController.showDebugGui = false; // 이제 실제 HUD가 있으니 임시 OnGUI는 끈다

            var flow = new GameObject("GameFlow").AddComponent<GameFlowController>();
            flow.miningController = miningController;

            // D17-N: 지인 테스트 준비 — 접속 시각·플레이 시간을 로컬 CSV에 남긴다. GameFlow와
            // 같은 독립 오브젝트에 붙여 둔다(특정 화면·채굴차에 종속되지 않는 전역 컴포넌트라서).
            flow.gameObject.AddComponent<SessionLogger>();

            // 2026-09-15: 테스트용 시간 배속(`?fast=10`). 지금까지 씬에만 붙어 있어서 이 메뉴를
            // 다시 누르면 조용히 사라졌다 — 부트스트랩에 넣어 다시 만들어도 남게 한다.
            // 출시 전에 빼는 것은 DebugTimeScale.cs 안의 #if로 한다(ugui-migration.md 맨 아래).
            flow.gameObject.AddComponent<DebugTimeScale>();

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

            // U-08(2026-09-24): 옛 UI Toolkit 루트 열 개를 여기서 만들던 자리였다. 화면이 전부
            // uGUI로 옮겨졌으므로 만들지도, 꺼 두지도 않는다 — 아래 세 줄이 전부다.
            BootstrapHudUgui.Build();
            BootstrapTutorialUgui.Build();
            BootstrapArtViewer.Build();   // `?art=1` 확인 화면도 같은 캔버스 아래라 같이 세운다

            var hudUgui = GameObject.Find("UI Canvas/HUD")?.GetComponent<MainHudUgui>();
            if (hudUgui != null)
            {
                hudUgui.target = miningController;
                hudUgui.audioHub = audioHub;
                // UiPanel 칸(업그레이드·제작·레이스·상자·설정)은 그 화면이 uGUI로 옮겨질 때
                // 여기에 한 줄씩 늘린다(U-02~U-07). 비어 있으면 그 버튼은 꺼진 채로 남는다.
            }
            else
            {
                Debug.LogWarning("[GemRacer] uGUI HUD를 못 찾았다. 'UI Canvas/HUD'가 안 세워졌는지 확인.");
            }

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
                "Root.uss의 .action-button flex-basis를 20%로 낮추는 것도 방법.\n" +
                "D17-N: 설정 화면 맨 아래 '피드백' 칸에 글을 적고 '저장'을 누르면 '저장됐어요' 문구가 " +
                "뜨는지, persistentDataPath/feedback.txt에 실제로 남는지 확인해 줄 것. 확인 필요 — " +
                "WebGL은 File IO가 IndexedDB 가상 파일시스템이라 페이지를 새로고침하면 이번 세션에 " +
                "쓴 내용이 안 남을 수 있다(동기화 시점 불확실, 에디터가 없어 확인 못 함) — 실제 지인 " +
                "테스트는 안드로이드 빌드로 하니 크게 문제는 안 되겠지만, 웹에서 먼저 눌러 볼 때는 " +
                "클립보드 복사(카카오톡 등에 바로 붙여넣기)가 되는지가 더 믿을 만한 확인 경로다. " +
                "같은 세션(GameFlow 오브젝트)에 SessionLogger도 붙어서 접속마다 session_log.csv에 " +
                "시작 시각·플레이 시간이 쌓이는데, 이것도 같은 이유로 웹에서는 새로고침 전까지만 확인 가능.\n" +
                "M-04: 화물칸이 상한에 처음 닿는 순간 '정제로 돌리시겠어요?' 화면이 뜨는지 확인해 줄 것 — " +
                "인스펙터에서 MiningRig의 CargoLevel을 낮추거나 시간을 빨리 감아서 재현. '레이스 나가기'를 " +
                "누르면 레이스 출전 패널이 열리면서 이 화면은 닫히고, '닫기'를 누르면 그냥 닫힌다. 화면을 " +
                "닫은 뒤 원석이 상한 아래로 내려갔다가(정제나 소비로) 다시 차면 또 떠야 한다(엣지 트리거).\n" +
                "M-07: 화면 아래 여섯 번째 '상점' 버튼(또는 화물칸 가득 참 화면의 '상점 보기')을 누르면 " +
                "상점 화면이 열린다. 아직 실제 결제가 없어 아홉 줄 전부 '구매' 버튼을 누르면 바로 " +
                "테스트 구매가 적용된다 — 화물칸 확장을 사면 HUD 게이지 상한이 그 자리에서 커지는지, " +
                "채굴 가속 패스를 사면 원석이 눈에 띄게 더 빨리 쌓이는지 확인해 줄 것. 구독·가속 패스 " +
                "줄에는 '활성 (N일 남음)'이 뜨는지도 봐 줄 것 — 세로 화면에서 여섯 개 버튼이 3+3으로 " +
                "고르게 줄바꿈되는지(action-button flex-basis를 25%에서 30%로 낮췄다)도 같이 확인.");
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

        /// <summary>D06-N: 광맥처럼 색만 다른 URP Lit 재질 하나. 이미 있으면 색만 맞춰 다시 쓴다(멱등).</summary>
        static Material CreateOrUpdateSimpleMaterial(string materialName, Color color)
        {
            string path = $"{MaterialFolder}/{materialName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                ApplyColor(existing, color);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning($"[GemRacer] URP Lit 셰이더를 못 찾았다 — {materialName} 재질을 못 만들었다.");
                return null;
            }

            EnsureFolder(MaterialFolder);
            var mat = new Material(shader) { name = materialName };
            ApplyColor(mat, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        /// <summary>D06-N: 채굴 먼지용 파티클 재질. 런타임 Shader.Find는 웹 빌드에서 셰이더가 빠져
        /// 분홍색이 될 수 있어서, 씬이 참조로 들고 있도록 에셋으로 만들어 둔다.</summary>
        static Material CreateOrUpdateParticleMaterial(string materialName, Color color)
        {
            string path = $"{MaterialFolder}/{materialName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                ApplyColor(existing, color);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogWarning($"[GemRacer] URP 파티클 셰이더를 못 찾았다 — {materialName} 재질 없이 간다(파티클이 기본 재질로 보인다).");
                return null;
            }

            EnsureFolder(MaterialFolder);
            var mat = new Material(shader) { name = materialName };
            ApplyColor(mat, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void ApplyColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
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
