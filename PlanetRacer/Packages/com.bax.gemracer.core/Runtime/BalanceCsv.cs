using System;
using System.Collections.Generic;
using System.Globalization;

namespace GemRacer.Core
{
    /// <summary>
    /// docs/design/balance/*.csv를 코어 모델로 읽어 들이는 최소 CSV 파서.
    /// 서버·Core.Tests·Unity 에디터 셋 다 이 클래스 하나로 읽는다 — 파싱 로직을 세 번 안 만든다.
    /// NuGet 없이 오프라인으로 돌아야 해서 콤마 분리 이상은 안 한다. 값 안에 콤마나
    /// 줄바꿈이 들어가는 셀은 지금 밸런스 표에 없다. 헤더 이름으로 열을 찾으므로
    /// 열 순서를 바꿔도 안전하다. '#'으로 시작하는 줄과 빈 줄은 주석으로 건너뛴다.
    /// </summary>
    public static class BalanceCsv
    {
        public static List<Planet> ParsePlanets(string csv)
        {
            var result = new List<Planet>();
            foreach (var row in Rows(csv))
            {
                result.Add(new Planet
                {
                    Id = Text(row, "id"),
                    NameKo = Text(row, "nameKo"),
                    Order = Int(row, "order"),
                    Circumference = Float(row, "circumference"),
                    Heat = Float(row, "heat"),
                    Cold = Float(row, "cold"),
                    Roughness = Float(row, "roughness"),
                    Liquid = Float(row, "liquid"),
                    Toxic = Float(row, "toxic"),
                    Gravity = Float(row, "gravity"),
                    Atmosphere = Float(row, "atmosphere"),
                    VeinCount = Int(row, "veinCount"),
                    VeinYield = Float(row, "veinYield"),
                });
            }
            return result;
        }

        public static List<Course> ParseCourses(string csv)
        {
            var result = new List<Course>();
            foreach (var row in Rows(csv))
            {
                result.Add(new Course
                {
                    Id = Text(row, "id"),
                    NameKo = Text(row, "nameKo"),
                    PlanetId = Text(row, "planetId"),
                    Length = Float(row, "length"),
                    Laps = Int(row, "laps"),
                    FlatRatio = Float(row, "flatRatio"),
                    RoughRatio = Float(row, "roughRatio"),
                    BoostRatio = Float(row, "boostRatio"),
                });
            }
            return result;
        }

        public static List<Part> ParseParts(string csv)
        {
            var result = new List<Part>();
            foreach (var row in Rows(csv))
            {
                result.Add(new Part
                {
                    Id = Text(row, "id"),
                    NameKo = Text(row, "nameKo"),
                    Slot = (PartSlot)Enum.Parse(typeof(PartSlot), Text(row, "slot")),
                    Grade = (PartGrade)Enum.Parse(typeof(PartGrade), Text(row, "grade")),
                    PlanetId = Text(row, "planetId"),
                    Base = new Stats
                    {
                        Power = Float(row, "power"),
                        Grip = Float(row, "grip"),
                        Suspension = Float(row, "suspension"),
                        Durability = Float(row, "durability"),
                        Boost = Float(row, "boost"),
                        Aero = Float(row, "aero"),
                        HeatResist = Float(row, "heatResist"),
                        Seal = Float(row, "seal"),
                        Filter = Float(row, "filter"),
                    }
                });
            }
            return result;
        }

        static IEnumerable<Dictionary<string, string>> Rows(string csv)
        {
            var lines = csv.Replace("\r\n", "\n").Split('\n');
            string[]? headers = null;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var cells = line.Split(',');
                if (headers == null) { headers = cells; continue; }

                var row = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length && i < cells.Length; i++)
                    row[headers[i].Trim()] = cells[i].Trim();
                yield return row;
            }
        }

        static string Text(Dictionary<string, string> row, string key) =>
            row.TryGetValue(key, out var v) ? v : "";

        static float Float(Dictionary<string, string> row, string key) =>
            row.TryGetValue(key, out var v) && v.Length > 0 ? float.Parse(v, CultureInfo.InvariantCulture) : 0f;

        static int Int(Dictionary<string, string> row, string key) =>
            row.TryGetValue(key, out var v) && v.Length > 0 ? int.Parse(v, CultureInfo.InvariantCulture) : 0;
    }
}
