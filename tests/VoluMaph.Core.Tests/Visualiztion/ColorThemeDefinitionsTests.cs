using Xunit;
using VoluMaph.Core.Color;
using System.Collections.Generic;

namespace VoluMaph.Core.Tests.Visualization;

public sealed class ColorThemeDefinitionsTests
{
    [Fact]
    public void CoolTheme_HasCorrectColorCount()
    {
        var colors = ColorThemeDefinitions.CoolColors;
        Assert.NotNull(colors);
        Assert.Equal(256, colors.Length);
    }

    [Fact]
    public void CoolTheme_ColorsAreOrderedCorrectly()
    {
        var colors = ColorThemeDefinitions.CoolColors;
        Assert.Equal(256, colors.Length);

        for (int i = 0; i < colors.Length; i++)
        {
            var prevColor = colors[i];
            var nextColor = i < colors.Length - 1 ? colors[i + 1] : colors[0];

            Assert.True(prevColor.A <= nextColor.A);
            Assert.True(prevColor.R <= nextColor.R);
            Assert.True(prevColor.G <= nextColor.G);
            Assert.True(prevColor.B <= nextColor.B);
        }
    }

    [Fact]
    public void CoolTheme_ColorsHaveConsistentGradient()
    {
        var colors = ColorThemeDefinitions.CoolColors;

        for (int i = 0; i < 10; i++)
        {
            var color = colors[i * 2];
            Assert.InRange(color.A, 0, 255);
            Assert.InRange(color.R, 0, 255);
            Assert.InRange(color.G, 0, 255);
            Assert.InRange(color.B, 0, 255);
        }
    }

    [Fact]
    public void WarmTheme_HasCorrectColorCount()
    {
        var colors = ColorThemeDefinitions.WarmColors;
        Assert.NotNull(colors);
        Assert.Equal(256, colors.Length);
    }

    [Fact]
    public void WarmTheme_ColorsAreOrderedCorrectly()
    {
        var colors = ColorThemeDefinitions.WarmColors;
        Assert.Equal(256, colors.Length);

        for (int i = 0; i < colors.Length; i++)
        {
            var prevColor = colors[i];
            var nextColor = i < colors.Length - 1 ? colors[i + 1] : colors[0];

            Assert.True(prevColor.A <= nextColor.A);
            Assert.True(prevColor.R <= nextColor.R);
            Assert.True(prevColor.G <= nextColor.G);
            Assert.True(prevColor.B <= nextColor.B);
        }
    }

    [Fact]
    public void WarmTheme_ColorsHaveConsistentGradient()
    {
        var colors = ColorThemeDefinitions.WarmColors;

        for (int i = 0; i < 10; i++)
        {
            var color = colors[i * 2];
            Assert.InRange(color.A, 0, 255);
            Assert.InRange(color.R, 0, 255);
            Assert.InRange(color.G, 0, 255);
            Assert.InRange(color.B, 0, 255);
        }
    }

    [Fact]
    public void ForestTheme_HasCorrectColorCount()
    {
        var colors = ColorThemeDefinitions.ForestColors;
        Assert.NotNull(colors);
        Assert.Equal(256, colors.Length);
    }

    [Fact]
    public void ForestTheme_ColorsAreOrderedCorrectly()
    {
        var colors = ColorThemeDefinitions.ForestColors;
        Assert.Equal(256, colors.Length);

        for (int i = 0; i < colors.Length; i++)
        {
            var prevColor = colors[i];
            var nextColor = i < colors.Length - 1 ? colors[i + 1] : colors[0];

            Assert.True(prevColor.A <= nextColor.A);
            Assert.True(prevColor.R <= nextColor.R);
            Assert.True(prevColor.G <= nextColor.G);
            Assert.True(prevColor.B <= nextColor.B);
        }
    }

    [Fact]
    public void ForestTheme_ColorsHaveConsistentGradient()
    {
        var colors = ColorThemeDefinitions.ForestColors;

        for (int i = 0; i < 10; i++)
        {
            var color = colors[i * 2];
            Assert.InRange(color.A, 0, 255);
            Assert.InRange(color.R, 0, 255);
            Assert.InRange(color.G, 0, 255);
            Assert.InRange(color.B, 0, 255);
            Assert.InRange(color.B, 0, 255);
        }
    }

    [Fact]
    public void OceanTheme_HasCorrectColorCount()
    {
        var colors = ColorThemeDefinitions.OceanColors;
        Assert.NotNull(colors);
        Assert.Equal(256, colors.Length);
    }

    [Fact]
    public void OceanTheme_ColorsAreOrderedCorrectly()
    {
        var colors = ColorThemeDefinitions.OceanColors;
        Assert.Equal(256, colors.Length);

        for (int i = 0; i < colors.Length; i++)
        {
            var prevColor = colors[i];
            var nextColor = i < colors.Length - 1 ? colors[i + 1] : colors[0];

            Assert.True(prevColor.A <= nextColor.A);
            Assert.True(prevColor.R <= nextColor.R);
            Assert.True(prevColor.G <= nextColor.G);
            Assert.True(prevColor.B <= nextColor.B);
        }
    }

    [Fact]
    public void OceanTheme_ColorsHaveConsistentGradient()
    {
        var colors = ColorThemeDefinitions.OceanColors;

        for (int i = 0; i < 10; i++)
        {
            var color = colors[i * 2];
            Assert.InRange(color.A, 0, 255);
            Assert.InRange(color.R, 0, 255);
            Assert.InRange(color.G, 0, 255);
            Assert.InRange(color.B, 0, 255);
            Assert.InRange(color.B, 0, 255);
        }
    }

    [Fact]
    public void SunsetTheme_HasCorrectColorCount()
    {
        var colors = ColorThemeDefinitions.SunsetColors;
        Assert.NotNull(colors);
        Assert.Equal(256, colors.Length);
    }

    [Fact]
    public void SunsetTheme_ColorsAreOrderedCorrectly()
    {
        var colors = ColorThemeDefinitions.SunsetColors;
        Assert.Equal(256, colors.Length);

        for (int i = 0; i < colors.Length; i++)
        {
            var prevColor = colors[i];
            var nextColor = i < colors.Length - 1 ? colors[i + 1] : colors[0];

            Assert.True(prevColor.A <= nextColor.A);
            Assert.True(prevColor.R <= nextColor.R);
            Assert.True(prevColor.G <= nextColor.G);
            Assert.True(prevColor.B <= nextColor.B);
            Assert.True(prevColor.B <= nextColor.B);
        }
    }

    [Fact]
    public void SunsetTheme_ColorsHaveConsistentGradient()
    {
        var colors = ColorThemeDefinitions.SunsetColors;

        for (int i = 0; i < 10; i++)
        {
            var color = colors[i * 2];
            Assert.InRange(color.A, 0, 255);
            Assert.InRange(color.R, 0, 255);
            Assert.InRange(color.G, 0, 255);
            Assert.InRange(color.B, 0, 255);
            Assert.InRange(color.B, 0, 255);
        }
    }

    [Fact]
    public void AllThemes_HaveUniqueColors()
    {
        var themes = new[]
        {
            ColorTheme.Heatmap,
            ColorTheme.Cool,
            ColorTheme.Warm,
            ColorTheme.Forest,
            ColorTheme.Ocean,
            ColorTheme.Sunset
        };

        var allColors = new List<VoluMaph.Core.Color.Color>();
        foreach (var theme in themes)
        {
            var palette = ColorThemeDefinitions.GetThemePalette(theme);
            allColors.AddRange(palette);
        }

        var uniqueColors = allColors.Distinct().ToList();
        Assert.True(uniqueColors.Count > 0);
    }

    [Fact]
    public void GetColorByValue_ReturnsCorrectColor()
    {
        var mapper = new SizeBasedColorMapper();
        var color = mapper.GetColor(1000000000, 500000000, ColorTheme.Heatmap);
        Assert.True(color.R > 0 && color.R < 255);
        Assert.True(color.G > 0 && color.G < 255);
        Assert.True(color.B > 0 && color.B < 255);
    }

    [Fact]
    public void GetColorByValue_ReturnsCorrectColorForDifferentSizes()
    {
        var mapper = new SizeBasedColorMapper();

        var colorSmall = mapper.GetColor(100000, 50000, ColorTheme.Heatmap);
        var colorMedium = mapper.GetColor(10000000000, 500000000, ColorTheme.Heatmap);
        var colorLarge = mapper.GetColor(10000000000, 5000000000, ColorTheme.Heatmap);

        Assert.Equal(colorSmall.R, colorMedium.R);
        Assert.Equal(colorMedium.R, colorLarge.R);
        Assert.Equal(colorSmall.G, colorMedium.G);
        Assert.Equal(colorSmall.B, colorLarge.B);
    }

    [Fact]
    public void GetColorByValue_WithCustomMaxSize_ReturnsAdjustedColor()
    {
        var mapper = new SizeBasedColorMapper();
        var color1 = mapper.GetColor(50000000, 100000000, ColorTheme.Heatmap);
        var color2 = mapper.GetColor(50000000, 1000000000, ColorTheme.Heatmap);

        Assert.NotEqual(color1, color2);
        Assert.True(color1.R > color2.R && color1.R < 255);
        Assert.True(color1.G > color2.G && color1.G < 255);
        Assert.True(color1.B > color2.B && color1.B < 255);
    }
}
