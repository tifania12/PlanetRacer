using System.IO;
using UnityEditor;
using UnityEngine;
using GemRacer.Core;
using GemRacer.Data;

namespace GemRacer.EditorTools
{
    /// <summary>
    /// D02-N: docs/design/balance/*.csv를 읽어 Assets/Data/Balance.asset(BalanceTable)에 채운다.
    /// CSV 파싱은 서버·Core.Tests와 같은 코드(Core의 BalanceCsv)를 쓴다 — 파싱 로직이 갈라지면
    /// CSV 값이 같아도 결과가 달라질 수 있어서다.
    /// 같은 메뉴를 다시 눌러도 같은 에셋을 덮어쓸 뿐 새로 만들지 않는다(멱등, CLAUDE.md 3번).
    ///
    /// GemRacer/2. 밸런스 CSV 가져오기
    /// </summary>
    public static class ImportBalance
    {
        const string AssetPath = "Assets/Data/Balance.asset";

        [MenuItem("GemRacer/2. 밸런스 CSV 가져오기")]
        public static void Import()
        {
            var balanceDir = Path.Combine(Application.dataPath, "..", "..", "docs", "design", "balance");
            balanceDir = Path.GetFullPath(balanceDir);

            if (!Directory.Exists(balanceDir))
            {
                Debug.LogError($"[GemRacer] 밸런스 CSV 폴더를 못 찾았다: {balanceDir}");
                return;
            }

            var planetsPath = Path.Combine(balanceDir, "planets.csv");
            var coursesPath = Path.Combine(balanceDir, "courses.csv");
            var partsPath = Path.Combine(balanceDir, "parts.csv");

            var table = AssetDatabase.LoadAssetAtPath<BalanceTable>(AssetPath);
            var isNew = table == null;
            if (isNew) table = ScriptableObject.CreateInstance<BalanceTable>();

            table.planets.Clear();
            foreach (var p in BalanceCsv.ParsePlanets(File.ReadAllText(planetsPath)))
                table.planets.Add(new BalanceTable.PlanetEntry
                {
                    id = p.Id, nameKo = p.NameKo, order = p.Order, circumference = p.Circumference,
                    heat = p.Heat, cold = p.Cold, roughness = p.Roughness, liquid = p.Liquid,
                    toxic = p.Toxic, gravity = p.Gravity, atmosphere = p.Atmosphere,
                    veinCount = p.VeinCount, veinYield = p.VeinYield,
                });

            table.courses.Clear();
            foreach (var c in BalanceCsv.ParseCourses(File.ReadAllText(coursesPath)))
                table.courses.Add(new BalanceTable.CourseEntry
                {
                    id = c.Id, nameKo = c.NameKo, planetId = c.PlanetId, length = c.Length, laps = c.Laps,
                    flatRatio = c.FlatRatio, roughRatio = c.RoughRatio, boostRatio = c.BoostRatio,
                });

            table.parts.Clear();
            foreach (var part in BalanceCsv.ParseParts(File.ReadAllText(partsPath)))
                table.parts.Add(new BalanceTable.PartEntry
                {
                    id = part.Id, nameKo = part.NameKo, slot = part.Slot.ToString(), grade = part.Grade.ToString(),
                    planetId = part.PlanetId,
                    power = part.Base.Power, grip = part.Base.Grip, suspension = part.Base.Suspension,
                    durability = part.Base.Durability, boost = part.Base.Boost, aero = part.Base.Aero,
                    heatResist = part.Base.HeatResist, seal = part.Base.Seal, filter = part.Base.Filter,
                });

            if (isNew)
            {
                EnsureFolder("Assets/Data");
                AssetDatabase.CreateAsset(table, AssetPath);
            }
            else
            {
                EditorUtility.SetDirty(table);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GemRacer] 밸런스 가져오기 완료: 행성 {table.planets.Count}개, " +
                      $"코스 {table.courses.Count}개, 부품 {table.parts.Count}개 → {AssetPath}");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
