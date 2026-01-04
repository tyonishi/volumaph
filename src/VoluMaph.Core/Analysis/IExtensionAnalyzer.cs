using VoluMaph.Core.Model;

namespace VoluMaph.Core.Analysis;

/// <summary>
/// Represents analysis result for a specific file extension.
/// </summary>
public sealed class ExtensionAnalysisResult
{
    /// <summary>
    /// Gets or sets the file extension (e.g., ".txt").
    /// </summary>
    public string Extension { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the total size in bytes for this extension.
    /// </summary>
    public long TotalSize { get; init; }

    /// <summary>
    /// Gets or sets the number of files with this extension.
    /// </summary>
    public int FileCount { get; init; }

    /// <summary>
    /// Gets or sets the percentage of total size for this extension.
    /// </summary>
    public double Percentage { get; init; }
}

/// <summary>
/// Interface for analyzing file extensions.
/// </summary>
public interface IExtensionAnalyzer
{
    /// <summary>
    /// Analyzes files by extension and returns statistics.
    /// </summary>
    /// <param name="root">The root folder to analyze.</param>
    /// <returns>A collection of extension analysis results sorted by total size.</returns>
    IEnumerable<ExtensionAnalysisResult> AnalyzeByExtension(FolderNode root);
}
