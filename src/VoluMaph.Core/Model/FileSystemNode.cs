namespace VoluMaph.Core.Model;

/// <summary>
/// Abstract base class representing a node in the file system.
/// </summary>
public abstract class FileSystemNode
{
    /// <summary>
    /// Gets the name of the file or folder.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the full path of the file or folder.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets or sets the size in bytes.
    /// </summary>
    public long Size { get; internal set; }

    /// <summary>
    /// Gets the creation date/time of the file or folder.
    /// </summary>
    public DateTime? CreatedAt { get; }

    /// <summary>
    /// Gets the last modified date/time of the file or folder.
    /// </summary>
    public DateTime? ModifiedAt { get; }

    /// <summary>
    /// Gets a value indicating whether this node is a folder.
    /// </summary>
    public bool IsFolder => this is FolderNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemNode"/> class.
    /// </summary>
    /// <param name="name">The name of the file or folder.</param>
    /// <param name="fullPath">The full path of the file or folder.</param>
    /// <param name="size">The size in bytes.</param>
    /// <param name="createdAt">The creation date/time.</param>
    /// <param name="modifiedAt">The last modified date/time.</param>
    protected FileSystemNode(
        string name,
        string fullPath,
        long size,
        DateTime? createdAt,
        DateTime? modifiedAt)
    {
        Name = name;
        FullPath = fullPath;
        Size = size;
        CreatedAt = createdAt;
        ModifiedAt = modifiedAt;
    }
}
