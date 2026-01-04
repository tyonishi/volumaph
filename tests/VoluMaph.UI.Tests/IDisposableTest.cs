using System;
using System.IO;
using Xunit;

namespace VoluMaph.UI.Tests;

/// <summary>
/// Base test class that creates and manages a temporary test directory.
/// </summary>
public abstract class IDisposableTest : IDisposable
{
    /// <summary>
    /// Gets the temporary test directory path.
    /// </summary>
    protected string TestDirectory { get; }

    /// <summary>
    /// Initializes a new instance of test class and creates a temporary test directory.
    /// </summary>
    protected IDisposableTest()
    {
        TestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(TestDirectory);
    }

    /// <summary>
    /// Creates a test file with optional size.
    /// </summary>
    /// <param name="relativePath">The relative path of the file to create.</param>
    /// <param name="size">The size in bytes for the file.</param>
    protected void CreateFile(string relativePath, long size = 0)
    {
        var fullPath = Path.Combine(TestDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(fullPath);
        if (size > 0)
        {
            stream.SetLength(size);
        }
    }

    /// <summary>
    /// Creates a test directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory to create.</param>
    protected void CreateDirectory(string relativePath)
    {
        var fullPath = Path.Combine(TestDirectory, relativePath);
        Directory.CreateDirectory(fullPath);
    }

    /// <summary>
    /// Disposes of test directory and cleans up temporary files.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(TestDirectory))
        {
            try
            {
                Directory.Delete(TestDirectory, true);
            }
            catch
            {
                // Silently ignore cleanup errors
            }
        }
    }
}
