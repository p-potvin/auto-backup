using AutoBackup.Models;
using AutoBackup.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace AutoBackup.Views;

/// <summary>
/// Settings page for global app preferences.
/// </summary>
public sealed partial class SettingsPage : Page
{
    private AppSettings _settings = null!;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _settings = App.Settings.LoadSettings();

        MinimizeToTrayCheck.IsChecked = _settings.MinimizeToTray;
        StartWithWindowsCheck.IsChecked = _settings.StartWithWindows;
        LogPathText.Text = SettingsService.LogFilePath;

        // Select current theme in the combo.
        foreach (ComboBoxItem item in ThemeCombo.Items.OfType<ComboBoxItem>())
        {
            if ((string)item.Tag == (_settings.Theme ?? ""))
            {
                ThemeCombo.SelectedItem = item;
                break;
            }
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
        => Frame.GoBack();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.MinimizeToTray = MinimizeToTrayCheck.IsChecked == true;
        _settings.StartWithWindows = StartWithWindowsCheck.IsChecked == true;

        var selectedThemeTag = (ThemeCombo.SelectedItem as ComboBoxItem)?.Tag as string;
        _settings.Theme = string.IsNullOrEmpty(selectedThemeTag) ? null : selectedThemeTag;

        App.Settings.SaveSettings(_settings);

        ApplyTheme(_settings.Theme);
        ManageStartup(_settings.StartWithWindows);

        Frame.GoBack();
    }

    private async void OpenLog_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var file = await Windows.Storage.StorageFile
                .GetFileFromPathAsync(SettingsService.LogFilePath);
            await Launcher.LaunchFileAsync(file);
        }
        catch
        {
            // Log file may not exist yet.
        }
    }

    // -------------------------------------------------------------------------

    private void ApplyTheme(string? theme)
    {
        var requested = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };

        if (App.MainWindow?.Content is FrameworkElement root)
            root.RequestedTheme = requested;
    }

    private static void ManageStartup(bool enable)
    {
        const string AppName = "AutoBackup";
        const string RegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegKey, writable: true);
        if (key is null) return;

        if (enable)
            key.SetValue(AppName, System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "");
        else
            key.DeleteValue(AppName, throwOnMissingValue: false);
    }
}
