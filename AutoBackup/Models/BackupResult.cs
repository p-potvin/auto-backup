namespace AutoBackup.Models;

/// <summary>
/// Outcome of a single backup run attempt.
/// </summary>
public enum BackupStatus
{
    Success,
    SkippedBlockingProcess,
    PartialSuccess,
    Failed
}

/// <summary>
/// Carries the result of one backup job execution, including retry information.
/// </summary>
public record BackupResult(
    string JobId,
    string CorrelationId,
    BackupStatus Status,
    int AttemptNumber,
    int FilesCopied,
    int FilesSkipped,
    string? DestinationFolder,
    string? ErrorMessage = null,
    Exception? Exception = null)
{
    public bool IsSuccess => Status == BackupStatus.Success
        || Status == BackupStatus.PartialSuccess;

    public DateTimeOffset CompletedAt { get; init; } = DateTimeOffset.UtcNow;
}
