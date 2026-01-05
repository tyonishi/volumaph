using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoluMaph.Core.Model;

namespace VoluMaph.Core.Scanning;

/// <summary>
/// Provides directory scanning functionality to build a file system tree.
/// </summary>
public sealed class DirectoryScanner : IScanner
{
    private DateTime _lastProgressReport = DateTime.MinValue;
    private const int ProgressReportIntervalMs = 100;
    private long _processedFiles;
    private long _processedBytes;

    /// <summary>
    /// Asynchronously scans the directory at the specified root path.
    /// </summary>
    /// <param name="rootPath">The root directory path to scan.</param>
    /// <param name="progress">Optional progress reporter for scan updates.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous scan operation, containing the root folder node.</returns>
    public async Task<FolderNode> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _lastProgressReport = DateTime.MinValue;
        _processedFiles = 0;
        _processedBytes = 0;

        var root = new FolderNode(rootPath);
        await ScanDirectoryRecursiveAsync(root, progress, cancellationToken).ConfigureAwait(false);
        return root;
    }

    private async Task ScanDirectoryRecursiveAsync(
        FolderNode folder,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            foreach (var filePath in Directory.EnumerateFiles(folder.FullPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var file = await GetFileNodeAsync(filePath, cancellationToken);
                    folder.AddChild(file);
                    _processedFiles++;
                    _processedBytes += file.Size;

                    ReportProgress(progress, filePath, cancellationToken);
                }
                catch (UnauthorizedAccessException)
                {
                    // Silently skip files we don't have access to
                }
                catch (IOException)
                {
                    // Silently skip files that can't be read
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Silently skip directories we don't have access to
            return;
        }
        catch (OperationCanceledException)
        {
            // Re-throw to allow cancellation to propagate
            throw;
        }
        catch (IOException)
        {
            // Silently skip directories that can't be read
            return;
        }
        catch (Exception)
        {
            // Silently skip directories that cause unexpected errors
            return;
        }

        try
        {
            foreach (var dirPath in Directory.EnumerateDirectories(folder.FullPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var subFolder = new FolderNode(dirPath);
                    folder.AddChild(subFolder);
                    await ScanDirectoryRecursiveAsync(subFolder, progress, cancellationToken).ConfigureAwait(false);

                    ReportProgress(progress, subFolder.FullPath, cancellationToken);
                }
                catch (UnauthorizedAccessException)
                {
                    // Silently skip directories we don't have access to
                }
                catch (IOException)
                {
                    // Silently skip directories that can't be read
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Silently skip directories we don't have access to
        }
        catch (OperationCanceledException)
        {
            // Re-throw to allow cancellation to propagate
            throw;
        }
        catch (IOException)
        {
            // Silently skip directories that can't be read
        }
        catch (Exception)
        {
            // Silently skip directories that cause unexpected errors
        }
    }

    /// <summary>
    /// Creates a file node for the specified file path asynchronously.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the file node.</returns>
    private static Task<FileNode> GetFileNodeAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileInfo = new FileInfo(filePath);
        return Task.FromResult(new FileNode(
            fileInfo.FullName,
            fileInfo.Length,
            fileInfo.CreationTime,
            fileInfo.LastWriteTime));
    }

    /// <summary>
    /// Reports progress to the progress reporter if enough time has passed.
    /// </summary>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="currentPath">The current path being processed.</param>
    /// <param name="cancellationToken">Cancellation token to check after reporting progress.</param>
    private void ReportProgress(IProgress<ScanProgress>? progress, string currentPath, CancellationToken cancellationToken)
    {
        if (progress != null && (DateTime.UtcNow - _lastProgressReport).TotalMilliseconds > ProgressReportIntervalMs)
        {
            progress.Report(new ScanProgress(currentPath, _processedFiles, _processedBytes));
            _lastProgressReport = DateTime.UtcNow;
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
