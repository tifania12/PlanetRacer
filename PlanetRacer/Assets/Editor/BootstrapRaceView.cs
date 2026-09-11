using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GemRacer.Planet;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// 레이스 화면 실험 씬. 원래는 2D 사이드뷰로 가야 하는지 판단하려고 만들었고,
    /// 지금은 아트 방향(행성 룩)을 확인하는 용도로도 쓴다.
    ///
    /// 2026-09-11 실험에서 정해진 값들과 그 근거:
    ///  - 표면 전체 균등 배치는 장식이 지평선에만 몰려 느려 보인다.
    ///  - 주행선 위까지 깔면 카메라가 파묻힌다. 가운데 8m는 비운다.
    ///  - 세로 화면은 가로가 좁아 옆 장식이 자주 화면 밖으로 나간다.
    ///    속도를 실어 나르는 건 지면 타일 텍스처(약 9m)이고 장식은 보조다.
    ///  - 카메라는 높이 2.2m, 뒤 4.5m, 8m 앞. 시야각은 속도 따라 62~88도.
    ///
    /// 주의: URP에서 RenderSettings.fog(Linear)를 켜면 화면 전체가 안개색으로 덮인다.
    /// 원경 깊이감이 필요하면 URP 방식으로 따로 넣어야 한다. 지금은 꺼 둔다. (백로그 A-06)
    ///
    /// GemRacer/3. 레이스 카메라 실험
    /// </summary>
    public static class BootstrapRaceView
    {
        const string ScenePath = "Assets/Scenes/RaceCameraSpike.unity";
        const string MaterialFolder = "Assets/Art/Materials";

        const float PlanetRadius = 60f;   // 채굴 행성(20m)보다 크게. 레이스 속도로 20m는 한 바퀴가 너무 짧다
        const float RaceSpeed = 22f;      // m/s, 한 바퀴 약 17초
        const int TrackDecoCount = 700;
        const int FarDecoCount = 260;
        const float TrackClearWidth = 8f;
        const float TrackBandWidth = 9f;

        [MenuItem("GemRacer/3. 레이스 카메라 실험")]
        public static void CreateRaceSpikeScene() => Build(PlanetLook.Ruby);

        [MenuItem("GemRacer/4. 레이스 실험 (쿼츠 룩)")]
        public static void CreateRaceSpikeQuartz() => Build(PlanetLook.Quartz);

        static void Build(PlanetLook look)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var groundMat = GetOrCreateMaterial($"Ground_{look.Id}", Color.white);
            var rockMat = GetOrCreateMaterial($"Deco_rock_{look.Id}", look.RockColor);
            var crystalMat = GetOrCreateMaterial($"Deco_crystal_{look.Id}", look.CrystalColor);
            var carMat = GetOrCreateMaterial($"Car_{look.Id}", look.CarColor);
            if (crystalMat != null) crystalMat.SetFloat("_Smoothness", 0.75f);
            if (rockMat != null) rockMat.SetFloat("_Smoothness", 0.12f);

            // 지면 텍스처. 타일 하나가 약 GroundTileMeters가 되도록 반복 횟수를 계산한다.
            if (!string.IsNullOrEmpty(look.GroundTexturePath) && groundMat != null)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(look.GroundTexturePath);
                if (tex != null)
                {
                    groundMat.SetTexture("_BaseMap", tex);
                    float repeats = (2f * Mathf.PI * PlanetRadius) / Mathf.Max(0.5f, look.GroundTileMeters);
                    groundMat.SetTextureScale("_BaseMap", new Vector2(repeats, repeats * 0.5f));
                    groundMat.SetFloat("_Smoothness", 0.18f);
                }
                else
                {
                    Debug.LogWarning($"[GemRacer] 지면 텍스처를 못 찾았다: {look.GroundTexturePath}. 단색으로 간다.");
                    groundMat.SetColor("_BaseColor", look.AmbientColor);
                }
            }
            else if (groundMat != null)
            {
                groundMat.SetColor("_BaseColor", new Color(0.86f, 0.88f, 0.92f));
            }

            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = $"Planet_{look.NameKo}";
            planet.transform.localScale = Vector3.one * (PlanetRadius * 2f);
            SetMaterial(planet, groundMat);

            var car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "RacingCar";
            car.transform.localScale = new Vector3(1.1f, 0.6f, 2.4f);
            car.transform.position = new Vector3(0f, 0f, PlanetRadius);
            var carCol = car.GetComponent<Collider>();
            if (carCol != null) Object.DestroyImmediate(carCol);
            SetMaterial(car, carMat);

            var orbitAxis = new Vector3(0.15f, 1f, 0f);
            var mover = car.AddComponent<SurfaceMover>();
            mover.planetCenter = planet.transform;
            mover.radius = PlanetRadius;
            mover.speed = RaceSpeed;
            mover.orbitAxis = orbitAxis;

            var trackRoot = new GameObject("TrackDecor");
            SurfaceScatter.ScatterAlongTrack(trackRoot.transform, PlanetRadius, TrackDecoCount * 65 / 100,
                orbitAxis, TrackClearWidth, TrackBandWidth,
                new SurfaceScatter.DecoSettings { Shape = PrimitiveType.Cube, Material = rockMat, MinScale = 0.6f, MaxScale = 2.6f }, 777);
            SurfaceScatter.ScatterAlongTrack(trackRoot.transform, PlanetRadius, TrackDecoCount * 35 / 100,
                orbitAxis, TrackClearWidth, TrackBandWidth,
                new SurfaceScatter.DecoSettings { Shape = PrimitiveType.Capsule, Material = crystalMat, MinScale = 0.5f, MaxScale = 1.6f }, 4242);

            var farRoot = new GameObject("FarDecor");
            SurfaceScatter.ScatterEven(farRoot.transform, PlanetRadius, FarDecoCount,
                new SurfaceScatter.DecoSettings { Shape = PrimitiveType.Cube, Material = rockMat, MinScale = 1.5f, MaxScale = 4.5f }, 31337);

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
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = look.SkyColor;

            var raceCam = mainCamera.GetComponent<RaceCamera>();
            if (raceCam == null) raceCam = mainCamera.gameObject.AddComponent<RaceCamera>();
            raceCam.target = car.transform;
            raceCam.planetCenter = planet.transform;
            raceCam.height = 2.2f;
            raceCam.distance = 4.5f;
            raceCam.lookAhead = 8f;

            var compareGo = new GameObject("CompareCamera (MiningStyle, disabled)");
            var compareCam = compareGo.AddComponent<Camera>();
            compareCam.nearClipPlane = 0.1f;
            compareCam.farClipPlane = 600f;
            compareCam.fieldOfView = 60f;
            compareCam.clearFlags = CameraClearFlags.SolidColor;
            compareCam.backgroundColor = look.SkyColor;
            var follow = compareGo.AddComponent<CameraFollow>();
            follow.target = car.transform;
            follow.distance = 12f;
            follow.height = 7f;
            compareGo.SetActive(false);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = look.AmbientColor;
            RenderSettings.fog = false;   // URP에서 켜면 화면 전체가 안개색이 된다

            var lightGo = GameObject.Find("Directional Light");
            if (lightGo != null)
            {
                lightGo.transform.rotation = Quaternion.Euler(18f, 140f, 0f);
                var lt = lightGo.GetComponent<Light>();
                if (lt != null) { lt.color = look.SunColor; lt.intensity = look.SunIntensity; lt.shadows = LightShadows.Soft; }
            }

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GemRacer] 레이스 실험 씬({look.NameKo}): {ScenePath}\n" +
                      $"반지름 {PlanetRadius}m, 속도 {RaceSpeed}m/s (한 바퀴 약 {2f * Mathf.PI * PlanetRadius / RaceSpeed:F0}초), " +
                      $"지면 타일 {look.GroundTileMeters}m, 주행로 옆 장식 {TrackDecoCount}개.\n" +
                      "Game 뷰를 세로(540x960)로 두고 Play하면 실제 화면 비율이다.");
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
