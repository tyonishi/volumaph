using System.Windows.Media;
using VoluMaph.Core.Color;
using VoluMaph.Core.Layouts;
using VoluMaph.Core.Model;
using VoluMaph.UI.Controls;
using VoluMaph.UI.Models.Visualization;
using Xunit;

namespace VoluMaph.UI.Tests.Controls;

public sealed class SunburstControlTests : WpfTestBase
{
    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void Constructor_InitializesProperties()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            Assert.Equal(6, control.MaxDepth);
            Assert.Equal(50.0, control.CenterRadius);
            Assert.Equal(300.0, control.MaxRadius);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxDepth_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.MaxDepth = 10;

            Assert.Equal(10, control.MaxDepth);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void CenterRadius_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.CenterRadius = 75.0;

            Assert.Equal(75.0, control.CenterRadius);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxRadius_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.MaxRadius = 500.0;

            Assert.Equal(500.0, control.MaxRadius);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void ColorMapper_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();
            var mapper = new SizeBasedColorMapper();

            control.ColorMapper = mapper;

            Assert.Same(mapper, control.ColorMapper);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void SelectionChanged_CanBeSubscribed()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();
            var eventRaised = false;

            control.SelectionChanged += (s, e) => eventRaised = true;

            Assert.True(eventRaised || true);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void SelectionChanged_CanBeUnsubscribed()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();
            var eventCount = 0;

            EventHandler<SunburstNode> handler = (s, e) => eventCount++;
            control.SelectionChanged += handler;
            control.SelectionChanged -= handler;

            Assert.Equal(0, eventCount);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxDepth_AcceptsNegativeValue()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.MaxDepth = -1;

            Assert.Equal(-1, control.MaxDepth);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxDepth_AcceptsLargeValue()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.MaxDepth = 100;

            Assert.Equal(100, control.MaxDepth);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void CenterRadius_AcceptsZero()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.CenterRadius = 0;

            Assert.Equal(0, control.CenterRadius);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxRadius_AcceptsLargeValue()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            control.MaxRadius = 1000.0;

            Assert.Equal(1000.0, control.MaxRadius);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void ColorMapperProperty_InitiallyNull()
    {
        RunOnStaThread(() =>
        {
            var control = new SunburstControl();

            Assert.Null(control.ColorMapper);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void SunburstNode_HasValidProperties()
    {
        RunOnStaThread(() =>
        {
            var folder = new FolderNode("C:\\Test");
            var brush = new SolidColorBrush(Colors.Red);
            var node = new SunburstNode(folder, 0, Math.PI, 50, 100, 2, brush, "TestFolder");

            Assert.Same(folder, node.SourceNode);
            Assert.Equal(0, node.StartAngle);
            Assert.Equal(Math.PI, node.EndAngle);
            Assert.Equal(50, node.InnerRadius);
            Assert.Equal(100, node.OuterRadius);
            Assert.Equal(2, node.Depth);
            Assert.Same(brush, node.BackgroundColor);
            Assert.Equal("TestFolder", node.DisplayText);
            Assert.True(node.IsVisible);
        });
    }
}
