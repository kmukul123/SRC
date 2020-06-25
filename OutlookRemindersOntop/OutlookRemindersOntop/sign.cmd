setlocal
set path=%Path%;C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool

pushd %~dp0\bin\OutlookRemindersOntop.Release

signtool sign RemindersOntop.exe
signtool timestamp /t http://timestamp.digicert.com RemindersOntop.exe



endlocal