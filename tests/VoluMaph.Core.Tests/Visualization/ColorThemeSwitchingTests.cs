using Xunit;
using ColorType = VoluMaph.Core.Color.Color;
using VoluMaph.Core.Color;

namespace VoluMaph.Core.Tests.Visualization;

public class ColorThemeSwitchingTests
{
    [Fact]
    public void GetThemePalette_WithHeatmap_ReturnsGradient()
    {
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Heatmap);

        Assert.Equal(256, palette.Length);
        Assert.Equal(ColorType.FromRgb(59, 82, 139), palette[0]);
        Assert.Equal(ColorType.FromRgb(139, 0, 0), palette[255]);
    }

    [Fact]
    public void GetThemePalette_WithCoolTheme_ReturnsCoolGradient()
    {
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Cool);

        Assert.Equal(256, palette.Length);
        Assert.Equal(ColorType.FromRgb(33, 102, 172), palette[0]);
        Assert.Equal(ColorType.FromRgb(222, 243, 246), palette[255]);
    }

    [Fact]
    public void GetThemePalette_WithWarmTheme_ReturnsWarmGradient()
    {
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Warm);

        Assert.Equal(256, palette.Length);
        Assert.Equal(ColorType.FromRgb(247, 234, 195), palette[0]);
        Assert.Equal(ColorType.FromRgb(227, 74, 51), palette[255]);
    }

    [Fact]
    public void GetThemePalette_WithForestTheme_ReturnsGreenGradient()
    {
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Forest);

        Assert.Equal(256, palette.Length);
        Assert.Equal(ColorType.FromRgb(27, 48, 31), palette[0]);
        Assert.Equal(ColorType.FromRgb(208, 240, 192), palette[255]);
    }

    [Fact]
    public void GetThemePalette_WithOceanTheme_ReturnsBlueGradient()
    {
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Ocean);

        Assert.Equal(256, palette.Length);
        Assert.Equal(ColorType.FromRgb(8, 64, 129), palette[0]);
        Assert.Equal(ColorType.FromRgb(239, 243, 255), palette[255]);
    }

    [Fact]
    public void GetThemePalette_WithSunsetTheme_ReturnsSunsetGradient()
    {
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Sunset);

        Assert.Equal(256, palette.Length);
        Assert.Equal(ColorType.FromRgb(62, 22, 56), palette[0]);
        Assert.Equal(ColorType.FromRgb(254, 224, 139), palette[255]);
    }

    [Fact]
    public void GetColor_WithCoolTheme_ReturnsInterpolatedColors()
    {
        var mapper = new SizeBasedColorMapper();
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Cool);

        var color1 = mapper.GetColor(0.0, ColorTheme.Cool);
        var color3 = mapper.GetColor(1.0, ColorTheme.Cool);

        Assert.Equal(palette[0], color1);
        Assert.Equal(palette[255], color3);
    }

    [Fact]
    public void GetColor_WithWarmTheme_ReturnsInterpolatedColors()
    {
        var mapper = new SizeBasedColorMapper();
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Warm);

        var color1 = mapper.GetColor(0.0, ColorTheme.Warm);
        var color3 = mapper.GetColor(1.0, ColorTheme.Warm);

        Assert.Equal(palette[0], color1);
        Assert.Equal(palette[255], color3);
    }

    [Fact]
    public void GetColor_WithForestTheme_ReturnsInterpolatedColors()
    {
        var mapper = new SizeBasedColorMapper();
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Forest);

        var color1 = mapper.GetColor(0.0, ColorTheme.Forest);
        var color3 = mapper.GetColor(1.0, ColorTheme.Forest);

        Assert.Equal(palette[0], color1);
        Assert.Equal(palette[255], color3);
    }

    [Fact]
    public void GetColor_WithOceanTheme_ReturnsInterpolatedColors()
    {
        var mapper = new SizeBasedColorMapper();
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Ocean);

        var color1 = mapper.GetColor(0.0, ColorTheme.Ocean);
        var color3 = mapper.GetColor(1.0, ColorTheme.Ocean);

        Assert.Equal(palette[0], color1);
        Assert.Equal(palette[255], color3);
    }

    [Fact]
    public void GetColor_WithSunsetTheme_ReturnsInterpolatedColors()
    {
        var mapper = new SizeBasedColorMapper();
        var palette = ColorThemeDefinitions.GetThemePalette(ColorTheme.Sunset);

        var color1 = mapper.GetColor(0.0, ColorTheme.Sunset);
        var color3 = mapper.GetColor(1.0, ColorTheme.Sunset);

        Assert.Equal(palette[0], color1);
        Assert.Equal(palette[255], color3);
    }

    [Fact]
    public void GetColor_WithMultipleThemes_ReturnsDifferentColors()
    {
        var mapper = new SizeBasedColorMapper();

        var heatColor = mapper.GetColor(0.5, ColorTheme.Heatmap);
        var coolColor = mapper.GetColor(0.5, ColorTheme.Cool);
        var warmColor = mapper.GetColor(0.5, ColorTheme.Warm);
        var forestColor = mapper.GetColor(0.5, ColorTheme.Forest);
        var oceanColor = mapper.GetColor(0.5, ColorTheme.Ocean);
        var sunsetColor = mapper.GetColor(0.5, ColorTheme.Sunset);

        var colors = new[] { heatColor, coolColor, warmColor, forestColor, oceanColor, sunsetColor };
        var distinctColors = colors.Distinct().Count();

        Assert.True(distinctColors >= 4);
    }
}
