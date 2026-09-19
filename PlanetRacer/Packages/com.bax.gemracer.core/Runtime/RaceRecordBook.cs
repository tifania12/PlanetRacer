using System;
using System.Collections.Generic;

namespace GemRacer.Core
{
    /// <summary>A-04: 레이스 결과 화면에서 "이전 기록 대비" 차이를 보여주기 위한 코스별 자기
    /// 최고 기록. SaveData가 JsonUtility 직렬화라 Dictionary를 못 써서(SaveData.cs 상단 주석과
    /// 같은 이유) 코스 id·기록을 병렬 리스트로 들고 다닌다(OwnedPartIds/OwnedPartEnhanceLevels와
    /// 같은 패턴) — 이 클래스는 그 병렬 리스트를 다루는 순수 함수만 제공한다. 리스트 자체는
    /// SaveData.RaceRecordCourseIds/RaceRecordBestSeconds를 그대로 넘기면 된다.</summary>
    public static class RaceRecordBook
    {
        public struct UpdateResult
        {
            public bool HasPreviousRecord;
            public float PreviousBestSeconds;
            public bool IsNewRecord;
            /// <summary>이번 기록 - 이전 최고 기록(초). 양수면 더 느려졌다는 뜻.
            /// HasPreviousRecord가 false면 의미 없다(항상 0).</summary>
            public float DeltaSeconds;
        }

        /// <summary>courseId의 기존 최고 기록을 찾아 이번 timeSeconds와 비교하고, 처음이거나
        /// 더 빠르면 courseIds/bestSeconds를 실제로 갱신한다(리스트를 직접 수정 — 호출하는 쪽이
        /// SaveData 필드를 그대로 넘기면 된다).</summary>
        public static UpdateResult Update(List<string> courseIds, List<float> bestSeconds, string courseId, float timeSeconds)
        {
            if (courseIds.Count != bestSeconds.Count)
                throw new ArgumentException("courseIds와 bestSeconds 길이가 다르다 — 세이브가 손상된 것으로 본다.");
            if (string.IsNullOrEmpty(courseId)) throw new ArgumentException("courseId가 비어 있다.");
            if (timeSeconds <= 0f) throw new ArgumentException("timeSeconds는 0보다 커야 한다.");

            var idx = courseIds.IndexOf(courseId);
            if (idx < 0)
            {
                courseIds.Add(courseId);
                bestSeconds.Add(timeSeconds);
                return new UpdateResult { HasPreviousRecord = false, PreviousBestSeconds = 0f, IsNewRecord = true, DeltaSeconds = 0f };
            }

            var previous = bestSeconds[idx];
            var isNewRecord = timeSeconds < previous;
            if (isNewRecord) bestSeconds[idx] = timeSeconds;
            return new UpdateResult
            {
                HasPreviousRecord = true,
                PreviousBestSeconds = previous,
                IsNewRecord = isNewRecord,
                DeltaSeconds = timeSeconds - previous,
            };
        }

        /// <summary>기록만 조회한다(갱신 없음). 없으면 null.</summary>
        public static float? BestOf(List<string> courseIds, List<float> bestSeconds, string courseId)
        {
            var idx = courseIds.IndexOf(courseId);
            return idx < 0 ? (float?)null : bestSeconds[idx];
        }
    }
}
