@echo off

::Project UppercuT - http://uppercut.googlecode.com

if '%2' NEQ '' goto usage
if '%3' NEQ '' goto usage
if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

SET APP_BDDDOC="..\lib\bdddoc\bdddoc.console.exe"
SET TEST_ASSEMBLY_NAME="roundhouse.tests.dll"

SET DIR=%~d0%~p0%

SET build.config.settings="%DIR%Settings\UppercuT.config"
"%DIR%lib\Nant\nant.exe" /f:.\build\compile.step -D:build.config.settings=%build.config.settings%

if %ERRORLEVEL% NEQ 0 goto errors

"%DIR%lib\Nant\nant.exe" /f:.\build\analyzers\test.step %1 -D:build.config.settings=%build.config.settings%
"%DIR%lib\Nant\nant.exe" /f:.\build.custom\bdddoc.build  -D:build.config.settings=%build.config.settings% -D:app.bdddoc=%APP_BDDDOC% -D:test_assembly=%TEST_ASSEMBLY_NAME%
"%DIR%lib\Nant\nant.exe" /f:.\build.custom\bdddoc.build open_results

if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:usage
echo.
echo Usage: bdddoc.bat
echo.
goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish