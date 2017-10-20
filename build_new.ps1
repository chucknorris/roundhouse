$MSBUILD=msbuild

$root = $PSScriptRoot;

$CODEDROP="$($root)\code_drop";
$MERGEDIR="$($CODEDROP)\merge";
$LOGDIR="$($CODEDROP)\log";
$ILMERGE="$($root)\lib\ILMerge\ILMerge.exe";

# $OUTDIR="$($root)\product\build_output\RoundhouseNew"
$OUTDIR="$($root)\build_output\RoundhousE\net461"
$TESTOUTDIR="$($root)\build_output\RoundhousE.UnitTests\net461"

pushd $root

"`n * Restoring nuget packages"
nuget restore -NonInteractive -Verbosity quiet

"`n * Building"
msbuild /nologo /v:q /fl /flp:"LogFile=$LOGDIR\msbuild.log;Verbosity=m" /p:Configuration=Build /p:Platform="Any CPU"

# Find nunit3-console dynamically 
"`n * Looking for nunit3-console.exe"
$nunit = $(dir -r $env:HOME\.nuget\packages\nunit* -i nunit3-console.exe | select -last 1)

"    - Found at $($nunit)"

"`n * Running unit tests`n"
$tests =  $(dir "$($TESTOUTDIR)\*.tests.dll");
 & $nunit --noheader --output "$($LOGDIR)\nunit.log" --err="$($LOGDIR)\nunit.errlog" $tests

pushd $OUTDIR

if (! (Test-Path $LOGDIR)) {
    mkdir $LOGDIR
}
if (! (Test-Path $MERGEDIR)) {
    mkdir $MERGEDIR
}

"`n * ILMerging"
& $ILMERGE /internalize:$($root)\build.custom\ilmerge.internalize.ignore.txt /target:exe /out:$MERGEDIR\rh.exe /log:$LOGDIR\ilmerge.log /ndebug /zeroPeKind /allowDup rh.exe $(dir *.dll)

popd
popd