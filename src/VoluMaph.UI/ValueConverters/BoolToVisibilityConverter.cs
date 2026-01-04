using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoluMaph.UI.ValueConverters;

/// <summary>
/// Converts boolean values to Visibility values.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts boolean value to Visibility.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter. Use "Inverse" to reverse the logic.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Visibility.Visible if true; otherwise, Visibility.Collapsed.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var inverse = parameter is string param && param.Equals("Inverse", StringComparison.OrdinalIgnoreCase);
            return boolValue != inverse ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts Visibility back to boolean value.
    /// </summary>
    /// <param name="value">The Visibility value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>True if Visibility.Visible; otherwise, false.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
