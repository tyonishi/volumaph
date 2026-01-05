using System;
using VoluMaph.UI.ViewModels;
using Xunit;

namespace VoluMaph.UI.Tests.ViewModels;

public sealed class VisualizationBookmarkTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var bookmark = new VisualizationBookmark("Test Bookmark", state);

        Assert.Equal("Test Bookmark", bookmark.Name);
        Assert.Same(state, bookmark.State);
    }

    [Fact]
    public void Name_WhenSet_RaisesPropertyChanged()
    {
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var bookmark = new VisualizationBookmark("Initial Name", state);
        var propertyChanged = false;

        bookmark.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationBookmark.Name))
            {
                propertyChanged = true;
            }
        };

        bookmark.Name = "New Name";

        Assert.True(propertyChanged);
    }

    [Fact]
    public void State_WhenSet_RaisesPropertyChanged()
    {
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);
        var bookmark = new VisualizationBookmark("Test", state1);
        var propertyChanged = false;

        bookmark.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationBookmark.State))
            {
                propertyChanged = true;
            }
        };

        bookmark.State = state2;

        Assert.True(propertyChanged);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        var state = new VisualizationState(1.0, 0.0, 0.0, null);

        Assert.Throws<ArgumentException>(() => new VisualizationBookmark(null!, state));
    }

    [Fact]
    public void Constructor_WithNullState_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new VisualizationBookmark("Test", null!));
    }

    [Fact]
    public void Name_WithEmptyString_ThrowsArgumentException()
    {
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var bookmark = new VisualizationBookmark("Test", state);

        Assert.Throws<ArgumentException>(() => bookmark.Name = string.Empty);
    }

    [Fact]
    public void Name_WithWhitespace_ThrowsArgumentException()
    {
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var bookmark = new VisualizationBookmark("Test", state);

        Assert.Throws<ArgumentException>(() => bookmark.Name = "   ");
    }
}
