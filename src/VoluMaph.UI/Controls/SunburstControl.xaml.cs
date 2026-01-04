using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VoluMaph.Core.Layouts;
using VoluMaph.Core.Color;
using VoluMaph.Core.Model;
using VoluMaph.UI.Behaviors;
using VoluMaph.UI.Models.Visualization;
using WpfPoint = System.Windows.Point;

namespace VoluMaph.UI.Controls;

public partial class SunburstControl : UserControl
{
    public static readonly DependencyProperty ColorMapperProperty =
        DependencyProperty.Register(
            nameof(ColorMapper),
            typeof(IColorMapper),
            typeof(SunburstControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MaxDepthProperty =
        DependencyProperty.Register(
            nameof(MaxDepth),
            typeof(int),
            typeof(SunburstControl),
            new PropertyMetadata(6));

    public static readonly DependencyProperty CenterRadiusProperty =
        DependencyProperty.Register(
            nameof(CenterRadius),
            typeof(double),
            typeof(SunburstControl),
            new PropertyMetadata(50.0));

    public static readonly DependencyProperty MaxRadiusProperty =
        DependencyProperty.Register(
            nameof(MaxRadius),
            typeof(double),
            typeof(SunburstControl),
            new PropertyMetadata(300.0));

    public event EventHandler<SunburstNode>? SelectionChanged;

    private readonly ISunburstLayout _layout;
    private readonly IColorMapper _colorMapper;
    private readonly List<SunburstNode> _nodes;
    private SunburstNode? _selectedNode;

    public IColorMapper ColorMapper
    {
        get => (IColorMapper)GetValue(ColorMapperProperty);
        set => SetValue(ColorMapperProperty, value);
    }

    public int MaxDepth
    {
        get => (int)GetValue(MaxDepthProperty);
        set => SetValue(MaxDepthProperty, value);
    }

    public double CenterRadius
    {
        get => (double)GetValue(CenterRadiusProperty);
        set => SetValue(CenterRadiusProperty, value);
    }

    public double MaxRadius
    {
        get => (double)GetValue(MaxRadiusProperty);
        set => SetValue(MaxRadiusProperty, value);
    }

    public SunburstControl()
    {
        InitializeComponent();

        _layout = new PolarSunburstLayout();
        _colorMapper = new SizeBasedColorMapper();
        _nodes = new List<SunburstNode>();

        ZoomPanBehavior.SetIsEnabled(this, true);

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateSunburst();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSunburst();
    }

    private void UpdateSunburst()
    {
        if (SunburstCanvas == null || ActualWidth == 0 || ActualHeight == 0)
            return;

        SunburstCanvas.Children.Clear();
        _nodes.Clear();

        var rootFolder = DataContext as FolderNode;

        if (rootFolder == null)
            return;

        var segments = _layout.CalculateLayout(rootFolder, CenterRadius, MaxRadius, MaxDepth);

        if (segments == null || segments.Count == 0)
            return;

        var totalSize = rootFolder.Size;

        foreach (var segment in segments)
        {
            if (segment.Node is FolderNode folder)
            {
                var color = ColorMapper?.GetColor(folder.Size, totalSize, ColorTheme.Heatmap)
                    ?? Core.Color.Color.FromArgb(255, 100, 100, 100);

                var wpfBrush = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

                var node = new SunburstNode(
                    folder,
                    segment.StartAngle,
                    segment.EndAngle,
                    segment.InnerRadius,
                    segment.OuterRadius,
                    segment.Depth,
                    wpfBrush,
                    folder.Name);

                _nodes.Add(node);
                SunburstCanvas.Children.Add(CreateSegmentElement(node));
            }
        }

        UpdateBreadcrumb();
    }

    private FrameworkElement CreateSegmentElement(SunburstNode node)
    {
        var path = CreateArcPath(
            node.StartAngle,
            node.EndAngle,
            node.InnerRadius,
            node.OuterRadius);

        path.Fill = node.BackgroundColor;
        path.Stroke = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
        path.StrokeThickness = 0.5;
        path.Tag = node;

        path.MouseEnter += OnSegmentMouseEnter;
        path.MouseLeave += OnSegmentMouseLeave;
        path.MouseLeftButtonUp += OnSegmentMouseLeftButtonUp;

        return path;
    }

    private Path CreateArcPath(double startAngle, double endAngle, double innerRadius, double outerRadius)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            var center = new WpfPoint(ActualWidth / 2, ActualHeight / 2);

            context.BeginFigure(
                PolarToCartesian(center, startAngle, outerRadius),
                false,
                false);

            context.ArcTo(
                PolarToCartesian(center, endAngle, outerRadius),
                new Size(outerRadius, outerRadius),
                0,
                endAngle - startAngle > Math.PI,
                SweepDirection.Clockwise,
                true,
                false);

            context.LineTo(
                PolarToCartesian(center, endAngle, innerRadius),
                true,
                false);

            context.ArcTo(
                PolarToCartesian(center, startAngle, innerRadius),
                new Size(innerRadius, innerRadius),
                0,
                endAngle - startAngle > Math.PI,
                SweepDirection.Counterclockwise,
                true,
                false);

            context.Close();
        }

        geometry.Freeze();

        return new Path { Data = geometry };
    }

    private static WpfPoint PolarToCartesian(WpfPoint center, double angle, double radius)
    {
        var x = center.X + Math.Cos(angle) * radius;
        var y = center.Y + Math.Sin(angle) * radius;
        return new WpfPoint(x, y);
    }

    private void OnSegmentMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Path path && path.Tag is SunburstNode node)
        {
            path.Stroke = new SolidColorBrush(System.Windows.Media.Colors.White);
            path.StrokeThickness = 1.5;

            ShowTooltip(node, e.GetPosition(SunburstCanvas));

            RaiseSelectionChanged(node);
        }
    }

    private void OnSegmentMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Path path && path.Tag is SunburstNode node)
        {
            path.Stroke = new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
            path.StrokeThickness = 0.5;

            HideTooltip();
        }
    }

    private void OnSegmentMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Path path && path.Tag is SunburstNode node)
        {
            var targetNode = node.SourceNode as FolderNode;
            DrillDownToNode(targetNode);
        }
    }

    private void ShowTooltip(SunburstNode node, WpfPoint position)
    {
        if (node.SourceNode is FolderNode folder)
        {
            TooltipName.Text = node.DisplayText;
            TooltipSize.Text = FormatSize(folder.Size);
            TooltipPath.Text = folder.FullPath;

            TooltipBorder.Visibility = Visibility.Visible;
        }
    }

    private void HideTooltip()
    {
        TooltipBorder.Visibility = Visibility.Collapsed;
    }

    private static string FormatSize(long size)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = size;
        int unitIndex = 0;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value:0.##} {units[unitIndex]}";
    }

    private void DrillDownToNode(FolderNode? folder)
    {
        if (folder == null)
            return;

        var node = _nodes.FirstOrDefault(n => n.SourceNode == folder);

        if (node != null)
        {
            _selectedNode = node;
            AnimateSegment(node);
            UpdateBreadcrumb();
        }
    }

    private void AnimateSegment(SunburstNode node)
    {
        var scaleTransform = new ScaleTransform(1.0, 1.0);
        var animation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.1,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    private void UpdateBreadcrumb()
    {
        if (_selectedNode?.SourceNode is not FolderNode folder)
        {
            BreadcrumbBorder.Visibility = Visibility.Collapsed;
            BreadcrumbItems.Items.Clear();
            return;
        }

        BreadcrumbBorder.Visibility = Visibility.Visible;
        BreadcrumbItems.Items.Clear();

        var pathParts = folder.FullPath.Split(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in pathParts)
        {
            var button = new Button
            {
                Content = part,
                Margin = new Thickness(2, 2, 2, 2),
                Padding = new Thickness(4, 2, 4, 2),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            BreadcrumbItems.Items.Add(button);
        }
    }

    private void RaiseSelectionChanged(SunburstNode node)
    {
        SelectionChanged?.Invoke(this, node);
    }
}
