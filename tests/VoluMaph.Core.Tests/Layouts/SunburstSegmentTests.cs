using VoluMaph.Core.Layouts;
using VoluMaph.Core.Model;
using Xunit;

namespace VoluMaph.Core.Tests.Layouts;

public sealed class SunburstSegmentTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 100, 1);

        Assert.Same(folder, segment.Node);
        Assert.Equal(0, segment.StartAngle);
        Assert.Equal(Math.PI, segment.EndAngle);
        Assert.Equal(50, segment.InnerRadius);
        Assert.Equal(100, segment.OuterRadius);
        Assert.Equal(1, segment.Depth);
    }

    [Fact]
    public void AngleRange_ReturnsCorrectValue()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 100, 1);

        Assert.Equal(Math.PI, segment.AngleRange, 0.01);
    }

    [Fact]
    public void AngleRange_WithDifferentAngles_ReturnsCorrectValue()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, Math.PI / 4, 3 * Math.PI / 4, 50, 100, 1);

        Assert.Equal(Math.PI / 2, segment.AngleRange, 0.01);
    }

    [Fact]
    public void MidAngle_ReturnsMiddlePoint()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 100, 1);

        Assert.Equal(Math.PI / 2, segment.MidAngle, 0.01);
    }

    [Fact]
    public void MidAngle_WithAsymmetricAngles_ReturnsCorrectMidpoint()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI / 2, 50, 100, 1);

        Assert.Equal(Math.PI / 4, segment.MidAngle, 0.01);
    }

    [Fact]
    public void MidRadius_ReturnsMiddlePoint()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 100, 1);

        Assert.Equal(75, segment.MidRadius, 0.01);
    }

    [Fact]
    public void GetCenterPoint_ReturnsCorrectCoordinates()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 100, 1);

        var center = segment.GetCenterPoint();
        var midRadius = segment.MidRadius;
        var expectedX = Math.Cos(Math.PI / 2) * midRadius;
        var expectedY = Math.Sin(Math.PI / 2) * midRadius;

        Assert.Equal(expectedX, center.X, 0.01);
        Assert.Equal(expectedY, center.Y, 0.01);
    }

    [Fact]
    public void GetCenterPoint_WithZeroAngle_ReturnsPositiveX()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, 0.1, 50, 100, 1);

        var center = segment.GetCenterPoint();
        var midAngle = segment.MidAngle;
        var midRadius = segment.MidRadius;
        var expectedX = Math.Cos(midAngle) * midRadius;
        var expectedY = Math.Sin(midAngle) * midRadius;

        Assert.Equal(expectedX, center.X, 0.01);
        Assert.Equal(expectedY, center.Y, 0.01);
    }

    [Fact]
    public void Depth_CanBeZero()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 100, 0);

        Assert.Equal(0, segment.Depth);
    }

    [Fact]
    public void InnerRadius_CanBeZero()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 0, 100, 1);

        Assert.Equal(0, segment.InnerRadius);
    }

    [Fact]
    public void Radii_CanBeEqual()
    {
        var folder = CreateFolderNode("/test", 100);
        var segment = new SunburstSegment(folder, 0, Math.PI, 50, 50, 1);

        Assert.Equal(50, segment.InnerRadius);
        Assert.Equal(50, segment.OuterRadius);
    }

    private static FolderNode CreateFolderNode(string path, long size)
    {
        return new FolderNode(path)
        {
            Size = size
        };
    }
}
