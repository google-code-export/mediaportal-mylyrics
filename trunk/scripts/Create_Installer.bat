@echo off
cls
Title Creating MediaPortal MyLyrics Installer

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

:: Get version from DLL
FOR /F "tokens=1-3" %%i IN ('Tools\sigcheck.exe "..\source\My Lyrics\bin\Release\MyLyrics.dll"') DO ( IF "%%i %%j"=="File version:" SET version=%%k )

:: trim version
SET version=%version:~0,-1%

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" MyLyrics.xmp2 /B /V=%version%

:: Parse version (Might be needed in the futute)
FOR /F "tokens=1-4 delims=." %%i IN ("%version%") DO ( 
	SET major=%%i
	SET minor=%%j
	SET build=%%k
	SET revision=%%l
)

:: Rename MPE1
rename ..\builds\MyLyrics-MAJOR.MINOR.BUILD.REVISION.MPE1 "MyLyrics-%major%.%minor%.%build%.%revision%.MPE1"
