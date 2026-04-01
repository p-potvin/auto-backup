using System.IO.Compression;
using AutoBackup.Models;

namespace AutoBackup.Services;

/// <summary>
/// Performs the actual file backup with retry logic, in-use file detection,
/// and optional ZIP archiving.
/// </summary>
public sealed class BackupService
{
    private const int MaxRetries = 2;
    private const int RetryDelayMs = 5_000;

    private readonly LoggingService _log;
    private readonly NotificationService _notifications;

    public BackupService(LoggingService log, NotificationService notifications)
    {
        _log = log;
        _notifications = notifications;
    }

    /// <summary>
    /// Executes a backup job, retrying up to <see cref="MaxRetries"/> times on failure.
    /// Returns the final <see cref="BackupResult"/>.
    /// </summary>
    public async Task<BackupResult> RunJobAsync(BackupJob job, CancellationToken ct = default)
    {
        var correlationId = GenerateCorrelationId();

        _log.Info(correlationId, $"Starting backup job '{job.Name}' (id={job.Id}).");

        if (IsBlockingProcessRunning(job.BlockingProcessName))
        {
            var msg = $"Skipped: blocking process '{job.BlockingProcessName}' is running.";
            _log.Warn(correlationId, msg);
            return new BackupResult(job.Id, correlationId, BackupStatus.SkippedBlockingProcess,
                0, 0, 0, null, msg);
        }

        BackupResult? lastResult = null;

        for (int attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            try
            {
                lastResult = await ExecuteBackupAsync(job, correlationId, attempt, ct);

                if (lastResult.IsSuccess)
                {
                    _log.Info(correlationId, $"Backup completed successfully on attempt {attempt}.");

                    if (job.SendNotifications)
                        await _notifications.SendSuccessAsync(job.Name, lastResult.DestinationFolder);

                    return lastResult;
                }
            }
            catch (OperationCanceledException)
            {
                _log.Warn(correlationId, "Backup cancelled by user.");
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(correlationId,
                    $"Attempt {attempt}/{MaxRetries + 1} failed: {ex.Message}", ex);

                lastResult = new BackupResult(job.Id, correlationId, BackupStatus.Failed,
                    attempt, 0, 0, null, ex.Message, ex);
            }

            if (attempt <= MaxRetries)
            {
                _log.Warn(correlationId,
                    $"Retrying in {RetryDelayMs / 1000}s… (attempt {attempt + 1}/{MaxRetries + 1})");
                await Task.Delay(RetryDelayMs, ct);
            }
        }

        _log.Error(correlationId, $"Backup job '{job.Name}' failed after all retries.");

        if (job.SendNotifications)
            await _notifications.SendErrorAsync(job.Name, lastResult?.ErrorMessage);

        return lastResult ?? new BackupResult(job.Id, correlationId, BackupStatus.Failed,
            MaxRetries + 1, 0, 0, null, "Unknown error");
    }

    // -------------------------------------------------------------------------

    private async Task<BackupResult> ExecuteBackupAsync(
        BackupJob job, string correlationId, int attempt, CancellationToken ct)
    {
        var now = DateTimeOffset.Now;
        var destFolderName = ResolveFolderName(job.FolderNamePattern, now);
        var destRoot = Path.Combine(job.DestinationPath, destFolderName);

        if (!Directory.Exists(job.SourcePath))
            throw new DirectoryNotFoundException($"Source path not found: {job.SourcePath}");

        if (job.ArchiveFormat == ArchiveFormat.Zip)
            return await CreateZipBackupAsync(job, destRoot, correlationId, attempt, ct);

        return await CopyFilesAsync(job, destRoot, correlationId, attempt, ct);
    }

    /// <summary>
    /// Copies files from source to destination, skipping files that are locked
    /// by another process (safest approach without requiring elevated privileges).
    /// </summary>
    private async Task<BackupResult> CopyFilesAsync(
        BackupJob job, string destRoot, string correlationId, int attempt, CancellationToken ct)
    {
        Directory.CreateDirectory(destRoot);

        var allFiles = Directory.EnumerateFiles(job.SourcePath, "*", SearchOption.AllDirectories);
        int copied = 0;
        int skipped = 0;

        foreach (var srcFile in allFiles)
        {
            ct.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(job.SourcePath, srcFile);
            var destFile = Path.Combine(destRoot, relativePath);
            var destDir = Path.GetDirectoryName(destFile)!;

            Directory.CreateDirectory(destDir);

            if (!TryOpenExclusive(srcFile))
            {
                _log.Warn(correlationId, $"Skipping locked file: {relativePath}");
                skipped++;
                continue;
            }

            // Use async copy to avoid blocking the thread pool
            await Task.Run(() => File.Copy(srcFile, destFile, overwrite: true), ct);
            copied++;
        }

        var status = skipped > 0 ? BackupStatus.PartialSuccess : BackupStatus.Success;
        return new BackupResult(job.Id, correlationId, status, attempt, copied, skipped, destRoot);
    }

    /// <summary>
    /// Creates a ZIP archive of the source folder in the destination directory.
    /// </summary>
    private async Task<BackupResult> CreateZipBackupAsync(
        BackupJob job, string destRoot, string correlationId, int attempt, CancellationToken ct)
    {
        Directory.CreateDirectory(job.DestinationPath);

        var zipPath = destRoot + ".zip";

        await Task.Run(() =>
        {
            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            var allFiles = Directory.EnumerateFiles(job.SourcePath, "*", SearchOption.AllDirectories);

            foreach (var srcFile in allFiles)
            {
                ct.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(job.SourcePath, srcFile);
                archive.CreateEntryFromFile(srcFile, relativePath, CompressionLevel.Optimal);
            }
        }, ct);

        return new BackupResult(job.Id, correlationId, BackupStatus.Success,
            attempt, 0, 0, zipPath);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Checks whether a named process is currently running.
    /// Returns false when <paramref name="processName"/> is null or empty.
    /// </summary>
    private static bool IsBlockingProcessRunning(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
            return false;

        var name = processName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase).Trim();
        return System.Diagnostics.Process.GetProcessesByName(name).Length > 0;
    }

    /// <summary>
    /// Attempts to open a file for exclusive read access to detect whether it
    /// is locked by another process.  Returns true when the file is accessible.
    /// </summary>
    private static bool TryOpenExclusive(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.None, 1, FileOptions.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>
    /// Expands a folder-name pattern by replacing {format} tokens with the
    /// corresponding <see cref="DateTimeOffset"/> formatted value.
    /// Example pattern: "backup-{yyyy-MM-dd_HH-mm}" → "backup-2025-06-15_14-30"
    /// </summary>
    public static string ResolveFolderName(string pattern, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return $"backup-{now:yyyy-MM-dd}";

        // Replace all {format} tokens with formatted DateTime values.
        return System.Text.RegularExpressions.Regex.Replace(
            pattern,
            @"\{([^}]+)\}",
            m => now.ToString(m.Groups[1].Value));
    }

    /// <summary>
    /// Generates a 7-character alphanumeric CorrelationId beginning with 'c'.
    /// </summary>
    private static string GenerateCorrelationId()
        => "c" + Guid.NewGuid().ToString("N")[..6];
}
