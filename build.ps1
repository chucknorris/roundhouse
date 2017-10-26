#!/usr/bin/env powershell

$MSBUILD=msbuild

$root = $PSScriptRoot;

$CODEDROP="$($root)\code_drop";
$LOGDIR="$($CODEDROP)\log";

$TESTOUTDIR="$($root)\product\roundhouse.tests\bin"

$onAppVeyor = $("$($env:APPVEYOR)" -eq "True");

pushd $root


"`n * Generating version number"
$gitVersion = (GitVersion | ConvertFrom-Json)

If ($onAppVeyor) {
    $newVersion="$($gitVersion.FullSemVer).$env:APPVEYOR_BUILD_NUMBER"
    Write-host "   - Updating appveyor build version to: $newVersion"
    $env:APPVEYOR_BUILD_VERSION="$newVersion"
    appveyor UpdateBuild -Version "$newVersion"
}

"`n * Restoring nuget packages"
nuget restore -NonInteractive -Verbosity quiet

# Create output and log dirs if they don't exist (don't know why this is necessary - works on my box...)
If (!(Test-Path $CODEDROP)) {
    $null = mkdir $CODEDROP;
}
If (!(Test-Path $LOGDIR)) {
    $null = mkdir $LOGDIR;
}


"`n * Building and packaging"
msbuild /t:"Build;Pack" /p:DropFolder=$CODEDROP /p:Version="$($gitVersion.FullSemVer)" /p:NoPackageAnalysis=true /nologo /v:m /fl /flp:"LogFile=$LOGDIR\msbuild.log;Verbosity=n" /p:Configuration=Build /p:Platform="Any CPU"

# Workaround until test filter is updated - remove then.
If ($onAppVeyor) {
    dir -r product/roundhouse.tests.integration -i roundhouse.tests.integration.dll | % { 
        rm -fo $_;
    }
}

# AppVeyor runs the test automagically, no need to run explicitly with nunit-console.exe. 
# But we want to run the tests on localhost too.
If (! $onAppVeyor) {

    # Find nunit3-console dynamically
    "`n * Looking for nunit3-console.exe"

    $nugetRoot = $env:NUGET_PACKAGES;
    If ("$($nugetRoot)" -eq "") {
        $nugetRoot = "~/.nuget"
    }

    $nunit = $(dir -r "$($nugetRoot)/packages/nunit*" -i nunit3-console.exe | Select-Object -last 1)

    "    - Found at $($nunit)"

    "`n * Running unit tests`n"
    $tests =  $(dir -r "$($TESTOUTDIR)" -i *.tests.dll);
    & $nunit --noheader --noresult --output "$($LOGDIR)/nunit.log" --err="$($LOGDIR)/nunit.errlog" $tests
}

popd
