@echo off
rem update the package

powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
dnvm upgrade
References\dnx-clr-win-x86.1.0.0-rc1-update1\dnu.cmd restore
nuget restore