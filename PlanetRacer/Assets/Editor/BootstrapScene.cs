using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GemRacer.Planet;
using GemRacer.Mining;
using GemRacer.Game;

namespace GemRacer.EditorTools
{
    /// <summary>씬을 코드로 만드는 부트스트랩. .unity 파일을 손으로 안 건드리고
    /// 이 메뉴를 눌러서 매번 같은 구성으로 다시 만든다. (CLAUDE.md 3번 규칙)
    ///
    /// GemRacer/1. 테스트 씬 만들기 — 구체 행성 + 표면을 도는 채굴차 큐브 + 카메라 팔로우.
    /// D01-N: 행성 표면 이동 테스트용 최소 씬. 실제 아트·행성 파라미터는 나중에.</summary>
    public static class BootstrapScene
    {
        const string ScenePath = "Assets/Scenes/TestPlanet.unity";
        const string MaterialFolder = "Assets/Art/Materials";
        const float PlanetRadius = 20f;

        [MenuItem("GemRacer/1. 테스트 씬 만들기")]
        public static void CreateTestScene()
        {
            EnsureFolder(MaterialFolder);

            // 6개 행성 보석 색 머티리얼을 전부 만들어 둔다. 지금 쓰는 건 쿼츠 하나뿐이지만
            // 나중에 행성이 늘어날 때 다시 안 만들어도 되게 여기서 한 번에 준비.
            Material quartzMaterial = null;
            foreach (var planetId in GemColors.PlanetIdsInOrder)
            {
                var mat = CreateOrUpdatePlanetMaterial(planetId);
                if (planetId == "quartz") quartzMaterial = mat;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "Planet_Quartz";
            planet.transform.position = Vector3.zero;
            // 기본 구체 프리미티브는 지름 1(반지름 0.5)이라, 반지름 20을 맞추려면 스케일 40.
            planet.transform.localScale = Vector3.one * (PlanetRadius * 2f);
            var planetRenderer = planet.GetComponent<Renderer>();
            if (planetRenderer != null && quartzMaterial != null) planetRenderer.sharedMaterial = quartzMaterial;

            var rig = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rig.name = "MiningRig";
            rig.transform.localScale = new Vector3(1.2f, 0.9f, 1.8f);
            // 적도 부근 한 점에서 시작(축의 극점과 거리를 둬서 SurfaceMover 주석의 가정을 지킨다).
            rig.transform.position = new Vector3(0f, 0f, PlanetRadius);

            var mover = rig.AddComponent<SurfaceMover>();
            mover.planetCenter = planet.transform;
            mover.radius = PlanetRadius;
            mover.speed = 3f;
            mover.orbitAxis = new Vector3(0.2f, 1f, 0f);

            // D04-N: 실시간 채굴 루프. 광맥 앞에 도착하면 mover를 멈추고 원석을 쌓는다(임시 OnGUI로 확인).
            var miningController = rig.AddComponent<MiningController>();
            miningController.planetId = "quartz";
            miningController.surfaceMover = mover;

            var flow = new GameObject("GameFlow").AddComponent<GameFlowController>();
            flow.miningController = miningController;

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

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GemRacer] 테스트 씬 생성 완료: {ScenePath}. Play를 누르면 채굴차가 행성 표면을 돌다가 " +
                "광맥 앞에서 멈춰 원석을 캔다(화면 좌상단 임시 표시로 확인).");
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
                Debug.LogWarning("[GemRacer] URP Lit 셰이더를 못 찾았다. URP 패키지가 프로젝트에 있는지 확인 — 아침 확인 필요.");
                return null;
            }

            var mat = new Material(shader) { name = $"Planet_{planetId}" };
            ApplyColor(mat, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void ApplyColor(Material mat, Color color)
        {
            // URP Lit은 _BaseColor를 쓴다. mat.color가 자동으로 매핑해 주긴 하지만,
            // 셰이더가 바뀌어도 안 깨지게 프로퍼티 존재 여부를 직접 확인한다.
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
