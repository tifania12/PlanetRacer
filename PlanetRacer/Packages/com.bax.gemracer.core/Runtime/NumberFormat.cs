using System;
using System.Globalization;

namespace GemRacer.Core
{
    /// <summary>E-10(economy-v2.md 3-5): 화면에 찍는 큰 숫자를 K·M·B·T로 줄인다.
    /// 500레벨이면 잔고가 10^10을 넘는데 F1 생짜로 찍으면 HUD 칸을 넘친다.
    /// 문화권(쉼표/마침표)에 따라 모양이 달라지면 안 되므로 항상 InvariantCulture로 찍는다.</summary>
    public static class NumberFormat
    {
        static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

        /// <summary>1000 미만은 소수 첫째 자리까지(기존 F1 표시 그대로), 그 위는 유효숫자 3자리.
        /// 예: 999.5 → "999.5", 1234 → "1.23K", 12345 → "12.3K", 123456 → "123K", 1.5e9 → "1.50B".
        /// T 위로는 단위가 없어 "1234T"처럼 T를 계속 쓴다. NaN·무한대는 "0"으로 보여 준다(화면이 깨지지 않게).</summary>
        public static string Abbreviate(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "0";
            if (value < 0) return "-" + Abbreviate(-value);
            if (value < 1000) 
            {
                // 999.96은 F1로 반올림하면 "1000.0"이 되니 K 쪽으로 넘긴다.
                if (Math.Round(value, 1) < 1000) return value.ToString("F1", CultureInfo.InvariantCulture);
            }

            int tier = 0;
            double scaled = value;
            while (scaled >= 1000 && tier < Suffixes.Length - 1)
            {
                scaled /= 1000;
                tier++;
            }

            string text = FormatThreeSignificant(scaled);
            // 999.6K는 3자리로 반올림하면 "1000"이 된다 — 한 단계 올려 "1.00M"으로.
            if (scaled < 1000 && double.Parse(text, CultureInfo.InvariantCulture) >= 1000 && tier < Suffixes.Length - 1)
            {
                tier++;
                text = FormatThreeSignificant(scaled / 1000);
            }
            return text + Suffixes[tier];
        }

        static string FormatThreeSignificant(double v)
        {
            if (v >= 100) return v.ToString("F0", CultureInfo.InvariantCulture);
            if (v >= 10) return v.ToString("F1", CultureInfo.InvariantCulture);
            return v.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}
