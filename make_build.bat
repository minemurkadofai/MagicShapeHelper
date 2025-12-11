@echo off
setlocal EnableDelayedExpansion

set "MOD_ROOT=%~dp0"
set "CSPROJ=%MOD_ROOT%MagicShapeHelper.csproj"
set "CONFIG=Release"
set "OUT_DLL=%MOD_ROOT%bin\Release\MagicShapeHelper.dll"

if not exist "%CSPROJ%" (
    echo [ERROR] MagicShapeHelper.csproj not found in %MOD_ROOT%
    exit /b 1
)

rem --- choose build tool: msbuild or dotnet msbuild ---
set "MSBUILD_CMD="
where msbuild >nul 2>&1 && set "MSBUILD_CMD=msbuild"
if "%MSBUILD_CMD%"=="" (
    where dotnet >nul 2>&1 && set "MSBUILD_CMD=dotnet msbuild"
)
if "%MSBUILD_CMD%"=="" (
    echo [ERROR] msbuild not found. Install Visual Studio Build Tools or .NET SDK.
    exit /b 1
)

echo [1/3] Building via %MSBUILD_CMD% ...
%MSBUILD_CMD% "%CSPROJ%" /p:Configuration=%CONFIG% /v:m || (
    echo [ERROR] Build failed.
    exit /b 1
)

if not exist "%OUT_DLL%" (
    echo [ERROR] Build succeeded but DLL not found: %OUT_DLL%
    exit /b 1
)

echo [2/3] Copy DLL to mod root...
copy /y "%OUT_DLL%" "%MOD_ROOT%MagicShapeHelper.dll" >nul || (
    echo [ERROR] Failed to copy DLL to mod root.
    exit /b 1
)

echo [3/3] Done. DLL ready at: %MOD_ROOT%MagicShapeHelper.dll
exit /b 0


