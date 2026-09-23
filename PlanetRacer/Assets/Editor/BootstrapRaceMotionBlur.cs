using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-22 레이스 모션 블러. URP Volume Profile 하나에 Motion Blur 오버라이드를 만들고,
    /// 지금 열려 있는 씬의 RaceCamera에만 후처리를 켠다.
    ///
    /// 채굴 화면은 느긋한 게 컨셉이라 안 켠다. 채굴 카메라(CameraFollow)는 후처리 자체가
    /// 꺼져 있어서 전역 볼륨이 씬에 있어도 영향을 받지 않는다 — 그래서 지금은 전용 레이어로
    /// 가르지 않았다. 나중에 채굴 카메라에도 후처리를 켤 일이 생기면 그때 레이어 +
    /// volumeLayerMask로 갈라야 한다(그 전까지는 필요 없는 복잡함이다).
    ///
    /// 값은 약하게 잡았다. 모바일 30fps와 WebGL을 같이 보는 프로젝트라(W-09가 이미 용량·
    /// 성능 예산에 걸려 있다) 품질은 Low, 세기는 0.35다. 세기를 더 올리려면 여기 상수만 고친다.
    ///
    /// 멱등하다 — 다시 눌러도 볼륨 오브젝트가 하나만 남고 값은 같은 자리로 돌아온다.
    /// </summary>
    static class BootstrapRaceMotionBlur
    {
        const string ProfilePath = "Assets/Settings/RaceMotionBlurProfile.asset";
        const string VolumeName = "RaceMotionBlurVolume";
        const float Intensity = 0.35f;
        const float Clamp = 0.05f;

        [MenuItem("GemRacer/30. 레이스 모션 블러 볼륨 (A-22)")]
        public static void Run()
        {
            var profile = CreateOrUpdateProfile();

            var raceCam = Object.FindFirstObjectByType<GemRacer.Planet.RaceCamera>();
            if (raceCam == null)
            {
                Debug.LogWarning("[A-22] 이 씬에는 RaceCamera가 없다. 프로파일(" + ProfilePath +
                                 ")만 만들어 두었다. 레이스 화면이 있는 씬을 열고 다시 누르면 볼륨까지 붙는다.");
                return;
            }

            // 전역 볼륨 하나. 카메라 자식으로 두면 씬을 옮겨도 같이 따라간다.
            var go = FindOrCreateVolumeObject(raceCam.transform);
            var volume = go.GetComponent<Volume>();
            if (volume == null) volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.weight = 1f;
            volume.sharedProfile = profile;

            // 이 카메라에서만 후처리를 켠다.
            var cam = raceCam.GetComponent<Camera>();
            var data = cam != null ? cam.GetUniversalAdditionalCameraData() : null;
            if (data != null) data.renderPostProcessing = true;

            EditorUtility.SetDirty(go);
            if (cam != null) EditorUtility.SetDirty(cam);
            EditorSceneManager.MarkSceneDirty(raceCam.gameObject.scene);

            Debug.Log("[A-22] 레이스 모션 블러 배선 끝. 카메라=" + raceCam.name +
                      " / 프로파일=" + ProfilePath + " / 세기=" + Intensity + " / 품질=Low");
        }

        static VolumeProfile CreateOrUpdateProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                var dir = System.IO.Path.GetDirectoryName(ProfilePath);
                if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets", "Settings");
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            if (!profile.TryGet<MotionBlur>(out var mb))
            {
                // 함정: VolumeProfile.Add()는 메모리에만 오버라이드를 얹는다. 에셋 파일에
                // 하위 오브젝트로 같이 넣어 주지 않으면 플레이에 들어가 에셋을 다시 읽는
                // 순간 통째로 사라진다(값은 에디터에서 멀쩡해 보이는데 런타임 스택에는
                // intensity=0으로 들어온다 — 2026-09-24 배선 세션에서 실제로 겪었다).
                mb = profile.Add<MotionBlur>(true);
                mb.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(mb, profile);
            }

            mb.active = true;
            mb.mode.overrideState = true;
            mb.mode.value = MotionBlurMode.CameraOnly;   // 차가 아니라 카메라가 움직여서 생기는 블러
            mb.quality.overrideState = true;
            mb.quality.value = MotionBlurQuality.Low;    // 모바일·WebGL 예산
            mb.intensity.overrideState = true;
            mb.intensity.value = Intensity;
            mb.clamp.overrideState = true;
            mb.clamp.value = Clamp;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static GameObject FindOrCreateVolumeObject(Transform parent)
        {
            var existing = parent.Find(VolumeName);
            if (existing != null) return existing.gameObject;

            // 예전에 카메라 밖에 만들어 둔 것이 있으면 그걸 쓴다(멱등 유지).
            foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (v.gameObject.name == VolumeName) return v.gameObject;

            var go = new GameObject(VolumeName);
            Undo.RegisterCreatedObjectUndo(go, "레이스 모션 블러 볼륨");
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
