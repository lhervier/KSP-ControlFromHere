@echo off
setlocal

echo =====================================
echo Removing existing Mod folder
echo =====================================

if exist "%KSPDIR%\GameData\ControlFromHereMod" rmdir /s /q "%KSPDIR%\GameData\ControlFromHereMod"

echo.
echo =====================================
echo Unzipping Mod
echo =====================================

powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path 'Release\ControlFromHereMod.zip' -DestinationPath '%KSPDIR%\GameData\ControlFromHereMod' -Force"
if errorlevel 1 (
    echo ERROR: Failed to unzip the Mod
    exit /b 1
)

echo.
echo Mod installed
echo.
echo Run at: %date% %time%
