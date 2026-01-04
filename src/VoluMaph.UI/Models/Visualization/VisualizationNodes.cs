using System.Windows.Media;
using VoluMaph.Core.Model;
using VoluMaph.Core.Color;

namespace VoluMaph.UI.Models.Visualization;

public sealed class TreemapNode
{
    public FileSystemNode SourceNode { get; }
    public Rect Bounds { get; }
    public int Depth { get; }
    public Brush BackgroundColor { get; }
    public string DisplayText { get; }
    public bool IsVisible => BackgroundColor != Brushes.Transparent;

    public TreemapNode(
        FileSystemNode sourceNode,
        Rect bounds,
        int depth,
        Brush backgroundColor,
        string displayText)
    {
        SourceNode = sourceNode;
        Bounds = bounds;
        Depth = depth;
        BackgroundColor = backgroundColor;
        DisplayText = displayText;
    }
}

public sealed class SunburstNode
{
    public FileSystemNode SourceNode { get; }
    public double StartAngle { get; }
    public double EndAngle { get; }
    public double InnerRadius { get; }
    public double OuterRadius { get; }
    public int Depth { get; }
    public Brush BackgroundColor { get; }
    public string DisplayText { get; }
    public Point CenterPoint { get; }
    public bool IsVisible => BackgroundColor != Brushes.Transparent;

    public SunburstNode(
        FileSystemNode sourceNode,
        double startAngle,
        double endAngle,
        double innerRadius,
        double outerRadius,
        int depth,
        Brush backgroundColor,
        string displayText)
    {
        SourceNode = sourceNode;
        StartAngle = startAngle;
        EndAngle = endAngle;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        Depth = depth;
        BackgroundColor = backgroundColor;
        DisplayText = displayText;
        CenterPoint = PolarToCartesian(startAngle, innerRadius, outerRadius);
    }

    private static Point PolarToCartesian(double angle, double innerRadius, double outerRadius)
    {
        var midRadius = (innerRadius + outerRadius) / 2;
        var x = Math.Cos(angle) * midRadius;
        var y = Math.Sin(angle) * midRadius;
        return new Point(x, y);
    }
}

public readonly record struct Rect(double X, double Y, double Width, double Height)
{
    public bool IsEmpty => Width <= 0 || Height <= 0;
    public double Area => Width * Height;

    public bool IntersectsWith(Rect other)
    {
        return !(other.X > X + Width ||
                 other.X + other.Width < X ||
                 other.Y > Y + Height ||
                 other.Y + other.Height < Y);
    }
}

public readonly record struct Point(double X, double Y);
