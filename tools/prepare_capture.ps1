# 화면 캡처가 가능한 상태인지 확인하고, 화면 보호기가 떠 있으면 끈다.
#
# 왜 필요한가 (2026-09-17 확인):
#   이미지 세션은 "유휴 300초 이상"일 때만 움직이도록 해 뒀는데, 이 PC의 화면 보호기는
#   600초에 뜬다. 그래서 세션이 화면을 보려는 시점엔 이미 화면 보호기가 떠 있다.
#   화면 보호기는 자기 전용 데스크톱(Screen-saver)에서 돌기 때문에, 기본 데스크톱에 붙어 있는
#   캡처 도구는 "액세스가 거부되었습니다"로 막힌다. 나흘 내내 0장이었던 진짜 이유가 이것이다.
#   (밤 세션들은 Mountain Base Camp·XBOX·NVIDIA 오버레이를 의심했는데 그건 아니다 —
#    그 창들이 다 떠 있는 상태에서도 사람이 PC 앞에 있으면 캡처가 된다.)
#
# 마우스를 움직이거나 키를 누르지 않는다. 화면 보호기 프로세스만 끝낸다.
# Tifania가 PC를 쓰는 중에 커서가 튀거나 창이 눌리는 일이 없게 하기 위해서다.
#
# 출력 마지막 줄이 판정이다:
#   CAPTURE-OK     캡처해도 된다
#   CAPTURE-BLOCKED 막혀 있다. 뒤에 이유가 붙는다. 세션은 여기서 멈추고 daily에 그 줄을 적는다

Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Text;
public class Cap {
  [DllImport("user32.dll", SetLastError=true)]
  public static extern bool SystemParametersInfo(uint a, uint b, ref bool c, uint d);
  [DllImport("user32.dll", SetLastError=true)]
  public static extern IntPtr OpenInputDesktop(uint flags, bool inherit, uint access);
  [DllImport("user32.dll", SetLastError=true)]
  public static extern bool CloseDesktop(IntPtr h);
  [DllImport("user32.dll", SetLastError=true)]
  public static extern bool GetUserObjectInformation(IntPtr h, int index, StringBuilder p, uint n, out uint need);
}
"@ -ErrorAction SilentlyContinue

function Get-InputDesktopName {
    $d = [Cap]::OpenInputDesktop(0, $false, 0x0001)   # DESKTOP_READOBJECTS
    if ($d -eq [IntPtr]::Zero) { return $null }       # 열 수 없으면 대개 잠금 화면(Winlogon)이다
    $sb = New-Object System.Text.StringBuilder 256
    $need = 0
    [void][Cap]::GetUserObjectInformation($d, 2, $sb, 256, [ref]$need)   # UOI_NAME
    [void][Cap]::CloseDesktop($d)
    return $sb.ToString()
}

function Test-ScreenSaverRunning {
    $r = $false
    [void][Cap]::SystemParametersInfo(0x0072, 0, [ref]$r, 0)   # SPI_GETSCREENSAVERRUNNING
    return $r
}

"입력 데스크톱: " + (Get-InputDesktopName)
"화면 보호기 실행 중: " + (Test-ScreenSaverRunning)

# .scr 프로세스는 보이면 무조건 끝낸다 — SPI 플래그를 믿지 않는다.
# 화면 보호기를 손으로 띄운 경우엔 프로세스는 살아 있는데 플래그가 False로 나온다(직접 확인).
$scr = Get-Process | Where-Object {
    try { $_.Path -and $_.Path.ToLower().EndsWith('.scr') } catch { $false }
}
if ($scr) {
    "화면 보호기를 끈다 (마우스·키 입력 없이 프로세스만 종료)"
    foreach ($proc in $scr) {
        "  종료: $($proc.ProcessName) (pid $($proc.Id))"
        try { $proc.Kill() } catch { "  종료 실패: $($_.Exception.Message)" }
    }
    Start-Sleep -Milliseconds 800
}

$desk = Get-InputDesktopName
$ss   = Test-ScreenSaverRunning

if ($null -eq $desk) {
    "CAPTURE-BLOCKED 입력 데스크톱을 열 수 없다 — 화면이 잠겨 있다(Winlogon). 세션이 풀 수 없다."
} elseif ($desk -ne 'Default') {
    "CAPTURE-BLOCKED 입력 데스크톱이 '$desk' 다 — 기본 데스크톱이 아니라 캡처가 막힌다."
} elseif ($ss) {
    "CAPTURE-BLOCKED 화면 보호기가 아직 살아 있다 — 종료에 실패했다."
} else {
    "CAPTURE-OK"
}
