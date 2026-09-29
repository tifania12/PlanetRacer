using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GemRacer.UI;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// A-25(2026-09-30): 강화 화면(`UI Canvas/Overlays/Upgrade`)에 UI 스킨을 입힌다.
    /// HUD(`GemRacer/37`)에 이은 두 번째 화면이다.
    ///
    /// **더하는(additive) 메뉴**라 여러 번 눌러도 된다 — 이미 있는 노드를 찾아 스프라이트만 바꾼다.
    /// `GemRacer/16`(강화 화면 세우기)을 다시 누르면 화면을 통째로 지우고 새로 만들기 때문에
    /// 손으로 물려 둔 `MainHudUgui.upgradePanel` 연결이 끊긴다. 그래서 이 메뉴를 따로 뒀다.
    ///
    /// 실제로 얹는 일은 `UiSkin.ApplyToUpgrade`가 한다 — 런타임(`UpgradeUgui.Awake`)도 같은
    /// 함수를 불러서, 씬에 구운 것과 빌드에서 물리는 것이 어긋날 수 없다.
    /// </summary>
    public static class BootstrapUpgradeSkin
    {
        [MenuItem("GemRacer/38. 강화 화면에 UI 스킨 입히기 (A-25)")]
        public static void Apply()
        {
            var panel = GameObject.Find("UI Canvas/Overlays/Upgrade");
            if (panel == null)
            {
                Debug.LogError("[GemRacer] 씬에서 'UI Canvas/Overlays/Upgrade'를 못 찾았다. " +
                               "MainGame 씬을 열고 다시 실행할 것(화면이 아직 없으면 'GemRacer/16' 먼저).");
                return;
            }

            var done = UiSkin.ApplyToUpgrade(panel.transform);
            if (done == 0)
            {
                Debug.LogWarning("[GemRacer] UI 스킨을 한 곳도 못 얹었다. " +
                                 "'Assets/Resources/Art/UI/ui-panel.png'·'ui-header.png'·'ui-button.png'이 있는지, " +
                                 "'GemRacer/91'로 임포트 설정(9-slice 테두리)을 넣었는지 확인할 것.");
                return;
            }

            EditorUtility.SetDirty(panel);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[GemRacer] 강화 화면에 UI 스킨 {done}곳 얹었다(화폐 띠 1 + 카드 4 + 버튼 5). 씬을 저장할 것.");
        }
    }
}
