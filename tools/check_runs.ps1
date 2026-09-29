# GitHub Actions 실행 결과를 API로 본다 (2026-09-29). 목록 페이지 아이콘은 취소된 실행이
# 실패처럼 보여서 세 번 잘못 읽었다(CLAUDE.md). 저장소가 공개라 토큰 없이 읽힌다.
#
#   powershell -NoProfile -File tools\check_runs.ps1 [브랜치] [개수]
param([string]$Branch = "claude/dev", [int]$Count = 6)

$url = "https://api.github.com/repos/tifania12/PlanetRacer/actions/runs?branch=$Branch&per_page=$Count"
$runs = (Invoke-RestMethod $url).workflow_runs
foreach ($r in $runs) {
    $sha = $r.head_sha.Substring(0, 7)
    Write-Output ("{0} {1} {2} {3} {4}" -f $r.run_number, $sha, $r.status, $r.conclusion, $r.created_at)
}
