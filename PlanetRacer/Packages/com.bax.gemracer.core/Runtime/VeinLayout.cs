using System;

namespace GemRacer.Core
{
    /// <summary>
    /// D06-N: 광맥이 행성 표면 어디에 있는지. 지금까지 광맥은 "시간이 지나면 하나 캔다"는 순수한
    /// 시간 개념이라 좌표가 아예 없었다 — 화면에 광맥을 놓으려면 "몇 번째 광맥이 어느 각도에
    /// 있는지"가 필요해서 여기에 순수 함수로 둔다. UnityEngine을 안 쓰니 서버 검증에서도 같은 값이
    /// 나온다(CLAUDE.md 1번).
    ///
    /// 배치 규칙 — 채굴차는 행성을 도는 큰 원(대원) 위를 일정한 각속도로 돈다. 광맥 VeinCount개를
    /// 그 원 위에 같은 간격으로 놓으면, 채굴차가 광맥 한 칸을 지나는 데 걸리는 시간이 정확히
    /// MiningRunState의 이동 단계 길이(Circumference / VeinCount / RigSpeed)와 같아진다. 그래서
    /// "도착했다"는 코어의 판정과 "광맥 앞에 섰다"는 화면이 어긋나지 않는다 — 화면 쪽에서 속도를
    /// 따로 적분하지 않고 코어의 진행도(MiningRunState.VeinProgress)를 각도로 바꿔 쓰기 때문에
    /// 몇 시간을 돌려도 오차가 쌓이지 않는다.
    ///
    /// 각도 0도는 채굴차의 출발 지점이고, k번 광맥은 (k + 1) × 간격 도에 있다 — 출발하자마자
    /// 0번 광맥으로 향하고, 마지막 광맥은 한 바퀴를 돌아 출발 지점에 놓인다.
    /// </summary>
    public static class VeinLayout
    {
        /// <summary>광맥 사이의 각도 간격(도).</summary>
        public static float AngleStepDegrees(Planet planet)
            => 360f / Math.Max(1, planet.VeinCount);

        /// <summary>index번 광맥의 각도(도). 0 이상 360 미만. 음수나 VeinCount 이상의 index도
        /// 한 바퀴 감아서 받는다 — 채굴차가 몇 바퀴째인지 신경 쓰지 않아도 되게.</summary>
        public static float VeinAngleDegrees(Planet planet, int index)
            => Normalize360(AngleStepDegrees(planet) * (index + 1));

        /// <summary>지나온 광맥 칸 수(MiningRunState.VeinProgress, 소수 포함)를 각도로 바꾼다.
        /// 여기서는 360도로 접지 않는다 — 회전을 그대로 누적해 쓰는 쪽이 화면에서 매끄럽다.</summary>
        public static float ProgressToAngleDegrees(Planet planet, float veinProgress)
            => AngleStepDegrees(planet) * veinProgress;

        /// <summary>몇 바퀴를 돌았든 0 이상 VeinCount 미만의 광맥 번호로 접는다.</summary>
        public static int WrapIndex(Planet planet, int index)
        {
            var count = Math.Max(1, planet.VeinCount);
            var i = index % count;
            return i < 0 ? i + count : i;
        }

        /// <summary>0 이상 360 미만으로 접는다.</summary>
        public static float Normalize360(float degrees)
        {
            var d = degrees % 360f;
            return d < 0f ? d + 360f : d;
        }
    }
}
