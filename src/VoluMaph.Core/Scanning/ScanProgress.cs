namespace VoluMaph.Core.Scanning;

/// <summary>
/// Represents progress information during a directory scan operation.
/// </summary>
public sealed class ScanProgress
{
    /// <summary>
    /// Gets the current file or folder path being processed.
    /// </summary>
    public string CurrentPath { get; }

    /// <summary>
    /// Gets the total number of files processed so far.
    /// </summary>
    public long ProcessedFiles { get; }

    /// <summary>
    /// Gets the total number of bytes processed so far.
    /// </summary>
    public long ProcessedBytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanProgress"/> class.
    /// </summary>
    /// <param name="currentPath">The current file or folder path being processed.</param>
    /// <param name="processedFiles">The total number of files processed.</param>
    /// <param name="processedBytes">The total number of bytes processed.</param>
    public ScanProgress(string currentPath, long processedFiles, long processedBytes)
    {
        CurrentPath = currentPath;
        ProcessedFiles = processedFiles;
        ProcessedBytes = processedBytes;
    }
}
