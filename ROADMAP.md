# AutoBackup Roadmap

This document outlines the planned trajectory for development.

## Phase 4: Retention & Cleanup
- **Automatic Cleanup**: Option to delete backups older than X days/weeks.
- **Smart Retention**: Keep 1 daily for a week, 1 weekly for a month, etc.
- **Size Quota**: Stop/Warn when the destination folder exceeds a specific size.

## Phase 5: Enhanced Archiving
- **Differential/Incremental Backups**: Only copy changed files to save space.
- **Password-protected ZIPs**: Support AES-256 encrypted archives.
- **Progress Reporting**: Real-time percentage progress bar for large backup jobs.

## Phase 6: Cloud & Sync
- **OneDrive/Dropbox/S3 Integration**: Support direct cloud backup destinations.
- **Webhook Support**: Notify external services via HTTP POST on job completion.

## Phase 7: UI/UX Refinement
- **Tray Dashboards**: Show quick stats in the tray context menu.
- **Filter/Exclude Patterns**: Support for `.gitignore` style exclude rules.
- **Dark Mode Sync**: Automatically follow Windows system theme changes.
