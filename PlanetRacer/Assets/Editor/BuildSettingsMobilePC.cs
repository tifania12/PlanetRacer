using UnityEditor;
using UnityEngine;

namespace GemRacer.EditorTools
{
    /// <summary>D15-N. 안드로이드·PC(Steam) 빌드 전에 확인할 Player Settings 중, 코드로 확실히
    /// 되는 부분만 여기서 적용한다. 나머지(패키지명, 키스토어)는 사람 판단이나 비밀 정보가 들어가서
    /// `docs/design/android-build-checklist.md`에 절차로만 남겨 뒀다.
    ///
    /// 두 메뉴 다 몇 개 필드만 덮어쓰는 멱등 함수라 몇 번을 다시 눌러도 같은 결과다.
    /// `ProjectSettings/ProjectSettings.asset`의 실제 YAML 필드명(defaultInterfaceOrientation의
    /// 직렬화 키는 defaultScreenOrientation, allowedAutorotateTo*, defaultScreenWidth/Height,
    /// resizableWindow, fullscreenMode)을 직접 대조하고 썼다 — 클라우드 세션엔 에디터가 없어서
    /// 컴파일 확인은 다음 PC 세션 몫이다.
    /// </summary>
    public static class BuildSettingsMobilePC
    {
        [MenuItem("GemRacer/9. 안드로이드 세로 고정 적용")]
        public static void ApplyAndroidPortraitLock()
        {
            // CLAUDE.md 6번 규칙: 모바일은 세로(9:16)가 기준 화면이다. 기기를 돌려도
            // 가로로 안 눕게 방향을 세로 하나로만 고정한다.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            AssetDatabase.SaveAssets();
            Debug.Log("[GemRacer] 안드로이드 화면 방향: 세로 고정 적용.");
        }

        [MenuItem("GemRacer/10. PC 세로 창 설정 적용")]
        public static void ApplyPcPortraitWindow()
        {
            // Steam(Standalone) 빌드도 기본 창을 세로 540×960으로 열어서, 모바일과 같은
            // 기준 화면을 PC에서도 그대로 볼 수 있게 한다. 리사이즈는 허용 — 창을 넓히면
            // ResponsiveLayout이 가로 960×540/태블릿 1280×800 배치로 바뀌는지 그 자리에서 확인할 수 있다.
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            AssetDatabase.SaveAssets();
            Debug.Log("[GemRacer] PC 기본 창: 540×960 세로, 리사이즈 허용, 창 모드 적용.");
        }
    }
}
