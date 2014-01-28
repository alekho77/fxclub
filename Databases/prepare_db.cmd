@echo off
echo Preparing Database

rem %1 => $(TargetDir)
rem %2 => $(SolutionDir)
rem %3 => SrcDbName
rem [%4] => DstDbName

set PACKAGES_DIR=%2..\Packages
set SRCDBNAME=%3
set DBDIR=%2..\Databases\%SRCDBNAME%DB\
if -%4==- (
set DSTDBNAME=%SRCDBNAME%
) else (
set DSTDBNAME=%4
)

if exist %1%DSTDBNAME%.fdb goto exit

echo Extract backup and create database
%PACKAGES_DIR%\7za e %DBDIR%%SRCDBNAME%.fbk.7z -o%DBDIR% -aos
gbak -c -v -user sysdba -pas masterkey  %DBDIR%%SRCDBNAME%.fbk %1%DSTDBNAME%.fdb

:exit