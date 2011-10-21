param($installPath, $toolsPath, $package)

$modules = Get-ChildItem $ToolsPath -Filter *.psm1
$modules | ForEach-Object { import-module -name  $_.FullName }

@"
========================
NuGet Package Uninstaller - global package uninstaller
========================
Please run 'Uninstall-PackageAll packageName' to remove packages from the entire solution.
========================
"@ | Write-Host