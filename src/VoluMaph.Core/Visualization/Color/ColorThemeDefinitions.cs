namespace VoluMaph.Core.Color;

public static class ColorThemeDefinitions
{
    public static readonly Color[] HeatmapColors = GenerateGradient(256,
        Color.FromRgb(59, 82, 139),
        Color.FromRgb(34, 139, 34),
        Color.FromRgb(255, 215, 0),
        Color.FromRgb(220, 53, 69),
        Color.FromRgb(139, 0, 0));

    public static readonly Color[] DiscreteColors = new Color[]
    {
        Color.FromRgb(230, 159, 0),
        Color.FromRgb(86, 180, 233),
        Color.FromRgb(0, 158, 115),
        Color.FromRgb(240, 228, 66)
    };

    public static readonly Color[] ViridisColors = GenerateGradient(256,
        Color.FromRgb(68, 1, 84),
        Color.FromRgb(59, 82, 139),
        Color.FromRgb(33, 144, 141),
        Color.FromRgb(94, 201, 98),
        Color.FromRgb(253, 231, 37));

    public static readonly Color[] PlasmaColors = GenerateGradient(256,
        Color.FromRgb(13, 8, 135),
        Color.FromRgb(126, 3, 168),
        Color.FromRgb(204, 71, 120),
        Color.FromRgb(248, 149, 64),
        Color.FromRgb(240, 249, 33));

    public static readonly Color[] CoolColors = GenerateGradient(256,
        Color.FromRgb(33, 102, 172),
        Color.FromRgb(66, 146, 198),
        Color.FromRgb(109, 189, 211),
        Color.FromRgb(168, 219, 222),
        Color.FromRgb(222, 243, 246));

    public static readonly Color[] WarmColors = GenerateGradient(256,
        Color.FromRgb(247, 234, 195),
        Color.FromRgb(255, 217, 158),
        Color.FromRgb(254, 178, 107),
        Color.FromRgb(253, 141, 60),
        Color.FromRgb(227, 74, 51));

    public static readonly Color[] ForestColors = GenerateGradient(256,
        Color.FromRgb(27, 48, 31),
        Color.FromRgb(51, 91, 56),
        Color.FromRgb(90, 148, 85),
        Color.FromRgb(145, 196, 125),
        Color.FromRgb(208, 240, 192));

    public static readonly Color[] OceanColors = GenerateGradient(256,
        Color.FromRgb(8, 64, 129),
        Color.FromRgb(49, 130, 189),
        Color.FromRgb(107, 174, 214),
        Color.FromRgb(189, 215, 231),
        Color.FromRgb(239, 243, 255));

    public static readonly Color[] SunsetColors = GenerateGradient(256,
        Color.FromRgb(62, 22, 56),
        Color.FromRgb(145, 40, 63),
        Color.FromRgb(220, 70, 56),
        Color.FromRgb(251, 146, 60),
        Color.FromRgb(254, 224, 139));

    private static Color[] GenerateGradient(int count, params Color[] colors)
    {
        if (colors.Length < 2)
            return colors.Length > 0 ? new[] { colors[0] } : Array.Empty<Color>();

        var result = new Color[count];
        if (count == 1)
        {
            result[0] = colors[0];
            return result;
        }

        int segments = colors.Length - 1;
        int colorsPerSegment = count / segments;

        for (int i = 0; i < segments; i++)
        {
            var start = colors[i];
            var end = colors[i + 1];
            int segmentCount = (i == segments - 1) ? (count - i * colorsPerSegment) : colorsPerSegment;

            for (int j = 0; j < segmentCount; j++)
            {
                double t = segmentCount > 1 ? j / (double)(segmentCount - 1) : 0;
                int index = i * colorsPerSegment + j;
                if (index >= count) continue;

                result[index] = Color.FromRgb(
                    (byte)(start.R + (end.R - start.R) * t),
                    (byte)(start.G + (end.G - start.G) * t),
                    (byte)(start.B + (end.B - start.B) * t));
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the color palette for a specified theme with gradient support.
    /// </summary>
    /// <param name="theme">The color theme to retrieve.</param>
    /// <returns>An array of colors representing the theme palette.</returns>
    public static Color[] GetThemePalette(ColorTheme theme)
    {
        return theme switch
        {
            ColorTheme.Heatmap => HeatmapColors,
            ColorTheme.Discrete => DiscreteColors,
            ColorTheme.ColorblindSafe => new Color[]
            {
                Color.FromRgb(33, 101, 172),
                Color.FromRgb(86, 180, 233),
                Color.FromRgb(0, 158, 115),
                Color.FromRgb(240, 228, 66)
            },
            ColorTheme.Viridis => ViridisColors,
            ColorTheme.Plasma => PlasmaColors,
            ColorTheme.Cool => CoolColors,
            ColorTheme.Warm => WarmColors,
            ColorTheme.Forest => ForestColors,
            ColorTheme.Ocean => OceanColors,
            ColorTheme.Sunset => SunsetColors,
            _ => HeatmapColors
        };
    }
}
