using VoluMaph.UI.Behaviors;
using Xunit;

namespace VoluMaph.UI.Tests.Behaviors;

public sealed class ZoomPanBehaviorTests
{
    [Fact]
    public void ZoomConstants_HaveCorrectValues()
    {
        Assert.Equal(0.1, ZoomPanBehavior.MinZoom);
        Assert.Equal(20.0, ZoomPanBehavior.MaxZoom);
        Assert.Equal(1.1, ZoomPanBehavior.ZoomStep);
    }

    [Fact]
    public void ZoomConstants_MinZoom_IsGreaterThanZero()
    {
        Assert.True(ZoomPanBehavior.MinZoom > 0);
    }

    [Fact]
    public void ZoomConstants_MaxZoom_IsLessThanHundred()
    {
        Assert.True(ZoomPanBehavior.MaxZoom < 100);
    }

    [Fact]
    public void ZoomConstants_ZoomStep_IsPositive()
    {
        Assert.True(ZoomPanBehavior.ZoomStep > 1.0);
    }

    [Fact]
    public void IsEnabledProperty_IsRegistered()
    {
        Assert.NotNull(ZoomPanBehavior.IsEnabledProperty);
        Assert.Equal("IsEnabled", ZoomPanBehavior.IsEnabledProperty.Name);
    }

    [Fact]
    public void ZoomTargetProperty_IsRegistered()
    {
        Assert.NotNull(ZoomPanBehavior.ZoomTargetProperty);
        Assert.Equal("ZoomTarget", ZoomPanBehavior.ZoomTargetProperty.Name);
    }

    [Fact]
    public void GetIsEnabled_WhenFalse_ReturnsFalse()
    {
        var mockElement = new MockUIElement();
        var result = ZoomPanBehavior.GetIsEnabled(mockElement);
        Assert.False(result);
    }

    [Fact]
    public void SetIsEnabled_WhenTrue_SetsToTrue()
    {
        var mockElement = new MockUIElement();
        ZoomPanBehavior.SetIsEnabled(mockElement, true);
        var result = ZoomPanBehavior.GetIsEnabled(mockElement);
        Assert.True(result);
    }

    [Fact]
    public void SetIsEnabled_WhenFalse_SetsToFalse()
    {
        var mockElement = new MockUIElement();
        ZoomPanBehavior.SetIsEnabled(mockElement, true);
        ZoomPanBehavior.SetIsEnabled(mockElement, false);
        var result = ZoomPanBehavior.GetIsEnabled(mockElement);
        Assert.False(result);
    }

    [Fact]
    public void GetZoomTarget_WhenNotSet_ReturnsNull()
    {
        var mockElement = new MockUIElement();
        var result = ZoomPanBehavior.GetZoomTarget(mockElement);
        Assert.Null(result);
    }

    [Fact]
    public void SetZoomTarget_WithValidValue_SetsValue()
    {
        var parentElement = new MockUIElement();
        var childElement = new MockUIElement();

        ZoomPanBehavior.SetZoomTarget(parentElement, childElement);
        var result = ZoomPanBehavior.GetZoomTarget(parentElement);

        Assert.Same(childElement, result);
    }

    [Fact]
    public void SetZoomTarget_WithNull_ClearsValue()
    {
        var mockElement = new MockUIElement();
        ZoomPanBehavior.SetZoomTarget(mockElement, new MockUIElement());
        var result = ZoomPanBehavior.GetZoomTarget(mockElement);
        Assert.Null(result);
    }

    [Fact]
    public void ZoomPanState_InitializesWithDefaults()
    {
        var state = ZoomPanBehavior.CreateZoomState();

        Assert.NotNull(state.Transform);
        Assert.NotNull(state.TranslateTransform);
        Assert.NotNull(state.ScaleTransform);
        Assert.False(state.IsDragging);
    }

    [Fact]
    public void ZoomPanState_TransformGroup_ContainsBothTransforms()
    {
        var state = ZoomPanBehavior.CreateZoomState();
        var scaleTransform = state.ScaleTransform;
        var translateTransform = state.TranslateTransform;

        Assert.Contains(scaleTransform, state.Transform.Children);
        Assert.Contains(translateTransform, state.Transform.Children);
        Assert.Equal(2, state.Transform.Children.Count);
    }

    [Fact]
    public void ZoomPanState_ScaleTransform_InitializesWithUnity()
    {
        var state = ZoomPanBehavior.CreateZoomState();
        var scaleTransform = state.ScaleTransform;

        Assert.Equal(1.0, scaleTransform.ScaleX);
        Assert.Equal(1.0, scaleTransform.ScaleY);
    }

    [Fact]
    public void ZoomPanState_TranslateTransform_InitializesWithDefaults()
    {
        var state = ZoomPanBehavior.CreateZoomState();
        var translateTransform = state.TranslateTransform;

        Assert.Equal(0, translateTransform.X);
        Assert.Equal(0, translateTransform.Y);
    }

    [Fact]
    public void ZoomPanState_TransformOrigin_IsCenterOfElement()
    {
        var state = ZoomPanBehavior.CreateZoomState();

        Assert.Equal(0.5, state.TransformOrigin.X);
        Assert.Equal(0.5, state.TransformOrigin.Y);
    }

    [Fact]
    public void ZoomPanState_LastMousePosition_InitializesAsDefault()
    {
        var state = ZoomPanBehavior.CreateZoomState();

        Assert.Equal(default, state.LastMousePosition);
    }
}

public class MockUIElement
{
    private int _wheelDelta;
    private MouseButtonEventArgs? _buttonArgs;
    private MouseEventArgs? _moveArgs;

    public int MouseWheelDelta
    {
        get => _wheelDelta;
        set => _wheelDelta = value;
    }

    public MouseButtonEventArgs? ButtonArgs
    {
        get => _buttonArgs;
        set => _buttonArgs = value;
    }

    public MouseEventArgs? MoveArgs
    {
        get => _moveArgs;
        set => _moveArgs = value;
    }
}
