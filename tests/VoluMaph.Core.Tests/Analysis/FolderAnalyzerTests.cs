using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using Xunit;

namespace VoluMaph.Core.Tests.Analysis;

public class FolderAnalyzerTests : IDisposableTest
{
    private readonly DirectoryScanner _scanner = new();
    private readonly FolderAnalyzer _analyzer = new();

    [Fact]
    public async Task AggregateFolderSizes_ShouldCalculateCorrectSizes()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateDirectory("subfolder");
        CreateFile("subfolder/file3.txt", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        _analyzer.AggregateFolderSizes(result);

        Assert.Equal(600, result.Size);
        Assert.Equal(2, result.FileCount);
        Assert.Equal(1, result.FolderCount);

        var subFolder = result.Children.OfType<FolderNode>().First(f => f.Name == "subfolder");
        Assert.Equal(300, subFolder.Size);
        Assert.Equal(1, subFolder.FileCount);
        Assert.Equal(0, subFolder.FolderCount);
    }

    [Fact]
    public async Task AggregateFolderSizes_ShouldHandleNestedFolders()
    {
        CreateFile("file1.txt", 100);
        CreateDirectory("folder1");
        CreateFile("folder1/file2.txt", 200);
        CreateDirectory("folder1/subfolder");
        CreateFile("folder1/subfolder/file3.txt", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        _analyzer.AggregateFolderSizes(result);

        Assert.Equal(600, result.Size);
        Assert.Equal(1, result.FileCount);
        Assert.Equal(2, result.FolderCount);

        var folder1 = result.Children.OfType<FolderNode>().First(f => f.Name == "folder1");
        Assert.Equal(500, folder1.Size);
        Assert.Equal(1, folder1.FileCount);
        Assert.Equal(1, folder1.FolderCount);

        var subFolder = folder1.Children.OfType<FolderNode>().First(f => f.Name == "subfolder");
        Assert.Equal(300, subFolder.Size);
        Assert.Equal(1, subFolder.FileCount);
        Assert.Equal(0, subFolder.FolderCount);
    }

    [Fact]
    public async Task Sort_BySizeDescending_ShouldReturnCorrectOrder()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 300);
        CreateFile("file3.txt", 200);
        CreateDirectory("folder1");
        CreateFile("folder1/file4.txt", 400);

        var result = await _scanner.ScanAsync(TestDirectory);
        _analyzer.AggregateFolderSizes(result);

        var sorted = _analyzer.Sort(result, SortOption.BySizeDescending).ToList();

        Assert.True(sorted[0].Size >= sorted[1].Size);
        Assert.True(sorted[1].Size >= sorted[2].Size);
    }

    [Fact]
    public async Task Sort_BySizeAscending_ShouldReturnCorrectOrder()
    {
        CreateFile("file1.txt", 300);
        CreateFile("file2.txt", 100);
        CreateFile("file3.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        _analyzer.AggregateFolderSizes(result);

        var sorted = _analyzer.Sort(result, SortOption.BySizeAscending).ToList();

        Assert.True(sorted[0].Size <= sorted[1].Size);
        Assert.True(sorted[1].Size <= sorted[2].Size);
    }

    [Fact]
    public async Task Sort_ByNameAscending_ShouldReturnAlphabeticalOrder()
    {
        CreateFile("c.txt", 100);
        CreateFile("a.txt", 100);
        CreateFile("b.txt", 100);

        var result = await _scanner.ScanAsync(TestDirectory);
        var sorted = _analyzer.Sort(result, SortOption.ByNameAscending).ToList();

        Assert.Equal("a.txt", sorted[0].Name);
        Assert.Equal("b.txt", sorted[1].Name);
        Assert.Equal("c.txt", sorted[2].Name);
    }

    [Fact]
    public async Task Sort_ByNameDescending_ShouldReturnReverseAlphabeticalOrder()
    {
        CreateFile("a.txt", 100);
        CreateFile("b.txt", 100);
        CreateFile("c.txt", 100);

        var result = await _scanner.ScanAsync(TestDirectory);
        var sorted = _analyzer.Sort(result, SortOption.ByNameDescending).ToList();

        Assert.Equal("c.txt", sorted[0].Name);
        Assert.Equal("b.txt", sorted[1].Name);
        Assert.Equal("a.txt", sorted[2].Name);
    }

    [Fact]
    public async Task Filter_ByMinSize_ShouldFilterCorrectly()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("file3.txt", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { MinSize = 200 };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, f => Assert.True(f.Size >= 200));
    }

    [Fact]
    public async Task Filter_ByMaxSize_ShouldFilterCorrectly()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("file3.txt", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { MaxSize = 200 };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, f => Assert.True(f.Size <= 200));
    }

    [Fact]
    public async Task Filter_ByExtensions_ShouldFilterCorrectly()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("file3.cs", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { Extensions = new[] { ".txt" } };
        var filtered = _analyzer.Filter(result, criteria).OfType<FileNode>().ToList();

        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, f => Assert.Equal(".txt", f.Extension));
    }

    [Fact]
    public async Task Filter_IncludeFilesOnly_ShouldReturnFilesOnly()
    {
        CreateFile("file1.txt", 100);
        CreateDirectory("folder1");
        CreateFile("folder1/file2.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { IncludeFolders = false, IncludeFiles = true };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.All(filtered, f => Assert.False(f.IsFolder));
    }

    [Fact]
    public async Task Filter_IncludeFoldersOnly_ShouldReturnFoldersOnly()
    {
        CreateFile("file1.txt", 100);
        CreateDirectory("folder1");
        CreateFile("folder1/file2.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { IncludeFolders = true, IncludeFiles = false };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.All(filtered, f => Assert.True(f.IsFolder));
    }

    [Fact]
    public async Task AggregateFolderSizes_ShouldHandleEmptyFolder()
    {
        CreateDirectory("emptyFolder");

        var result = await _scanner.ScanAsync(TestDirectory);
        _analyzer.AggregateFolderSizes(result);

        var emptyFolder = result.Children.OfType<FolderNode>().First(f => f.Name == "emptyFolder");
        Assert.Equal(0, emptyFolder.Size);
        Assert.Equal(0, emptyFolder.FileCount);
        Assert.Equal(0, emptyFolder.FolderCount);
    }
}
