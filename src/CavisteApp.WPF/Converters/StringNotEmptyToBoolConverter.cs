using System.Globalization;
using System.Windows.Data;

namespace CavisteApp.WPF.Converters;

// Convertit une chaîne de caractères en booléen : true si la chaîne n'est pas vide, false sinon
public class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}