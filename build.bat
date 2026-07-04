@echo off
REM Thin wrapper: delegates to the shared generic build in KSP-Shared\tools.
setlocal
cd /d "%~dp0"
set "MOD_NAME=ControlFromHereMod"
set "MOD_SLN=ControlFromHere.sln"
call "KSP-Shared\tools\build.bat"
exit /b %errorlevel%
