setlocal enabledelayedexpansion

SET configurationName=%1
ECHO %configurationName%

IF %configurationName%=="Debug" GOTO OnDebug
IF %configurationName%=="Release" GOTO OnRelease

:OnDebug
taskkill  /im 3dsmax.exe /f /fi "STATUS eq RUNNING"
GOTO Close

:OnRelease
Powershell.exe -Executionpolicy Remotesigned -File "%~dp0UpdateAssemblyVersion.ps1"
GOTO Close

:Close
pause
exit
