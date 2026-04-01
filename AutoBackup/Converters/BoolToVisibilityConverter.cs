using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AutoBackup.Converters;

/// <summary>
/// Converts a <see cref="bool"/> to <see cref="Visibility"/>.
/// Also handles nullable booleans and null object references (null → Collapsed).
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        bool visible = value switch
        {
            bool b => b,
            null => false,
            _ => true
        };

        bool invert = parameter is "Invert";
        return (visible ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => value is Visibility.Visible;
}
