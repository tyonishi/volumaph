using System;
using VoluMaph.Core.Model;
using Xunit;

namespace VoluMaph.Core.Tests.Model;

public class FileNodeTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var file = new FileNode(
            @"C:\test\file.txt",
            1024,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 2));

        Assert.Equal("file.txt", file.Name);
        Assert.Equal(@"C:\test\file.txt", file.FullPath);
        Assert.Equal(1024, file.Size);
        Assert.Equal(new DateTime(2024, 1, 1), file.CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 2), file.ModifiedAt);
        Assert.Equal(".txt", file.Extension);
        Assert.False(file.IsFolder);
    }

    [Fact]
    public void Constructor_ShouldNormalizeExtensionToLower()
    {
        var file = new FileNode(@"C:\test\FILE.TXT", 0, DateTime.Now, DateTime.Now);

        Assert.Equal(".txt", file.Extension);
    }
}
