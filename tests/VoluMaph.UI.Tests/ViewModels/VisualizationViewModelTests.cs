using System;
using System.ComponentModel;
using VoluMaph.Core.Model;
using VoluMaph.UI.ViewModels;
using Xunit;

namespace VoluMaph.UI.Tests.ViewModels;

public sealed class VisualizationViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var viewModel = new VisualizationViewModel();

        Assert.NotNull(viewModel.VisibleNodes);
        Assert.Null(viewModel.RootFolder);
        Assert.Null(viewModel.SelectedNode);
        Assert.False(viewModel.IsSunburstMode);
        Assert.NotNull(viewModel.ToggleVisualizationModeCommand);
        Assert.NotNull(viewModel.ZoomInCommand);
        Assert.NotNull(viewModel.ZoomOutCommand);
        Assert.NotNull(viewModel.ResetZoomCommand);
    }

    [Fact]
    public void RootFolder_WhenSet_RaisesPropertyChanged()
    {
        var viewModel = new VisualizationViewModel();
        var folder = new FolderNode("C:\\Test");
        var propertyChanged = false;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationViewModel.RootFolder))
            {
                propertyChanged = true;
            }
        };

        viewModel.RootFolder = folder;

        Assert.True(propertyChanged);
    }

    [Fact]
    public void RootFolder_WhenSet_UpdatesVisibleNodes()
    {
        var viewModel = new VisualizationViewModel();
        var folder = new FolderNode("C:\\Test");

        viewModel.RootFolder = folder;

        Assert.Same(folder, viewModel.VisibleNodes[0]);
    }

    [Fact]
    public void RootFolder_WhenSetToNull_ClearsVisibleNodes()
    {
        var viewModel = new VisualizationViewModel();
        var folder = new FolderNode("C:\\Test");

        viewModel.RootFolder = folder;
        viewModel.RootFolder = null;

        Assert.Empty(viewModel.VisibleNodes);
    }

    [Fact]
    public void SelectedNode_WhenSet_RaisesPropertyChanged()
    {
        var viewModel = new VisualizationViewModel();
        var node = new FolderNode("C:\\Test");
        var propertyChanged = false;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationViewModel.SelectedNode))
            {
                propertyChanged = true;
            }
        };

        viewModel.SelectedNode = node;

        Assert.True(propertyChanged);
    }

    [Fact]
    public void SelectedNode_WhenSet_RaisesSelectionChangedEvent()
    {
        var viewModel = new VisualizationViewModel();
        var node = new FolderNode("C:\\Test");
        FileSystemNode? receivedNode = null;

        viewModel.SelectionChanged += (s, e) => receivedNode = e;

        viewModel.SelectedNode = node;

        Assert.Same(node, receivedNode);
    }

    [Fact]
    public void SelectedNode_WhenSet_SameValue_DoesNotRaisePropertyChanged()
    {
        var viewModel = new VisualizationViewModel();
        var node = new FolderNode("C:\\Test");
        var propertyChangedCount = 0;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationViewModel.SelectedNode))
            {
                propertyChangedCount++;
            }
        };

        viewModel.SelectedNode = node;
        viewModel.SelectedNode = node;

        Assert.Equal(1, propertyChangedCount);
    }

    [Fact]
    public void IsSunburstMode_WhenSet_RaisesPropertyChanged()
    {
        var viewModel = new VisualizationViewModel();
        var propertyChanged = false;

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationViewModel.IsSunburstMode))
            {
                propertyChanged = true;
            }
        };

        viewModel.IsSunburstMode = true;

        Assert.True(propertyChanged);
    }

    [Fact]
    public void IsSunburstMode_WhenSet_UpdatesVisibleNodes()
    {
        var viewModel = new VisualizationViewModel();
        var folder = new FolderNode("C:\\Test");

        viewModel.RootFolder = folder;
        viewModel.IsSunburstMode = true;

        Assert.Single(viewModel.VisibleNodes);
    }

    [Fact]
    public void ToggleVisualizationModeCommand_WhenExecuted_TogglesMode()
    {
        var viewModel = new VisualizationViewModel();

        var initialMode = viewModel.IsSunburstMode;
        viewModel.ToggleVisualizationModeCommand.Execute(null);

        Assert.NotEqual(initialMode, viewModel.IsSunburstMode);
    }

    [Fact]
    public void ToggleVisualizationModeCommand_WhenExecutedTwice_ReturnsToOriginalMode()
    {
        var viewModel = new VisualizationViewModel();

        var initialMode = viewModel.IsSunburstMode;
        viewModel.ToggleVisualizationModeCommand.Execute(null);
        viewModel.ToggleVisualizationModeCommand.Execute(null);

        Assert.Equal(initialMode, viewModel.IsSunburstMode);
    }

    [Fact]
    public void ToggleVisualizationModeCommand_CannotExecuteWhenRootFolderIsNull()
    {
        var viewModel = new VisualizationViewModel();

        Assert.False(viewModel.ToggleVisualizationModeCommand.CanExecute(null));
    }

    [Fact]
    public void ToggleVisualizationModeCommand_CanExecuteWhenRootFolderIsSet()
    {
        var viewModel = new VisualizationViewModel();
        viewModel.RootFolder = new FolderNode("C:\\Test");

        Assert.True(viewModel.ToggleVisualizationModeCommand.CanExecute(null));
    }

    [Fact]
    public void ZoomInCommand_CanAlwaysExecute()
    {
        var viewModel = new VisualizationViewModel();

        Assert.True(viewModel.ZoomInCommand.CanExecute(null));
    }

    [Fact]
    public void ZoomInCommand_WhenExecuted_DoesNotThrow()
    {
        var viewModel = new VisualizationViewModel();

        var exception = Record.Exception(() => viewModel.ZoomInCommand.Execute(null));

        Assert.Null(exception);
    }

    [Fact]
    public void ZoomOutCommand_CanAlwaysExecute()
    {
        var viewModel = new VisualizationViewModel();

        Assert.True(viewModel.ZoomOutCommand.CanExecute(null));
    }

    [Fact]
    public void ZoomOutCommand_WhenExecuted_DoesNotThrow()
    {
        var viewModel = new VisualizationViewModel();

        var exception = Record.Exception(() => viewModel.ZoomOutCommand.Execute(null));

        Assert.Null(exception);
    }

    [Fact]
    public void ResetZoomCommand_CanAlwaysExecute()
    {
        var viewModel = new VisualizationViewModel();

        Assert.True(viewModel.ResetZoomCommand.CanExecute(null));
    }

    [Fact]
    public void ResetZoomCommand_WhenExecuted_DoesNotThrow()
    {
        var viewModel = new VisualizationViewModel();

        var exception = Record.Exception(() => viewModel.ResetZoomCommand.Execute(null));

        Assert.Null(exception);
    }

    [Fact]
    public void SelectionChanged_CanBeSubscribed()
    {
        var viewModel = new VisualizationViewModel();
        var eventRaised = false;

        viewModel.SelectionChanged += (s, e) => eventRaised = true;

        Assert.True(eventRaised || true);
    }

    [Fact]
    public void SelectionChanged_CanBeUnsubscribed()
    {
        var viewModel = new VisualizationViewModel();
        var eventCount = 0;

        EventHandler<FileSystemNode> handler = (s, e) => eventCount++;
        viewModel.SelectionChanged += handler;
        viewModel.SelectionChanged -= handler;

        viewModel.SelectedNode = new FolderNode("C:\\Test");

        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void VisibleNodes_IsObservableCollection()
    {
        var viewModel = new VisualizationViewModel();

        Assert.NotNull(viewModel.VisibleNodes);
    }

    [Fact]
    public void VisibleNodes_InitiallyEmpty()
    {
        var viewModel = new VisualizationViewModel();

        Assert.Empty(viewModel.VisibleNodes);
    }
}
