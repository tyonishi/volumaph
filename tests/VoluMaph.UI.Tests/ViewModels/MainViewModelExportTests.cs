using System;
using System.IO;
using System.Text;
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

public class MainViewModelExportTests : IDisposableTest
{
    private readonly IScanner _scanner = new DirectoryScanner();
    private readonly IFolderAnalyzer _analyzer = new FolderAnalyzer();
    private readonly IExtensionAnalyzer _extensionAnalyzer = new ExtensionAnalyzer();
    private readonly IDuplicateDetector _duplicateDetector = new DuplicateDetector();
    private readonly Mock<ISettingsProvider> _mockSettingsProvider = new();
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly MainViewModel _viewModel;

    public MainViewModelExportTests()
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
    public async Task ExportToCsvAsync_WithValidData_ShouldCreateCsvFile()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("file3.txt", 300);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.csv");
        await _viewModel.ExportToCsvAsync(outputPath);

        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task ExportToCsvAsync_WithValidData_ShouldContainCorrectHeaders()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.csv");
        await _viewModel.ExportToCsvAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("Name,Size,Path", content);
    }

    [Fact]
    public async Task ExportToCsvAsync_WithValidData_ShouldContainFileData()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.csv");
        await _viewModel.ExportToCsvAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("file1.txt", content);
        Assert.Contains("file2.txt", content);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldUseUtf8BomEncoding()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.csv");
        await _viewModel.ExportToCsvAsync(outputPath);

        var bytes = await File.ReadAllBytesAsync(outputPath);
        var bom = Encoding.UTF8.GetPreamble();

        Assert.True(bytes.Length >= bom.Length);
        for (int i = 0; i < bom.Length; i++)
        {
            Assert.Equal(bom[i], bytes[i]);
        }
    }

    [Fact]
    public async Task ExportToHtmlAsync_WithValidData_ShouldCreateHtmlFile()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.html");
        await _viewModel.ExportToHtmlAsync(outputPath);

        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task ExportToHtmlAsync_WithValidData_ShouldContainHtmlTags()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.html");
        await _viewModel.ExportToHtmlAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("<!DOCTYPE html>", content);
        Assert.Contains("<html", content);
        Assert.Contains("</html>", content);
    }

    [Fact]
    public async Task ExportToHtmlAsync_WithValidData_ShouldContainTableStructure()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.html");
        await _viewModel.ExportToHtmlAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("<table", content);
        Assert.Contains("</table>", content);
        Assert.Contains("<thead", content);
        Assert.Contains("<tbody", content);
    }

    [Fact]
    public async Task ExportToHtmlAsync_ShouldContainCssStyles()
    {
        CreateFile("file1.txt", 100);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.html");
        await _viewModel.ExportToHtmlAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("<style", content);
        Assert.Contains("</style>", content);
    }

    [Fact]
    public async Task ExportToHtmlAsync_WithValidData_ShouldContainFileData()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        await _viewModel.ScanFolderAsync(TestDirectory);

        var outputPath = Path.Combine(TestDirectory, "export.html");
        await _viewModel.ExportToHtmlAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("file1.txt", content);
        Assert.Contains("file2.txt", content);
    }
}
