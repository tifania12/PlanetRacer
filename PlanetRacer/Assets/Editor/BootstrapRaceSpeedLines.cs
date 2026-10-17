using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using GemRacer.Planet;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-27 레이스 속도선 겹침 배선. 지금 열려 있는 씬에 전용 캔버스(RaceSpeedLinesCanvas)를 하나 세우고
    /// 화면을 꽉 채우는 RawImage에 <see cref="RaceSpeedLinesOverlay"/>를 단다.
    ///
    /// 캔버스는 Screen Space - Overlay, sortingOrder는 낮게(-10) 둔다 — 레이스 중 HUD·팝업 아래에 깔려야 한다.
    /// 그림은 실행 중에 Resources에서 물리고, 투명도는 속도에 따라 컴포넌트가 바꾼다.
    /// 멱등하다 — 다시 눌러도 캔버스는 하나만 남는다.
    ///
    /// GemRacer/49. 레이스 속도선 겹침 (A-27)
    /// </summary>
    static class BootstrapRaceSpeedLines
    {
        const string CanvasName = "RaceSpeedLinesCanvas";

        [MenuItem("GemRacer/49. 레이스 속도선 겹침 (A-27)")]
        public static void Run()
        {
            var raceCam = Object.FindFirstObjectByType<RaceCamera>();
            if (raceCam == null)
            {
                Debug.LogWarning("[A-27] 이 씬에는 RaceCamera가 없다. 레이스 화면이 있는 씬" +
                                 "(Assets/Scenes/RaceCameraSpike.unity)을 열고 다시 누른다.");
                return;
            }

            var existing = GameObject.Find(CanvasName);
            var go = existing != null ? existing : new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -10;

            var imgTr = go.transform.Find("SpeedLines");
            if (imgTr == null)
            {
                var child = new GameObject("SpeedLines", typeof(RectTransform), typeof(RawImage));
                child.transform.SetParent(go.transform, false);
                imgTr = child.transform;
            }
            var rt = (RectTransform)imgTr;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var raw = imgTr.GetComponent<RawImage>();
            raw.raycastTarget = false;

            var overlay = go.GetComponent<RaceSpeedLinesOverlay>();
            if (overlay == null) overlay = go.AddComponent<RaceSpeedLinesOverlay>();
            overlay.raceCamera = raceCam;
            overlay.image = raw;

            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log("[A-27] 속도선 겹침을 세웠다: " + CanvasName + ". Play하고 차가 최고 속도에 가까워지면 " +
                      "화면 가장자리에 선이 진해진다. 천천히 달릴 때는 보이지 않는다.");
        }
    }
}
