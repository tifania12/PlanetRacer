using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GemRacer.Planet;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// 레이스 화면 속도감 실험 씬. 2D 사이드뷰로 가야 하는지 판단하려고 만들었다.
    ///
    /// 물어본 것: 구체 행성 위에서 낮은 카메라 + 넓은 시야각 + 주행로 옆 장식만으로
    /// 세로 화면(9:16)에서 "빠르다"가 읽히는가?
    ///
    /// 2026-09-11 실험 결과와 아래 값의 근거:
    ///   - 표면 전체에 균등하게 뿌리면 장식이 전부 지평선에 몰려 화면에서 안 움직인다. 느려 보인다.
    ///   - 주행선 위에까지 깔면 카메라가 장식에 파묻혀 앞이 안 보인다.
    ///   - 주행로 8m를 비우고 그 바깥 9m 띠에 700개 정도가 적당했다.
    ///     앞쪽 장식이 화면을 가로질러 빠져나가는 그림이 나온다. 이게 속도감의 정체다.
    ///   - 카메라는 높이 2.2m, 뒤 4.5m, 8m 앞을 봄(아래로 약 15도). 지평선이 화면 위쪽에 걸린다.
    ///
    /// GemRacer/3. 레이스 카메라 실험
    /// </summary>
    public static class BootstrapRaceView
    {
        const string ScenePath = "Assets/Scenes/RaceCameraSpike.unity";
        const string MaterialFolder = "Assets/Art/Materials";

        // 채굴 행성(반지름 20)보다 크게 잡았다. 레이스 속도로 반지름 20을 돌면
        // 한 바퀴가 몇 초밖에 안 돼서 코스라는 느낌이 안 난다.
        const float PlanetRadius = 60f;
        const float RaceSpeed = 22f;      // m/s. 한 바퀴 약 17초
        const int TrackDecoCount = 700;   // 주행로 옆 띠
        const int FarDecoCount = 260;     // 원경(지평선 너머 실루엣용)
        const float TrackClearWidth = 8f; // 비워 둘 주행로 폭(m)
        const float TrackBandWidth = 9f;  // 그 바깥 장식 띠 폭(m)

        [MenuItem("GemRacer/3. 레이스 카메라 실험")]
        public static void CreateRaceSpikeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var quartzMat = GetOrCreateMaterial("Planet_quartz_spike", new Color(0.86f, 0.88f, 0.92f));
            var rockMat = GetOrCreateMaterial("Deco_rock_spike", new Color(0.52f, 0.55f, 0.62f));
            var crystalMat = GetOrCreateMaterial("Deco_crystal_spike", new Color(0.55f, 0.78f, 0.95f));

            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "Planet_Quartz";
            planet.transform.position = Vector3.zero;
            planet.transform.localScale = Vector3.one * (PlanetRadius * 2f);
            SetMaterial(planet, quartzMat);

            var car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "RacingCar";
            car.transform.localScale = new Vector3(1.1f, 0.6f, 2.4f);
            car.transform.position = new Vector3(0f, 0f, PlanetRadius);
            var carCol = car.GetComponent<Collider>();
            if (carCol != null) Object.DestroyImmediate(carCol);
            SetMaterial(car, GetOrCreateMaterial("Car_spike", new Color(0.90f, 0.29f, 0.24f)));

            var orbitAxis = new Vector3(0.15f, 1f, 0f);
            var mover = car.AddComponent<SurfaceMover>();
            mover.planetCenter = planet.transform;
            mover.radius = PlanetRadius;
            mover.speed = RaceSpeed;
            mover.orbitAxis = orbitAxis;

            // 주행로 옆 띠. 속도감의 핵심.
            var trackRoot = new GameObject("TrackDecor");
            SurfaceScatter.ScatterAlongTrack(trackRoot.transform, PlanetRadius, TrackDecoCount * 65 / 100,
                orbitAxis, TrackClearWidth, TrackBandWidth,
                new SurfaceScatter.DecoSettings { Shape = PrimitiveType.Cube, Material = rockMat, MinScale = 0.6f, MaxScale = 2.6f },
                seed: 777);
            SurfaceScatter.ScatterAlongTrack(trackRoot.transform, PlanetRadius, TrackDecoCount * 35 / 100,
                orbitAxis, TrackClearWidth, TrackBandWidth,
                new SurfaceScatter.DecoSettings { Shape = PrimitiveType.Capsule, Material = crystalMat, MinScale = 0.5f, MaxScale = 1.6f },
                seed: 4242);

            // 원경. 지평선 너머로 실루엣이 보여서 행성이 넓게 느껴진다.
            var farRoot = new GameObject("FarDecor");
            SurfaceScatter.ScatterEven(farRoot.transform, PlanetRadius, FarDecoCount,
                new SurfaceScatter.DecoSettings { Shape = PrimitiveType.Cube, Material = rockMat, MinScale = 1.5f, MaxScale = 4.5f },
                seed: 31337);

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var camGo = new GameObject("Main Camera");
                mainCamera = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            mainCamera.gameObject.name = "RaceCamera (Main)";
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 600f;
            var raceCam = mainCamera.GetComponent<RaceCamera>();
            if (raceCam == null) raceCam = mainCamera.gameObject.AddComponent<RaceCamera>();
            raceCam.target = car.transform;
            raceCam.planetCenter = planet.transform;
            raceCam.height = 2.2f;
            raceCam.distance = 4.5f;
            raceCam.lookAhead = 8f;

            // 비교용. 채굴 스타일(멀리서, 좁은 시야각). 켜고 끄면서 차이를 본다.
            var compareGo = new GameObject("CompareCamera (MiningStyle, disabled)");
            var compareCam = compareGo.AddComponent<Camera>();
            compareCam.nearClipPlane = 0.1f;
            compareCam.farClipPlane = 600f;
            compareCam.fieldOfView = 60f;
            var follow = compareGo.AddComponent<CameraFollow>();
            follow.target = car.transform;
            follow.distance = 12f;
            follow.height = 7f;
            compareGo.SetActive(false);

            // 해를 낮게 깔면 장식이 긴 그림자를 만든다. 그림자가 흘러가는 것도 속도 신호다.
            var lightGo = GameObject.Find("Directional Light");
            if (lightGo != null)
            {
                lightGo.transform.rotation = Quaternion.Euler(18f, 140f, 0f);
                var lt = lightGo.GetComponent<Light>();
                if (lt != null) { lt.intensity = 1.15f; lt.shadows = LightShadows.Soft; }
            }

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GemRacer] 레이스 카메라 실험 씬: {ScenePath}\n" +
                      $"반지름 {PlanetRadius}m, 속도 {RaceSpeed}m/s (한 바퀴 약 {2f * Mathf.PI * PlanetRadius / RaceSpeed:F0}초), " +
                      $"주행로 옆 장식 {TrackDecoCount}개 + 원경 {FarDecoCount}개.\n" +
                      "Play 후 Game 뷰를 세로(540x960)로 두고 보면 실제 화면 비율이다. " +
                      "비교하려면 CompareCamera를 켜고 RaceCamera (Main)을 끈다.");
        }

        static Material GetOrCreateMaterial(string name, Color color)
        {
            EnsureFolder(MaterialFolder);
            string path = $"{MaterialFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) { ApplyColor(existing, color); return existing; }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("[GemRacer] URP Lit 셰이더를 못 찾았다. URP 설정 확인 필요.");
                return null;
            }
            var mat = new Material(shader) { name = name };
            ApplyColor(mat, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void ApplyColor(Material mat, Color color)
        {
            if (mat == null) return;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
        }

        static void SetMaterial(GameObject go, Material mat)
        {
            if (go == null || mat == null) return;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
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
