using AutoBackup.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoBackup.ViewModels;

/// <summary>
/// Wraps a <see cref="BackupJob"/> for data-binding in list views and detail panels.
/// </summary>
public sealed partial class BackupJobViewModel : ObservableObject
{
    public BackupJob Job { get; }

    public string Id => Job.Id;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _sourcePath;

    [ObservableProperty]
    private string _destinationPath;

    [ObservableProperty]
    private string _folderNamePattern;

    [ObservableProperty]
    private int _intervalMinutes;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private string? _blockingProcessName;

    [ObservableProperty]
    private bool _sendNotifications;

    [ObservableProperty]
    private ArchiveFormat _archiveFormat;

    [ObservableProperty]
    private DateTimeOffset? _lastRunUtc;

    public string IntervalDisplay => IntervalMinutes switch
    {
        < 60 => $"Every {IntervalMinutes} min",
        60 => "Every hour",
        _ when IntervalMinutes % 60 == 0 => $"Every {IntervalMinutes / 60} h",
        _ => $"Every {IntervalMinutes} min"
    };

    public string LastRunDisplay => LastRunUtc.HasValue
        ? LastRunUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        : "Never";

    public string StatusBadge => IsEnabled ? "Enabled" : "Paused";

    public BackupJobViewModel(BackupJob job)
    {
        Job = job;
        _name = job.Name;
        _sourcePath = job.SourcePath;
        _destinationPath = job.DestinationPath;
        _folderNamePattern = job.FolderNamePattern;
        _intervalMinutes = job.IntervalMinutes;
        _isEnabled = job.IsEnabled;
        _blockingProcessName = job.BlockingProcessName;
        _sendNotifications = job.SendNotifications;
        _archiveFormat = job.ArchiveFormat;
        _lastRunUtc = job.LastRunUtc;
    }

    /// <summary>
    /// Refreshes observable properties from the underlying <see cref="Job"/> model
    /// (call after mutating Job directly).
    /// </summary>
    public void RefreshState()
    {
        IsEnabled = Job.IsEnabled;
        LastRunUtc = Job.LastRunUtc;
        OnPropertyChanged(nameof(StatusBadge));
        OnPropertyChanged(nameof(LastRunDisplay));
    }
}
