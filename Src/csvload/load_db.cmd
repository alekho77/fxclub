@echo off

for %%f in (..\..\Packages\FxArchives\*.csv) do (
set p=%%~nf
echo Loading !p:~0,6! from "%%f"...
csvload fxtest.fdb %%f !p:~0,6!
)
