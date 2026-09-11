# GitHub Actions 빌드가 막혔을 때 PC에서 직접 WebGL을 빌드해 Cloudflare Pages에 올린다.
# 쓰는 법:  powershell -ExecutionPolicy Bypass -File tools\deploy_web.ps1
#
# 처음 한 번만: npm i -g wrangler  그리고  wrangler login

$ErrorActionPreference = "Stop"
$root    = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "PlanetRacer"
$out     = Join-Path $root "build\WebGL\PlanetRacer"
$unity   = "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"

if (-not (Test-Path $unity)) { throw "Unity를 못 찾았다: $unity" }

Write-Host "WebGL 빌드 시작. 처음이면 20분쯤 걸린다." -ForegroundColor Cyan
& $unity -batchmode -nographics -quit `
    -projectPath $project `
    -buildTarget WebGL `
    -executeMethod GemRacer.EditorTools.WebGLBuild.Build `
    -customBuildPath $out `
    -logFile (Join-Path $root "build\unity_webgl.log")
if ($LASTEXITCODE -ne 0) { throw "Unity 빌드 실패. build\unity_webgl.log를 확인할 것." }

Copy-Item (Join-Path $root "web\_headers") (Join-Path $out "_headers") -Force

# Cloudflare Pages는 파일 하나가 25MiB를 넘으면 거부한다
$big = Get-ChildItem $out -Recurse -File | Where-Object { $_.Length -gt 25MB }
if ($big) {
    $big | Format-Table Name, @{N="MB";E={[math]::Round($_.Length/1MB,1)}}
    throw "25MiB를 넘는 파일이 있다. 에셋을 줄이거나 R2로 옮겨야 한다."
}

Write-Host "Cloudflare Pages 업로드" -ForegroundColor Cyan
wrangler pages deploy $out --project-name=planetracer --branch=main
