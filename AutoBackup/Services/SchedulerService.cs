using AutoBackup.Models;

namespace AutoBackup.Services;

/// <summary>
/// Manages all active <see cref="BackupJob"/> timers, scheduling and triggering
/// backup runs at each job's configured interval.
/// </summary>
public sealed class SchedulerService : IDisposable
{
    private readonly Dictionary<string, Timer> _timers = [];
    private readonly BackupService _backupService;
    private readonly LoggingService _log;

    public event EventHandler<BackupResult>? BackupCompleted;

    public SchedulerService(
        BackupService backupService,
        SettingsService settingsService,
        LoggingService log)
    {
        _backupService = backupService;
        _ = settingsService; // reserved for future use
        _log = log;
    }

    /// <summary>Starts timers for all enabled jobs.</summary>
    public void Start(IEnumerable<BackupJob> jobs)
    {
        foreach (var job in jobs)
            Schedule(job);
    }

    /// <summary>Schedules (or re-schedules) a single job.</summary>
    public void Schedule(BackupJob job)
    {
        Cancel(job.Id);

        if (!job.IsEnabled)
            return;

        var intervalMs = (int)TimeSpan.FromMinutes(job.IntervalMinutes).TotalMilliseconds;
        var timer = new Timer(OnTimerElapsed, job, intervalMs, intervalMs);
        _timers[job.Id] = timer;

        _log.Info("csched0",
            $"Scheduled job '{job.Name}' every {job.IntervalMinutes} min.");
    }

    /// <summary>Cancels the timer for a job without removing it from persistence.</summary>
    public void Cancel(string jobId)
    {
        if (_timers.Remove(jobId, out var timer))
            timer.Dispose();
    }

    /// <summary>Triggers a job run immediately, outside the normal schedule.</summary>
    public async Task RunNowAsync(BackupJob job)
    {
        var result = await _backupService.RunJobAsync(job);
        OnBackupCompleted(job, result);
    }

    public void Dispose()
    {
        foreach (var t in _timers.Values)
            t.Dispose();
        _timers.Clear();
    }

    // -------------------------------------------------------------------------

    private async void OnTimerElapsed(object? state)
    {
        if (state is not BackupJob job)
            return;

        var result = await _backupService.RunJobAsync(job);
        OnBackupCompleted(job, result);
    }

    private void OnBackupCompleted(BackupJob job, BackupResult result)
    {
        if (result.IsSuccess)
            job.LastRunUtc = DateTimeOffset.UtcNow;

        BackupCompleted?.Invoke(this, result);
    }
}
