param([string] $OutputSrcDir, [string] $SolutionDir)

$buildPath = "$SolutionDir/Build"
$pluginOutputPath = "$buildPath/FluentPassFinder"
if ((Test-Path -Path $pluginOutputPath )) {
    Remove-Item -Recurse $pluginOutputPath
}

New-Item -ItemType directory -Name FluentPassFinder -Path $buildPath
New-Item -ItemType directory -Name bin -Path $pluginOutputPath

Copy-Item $OutputSrcDir/*.dll -Exclude KeePass* -Destination $pluginOutputPath
Copy-Item $OutputSrcDir/bin/*.dll -Exclude KeePass* -Destination $pluginOutputPath/bin/
Copy-Item $OutputSrcDir/bin/FluentPassFinder* -Exclude *.pdb -Destination $pluginOutputPath/bin/