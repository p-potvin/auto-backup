# AutoBackup

A lightweight Windows native background backup utility built with **C# .NET 8** and **WinUI 3**.

## Features

- **Multiple backup jobs** — add as many source → destination pairs as you need.
- **Custom folder naming** — use `{yyyy}`, `{MM}`, `{dd}`, `{HH}`, `{mm}` tokens to build date-stamped destination folders (e.g. `backup-{yyyy-MM-dd}`).
- **Configurable schedule** — set a backup interval in minutes (15 min, 30 min, 1 h, 2 h, …).
- **Blocking process guard** — optionally skip a backup job while a specific app (e.g. `outlook.exe`) is running, to prevent file corruption.
- **Windows toast notifications** — receive a system notification on backup completion or failure.
- **ZIP archiving** — choose between a plain folder copy or a compressed ZIP archive.
- **Resilient copy** — locked files are gracefully skipped; failed runs are retried twice (5 s delay), then a failure notification is sent.
- **Structured log file** — every event is logged with a 7-char CorrelationId to `%LOCALAPPDATA%\VaultWares\AutoBackup\logs\autobackup.log`.
- **System tray** — the app hides to the notification area when closed (configurable). Double-click the tray icon to reopen; right-click for a context menu.
- **Start with Windows** — optional auto-start at login via the Windows registry.

## Tech Stack

| Layer | Library |
|---|---|
| UI Framework | WinUI 3 (Windows App SDK 1.5) |
| Language | C# 12 / .NET 8 |
| MVVM | CommunityToolkit.Mvvm 8 |
| Tray Icon | WinUIEx 2.3 |
| Notifications | Microsoft.Windows.AppNotifications (WinRT) |
| Compression | System.IO.Compression (BCL) |
| Persistence | System.Text.Json (BCL) |

## Requirements

- Windows 10 version 1809 (build 17763) or later
- [Windows App Runtime 1.5](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
- Visual Studio 2022 17.8+ with the **Windows App SDK** workload

## Build

```powershell
# Clone
git clone https://github.com/p-potvin/auto-backup.git
cd auto-backup

# Restore and build (x64 Debug)
dotnet build AutoBackup/AutoBackup.csproj -c Debug -p:Platform=x64

# Run
dotnet run --project AutoBackup/AutoBackup.csproj -p:Platform=x64
```

## Data locations

| Item | Path |
|---|---|
| Job configurations | `%LOCALAPPDATA%\VaultWares\AutoBackup\jobs.json` |
| App settings | `%LOCALAPPDATA%\VaultWares\AutoBackup\settings.json` |
| Log file | `%LOCALAPPDATA%\VaultWares\AutoBackup\logs\autobackup.log` |

## Folder name pattern tokens

| Token | Example output |
|---|---|
| `{yyyy}` | `2025` |
| `{MM}` | `06` |
| `{dd}` | `15` |
| `{HH}` | `14` |
| `{mm}` | `30` |
| `{yyyy-MM-dd}` | `2025-06-15` |
| `{yyyy-MM-dd_HH-mm}` | `2025-06-15_14-30` |

## License

MIT — see [LICENSE](LICENSE).
