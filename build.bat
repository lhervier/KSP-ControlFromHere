@echo off
setlocal

echo =========
echo Building
echo =========

echo Removing Release folder
if exist Release rmdir /s /q Release

echo Creating Release folders
mkdir Release\ControlFromHereMod
mkdir Release\ControlFromHereMod\Textures
mkdir Release\ControlFromHereMod\Localization

echo Building Mod DLL
dotnet build ControlFromHere.sln
if errorlevel 1 (
    echo ERROR: Failed to build the Mod DLL
    exit /b 1
)

echo Copying Mod dll file
copy /y "Output\bin\ControlFromHereMod.dll" "Release\ControlFromHereMod"
if errorlevel 1 (
    echo ERROR: Failed to copy the Mod DLL
    exit /b 1
)

echo Copying icon file
copy /y "GameData\ControlFromHereMod\*.png" "Release\ControlFromHereMod"
if errorlevel 1 (
    echo ERROR: Failed to copy the icon file
    exit /b 1
)

echo Copying shared TMP sprite textures
copy /y "KSP-Shared\GameData\Textures\*" "Release\ControlFromHereMod\Textures"
if errorlevel 1 (
    echo ERROR: Failed to copy the shared textures
    exit /b 1
)

echo Copying localization files
copy /y "GameData\ControlFromHereMod\Localization\*" "Release\ControlFromHereMod\Localization"
if errorlevel 1 (
    echo ERROR: Failed to copy the localization files
    exit /b 1
)

echo Zipping Mod
powershell -Command "Compress-Archive -Path 'Release\ControlFromHereMod\*' -DestinationPath 'Release\ControlFromHereMod.zip' -Force"
if errorlevel 1 (
    echo ERROR: Failed to zip the Mod
    exit /b 1
)

echo Removing Mod folder
rmdir /s /q Release\ControlFromHereMod

echo Build Complete
echo.
echo Run at: %date% %time%
