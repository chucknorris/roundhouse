@echo off

::Project UppercuT - http://projectuppercut.org

if '%2' NEQ '' goto usage
if '%3' NEQ '' goto usage
if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

SET DIR=%~d0%~p0%
SET NANT="%DIR%lib\Nant\nant.exe"
SET build.config.settings="%DIR%settings\UppercuT.config"

%NANT% %1 /f:.\build\default.build -D:build.config.settings=%build.config.settings%
if %ERRORLEVEL% NEQ 0 goto errors

SET dirs.merge.from="%DIR%code_drop\RoundhousE\ConsoleApp"
SET file.merge.name="rh.exe"
:: library, exe, winexe
SET merge.target.type="exe"
%NANT% %1 /f:.\build\ilmerge.build -D:build.config.settings=%build.config.settings% -D:dirs.merge.from=%dirs.merge.from% -D:file.merge.name=%file.merge.name% -D:merge.target.type=%merge.target.type%
if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:usage
echo.
echo Usage: build.bat
echo.
goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish