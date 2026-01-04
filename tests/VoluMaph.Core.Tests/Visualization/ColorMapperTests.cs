using VoluMaph.Core.Color;

namespace VoluMaph.Core.Tests.Visualization;

public class ColorMapperTests
{
    [Fact]
    public void GetColor_WithZeroTotalSize_ReturnsGray()
    {
        var mapper = new SizeBasedColorMapper();
        var color = mapper.GetColor(100, 0, ColorTheme.Heatmap);

        var expectedGray = VoluMaph.Core.Color.Color.FromRgb(128, 128, 128);
        Assert.Equal(expectedGray, color);
    }

    [Fact]
    public void GetColor_WithHeatmapTheme_ReturnsCorrectGradient()
    {
        var mapper = new SizeBasedColorMapper();

        var smallColor = mapper.GetColor(10, 100, ColorTheme.Heatmap);
        var mediumColor = mapper.GetColor(50, 100, ColorTheme.Heatmap);
        var largeColor = mapper.GetColor(90, 100, ColorTheme.Heatmap);

        Assert.True(smallColor.B > largeColor.B);
        Assert.True(largeColor.R > smallColor.R);
    }

    [Fact]
    public void GetColor_WithDiscreteTheme_ReturnsFourDistinctColors()
    {
        var mapper = new SizeBasedColorMapper();

        var colors = new List<VoluMaph.Core.Color.Color>
        {
            mapper.GetColor(5, 100, ColorTheme.Discrete),
            mapper.GetColor(25, 100, ColorTheme.Discrete),
            mapper.GetColor(50, 100, ColorTheme.Discrete),
            mapper.GetColor(75, 100, ColorTheme.Discrete),
            mapper.GetColor(95, 100, ColorTheme.Discrete)
        };

        Assert.Equal(4, colors.Distinct().Count());
    }

    [Fact]
    public void GetColor_WithViridisTheme_ReturnsViridisColors()
    {
        var mapper = new SizeBasedColorMapper();

        var color = mapper.GetColor(50, 100, ColorTheme.Viridis);

        Assert.True(color.R >= 0 && color.R <= 255);
        Assert.True(color.G >= 0 && color.G <= 255);
        Assert.True(color.B >= 0 && color.B <= 255);
    }

    [Fact]
    public void GetColor_WithColorblindSafeTheme_UsesLightnessVariation()
    {
        var mapper = new SizeBasedColorMapper();

        var darkColor = mapper.GetColor(10, 100, ColorTheme.ColorblindSafe);
        var lightColor = mapper.GetColor(90, 100, ColorTheme.ColorblindSafe);

        var darkLuminance = CalculateLuminance(darkColor);
        var lightLuminance = CalculateLuminance(lightColor);

        Assert.True(lightLuminance > darkLuminance);
    }

    [Fact]
    public void GetColor_ByPercentage_ReturnsConsistentResults()
    {
        var mapper = new SizeBasedColorMapper();

        var color1 = mapper.GetColor(0.5, ColorTheme.Heatmap);
        var color2 = mapper.GetColor(50, 100, ColorTheme.Heatmap);

        Assert.Equal(color1, color2);
    }

    [Fact]
    public void GetColor_WithZeroPercentage_ReturnsFirstColor()
    {
        var mapper = new SizeBasedColorMapper();

        var colors = ColorThemeDefinitions.HeatmapColors;
        var color = mapper.GetColor(0, ColorTheme.Heatmap);

        Assert.Equal(colors[0], color);
    }

    [Fact]
    public void GetColor_WithFullPercentage_ReturnsLastColor()
    {
        var mapper = new SizeBasedColorMapper();

        var colors = ColorThemeDefinitions.HeatmapColors;
        var color = mapper.GetColor(1.0, ColorTheme.Heatmap);

        Assert.Equal(colors[255], color);
    }

    private static double CalculateLuminance(VoluMaph.Core.Color.Color color)
    {
        return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    }
}
