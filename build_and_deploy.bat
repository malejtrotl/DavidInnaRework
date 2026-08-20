@echo off
setlocal

set "PROJECT_DIR=%~dp0"
set "TARGET_DIR=C:\Program Files (x86)\Steam\steamapps\common\Breach Wanderers\BepInEx\plugins\DavidInnaRework"
set "DLL_NAME=DavidInnaRework.dll"
set "BUILD_OUTPUT=%PROJECT_DIR%bin\Debug\net6.0\%DLL_NAME%"

echo Building %PROJECT_DIR%...
dotnet build "%PROJECT_DIR%DavidInnaRework.csproj"
if errorlevel 1 (
    echo.
    echo Build FAILED. Not copying DLL.
    exit /b 1
)

echo.
echo Build succeeded. Copying DLL to game plugins folder...

if not exist "%TARGET_DIR%" (
    mkdir "%TARGET_DIR%"
)

copy /Y "%BUILD_OUTPUT%" "%TARGET_DIR%\%DLL_NAME%"
if errorlevel 1 (
    echo.
    echo Copy FAILED.
    exit /b 1
)

echo.
echo Deployed %DLL_NAME% to "%TARGET_DIR%".
exit /b 0
