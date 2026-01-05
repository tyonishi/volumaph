using VoluMaph.Core.Model;

namespace VoluMaph.Core.Layouts;

public interface ISunburstLayout
{
    List<SunburstSegment> CalculateLayout(FolderNode root, double centerRadius, double maxRadius, int maxDepth);
}

public sealed record SunburstSegment(
    FileSystemNode Node,
    double StartAngle,
    double EndAngle,
    double InnerRadius,
    double OuterRadius,
    int Depth)
{
    public double AngleRange => EndAngle - StartAngle;
    public double MidAngle => (StartAngle + EndAngle) / 2;
    public double MidRadius => (InnerRadius + OuterRadius) / 2;

    public Point GetCenterPoint()
    {
        var x = Math.Cos(MidAngle) * MidRadius;
        var y = Math.Sin(MidAngle) * MidRadius;
        return new Point(x, y);
    }
}

public readonly record struct Point(double X, double Y);
