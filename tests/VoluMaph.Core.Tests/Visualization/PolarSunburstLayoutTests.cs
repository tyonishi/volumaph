using VoluMaph.Core.Model;
using VoluMaph.Core.Layouts;

namespace VoluMaph.Core.Tests.Visualization;

public class PolarSunburstLayoutTests
{
    [Fact]
    public void CalculateLayout_WithEmptyRoot_ReturnsEmptyList()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(0);

        var result = layout.CalculateLayout(root, 0, 300, 6);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateLayout_WithSingleNode_ReturnsOneSegment()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);

        var result = layout.CalculateLayout(root, 50, 300, 6);

        Assert.Single(result);
    }

    [Fact]
    public void CalculateLayout_RootSegment_CoversFullCircle()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);

        var result = layout.CalculateLayout(root, 0, 300, 6);

        Assert.Single(result);
        Assert.Equal(0, result[0].StartAngle, 0.01);
        Assert.Equal(2 * Math.PI, result[0].EndAngle, 0.01);
    }

    [Fact]
    public void CalculateLayout_InnerRadius_MatchesCenterRadius()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);
        var centerRadius = 50.0;

        var result = layout.CalculateLayout(root, centerRadius, 300, 6);

        Assert.Equal(centerRadius, result[0].InnerRadius, 0.01);
    }

    [Fact]
    public void CalculateLayout_OuterRadius_LessThanMaxRadius()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);
        var centerRadius = 50.0;
        var maxRadius = 300.0;

        var result = layout.CalculateLayout(root, centerRadius, maxRadius, 6);

        Assert.True(result[0].OuterRadius <= maxRadius);
        Assert.True(result[0].OuterRadius > centerRadius);
    }

    [Fact]
    public void CalculateLayout_DepthStartsAtZero()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);

        var result = layout.CalculateLayout(root, 50, 300, 6);

        Assert.Equal(0, result[0].Depth);
    }

    [Fact]
    public void CalculateLayout_WithMultipleNodes_CreatesMultipleSegments()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);
        root.AddChild(CreateFolderNode(50));
        root.AddChild(CreateFolderNode(50));

        var result = layout.CalculateLayout(root, 50, 300, 6);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void CalculateLayout_ChildAngles_SumToParentAngle()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);
        var child1 = CreateFolderNode(50);
        var child2 = CreateFolderNode(50);
        root.AddChild(child1);
        root.AddChild(child2);

        var result = layout.CalculateLayout(root, 50, 300, 6);

        var rootAngleRange = result[0].EndAngle - result[0].StartAngle;
        var childAngleRange = result[1].EndAngle - result[1].StartAngle;

        Assert.Equal(rootAngleRange / 2.0, childAngleRange, 0.01);
    }

    [Fact]
    public void CalculateLayout_MaxDepth_LimitsRecursion()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);
        AddChildren(root, 10, 6);

        var result = layout.CalculateLayout(root, 50, 300, 3);

        var maxDepth = result.Max(r => r.Depth);
        Assert.Equal(3, maxDepth);
    }

    [Fact]
    public void GetCenterPoint_ReturnsMidRadius()
    {
        var layout = new PolarSunburstLayout();
        var root = CreateFolderNode(100);

        var result = layout.CalculateLayout(root, 50, 300, 6);

        var center = result[0].GetCenterPoint();

        var midRadius = (result[0].InnerRadius + result[0].OuterRadius) / 2;
        var expectedX = Math.Cos(Math.PI) * midRadius;
        var expectedY = Math.Sin(Math.PI) * midRadius;

        Assert.Equal(expectedX, center.X, 0.01);
        Assert.Equal(expectedY, center.Y, 0.01);
    }

    private static FolderNode CreateFolderNode(long size)
    {
        return new FolderNode($"/folder/{Guid.NewGuid()}")
        {
            Size = size
        };
    }

    private static void AddChildren(FolderNode parent, int count, int levels)
    {
        if (levels == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            var child = CreateFolderNode(10);
            parent.AddChild(child);
            AddChildren(child, count, levels - 1);
        }
    }
}
