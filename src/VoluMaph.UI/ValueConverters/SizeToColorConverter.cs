using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VoluMaph.UI.ValueConverters;

/// <summary>
/// Converts file size values to color brushes based on size.
/// </summary>
public class SizeToColorConverter : IValueConverter
{
    /// <summary>
    /// Converts file size to color brush.
    /// </summary>
    /// <param name="value">The file size in bytes.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>A SolidColorBrush based on file size category.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            return GetSizeColor(size);
        }

        return Brushes.Black;
    }

    /// <summary>
    /// Converts color back to size.
    /// </summary>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the color brush for a given file size.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>A SolidColorBrush representing the size category.</returns>
    private static Brush GetSizeColor(long bytes)
    {
        if (bytes >= 1024L * 1024L * 1024 * 1024)
            return new SolidColorBrush(Color.FromRgb(220, 53, 69));
        if (bytes >= 1024L * 1024L * 1024)
            return new SolidColorBrush(Color.FromRgb(255, 128, 0));
        if (bytes >= 1024L * 1024L * 10)
            return new SolidColorBrush(Color.FromRgb(255, 215, 0));
        if (bytes >= 1024L * 1024L)
            return new SolidColorBrush(Color.FromRgb(46, 204, 113));

        return Brushes.Gray;
    }
}
