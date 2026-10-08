@echo off
setlocal
cd /d "%~dp0"
set "REVIT=C:\Program Files\Autodesk\Revit 2025"
set "DEST=%APPDATA%\Autodesk\Revit\Addins\2025"

if not exist "%REVIT%\RevitAPI.dll" (
  echo Revit 2025 not found at "%REVIT%".
  goto :fail
)
tasklist /FI "IMAGENAME eq Revit.exe" | find /I "Revit.exe" >nul
if not errorlevel 1 (
  echo Revit is running. Close Revit, then run build.bat again.
  goto :fail
)

dotnet build TagReconciler.csproj -c Release -p:RevitInstallDir="%REVIT%"
if errorlevel 1 goto :fail

if not exist "%DEST%\TagReconciler" mkdir "%DEST%\TagReconciler"
copy /Y "bin\Release\TagReconciler.dll" "%DEST%\TagReconciler\" >nul || goto :fail
copy /Y "TagReconciler.config.json" "%DEST%\TagReconciler\" >nul || goto :fail
powershell -NoProfile -Command "(Get-Content 'TagReconciler.addin.template') -replace '\{ASM\}', '%DEST%\TagReconciler\TagReconciler.dll' | Set-Content -Encoding UTF8 '%DEST%\TagReconciler.addin'" || goto :fail

echo.
echo INSTALLED to %DEST%
echo Start Revit, open DC_Plant.rvt, then Add-Ins - External Tools.
pause
exit /b 0

:fail
echo.
echo FAILED. Copy everything in this window into the chat.
pause
exit /b 1
