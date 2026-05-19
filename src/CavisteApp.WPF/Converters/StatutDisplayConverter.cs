using System.Globalization;
using System.Windows.Data;

namespace CavisteApp.WPF.Converters;

public class StatutDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is null ? "Tous" : value.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}