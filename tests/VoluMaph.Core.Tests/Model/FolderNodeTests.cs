using System;
using System.Collections.Generic;
using System.Linq;
using VoluMaph.Core.Model;
using Xunit;

namespace VoluMaph.Core.Tests.Model;

public class FolderNodeTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var folder = new FolderNode(@"C:\test");

        Assert.Equal("test", folder.Name);
        Assert.Equal(@"C:\test", folder.FullPath);
        Assert.Equal(0, folder.Size);
        Assert.Equal(DateTime.MinValue, folder.CreatedAt);
        Assert.Equal(DateTime.MinValue, folder.ModifiedAt);
        Assert.True(folder.IsFolder);
        Assert.Empty(folder.Children);
        Assert.Equal(0, folder.FileCount);
        Assert.Equal(0, folder.FolderCount);
    }

    [Fact]
    public void AddChild_ShouldAddNodeToChildren()
    {
        var folder = new FolderNode(@"C:\test");
        var file = new FileNode(@"C:\test\file.txt", 100, DateTime.Now, DateTime.Now);

        folder.AddChild(file);

        Assert.Single(folder.Children);
        Assert.Same(file, folder.Children[0]);
    }

    [Fact]
    public void Children_ShouldReturnReadOnlyList()
    {
        var folder = new FolderNode(@"C:\test");
        var file = new FileNode(@"C:\test\file.txt", 100, DateTime.Now, DateTime.Now);
        folder.AddChild(file);

        var children = folder.Children;

        Assert.True(children is IReadOnlyList<FileSystemNode>);
    }
}
