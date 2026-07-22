@echo off
setlocal enabledelayedexpansion

rd publish /s

dotnet publish "src\PvzLauncherRemake\PvzLauncherRemake.csproj" -c Release -o "publish\bin"
dotnet publish "src\ExecuteShell\ExecuteShell.csproj" -c Release -o "publish"
dotnet publish "src\StdUpdateService\StdUpdateService.csproj" -c Release -o "publish\bin"

del "publish\PvzLauncher.deps.json"
del "publish\PvzLauncher.pdb"
del "publish\bin\PvzLauncherRemake.deps.json"
del "publish\bin\PvzLauncherRemake.pdb"
del "publish\bin\StdUpdateService.pdb"

pause