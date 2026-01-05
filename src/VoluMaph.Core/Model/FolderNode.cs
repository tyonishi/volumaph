using System;
using System.IO;
using System.Linq;

namespace VoluMaph.Core.Model;

/// <summary>
/// Represents a folder node in the file system.
/// </summary>
public sealed class FolderNode : FileSystemNode
{
    private readonly List<FileSystemNode> _children = new();

    /// <summary>
    /// Gets the read-only list of child nodes (files and subfolders).
    /// </summary>
    public IReadOnlyList<FileSystemNode> Children => _children;

    /// <summary>
    /// Gets or sets the total number of files in this folder and subfolders.
    /// </summary>
    public int FileCount { get; internal set; }

    /// <summary>
    /// Gets or sets the total number of subfolders in this folder.
    /// </summary>
    public int FolderCount { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderNode"/> class.
    /// </summary>
    /// <param name="fullPath">The full path to the folder.</param>
    public FolderNode(string fullPath)
        : base(
            name: Path.GetFileName(fullPath),
            fullPath: fullPath,
            size: 0,
            createdAt: DateTime.MinValue,
            modifiedAt: DateTime.MinValue)
    {
    }

    /// <summary>
    /// Adds a child node to this folder.
    /// </summary>
    /// <param name="node">The node to add (file or subfolder).</param>
    internal void AddChild(FileSystemNode node)
    {
        var newPath = Path.TrimEndingDirectorySeparator(node.FullPath);

        if (_children.Any(existing => Path.TrimEndingDirectorySeparator(existing.FullPath)
            .Equals(newPath, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _children.Add(node);
    }

    /// <summary>
    /// Sorts children: folders first, then by name (ascending).
    /// </summary>
    public void SortChildren()
    {
        _children.Sort((a, b) =>
        {
            if (a.IsFolder && !b.IsFolder) return -1;
            if (!a.IsFolder && b.IsFolder) return 1;
            return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });

        foreach (var child in _children)
        {
            if (child is FolderNode folder)
            {
                folder.SortChildren();
            }
        }
    }
}
