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
/// Represents a group of duplicate files.
/// </summary>
public sealed class DuplicateGroup
{
    /// <summary>
    /// Gets or sets the file size in bytes for this duplicate group.
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Gets or sets the file hash (if calculated by hash).
    /// </summary>
    public string? Hash { get; init; }

    /// <summary>
    /// Gets or sets the list of duplicate files in this group.
    /// </summary>
    public List<FileNode> Files { get; init; } = new();
}

/// <summary>
/// Interface for detecting duplicate files.
/// </summary>
public interface IDuplicateDetector
{
    /// <summary>
    /// Detects duplicate files by calculating their hash values.
    /// </summary>
    /// <param name="root">The root folder to analyze.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing groups of duplicate files.</returns>
    Task<IEnumerable<DuplicateGroup>> DetectDuplicatesByHash(FolderNode root, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects duplicate files by comparing their sizes.
    /// </summary>
    /// <param name="root">The root folder to analyze.</param>
    /// <returns>A collection of duplicate file groups (files with same size).</returns>
    IEnumerable<DuplicateGroup> DetectDuplicatesBySize(FolderNode root);
}
