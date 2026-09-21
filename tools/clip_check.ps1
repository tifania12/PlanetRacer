$c = Get-Clipboard -Raw
if ($null -eq $c) { Write-Output "CLIP=NULL"; exit }
Write-Output ("LEN=" + $c.Length)
$n = [Math]::Min(30, $c.Length)
Write-Output ("HEAD=" + $c.Substring(0,$n))
