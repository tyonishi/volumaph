using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using Xunit;

namespace VoluMaph.Core.Tests.Analysis;

public class DuplicateDetectorTests : IDisposableTest
{
    private readonly DirectoryScanner _scanner = new();
    private readonly DuplicateDetector _detector = new();

    [Fact]
    public async Task DetectDuplicatesBySize_ShouldFindFilesWithSameSize()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 100);
        CreateFile("file3.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        var duplicates = _detector.DetectDuplicatesBySize(result).ToList();

        Assert.Single(duplicates);
        Assert.Equal(2, duplicates[0].Files.Count);
        Assert.Contains(duplicates[0].Files, f => f.Name == "file1.txt");
        Assert.Contains(duplicates[0].Files, f => f.Name == "file2.txt");
    }

    [Fact]
    public async Task DetectDuplicatesBySize_ShouldHandleNoDuplicates()
    {
        CreateFile("file1.txt", 100);
        CreateFile("file2.txt", 200);
        CreateFile("file3.txt", 300);

        var result = await _scanner.ScanAsync(TestDirectory);
        var duplicates = _detector.DetectDuplicatesBySize(result).ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public async Task DetectDuplicatesBySize_ShouldHandleNestedFolders()
    {
        CreateFile("root.txt", 100);
        CreateDirectory("subfolder");
        CreateFile("subfolder/nested.txt", 100);
        CreateFile("subfolder/file.txt", 200);

        var result = await _scanner.ScanAsync(TestDirectory);
        var duplicates = _detector.DetectDuplicatesBySize(result).ToList();

        Assert.Single(duplicates);
        Assert.Equal(2, duplicates[0].Files.Count);
    }

    [Fact]
    public async Task DetectDuplicatesByHash_ShouldFindExactDuplicates()
    {
        var content1 = Encoding.UTF8.GetBytes("same content");
        var content2 = Encoding.UTF8.GetBytes("same content");
        var content3 = Encoding.UTF8.GetBytes("different content");

        WriteFile("file1.txt", content1);
        WriteFile("file2.txt", content2);
        WriteFile("file3.txt", content3);

        var result = await _scanner.ScanAsync(TestDirectory);
        var duplicates = (await _detector.DetectDuplicatesByHash(result)).ToList();

        Assert.Single(duplicates);
        Assert.Equal(2, duplicates[0].Files.Count);
        Assert.Contains(duplicates[0].Files, f => f.Name == "file1.txt");
        Assert.Contains(duplicates[0].Files, f => f.Name == "file2.txt");
    }

    [Fact]
    public async Task DetectDuplicatesByHash_ShouldHandleNoDuplicates()
    {
        WriteFile("file1.txt", Encoding.UTF8.GetBytes("content1"));
        WriteFile("file2.txt", Encoding.UTF8.GetBytes("content2"));
        WriteFile("file3.txt", Encoding.UTF8.GetBytes("content3"));

        var result = await _scanner.ScanAsync(TestDirectory);
        var duplicates = (await _detector.DetectDuplicatesByHash(result)).ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public async Task DetectDuplicatesByHash_ShouldHandleMultipleGroups()
    {
        var content1 = Encoding.UTF8.GetBytes("same1");
        var content2 = Encoding.UTF8.GetBytes("same1");
        var content3 = Encoding.UTF8.GetBytes("same2");
        var content4 = Encoding.UTF8.GetBytes("same2");

        WriteFile("file1.txt", content1);
        WriteFile("file2.txt", content2);
        WriteFile("file3.txt", content3);
        WriteFile("file4.txt", content4);

        var result = await _scanner.ScanAsync(TestDirectory);
        var duplicates = (await _detector.DetectDuplicatesByHash(result)).ToList();

        Assert.Equal(2, duplicates.Count);
        Assert.True(duplicates.All(d => d.Files.Count == 2));
    }

    private void WriteFile(string relativePath, byte[] content)
    {
        var fullPath = Path.Combine(TestDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllBytes(fullPath, content);
    }
}
