using VoluMaph.Core.Analysis;

namespace VoluMaph.Infrastructure.Settings;

/// <summary>
/// Application settings.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Gets or sets the last selected drive.
    /// </summary>
    public string LastSelectedDrive { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default sort option.
    /// </summary>
    public SortOption DefaultSortOption { get; set; } = SortOption.BySizeDescending;

    /// <summary>
    /// Gets or sets a value indicating whether dark theme is enabled.
    /// </summary>
    public bool IsDarkTheme { get; set; } = true;
}
