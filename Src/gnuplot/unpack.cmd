@echo off
set PACKAGE_NAME=gp460win32.zip
set OUT_DIR=%1\Packages
set PACKAGES_DIR=%2..\Packages

if exist %OUT_DIR%\gnuplot goto exit

:extract
mkdir %1\Packages

echo ... Extract files
%PACKAGES_DIR%\7za x %PACKAGES_DIR%\%PACKAGE_NAME% -o%OUT_DIR%

:exit