@echo off
setlocal

echo.
echo -------------------------------------------
echo Updating the KSP-Shared submodule
echo -------------------------------------------

git submodule update --remote --merge KSP-Shared
if errorlevel 1 (
    echo ERROR: Failed to update the KSP-Shared submodule
    exit /b 1
)

echo.
echo KSP-Shared submodule updated successfully
echo.
echo If the library changed, remember to commit the new pointer:
echo   git add KSP-Shared ^&^& git commit -m "Bump KSP-Shared"
