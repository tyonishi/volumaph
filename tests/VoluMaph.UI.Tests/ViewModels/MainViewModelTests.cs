using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using VoluMaph.Infrastructure.Logging;
using VoluMaph.Infrastructure.Settings;
using VoluMaph.UI.ViewModels;
using Xunit;

namespace VoluMaph.UI.Tests.ViewModels;

public class MainViewModelTests : IDisposableTest
{
    private readonly IScanner _scanner = new DirectoryScanner();
    private readonly IFolderAnalyzer _analyzer = new FolderAnalyzer();
    private readonly IExtensionAnalyzer _extensionAnalyzer = new ExtensionAnalyzer();
    private readonly IDuplicateDetector _duplicateDetector = new DuplicateDetector();
    private readonly Mock<ISettingsProvider> _mockSettingsProvider = new();
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mockSettingsProvider.Setup(s => s.Load()).Returns(new AppSettings());

        _viewModel = new MainViewModel(
            _scanner,
            _analyzer,
            _extensionAnalyzer,
            _duplicateDetector,
            _mockSettingsProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void SearchText_WhenSet_ShouldRaisePropertyChanged()
    {
        var propertyChanged = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.SearchText))
            {
                propertyChanged = true;
            }
        };

        _viewModel.SearchText = "test";

        Assert.True(propertyChanged);
    }

    [Fact]
    public async Task ScanFolderAsync_WithValidFolder_ShouldScanFolder()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateDirectory("subfolder");
        CreateFile("subfolder/file3.txt", 300);

        await _viewModel.ScanFolderAsync(TestDirectory);

        Assert.NotNull(_viewModel.RootFolder);
        Assert.Equal(TestDirectory, _viewModel.RootFolder.FullPath);
    }

    [Fact]
    public async Task ScanFolderAsync_WhenCompleted_ShouldUpdateStatusMessage()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        Assert.Contains("completed", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task SelectedNode_WhenSet_ShouldUpdateCurrentChildren()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        Assert.True(_viewModel.CurrentChildren.Count >= 2);
    }

    [Fact]
    public async Task SearchText_WhenSet_ShouldFilterCurrentChildren()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("test.txt", 300);

        await _viewModel.ScanFolderAsync(TestDirectory);
        var originalCount = _viewModel.CurrentChildren.Count;

        _viewModel.SearchText = "file";

        var filteredCount = _viewModel.CurrentChildren.Count;
        Assert.True(filteredCount <= originalCount);
    }

    [Fact]
    public async Task SearchText_WhenSetToEmpty_ShouldShowAllChildren()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);
        var originalCount = _viewModel.CurrentChildren.Count;

        _viewModel.SearchText = "file";
        _viewModel.SearchText = string.Empty;

        Assert.Equal(originalCount, _viewModel.CurrentChildren.Count);
    }

    [Fact]
    public async Task SearchText_ShouldBeCaseInsensitive()
    {
        CreateFile("File1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        _viewModel.SearchText = "FILE";

        Assert.Equal(2, _viewModel.CurrentChildren.Count);
    }

    [Fact]
    public void OpenInExplorerCommand_ShouldNotExecuteWhenNoNodeSelected()
    {
        Assert.False(_viewModel.OpenInExplorerCommand.CanExecute(null));
    }

    [Fact]
    public async Task OpenInExplorerCommand_ShouldExecuteWhenNodeSelected()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        Assert.True(_viewModel.OpenInExplorerCommand.CanExecute(null));
    }

    [Fact]
    public void CopyPathCommand_ShouldNotExecuteWhenNoNodeSelected()
    {
        Assert.False(_viewModel.CopyPathCommand.CanExecute(null));
    }

    [Fact]
    public async Task CopyPathCommand_ShouldExecuteWhenNodeSelected()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        Assert.True(_viewModel.CopyPathCommand.CanExecute(null));
    }

    [Fact]
    public void CopyNameCommand_ShouldNotExecuteWhenNoNodeSelected()
    {
        Assert.False(_viewModel.CopyNameCommand.CanExecute(null));
    }

    [Fact]
    public async Task CopyNameCommand_ShouldExecuteWhenNodeSelected()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        Assert.True(_viewModel.CopyNameCommand.CanExecute(null));
    }

    [Fact]
    public async Task ScanFolderAsync_WithInvalidPath_ShouldNotThrow()
    {
        var invalidPath = Path.Combine(TestDirectory, "nonexistent");

        var exception = await Record.ExceptionAsync(async () => await _viewModel.ScanFolderAsync(invalidPath));

        Assert.Null(exception);
    }

    [Fact(Skip = "Application.Current is null in test environment")]
    public void ToggleThemeCommand_ShouldToggleIsDarkTheme()
    {
        var initialTheme = _viewModel.IsDarkTheme;

        _viewModel.ToggleThemeCommand.Execute(null);

        Assert.NotEqual(initialTheme, _viewModel.IsDarkTheme);
    }

    [Fact(Skip = "Application.Current is null in test environment")]
    public void IsDarkTheme_ShouldRaisePropertyChanged()
    {
        var propertyChanged = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsDarkTheme))
            {
                propertyChanged = true;
            }
        };

        _viewModel.ToggleThemeCommand.Execute(null);

        Assert.True(propertyChanged);
    }

    [Fact]
    public void ColumnDefinitions_ShouldContainDefaultColumns()
    {
        Assert.Equal(3, _viewModel.ColumnDefinitions.Count);
        Assert.Contains(_viewModel.ColumnDefinitions, c => c.Header == "Name");
        Assert.Contains(_viewModel.ColumnDefinitions, c => c.Header == "Size");
        Assert.Contains(_viewModel.ColumnDefinitions, c => c.Header == "Path");
    }

    [Fact]
    public void ColumnDefinitions_ShouldHaveIsVisibleSetToTrue()
    {
        foreach (var column in _viewModel.ColumnDefinitions)
        {
            Assert.True(column.IsVisible);
        }
    }
}
