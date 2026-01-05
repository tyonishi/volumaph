using VoluMaph.Core.Model;

namespace VoluMaph.Core.Layouts;

public interface ITreemapLayout
{
    List<TreemapRect> CalculateLayout(IReadOnlyList<FileSystemNode> children, Rect bounds);
}

public sealed record TreemapRect(
    FileSystemNode Node,
    Rect Bounds)
{
    public double X => Bounds.X;
    public double Y => Bounds.Y;
    public double Width => Bounds.Width;
    public double Height => Bounds.Height;
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
