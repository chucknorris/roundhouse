@echo off
SET DIR=%~dp0

SET DLL=%DIR%\rh.dll 
dotnet %DLL% %*