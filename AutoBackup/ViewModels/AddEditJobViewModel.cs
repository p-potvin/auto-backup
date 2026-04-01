using AutoBackup.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoBackup.ViewModels;

/// <summary>
/// ViewModel for the Add / Edit Job dialog.
/// </summary>
public sealed partial class AddEditJobViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _destinationPath = string.Empty;

    [ObservableProperty]
    private string _folderNamePattern = "backup-{yyyy-MM-dd}";

    [ObservableProperty]
    private int _intervalMinutes = 60;

    [ObservableProperty]
    private bool _sendNotifications = true;

    [ObservableProperty]
    private string? _blockingProcessName;

    [ObservableProperty]
    private ArchiveFormat _archiveFormat = ArchiveFormat.None;

    [ObservableProperty]
    private string _folderNamePreview = string.Empty;

    /// <summary>When editing an existing job, this holds its Id.</summary>
    public string? ExistingJobId { get; private set; }

    public bool IsEditing => ExistingJobId is not null;

    public bool CanSave => !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(SourcePath)
        && !string.IsNullOrWhiteSpace(DestinationPath);

    public IReadOnlyList<int> IntervalOptions { get; } =
        [15, 30, 60, 120, 360, 720, 1440];

    public IReadOnlyList<ArchiveFormat> ArchiveOptions { get; } =
        Enum.GetValues<ArchiveFormat>();

    // -------------------------------------------------------------------------

    /// <summary>Populates the form from an existing <see cref="BackupJob"/>.</summary>
    public void LoadFrom(BackupJob job)
    {
        ExistingJobId = job.Id;
        Name = job.Name;
        SourcePath = job.SourcePath;
        DestinationPath = job.DestinationPath;
        FolderNamePattern = job.FolderNamePattern;
        IntervalMinutes = job.IntervalMinutes;
        SendNotifications = job.SendNotifications;
        BlockingProcessName = job.BlockingProcessName;
        ArchiveFormat = job.ArchiveFormat;
        RefreshPreview();
    }

    /// <summary>Builds a new or updated <see cref="BackupJob"/> from current form values.</summary>
    public BackupJob ToJob()
    {
        var job = ExistingJobId is null
            ? new BackupJob()
            : new BackupJob { Id = ExistingJobId! };

        job.Name = Name.Trim();
        job.SourcePath = SourcePath.Trim();
        job.DestinationPath = DestinationPath.Trim();
        job.FolderNamePattern = FolderNamePattern.Trim();
        job.IntervalMinutes = IntervalMinutes;
        job.SendNotifications = SendNotifications;
        job.BlockingProcessName = string.IsNullOrWhiteSpace(BlockingProcessName)
            ? null
            : BlockingProcessName.Trim();
        job.ArchiveFormat = ArchiveFormat;
        return job;
    }

    [RelayCommand]
    private void RefreshPreview()
    {
        FolderNamePreview = Services.BackupService.ResolveFolderName(
            FolderNamePattern, DateTimeOffset.Now);
    }

    partial void OnFolderNamePatternChanged(string value) => RefreshPreview();
}
