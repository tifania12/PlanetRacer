using System;
using System.IO;
using UnityEngine;

namespace GemRacer.Diagnostics
{
    /// <summary>D17-N: 지인 테스트 준비 — 접속 시각·플레이 시간을 로컬 파일에 남긴다. 나중에
    /// D23-N(관문 판정: 3일 연속 접속 인원, 이탈 지점)이 이 로그를 읽어 분석할 몫이라, 지금은
    /// 형식을 단순하게(CSV, 한 줄에 세션 하나) 남기는 데만 집중한다.
    ///
    /// SaveData(코어)에 넣지 않은 이유: SaveData는 매번 통째로 덮어써서(SaveService.Save) 지난
    /// 세션 기록이 남지 않는다. 세션 로그는 원래 "누적해서 쌓이는 이력"이라 SaveData와 성격이
    /// 다르다 — 그래서 SaveData와 별개로 이 클래스가 직접 파일에 append한다.
    ///
    /// "세션" = 앱이 화면 앞에 있는 동안(포그라운드) 한 구간. 모바일에서 홈 버튼으로 백그라운드에
    /// 갔다가(OnApplicationPause(true)) 돌아오면(OnApplicationPause(false)) 새 세션으로 끊어서
    /// 센다 — MiningController의 자동 저장/오프라인 보상과 같은 "일시정지 = 세션 경계" 관점이다.</summary>
    [DisallowMultipleComponent]
    public sealed class SessionLogger : MonoBehaviour
    {
        const string FileName = "session_log.csv";
        const string Header = "start_unix,start_iso_utc,duration_seconds";

        static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        long _segmentStartUnixSeconds;

        void Awake() => BeginSegment();

        // OnApplicationQuit이 모바일에서 호출이 보장되지 않는 것과 같은 이유로(MiningController
        // 주석 참고) 백그라운드 전환(OnApplicationPause(true))을 세션 종료의 주된 신호로 쓴다.
        void OnApplicationPause(bool paused)
        {
            if (paused) EndSegment();
            else BeginSegment();
        }

        void OnApplicationQuit() => EndSegment();

        void BeginSegment() => _segmentStartUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        void EndSegment()
        {
            if (_segmentStartUnixSeconds <= 0) return; // 이미 끊은 세션을 두 번 기록하지 않는다

            var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // 기기 시계가 거꾸로 조정된 경우(드묾)에도 음수 시간을 기록하지 않는다 —
            // RaceFuel.Recover의 Math.Max(0L, ...)과 같은 방어.
            var durationSeconds = Math.Max(0L, nowUnixSeconds - _segmentStartUnixSeconds);
            AppendLine(_segmentStartUnixSeconds, durationSeconds);
            _segmentStartUnixSeconds = 0;
        }

        static void AppendLine(long startUnixSeconds, long durationSeconds)
        {
            try
            {
                var isFirstWrite = !File.Exists(FilePath);
                var startIso = DateTimeOffset.FromUnixTimeSeconds(startUnixSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ");
                var line = $"{startUnixSeconds},{startIso},{durationSeconds}\n";
                if (isFirstWrite) File.AppendAllText(FilePath, Header + "\n");
                File.AppendAllText(FilePath, line);
            }
            catch (Exception e)
            {
                // 로그 기록 실패가 게임 플레이를 막으면 안 된다 — 경고만 남기고 넘어간다.
                Debug.LogWarning($"[GemRacer] 세션 로그 기록 실패: {e.Message}");
            }
        }
    }
}
