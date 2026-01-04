using System;
using System.Collections.Generic;
using System.Linq;
using VoluMaph.Core.Model;

namespace VoluMaph.Core.Analysis;

/// <summary>
/// Interface for analyzing folders.
/// </summary>
public interface IFolderAnalyzer
{
    /// <summary>
    /// Aggregates folder sizes recursively through all subfolders.
    /// </summary>
    /// <param name="root">The root folder to aggregate.</param>
    void AggregateFolderSizes(FolderNode root);

    /// <summary>
    /// Sorts folder children based on specified option.
    /// </summary>
    /// <param name="folder">The folder to sort.</param>
    /// <param name="option">The sort option.</param>
    /// <returns>A sorted collection of file system nodes.</returns>
    IEnumerable<FileSystemNode> Sort(FolderNode folder, SortOption option);

    /// <summary>
    /// Filters folder children based on specified criteria.
    /// </summary>
    /// <param name="folder">The folder to filter.</param>
    /// <param name="criteria">The filter criteria.</param>
    /// <returns>A filtered collection of file system nodes.</returns>
    IEnumerable<FileSystemNode> Filter(
        FolderNode folder,
        FilterCriteria criteria);
}

/// <summary>
/// Sort options for folder analysis.
/// </summary>
public enum SortOption
{
    /// <summary>Sort by file size in descending order.</summary>
    BySizeDescending,
    /// <summary>Sort by file size in ascending order.</summary>
    BySizeAscending,
    /// <summary>Sort by file name in ascending order.</summary>
    ByNameAscending,
    /// <summary>Sort by file name in descending order.</summary>
    ByNameDescending,
    /// <summary>Sort by file count in descending order.</summary>
    ByFileCountDescending
}

/// <summary>
/// Filter criteria for folder analysis.
/// </summary>
public sealed class FilterCriteria
{
    /// <summary>Gets or sets minimum file size filter (bytes).</summary>
    public long? MinSize { get; init; }

    /// <summary>Gets or sets maximum file size filter (bytes).</summary>
    public long? MaxSize { get; init; }

    /// <summary>Gets or sets array of file extensions to filter by.</summary>
    public string[]? Extensions { get; init; }

    /// <summary>Gets or sets whether to include folders in results.</summary>
    public bool IncludeFolders { get; init; } = true;

    /// <summary>Gets or sets whether to include files in results.</summary>
    public bool IncludeFiles { get; init; } = true;

    /// <summary>Gets or sets filter for files modified after N days ago.</summary>
    public int? ModifiedAfterDays { get; init; }

    /// <summary>Gets or sets filter for files modified before N days ago.</summary>
    public int? ModifiedBeforeDays { get; init; }

    /// <summary>Gets or sets filter for files created after N days ago.</summary>
    public int? CreatedAfterDays { get; init; }

    /// <summary>Gets or sets filter for files created before N days ago.</summary>
    public int? CreatedBeforeDays { get; init; }
}
