@echo off
set PACKAGE_NAME=FxArchives
set OUT_DIR=%1\Packages\%PACKAGE_NAME%
set PACKAGES_DIR=%2..\Packages

if exist %OUT_DIR% goto exit

:extract
mkdir %OUT_DIR%

echo ... Extract files
%PACKAGES_DIR%\unzip %PACKAGES_DIR%\%PACKAGE_NAME%\*.zip -d %OUT_DIR%

:exit