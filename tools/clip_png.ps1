# 클립보드에 있는 그림을 PNG 파일로 그대로 떨군다 (2026-09-23 20시 이미지 세션에서 추가).
#
# 왜 필요한가. ChatGPT 데스크탑 앱에서 그림을 받는 길이 두 개인데 둘 다 걸릴 때가 있다:
#   - 오른쪽 클릭 → "사본 다운로드": 저장 대화상자를 거쳐야 해서 느리다.
#   - 라이트박스 → "Save Image As...": 앱이 `getOwnerBrowserWindow`로 터지는 버그가 있다.
# 그런데 같은 메뉴의 "이미지 복사"(라이트박스에서는 "Copy Image")는 한 번도 안 걸렸고,
# Chromium이 클립보드에 **PNG 원본 바이트**를 그대로 올려 준다. 그걸 파일로 쓰면 된다.
#
# 중요: System.Drawing.Bitmap/DIB 경로로 받으면 알파가 날아가서 check_alpha.py가
# "알파 채널이 없다"로 실패한다. 반드시 "PNG" 형식을 집어서 바이트를 그대로 써야 한다.
# 그러면 "사본 다운로드"로 받은 것과 똑같은 1254x1254 RGBA 파일이 나온다.
#
# 쓰는 법 (-STA가 없으면 클립보드를 못 읽는다):
#   powershell -STA -ExecutionPolicy Bypass -File "E:\Unity\PlanetRacer\tools\clip_png.ps1" `
#     -Out "E:\Unity\PlanetRacer\PlanetRacer\Assets\Screenshots\cand01.png"
param([string]$Out)
Add-Type -AssemblyName System.Windows.Forms
$fmts = [System.Windows.Forms.Clipboard]::GetDataObject().GetFormats()
Write-Output ("FORMATS=" + ($fmts -join ","))
$d = [System.Windows.Forms.Clipboard]::GetData("PNG")
if ($null -eq $d) { Write-Output "NO-PNG-FORMAT"; exit 1 }
$ms = [System.IO.MemoryStream]$d
$bytes = $ms.ToArray()
[System.IO.File]::WriteAllBytes($Out, $bytes)
Write-Output ("WROTE=" + $bytes.Length)
