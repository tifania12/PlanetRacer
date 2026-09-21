$repo = "E:\Unity\PlanetRacer\.git"
Get-ChildItem $repo -Recurse -Force -Filter "*.lock" | ForEach-Object {
  Write-Output ("LOCK: " + $_.FullName)
  Remove-Item -LiteralPath $_.FullName -Force -ErrorAction SilentlyContinue
}
Get-ChildItem (Join-Path $repo "objects") -Recurse -Force -Filter "tmp_obj_*" -ErrorAction SilentlyContinue | ForEach-Object {
  Remove-Item -LiteralPath $_.FullName -Force -ErrorAction SilentlyContinue
}
Write-Output "CLEARED"
