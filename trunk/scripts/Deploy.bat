@echo off
cls
Title Deploying MediaPortal MyLyrics (RELEASE)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

copy /y source\My Lyrics\bin\Release\LyricsEngine.dll "%PROGS%\Team MediaPortal\MediaPortal\"
copy /y source\My Lyrics\bin\Release\TranslateProvider.dll "%PROGS%\Team MediaPortal\MediaPortal\"
copy /y source\My Lyrics\bin\Release\MyLyrics.dll "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"

cd scripts