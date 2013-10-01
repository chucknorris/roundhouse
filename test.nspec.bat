@echo off

SET DIR=%cd%
SET BUILD_DIR=%~d0%~p0%
SET NANT="%BUILD_DIR%lib\Nant\nant.exe"
SET build.config.settings="%DIR%\settings\UppercuT.config"

%NANT% /f:"%BUILD_DIR%build\compile.step" -D:build.config.settings=%build.config.settings%

if %ERRORLEVEL% NEQ 0 goto errors

.\packages\nspec.0.9.67\tools\NSpecRunner.exe build_output\RoundhousE\roundhouse.tests.integration.dll

if %ERRORLEVEL% NEQ 0 goto errors

goto finish


:errors
EXIT /B %ERRORLEVEL%

:finish
