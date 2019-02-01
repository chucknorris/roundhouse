#!/usr/bin/env pwsh

$Root="$($PSScriptRoot)/..";

& "$Root/code_drop/merge/rh.exe" --dt postgres --cs "host=localhost; port=5432; database=db; username=postgres; password=monkeybusiness;"
