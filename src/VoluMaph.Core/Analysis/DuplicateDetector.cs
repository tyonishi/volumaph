using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoluMaph.Core.Model;

namespace VoluMaph.Core.Analysis;

/// <summary>
/// Provides duplicate file detection functionality.
/// </summary>
public sealed class DuplicateDetector : IDuplicateDetector
{
    /// <summary>
    /// Detects duplicate files by comparing their sizes.
    /// </summary>
    /// <param name="root">The root folder to analyze.</param>
    /// <returns>A collection of duplicate file groups (files with same size).</returns>
    public IEnumerable<DuplicateGroup> DetectDuplicatesBySize(FolderNode root)
    {
        var fileGroups = new Dictionary<long, List<FileNode>>();

        CollectFilesBySize(root, fileGroups);

        return fileGroups
            .Where(g => g.Value.Count > 1)
            .Select(g => new DuplicateGroup
            {
                FileSize = g.Key,
                Files = g.Value
            })
            .OrderByDescending(g => g.FileSize);
    }

    /// <summary>
    /// Detects duplicate files by calculating their SHA256 hash values.
    /// </summary>
    /// <param name="root">The root folder to analyze.</param>
    /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
    /// <returns>A task representing the asynchronous operation, containing groups of duplicate files.</returns>
    public async Task<IEnumerable<DuplicateGroup>> DetectDuplicatesByHash(FolderNode root, CancellationToken cancellationToken = default)
    {
        var hashGroups = new Dictionary<string, List<FileNode>>();

        await CollectFilesByHash(root, hashGroups, cancellationToken).ConfigureAwait(false);

        return hashGroups
            .Where(g => g.Value.Count > 1)
            .Select(g => new DuplicateGroup
            {
                Hash = g.Key,
                FileSize = g.Value.First().Size,
                Files = g.Value
            })
            .OrderByDescending(g => g.FileSize);
    }

    private static void CollectFilesBySize(FolderNode folder, Dictionary<long, List<FileNode>> groups)
    {
        foreach (var child in folder.Children)
        {
            if (child is FileNode file)
            {
                if (!groups.TryGetValue(file.Size, out var fileList))
                {
                    fileList = new List<FileNode>();
                    groups[file.Size] = fileList;
                }
                fileList.Add(file);
            }
            else if (child is FolderNode subFolder)
            {
                CollectFilesBySize(subFolder, groups);
            }
        }
    }

    private static async Task CollectFilesByHash(FolderNode folder, Dictionary<string, List<FileNode>> groups, CancellationToken cancellationToken)
    {
        foreach (var child in folder.Children)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (child is FileNode file)
            {
                try
                {
                    var hash = await ComputeFileHashAsync(file.FullPath, cancellationToken);
                    
                    if (!groups.TryGetValue(hash, out var fileList))
                    {
                        fileList = new List<FileNode>();
                        groups[hash] = fileList;
                    }
                    fileList.Add(file);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                }
            }
            else if (child is FolderNode subFolder)
            {
                await CollectFilesByHash(subFolder, groups, cancellationToken);
            }
        }
    }

    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
