using Microsoft.UI.Xaml.Data;

namespace AutoBackup.Converters;

/// <summary>
/// Returns "Edit Backup Job" when the input is true, "Add Backup Job" otherwise.
/// Used to set the ContentDialog title based on <see cref="ViewModels.AddEditJobViewModel.IsEditing"/>.
/// </summary>
public sealed class EditTitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
        => value is true ? "Edit Backup Job" : "Add Backup Job";

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
