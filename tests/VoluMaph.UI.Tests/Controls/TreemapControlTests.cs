using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using VoluMaph.Core.Color;
using VoluMaph.Core.Layouts;
using VoluMaph.Core.Model;
using VoluMaph.UI.Controls;
using VoluMaph.UI.Models.Visualization;

namespace VoluMaph.UI.Tests.Controls;

public sealed class TreemapControlTests
{
    public TreemapControlTests()
    {
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var control = new TreemapControl();

        Assert.NotNull(control.Nodes);
        Assert.Equal(6, control.MaxDepth);
        Assert.Equal(100.0, control.MinDisplaySize);
    }

    [Fact]
    public void MaxDepth_UpdatesProperty()
    {
        var control = new TreemapControl();

        control.MaxDepth = 10;

        Assert.Equal(10, control.MaxDepth);
    }

    [Fact]
    public void MinDisplaySize_UpdatesProperty()
    {
        var control = new TreemapControl();

        control.MinDisplaySize = 50.0;

        Assert.Equal(50.0, control.MinDisplaySize);
    }

    [Fact]
    public void Nodes_UpdatesProperty()
    {
        var control = new TreemapControl();
        var nodes = new ObservableCollection<TreemapNode>();

        control.Nodes = nodes;

        Assert.Same(nodes, control.Nodes);
    }

    [Fact]
    public void ColorMapper_UpdatesProperty()
    {
        var control = new TreemapControl();
        var mapper = new SizeBasedColorMapper();

        control.ColorMapper = mapper;

        Assert.Same(mapper, control.ColorMapper);
    }
}
