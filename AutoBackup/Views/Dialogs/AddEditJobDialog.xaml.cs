using AutoBackup.Models;
using AutoBackup.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AutoBackup.Views.Dialogs;

/// <summary>
/// Dialog for adding a new backup job or editing an existing one.
/// </summary>
public sealed partial class AddEditJobDialog : ContentDialog
{
    public AddEditJobViewModel ViewModel { get; } = new();

    /// <summary>The validated <see cref="BackupJob"/> produced on Save, or null if cancelled.</summary>
    public BackupJob? ResultJob { get; private set; }

    public AddEditJobDialog(BackupJob? existingJob)
    {
        InitializeComponent();

        if (existingJob is not null)
            ViewModel.LoadFrom(existingJob);

        PrimaryButtonClick += OnSaveClick;
    }

    // ── Folder Pickers ───────────────────────────────────────────────────────

    private async void BrowseSource_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickFolderAsync();
        if (path is not null)
            ViewModel.SourcePath = path;
    }

    private async void BrowseDest_Click(object sender, RoutedEventArgs e)
    {
        var path = await PickFolderAsync();
        if (path is not null)
            ViewModel.DestinationPath = path;
    }

    private void OnSaveClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ResultJob = ViewModel.ToJob();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
        picker.FileTypeFilter.Add("*");

        // Associate the picker with the app window (required for unpackaged apps).
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow!);
        InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
