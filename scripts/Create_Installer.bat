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

FOR /F "tokens=1-3" %%i IN ('Tools\sigcheck.exe "..\source\My Lyrics\bin\Release\MyLyrics.dll"') DO ( IF "%%i %%j"=="File version:" SET version=%%k )
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" MyLyrics.xmp2 /B /V=%version%
