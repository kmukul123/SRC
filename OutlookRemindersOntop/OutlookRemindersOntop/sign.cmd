@echo off
setlocal
set "SIGNE_PATH=C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
set "CERT_FILE=%~dp0\TestKey.pfx"
set "TARGET_EXE=%~dp0\bin\OutlookRemindersOntop.Release\RemindersOntop.exe"

if not exist "%SIGNE_PATH%" (
    echo Error: signtool.exe not found at %SIGNE_PATH%
    exit /b 1
)

if not exist "%CERT_FILE%" (
    echo Error: Certificate file %CERT_FILE% not found.
    exit /b 1
)

if not exist "%TARGET_EXE%" (
    echo Error: Target executable %TARGET_EXE% not found. Please build the project first.
    exit /b 1
)

set /p PASSWORD="Enter PFX password: "

echo Signing %TARGET_EXE%...
"%SIGNE_PATH%" sign /f "%CERT_FILE%" /p "%PASSWORD%" /v "%TARGET_EXE%"
if %errorlevel% neq 0 (
    echo Error signing file.
    exit /b %errorlevel%
)

echo Adding timestamp...
"%SIGNE_PATH%" timestamp /t http://timestamp.digicert.com "%TARGET_EXE%"
if %errorlevel% neq 0 (
    echo Error adding timestamp.
    exit /b %errorlevel%
)

echo.
echo Success! RemindersOntop.exe has been signed and timestamped.
pause
endlocal