using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoluMaph.UI.ValueConverters;

/// <summary>
/// Converts file/folder names to icon strings.
/// </summary>
public class IconConverter : IValueConverter
{
    /// <summary>
    /// Converts file/folder name to icon.
    /// </summary>
    /// <param name="value">The file or folder name.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>File icon ("📄") if name contains a dot; otherwise, folder icon ("📁").</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string name)
        {
            return name.Contains('.') ? "📄" : "📁";
        }
        return "📄";
    }

    /// <summary>
    /// Converts icon back to name.
    /// </summary>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
