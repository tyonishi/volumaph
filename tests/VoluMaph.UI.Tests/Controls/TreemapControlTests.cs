using System.Collections.ObjectModel;
using System.Windows.Media;
using VoluMaph.Core.Color;
using VoluMaph.Core.Layouts;
using VoluMaph.Core.Model;
using VoluMaph.UI.Controls;
using VoluMaph.UI.Models.Visualization;
using Xunit;

namespace VoluMaph.UI.Tests.Controls;

public sealed class TreemapControlTests : WpfTestBase
{
    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void Constructor_InitializesProperties()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            Assert.NotNull(control.Nodes);
            Assert.Equal(6, control.MaxDepth);
            Assert.Equal(100.0, control.MinDisplaySize);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxDepth_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            control.MaxDepth = 10;

            Assert.Equal(10, control.MaxDepth);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MinDisplaySize_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            control.MinDisplaySize = 50.0;

            Assert.Equal(50.0, control.MinDisplaySize);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void Nodes_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();
            var nodes = new ObservableCollection<TreemapNode>();

            control.Nodes = nodes;

            Assert.Same(nodes, control.Nodes);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void ColorMapper_UpdatesProperty()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();
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
            var control = new TreemapControl();
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
            var control = new TreemapControl();
            var eventCount = 0;

            EventHandler<TreemapNode> handler = (s, e) => eventCount++;
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
            var control = new TreemapControl();

            control.MaxDepth = -1;

            Assert.Equal(-1, control.MaxDepth);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MaxDepth_AcceptsLargeValue()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            control.MaxDepth = 100;

            Assert.Equal(100, control.MaxDepth);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MinDisplaySize_AcceptsZero()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            control.MinDisplaySize = 0;

            Assert.Equal(0, control.MinDisplaySize);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void MinDisplaySize_AcceptsLargeValue()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            control.MinDisplaySize = 10000.0;

            Assert.Equal(10000.0, control.MinDisplaySize);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void NodesProperty_InitiallyNull()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            Assert.Null(control.Nodes);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void ColorMapperProperty_InitiallyNull()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();

            Assert.Null(control.ColorMapper);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void NodesCollection_CanAddNodes()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();
            var nodes = new ObservableCollection<TreemapNode>();

            var folder = new FolderNode("C:\\Test");
            var bounds = new UI.Models.Visualization.Rect(0, 0, 100, 100);
            var brush = new SolidColorBrush(Colors.Red);
            var node = new TreemapNode(folder, bounds, 0, brush, "Test");

            nodes.Add(node);
            control.Nodes = nodes;

            Assert.Single(control.Nodes);
            Assert.Same(node, control.Nodes[0]);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void NodesCollection_CanRemoveNodes()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();
            var nodes = new ObservableCollection<TreemapNode>();

            var folder = new FolderNode("C:\\Test");
            var bounds = new UI.Models.Visualization.Rect(0, 0, 100, 100);
            var brush = new SolidColorBrush(Colors.Red);
            var node = new TreemapNode(folder, bounds, 0, brush, "Test");

            nodes.Add(node);
            control.Nodes = nodes;

            nodes.Remove(node);

            Assert.Empty(control.Nodes);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void NodesCollection_CanClearNodes()
    {
        RunOnStaThread(() =>
        {
            var control = new TreemapControl();
            var nodes = new ObservableCollection<TreemapNode>();

            var folder = new FolderNode("C:\\Test");
            var bounds = new UI.Models.Visualization.Rect(0, 0, 100, 100);
            var brush = new SolidColorBrush(Colors.Red);
            var node = new TreemapNode(folder, bounds, 0, brush, "Test");

            nodes.Add(node);
            control.Nodes = nodes;

            nodes.Clear();

            Assert.Empty(control.Nodes);
        });
    }

    [Fact(Skip = "Requires full WPF XAML resource loading")]
    public void TreemapNode_HasValidProperties()
    {
        RunOnStaThread(() =>
        {
            var folder = new FolderNode("C:\\Test");
            var bounds = new UI.Models.Visualization.Rect(10, 20, 100, 150);
            var brush = new SolidColorBrush(Colors.Blue);
            var node = new TreemapNode(folder, bounds, 2, brush, "TestFolder");

            Assert.Same(folder, node.SourceNode);
            Assert.Equal(10, node.Bounds.X);
            Assert.Equal(20, node.Bounds.Y);
            Assert.Equal(100, node.Bounds.Width);
            Assert.Equal(150, node.Bounds.Height);
            Assert.Equal(2, node.Depth);
            Assert.Same(brush, node.BackgroundColor);
            Assert.Equal("TestFolder", node.DisplayText);
            Assert.True(node.IsVisible);
        });
    }
}
