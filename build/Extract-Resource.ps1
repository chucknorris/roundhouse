#!/usr/bin/env pwsh

# Extracts an embedded resource from a dll and stores it to a file.

Param(
    [Parameter(Mandatory=$true)] $File, 
    [Parameter(Mandatory=$true)] $ResourceName, 
    [Parameter(Mandatory=$false)] $OutFile = "")

$ErrorActionPreference = "Stop";


If (! (Test-Path $File)) {
    Write-Error "File not found: $File";
}

$f = Get-Item $File

If ($OutFile -ne "") {
    $OutputFile = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutFile)
} Else {
    $outPath = Get-Item . 
    $OutputFile = Join-Path $outPath $ResourceName;
}

# Make parent dir
If (! (Test-Path $OutputFile)) {
    $null = New-Item -ItemType Directory -Path $OutputFile;
    $null = Remove-Item $OutputFile;
}

#Powershell and powershell Core have different ways of doing this, so safeguard.
$IsWin = ($IsWindows -or $env:OS -eq "Windows_NT")
If ($IsWin) {
    [System.Reflection.Assembly] $assembly = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($f);
} Else {
    [System.Reflection.Assembly] $assembly = [System.Reflection.Assembly]::LoadFile($f);
}


$stream = $assembly.GetManifestResourceStream($ResourceName);
If ($stream.CanSeek) {
    $size = [System.Convert]::ToInt32($stream.Length)
} Else {
    $size = 0;
}

$ms = New-Object System.IO.MemoryStream $size

[byte[]] $buffer = New-Object byte[] 2048 ;

do {
    $len = $stream.Read($buffer, 0, 2048)
    $ms.Write($buffer, 0, $len)
    
} until ($len -eq 0)

$stream.Dispose();

$fs = [System.IO.File]::OpenWrite($OutputFile)
$ms.WriteTo($fs);
$fs.Flush();
$fs.Close();
$fs.Dispose();

$ms.Dispose();


