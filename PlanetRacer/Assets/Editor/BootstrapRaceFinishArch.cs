using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using GemRacer.Planet;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-27 결승 아치 배선. 지금 열려 있는 씬에 판 한 장(RaceFinishArch)을 세우고
    /// <see cref="GemRacer.Planet.RaceFinishArch"/>를 단다.
    ///
    /// 자리는 차(<see cref="SurfaceMover"/>)의 출발 지점이다 — 한 바퀴의 결승선이 거기이기 때문이다.
    /// 행성 중심·반지름·궤도축을 그 컴포넌트에서 그대로 읽어 오므로, 레이스 실험 씬을 다시 만들어
    /// 값이 바뀌어도 메뉴를 다시 누르면 맞는 자리로 간다.
    ///
    /// 머티리얼은 URP/Unlit 투명이다. 아치 그림은 가운데가 비어 있어서(알파) 불투명으로 두면
    /// 네모난 판이 그대로 보인다. 그림자도 끈다 — 투명한 판의 그림자는 네모로 떨어진다.
    ///
    /// 멱등하다 — 다시 눌러도 판이 하나만 남고 값은 같은 자리로 돌아온다.
    ///
    /// GemRacer/48. 레이스 결승 아치 (A-27)
    /// </summary>
    static class BootstrapRaceFinishArch
    {
        const string ArchName = "RaceFinishArch";
        const string MaterialFolder = "Assets/Art/Materials";
        const string MaterialPath = MaterialFolder + "/RaceFinishArch.mat";
        const string TexturePath = "Assets/Resources/Art/Race/race-finish-banner.png";
        const float ArchWidth = 14f;   // 주행로 8m + 양쪽 여유. 16:9 그림이라 높이는 약 7.9m

        [MenuItem("GemRacer/48. 레이스 결승 아치 (A-27)")]
        public static void Run()
        {
            var mover = Object.FindFirstObjectByType<SurfaceMover>();
            if (mover == null)
            {
                Debug.LogWarning("[A-27] 이 씬에는 SurfaceMover(달리는 차)가 없다. 레이스 화면이 있는 씬" +
                                 "(Assets/Scenes/RaceCameraSpike.unity)을 열고 다시 누른다.");
                return;
            }

            var center = mover.planetCenter != null ? mover.planetCenter.position : Vector3.zero;
            var startDirection = mover.transform.position - center;
            if (startDirection.sqrMagnitude < 0.0001f) startDirection = Vector3.forward;

            var material = GetOrCreateMaterial();
            var arch = FindOrCreateArch();

            var renderer = arch.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }

            var comp = arch.GetComponent<RaceFinishArch>();
            if (comp == null) comp = arch.AddComponent<RaceFinishArch>();
            comp.planetCenter = mover.planetCenter;
            comp.radius = mover.radius;
            comp.orbitAxis = mover.orbitAxis;
            comp.lapStartDirection = startDirection.normalized;
            comp.width = ArchWidth;
            comp.Place();

            EditorUtility.SetDirty(arch);
            if (renderer != null) EditorUtility.SetDirty(renderer);
            EditorSceneManager.MarkSceneDirty(mover.gameObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[A-27] 결승 아치를 세웠다: {ArchName}, 폭 {ArchWidth}m, 반지름 {mover.radius}m, " +
                      $"출발 지점 {comp.lapStartDirection}.\n" +
                      "Play하면 차가 출발하자마자 아치를 통과하고, 한 바퀴(약 17초) 뒤에 정면으로 다시 들어온다.");
        }

        static GameObject FindOrCreateArch()
        {
            var existing = GameObject.Find(ArchName);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = ArchName;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);   // 아치를 클릭하거나 부딪힐 일은 없다
            return go;
        }

        static Material GetOrCreateMaterial()
        {
            EnsureFolder(MaterialFolder);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture == null)
            {
                Debug.LogWarning($"[A-27] 아치 그림을 못 찾았다: {TexturePath}. " +
                                 "머티리얼만 만들어 둔다 — 실행 중에 Resources에서 다시 찾는다.");
            }

            var existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (existing != null)
            {
                if (texture != null) existing.mainTexture = texture;
                MakeTransparent(existing);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogWarning("[A-27] URP Unlit 셰이더를 못 찾았다. URP 설정 확인 필요.");
                return null;
            }
            var mat = new Material(shader) { name = "RaceFinishArch" };
            if (texture != null) mat.mainTexture = texture;
            MakeTransparent(mat);
            AssetDatabase.CreateAsset(mat, MaterialPath);
            return mat;
        }

        /// <summary>URP/Unlit을 알파 블렌딩 투명으로 바꾼다. 인스펙터에서 Surface Type을 Transparent로
        /// 고른 것과 같은 상태다 — 값·키워드·렌더 큐 셋을 다 맞춰야 실제로 투명해진다.</summary>
        static void MakeTransparent(Material mat)
        {
            if (mat == null) return;
            mat.SetFloat("_Surface", 1f);            // 0 불투명 / 1 투명
            mat.SetFloat("_Blend", 0f);              // 0 알파 블렌딩
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.SetShaderPassEnabled("ShadowCaster", false);
            mat.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(mat);
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
