using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CavisteApp.WPF.Converters
{
    /// <summary>
    /// null  -> Visible   (aucune vue chargée : on affiche l'animation)
    /// !null -> Collapsed (une vue est chargée : on cache l'animation)
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
