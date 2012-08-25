@echo off
cls
Title Building MediaPortal MyLyrics (DEBUG)
cd ..

cd source
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG MyLyrics.sln
cd ..

cd scripts