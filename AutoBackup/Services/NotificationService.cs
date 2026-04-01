using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace AutoBackup.Services;

/// <summary>
/// Sends Windows toast notifications via the WinRT App Notification APIs.
/// Silently swallows errors so that a notification failure never crashes the backup.
/// </summary>
public sealed class NotificationService
{
    private readonly LoggingService _log;
    private bool _registered;

    public NotificationService(LoggingService log)
    {
        _log = log;
    }

    /// <summary>
    /// Registers the app with the Windows notification platform.
    /// Must be called once, before the first notification is sent.
    /// </summary>
    public void Initialize()
    {
        try
        {
            AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            AppNotificationManager.Default.Register();
            _registered = true;
        }
        catch (Exception ex)
        {
            _log.Warn("cnotify0", $"Could not register notification manager: {ex.Message}");
        }
    }

    public void Unregister()
    {
        if (_registered)
        {
            try { AppNotificationManager.Default.Unregister(); }
            catch { /* best-effort */ }
        }
    }

    /// <summary>Sends a "backup completed" toast notification.</summary>
    public Task SendSuccessAsync(string jobName, string? destination)
    {
        var body = string.IsNullOrEmpty(destination)
            ? "Your files were backed up successfully."
            : $"Backed up to: {destination}";

        return SendAsync("✅ Backup Complete", jobName, body);
    }

    /// <summary>Sends a "backup failed" toast notification.</summary>
    public Task SendErrorAsync(string jobName, string? errorMessage)
    {
        var body = string.IsNullOrEmpty(errorMessage)
            ? "An unexpected error occurred."
            : errorMessage;

        return SendAsync("❌ Backup Failed", jobName, body);
    }

    // -------------------------------------------------------------------------

    private Task SendAsync(string title, string subtitle, string body)
    {
        if (!_registered)
            return Task.CompletedTask;

        try
        {
            var builder = new AppNotificationBuilder()
                .AddText(title)
                .AddText(subtitle)
                .AddText(body);

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
        catch (Exception ex)
        {
            _log.Warn("cnotify1", $"Failed to send notification: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private void OnNotificationInvoked(
        AppNotificationManager sender, AppNotificationActivatedEventArgs args) { }
}
