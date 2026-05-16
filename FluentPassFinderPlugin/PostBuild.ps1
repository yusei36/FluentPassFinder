param([string] $OutputSrcDir, [string] $AppBinDir, [string] $SolutionDir, [string] $Configuration)

$buildPath = "$SolutionDir/build/KeePass/Plugins"
$pluginOutputPath = "$buildPath/FluentPassFinder"
if ((Test-Path -Path $pluginOutputPath )) {
    Remove-Item -Recurse $pluginOutputPath
}

New-Item -ItemType directory -Name FluentPassFinder -Path $buildPath
New-Item -ItemType directory -Name bin -Path $pluginOutputPath

Copy-Item $OutputSrcDir/*.dll -Exclude KeePass* -Destination $pluginOutputPath
Copy-Item $AppBinDir/*.dll -Exclude KeePass* -Destination $pluginOutputPath/bin/

# Copy native runtime assets (e.g. libSkiaSharp.dll for Avalonia/SkiaSharp)
$nativeAssetsDir = "$AppBinDir/runtimes/win-x64/native"
if (Test-Path $nativeAssetsDir) {
    Copy-Item "$nativeAssetsDir/*.dll" -Destination $pluginOutputPath/bin/
}

if ($Configuration -eq "Debug") {
    Copy-Item $OutputSrcDir/*.pdb -Destination $pluginOutputPath
    Copy-Item $AppBinDir/FluentPassFinder* -Destination $pluginOutputPath/bin/
}
else {
    Copy-Item $AppBinDir/FluentPassFinder* -Exclude *.pdb -Destination $pluginOutputPath/bin/
}