using System.Text.Json.Serialization;

namespace AutoBackup.Models;

/// <summary>
/// Supported archive formats for backup output.
/// </summary>
public enum ArchiveFormat
{
    None,
    Zip
}

/// <summary>
/// Represents a single backup job configuration — its source, destination,
/// schedule, and all user-defined options.
/// </summary>
public class BackupJob
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..7];

    public string Name { get; set; } = string.Empty;

    public string SourcePath { get; set; } = string.Empty;

    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Folder name pattern with optional DateTime format tokens.
    /// Example: "backup-{yyyy-MM-dd_HH-mm}"
    /// </summary>
    public string FolderNamePattern { get; set; } = "backup-{yyyy-MM-dd}";

    /// <summary>Backup interval in minutes.</summary>
    public int IntervalMinutes { get; set; } = 60;

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// If set, the backup is skipped while the named process is running
    /// (protects against in-use file corruption).
    /// </summary>
    public string? BlockingProcessName { get; set; }

    public bool SendNotifications { get; set; } = true;

    public ArchiveFormat ArchiveFormat { get; set; } = ArchiveFormat.None;

    /// <summary>UTC timestamp of the last successful backup run.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? LastRunUtc { get; set; }

    /// <summary>UTC timestamp of the next scheduled backup run.</summary>
    [JsonIgnore]
    public DateTimeOffset NextRunUtc => (LastRunUtc ?? DateTimeOffset.UtcNow)
        .AddMinutes(IntervalMinutes);
}
