@echo off

dotnet publish "PvzLauncherRemake\PvzLauncherRemake.csproj" -c Release -o "publish\bin"
dotnet publish "ExecuteShell\ExecuteShell.csproj" -c Release -o "publish"