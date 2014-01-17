@ECHO OFF

REM The following directory is for .NET 2.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%

echo Installing Broadcast Command Service...
echo ---------------------------------------------------
installutil /u Fanex.MailDeliver.MailServer.exe /servicename="Fanex Email Service Ver 1"
echo ---------------------------------------------------
echo Done.