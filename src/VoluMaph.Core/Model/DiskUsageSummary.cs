namespace VoluMaph.Core.Model;

/// <summary>
/// Represents summary of disk usage statistics.
/// </summary>
public sealed class DiskUsageSummary
{
    /// <summary>
    /// Gets the total disk size in bytes.
    /// </summary>
    public long TotalSize { get; }

    /// <summary>
    /// Gets the used disk size in bytes.
    /// </summary>
    public long UsedSize { get; }

    /// <summary>
    /// Gets the free disk size in bytes.
    /// </summary>
    public long FreeSize => TotalSize - UsedSize;

    /// <summary>
    /// Gets the total number of files.
    /// </summary>
    public int TotalFiles { get; }

    /// <summary>
    /// Gets the total number of folders.
    /// </summary>
    public int TotalFolders { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskUsageSummary"/> class.
    /// </summary>
    /// <param name="totalSize">The total disk size in bytes.</param>
    /// <param name="usedSize">The used disk size in bytes.</param>
    /// <param name="totalFiles">The total number of files.</param>
    /// <param name="totalFolders">The total number of folders.</param>
    public DiskUsageSummary(long totalSize, long usedSize, int totalFiles, int totalFolders)
    {
        TotalSize = totalSize;
        UsedSize = usedSize;
        TotalFiles = totalFiles;
        TotalFolders = totalFolders;
    }
}
