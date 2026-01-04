# VoluMaph Detailed Design Document

---

## Overall Architecture Review

The goal is to build an **extensible C# application with clean separation between scan engine and UI**.

The layered architecture is as follows:

```text
VoluMaph
 ├─ VoluMaph.Core          … Scan・Model・Analysis ✅ Implemented
 ├─ VoluMaph.Infrastructure … Settings・Logging・Native Interop ✅ Implemented
 └─ VoluMaph.UI             … WPF/WinUI + MVVM ✅ Implemented
```

### Implementation Status Summary
| Layer | Module | Status | Tests |
|--------|-----------|--------|--------|
| Core | Scanner (DirectoryScanner) | ✅ Complete | 6 tests |
| Core | FileSystemModel | ✅ Complete | 5 tests |
| Core | Analyzer (FolderAnalyzer) | ✅ Complete | 12 tests |
| Core | ExtensionAnalyzer | ✅ Complete | 6 tests |
| Core | DuplicateDetector | ✅ Complete | 6 tests |
| Infrastructure | Settings | ✅ Complete | - |
| Infrastructure | Logging | ✅ Complete | - |
| UI | MainViewModel | ✅ Complete | 28 tests |
| UI | MainWindow & Controls | ✅ Complete | - |
| **Total** | | **✅ Complete** | **69 tests** |

---

## Core Layer Details

### Scanning Interface Design

First, abstract the scanner with an interface.

```csharp
// VoluMaph.Core/Scanning/IScanner.cs
public interface IScanner
{
    Task<FolderNode> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
```

```csharp
// VoluMaph.Core/Scanning/ScanProgress.cs
public sealed class ScanProgress
{
    public string CurrentPath { get; }
    public long ProcessedFiles { get; }
    public long ProcessedBytes { get; }

    public ScanProgress(string currentPath, long processedFiles, long processedBytes)
    {
        CurrentPath = currentPath;
        ProcessedFiles = processedFiles;
        ProcessedBytes = processedBytes;
    }
}
```

```csharp
// VoluMaph.Core/Scanning/ScanProgressEventArgs.cs
public sealed class ScanProgressEventArgs : EventArgs
{
    public string CurrentPath { get; }
    public long ProcessedFiles { get; }
    public long ProcessedBytes { get; }

    public ScanProgressEventArgs(string currentPath, long processedFiles, long processedBytes)
    {
        CurrentPath = currentPath;
        ProcessedFiles = processedFiles;
        ProcessedBytes = processedBytes;
    }
}
```

#### Normal Scanner (DirectoryScanner)

```csharp
// VoluMaph.Core/Scanning/DirectoryScanner.cs
public sealed class DirectoryScanner : IScanner
{
    public async Task<FolderNode> ScanAsync(
        string rootPath,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var root = new FolderNode(rootPath);
            ScanDirectory(root, progress, cancellationToken);
            return root;
        }, cancellationToken);
    }

    private void ScanDirectory(
        FolderNode folder,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        // Use Directory.EnumerateFiles / EnumerateDirectories for recursive scan
        // Report progress with progress?.Report() (IProgress<T> is thread-safe)
    }
}
```

Future `MftScanner` can be swapped in simply by implementing the same `IScanner` interface.

---

### FileSystem Model Design

Common models used throughout the application.

```csharp
// VoluMaph.Core/Model/FileSystemNode.cs
public abstract class FileSystemNode
{
    public string Name { get; }
    public string FullPath { get; }
    public long Size { get; internal set; }
    public DateTime? CreatedAt { get; }
    public DateTime? ModifiedAt { get; }

    public bool IsFolder => this is FolderNode;

    protected FileSystemNode(
        string name,
        string fullPath,
        long size,
        DateTime? createdAt,
        DateTime? modifiedAt)
    {
        Name = name;
        FullPath = fullPath;
        Size = size;
        CreatedAt = createdAt;
        ModifiedAt = modifiedAt;
    }
}
```

```csharp
// VoluMaph.Core/Model/FolderNode.cs
public sealed class FolderNode : FileSystemNode
{
    private readonly List<FileSystemNode> _children = new();

    public IReadOnlyList<FileSystemNode> Children => _children;

    public int FileCount { get; internal set; }
    public int FolderCount { get; internal set; }

    public FolderNode(string fullPath)
        : base(
            name: Path.GetFileName(fullPath),
            fullPath: fullPath,
            size: 0,
            createdAt: null,
            modifiedAt: null)
    {
    }

    internal void AddChild(FileSystemNode node)
    {
        _children.Add(node);
    }
}
```

```csharp
// VoluMaph.Core/Model/FileNode.cs
public sealed class FileNode : FileSystemNode
{
    public string Extension { get; }

    public FileNode(
        string fullPath,
        long size,
        DateTime createdAt,
        DateTime modifiedAt)
        : base(
            name: Path.GetFileName(fullPath),
            fullPath: fullPath,
            size: size,
            createdAt: createdAt,
            modifiedAt: modifiedAt)
    {
        Extension = Path.GetExtension(fullPath).ToLowerInvariant();
    }
}
```

#### Disk Statistics

```csharp
// VoluMaph.Core/Model/DiskUsageSummary.cs
public sealed class DiskUsageSummary
{
    public long TotalSize { get; }
    public long UsedSize { get; }
    public long FreeSize => TotalSize - UsedSize;

    public int TotalFiles { get; }
    public int TotalFolders { get; }

    public DiskUsageSummary(long totalSize, long usedSize, int totalFiles, int totalFolders)
    {
        TotalSize = totalSize;
        UsedSize = usedSize;
        TotalFiles = totalFiles;
        TotalFolders = totalFolders;
    }
}
```

---

### Analyzer Layer Details

Handles folder size aggregation, sorting, and filtering.

```csharp
// VoluMaph.Core/Analysis/IFolderAnalyzer.cs
public interface IFolderAnalyzer
{
    void AggregateFolderSizes(FolderNode root);
    IEnumerable<FileSystemNode> Sort(FolderNode folder, SortOption option);
    IEnumerable<FileSystemNode> Filter(
        FolderNode folder,
        FilterCriteria criteria);
}
```

```csharp
// VoluMaph.Core/Analysis/SortOption.cs
public enum SortOption
{
    BySizeDescending,
    BySizeAscending,
    ByNameAscending,
    ByNameDescending,
    ByFileCountDescending
}
```

```csharp
// VoluMaph.Core/Analysis/FilterCriteria.cs
public sealed class FilterCriteria
{
    public long? MinSize { get; init; }
    public long? MaxSize { get; init; }
    public string[]? Extensions { get; init; }
    public bool IncludeFolders { get; init; } = true;
    public bool IncludeFiles { get; init; } = true;
    public int? ModifiedAfterDays { get; init; }
    public int? ModifiedBeforeDays { get; init; }
    public int? CreatedAfterDays { get; init; }
    public int? CreatedBeforeDays { get; init; }
}
```

```csharp
// VoluMaph.Core/Analysis/FolderAnalyzer.cs
public sealed class FolderAnalyzer : IFolderAnalyzer
{
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
                    folderCount++;
                    break;
            }
        }

        folder.Size = totalSize;
        folder.FileCount = fileCount;
        folder.FolderCount = folderCount;
        return totalSize;
    }

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
```

#### Extension-Based Aggregation Analyzer

```csharp
// VoluMaph.Core/Analysis/IExtensionAnalyzer.cs
public interface IExtensionAnalyzer
{
    IEnumerable<ExtensionAnalysisResult> AnalyzeByExtension(FolderNode root);
}

public sealed class ExtensionAnalysisResult
{
    public string Extension { get; init; } = string.Empty;
    public long TotalSize { get; init; }
    public int FileCount { get; init; }
    public double Percentage { get; init; }
}

// VoluMaph.Core/Analysis/ExtensionAnalyzer.cs
public sealed class ExtensionAnalyzer : IExtensionAnalyzer
{
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
```

#### Duplicate File Detection Analyzer

```csharp
// VoluMaph.Core/Analysis/IDuplicateDetector.cs
public interface IDuplicateDetector
{
    Task<IEnumerable<DuplicateGroup>> DetectDuplicatesByHash(FolderNode root, CancellationToken cancellationToken = default);
    IEnumerable<DuplicateGroup> DetectDuplicatesBySize(FolderNode root);
}

public sealed class DuplicateGroup
{
    public long FileSize { get; init; }
    public string? Hash { get; init; }
    public List<FileNode> Files { get; init; } = new();
}
```

---

## Infrastructure Layer Details

### Settings Management

```csharp
// VoluMaph.Infrastructure/Settings/AppSettings.cs
public sealed class AppSettings
{
    public string LastSelectedDrive { get; set; } = string.Empty;
    public SortOption DefaultSortOption { get; set; } = SortOption.BySizeDescending;
    public bool IsDarkTheme { get; set; } = true;
}
```

```csharp
// VoluMaph.Infrastructure/Settings/ISettingsProvider.cs
public interface ISettingsProvider
{
    AppSettings Load();
    void Save(AppSettings settings);
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
```

```csharp
// VoluMaph.Infrastructure/Settings/JsonSettingsProvider.cs
public sealed class JsonSettingsProvider : ISettingsProvider
{
    private readonly string _filePath;

    public JsonSettingsProvider(string filePath)
    {
        _filePath = filePath;
    }

    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
            return new AppSettings();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<AppSettings>(json)
               ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_filePath, json);
    }
}
```

### Logging (Simple Version)

```csharp
// VoluMaph.Infrastructure/Logging/ILogger.cs
public interface ILogger
{
    void Info(string message);
    void Error(string message, Exception? ex = null);
}
```

```csharp
// VoluMaph.Infrastructure/Logging/SimpleLogger.cs
public sealed class SimpleLogger : ILogger
{
    private readonly string _logFilePath;

    public SimpleLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public void Info(string message) => Write("INFO", message);

    public void Error(string message, Exception? ex = null)
        => Write("ERROR", $"{message} {ex}");

    private void Write(string level, string message)
    {
        var line = $"{DateTime.Now:O} [{level}] {message}";
        File.AppendAllLines(_logFilePath, new[] { line });
    }
}
```

---

## UI Layer (MVVM) Details

Examples assume WPF, but the structure is nearly identical for WinUI 3.

### ViewModel Base Class

```csharp
// VoluMaph.UI/ViewModels/ViewModelBase.cs
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

### RelayCommand (Async-Support)

```csharp
// VoluMaph.UI/Commands/RelayCommand.cs
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

```csharp
// VoluMaph.UI/Commands/AsyncRelayCommand.cs
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (_isExecuting)
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(parameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

### MainViewModel

```csharp
// VoluMaph.UI/ViewModels/MainViewModel.cs
public sealed class MainViewModel : ViewModelBase
{
    private readonly IScanner _scanner;
    private readonly IFolderAnalyzer _analyzer;
    private readonly IExtensionAnalyzer _extensionAnalyzer;
    private readonly IDuplicateDetector _duplicateDetector;
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger _logger;

    private FolderNode? _rootFolder;
    private FileSystemNode? _selectedNode;
    private bool _isScanning;
    private double _progress;
    private string _statusMessage = string.Empty;
    private string _selectedDrive = string.Empty;
    private string _searchText = string.Empty;
    private bool _isDarkTheme = true;
    private int? _modifiedAfterDays;
    private int? _modifiedBeforeDays;
    private int? _createdAfterDays;
    private int? _createdBeforeDays;

    public ObservableCollection<string> AvailableDrives { get; } = new();
    public ObservableCollection<FileSystemNode> CurrentChildren { get; } = new();
    public ObservableCollection<ExtensionAnalysisResult> ExtensionAnalysis { get; } = new();
    public ObservableCollection<DuplicateGroup> DuplicateGroups { get; } = new();

    public FolderNode? RootFolder
    {
        get => _rootFolder;
        private set { _rootFolder = value; RaisePropertyChanged(); }
    }

    public FileSystemNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set { _isScanning = value; RaisePropertyChanged(); }
    }

    public double Progress
    {
        get => _progress;
        private set { _progress = value; RaisePropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; RaisePropertyChanged(); }
    }

    public string SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            _selectedDrive = value;
            RaisePropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
        }
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        private set
        {
            _isDarkTheme = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(ToggleThemeText));
        }
    }

    public string ToggleThemeText => IsDarkTheme ? "☀ Switch to Light Mode" : "🌙 Switch to Dark Mode";

    public int? ModifiedAfterDays
    {
        get => _modifiedAfterDays;
        set
        {
            _modifiedAfterDays = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
        }
    }

    public int? ModifiedBeforeDays
    {
        get => _modifiedBeforeDays;
        set
        {
            _modifiedBeforeDays = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
        }
    }

    public int? CreatedAfterDays
    {
        get => _createdAfterDays;
        set
        {
            _createdAfterDays = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
        }
    }

    public int? CreatedBeforeDays
    {
        get => _createdBeforeDays;
        set
        {
            _createdBeforeDays = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
        }
    }

    public ICommand ScanCommand { get; }
    public ICommand RefreshDrivesCommand { get; }
    public ICommand OpenInExplorerCommand { get; }
    public ICommand CopyPathCommand { get; }
    public ICommand CopyNameCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ExportToCsvCommand { get; }
    public ICommand ExportToHtmlCommand { get; }
    public ICommand AnalyzeExtensionsCommand { get; }
    public ICommand DetectDuplicatesBySizeCommand { get; }
    public ICommand DetectDuplicatesByHashCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    public MainViewModel(
        IScanner scanner,
        IFolderAnalyzer analyzer,
        IExtensionAnalyzer extensionAnalyzer,
        IDuplicateDetector duplicateDetector,
        ISettingsProvider settingsProvider,
        ILogger logger)
    {
        _scanner = scanner;
        _analyzer = analyzer;
        _extensionAnalyzer = extensionAnalyzer;
        _duplicateDetector = duplicateDetector;
        _settingsProvider = settingsProvider;
        _logger = logger;

        ScanCommand = new AsyncRelayCommand(async _ => await ScanAsync(), _ => !IsScanning && !string.IsNullOrEmpty(SelectedDrive));
        RefreshDrivesCommand = new RelayCommand(_ => LoadDrives());
        OpenInExplorerCommand = new RelayCommand(_ => OpenInExplorer(), _ => SelectedNode != null);
        CopyPathCommand = new RelayCommand(_ => CopyPath(), _ => SelectedNode != null);
        CopyNameCommand = new RelayCommand(_ => CopyName(), _ => SelectedNode != null);
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        ExportToCsvCommand = new AsyncRelayCommand(async _ => await ExportToCsvAsync(), _ => !IsScanning && CurrentChildren.Count > 0);
        ExportToHtmlCommand = new AsyncRelayCommand(async _ => await ExportToHtmlAsync(), _ => !IsScanning && CurrentChildren.Count > 0);
        AnalyzeExtensionsCommand = new AsyncRelayCommand(async _ => await AnalyzeExtensionsAsync(), _ => !IsScanning && RootFolder != null);
        DetectDuplicatesBySizeCommand = new AsyncRelayCommand(async _ => await DetectDuplicatesBySizeAsync(), _ => !IsScanning && RootFolder != null);
        DetectDuplicatesByHashCommand = new AsyncRelayCommand(async _ => await DetectDuplicatesByHashAsync(), _ => !IsScanning && RootFolder != null);
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

        LoadDrives();
        LoadSettings();
    }

    private void LoadDrives()
    {
        AvailableDrives.Clear();
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            AvailableDrives.Add(drive.Name);
        }
    }

    private void LoadSettings()
    {
        var settings = _settingsProvider.Load();
        if (AvailableDrives.Contains(settings.LastSelectedDrive))
        {
            SelectedDrive = settings.LastSelectedDrive;
        }
        else if (AvailableDrives.Count > 0)
        {
            SelectedDrive = AvailableDrives[0];
        }

        IsDarkTheme = settings.IsDarkTheme;
    }

    private async Task ScanAsync()
    {
        if (string.IsNullOrEmpty(SelectedDrive))
            return;

        IsScanning = true;
        StatusMessage = "Scanning...";
        Progress = 0;

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                // IProgress<T> is automatically invoked on the UI thread
                StatusMessage = $"Scanning: {p.CurrentPath}";
            });

            var root = await _scanner.ScanAsync(SelectedDrive, progress);
            _analyzer.AggregateFolderSizes(root);
            RootFolder = root;
            SelectedNode = root;
            StatusMessage = "Scan completed.";
        }
        catch (Exception ex)
        {
            _logger.Error("Scan failed.", ex);
            StatusMessage = "Scan failed.";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void UpdateCurrentChildren()
    {
        CurrentChildren.Clear();
        if (SelectedNode is FolderNode folder)
        {
            var criteria = new FilterCriteria
            {
                ModifiedAfterDays = ModifiedAfterDays,
                ModifiedBeforeDays = ModifiedBeforeDays,
                CreatedAfterDays = CreatedAfterDays,
                CreatedBeforeDays = CreatedBeforeDays,
                IncludeFiles = true,
                IncludeFolders = true
            };

            var filtered = _analyzer.Filter(folder, criteria);

            foreach (var node in filtered)
            {
                if (string.IsNullOrEmpty(SearchText) || node.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                {
                    CurrentChildren.Add(node);
                }
            }
        }
    }

    private Task AnalyzeExtensionsAsync()
    {
        if (RootFolder == null)
            return Task.CompletedTask;

        try
        {
            ExtensionAnalysis.Clear();
            var analysis = _extensionAnalyzer.AnalyzeByExtension(RootFolder);
            foreach (var item in analysis)
            {
                ExtensionAnalysis.Add(item);
            }
            StatusMessage = $"Extension analysis completed: {ExtensionAnalysis.Count} file types found.";
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to analyze extensions.", ex);
            StatusMessage = "Failed to analyze extensions.";
        }

        return Task.CompletedTask;
    }

    private Task DetectDuplicatesBySizeAsync()
    {
        if (RootFolder == null)
            return Task.CompletedTask;

        try
        {
            DuplicateGroups.Clear();
            var duplicates = _duplicateDetector.DetectDuplicatesBySize(RootFolder);
            foreach (var group in duplicates)
            {
                DuplicateGroups.Add(group);
            }
            StatusMessage = $"Duplicate detection completed: {DuplicateGroups.Count} duplicate groups found.";
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to detect duplicates by size.", ex);
            StatusMessage = "Failed to detect duplicates.";
        }

        return Task.CompletedTask;
    }

    private async Task DetectDuplicatesByHashAsync()
    {
        if (RootFolder == null)
            return;

        try
        {
            IsScanning = true;
            StatusMessage = "Detecting duplicates by hash...";

            DuplicateGroups.Clear();
            var duplicates = await _duplicateDetector.DetectDuplicatesByHash(RootFolder);
            foreach (var group in duplicates)
            {
                DuplicateGroups.Add(group);
            }
            StatusMessage = $"Duplicate detection completed: {DuplicateGroups.Count} duplicate groups found.";
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to detect duplicates by hash.", ex);
            StatusMessage = "Failed to detect duplicates.";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void ClearFilters()
    {
        ModifiedAfterDays = null;
        ModifiedBeforeDays = null;
        CreatedAfterDays = null;
        CreatedBeforeDays = null;
        SearchText = string.Empty;
        StatusMessage = "Filters cleared.";
    }

    private void OpenInExplorer()
    {
        if (SelectedNode == null)
            return;

        try
        {
            if (SelectedNode is FolderNode folder)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folder.FullPath,
                    UseShellExecute = true
                });
            }
            else if (SelectedNode is FileNode file)
            {
                var directory = Path.GetDirectoryName(file.FullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = directory,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to open in Explorer.", ex);
        }
    }

    private void CopyPath()
    {
        if (SelectedNode == null)
            return;

        try
        {
            Clipboard.SetText(SelectedNode.FullPath);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to copy path.", ex);
        }
    }

    private void CopyName()
    {
        if (SelectedNode == null)
            return;

        try
        {
            Clipboard.SetText(SelectedNode.Name);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to copy name.", ex);
        }
    }

    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        var settings = _settingsProvider.Load();
        settings.IsDarkTheme = IsDarkTheme;
        _settingsProvider.Save(settings);

        App.Current?.Resources.MergedDictionaries.Clear();
        var themeUri = new Uri(IsDarkTheme
            ? "/Themes/DarkTheme.xaml"
            : "/Themes/LightTheme.xaml", UriKind.Relative);
        App.Current?.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
    }

    public async Task ExportToCsvAsync()
    {
        await ExportToCsvAsync(null);
    }

    public async Task ExportToCsvAsync(string? filePath)
    {
        if (CurrentChildren.Count == 0)
        {
            StatusMessage = "No data to export.";
            return;
        }

        if (string.IsNullOrEmpty(filePath))
        {
            filePath = $"VoluMaph_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        }

        try
        {
            var content = GenerateCsvContent();
            var bom = Encoding.UTF8.GetPreamble();
            var bytes = bom.Concat(Encoding.UTF8.GetBytes(content)).ToArray();
            await File.WriteAllBytesAsync(filePath, bytes);
            StatusMessage = $"CSV exported to {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to export CSV.", ex);
            StatusMessage = "Failed to export CSV.";
        }
    }

    private string GenerateCsvContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name,Size,Path");

        foreach (var node in CurrentChildren)
        {
            var name = EscapeCsvValue(node.Name);
            var size = EscapeCsvValue(node.Size.ToString());
            var path = EscapeCsvValue(node.FullPath);
            sb.AppendLine($"{name},{size},{path}");
        }

        return sb.ToString();
    }

    private static string EscapeCsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    public async Task ExportToHtmlAsync()
    {
        await ExportToHtmlAsync(null);
    }

    public async Task ExportToHtmlAsync(string? filePath)
    {
        if (CurrentChildren.Count == 0)
        {
            StatusMessage = "No data to export.";
            return;
        }

        if (string.IsNullOrEmpty(filePath))
        {
            filePath = $"VoluMaph_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        }

        try
        {
            await File.WriteAllTextAsync(filePath, GenerateHtmlContent(), Encoding.UTF8);
            StatusMessage = $"HTML exported to {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to export HTML.", ex);
            StatusMessage = "Failed to export HTML.";
        }
    }

    private string GenerateHtmlContent()
    {
        var sb = new StringBuilder();
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var title = $"VoluMaph Report - {timestamp}";
        var totalSize = CurrentChildren.Sum(n => n.Size);

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>{title}</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        * { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 20px; background-color: #f5f5f5; }");
        sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
        sb.AppendLine("        h1 { color: #333; margin-bottom: 10px; font-size: 28px; }");
        sb.AppendLine("        .timestamp { color: #666; margin-bottom: 20px; font-size: 14px; }");
        sb.AppendLine("        .summary { background-color: #f0f0f0; padding: 15px; border-radius: 4px; margin-bottom: 20px; }");
        sb.AppendLine("        .summary strong { color: #333; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        sb.AppendLine("        thead { background-color: #0078d4; color: white; }");
        sb.AppendLine("        th, td { padding: 12px 15px; text-align: left; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("        th { font-weight: 600; font-size: 14px; }");
        sb.AppendLine("        tr:hover { background-color: #f5f5f5; }");
        sb.AppendLine("        .folder { color: #0078d4; font-weight: 500; }");
        sb.AppendLine("        .file { color: #333; }");
        sb.AppendLine("        .size { text-align: right; }");
        sb.AppendLine("        .path { color: #666; font-size: 13px; max-width: 400px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }");
        sb.AppendLine("        @media print { body { background-color: white; } .container { box-shadow: none; } }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine($"        <h1>VoluMaph Report</h1>");
        sb.AppendLine($"        <div class=\"timestamp\">Generated: {timestamp}</div>");
        sb.AppendLine($"        <div class=\"summary\">");
        sb.AppendLine($"            <strong>Total Items:</strong> {CurrentChildren.Count} | ");
        sb.AppendLine($"            <strong>Total Size:</strong> {FormatSize(totalSize)}");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <table>");
        sb.AppendLine("            <thead>");
        sb.AppendLine("                <tr>");
        sb.AppendLine("                    <th>Name</th>");
        sb.AppendLine("                    <th>Size</th>");
        sb.AppendLine("                    <th>Path</th>");
        sb.AppendLine("                </tr>");
        sb.AppendLine("            </thead>");
        sb.AppendLine("            <tbody>");

        foreach (var node in CurrentChildren)
        {
            var cssClass = node.IsFolder ? "folder" : "file";
            sb.AppendLine("                <tr>");
            sb.AppendLine($"                    <td class=\"{cssClass}\">{System.Net.WebUtility.HtmlEncode(node.Name)}</td>");
            sb.AppendLine($"                    <td class=\"size\">{FormatSize(node.Size)}</td>");
            sb.AppendLine($"                    <td class=\"path\">{System.Net.WebUtility.HtmlEncode(node.FullPath)}</td>");
            sb.AppendLine("                </tr>");
        }

        sb.AppendLine("            </tbody>");
        sb.AppendLine("        </table>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
```

### MainWindow Configuration Example

The XAML side can be roughly structured as follows.

**Note**: WPF's TreeView SelectedItem property cannot be directly bound. For MVP, handle this in code-behind or use a Behavior.

```xml
<Window x:Class="VoluMaph.UI.Views.MainWindow"
        ...>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Top: Drive selection + Scan button -->
        <StackPanel Orientation="Horizontal" Grid.Row="0" Margin="8">
            <ComboBox ItemsSource="{Binding AvailableDrives}"
                      SelectedItem="{Binding SelectedDrive}"
                      Width="150" Margin="0,0,8,0"/>
            <Button Content="Scan"
                    Command="{Binding ScanCommand}" />
        </StackPanel>

        <!-- Center: Left tree / Right table -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="2*"/>
                <ColumnDefinition Width="3*"/>
            </Grid.ColumnDefinitions>

            <!-- Folder tree -->
            <TreeView Grid.Column="0"
                      ItemsSource="{Binding RootFolder.Children}"
                      SelectedItemChanged="TreeView_SelectedItemChanged">
                <TreeView.ItemTemplate>
                    <HierarchicalDataTemplate ItemsSource="{Binding Children}">
                        <TextBlock Text="{Binding Name}" />
                    </HierarchicalDataTemplate>
                </TreeView.ItemTemplate>
            </TreeView>

            <!-- Detail list -->
            <DataGrid Grid.Column="1"
                      ItemsSource="{Binding CurrentChildren}"
                      AutoGenerateColumns="False">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
                    <DataGridTextColumn Header="Size" Binding="{Binding Size}" />
                    <DataGridTextColumn Header="Path" Binding="{Binding FullPath}" />
                </DataGrid.Columns>
            </DataGrid>
        </Grid>

        <!-- Bottom: Status bar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem Content="{Binding StatusMessage}" />
            <StatusBarItem>
                <ProgressBar Width="150"
                             Minimum="0" Maximum="100"
                             Value="{Binding Progress}" />
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

```csharp
// VoluMaph.UI/Views/MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel && e.NewValue is FileSystemNode node)
        {
            viewModel.SelectedNode = node;
        }
    }
}
```

---
