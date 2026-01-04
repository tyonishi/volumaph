using System;
using System.Threading;
using System.Threading.Tasks;
using VoluMaph.Core.Model;

namespace VoluMaph.Core.Scanning;

/// <summary>
/// Interface for directory scanning operations.
/// </summary>
public interface IScanner
{
    /// <summary>
    /// Asynchronously scans the directory at the specified path.
    /// </summary>
    /// <param name="rootPath">The root directory path to scan.</param>
    /// <param name="progress">Optional progress reporter for scan updates.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous scan operation, containing the root folder node.</returns>
    Task<FolderNode> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
