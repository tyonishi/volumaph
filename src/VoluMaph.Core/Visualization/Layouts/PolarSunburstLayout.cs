using VoluMaph.Core.Model;

namespace VoluMaph.Core.Layouts;

public sealed class PolarSunburstLayout : ISunburstLayout
{
    public List<SunburstSegment> CalculateLayout(FolderNode root, double centerRadius, double maxRadius, int maxDepth)
    {
        var segments = new List<SunburstSegment>();
        var totalSize = root.Size;

        if (totalSize == 0)
            return segments;

        CalculateRecursive(
            root,
            0,
            Math.PI * 2,
            centerRadius,
            maxRadius,
            0,
            maxDepth,
            totalSize,
            segments);

        return segments;
    }

    private static void CalculateRecursive(
        FolderNode folder,
        double startAngle,
        double endAngle,
        double innerRadius,
        double outerRadius,
        int depth,
        int maxDepth,
        long totalSize,
        List<SunburstSegment> segments)
    {
        var angleRange = endAngle - startAngle;

        var segment = new SunburstSegment(
            folder,
            startAngle,
            endAngle,
            innerRadius,
            outerRadius,
            depth);

        segments.Add(segment);

        if (depth >= maxDepth || folder.Children.Count == 0)
            return;

        var sortedChildren = folder.Children
            .Where(c => c.Size > 0)
            .OrderByDescending(c => c.Size)
            .ToList();

        if (sortedChildren.Count == 0)
            return;

        var nextInnerRadius = outerRadius;
        var nextOuterRadius = innerRadius + (outerRadius - innerRadius) * 0.8;

        var currentAngle = startAngle;

        foreach (var child in sortedChildren)
        {
            if (child is FolderNode subFolder)
            {
                var childSizeRatio = (double)child.Size / folder.Size;
                var childAngleRange = angleRange * childSizeRatio;
                var childEndAngle = currentAngle + childAngleRange;

                CalculateRecursive(
                    subFolder,
                    currentAngle,
                    childEndAngle,
                    nextInnerRadius,
                    nextOuterRadius,
                    depth + 1,
                    maxDepth,
                    totalSize,
                    segments);

                currentAngle = childEndAngle;
            }
        }
    }
}
