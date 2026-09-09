@echo off
setlocal
set "SIGNE_PATH=C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
set "CERT_FILE=%~dp0TestKey.pfx"
set "TARGET_EXE=%~dp0bin\OutlookRemindersOntop.Release\RemindersOntop.exe"

if not exist "%SIGNE_PATH%" (
    echo Error: signtool.exe not found at "%SIGNE_PATH%"
    exit /b 1
)

if not exist "%CERT_FILE%" (
    echo Error: Certificate file "%CERT_FILE%" not found. 
    echo Please run 'powershell .\create_cert.ps1' first to generate a fresh one.
    exit /b 1
)

if not exist "%TARGET_EXE%" (
    echo Error: Target executable "%TARGET_EXE%" not found. Please build the project first.
    exit /b 1
)

echo.
echo --------------------------------------------------------
echo Final Signing Step
echo --------------------------------------------------------
set /p PASSWORD="Enter PFX password: "

echo.
echo Signing "%TARGET_EXE%"...
"%SIGNE_PATH%" sign /f "%CERT_FILE%" /p "%PASSWORD%" /fd SHA256 /a /v "%TARGET_EXE%"
if %errorlevel% neq 0 (
    echo.
    echo SignTool Error: Ensure you used the same password you entered during 'create_cert.ps1'.
    exit /b %errorlevel%
)

echo.
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