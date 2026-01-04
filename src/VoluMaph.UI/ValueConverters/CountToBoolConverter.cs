using System;
using System.Globalization;
using System.Windows.Data;

namespace VoluMaph.UI.ValueConverters;

/// <summary>
/// Converts integer count values to boolean values.
/// </summary>
public class CountToBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts count value to boolean.
    /// </summary>
    /// <param name="value">The integer count value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>True if count is greater than 0; otherwise, false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }

        return false;
    }

    /// <summary>
    /// Converts boolean back to count value.
    /// </summary>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}