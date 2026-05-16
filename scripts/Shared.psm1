# SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
# SPDX-License-Identifier: GPL-3.0-or-later
Set-StrictMode -Version Latest

function Write-Step([string]$msg) {
    Write-Host "`n==> $msg" -ForegroundColor Cyan
}

# Returns hashtable: FileVersion, Version (e.g. "0.1.0-beta.1")
function Get-BuildVersions([string]$RepoRoot) {
    $xml             = [xml](Get-Content "$RepoRoot\Directory.Build.props")
    $fileVersionNode = $xml.SelectSingleNode('//FileVersion')
    $prefixNode      = $xml.SelectSingleNode('//VersionPrefix')
    $suffixNode      = $xml.SelectSingleNode('//VersionSuffix')
    if (-not $fileVersionNode) { throw "FileVersion not found in Directory.Build.props" }
    $fileVersion = $fileVersionNode.InnerText
    $prefix      = if ($prefixNode) { $prefixNode.InnerText } else { '' }
    $suffix      = if ($suffixNode) { $suffixNode.InnerText } else { '' }
    $version     = if ($suffix) { "$prefix-$suffix" } else { $prefix }
    return @{ FileVersion = $fileVersion; Version = $version }
}

function Invoke-Build([string]$RepoRoot, [string]$Configuration) {
    $sln = Get-ChildItem -Path $RepoRoot -Filter '*.sln' | Select-Object -First 1 -ExpandProperty FullName
    & dotnet restore $sln --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }
    & dotnet build $sln --no-restore -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
    Write-Host "  Build OK."
}

function Get-PluginVersion([string]$PluginDir) {
    $dllPath = Join-Path $PluginDir 'FluentPassFinderPlugin.dll'
    if (-not (Test-Path $dllPath)) { throw "Plugin DLL not found: $dllPath" }
    return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath).ProductVersion
}

function Invoke-GenerateLicenseNotices {
    param(
        [string]$RepoRoot,
        [string]$OutputFile
    )
    $toolList = & dotnet tool list --global 2>&1
    if (-not ($toolList | Select-String 'nuget-license')) {
        Write-Host "  Installing nuget-license..."
        & dotnet tool install --global nuget-license --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "Failed to install nuget-license" }
    }

    $slnPath = Get-ChildItem -Path $RepoRoot -Filter '*.sln' | Select-Object -First 1 -ExpandProperty FullName
    Write-Host "  Restoring NuGet packages..."
    & dotnet restore $slnPath --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed (exit $LASTEXITCODE)" }

    $projects = Get-ChildItem -Path $RepoRoot -Filter '*.csproj' -Recurse |
                Where-Object { $_.FullName -notlike '*\obj\*' } |
                Select-Object -ExpandProperty FullName

    $result   = [System.Collections.Generic.List[string]]::new()
    $seenPkgs = @{}

    foreach ($proj in $projects) {
        $projName  = [IO.Path]::GetFileNameWithoutExtension($proj)
        $lines     = & nuget-license -i $proj --include-transitive
        if ($LASTEXITCODE -ne 0) { throw "nuget-license failed (exit $LASTEXITCODE) for $(Split-Path $proj -Leaf)" }
        $dataLines = @($lines | Where-Object { $_ })

        $result.Add('')
        $result.Add("# $projName")
        if ($dataLines.Count -eq 0) {
            $result.Add('  (no third-party packages)')
        } else {
            foreach ($line in $dataLines) { $result.Add([string]$line) }
        }

        $jsonLines = & nuget-license -i $proj --include-transitive -o Json
        if ($LASTEXITCODE -ne 0) { throw "nuget-license JSON pass failed for $(Split-Path $proj -Leaf)" }
        ($jsonLines | ConvertFrom-Json) | ForEach-Object {
            $key = "$($_.PackageId)_$($_.PackageVersion)"
            if (-not $seenPkgs.ContainsKey($key)) { $seenPkgs[$key] = $_ }
        }
    }

    $byLicense = $seenPkgs.Values |
                 Where-Object { $_.License } |
                 Group-Object License |
                 Sort-Object Name

    $result.Add('')
    $result.Add('---')
    $result.Add('# License Texts')

    foreach ($group in $byLicense) {
        $expression = $group.Name
        $pkgs       = $group.Group | Sort-Object PackageId

        $result.Add('')
        $result.Add("## $expression")
        $result.Add('')
        $result.Add('Packages:')
        foreach ($pkg in $pkgs) { $result.Add("  $($pkg.PackageId) $($pkg.PackageVersion)") }
        $result.Add('')

        $spdxUrl = "https://raw.githubusercontent.com/spdx/license-list-data/main/text/$expression.txt"
        try {
            $licText = (Invoke-WebRequest -Uri $spdxUrl -UseBasicParsing -ErrorAction Stop).Content
            foreach ($line in ($licText -split '\r?\n')) { $result.Add($line) }
        } catch {
            $result.Add("[License text unavailable. See: $spdxUrl]")
        }
    }

    $result | Set-Content -Path $OutputFile -Encoding utf8
    Write-Host "  Generated: $(Split-Path $OutputFile -Leaf)"
}

function Invoke-ILRepack {
    param(
        [string]$Dir,
        [string]$Primary,
        [string[]]$ExcludePatterns = @()
    )

    $toolList = & dotnet tool list --global 2>&1
    if (-not ($toolList | Select-String 'dotnet-ilrepack')) {
        Write-Host "  Installing dotnet-ilrepack..."
        & dotnet tool install --global dotnet-ilrepack --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "Failed to install dotnet-ilrepack" }
    }

    $primaryPath = Join-Path $Dir $Primary
    if (-not (Test-Path $primaryPath)) { throw "Primary assembly not found: $primaryPath" }

    # Sort: third-party packages first, then FluentPassFinder* - ensures dependencies
    # are already loaded by the time ILRepack processes the main assemblies.
    # Skip native DLLs by attempting to read their assembly name; native DLLs throw
    # BadImageFormatException and are excluded regardless of their filename.
    $secondaryDlls = @(Get-ChildItem $Dir -Filter '*.dll' |
                       Where-Object {
                           if ($_.Name -eq $Primary) { return $false }
                           foreach ($pattern in $ExcludePatterns) {
                               if ($_.Name -like $pattern) { return $false }
                           }
                           try { $null = [Reflection.AssemblyName]::GetAssemblyName($_.FullName); $true }
                           catch { $false }
                       } |
                       Sort-Object { if ($_.Name -like 'FluentPassFinder*') { 1 } else { 0 } }, Name |
                       Select-Object -ExpandProperty FullName)

    if ($secondaryDlls.Count -eq 0) {
        Write-Host "  No secondary DLLs found; skipping merge."
        return
    }

    $ext        = [IO.Path]::GetExtension($Primary)
    $baseName   = [IO.Path]::GetFileNameWithoutExtension($Primary)
    $mergedPath = Join-Path $Dir "${baseName}_merged${ext}"
    $repackArgs = @("/out:$mergedPath") + $primaryPath + $secondaryDlls

    & ilrepack @repackArgs
    if ($LASTEXITCODE -ne 0) { throw "ILRepack failed with exit code $LASTEXITCODE" }

    Move-Item $mergedPath $primaryPath -Force

    $mergedPdb = [IO.Path]::ChangeExtension($mergedPath, '.pdb')
    if (Test-Path $mergedPdb) {
        Move-Item $mergedPdb ([IO.Path]::ChangeExtension($primaryPath, '.pdb')) -Force
    }

    foreach ($dll in $secondaryDlls) {
        Remove-Item $dll -Force
        $pdb = [IO.Path]::ChangeExtension($dll, '.pdb')
        if (Test-Path $pdb) { Remove-Item $pdb -Force }
    }

    Write-Host "  Merged $($secondaryDlls.Count + 1) assemblies into $Primary"
}

Export-ModuleMember -Function Write-Step, Get-BuildVersions, Invoke-Build, Get-PluginVersion, Invoke-GenerateLicenseNotices, Invoke-ILRepack
