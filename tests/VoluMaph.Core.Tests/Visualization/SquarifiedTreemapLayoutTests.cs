using VoluMaph.Core.Model;
using VoluMaph.Core.Layouts;

namespace VoluMaph.Core.Tests.Visualization;

public class SquarifiedTreemapLayoutTests
{
    [Fact]
    public void CalculateLayout_WithEmptyChildren_ReturnsEmptyList()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = Array.Empty<FileSystemNode>();
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Empty(result);
    }

    [Fact]
    public void CalculateLayout_WithEmptyBounds_ReturnsEmptyList()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 0, 0);
        var children = new[] { CreateFolderNode(100) };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Empty(result);
    }

    [Fact]
    public void CalculateLayout_WithSingleNode_ReturnsSingleRect()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[] { CreateFolderNode(100) };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Single(result);
        Assert.Equal(1000, result[0].Width);
        Assert.Equal(800, result[0].Height);
    }

    [Fact]
    public void CalculateLayout_WithTwoNodes_SplitsHorizontally()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(50),
            CreateFolderNode(50)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(2, result.Count);
        Assert.True(result[0].X < result[1].X);
        Assert.Equal(bounds.Y, result[0].Y);
        Assert.Equal(bounds.Y, result[1].Y);
    }

    [Fact]
    public void CalculateLayout_WithTwoNodes_VerticalBounds_SplitsVertically()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 400, 1000);
        var children = new[]
        {
            CreateFolderNode(50),
            CreateFolderNode(50)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(2, result.Count);
        Assert.Equal(bounds.X, result[0].X);
        Assert.Equal(bounds.X, result[1].X);
        Assert.True(result[0].Y < result[1].Y);
    }

    [Fact]
    public void CalculateLayout_WithZeroSizeNode_ExcludesNode()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(100),
            CreateFolderNode(0),
            CreateFolderNode(50)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CalculateLayout_WithMultipleNodes_OrdersBySizeDescending()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(10),
            CreateFolderNode(100),
            CreateFolderNode(50)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(3, result.Count);
        Assert.Equal(100, result[0].Node.Size);
        Assert.Equal(50, result[1].Node.Size);
        Assert.Equal(10, result[2].Node.Size);
    }

    [Fact]
    public void CalculateLayout_PreservesTotalArea()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(100),
            CreateFolderNode(200),
            CreateFolderNode(300)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        var calculatedArea = result.Sum(r => r.Bounds.Area);
        var totalSize = children.Sum(c => c.Size);
        var expectedArea = bounds.Area * (totalSize / (double)totalSize);
        Assert.Equal(expectedArea, calculatedArea, 0.01);
    }

    [Fact]
    public void CalculateLayout_WithFourNodes_CreatesFourRects()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(25),
            CreateFolderNode(25),
            CreateFolderNode(25),
            CreateFolderNode(25)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void CalculateLayout_RectanglesDontOverlap()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(100),
            CreateFolderNode(200),
            CreateFolderNode(150),
            CreateFolderNode(50)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        for (int i = 0; i < result.Count; i++)
        {
            for (int j = i + 1; j < result.Count; j++)
            {
                var r1 = result[i].Bounds;
                var r2 = result[j].Bounds;
                
                var intersects = !(r1.X + r1.Width <= r2.X ||
                                 r2.X + r2.Width <= r1.X ||
                                 r1.Y + r1.Height <= r2.Y ||
                                 r2.Y + r2.Height <= r1.Y);
                
                Assert.False(intersects, $"Rectangles {i} and {j} overlap: {r1} vs {r2}");
            }
        }
    }

    [Fact]
    public void CalculateLayout_RectanglesWithinBounds()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(100),
            CreateFolderNode(200),
            CreateFolderNode(150)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        foreach (var rect in result)
        {
            Assert.True(rect.X >= bounds.X);
            Assert.True(rect.Y >= bounds.Y);
            Assert.True(rect.X + rect.Width <= bounds.X + bounds.Width);
            Assert.True(rect.Y + rect.Height <= bounds.Y + bounds.Height);
        }
    }

    [Fact]
    public void CalculateLayout_SizeProportionalToNodeSize()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(100),
            CreateFolderNode(200)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        var totalSize = children.Sum(c => c.Size);
        var ratio1 = result[0].Bounds.Area / bounds.Area;
        var ratio2 = result[1].Bounds.Area / bounds.Area;
        
        Assert.Equal(100.0 / 300.0, ratio1, 0.01);
        Assert.Equal(200.0 / 300.0, ratio2, 0.01);
    }

    [Fact]
    public void CalculateLayout_WithVeryDifferentSizes_HandlesGracefully()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(1),
            CreateFolderNode(1000)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CalculateLayout_WithManyNodes_HandlesGracefully()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = Enumerable.Range(0, 100)
            .Select(i => CreateFolderNode((i + 1) * 10))
            .ToArray();
        
        var result = layout.CalculateLayout(children, bounds);
        
        Assert.Equal(100, result.Count);
    }

    [Fact]
    public void CalculateLayout_AspectRatio_Balanced()
    {
        var layout = new SquarifiedTreemapLayout();
        var bounds = new Rect(0, 0, 1000, 800);
        var children = new[]
        {
            CreateFolderNode(250),
            CreateFolderNode(250),
            CreateFolderNode(250),
            CreateFolderNode(250)
        };
        
        var result = layout.CalculateLayout(children, bounds);
        
        foreach (var rect in result)
        {
            var aspectRatio = Math.Max(rect.Width / rect.Height, rect.Height / rect.Width);
            Assert.True(aspectRatio < 4.0, $"Aspect ratio {aspectRatio} is too large");
        }
    }

    private static FolderNode CreateFolderNode(long size)
    {
        return new FolderNode($"/folder/{Guid.NewGuid()}")
        {
            Size = size
        };
    }
}
