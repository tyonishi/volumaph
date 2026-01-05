using System;
using System.Collections.ObjectModel;
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

namespace VoluMaph.UI.Controls;

public partial class TreemapControl : UserControl
{
    public static readonly DependencyProperty NodesProperty =
        DependencyProperty.Register(
            nameof(Nodes),
            typeof(ObservableCollection<TreemapNode>),
            typeof(TreemapControl),
            new PropertyMetadata(null, OnNodesPropertyChanged));

    public static readonly DependencyProperty ColorMapperProperty =
        DependencyProperty.Register(
            nameof(ColorMapper),
            typeof(IColorMapper),
            typeof(TreemapControl),
            new PropertyMetadata(null));

     public static readonly DependencyProperty MaxDepthProperty =
        DependencyProperty.Register(
            nameof(MaxDepth),
            typeof(int),
            typeof(TreemapControl),
            new PropertyMetadata(4));

    public static readonly DependencyProperty MinDisplaySizeProperty =
        DependencyProperty.Register(
            nameof(MinDisplaySize),
            typeof(double),
            typeof(TreemapControl),
            new PropertyMetadata(500.0));

    public static readonly DependencyProperty ColorThemeProperty =
        DependencyProperty.Register(
            nameof(ColorTheme),
            typeof(ColorTheme),
            typeof(TreemapControl),
            new PropertyMetadata(ColorTheme.Heatmap, (d, e) =>
            {
                if (d is TreemapControl control)
                {
                    control.UpdateTreemap();
                }
            }));

    public event EventHandler<TreemapNode>? SelectionChanged;

    private readonly ITreemapLayout _layout;
    private readonly IColorMapper _colorMapper;

    public ObservableCollection<TreemapNode> Nodes
    {
        get => (ObservableCollection<TreemapNode>)GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

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

    public double MinDisplaySize
    {
        get => (double)GetValue(MinDisplaySizeProperty);
        set => SetValue(MinDisplaySizeProperty, value);
    }

    public ColorTheme ColorTheme
    {
        get => (ColorTheme)GetValue(ColorThemeProperty);
        set => SetValue(ColorThemeProperty, value);
    }

    public TreemapControl()
    {
        InitializeComponent();

        _layout = new SquarifiedTreemapLayout();
        _colorMapper = new SizeBasedColorMapper();
        ColorMapper = _colorMapper;

        ZoomPanBehavior.SetIsEnabled(TreemapCanvas, true);

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateTreemap();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateTreemap();
    }

    private static void OnNodesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreemapControl control)
        {
            control.UpdateTreemap();
        }
    }

    private void UpdateTreemap()
    {
        if (TreemapCanvas == null || ActualWidth == 0 || ActualHeight == 0)
            return;

        // Memory optimization: clear old elements first
        var oldChildren = TreemapCanvas.Children.Cast<object>().ToList();
        TreemapCanvas.Children.Clear();
        oldChildren.Clear();

        var rootFolder = DataContext as FolderNode;

        if (rootFolder == null)
            return;

        var bounds = new Core.Layouts.Rect(0, 0, ActualWidth, ActualHeight);
        CalculateTreemapRecursive(rootFolder, bounds, 0, rootFolder.Size);
        
        // Force garbage collection for large datasets
        if (TreemapCanvas.Children.Count > 1000)
        {
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }

    private void CalculateTreemapRecursive(
        FolderNode folder,
        Core.Layouts.Rect bounds,
        int depth,
        long totalSize)
    {
        if (depth >= MaxDepth || folder.Size == 0)
            return;

        if (bounds.Width * bounds.Height < MinDisplaySize)
            return;

        var color = ColorMapper?.GetColor(folder.Size, totalSize, ColorTheme)
            ?? Core.Color.Color.FromArgb(255, 100, 100, 100);

        var wpfBrush = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

        var rectStruct = new Models.Visualization.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        var node = new TreemapNode(folder, rectStruct, depth, wpfBrush, folder.Name);
        TreemapCanvas.Children.Add(CreateNodeElement(node));

        if (depth < MaxDepth)
        {
            var sortedChildren = folder.Children
                .Where(n => n.Size > 0)
                .OrderByDescending(n => n.Size)
                .ToList();

            if (sortedChildren.Count == 0)
                return;

            var childrenBounds = _layout.CalculateLayout(sortedChildren, bounds);

            if (childrenBounds == null || childrenBounds.Count == 0)
                return;

            foreach (var treemapRect in childrenBounds)
            {
                if (treemapRect.Node is FolderNode subFolder)
                {
                    CalculateTreemapRecursive(subFolder, treemapRect.Bounds, depth + 1, totalSize);
                }
            }
        }
    }

    private FrameworkElement CreateNodeElement(TreemapNode node)
    {
        var rectangle = new Rectangle
        {
            Width = node.Bounds.Width,
            Height = node.Bounds.Height,
            Fill = node.BackgroundColor,
            Stroke = new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(50, 0, 0, 0)),
            StrokeThickness = 0.5,
            RadiusX = node.Bounds.Width < 20 ? 2 : 0,
            RadiusY = node.Bounds.Height < 20 ? 2 : 0,
            Tag = node
        };

        rectangle.MouseEnter += OnRectangleMouseEnter;
        rectangle.MouseLeave += OnRectangleMouseLeave;
        rectangle.MouseLeftButtonUp += OnRectangleMouseLeftButtonUp;

        return rectangle;
    }

    private void OnRectangleMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Rectangle rectangle && rectangle.Tag is TreemapNode node)
        {
            rectangle.Stroke = new SolidColorBrush(System.Windows.Media.Colors.White);
            rectangle.StrokeThickness = 1.5;

            // Show tooltip near the mouse (convert canvas coords to control coords)
            var posOnCanvas = e.GetPosition(TreemapCanvas);
            ShowTooltip(node, posOnCanvas);

            RaiseSelectionChanged(node);
        }
    }

    private void OnRectangleMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Rectangle rectangle && rectangle.Tag is TreemapNode node)
        {
            rectangle.Stroke = new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(50, 0, 0, 0));
            rectangle.StrokeThickness = 0.5;

            HideTooltip();
        }
    }

    private void OnRectangleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is Rectangle rectangle && rectangle.Tag is TreemapNode node)
        {
            var targetNode = node.SourceNode as FolderNode;
            ZoomToNode(targetNode);
        }
    }

    private void ZoomToNode(FolderNode? folder)
    {
        if (folder == null || TreemapCanvas == null)
            return;

        SelectionChanged?.Invoke(this, null);
    }

    private void ShowTooltip(TreemapNode node, System.Windows.Point position)
    {
        if (node?.SourceNode != null)
        {
            TooltipName.Text = node.DisplayText;
            TooltipSize.Text = FormatSize(node.SourceNode.Size);
            TooltipPath.Text = node.SourceNode.FullPath;

            // Convert position on TreemapCanvas to position relative to this control
            var relative = TreemapCanvas.TransformToVisual(this).Transform(position);

            // Position tooltip with offset and clamp to control bounds when possible
            double left = relative.X + 10;
            double top = relative.Y + 10;

            // If ActualWidth/Height are available, clamp to avoid overflow
            if (TooltipBorder.ActualWidth > 0 && TooltipBorder.ActualHeight > 0)
            {
                if (left + TooltipBorder.ActualWidth > ActualWidth)
                {
                    left = Math.Max(0, ActualWidth - TooltipBorder.ActualWidth - 10);
                }
                if (top + TooltipBorder.ActualHeight > ActualHeight)
                {
                    top = Math.Max(0, ActualHeight - TooltipBorder.ActualHeight - 10);
                }
            }

            TooltipBorder.Margin = new Thickness(left, top, 0, 0);
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

    private void RaiseSelectionChanged(TreemapNode node)
    {
        SelectionChanged?.Invoke(this, node);
    }
}
