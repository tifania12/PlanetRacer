using System.IO;
using UnityEngine;
using GemRacer.Core;

namespace GemRacer.Save
{
    /// <summary>
    /// D03-N: 세이브 파일 저장/불러오기. Application.persistentDataPath에 JSON으로 쓴다.
    /// 저장 중간에 앱이 꺼지면(배터리, 강제 종료) 파일이 반쯤 쓰인 채로 남을 수 있어서,
    /// 임시 파일에 다 쓴 뒤 마지막에 이름을 바꾸는 원자적 쓰기를 쓴다 — 어중간한 상태가
    /// 존재하지 않는다(CLAUDE.md 세이브 규칙).
    /// </summary>
    public static class SaveService
    {
        const string FileName = "save.json";

        static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
        static string TempFilePath => FilePath + ".tmp";

        public static void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(TempFilePath, json);
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(TempFilePath, FilePath);
        }

        /// <summary>파일이 없거나 읽는 데 실패하면 새 세이브를 준다 — 첫 실행이거나 파일이
        /// 깨진 경우 게임이 죽지 않고 처음부터 시작한다.</summary>
        public static SaveData Load()
        {
            if (!File.Exists(FilePath)) return new SaveData();

            try
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GemRacer] 세이브 파일을 읽는 데 실패했다({e.Message}). 새로 시작한다: {FilePath}");
                return new SaveData();
            }
        }

        public static bool Exists() => File.Exists(FilePath);
    }
}
