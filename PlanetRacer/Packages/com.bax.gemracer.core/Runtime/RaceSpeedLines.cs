using System;

namespace GemRacer.Core
{
    /// <summary>A-27 레이스 속도선 겹침의 투명도 곡선. 화면 가장자리에 깔리는 속도선 그림이
    /// 속도에 따라 얼마나 진하게 보일지만 정한다. 그림·카메라는 Unity 쪽이 맡는다.
    /// 천천히 달릴 때까지 선이 보이면 "항상 빠른" 화면이 되어 속도감이 사라지므로,
    /// 시작 속도 아래에서는 완전히 꺼 두고 그 위부터 부드럽게 올린다.</summary>
    public static class RaceSpeedLines
    {
        /// <summary>최고 속도 대비 이 비율 아래에서는 속도선이 없다.</summary>
        public const double StartFraction = 0.35;

        /// <summary>최고 속도에서도 이 값을 넘지 않는다. 차와 코스를 가리면 안 된다.</summary>
        public const double MaxOpacity = 0.6;

        /// <summary>0 ~ <see cref="MaxOpacity"/>. fullSpeed는 속도선이 가장 진해지는 속도(m/s).
        /// 속도가 음수·NaN이거나 fullSpeed가 0 이하면 0(꺼짐)을 돌려준다.</summary>
        public static double Opacity(double speed, double fullSpeed)
        {
            if (double.IsNaN(speed) || double.IsNaN(fullSpeed) || fullSpeed <= 0 || speed <= 0) return 0;
            double k = Math.Min(1.0, speed / fullSpeed);
            if (k <= StartFraction) return 0;
            double t = (k - StartFraction) / (1.0 - StartFraction);
            double smooth = t * t * (3.0 - 2.0 * t);   // 켜지는 순간과 최대에서 기울기가 0이라 깜빡이지 않는다
            return smooth * MaxOpacity;
        }
    }
}
