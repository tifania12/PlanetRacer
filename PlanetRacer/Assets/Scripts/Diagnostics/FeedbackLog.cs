using System;
using System.IO;
using UnityEngine;

namespace GemRacer.Diagnostics
{
    /// <summary>D17-N: 지인 테스트용 "게임 안 피드백" 버튼이 부르는 저장 로직. 텍스트를 그대로
    /// 로컬 파일(persistentDataPath/feedback.txt)에 이어서 남긴다 — 지인 테스트 규모(5명)에
    /// 서버 업로드는 과하고, 오프라인에서도 동작해야 한다(GDD 목표: 모바일+Steam 오프라인 플레이).
    ///
    /// "공유"는 별도 네이티브 공유 시트 플러그인 없이, 클립보드 복사(GUIUtility.systemCopyBuffer)로
    /// 대신한다 — 카카오톡 등 아무 메신저에나 붙여넣어 보낼 수 있으면 충분하고, 이 API는 오래된
    /// 표준 Unity API라 에디터 없이도 확신할 수 있다(CLAUDE.md 3번 규칙 — 확실하지 않은 API는
    /// 안 쓴다).</summary>
    public static class FeedbackLog
    {
        const string FileName = "feedback.txt";

        static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>빈 문자열/공백뿐이면 아무 일도 안 하고 false. 성공하면 파일에 append하고
        /// 클립보드에도 복사한 뒤 true.</summary>
        public static bool Append(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var trimmed = text.Trim();
            try
            {
                var stamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                File.AppendAllText(FilePath, $"[{stamp}]\n{trimmed}\n\n");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GemRacer] 피드백 저장 실패: {e.Message}");
                return false;
            }

            GUIUtility.systemCopyBuffer = trimmed;
            return true;
        }
    }
}
