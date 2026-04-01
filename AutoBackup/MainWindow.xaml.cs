using AutoBackup.Views;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace AutoBackup;

/// <summary>
/// The single application window. Navigates to <see cref="MainPage"/> on load
/// and minimises to the system tray when closed (if that option is enabled).
/// </summary>
public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        Title = "AutoBackup";
        AppWindow.SetIcon("Assets/app-icon.ico");
        ExtendsContentIntoTitleBar = true;

        Closed += OnWindowClosed;

        RootFrame.Navigate(typeof(MainPage));
    }

    // -------------------------------------------------------------------------

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        var settings = App.Settings.LoadSettings();

        if (settings.MinimizeToTray)
        {
            // Prevent the window from being destroyed — hide it instead.
            args.Handled = true;
            this.Hide();
        }
        else
        {
            App.Notifications.Unregister();
            App.Scheduler.Dispose();
            App.Log.Dispose();
        }
    }

    private void TrayOpen_Click(object sender, RoutedEventArgs e)
    {
        this.Show();
        Activate();
    }

    private void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        App.Notifications.Unregister();
        App.Scheduler.Dispose();
        App.Log.Dispose();
        Application.Current.Exit();
    }
}
