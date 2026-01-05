using VoluMaph.Core.Model;

namespace VoluMaph.Core.Layouts;

/// <summary>
/// Squarified Treemap Layout - iterative implementation to avoid stack overflow.
/// Optimized with prefix sums and zero-size node handling.
/// </summary>
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
        if (totalSize <= 0)
            return new List<TreemapRect>();

        var results = new List<TreemapRect>();

        // Pre-compute prefix sums for O(1) range sum queries
        var prefixSums = ComputePrefixSums(nodes);

        SquarifyIterative(nodes, 0, nodes.Count, bounds, totalSize, prefixSums, results);

        return results;
    }

    /// <summary>
    /// Compute prefix sums array for efficient range sum queries.
    /// </summary>
    private static long[] ComputePrefixSums(List<FileSystemNode> nodes)
    {
        var prefix = new long[nodes.Count + 1];
        prefix[0] = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            prefix[i + 1] = prefix[i] + nodes[i].Size;
        }
        return prefix;
    }

    /// <summary>
    /// Get sum of sizes in range [start, end) using prefix sums.
    /// </summary>
    private static long GetRangeSum(long[] prefixSums, int start, int end)
    {
        return prefixSums[end] - prefixSums[start];
    }

    /// <summary>
    /// Iterative version of Squarify to avoid stack overflow with large datasets.
    /// </summary>
    private static void SquarifyIterative(
        List<FileSystemNode> nodes,
        int start,
        int end,
        Rect bounds,
        long totalSize,
        long[] prefixSums,
        List<TreemapRect> results)
    {
        int i = start;
        double bX = bounds.X, bY = bounds.Y, bW = bounds.Width, bH = bounds.Height;
        long remainingTotal = totalSize;

        while (i < end)
        {
            if (end - i <= 2)
            {
                LayoutRowSafe(nodes, i, end, new Rect(bX, bY, bW, bH), remainingTotal, prefixSums, results);
                break;
            }

            int rowEnd = i + 1;
            double minAspect = double.MaxValue;
            int bestRowEnd = rowEnd;

            while (rowEnd <= end)
            {
                var currentAspect = CalculateWorstAspectRatioSafe(
                    nodes, i, rowEnd, new Rect(bX, bY, bW, bH), prefixSums);

                // If aspect ratio is NaN or worse, stop expanding row
                if (double.IsNaN(currentAspect) || currentAspect > minAspect)
                    break;

                minAspect = currentAspect;
                bestRowEnd = rowEnd;
                rowEnd++;
            }

            LayoutRowSafe(nodes, i, bestRowEnd, new Rect(bX, bY, bW, bH), remainingTotal, prefixSums, results);

            var rowSize = GetRangeSum(prefixSums, i, bestRowEnd);
            if (rowSize <= 0)
            {
                // If row has zero size, advance to avoid infinite loop
                i = bestRowEnd;
                continue;
            }

            var rowArea = (bW * bH) * (rowSize / (double)remainingTotal);

            if (bW > bH)
            {
                var rowWidth = rowArea / bH;
                bX += rowWidth;
                bW -= rowWidth;
            }
            else
            {
                var rowHeight = rowArea / bW;
                bY += rowHeight;
                bH -= rowHeight;
            }

            remainingTotal -= rowSize;
            i = bestRowEnd;
        }
    }

    /// <summary>
    /// Layout a single row with zero-size node handling.
    /// </summary>
    private static void LayoutRowSafe(
        List<FileSystemNode> nodes,
        int start,
        int end,
        Rect bounds,
        long totalSize,
        long[] prefixSums,
        List<TreemapRect> results)
    {
        var rowSize = GetRangeSum(prefixSums, start, end);
        if (rowSize <= 0)
        {
            // Create zero-area rects for zero-size nodes
            for (int k = start; k < end; k++)
            {
                results.Add(new TreemapRect(nodes[k], new Rect(bounds.X, bounds.Y, 0, 0)));
            }
            return;
        }

        var rowArea = bounds.Area * (rowSize / (double)totalSize);
        double x = bounds.X, y = bounds.Y;
        bool horizontal = bounds.Width > bounds.Height;

        for (int k = start; k < end; k++)
        {
            var node = nodes[k];
            Rect nodeBounds;

            if (horizontal)
            {
                var nodeWidth = rowArea * (node.Size / (double)rowSize) / bounds.Height;
                nodeBounds = new Rect(x, y, nodeWidth, bounds.Height);
                x += nodeWidth;
            }
            else
            {
                var nodeHeight = rowArea * (node.Size / (double)rowSize) / bounds.Width;
                nodeBounds = new Rect(x, y, bounds.Width, nodeHeight);
                y += nodeHeight;
            }

            results.Add(new TreemapRect(node, nodeBounds));
        }
    }

    /// <summary>
    /// Calculate worst aspect ratio with NaN handling.
    /// </summary>
    private static double CalculateWorstAspectRatioSafe(
        List<FileSystemNode> nodes,
        int start,
        int end,
        Rect bounds,
        long[] prefixSums)
    {
        var rowSize = GetRangeSum(prefixSums, start, end);
        if (rowSize <= 0)
            return double.NaN;

        double worst = 1.0;
        bool horizontal = bounds.Width > bounds.Height;
        var rowDimension = horizontal ? bounds.Width : bounds.Height;

        for (int i = start; i < end; i++)
        {
            var node = nodes[i];
            var nodeDimension = rowDimension * (node.Size / (double)rowSize);
            var otherDimension = horizontal ? bounds.Height : bounds.Width;

            if (otherDimension <= 0)
                continue;

            var ratio = Math.Max(nodeDimension / otherDimension, otherDimension / nodeDimension);
            if (ratio > worst)
            {
                worst = ratio;
            }
        }

        return worst;
    }
}
