#!/usr/bin/env python3
"""관문 판정(D23-N)용 — 테스터별 session_log.csv에서 '3일 연속 접속' 인원을 센다.

사용법:  python tools/gate_retention.py 이름1.csv 이름2.csv ...
         (파일 이름이 곧 테스터 이름으로 표시된다)

날짜는 KST(UTC+9)로 바꿔서 센다. UTC로 세면 자정 근처 세션이 하루씩 밀린다.
형식은 docs/design/session-log-format.md 참고.
"""
import csv
import sys
from datetime import datetime, timedelta, timezone
from pathlib import Path

KST = timezone(timedelta(hours=9))


def load_days(path):
    """날짜(KST) → 그날 총 플레이 초."""
    days = {}
    with open(path, newline="", encoding="utf-8") as f:
        for row in csv.DictReader(f):
            try:
                start = datetime.fromtimestamp(int(row["start_unix"]), KST).date()
                sec = max(0, int(row["duration_seconds"]))
            except (KeyError, ValueError):
                continue  # 깨진 줄은 건너뛴다
            days[start] = days.get(start, 0) + sec
    return days


def longest_streak(days):
    best = run = 0
    prev = None
    for d in sorted(days):
        run = run + 1 if prev is not None and d - prev == timedelta(days=1) else 1
        best = max(best, run)
        prev = d
    return best


def main(paths):
    if not paths:
        print(__doc__)
        return 1
    streak3 = 0
    for p in paths:
        days = load_days(p)
        streak = longest_streak(days)
        total_min = sum(days.values()) / 60
        streak3 += streak >= 3
        print(f"{Path(p).stem}: 접속 {len(days)}일, 최장 연속 {streak}일, 총 {total_min:.0f}분")
    print(f"3일 연속 접속: {streak3}/{len(paths)}명")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
