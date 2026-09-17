using GemRacer.Core;

namespace GemRacer.Mining
{
    /// <summary>M-11: 지금 돌고 있는 것이 모바일 판인지 Steam 판인지 코어에 알려 주는 글루.
    /// 코어(PlatformConfig)는 "이 판이면 뭐가 다른가"만 알고, "지금 어느 판인가"는 여기가 정한다.
    ///
    /// 빌드 타깃(UNITY_STANDALONE 같은 것)으로 정하지 않고 전용 정의 GEMRACER_STEAM으로 정한다.
    /// 이유는 두 가지다.
    /// 1) 에디터는 늘 Standalone이라 타깃으로 정하면 에디터에서 Play만 눌러도 Steam 판으로 읽혀서
    ///    화면이 달라진다. 확인하려던 것과 다른 것을 보게 된다.
    /// 2) Steam 판은 스토어 SDK가 같이 들어가야 성립하는 의도적인 빌드다. 빌드 타깃을 바꾸다가
    ///    실수로 켜지는 것보다 명시적으로 켜는 쪽이 맞다.
    ///
    /// Steam 빌드를 낼 때 Player Settings > Scripting Define Symbols에 GEMRACER_STEAM을 넣는다.
    /// 지금 CI(webgl.yml)에도 모바일 빌드에도 넣지 않았으니 전부 Mobile이다 — 즉 이 파일이
    /// 들어왔다고 해서 기존 빌드의 동작이 바뀌는 곳은 하나도 없다.</summary>
    public static class GamePlatform
    {
        /// <summary>빌드 정의가 정한 판. 에디터에서 반대쪽 화면을 보고 싶으면 이걸 고치지 말고
        /// MiningController의 overrideStorePlatform을 켠다.</summary>
        public static StorePlatform Build =>
#if GEMRACER_STEAM
            StorePlatform.Steam;
#else
            StorePlatform.Mobile;
#endif
    }
}
