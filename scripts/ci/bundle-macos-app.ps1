#!/usr/bin/env pwsh
# Copyright 2021-2025 ModSync
# Licensed under the Business Source License 1.1 (BSL 1.1).
# See LICENSE.txt file in the project root for full license information.

param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDir,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$ProjectFile = "src/ModSync.GUI/ModSync.csproj",
    [string]$Framework = "net9.0",
    [string]$RuntimeIdentifier = "osx-arm64",
    [string]$Platform = "arm64",
    [string]$SourceInfoPlist = "Info.plist",
    [string]$IconSource = "src/ModSync.GUI/icon53.icns"
)

$ErrorActionPreference = "Stop"

$cleanVersion = $Version -replace '^v', ''
$publishPath = (Resolve-Path $PublishDir).Path
$appPath = Join-Path $publishPath "ModSync.app"

Write-Host "📦 Finalizing macOS app bundle at: $appPath"

if (-not (Test-Path $appPath)) {
    throw "ModSync.app was not produced under $publishPath"
}

if (-not (Test-Path $SourceInfoPlist)) {
    throw "Source Info.plist not found: $SourceInfoPlist"
}

$bundleInfoPlist = Join-Path $appPath "Contents/Info.plist"
Copy-Item $SourceInfoPlist $bundleInfoPlist -Force
Write-Host "✅ Installed bundle Info.plist from $SourceInfoPlist"

if (Test-Path $IconSource) {
    $resourcesDir = Join-Path $appPath "Contents/Resources"
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
    Copy-Item $IconSource (Join-Path $resourcesDir "icon53.icns") -Force
    Write-Host "✅ Installed bundle icon"
}

$plistContent = Get-Content $bundleInfoPlist -Raw
if ($plistContent -notmatch 'CFBundleURLTypes') {
    throw "Bundle Info.plist is missing CFBundleURLTypes"
}

if ($plistContent -notmatch '<string>nxm</string>') {
    throw "Bundle Info.plist is missing nxm URL scheme registration"
}

$executablePath = Join-Path $appPath "Contents/MacOS/ModSync"
if (-not (Test-Path $executablePath)) {
    throw "Bundle executable missing at $executablePath"
}

Write-Host "✅ macOS app bundle validated (version v$cleanVersion, nxm scheme present)"
