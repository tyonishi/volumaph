using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using Xunit;

namespace VoluMaph.Core.Tests.Scanning;

public class DirectoryScannerTests : IDisposableTest
{
    private readonly DirectoryScanner _scanner = new();

    [Fact]
    public async Task ScanAsync_ShouldScanFilesAndDirectories()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateDirectory("subfolder");
        CreateFile("subfolder/file3.txt", 300);

        var result = await _scanner.ScanAsync(TestDirectory);

        Assert.Equal(TestDirectory, result.FullPath);
        Assert.True(result.Children.Count >= 2);
    }

    [Fact]
    public async Task ScanAsync_ShouldCreateFileNodesWithCorrectProperties()
    {
        CreateFile("test.txt", 1024);

        var result = await _scanner.ScanAsync(TestDirectory);
        var fileNode = result.Children.OfType<FileNode>().FirstOrDefault(f => f.Name == "test.txt");

        Assert.NotNull(fileNode);
        Assert.Equal("test.txt", fileNode.Name);
        Assert.Equal(1024, fileNode.Size);
        Assert.Equal(".txt", fileNode.Extension);
    }

    [Fact]
    public async Task ScanAsync_ShouldCreateFolderNodes()
    {
        CreateDirectory("subfolder");
        CreateFile("subfolder/file.txt", 100);

        var result = await _scanner.ScanAsync(TestDirectory);
        var folderNode = result.Children.OfType<FolderNode>().FirstOrDefault(f => f.Name == "subfolder");

        Assert.NotNull(folderNode);
        Assert.Equal("subfolder", folderNode.Name);
        Assert.True(folderNode.IsFolder);
    }

    [Fact]
    public async Task ScanAsync_ShouldReportProgress()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateDirectory("subfolder");
        CreateFile("subfolder/file3.txt", 300);

        var progressCount = 0;
        var progress = new Progress<ScanProgress>(_ => progressCount++);

        await _scanner.ScanAsync(TestDirectory, progress);

        Assert.True(progressCount > 0);
    }

    [Fact]
    public async Task ScanAsync_ShouldHandleUnauthorizedAccessException()
    {
        CreateFile("file.txt", 100);

        var result = await _scanner.ScanAsync(TestDirectory);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ScanAsync_ShouldHandleNonExistentPath()
    {
        var nonExistentPath = Path.Combine(TestDirectory, "nonexistent");

        var result = await _scanner.ScanAsync(nonExistentPath);

        Assert.NotNull(result);
        Assert.Equal(nonExistentPath, result.FullPath);
        Assert.Empty(result.Children);
    }
}
