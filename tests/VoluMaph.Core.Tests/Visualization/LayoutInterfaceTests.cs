using VoluMaph.Core.Model;
using VoluMaph.Core.Layouts;

namespace VoluMaph.Core.Tests.Visualization;

public class TreemapLayoutInterfaceTests
{
    [Fact]
    public void ITreemapLayout_IsInterface()
    {
        var layoutType = typeof(ITreemapLayout);
        Assert.True(layoutType.IsInterface);
    }

    [Fact]
    public void ITreemapLayout_CalculateLayout_ReturnsList()
    {
        var layoutType = typeof(ITreemapLayout);
        var method = layoutType.GetMethod(nameof(ITreemapLayout.CalculateLayout));

        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType);
        Assert.Equal(typeof(List<>), method.ReturnType.GetGenericTypeDefinition());
    }

    [Fact]
    public void TreemapRect_IsRecord()
    {
        var rectType = typeof(TreemapRect);
        var baseType = rectType.BaseType;

        Assert.NotNull(baseType);
        Assert.Contains("Value", baseType.FullName);
    }

    [Fact]
    public void TreemapRect_HasRequiredProperties()
    {
        var rectType = typeof(TreemapRect);
        var nodeProperty = rectType.GetProperty(nameof(TreemapRect.Node));
        var boundsProperty = rectType.GetProperty(nameof(TreemapRect.Bounds));

        Assert.NotNull(nodeProperty);
        Assert.NotNull(boundsProperty);
    }

    [Fact]
    public void Rect_Struct_HasAreaProperty()
    {
        var rectType = typeof(Rect);
        var areaProperty = rectType.GetProperty(nameof(Rect.Area));

        Assert.NotNull(areaProperty);
        Assert.Equal(typeof(double), areaProperty.PropertyType);
    }

    [Fact]
    public void Rect_Struct_HasIsEmptyProperty()
    {
        var rectType = typeof(Rect);
        var isEmptyProperty = rectType.GetProperty(nameof(Rect.IsEmpty));

        Assert.NotNull(isEmptyProperty);
        Assert.Equal(typeof(bool), isEmptyProperty.PropertyType);
    }

    [Fact]
    public void Rect_Struct_HasIntersectsWithMethod()
    {
        var rectType = typeof(Rect);
        var method = rectType.GetMethod(nameof(Rect.IntersectsWith));

        Assert.NotNull(method);
        Assert.Equal(typeof(bool), method.ReturnType);
    }

    [Fact]
    public void TreemapRect_HasComputedProperties()
    {
        var rectType = typeof(TreemapRect);
        var xProperty = rectType.GetProperty(nameof(TreemapRect.X));
        var yProperty = rectType.GetProperty(nameof(TreemapRect.Y));
        var widthProperty = rectType.GetProperty(nameof(TreemapRect.Width));
        var heightProperty = rectType.GetProperty(nameof(TreemapRect.Height));

        Assert.NotNull(xProperty);
        Assert.NotNull(yProperty);
        Assert.NotNull(widthProperty);
        Assert.NotNull(heightProperty);
    }
}

public class SunburstLayoutInterfaceTests
{
    [Fact]
    public void ISunburstLayout_IsInterface()
    {
        var layoutType = typeof(ISunburstLayout);
        Assert.True(layoutType.IsInterface);
    }

    [Fact]
    public void ISunburstLayout_CalculateLayout_ReturnsList()
    {
        var layoutType = typeof(ISunburstLayout);
        var method = layoutType.GetMethod(nameof(ISunburstLayout.CalculateLayout));

        Assert.NotNull(method);
        Assert.True(method.ReturnType.IsGenericType);
        Assert.Equal(typeof(List<>), method.ReturnType.GetGenericTypeDefinition());
    }

    [Fact]
    public void SunburstSegment_IsRecord()
    {
        var segmentType = typeof(SunburstSegment);
        var baseType = segmentType.BaseType;

        Assert.NotNull(baseType);
        Assert.Contains("Value", baseType.FullName);
    }

    [Fact]
    public void SunburstSegment_HasRequiredProperties()
    {
        var segmentType = typeof(SunburstSegment);
        var nodeProperty = segmentType.GetProperty(nameof(SunburstSegment.Node));
        var startAngleProperty = segmentType.GetProperty(nameof(SunburstSegment.StartAngle));
        var endAngleProperty = segmentType.GetProperty(nameof(SunburstSegment.EndAngle));
        var innerRadiusProperty = segmentType.GetProperty(nameof(SunburstSegment.InnerRadius));
        var outerRadiusProperty = segmentType.GetProperty(nameof(SunburstSegment.OuterRadius));
        var depthProperty = segmentType.GetProperty(nameof(SunburstSegment.Depth));

        Assert.NotNull(nodeProperty);
        Assert.NotNull(startAngleProperty);
        Assert.NotNull(endAngleProperty);
        Assert.NotNull(innerRadiusProperty);
        Assert.NotNull(outerRadiusProperty);
        Assert.NotNull(depthProperty);
    }

    [Fact]
    public void SunburstSegment_HasComputedProperties()
    {
        var segmentType = typeof(SunburstSegment);
        var angleRangeProperty = segmentType.GetProperty(nameof(SunburstSegment.AngleRange));
        var midAngleProperty = segmentType.GetProperty(nameof(SunburstSegment.MidAngle));
        var midRadiusProperty = segmentType.GetProperty(nameof(SunburstSegment.MidRadius));

        Assert.NotNull(angleRangeProperty);
        Assert.NotNull(midAngleProperty);
        Assert.NotNull(midRadiusProperty);
    }

    [Fact]
    public void SunburstSegment_GetCenterPoint_ReturnsPoint()
    {
        var segmentType = typeof(SunburstSegment);
        var method = segmentType.GetMethod(nameof(SunburstSegment.GetCenterPoint));

        Assert.NotNull(method);
        Assert.Equal(typeof(Point), method.ReturnType);
    }

    [Fact]
    public void Point_Struct_HasXYProperties()
    {
        var pointType = typeof(Point);
        var xProperty = pointType.GetProperty(nameof(Point.X));
        var yProperty = pointType.GetProperty(nameof(Point.Y));

        Assert.NotNull(xProperty);
        Assert.NotNull(yProperty);
        Assert.Equal(typeof(double), xProperty.PropertyType);
        Assert.Equal(typeof(double), yProperty.PropertyType);
    }
}
