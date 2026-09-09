param(
    [string]$SourceDir = "D:\SRC\SRC\ReadAloudPlus",
    [string]$OutputDir = "D:\SRC\SRC\ReadAloudPlus\dist"
)

if (-not (Test-Path $OutputDir)) {
    [System.IO.Directory]::CreateDirectory($OutputDir) | Out-Null
}

$manifestPath = Join-Path $SourceDir "manifest.json"
$manifestJson = Get-Content $manifestPath -Raw | ConvertFrom-Json
$version = $manifestJson.version
$zipName = "ReadAloudPlus-v$version.zip"
$zipPath = Join-Path $OutputDir $zipName

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

$tempStaging = Join-Path $OutputDir "staging"
if (Test-Path $tempStaging) {
    Remove-Item -Recurse -Force $tempStaging
}
[System.IO.Directory]::CreateDirectory($tempStaging) | Out-Null

$includePaths = @(
    "manifest.json",
    "background",
    "content",
    "options",
    "popup",
    "shared",
    "icons"
)

foreach ($item in $includePaths) {
    $src = Join-Path $SourceDir $item
    $dest = Join-Path $tempStaging $item
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dest -Recurse -Force
    }
}

Compress-Archive -Path "$tempStaging\*" -DestinationPath $zipPath -Force
Remove-Item -Recurse -Force $tempStaging

Write-Output "Successfully packaged extension to: $zipPath"
$fileInfo = Get-Item $zipPath
Write-Output "Archive size: $([Math]::Round($fileInfo.Length / 1KB, 2)) KB"
