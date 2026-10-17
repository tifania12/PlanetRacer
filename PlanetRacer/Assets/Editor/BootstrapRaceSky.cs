using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GemRacer.Planet;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-27 레이스 하늘 판 배선. 지금 열려 있는 씬의 <see cref="RaceCamera"/> 자식으로
    /// 평면 한 장(RaceSkyPlane)을 붙이고 <see cref="GemRacer.Planet.RaceSkyPlane"/>을 단다.
    ///
    /// 그림은 행성별로 여섯 장(<c>Resources/Art/Race/race-sky-*</c>)이고, 실행 중에는 컴포넌트가
    /// 행성 아이디를 보고 물린다. 에디터에서도 눈으로 보이게 머티리얼에는 지금 씬의 행성 그림을
    /// 미리 물려 둔다 — 씬에 <c>Planet_{이름}</c> 오브젝트가 있으면 그 이름으로 행성을 알아낸다.
    ///
    /// 머티리얼은 URP/Unlit이다. 하늘은 빛을 받으면 안 된다 — 행성마다 태양 색·세기가 달라서
    /// Lit으로 두면 같은 그림이 행성에 따라 다른 색으로 보인다. 그림자도 주고받지 않는다.
    ///
    /// 멱등하다 — 다시 눌러도 판이 하나만 남고 값은 같은 자리로 돌아온다.
    ///
    /// GemRacer/47. 레이스 하늘 판 (A-27)
    /// </summary>
    static class BootstrapRaceSky
    {
        const string PlaneName = "RaceSkyPlane";
        const string MaterialFolder = "Assets/Art/Materials";
        const string MaterialPath = MaterialFolder + "/RaceSky.mat";
        const string SkyResourcePrefix = "Assets/Resources/Art/Race/race-sky-";
        const float PlaneDistance = 300f;   // 행성 반지름 60m보다 멀고 farClipPlane 600m보다 가깝다

        [MenuItem("GemRacer/47. 레이스 하늘 판 (A-27)")]
        public static void Run()
        {
            var raceCam = Object.FindFirstObjectByType<RaceCamera>();
            if (raceCam == null)
            {
                Debug.LogWarning("[A-27] 이 씬에는 RaceCamera가 없다. 레이스 화면이 있는 씬" +
                                 "(Assets/Scenes/RaceCameraSpike.unity)을 열고 다시 누른다.");
                return;
            }

            var cam = raceCam.GetComponent<Camera>();
            if (cam != null && cam.farClipPlane <= PlaneDistance)
            {
                Debug.LogWarning($"[A-27] 카메라 farClipPlane({cam.farClipPlane}m)이 판 거리({PlaneDistance}m)보다 " +
                                 "가깝다. 판이 잘려 안 보인다 — farClipPlane을 늘리거나 거리를 줄여야 한다.");
            }

            string planetId = GuessPlanetId();
            var material = GetOrCreateMaterial(planetId);

            var plane = FindOrCreatePlane(raceCam.transform);
            var renderer = plane.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }

            var sky = plane.GetComponent<RaceSkyPlane>();
            if (sky == null) sky = plane.AddComponent<RaceSkyPlane>();
            sky.planetId = planetId;
            sky.distance = PlaneDistance;

            // 처음 한 프레임도 배경색이 비치지 않게 에디터에서도 자리를 잡아 둔다.
            plane.transform.localPosition = new Vector3(0f, 0f, PlaneDistance);
            plane.transform.localRotation = Quaternion.identity;

            EditorUtility.SetDirty(plane);
            if (renderer != null) EditorUtility.SetDirty(renderer);
            EditorSceneManager.MarkSceneDirty(raceCam.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[A-27] 하늘 판을 붙였다: {raceCam.name}/{PlaneName}, 행성 {planetId}, 거리 {PlaneDistance}m.\n" +
                      "Game 뷰를 세로(540x960)로 두고 Play하면 실제 비율이다. 시야각이 넓어져도 " +
                      "판이 같이 커지므로 테두리에 배경색이 비치지 않는다.");
        }

        /// <summary>씬의 <c>Planet_{한글이름}</c> 오브젝트로 행성을 알아낸다. 못 찾으면 루비.</summary>
        static string GuessPlanetId()
        {
            foreach (var look in PlanetLook.All)
            {
                if (GameObject.Find($"Planet_{look.NameKo}") != null) return look.Id;
            }
            return PlanetLook.Ruby.Id;
        }

        static GameObject FindOrCreatePlane(Transform parent)
        {
            var existing = parent.Find(PlaneName);
            if (existing != null) return existing.gameObject;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = PlaneName;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);   // 하늘을 클릭하면 안 된다
            go.transform.SetParent(parent, false);
            return go;
        }

        static Material GetOrCreateMaterial(string planetId)
        {
            EnsureFolder(MaterialFolder);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SkyResourcePrefix + planetId + ".png");
            if (texture == null)
            {
                Debug.LogWarning($"[A-27] 하늘 그림을 못 찾았다: {SkyResourcePrefix}{planetId}.png. " +
                                 "머티리얼만 만들어 둔다 — 실행 중에 Resources에서 다시 찾는다.");
            }

            var existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (existing != null)
            {
                if (texture != null) existing.mainTexture = texture;
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogWarning("[A-27] URP Unlit 셰이더를 못 찾았다. URP 설정 확인 필요.");
                return null;
            }
            var mat = new Material(shader) { name = "RaceSky" };
            if (texture != null) mat.mainTexture = texture;
            AssetDatabase.CreateAsset(mat, MaterialPath);
            return mat;
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
