using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using Xunit;

namespace VoluMaph.Core.Tests.Analysis;

public class ExtensionAnalyzerTests : IDisposableTest
{
    private readonly DirectoryScanner _scanner = new();
    private readonly FolderAnalyzer _folderAnalyzer = new();
    private readonly ExtensionAnalyzer _extensionAnalyzer = new();

    [Fact]
    public async Task AnalyzeByExtension_ShouldGroupByExtension()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("file3.cs", 300);
        CreateFile("file4.cs", 400);
        CreateFile("file5.js", 500);

        var result = await _scanner.ScanAsync(TestDirectory);
        var analysis = _extensionAnalyzer.AnalyzeByExtension(result).ToList();

        Assert.Equal(3, analysis.Count);
        Assert.Contains(analysis, e => e.Extension == ".txt" && e.TotalSize == 300 && e.FileCount == 2);
        Assert.Contains(analysis, e => e.Extension == ".cs" && e.TotalSize == 700 && e.FileCount == 2);
        Assert.Contains(analysis, e => e.Extension == ".js" && e.TotalSize == 500 && e.FileCount == 1);
    }

    [Fact]
    public async Task AnalyzeByExtension_ShouldIncludeNoExtensionFiles()
    {
        CreateFile("file_without_extension", 100);
        CreateFile("file.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        var analysis = _extensionAnalyzer.AnalyzeByExtension(result).ToList();

        Assert.Equal(2, analysis.Count);
        Assert.Contains(analysis, e => e.Extension == ".txt" && e.TotalSize == 200);
        Assert.Contains(analysis, e => e.Extension == "(none)" && e.TotalSize == 100);
    }

    [Fact]
    public async Task AnalyzeByExtension_ShouldHandleNestedFolders()
    {
        CreateFile("root.txt", 100);
        CreateDirectory("subfolder");
        CreateFile("subfolder/nested.txt", 200);
        CreateFile("subfolder/file.cs", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var analysis = _extensionAnalyzer.AnalyzeByExtension(result).ToList();

        Assert.Equal(2, analysis.Count);
        Assert.Contains(analysis, e => e.Extension == ".txt" && e.TotalSize == 300 && e.FileCount == 2);
        Assert.Contains(analysis, e => e.Extension == ".cs" && e.TotalSize == 300 && e.FileCount == 1);
    }

    [Fact]
    public async Task AnalyzeByExtension_ShouldReturnDescendingSizeOrder()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.cs", 500);
        CreateFile("file3.js", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var analysis = _extensionAnalyzer.AnalyzeByExtension(result).ToList();

        Assert.True(analysis[0].TotalSize >= analysis[1].TotalSize);
        Assert.True(analysis[1].TotalSize >= analysis[2].TotalSize);
    }

    [Fact]
    public async Task AnalyzeByExtension_ShouldHandleEmptyFolder()
    {
        var result = await _scanner.ScanAsync(TestDirectory);
        var analysis = _extensionAnalyzer.AnalyzeByExtension(result).ToList();

        Assert.Empty(analysis);
    }

    [Fact]
    public async Task AnalyzeByExtension_ShouldCalculatePercentage()
    {
        CreateFile("file1.txt", 400);
        CreateFile("file2.cs", 300);
        CreateFile("file3.js", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var analysis = _extensionAnalyzer.AnalyzeByExtension(result).ToList();

        Assert.Equal(40.0, analysis[0].Percentage, 1);
        Assert.Equal(30.0, analysis[1].Percentage, 1);
        Assert.Equal(30.0, analysis[2].Percentage, 1);
    }
}
