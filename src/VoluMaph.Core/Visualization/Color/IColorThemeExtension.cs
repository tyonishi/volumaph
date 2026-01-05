namespace VoluMaph.Core.Color;

/// <summary>
/// Defines the contract for custom color theme extensions.
/// Allows plugins to provide custom color palettes for visualization.
/// </summary>
public interface IColorThemeExtension
{
    /// <summary>
    /// Gets the unique identifier for this theme extension.
    /// </summary>
    string ThemeId { get; }

    /// <summary>
    /// Gets the display name for this theme.
    /// </summary>
    string ThemeName { get; }

    /// <summary>
    /// Gets the description for this theme.
    /// </summary>
    string ThemeDescription { get; }

    /// <summary>
    /// Gets the color palette for this theme.
    /// </summary>
    /// <returns>An array of colors representing the theme palette.</returns>
    Color[] GetPalette();

    /// <summary>
    /// Gets a color from the palette based on a percentage value.
    /// </summary>
    /// <param name="percentage">The percentage value (0.0 to 1.0).</param>
    /// <returns>The interpolated color for the given percentage.</returns>
    Color GetColor(double percentage);

    /// <summary>
    /// Determines whether this theme supports gradient interpolation.
    /// </summary>
    bool SupportsGradient { get; }
}
