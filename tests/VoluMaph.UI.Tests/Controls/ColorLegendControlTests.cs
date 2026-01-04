using System.Windows;
using System.Windows.Media;
using VoluMaph.Core.Color;
using VoluMaph.UI.Controls;

namespace VoluMaph.UI.Tests.Controls;

public class ColorLegendControlTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultTheme()
    {
        var control = new ColorLegendControl();

        Assert.Equal(ColorTheme.Heatmap, control.Theme);
        Assert.NotNull(control.LegendItems);
        Assert.Equal(5, control.LegendItems.Count);
    }

    [Fact]
    public void Constructor_InitializesLegendItems()
    {
        var control = new ColorLegendControl();

        Assert.NotNull(control.LegendItems);
        Assert.Equal(5, control.LegendItems.Count);

        var firstItem = control.LegendItems[0];
        Assert.NotNull(firstItem.Color);
        Assert.Equal("0%", firstItem.Label);
    }

    [Fact]
    public void ThemeProperty_WhenChanged_UpdatesLegendItems()
    {
        var control = new ColorLegendControl();
        control.Theme = ColorTheme.Cool;

        Assert.Equal(5, control.LegendItems.Count);

        var firstColor = ((SolidColorBrush)control.LegendItems[0].Color).Color;
        Assert.Equal(33, firstColor.R);
        Assert.Equal(102, firstColor.G);
        Assert.Equal(172, firstColor.B);
    }

    [Fact]
    public void ThemeProperty_WhenSetToWarm_UpdatesLegendColors()
    {
        var control = new ColorLegendControl();
        control.Theme = ColorTheme.Warm;

        Assert.Equal(5, control.LegendItems.Count);

        var lastColor = ((SolidColorBrush)control.LegendItems[4].Color).Color;
        Assert.Equal(227, lastColor.R);
        Assert.Equal(74, lastColor.G);
        Assert.Equal(51, lastColor.B);
    }

    [Fact]
    public void ThemeDescription_WhenThemeChanges_UpdatesDescription()
    {
        var control = new ColorLegendControl();

        control.Theme = ColorTheme.Forest;
        Assert.Contains("Dark green → Light green gradient", control.ThemeDescription);

        control.Theme = ColorTheme.Ocean;
        Assert.Contains("Dark blue → Light blue gradient", control.ThemeDescription);
    }

    [Fact]
    public void LegendItems_WithFourColorTheme_HasCorrectLabels()
    {
        var control = new ColorLegendControl();
        control.Theme = ColorTheme.Discrete;

        Assert.Equal(4, control.LegendItems.Count);
        Assert.Equal("0%", control.LegendItems[0].Label);
        Assert.Equal("33%", control.LegendItems[1].Label);
        Assert.Equal("66%", control.LegendItems[2].Label);
        Assert.Equal("100%", control.LegendItems[3].Label);
    }

    [Fact]
    public void LegendItems_WithFiveColorTheme_HasCorrectLabels()
    {
        var control = new ColorLegendControl();
        control.Theme = ColorTheme.Heatmap;

        Assert.Equal(5, control.LegendItems.Count);
        Assert.Equal("0%", control.LegendItems[0].Label);
        Assert.Equal("25%", control.LegendItems[1].Label);
        Assert.Equal("50%", control.LegendItems[2].Label);
        Assert.Equal("75%", control.LegendItems[3].Label);
        Assert.Equal("100%", control.LegendItems[4].Label);
    }

    [Fact]
    public void ThemeProperty_WithAllThemes_UpdatesCorrectly()
    {
        var control = new ColorLegendControl();
        var themes = new[]
        {
            ColorTheme.Heatmap,
            ColorTheme.Discrete,
            ColorTheme.ColorblindSafe,
            ColorTheme.Viridis,
            ColorTheme.Plasma,
            ColorTheme.Cool,
            ColorTheme.Warm,
            ColorTheme.Forest,
            ColorTheme.Ocean,
            ColorTheme.Sunset
        };

        foreach (var theme in themes)
        {
            control.Theme = theme;
            Assert.Equal(theme, control.Theme);
            Assert.True(control.LegendItems.Count >= 4);
        }
    }

    [Fact]
    public void ThemeProperty_WhenChangedMultipleTimes_UpdatesLegendItemsCorrectly()
    {
        var control = new ColorLegendControl();

        control.Theme = ColorTheme.Cool;
        var coolFirstItem = control.LegendItems[0];

        control.Theme = ColorTheme.Warm;
        var warmFirstItem = control.LegendItems[0];

        control.Theme = ColorTheme.Cool;
        var coolFirstItemAgain = control.LegendItems[0];

        Assert.Equal(coolFirstItem.Label, coolFirstItemAgain.Label);
    }

    [Fact]
    public void ThemeDescription_WithAllThemes_HasCorrectDescriptions()
    {
        var control = new ColorLegendControl();

        control.Theme = ColorTheme.Heatmap;
        Assert.Contains("gradient showing file size", control.ThemeDescription);

        control.Theme = ColorTheme.Discrete;
        Assert.Contains("distinct colors", control.ThemeDescription);

        control.Theme = ColorTheme.ColorblindSafe;
        Assert.Contains("accessibility", control.ThemeDescription);

        control.Theme = ColorTheme.Viridis;
        Assert.Contains("Purple", control.ThemeDescription);

        control.Theme = ColorTheme.Sunset;
        Assert.Contains("Deep purple", control.ThemeDescription);
    }

    [Fact]
    public void LegendItems_ColorsAreValidBrushes()
    {
        var control = new ColorLegendControl();
        control.Theme = ColorTheme.Viridis;

        foreach (var item in control.LegendItems)
        {
            Assert.NotNull(item.Color);
            Assert.IsType<SolidColorBrush>(item.Color);

            var solidBrush = (SolidColorBrush)item.Color;
            Assert.InRange(solidBrush.Color.R, 0, 255);
            Assert.InRange(solidBrush.Color.G, 0, 255);
            Assert.InRange(solidBrush.Color.B, 0, 255);
        }
    }
}
