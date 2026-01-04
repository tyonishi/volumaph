using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using Xunit;

namespace VoluMaph.Core.Tests.Filtering;

public class FileAgeFilterTests : IDisposableTest
{
    private readonly DirectoryScanner _scanner = new();
    private readonly FolderAnalyzer _analyzer = new();

    [Fact]
    public async Task Filter_ByModifiedAfterDays_ShouldReturnRecentFiles()
    {
        var now = DateTime.Now;
        var oldDate = now.AddDays(-10);
        var recentDate = now.AddDays(-2);

        CreateFile("old_file.txt", 100);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "old_file.txt"), oldDate);

        CreateFile("recent_file.txt", 200);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "recent_file.txt"), recentDate);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { ModifiedAfterDays = 5 };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Single(filtered);
        Assert.Equal("recent_file.txt", filtered[0].Name);
    }

    [Fact]
    public async Task Filter_ByModifiedBeforeDays_ShouldReturnOldFiles()
    {
        var now = DateTime.Now;
        var oldDate = now.AddDays(-10);
        var recentDate = now.AddDays(-2);

        CreateFile("old_file.txt", 100);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "old_file.txt"), oldDate);

        CreateFile("recent_file.txt", 200);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "recent_file.txt"), recentDate);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { ModifiedBeforeDays = 5 };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Single(filtered);
        Assert.Equal("old_file.txt", filtered[0].Name);
    }

    [Fact]
    public async Task Filter_ByCreatedAfterDays_ShouldReturnRecentFiles()
    {
        var now = DateTime.Now;
        var oldDate = now.AddDays(-10);
        var recentDate = now.AddDays(-2);

        CreateFile("old_file.txt", 100);
        File.SetCreationTime(Path.Combine(TestDirectory, "old_file.txt"), oldDate);

        CreateFile("recent_file.txt", 200);
        File.SetCreationTime(Path.Combine(TestDirectory, "recent_file.txt"), recentDate);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { CreatedAfterDays = 5 };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Single(filtered);
        Assert.Equal("recent_file.txt", filtered[0].Name);
    }

    [Fact]
    public async Task Filter_ByCreatedBeforeDays_ShouldReturnOldFiles()
    {
        var now = DateTime.Now;
        var oldDate = now.AddDays(-10);
        var recentDate = now.AddDays(-2);

        CreateFile("old_file.txt", 100);
        File.SetCreationTime(Path.Combine(TestDirectory, "old_file.txt"), oldDate);

        CreateFile("recent_file.txt", 200);
        File.SetCreationTime(Path.Combine(TestDirectory, "recent_file.txt"), recentDate);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria { CreatedBeforeDays = 5 };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Single(filtered);
        Assert.Equal("old_file.txt", filtered[0].Name);
    }

    [Fact]
    public async Task Filter_CombinedDateAndSizeFilters_ShouldApplyBoth()
    {
        var now = DateTime.Now;
        var oldDate = now.AddDays(-10);
        var recentDate = now.AddDays(-2);

        CreateFile("old_small.txt", 100);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "old_small.txt"), oldDate);

        CreateFile("old_large.txt", 500);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "old_large.txt"), oldDate);

        CreateFile("recent_small.txt", 200);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "recent_small.txt"), recentDate);

        CreateFile("recent_large.txt", 600);
        File.SetLastWriteTime(Path.Combine(TestDirectory, "recent_large.txt"), recentDate);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria
        {
            ModifiedBeforeDays = 5,
            MinSize = 300
        };
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Single(filtered);
        Assert.Equal("old_large.txt", filtered[0].Name);
    }

    [Fact]
    public async Task Filter_NoDateCriteria_ShouldReturnAllFiles()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        var criteria = new FilterCriteria();
        var filtered = _analyzer.Filter(result, criteria).ToList();

        Assert.Equal(2, filtered.Count);
    }
}
