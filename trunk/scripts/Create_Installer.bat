@echo off
cls
Title Creating MediaPortal MyLyrics Installer

:: Check for modification
svn status ..\source | findstr "^M"
if ERRORLEVEL 0 (
	echo There are modifications in source folder. Aborting.
	pause
	exit 1
)

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

:: Temp xmp2 file
copy MyLyrics.xmp2 MyLyricsTemp.xmp2

:: Sed "update-{VERSION}.xml" from xmp2 file
Tools\sed.exe -i "s/update-{VERSION}.xml/update-%version%.xml/g" MyLyricsTemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" MyLyricsTemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del MyLyricsTemp.xmp2

:: Sed "MyLyrics-{VERSION}.MPE1" from update.xml
Tools\sed.exe -i "s/MyLyrics-{VERSION}.MPE1/MyLyrics-%version%.MPE1/g" update-%version%.xml

:: Parse version (Might be needed in the futute)
FOR /F "tokens=1-4 delims=." %%i IN ("%version%") DO ( 
	SET major=%%i
	SET minor=%%j
	SET build=%%k
	SET revision=%%l
)

:: Rename MPE1
if exist "..\builds\MyLyrics-%major%.%minor%.%build%.%revision%.MPE1" del "..\builds\MyLyrics-%major%.%minor%.%build%.%revision%.MPE1"
rename ..\builds\MyLyrics-MAJOR.MINOR.BUILD.REVISION.MPE1 "MyLyrics-%major%.%minor%.%build%.%revision%.MPE1"


