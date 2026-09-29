using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-25(2026-09-29): HUD에 UI 스킨을 입힌다. **더하는(additive) 메뉴**라 여러 번 눌러도 된다 —
    /// 이미 있는 노드를 찾아 스프라이트만 바꾼다.
    ///
    /// `GemRacer/13`(HUD 세우기)을 다시 누르면 안 된다는 규칙(CLAUDE.md)이 있어서 이렇게 만들었다.
    /// 지금 `MainGame.unity`에는 화면 열여섯 개가 각자의 메뉴로 쌓여 있고, 13번은 "UI Canvas"를
    /// 통째로 지우고 다시 만들기 때문에 그걸 다 날린다.
    ///
    /// 실제로 얹는 일은 `UiSkin.ApplyToHud`가 한다 — 런타임(`MainHudUgui.Awake`)도 같은 함수를
    /// 불러서, 씬에 구운 것과 빌드에서 물리는 것이 어긋날 수 없다.
    /// </summary>
    public static class BootstrapHudSkin
    {
        [MenuItem("GemRacer/37. HUD에 UI 스킨 입히기 (A-25)")]
        public static void Apply()
        {
            var hud = GameObject.Find("UI Canvas/HUD");
            if (hud == null)
            {
                Debug.LogError("[GemRacer] 씬에서 'UI Canvas/HUD'를 못 찾았다. " +
                               "MainGame 씬을 열고 다시 실행할 것(HUD가 아직 없으면 'GemRacer/13' 먼저).");
                return;
            }

            var done = UiSkin.ApplyToHud(hud.transform);
            if (done == 0)
            {
                Debug.LogWarning("[GemRacer] UI 스킨을 한 곳도 못 얹었다. " +
                                 "'Assets/Resources/Art/UI/ui-panel.png'·'ui-button.png'이 있는지, " +
                                 "'GemRacer/91'로 임포트 설정(9-slice 테두리)을 넣었는지 확인할 것.");
                return;
            }

            EditorUtility.SetDirty(hud);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[GemRacer] HUD에 UI 스킨 {done}곳 얹었다(상태바 + 액션 줄 버튼). 씬을 저장할 것.");
        }
    }
}
