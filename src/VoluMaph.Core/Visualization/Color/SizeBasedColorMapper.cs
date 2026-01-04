namespace VoluMaph.Core.Color;

public sealed class SizeBasedColorMapper : IColorMapper
{
    public Color GetColor(long size, long totalSize, ColorTheme theme)
    {
        if (totalSize == 0)
            return Color.FromRgb(128, 128, 128);

        var percentage = (double)size / totalSize;
        return GetColor(percentage, theme);
    }

    public Color GetColor(double percentage, ColorTheme theme)
    {
        return theme switch
        {
            ColorTheme.Heatmap => GetHeatmapColor(percentage),
            ColorTheme.Discrete => GetDiscreteColor(percentage),
            ColorTheme.ColorblindSafe => GetColorblindSafeColor(percentage),
            ColorTheme.Viridis => GetViridisColor(percentage),
            ColorTheme.Plasma => GetPlasmaColor(percentage),
            _ => GetHeatmapColor(percentage)
        };
    }

    private Color GetHeatmapColor(double percentage)
    {
        var colors = ColorThemeDefinitions.HeatmapColors;
        var index = Math.Min(
            (int)(percentage * (colors.Length - 1)),
            colors.Length - 1);

        if (index < colors.Length - 1)
        {
            var nextIndex = index + 1;
            var localPercentage = (percentage * (colors.Length - 1)) - index;
            return InterpolateColor(colors[index], colors[nextIndex], localPercentage);
        }

        return colors[index];
    }

    private Color GetDiscreteColor(double percentage)
    {
        var colors = ColorThemeDefinitions.DiscreteColors;
        var index = Math.Min(
            (int)(percentage * colors.Length),
            colors.Length - 1);
        return colors[index];
    }

    private Color GetColorblindSafeColor(double percentage)
    {
        var baseHue = 240;
        var saturation = 80;
        var lightness = 30 + (percentage * 50);

        return HslToRgb(baseHue, saturation, lightness);
    }

    private Color GetViridisColor(double percentage)
    {
        return InterpolateFromPalette(percentage, ColorThemeDefinitions.ViridisColors);
    }

    private Color GetPlasmaColor(double percentage)
    {
        return InterpolateFromPalette(percentage, ColorThemeDefinitions.PlasmaColors);
    }

    private Color InterpolateFromPalette(double percentage, Color[] colors)
    {
        var index = Math.Min(
            (int)(percentage * (colors.Length - 1)),
            colors.Length - 1);

        if (index < colors.Length - 1)
        {
            var nextIndex = index + 1;
            var localPercentage = (percentage * (colors.Length - 1)) - index;
            return InterpolateColor(colors[index], colors[nextIndex], localPercentage);
        }

        return colors[index];
    }

    private static Color InterpolateColor(Color color1, Color color2, double percentage)
    {
        var r = (byte)(color1.R + (color2.R - color1.R) * percentage);
        var g = (byte)(color1.G + (color2.G - color1.G) * percentage);
        var b = (byte)(color1.B + (color2.B - color1.B) * percentage);
        return Color.FromRgb(r, g, b);
    }

    private static Color HslToRgb(double h, double s, double l)
    {
        s /= 100;
        l /= 100;

        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        var m = l - c / 2;

        double r, g, b;

        if (h < 60)
        {
            r = c; g = x; b = 0;
        }
        else if (h < 120)
        {
            r = x; g = c; b = 0;
        }
        else if (h < 180)
        {
            r = 0; g = c; b = x;
        }
        else if (h < 240)
        {
            r = 0; g = x; b = c;
        }
        else if (h < 300)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }
}
