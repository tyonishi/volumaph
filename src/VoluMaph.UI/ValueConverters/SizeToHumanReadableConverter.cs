using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoluMaph.UI.ValueConverters;

/// <summary>
/// Converts byte values to human-readable size strings.
/// </summary>
public class SizeToHumanReadableConverter : IValueConverter
{
    /// <summary>
    /// Converts byte size to human-readable string.
    /// </summary>
    /// <param name="value">The byte size value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>A human-readable size string (e.g., "1.5 MB").</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            return FormatBytes(size);
        }

        return value;
    }

    /// <summary>
    /// Converts size string back to bytes.
    /// </summary>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Formats bytes into human-readable string.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A human-readable string (e.g., "1.5 MB").</returns>
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:N2} {sizes[order]}";
    }
}
