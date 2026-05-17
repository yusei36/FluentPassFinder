# SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
# SPDX-License-Identifier: GPL-3.0-or-later
#Requires -Version 5.1
<#
.SYNOPSIS
    Builds and packages FluentPassFinder for distribution.

.DESCRIPTION
    1. Generates third-party license notices.
    2. Builds the plugin and WPF app.
    3. Merges plugin DLLs into FluentPassFinderPlugin.dll via ILRepack.
    4. Publishes FluentPassFinder as a single-file executable (dotnet publish).
    5. Produces a zip archive ready for distribution:
         FluentPassFinder-<version>.zip
           FluentPassFinderPlugin/        FluentPassFinderPlugin.dll (merged)
           FluentPassFinderPlugin/bin/    FluentPassFinder.exe (single-file) + native DLLs
           README.md
           LICENSE
           THIRD_PARTY_NOTICES.txt

.PARAMETER Configuration
    Build configuration: Debug or Release. Defaults to Release.

.PARAMETER SkipBuild
    Skip the dotnet build and publish steps; use if you already have build output.

.EXAMPLE
    .\Publish-Package.ps1
    .\Publish-Package.ps1 -Configuration Debug
    .\Publish-Package.ps1 -SkipBuild
#>
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module "$PSScriptRoot\Shared.psm1" -Force

$RepoRoot   = Split-Path $PSScriptRoot -Parent
$PluginDir  = "$RepoRoot\build\KeePass\Plugins\FluentPassFinder"
$OutputDir  = "$RepoRoot\build"

$versions = Get-BuildVersions $RepoRoot
Write-Host "FluentPassFinder $($versions.Version) ($Configuration)" -ForegroundColor White

# -- 1. Generate third-party license notices ------------------------------------
$noticesPath = "$OutputDir\THIRD_PARTY_NOTICES.txt"
New-Item $OutputDir -ItemType Directory -Force | Out-Null
Write-Step "Generating third-party license notices"
Invoke-GenerateLicenseNotices -RepoRoot $RepoRoot -OutputFile $noticesPath

# -- 2. Build -------------------------------------------------------------------
if (-not $SkipBuild) {
    Write-Step "Building"
    Invoke-Build -RepoRoot $RepoRoot -Configuration $Configuration
}

if (-not (Test-Path $PluginDir)) {
    throw "Plugin output directory not found: $PluginDir`nRun a build first or omit -SkipBuild."
}

# -- 3. Merge plugin DLLs with ILRepack ----------------------------------------
Write-Step "Merging plugin DLLs with ILRepack"
Invoke-ILRepack -Dir $PluginDir -Primary 'FluentPassFinderPlugin.dll'

# -- 4. Publish finder as single-file executable --------------------------------
# ILRepack cannot merge Avalonia assemblies (duplicate CompiledAvaloniaXaml.!XamlLoader
# types). Use dotnet publish -p:PublishSingleFile=true instead, which bundles all
# managed DLLs into the exe. Native DLLs (av_libglesv2, libSkiaSharp, etc.) remain
# alongside and cannot be bundled.
Write-Step "Publishing FluentPassFinder as single-file executable"
$finderBinDir = "$PluginDir\bin"
$finderProj   = "$RepoRoot\src\FluentPassFinder\FluentPassFinder.csproj"
Remove-Item $finderBinDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item $finderBinDir -ItemType Directory -Force | Out-Null
& dotnet publish $finderProj -c $Configuration -o $finderBinDir --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

# -- 5. Assemble zip ------------------------------------------------------------
$tags    = @()
if ($Configuration -eq 'Debug') { $tags += 'debug' }
$suffix  = if ($tags.Count -gt 0) { '-' + ($tags -join '-') } else { '' }
$zipName = "FluentPassFinder-$($versions.Version)$suffix.zip"
$zipPath = "$OutputDir\$zipName"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Step "Assembling release archive: $zipName"

$stagingDir = "$OutputDir\publish-staging"
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item $stagingDir -ItemType Directory | Out-Null

$stagingPluginDir = "$stagingDir\FluentPassFinderPlugin"
New-Item $stagingPluginDir -ItemType Directory | Out-Null
New-Item "$stagingPluginDir\bin" -ItemType Directory | Out-Null

# Copy merged plugin DLL (and PDB for Debug)
$pluginExtensions = if ($Configuration -eq 'Debug') { '.dll', '.pdb' } else { '.dll' }
Get-ChildItem $PluginDir -File | Where-Object { $_.Extension -in $pluginExtensions } |
    Copy-Item -Destination $stagingPluginDir

# Copy single-file finder exe + native DLLs (no PDB for Release)
$binExtensions = if ($Configuration -eq 'Debug') { '.exe', '.dll', '.pdb' } else { '.exe', '.dll' }
Get-ChildItem $finderBinDir -File | Where-Object { $_.Extension -in $binExtensions } |
    Copy-Item -Destination "$stagingPluginDir\bin\"

# Copy documentation
Copy-Item "$RepoRoot\README.md" "$stagingPluginDir\README.md"
Copy-Item "$RepoRoot\LICENSE"   "$stagingPluginDir\LICENSE"
Copy-Item $noticesPath          "$stagingPluginDir\THIRD_PARTY_NOTICES.txt"

Compress-Archive -Path "$stagingDir\*" -DestinationPath $zipPath
Remove-Item $stagingDir -Recurse -Force

# -- 6. Verify ------------------------------------------------------------------
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($zipPath)
Write-Host "Zip entries:"
$zip.Entries | ForEach-Object { Write-Host "  $($_.FullName)  ($([math]::Round($_.Length / 1KB, 1)) KB)" }
$zip.Dispose()

$hash    = (Get-FileHash $zipPath -Algorithm SHA256).Hash
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)

$productVersion = Get-PluginVersion -PluginDir $PluginDir
Write-Host "  Version:  $productVersion ($Configuration)" -ForegroundColor Green
Write-Host "  Archive:  $zipPath ($zipSize MB)" -ForegroundColor Green
Write-Host "  SHA256:   $hash" -ForegroundColor Green
