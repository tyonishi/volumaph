using System.Collections.Generic;
using System.Linq;
using VoluMaph.Core.Model;

namespace VoluMaph.Core.Analysis;

/// <summary>
/// Provides file extension analysis functionality.
/// </summary>
public sealed class ExtensionAnalyzer : IExtensionAnalyzer
{
    /// <summary>
    /// Analyzes files by extension and returns statistics.
    /// </summary>
    /// <param name="root">The root folder to analyze.</param>
    /// <returns>A collection of extension analysis results sorted by total size.</returns>
    public IEnumerable<ExtensionAnalysisResult> AnalyzeByExtension(FolderNode root)
    {
        var extensionGroups = new Dictionary<string, List<FileNode>>();

        CollectFilesByExtension(root, extensionGroups);

        var totalSize = extensionGroups.Values.SelectMany(v => v).Sum(f => f.Size);

        return extensionGroups
            .Select(g => new ExtensionAnalysisResult
            {
                Extension = g.Key,
                TotalSize = g.Value.Sum(f => f.Size),
                FileCount = g.Value.Count,
                Percentage = totalSize > 0 ? (g.Value.Sum(f => f.Size) * 100.0 / totalSize) : 0
            })
            .OrderByDescending(r => r.TotalSize);
    }

    private static void CollectFilesByExtension(FolderNode folder, Dictionary<string, List<FileNode>> groups)
    {
        foreach (var child in folder.Children)
        {
            if (child is FileNode file)
            {
                var extension = string.IsNullOrEmpty(file.Extension) ? "(none)" : file.Extension;
                if (!groups.ContainsKey(extension))
                {
                    groups[extension] = new List<FileNode>();
                }
                groups[extension].Add(file);
            }
            else if (child is FolderNode subFolder)
            {
                CollectFilesByExtension(subFolder, groups);
            }
        }
    }
}
