param(
    [Parameter(Mandatory=$true)][string]$GameDir,
    [Parameter(Mandatory=$true)][string]$UndertaleModCli
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Out = Join-Path $Root "build\UNDERTALE-Undercraft"
$Data = Join-Path $GameDir "data.win"

if (-not (Test-Path $Data)) { throw "data.win not found in $GameDir" }

python (Join-Path $Root "tools\verify_target.py") $Data
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (Test-Path $Out) { Remove-Item -Recurse -Force $Out }
New-Item -ItemType Directory -Force -Path $Out | Out-Null

foreach ($name in @("UNDERTALE.exe", "options.ini", "credits.txt")) {
    $source = Join-Path $GameDir $name
    if (Test-Path $source) { Copy-Item $source $Out }
}

& $UndertaleModCli load $Data -s (Join-Path $Root "mod\UndercraftBootstrap.csx") -o (Join-Path $Out "data.win") --overwrite
if ($LASTEXITCODE -ne 0) { throw "UndertaleModCli failed with exit code $LASTEXITCODE" }

Write-Host "Undercraft build created at: $Out"
