$MSBUILD=msbuild

$root = $PSScriptRoot;

$CODEDROP="$($root)\code_drop";
$LOGDIR="$($CODEDROP)\log";

$TESTOUTDIR="$($root)\build_output\RoundhousE.UnitTests\net461"

pushd $root


"`n * Generating version number"

# Might use nuget to get the GitVersion.dll, and use it from PowerShell, like this:
#
#nuget install gitversion -out "$($env:TEMP)" -Verbosity quiet
#$gitVersionDll = $(dir -r $env:TEMP\gitversion.* -i GitVersionCore.dll | Select-Object -last 1)
#Add-Type -Path "$($gitVersionDll)";
#
# or, simpler, with chocolatey: 
#choco inst gitversion.portable

$gitVersion = (GitVersion | ConvertFrom-Json)

"`n * Restoring nuget packages"
nuget restore -NonInteractive -Verbosity quiet

#"`n * Building"
#msbuild /nologo /v:q /fl /flp:"LogFile=$LOGDIR\msbuild.log;Verbosity=m" /p:Configuration=Build /p:Platform="Any CPU"

"`n * Building and packaging"
msbuild /t:Pack /p:DropFolder=$CODEDROP /p:Version="$($gitVersion.FullSemVer)" /p:NoPackageAnalysis=true /nologo /v:q /fl /flp:"LogFile=$LOGDIR\msbuild-nuget.log;Verbosity=m" /p:Configuration=Build /p:Platform="Any CPU"

# Find nunit3-console dynamically
"`n * Looking for nunit3-console.exe"
$nunit = $(dir -r $env:HOME\.nuget\packages\nunit* -i nunit3-console.exe | Select-Object -last 1)

"    - Found at $($nunit)"

"`n * Running unit tests`n"
$tests =  $(dir "$($TESTOUTDIR)\*.tests.dll");
 & $nunit --noheader --noresult --output "$($LOGDIR)\nunit.log" --err="$($LOGDIR)\nunit.errlog" $tests

#"`n * Packaging"
#msbuild /t:Pack /p:Version="$($gitVersion.FullSemVer)" /p:NoPackageAnalysis=true /p:PackageOutputDir=$CODEDROP /nologo /v:q /fl /flp:"LogFile=$LOGDIR\msbuild-nuget.log;Verbosity=m" /p:Configuration=Build /p:Platform="Any CPU"

popd