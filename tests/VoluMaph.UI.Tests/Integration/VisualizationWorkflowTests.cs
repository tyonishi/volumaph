using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Color;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using VoluMaph.Infrastructure.Logging;
using VoluMaph.Infrastructure.Settings;
using VoluMaph.UI.ViewModels;
using Xunit;

namespace VoluMaph.UI.Tests.Integration;

public class VisualizationWorkflowTests : IDisposable
{
    private readonly IScanner _scanner;
    private readonly IFolderAnalyzer _analyzer;
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly string _testDirectory;

    public VisualizationWorkflowTests()
    {
        _scanner = new DirectoryScanner();
        _analyzer = new FolderAnalyzer();
        _logger = new SimpleLogger(Path.Combine(Path.GetTempPath(), "test.log"));
        _settingsProvider = new JsonSettingsProvider("test-settings.json");
        _testDirectory = Path.Combine(Path.GetTempPath(), $"VoluMaph_Test_{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task Workflow_ScanAndVisualize_ShouldSucceed()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file1.txt"), "test content");
        File.WriteAllText(Path.Combine(testFolder, "file2.txt"), "more test content");

        await mainViewModel.ScanFolderAsync(testFolder);

        Assert.NotNull(mainViewModel.RootFolder);
        Assert.Equal(2, mainViewModel.RootFolder?.Children.Count);
        Assert.NotNull(mainViewModel.VisualizationViewModel);
    }

    [Fact]
    public async Task Workflow_ToggleVisualization_ShouldSwitchView()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        Assert.False(mainViewModel.ShowVisualization);

        mainViewModel.ToggleVisualizationCommand?.Execute(null);

        Assert.True(mainViewModel.ShowVisualization);
    }

    [Fact]
    public async Task VisualizationViewModel_ColorTheme_ShouldSyncWithMainViewModel()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        var initialTheme = mainViewModel.ColorTheme;
        var vizTheme = mainViewModel.VisualizationViewModel.ColorTheme;

        mainViewModel.ColorTheme = ColorTheme.Viridis;

        Assert.Equal(ColorTheme.Viridis, mainViewModel.ColorTheme);
        Assert.Equal(ColorTheme.Viridis, mainViewModel.VisualizationViewModel.ColorTheme);
    }

    [Fact]
    public async Task VisualizationViewModel_ToggleMode_ShouldSwitchBetweenTreemapAndSunburst()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        var initialMode = mainViewModel.VisualizationViewModel.IsSunburstMode;

        mainViewModel.VisualizationViewModel.ToggleVisualizationModeCommand?.Execute(null);

        Assert.NotEqual(initialMode, mainViewModel.VisualizationViewModel.IsSunburstMode);
    }

    [Fact]
    public async Task VisualizationViewModel_VisibleNodes_ShouldReflectRootFolderChildren()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(testFolder, "file2.txt"), "content2");
        File.WriteAllText(Path.Combine(testFolder, "file3.txt"), "content3");

        await mainViewModel.ScanFolderAsync(testFolder);

        Assert.NotNull(mainViewModel.VisualizationViewModel.VisibleNodes);
        Assert.Equal(3, mainViewModel.VisualizationViewModel.VisibleNodes.Count);
    }

    [Fact]
    public async Task VisualizationViewModel_ZoomCommands_ShouldUpdateZoomLevel()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        var initialZoom = mainViewModel.VisualizationViewModel.ZoomLevel;

        mainViewModel.VisualizationViewModel.ZoomInCommand?.Execute(null);
        Assert.True(mainViewModel.VisualizationViewModel.ZoomLevel > initialZoom);

        var midZoom = mainViewModel.VisualizationViewModel.ZoomLevel;
        mainViewModel.VisualizationViewModel.ZoomOutCommand?.Execute(null);
        Assert.True(mainViewModel.VisualizationViewModel.ZoomLevel < midZoom);

        mainViewModel.VisualizationViewModel.ResetZoomCommand?.Execute(null);
        Assert.Equal(1.0, mainViewModel.VisualizationViewModel.ZoomLevel);
    }

    [Fact]
    public async Task VisualizationViewModel_SelectionChanged_ShouldFireEvent()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        FileSystemNode? selectedNode = null;
        mainViewModel.VisualizationViewModel.SelectionChanged += (s, e) => selectedNode = e;

        var firstNode = mainViewModel.VisualizationViewModel.VisibleNodes[0];
        mainViewModel.VisualizationViewModel.SelectedNode = firstNode;

        Assert.NotNull(selectedNode);
        Assert.Same(firstNode, selectedNode);
    }

    [Fact]
    public async Task VisualizationViewModel_WithNestedFolders_ShouldCalculateCorrectDepth()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        var subFolder = Path.Combine(testFolder, "SubFolder");
        Directory.CreateDirectory(subFolder);
        var deepFolder = Path.Combine(subFolder, "DeepFolder");
        Directory.CreateDirectory(deepFolder);
        File.WriteAllText(Path.Combine(deepFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        Assert.NotNull(mainViewModel.RootFolder);
        mainViewModel.VisualizationViewModel.AutoThemeSelection = true;

        var theme = mainViewModel.VisualizationViewModel.ColorTheme;
        Assert.True(Enum.IsDefined(typeof(ColorTheme), theme));
    }

    [Fact]
    public async Task Workflow_CompleteVisualizationCycle_ShouldWork()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);

        for (int i = 0; i < 10; i++)
        {
            File.WriteAllText(Path.Combine(testFolder, $"file{i}.txt"), $"content{i}");
        }

        await mainViewModel.ScanFolderAsync(testFolder);

        Assert.NotNull(mainViewModel.RootFolder);
        Assert.Equal(10, mainViewModel.RootFolder?.Children.Count);

        mainViewModel.ToggleVisualizationCommand?.Execute(null);
        Assert.True(mainViewModel.ShowVisualization);

        mainViewModel.VisualizationViewModel.ToggleVisualizationModeCommand?.Execute(null);
        Assert.True(mainViewModel.VisualizationViewModel.IsSunburstMode);

        mainViewModel.VisualizationViewModel.ToggleVisualizationModeCommand?.Execute(null);
        Assert.False(mainViewModel.VisualizationViewModel.IsSunburstMode);

        mainViewModel.VisualizationViewModel.ZoomInCommand?.Execute(null);
        mainViewModel.VisualizationViewModel.ResetZoomCommand?.Execute(null);
        Assert.Equal(1.0, mainViewModel.VisualizationViewModel.ZoomLevel);

        mainViewModel.ToggleVisualizationCommand?.Execute(null);
        Assert.False(mainViewModel.ShowVisualization);
    }

    [Fact]
    public async Task VisualizationViewModel_Commands_ShouldExecuteWhenCanExecute()
    {
        var mainViewModel = new MainViewModel(
            _scanner,
            _analyzer,
            new ExtensionAnalyzer(),
            new DuplicateDetector(),
            _settingsProvider,
            _logger);

        var testFolder = Path.Combine(_testDirectory, "TestFolder");
        Directory.CreateDirectory(testFolder);
        File.WriteAllText(Path.Combine(testFolder, "file.txt"), "content");

        await mainViewModel.ScanFolderAsync(testFolder);

        Assert.NotNull(mainViewModel.VisualizationViewModel.ToggleVisualizationModeCommand);
        Assert.NotNull(mainViewModel.VisualizationViewModel.ZoomInCommand);
        Assert.NotNull(mainViewModel.VisualizationViewModel.ZoomOutCommand);
        Assert.NotNull(mainViewModel.VisualizationViewModel.ResetZoomCommand);
    }
}
