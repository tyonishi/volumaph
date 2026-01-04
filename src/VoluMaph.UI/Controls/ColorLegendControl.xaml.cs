using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoluMaph.Core.Color;

namespace VoluMaph.UI.Controls;

public sealed partial class ColorLegendControl : UserControl
{
    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(
            nameof(Theme),
            typeof(ColorTheme),
            typeof(ColorLegendControl),
            new PropertyMetadata(ColorTheme.Heatmap, OnThemeChanged));

    public static readonly DependencyProperty ThemeDescriptionProperty =
        DependencyProperty.Register(
            nameof(ThemeDescription),
            typeof(string),
            typeof(ColorLegendControl),
            new PropertyMetadata(string.Empty));

    public ColorTheme Theme
    {
        get => (ColorTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public string ThemeDescription
    {
        get => (string)GetValue(ThemeDescriptionProperty);
        set => SetValue(ThemeDescriptionProperty, value);
    }

    public ObservableCollection<ColorLegendItem> LegendItems { get; }

    private static readonly Dictionary<ColorTheme, string> ThemeDescriptions = new()
    {
        { ColorTheme.Heatmap, "Blue → Green → Yellow → Red gradient showing file size" },
        { ColorTheme.Discrete, "Four distinct colors for easy differentiation" },
        { ColorTheme.ColorblindSafe, "Blue with varying lightness for accessibility" },
        { ColorTheme.Viridis, "Purple → Blue → Green → Yellow gradient" },
        { ColorTheme.Plasma, "Purple → Pink → Orange → Yellow gradient" },
        { ColorTheme.Cool, "Deep blue → Cyan gradient" },
        { ColorTheme.Warm, "Pale yellow → Orange → Red gradient" },
        { ColorTheme.Forest, "Dark green → Light green gradient" },
        { ColorTheme.Ocean, "Dark blue → Light blue gradient" },
        { ColorTheme.Sunset, "Deep purple → Orange → Yellow gradient" },
        { ColorTheme.ExtensionBased, "Colors based on file extension" }
    };

    public ColorLegendControl()
    {
        InitializeComponent();
        DataContext = this;
        LegendItems = new ObservableCollection<ColorLegendItem>();
        UpdateLegend();
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorLegendControl control)
        {
            control.ThemeDescription = ThemeDescriptions[(ColorTheme)e.NewValue];
            control.UpdateLegend();
        }
    }

    private void UpdateLegend()
    {
        LegendItems.Clear();
        var palette = ColorThemeDefinitions.GetThemePalette(Theme);
        var labels = GetLegendLabels(palette.Length);

        for (var i = 0; i < palette.Length; i++)
        {
            var color = palette[i];
            var brush = new SolidColorBrush(
                System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
            LegendItems.Add(new ColorLegendItem(brush, labels[i]));
        }
    }

    private static string[] GetLegendLabels(int count)
    {
        return count switch
        {
            4 => new[] { "0%", "33%", "66%", "100%" },
            5 => new[] { "0%", "25%", "50%", "75%", "100%" },
            _ => GeneratePercentageLabels(count)
        };
    }

    private static string[] GeneratePercentageLabels(int count)
    {
        var labels = new string[count];
        for (var i = 0; i < count; i++)
        {
            var percentage = (int)((double)i / (count - 1) * 100);
            labels[i] = $"{percentage}%";
        }

        return labels;
    }
}

public sealed class ColorLegendItem
{
    public ColorLegendItem(Brush color, string label)
    {
        Color = color;
        Label = label;
    }

    public Brush Color { get; }
    public string Label { get; }
}
