SETLOCAL enabledelayedexpansion
@ECHO off

SET max_version=%2
SET source_dir="%~dp0assemblies\%max_version%\"
ECHO %source_dir%

IF %1=="Debug" GOTO OnDebug
IF %1=="Release" GOTO OnRelease



:OnDebug
SET dest_dir="C:\Program Files\Autodesk\3ds Max %max_version%"
GOTO CopyFiles

:OnRelease
SET dest_dir="D:\KittyHawk\ASSETS\Tool\GraphPlug\3DSMax\%max_version%"
GOTO CopyFiles

:CopyFiles
ECHO :: Copying plug-in files
ECHO :: From: %source_dir%
ECHO :: To: %dest_dir%\bin\assemblies\
ECHO :: %1
COPY %source_dir%\*.dll %dest_dir%\bin\assemblies\

IF %1=="Debug" GOTO DebugOnMax
GOTO Close

:NoConfiguration
ECHO "No Configuaration"
GOTO Close

:DebugOnMax
START /d "C:\Program Files\Autodesk\3ds Max %max_version%\" 3dsmax.exe "D:\KittyHawk\ASSETS\KittyHawk_Data\OBJECT\PLANES\B787-10\_source\B787_10.max"
GOTO Close

:Close
PAUSE
EXIT