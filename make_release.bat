@echo off
setlocal EnableDelayedExpansion

rem --- Paths ---
set "MOD_ROOT=%~dp0"
set "BUILD_DIR=%MOD_ROOT%build"
set "TMP_DIR=%BUILD_DIR%\tmp"
set "ZIP_PATH=%BUILD_DIR%\MagicShapeHelper.zip"
set "DLL_PATH=%MOD_ROOT%MagicShapeHelper.dll"
set "INFO_PATH=%MOD_ROOT%Info.json"

rem --- Prepare directories ---
echo [1/5] Preparing build folders...
if exist "%TMP_DIR%" rd /s /q "%TMP_DIR%"
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"
mkdir "%TMP_DIR%" >nul 2>&1

rem --- Check required files ---
if not exist "%DLL_PATH%" (
    echo [ERROR] Missing MagicShapeHelper.dll in mod root: %MOD_ROOT%
    exit /b 1
)
if not exist "%INFO_PATH%" (
    echo [ERROR] Missing Info.json in mod root: %MOD_ROOT%
    exit /b 1
)

echo [2/5] Copying Info.json and DLL into tmp...
copy /y "%DLL_PATH%" "%TMP_DIR%" >nul
copy /y "%INFO_PATH%" "%TMP_DIR%" >nul

rem --- Remove old zip ---
echo [3/5] Removing old archive if exists...
if exist "%ZIP_PATH%" del /f /q "%ZIP_PATH%"

rem --- Create zip ---
echo [4/5] Creating MagicShapeHelper.zip...
powershell -NoLogo -NoProfile -Command "Compress-Archive -Path '%TMP_DIR%\*' -DestinationPath '%ZIP_PATH%' -Force" || goto :zip_error

echo [5/5] Cleaning temp...
if exist "%TMP_DIR%" rd /s /q "%TMP_DIR%"

echo Done. Archive created at:
"%ZIP_PATH%"
exit /b 0

:zip_error
echo [ERROR] Failed to create archive. Check PowerShell Compress-Archive availability.
if exist "%TMP_DIR%" rd /s /q "%TMP_DIR%"
exit /b 1


