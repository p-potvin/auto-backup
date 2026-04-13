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

        var interval = TimeSpan.FromMinutes(job.IntervalMinutes);
        var firstDelay = CalculateInitialDelay(job, interval);

        var timer = new Timer(OnTimerElapsed, job, firstDelay, interval);
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

    private static TimeSpan CalculateInitialDelay(BackupJob job, TimeSpan interval)
    {
        if (!job.LastRunUtc.HasValue)
            return interval;

        var nextRun = job.LastRunUtc.Value.Add(interval);
        var remaining = nextRun - DateTimeOffset.UtcNow;

        if (remaining <= TimeSpan.Zero)
            return TimeSpan.FromSeconds(2); // Overdue: run shortly after startup

        return remaining;
    }

    private async void OnTimerElapsed(object? state)
    {
        if (state is not BackupJob job)
            return;

        try
        {
            var result = await _backupService.RunJobAsync(job);
            OnBackupCompleted(job, result);
        }
        catch (Exception ex)
        {
            _log.Error("csched-err", $"Background timer for '{job.Name}' failed: {ex.Message}", ex);
        }
    }

    private void OnBackupCompleted(BackupJob job, BackupResult result)
    {
        if (result.IsSuccess)
            job.LastRunUtc = DateTimeOffset.UtcNow;

        BackupCompleted?.Invoke(this, result);
    }
}
