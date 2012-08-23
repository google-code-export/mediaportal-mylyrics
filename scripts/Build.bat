@echo off
cls
Title Building MediaPortal MyLyrics (RELEASE)
cd ..

cd source
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=RELEASE MyLyrics.sln
cd ..

cd scripts
