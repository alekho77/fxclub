@echo off
set PACKAGE_NAME=icts-Win32COMApiTrader-3-0-34
set OUT_DIR=%1\Packages\%PACKAGE_NAME%
set PACKAGES_DIR=%2..\Packages

if exist %OUT_DIR% goto exit

:extract
mkdir %1\Packages

echo ... Extract files
%PACKAGES_DIR%\unzip %PACKAGES_DIR%\%PACKAGE_NAME%.zip -d %OUT_DIR%

echo ... Register FxComApiTrader
regsvr32 /s %OUT_DIR%\bin\FxComApiTrader.dll

:exit