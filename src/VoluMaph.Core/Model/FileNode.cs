namespace VoluMaph.Core.Model;

/// <summary>
/// Represents a file node in the file system.
/// </summary>
public sealed class FileNode : FileSystemNode
{
    /// <summary>
    /// Gets the file extension including the dot (e.g., ".txt").
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNode"/> class.
    /// </summary>
    /// <param name="fullPath">The full path to the file.</param>
    /// <param name="size">The file size in bytes.</param>
    /// <param name="createdAt">The creation date/time of the file.</param>
    /// <param name="modifiedAt">The last modified date/time of the file.</param>
    public FileNode(
        string fullPath,
        long size,
        DateTime createdAt,
        DateTime modifiedAt)
        : base(
            name: System.IO.Path.GetFileName(fullPath),
            fullPath: fullPath,
            size: size,
            createdAt: createdAt,
            modifiedAt: modifiedAt)
    {
        Extension = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
    }
}
