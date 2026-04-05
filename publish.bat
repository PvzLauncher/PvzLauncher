@echo off
setlocal enabledelayedexpansion

rd publish /s

dotnet publish "PvzLauncherRemake\PvzLauncherRemake.csproj" -c Release -o "publish\bin"
dotnet publish "ExecuteShell\ExecuteShell.csproj" -c Release -o "publish"
dotnet publish "StdUpdateService\StdUpdateService.csproj" -c Release -r win-x86 --self-contained false -o "publish\bin" -p:PublishSingleFile=true -p:DebugType=embedded

del "publish\PvzLauncher.deps.json"
del "publish\PvzLauncher.pdb"
del "publish\bin\PvzLauncherRemake.deps.json"
del "publish\bin\PvzLauncherRemake.pdb"

pause