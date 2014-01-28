@echo off
set PACKAGE_NAME=NETProvider-2.7.5-NET35
set OUT_DIR=%1\Packages\%PACKAGE_NAME%
set PACKAGES_DIR=%2..\Packages

if exist %OUT_DIR% goto exit

:extract
mkdir %1\Packages

echo ... Extract files
%PACKAGES_DIR%\7za e %PACKAGES_DIR%\%PACKAGE_NAME%.7z -o%OUT_DIR%

:exit