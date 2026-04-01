using System.Collections.ObjectModel;
using AutoBackup.Models;
using AutoBackup.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;

namespace AutoBackup.ViewModels;

/// <summary>
/// ViewModel for the main window, holding the list of backup jobs and
/// coordinating with the scheduler.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly SchedulerService _scheduler;
    private readonly LoggingService _log;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private ObservableCollection<BackupJobViewModel> _jobs = [];

    [ObservableProperty]
    private BackupJobViewModel? _selectedJob;

    [ObservableProperty]
    private string _statusMessage = "Ready.";

    [ObservableProperty]
    private ObservableCollection<string> _logLines = [];

    public MainViewModel(
        SettingsService settings,
        SchedulerService scheduler,
        LoggingService log)
    {
        _settings = settings;
        _scheduler = scheduler;
        _log = log;
        _dispatcher = DispatcherQueue.GetForCurrentThread();

        _log.LineAdded += OnLogLineAdded;
        _scheduler.BackupCompleted += OnBackupCompleted;

        LoadJobs();
    }

    // -------------------------------------------------------------------------

    [RelayCommand]
    public void AddJob(BackupJob job)
    {
        var vm = new BackupJobViewModel(job);
        Jobs.Add(vm);
        _scheduler.Schedule(job);
        SaveJobs();
        StatusMessage = $"Job '{job.Name}' added.";
    }

    [RelayCommand]
    public void UpdateJob(BackupJob job)
    {
        var existing = Jobs.FirstOrDefault(j => j.Id == job.Id);
        if (existing is null) return;

        var idx = Jobs.IndexOf(existing);
        Jobs[idx] = new BackupJobViewModel(job);

        _scheduler.Cancel(job.Id);
        _scheduler.Schedule(job);
        SaveJobs();
        StatusMessage = $"Job '{job.Name}' updated.";
    }

    [RelayCommand]
    public void RemoveJob(BackupJobViewModel jobVm)
    {
        _scheduler.Cancel(jobVm.Id);
        Jobs.Remove(jobVm);
        SaveJobs();
        StatusMessage = $"Job '{jobVm.Name}' removed.";
    }

    [RelayCommand]
    public async Task RunJobNowAsync(BackupJobViewModel jobVm)
    {
        StatusMessage = $"Running '{jobVm.Name}'…";
        await _scheduler.RunNowAsync(jobVm.Job);
    }

    [RelayCommand]
    public void ToggleJobEnabled(BackupJobViewModel jobVm)
    {
        jobVm.Job.IsEnabled = !jobVm.Job.IsEnabled;
        jobVm.RefreshState();

        if (jobVm.Job.IsEnabled)
            _scheduler.Schedule(jobVm.Job);
        else
            _scheduler.Cancel(jobVm.Id);

        SaveJobs();
    }

    // -------------------------------------------------------------------------

    private void LoadJobs()
    {
        var loaded = _settings.LoadJobs();
        Jobs = new ObservableCollection<BackupJobViewModel>(
            loaded.Select(j => new BackupJobViewModel(j)));

        _scheduler.Start(loaded);
    }

    private void SaveJobs()
        => _settings.SaveJobs(Jobs.Select(j => j.Job));

    private void OnBackupCompleted(object? sender, BackupResult result)
    {
        var status = result.Status switch
        {
            BackupStatus.Success => $"✅ '{FindJobName(result.JobId)}' completed.",
            BackupStatus.PartialSuccess => $"⚠️ '{FindJobName(result.JobId)}' — some files skipped.",
            BackupStatus.SkippedBlockingProcess => $"⏭️ '{FindJobName(result.JobId)}' skipped (app running).",
            BackupStatus.Failed => $"❌ '{FindJobName(result.JobId)}' failed: {result.ErrorMessage}",
            _ => "Unknown status."
        };

        _dispatcher.TryEnqueue(() =>
        {
            StatusMessage = status;
            SaveJobs();
        });
    }

    private void OnLogLineAdded(object? sender, string line)
    {
        _dispatcher.TryEnqueue(() =>
        {
            if (LogLines.Count > 500)
                LogLines.RemoveAt(0);
            LogLines.Add(line);
        });
    }

    private string FindJobName(string jobId)
        => Jobs.FirstOrDefault(j => j.Id == jobId)?.Name ?? jobId;
}
