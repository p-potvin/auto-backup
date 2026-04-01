using AutoBackup.ViewModels;
using AutoBackup.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AutoBackup.Views;

/// <summary>
/// Main page showing the list of backup jobs and their details.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = App.MainVm;

    public MainPage()
    {
        InitializeComponent();
    }

    // ── Toolbar ──────────────────────────────────────────────────────────────

    private async void AddJob_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEditJobDialog(null) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.ResultJob is not null)
            ViewModel.AddJob(dialog.ResultJob);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SettingsPage));

    // ── Detail Panel ─────────────────────────────────────────────────────────

    private async void EditJob_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedJob is null) return;

        var dialog = new AddEditJobDialog(ViewModel.SelectedJob.Job) { XamlRoot = XamlRoot };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.ResultJob is not null)
            ViewModel.UpdateJob(dialog.ResultJob);
    }

    private async void RunJobNow_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedJob is null) return;
        await ViewModel.RunJobNowAsync(ViewModel.SelectedJob);
    }

    private void ToggleEnabled_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedJob is not null)
            ViewModel.ToggleJobEnabled(ViewModel.SelectedJob);
    }

    private async void DeleteJob_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedJob is null) return;

        var confirm = new ContentDialog
        {
            Title = "Remove Job",
            Content = $"Remove backup job '{ViewModel.SelectedJob.Name}'?",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
            ViewModel.RemoveJob(ViewModel.SelectedJob);
    }
}
