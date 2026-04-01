using AutoBackup.Services;
using AutoBackup.ViewModels;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace AutoBackup;

/// <summary>
/// Bootstraps services and the main window.  When the user closes the window the
/// app minimises to the system tray (configurable) instead of exiting.
/// </summary>
public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }

    // Shared services — instantiated once and reused across the app.
    public static LoggingService Log { get; private set; } = null!;
    public static NotificationService Notifications { get; private set; } = null!;
    public static SettingsService Settings { get; private set; } = null!;
    public static BackupService BackupSvc { get; private set; } = null!;
    public static SchedulerService Scheduler { get; private set; } = null!;
    public static MainViewModel MainVm { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Boot services.
        Log = new LoggingService(SettingsService.LogFilePath);
        Notifications = new NotificationService(Log);
        Notifications.Initialize();
        Settings = new SettingsService();
        BackupSvc = new BackupService(Log, Notifications);
        Scheduler = new SchedulerService(BackupSvc, Settings, Log);
        MainVm = new MainViewModel(Settings, Scheduler, Log);

        MainWindow = new MainWindow();
        MainWindow.Activate();

        Log.Info("capp000", "AutoBackup started.");
    }
}
