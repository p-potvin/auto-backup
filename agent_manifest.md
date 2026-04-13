# Agent Manifest: AutoBackup

## Project Summary
AutoBackup is a WinUI 3 application for managing local automated backup jobs.

## Architectural Decisions
- **Framework**: WinUI 3 (Windows App SDK) with WinUIEx for tray support.
- **State Management**: MVVM via CommunityToolkit.Mvvm.
- **Persistence**: JSON files in `%LOCALAPPDATA%\VaultWares\AutoBackup`.
- **Concurrency**: `System.Threading.Timer` for scheduling; `Task` for backup execution.
- **Registry Choice**: Uses `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run` for startup.

## Verified Commands
- **Build**: `dotnet build AutoBackup/AutoBackup.csproj -c Debug -p:Platform=x64`
- **Run**: `dotnet run --project AutoBackup/AutoBackup.csproj -p:Platform=x64`
- **Clean**: `dotnet clean AutoBackup/AutoBackup.csproj`

## Critical Path Issues
- **Locked Files**: Backup logic skips locked files with a log warning to avoid permission crashes.
- **Context Isolation**: Always ensure `x64` platform is specified during build/run to avoid SDK mismatch.
