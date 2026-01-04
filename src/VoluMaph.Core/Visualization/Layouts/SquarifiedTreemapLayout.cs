using VoluMaph.Core.Model;

namespace VoluMaph.Core.Layouts;

public sealed class SquarifiedTreemapLayout : ITreemapLayout
{
    public List<TreemapRect> CalculateLayout(IReadOnlyList<FileSystemNode> children, Rect bounds)
    {
        if (children.Count == 0 || bounds.IsEmpty)
            return new List<TreemapRect>();

        var nodes = children
            .Where(n => n.Size > 0)
            .OrderByDescending(n => n.Size)
            .ToList();

        if (nodes.Count == 0)
            return new List<TreemapRect>();

        var totalSize = nodes.Sum(n => n.Size);
        var results = new List<TreemapRect>();

        Squarify(nodes, 0, nodes.Count, bounds, totalSize, results);

        return results;
    }

    private static void Squarify(
        List<FileSystemNode> nodes,
        int start,
        int end,
        Rect bounds,
        long totalSize,
        List<TreemapRect> results)
    {
        if (start >= end)
            return;

        if (end - start <= 2)
        {
            LayoutRow(nodes, start, end, bounds, totalSize, results);
            return;
        }

        var rowEnd = start + 1;
        var minAspectRatio = double.MaxValue;
        var bestRowEnd = rowEnd;

        while (rowEnd <= end)
        {
            var currentAspectRatio = CalculateWorstAspectRatio(nodes, start, rowEnd, bounds);
            if (currentAspectRatio > minAspectRatio)
            {
                break;
            }

            minAspectRatio = currentAspectRatio;
            bestRowEnd = rowEnd;
            rowEnd++;
        }

        LayoutRow(nodes, start, bestRowEnd, bounds, totalSize, results);

        var rowSize = nodes.Skip(start).Take(bestRowEnd - start).Sum(n => n.Size);
        var remainingSize = totalSize - rowSize;

        if (bounds.Width > bounds.Height)
        {
            var rowWidth = bounds.Width * (rowSize / (double)totalSize);
            var nextBounds = new Rect(bounds.X + rowWidth, bounds.Y, bounds.Width - rowWidth, bounds.Height);
            Squarify(nodes, bestRowEnd, end, nextBounds, totalSize, results);
        }
        else
        {
            var rowHeight = bounds.Height * (rowSize / (double)totalSize);
            var nextBounds = new Rect(bounds.X, bounds.Y + rowHeight, bounds.Width, bounds.Height - rowHeight);
            Squarify(nodes, bestRowEnd, end, nextBounds, totalSize, results);
        }
    }

    private static void LayoutRow(
        List<FileSystemNode> nodes,
        int start,
        int end,
        Rect bounds,
        long totalSize,
        List<TreemapRect> results)
    {
        var rowSize = nodes.Skip(start).Take(end - start).Sum(n => n.Size);
        var rowArea = bounds.Area * (rowSize / (double)totalSize);

        double x = bounds.X;
        double y = bounds.Y;

        bool horizontal = bounds.Width > bounds.Height;

        for (int i = start; i < end; i++)
        {
            var node = nodes[i];
            var nodeArea = rowArea * (node.Size / (double)rowSize);

            Rect nodeBounds;
            if (horizontal)
            {
                var nodeWidth = bounds.Width * (node.Size / (double)rowSize);
                nodeBounds = new Rect(x, y, nodeWidth, bounds.Height);
                x += nodeWidth;
            }
            else
            {
                var nodeHeight = bounds.Height * (node.Size / (double)rowSize);
                nodeBounds = new Rect(x, y, bounds.Width, nodeHeight);
                y += nodeHeight;
            }

            results.Add(new TreemapRect(node, nodeBounds));
        }
    }

    private static double CalculateWorstAspectRatio(
        List<FileSystemNode> nodes,
        int start,
        int end,
        Rect bounds)
    {
        var rowSize = nodes.Skip(start).Take(end - start).Sum(n => n.Size);
        double worst = 1.0;

        bool horizontal = bounds.Width > bounds.Height;
        var rowDimension = horizontal ? bounds.Width : bounds.Height;

        for (int i = start; i < end; i++)
        {
            var node = nodes[i];
            var nodeDimension = rowDimension * (node.Size / (double)rowSize);
            var otherDimension = horizontal ? bounds.Height : bounds.Width;

            var ratio = Math.Max(nodeDimension / otherDimension, otherDimension / nodeDimension);
            if (ratio > worst)
            {
                worst = ratio;
            }
        }

        return worst;
    }
}
