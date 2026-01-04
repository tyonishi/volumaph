using System;
using System.Collections.Generic;
using System.Linq;
using VoluMaph.Core.Model;

namespace VoluMaph.Core.Analysis;

/// <summary>
/// Provides folder analysis functionality including size aggregation, sorting, and filtering.
/// </summary>
public sealed class FolderAnalyzer : IFolderAnalyzer
{
    /// <summary>
    /// Aggregates folder sizes recursively through all subfolders.
    /// </summary>
    /// <param name="root">The root folder to aggregate.</param>
    public void AggregateFolderSizes(FolderNode root)
    {
        AggregateRecursive(root);
    }

    private long AggregateRecursive(FolderNode folder)
    {
        long totalSize = 0;
        int fileCount = 0;
        int folderCount = 0;

        foreach (var child in folder.Children)
        {
            switch (child)
            {
                case FileNode file:
                    totalSize += file.Size;
                    fileCount++;
                    break;
                case FolderNode subFolder:
                    long subSize = AggregateRecursive(subFolder);
                    totalSize += subSize;
                    folderCount += subFolder.FolderCount + 1;
                    break;
            }
        }

        folder.Size = totalSize;
        folder.FileCount = fileCount;
        folder.FolderCount = folderCount;
        return totalSize;
    }

    /// <summary>
    /// Sorts folder children based on specified option.
    /// </summary>
    /// <param name="folder">The folder to sort.</param>
    /// <param name="option">The sort option.</param>
    /// <returns>A sorted collection of file system nodes.</returns>
    public IEnumerable<FileSystemNode> Sort(FolderNode folder, SortOption option)
    {
        IEnumerable<FileSystemNode> query = folder.Children;

        return option switch
        {
            SortOption.BySizeDescending => query.OrderByDescending(c => c.Size),
            SortOption.BySizeAscending  => query.OrderBy(c => c.Size),
            SortOption.ByNameAscending  => query.OrderBy(c => c.Name),
            SortOption.ByNameDescending => query.OrderByDescending(c => c.Name),
            SortOption.ByFileCountDescending => query.OrderByDescending(c =>
                c is FolderNode f ? f.FileCount : 0),
            _ => query
        };
    }

    /// <summary>
    /// Filters folder children based on specified criteria.
    /// </summary>
    /// <param name="folder">The folder to filter.</param>
    /// <param name="criteria">The filter criteria.</param>
    /// <returns>A filtered collection of file system nodes.</returns>
    public IEnumerable<FileSystemNode> Filter(FolderNode folder, FilterCriteria criteria)
    {
        IEnumerable<FileSystemNode> query = folder.Children;

        if (criteria.MinSize.HasValue)
            query = query.Where(n => n.Size >= criteria.MinSize.Value);

        if (criteria.MaxSize.HasValue)
            query = query.Where(n => n.Size <= criteria.MaxSize.Value);

        if (criteria.Extensions is { Length: > 0 })
            query = query.Where(n =>
                n is FileNode f && criteria.Extensions.Contains(f.Extension));

        if (!criteria.IncludeFiles)
            query = query.Where(n => n is FolderNode);

        if (!criteria.IncludeFolders)
            query = query.Where(n => n is FileNode);

        if (criteria.ModifiedAfterDays.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-criteria.ModifiedAfterDays.Value);
            query = query.Where(n => n.ModifiedAt >= cutoffDate);
        }

        if (criteria.ModifiedBeforeDays.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-criteria.ModifiedBeforeDays.Value);
            query = query.Where(n => n.ModifiedAt < cutoffDate);
        }

        if (criteria.CreatedAfterDays.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-criteria.CreatedAfterDays.Value);
            query = query.Where(n => n.CreatedAt >= cutoffDate);
        }

        if (criteria.CreatedBeforeDays.HasValue)
        {
            var cutoffDate = DateTime.Now.AddDays(-criteria.CreatedBeforeDays.Value);
            query = query.Where(n => n.CreatedAt < cutoffDate);
        }

        return query;
    }
}
