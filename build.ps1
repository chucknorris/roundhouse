#!/usr/bin/env pwsh

$MSBUILD=msbuild

$root = $PSScriptRoot;

$CODEDROP="$($root)/code_drop";
$PACKAGEDIR="$($CODEDROP)/packages";
$LOGDIR="$($CODEDROP)/log";

$TESTOUTDIR="$($root)/product/roundhouse.tests/bin"

$onAppVeyor = $("$($env:APPVEYOR)" -eq "True");

Push-Location $root


"`n"
" * Generating version number"
$gitVersion = (gitversion | ConvertFrom-Json)

If ($onAppVeyor) {
    $newVersion="$($gitVersion.FullSemVer)"
    Write-host "   - Updating appveyor build version to: $newVersion"
    $env:APPVEYOR_BUILD_VERSION="$newVersion"
    appveyor UpdateBuild -Version "$newVersion"
}

" * Updating NuGet to handle newer license metadata"
nuget update -self -Verbosity quiet

" * Restoring nuget packages"
nuget restore -NonInteractive -Verbosity quiet

# Create output and log dirs if they don't exist (don't know why this is necessary - works on my box...)
If (!(Test-Path $PACKAGEDIR)) {
    $null = mkdir $PACKAGEDIR;
}
If (!(Test-Path $LOGDIR)) {
    $null = mkdir $LOGDIR;
}

" * Extracting keywords.txt so that MySql works after ILMerge"

$file = $(Get-ChildItem -Recurse -Include MySql.Data.dll ~/.nuget/packages/mysql.data/ | Select-Object -Last 1)
& "$root/build/Extract-Resource.ps1" -File $file -ResourceName MySql.Data.keywords.txt -OutFile generated/MySql.Data/keywords.txt


" * Building and packaging"
msbuild /t:"Build" /p:DropFolder=$CODEDROP /p:Version="$($gitVersion.FullSemVer)" /p:NoPackageAnalysis=true /nologo /v:q /fl /flp:"LogFile=$LOGDIR/msbuild.log;Verbosity=n" /p:Configuration=Build /p:Platform="Any CPU"

"    - NuGet libraries"
dotnet pack -nologo --no-build -v q -p:Version="$($gitVersion.FullSemVer)" -p:NoPackageAnalysis=true -p:Configuration=Build -p:Platform="Any CPU" -o ${PACKAGEDIR}


"    - net461 command-line nuget package"

nuget pack product/roundhouse.console/roundhouse.nuspec -OutputDirectory "$CODEDROP/packages" -Verbosity quiet -NoPackageAnalysis -Version "$($gitVersion.FullSemVer)" 
msbuild /t:"Pack" product/roundhouse.tasks/roundhouse.tasks.csproj  /p:DropFolder=$CODEDROP /p:Version="$($gitVersion.FullSemVer)" /p:NoPackageAnalysis=true /nologo /v:q /fl /flp:"LogFile=$LOGDIR/msbuild.roundhouse.tasks.pack.log;Verbosity=n" /p:Configuration=Build /p:Platform="Any CPU"

"    - netcoreapp2.1 global tool dotnet-roundhouse"

dotnet publish -v q -nologo --no-restore product/roundhouse.console -p:NoPackageAnalysis=true -p:TargetFramework=netcoreapp2.1 -p:Version="$($gitVersion.FullSemVer)" -p:Configuration=Build -p:Platform="Any CPU"
dotnet pack -v q -nologo --no-restore --no-build product/roundhouse.console -p:NoPackageAnalysis=true -p:TargetFramework=netcoreapp2.1 -o ${PACKAGEDIR} -p:Version="$($gitVersion.FullSemVer)" -p:Configuration=Build -p:Platform="Any CPU"


# " * Packaging netcoreapp2.1 global tool dotnet-roundhouse`n"

# nuget pack -Verbosity quiet -outputdirectory ${PACKAGEDIR} .\product\roundhouse.console\roundhouse.tool.nuspec -Properties "Version=$($gitVersion.FullSemVer);NoPackageAnalysis=true"

# AppVeyor runs the test automagically, no need to run explicitly with nunit-console.exe. 
# But we want to run the tests on localhost too.
If (! $onAppVeyor) {

    "`n * Running unit tests`n"

    # Find test projects
    $testAssemblies = $(dir -r -i *.tests.dll)

    $testAssemblies | ? { $_.FullName -NotLike "*obj*" } | % {
        dotnet vstest $_
    }
}

