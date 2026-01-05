using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Color;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using VoluMaph.Infrastructure.Logging;
using VoluMaph.Infrastructure.Settings;
using VoluMaph.UI.Commands;
using VoluMaph.UI.Services;

namespace VoluMaph.UI.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    public event Action<string, string, Brush?>? ShowToastRequested;
    private readonly IScanner _scanner;
    private readonly IFolderAnalyzer _analyzer;
    private readonly IExtensionAnalyzer _extensionAnalyzer;
    private readonly IDuplicateDetector _duplicateDetector;
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger _logger;
    private readonly VisualizationViewModel _visualizationViewModel;

    private FolderNode? _rootFolder;
    private FileSystemNode? _selectedNode;
    private bool _isScanning;
    private bool _isScanJustCompleted;
    private double _progress;
    private string _statusMessage = string.Empty;
    private CancellationTokenSource? _cancellationTokenSource;
    private string _selectedDrive = string.Empty;
    private string _searchText = string.Empty;
    private bool _isDarkTheme = true;
    private int? _modifiedAfterDays;
    private int? _modifiedBeforeDays;
    private int? _createdAfterDays;
    private int? _createdBeforeDays;
    private bool _isSummaryExpanded = true;
    private bool _isFiltersExpanded = false;
    private string _minSizeFilter = string.Empty;
    private string _maxSizeFilter = string.Empty;
    private string _selectedExtension = string.Empty;
    private string _sortColumn = "Size";
    private ListSortDirection _sortDirection = ListSortDirection.Descending;
    private ColorTheme _colorTheme = ColorTheme.Heatmap;
    private bool _showVisualization = false;

    public sealed class ColumnDefinition
    {
        public string Header { get; set; } = string.Empty;
        public string Binding { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public double Width { get; set; }
    }

    private readonly ObservableCollection<FileSystemNode> _currentChildren;
    private ICollectionView? _childrenView;

    public ObservableCollection<string> AvailableDrives { get; } = new();
    public ObservableCollection<ColorTheme> AvailableColorThemes { get; } = new();
    public ObservableCollection<FileSystemNode> CurrentChildren => _currentChildren;
    public VisualizationViewModel VisualizationViewModel => _visualizationViewModel;
    public ICollectionView ChildrenView
    {
        get
        {
            if (_childrenView == null)
            {
                _childrenView = CollectionViewSource.GetDefaultView(_currentChildren);
                _childrenView.SortDescriptions.Add(new SortDescription("Size", ListSortDirection.Descending));
            }
            return _childrenView;
        }
    }
    public ObservableCollection<ColumnDefinition> ColumnDefinitions { get; } = new();
    public ObservableCollection<ExtensionAnalysisResult> ExtensionAnalysis { get; } = new();
    public ObservableCollection<DuplicateGroup> DuplicateGroups { get; } = new();
    public ObservableCollection<string> AvailableExtensions { get; } = new();

    public FolderNode? RootFolder
    {
        get => _rootFolder;
        private set
        {
            _rootFolder = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(TotalSize));
            RaisePropertyChanged(nameof(TotalFiles));
            RaisePropertyChanged(nameof(TotalFolders));
            RaisePropertyChanged(nameof(LargestFile));
            RaisePropertyChanged(nameof(LargestFileSize));
            RefreshCommandStates();
        }
    }


    public FileSystemNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            RaisePropertyChanged();
            UpdateCurrentChildren();
            RefreshCommandStates();
        }
    }


    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            _isScanning = value;
            RaisePropertyChanged();
            RefreshCommandStates();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsScanJustCompleted
    {
        get => _isScanJustCompleted;
        private set { _isScanJustCompleted = value; RaisePropertyChanged(); }
    }

    public double Progress
    {
        get => _progress;
        private set { _progress = value; RaisePropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; RaisePropertyChanged(); }
    }

    public string SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            _selectedDrive = value;
            RaisePropertyChanged();
            RefreshCommandStates();
        }
    }


    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;
            _searchText = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
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

    public string ToggleThemeText => IsDarkTheme ? "☀ ライトモードに切り替え" : "🌙 ダークモードに切り替え";

    public ColorTheme ColorTheme
    {
        get => _colorTheme;
        set
        {
            if (_colorTheme != value)
            {
                _colorTheme = value;
                _visualizationViewModel.ColorTheme = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool ShowVisualization
    {
        get => _showVisualization;
        set
        {
            if (_showVisualization != value)
            {
                _showVisualization = value;
                RaisePropertyChanged();
            }
        }
    }

    public int? ModifiedAfterDays
    {
        get => _modifiedAfterDays;
        set
        {
            if (_modifiedAfterDays == value)
                return;
            _modifiedAfterDays = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public int? ModifiedBeforeDays
    {
        get => _modifiedBeforeDays;
        set
        {
            if (_modifiedBeforeDays == value)
                return;
            _modifiedBeforeDays = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public int? CreatedAfterDays
    {
        get => _createdAfterDays;
        set
        {
            if (_createdAfterDays == value)
                return;
            _createdAfterDays = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public int? CreatedBeforeDays
    {
        get => _createdBeforeDays;
        set
        {
            if (_createdBeforeDays == value)
                return;
            _createdBeforeDays = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public bool IsSummaryExpanded
    {
        get => _isSummaryExpanded;
        set { _isSummaryExpanded = value; RaisePropertyChanged(); }
    }

    public bool IsFiltersExpanded
    {
        get => _isFiltersExpanded;
        set { _isFiltersExpanded = value; RaisePropertyChanged(); }
    }

    public bool HasActiveFilters
    {
        get
        {
            return !string.IsNullOrEmpty(SearchText) ||
                   !string.IsNullOrEmpty(MinSizeFilter) ||
                   !string.IsNullOrEmpty(MaxSizeFilter) ||
                   !string.IsNullOrEmpty(SelectedExtension) ||
                   ModifiedAfterDays.HasValue ||
                   ModifiedBeforeDays.HasValue ||
                   CreatedAfterDays.HasValue ||
                   CreatedBeforeDays.HasValue;
        }
    }

    public string MinSizeFilter
    {
        get => _minSizeFilter;
        set
        {
            if (_minSizeFilter == value)
                return;
            _minSizeFilter = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public string MaxSizeFilter
    {
        get => _maxSizeFilter;
        set
        {
            if (_maxSizeFilter == value)
                return;
            _maxSizeFilter = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public string SelectedExtension
    {
        get => _selectedExtension;
        set
        {
            if (_selectedExtension == value)
                return;
            _selectedExtension = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasActiveFilters));
            UpdateCurrentChildren();
        }
    }

    public long TotalSize
    {
        get
        {
            if (RootFolder == null)
                return 0;
            return RootFolder.Size;
        }
    }

    public int TotalFiles
    {
        get
        {
            if (RootFolder == null)
                return 0;
            return CountFiles(RootFolder);
        }
    }

    public int TotalFolders
    {
        get
        {
            if (RootFolder == null)
                return 0;
            return CountFolders(RootFolder);
        }
    }

    public string LargestFile
    {
        get
        {
            if (RootFolder == null)
                return string.Empty;
            var largest = FindLargestFile(RootFolder);
            return largest?.Name ?? string.Empty;
        }
    }

    public long LargestFileSize
    {
        get
        {
            if (RootFolder == null)
                return 0;
            var largest = FindLargestFile(RootFolder);
            return largest?.Size ?? 0;
        }
    }

    public string SortColumn
    {
        get => _sortColumn;
        set { _sortColumn = value; RaisePropertyChanged(); }
    }

    public ListSortDirection SortDirection
    {
        get => _sortDirection;
        set { _sortDirection = value; RaisePropertyChanged(); }
    }

    public ICommand ScanCommand { get; } = default!;
    public ICommand CancelCommand { get; } = default!;
    public ICommand RefreshCommand { get; } = default!;
    public ICommand RefreshDrivesCommand { get; } = default!;
    public ICommand SortCommand { get; } = default!;
    public ICommand OpenInExplorerCommand { get; } = default!;
    public ICommand CopyPathCommand { get; } = default!;
    public ICommand CopyNameCommand { get; } = default!;
    public ICommand ToggleThemeCommand { get; } = default!;
    public ICommand ToggleFiltersCommand { get; } = default!;
    public ICommand ExportToCsvCommand { get; } = default!;
    public ICommand ExportToHtmlCommand { get; } = default!;
    public ICommand AnalyzeExtensionsCommand { get; } = default!;
    public ICommand DetectDuplicatesBySizeCommand { get; } = default!;
    public ICommand DetectDuplicatesByHashCommand { get; } = default!;
    public ICommand ClearFiltersCommand { get; } = default!;
    public ICommand ToggleVisualizationCommand { get; } = default!;
    public ICommand ScreenshotCommand { get; } = default!;

    public event Action? ScreenshotRequested;
    public IDialogService? DialogService { get; set; }


    private void ShowToast(string message, string icon = "\xE8FB", Brush? iconColor = null)
    {
        ShowToastRequested?.Invoke(message, icon, iconColor);
    }

    private void RefreshCommandStates()
    {
        (ScanCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RefreshCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (OpenInExplorerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CopyPathCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CopyNameCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ExportToCsvCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportToHtmlCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AnalyzeExtensionsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (DetectDuplicatesBySizeCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (DetectDuplicatesByHashCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ToggleVisualizationCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ScreenshotCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private static string NormalizePath(string path)
    {
        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string GetDefaultExportPath(string extension)
    {
        var exportDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoluMaph");
        Directory.CreateDirectory(exportDirectory);
        var fileName = extension.Equals("csv", StringComparison.OrdinalIgnoreCase)
            ? $"VoluMaph_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            : $"VoluMaph_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        return Path.Combine(exportDirectory, fileName);
    }

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
        _currentChildren = new ObservableCollection<FileSystemNode>();
        _visualizationViewModel = new VisualizationViewModel();

        InitializeColumnDefinitions();
        InitializeColorThemes();
        LoadDrives();
        LoadSettings();

        ScanCommand = new AsyncRelayCommand(async _ => await ScanAsync(), _ => !IsScanning && !string.IsNullOrEmpty(SelectedDrive));
        CancelCommand = new RelayCommand(_ => CancelScan(), _ => IsScanning);
        RefreshCommand = new AsyncRelayCommand(async _ => await RefreshAsync(), _ => !IsScanning && !string.IsNullOrEmpty(SelectedDrive));
        RefreshDrivesCommand = new RelayCommand(_ => LoadDrives());
        SortCommand = new RelayCommand(column => Sort(column?.ToString()));
        OpenInExplorerCommand = new RelayCommand(_ => OpenInExplorer(), _ => SelectedNode != null);
        CopyPathCommand = new RelayCommand(_ => CopyPath(), _ => SelectedNode != null);
        CopyNameCommand = new RelayCommand(_ => CopyName(), _ => SelectedNode != null);
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        ToggleFiltersCommand = new RelayCommand(_ => ToggleFilters());
        ExportToCsvCommand = new AsyncRelayCommand(async _ => await ExportToCsvAsync(), _ => !IsScanning && CurrentChildren.Count > 0);
        ExportToHtmlCommand = new AsyncRelayCommand(async _ => await ExportToHtmlAsync(), _ => !IsScanning && CurrentChildren.Count > 0);
        AnalyzeExtensionsCommand = new AsyncRelayCommand(async _ => await AnalyzeExtensionsAsync(), _ => !IsScanning && RootFolder != null);
        DetectDuplicatesBySizeCommand = new AsyncRelayCommand(async _ => await DetectDuplicatesBySizeAsync(), _ => !IsScanning && RootFolder != null);
        DetectDuplicatesByHashCommand = new AsyncRelayCommand(async _ => await DetectDuplicatesByHashAsync(), _ => !IsScanning && RootFolder != null);
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
        ToggleVisualizationCommand = new RelayCommand(_ => ToggleVisualization(), _ => RootFolder != null);
        ScreenshotCommand = new RelayCommand(_ => ScreenshotRequested?.Invoke(), _ => CurrentChildren.Count > 0);
    }

    private void InitializeColumnDefinitions()
    {
        ColumnDefinitions.Clear();
        ColumnDefinitions.Add(new ColumnDefinition { Header = "Name", Binding = "Name", IsVisible = true, Width = double.NaN });
        ColumnDefinitions.Add(new ColumnDefinition { Header = "Size", Binding = "Size", IsVisible = true, Width = 120 });
        ColumnDefinitions.Add(new ColumnDefinition { Header = "Path", Binding = "FullPath", IsVisible = true, Width = double.NaN });
    }

    private void InitializeColorThemes()
    {
        AvailableColorThemes.Clear();
        foreach (ColorTheme theme in Enum.GetValues(typeof(ColorTheme)))
        {
            AvailableColorThemes.Add(theme);
        }
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

    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsProvider.LoadAsync();
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

    private async Task SaveSettingsAsync()
    {
        var settings = _settingsProvider.Load();
        settings.LastSelectedDrive = SelectedDrive;
        settings.IsDarkTheme = IsDarkTheme;
        await _settingsProvider.SaveAsync(settings);
    }

    private async Task ScanAsync()
    {
        if (string.IsNullOrEmpty(SelectedDrive))
            return;

        var root = await PerformScanAsync(SelectedDrive);
        if (root != null)
        {
            RootFolder = root;
            SelectedNode = root;
        }
    }

    private void UpdateCurrentChildren()
    {
        _currentChildren.Clear();
        AvailableExtensions.Clear();
        AvailableExtensions.Add("All");

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
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var tempChildren = new List<FileSystemNode>();
            var uniquePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var minSize = long.TryParse(MinSizeFilter, out var min) ? min : 0;
            var maxSize = long.TryParse(MaxSizeFilter, out var max) ? max : long.MaxValue;
            var selectedExtension = SelectedExtension;
            if (string.IsNullOrEmpty(selectedExtension))
            {
                selectedExtension = "All";
                if (_selectedExtension != "All")
                {
                    _selectedExtension = "All";
                    RaisePropertyChanged(nameof(SelectedExtension));
                }
            }

            foreach (var node in filtered)
            {
                var pathKey = NormalizePath(node.FullPath);
                if (!uniquePaths.Add(pathKey))
                {
                    continue;
                }

                if (node is FileNode fileNode)
                {
                    var ext = Path.GetExtension(fileNode.Name);
                    if (!string.IsNullOrEmpty(ext))
                    {
                        extensions.Add(ext);
                    }
                }

                if (node.Size >= minSize && node.Size <= maxSize &&
                    (selectedExtension == "All" || (node is FileNode && Path.GetExtension(node.Name).Equals(selectedExtension, StringComparison.OrdinalIgnoreCase))))
                {
                    if (string.IsNullOrEmpty(SearchText) || node.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    {
                        tempChildren.Add(node);
                    }
                }
            }

            foreach (var node in tempChildren)
            {
                _currentChildren.Add(node);
            }

            foreach (var ext in extensions.OrderBy(e => e))
            {
                AvailableExtensions.Add(ext);
            }
        }
        else
        {
            if (_selectedExtension != "All")
            {
                _selectedExtension = "All";
                RaisePropertyChanged(nameof(SelectedExtension));
            }
        }

        if (_childrenView != null)
        {
            _childrenView.Refresh();
        }

        RefreshCommandStates();
    }

    public async Task ScanFolderAsync(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            return;

        var root = await PerformScanAsync(folderPath);
        if (root != null)
        {
            RootFolder = root;
            SelectedNode = root;
        }
    }

    private async Task<FolderNode?> PerformScanAsync(string path)
    {
        IsScanning = true;
        StatusMessage = "Scanning...";
        Progress = 0;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                StatusMessage = $"Scanning: {p.CurrentPath}";
            });

            var root = await Task.Run(() => _scanner.ScanAsync(path, progress, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            await Task.Run(() => _analyzer.AggregateFolderSizes(root), _cancellationTokenSource.Token);
            root?.SortChildren();
            _visualizationViewModel.RootFolder = root;
            StatusMessage = "Scan completed.";
            Progress = 100;
            _isScanJustCompleted = true;
            SelectedNode = root;
            _isScanJustCompleted = false;
            ShowToast("Scan completed successfully", "\xE8FB", new SolidColorBrush(Colors.Green));
            return root;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan cancelled.";
            ShowToast("Scan cancelled", "\xE711", new SolidColorBrush(Colors.Yellow));
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error("Scan failed.", ex);
            StatusMessage = "Scan failed.";
            ShowToast("Scan failed: " + ex.Message, "\xE7BA", new SolidColorBrush(Colors.Red));
            return null;
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            IsScanning = false;
        }
    }

    private void CancelScan()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Cancelling scan...";
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

    private async void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        var settings = _settingsProvider.Load();
        settings.IsDarkTheme = IsDarkTheme;
        await _settingsProvider.SaveAsync(settings);

        var themeResource = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("DarkTheme.xaml") || d.Source.OriginalString.Contains("LightTheme.xaml")));
        if (themeResource != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(themeResource);
        }

        var themeUri = new Uri(IsDarkTheme
            ? "/Themes/DarkTheme.xaml"
            : "/Themes/LightTheme.xaml", UriKind.Relative);
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
    }

    private void ToggleFilters()
    {
        IsFiltersExpanded = !IsFiltersExpanded;
    }

    private async Task RefreshAsync()
    {
        if (string.IsNullOrEmpty(SelectedDrive))
            return;

        await ScanAsync();
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
            if (DialogService != null)
            {
                var defaultName = $"VoluMaph_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var chosen = DialogService.ShowSaveFile(defaultName, "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*");
                if (string.IsNullOrEmpty(chosen))
                    return; // cancelled
                filePath = chosen;
            }
            else
            {
                filePath = GetDefaultExportPath("csv");
            }
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
        var sb = new System.Text.StringBuilder();
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
            if (DialogService != null)
            {
                var defaultName = $"VoluMaph_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                var chosen = DialogService.ShowSaveFile(defaultName, "HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*");
                if (string.IsNullOrEmpty(chosen))
                    return; // cancelled
                filePath = chosen;
            }
            else
            {
                filePath = GetDefaultExportPath("html");
            }
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
        var sb = new System.Text.StringBuilder();
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

    private async Task AnalyzeExtensionsAsync()
    {
        if (RootFolder == null)
            return;

        try
        {
            ExtensionAnalysis.Clear();
            var analysis = await Task.Run(() => _extensionAnalyzer.AnalyzeByExtension(RootFolder));
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
    }

    private async Task DetectDuplicatesBySizeAsync()
    {
        if (RootFolder == null)
            return;

        try
        {
            DuplicateGroups.Clear();
            var duplicates = await Task.Run(() => _duplicateDetector.DetectDuplicatesBySize(RootFolder));
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

    private void ToggleVisualization()
    {
        ShowVisualization = !ShowVisualization;
        StatusMessage = ShowVisualization ? "Visualization enabled" : "Visualization disabled";
    }

    private static int CountFiles(FolderNode folder)
    {
        int count = 0;
        foreach (var child in folder.Children)
        {
            if (child is FileNode)
            {
                count++;
            }
            else if (child is FolderNode subFolder)
            {
                count += CountFiles(subFolder);
            }
        }
        return count;
    }

    private static int CountFolders(FolderNode folder)
    {
        int count = 0;
        foreach (var child in folder.Children)
        {
            if (child is FolderNode subFolder)
            {
                count++;
                count += CountFolders(subFolder);
            }
        }
        return count;
    }

    private static FileNode? FindLargestFile(FolderNode folder)
    {
        FileNode? largest = null;
        foreach (var child in folder.Children)
        {
            if (child is FileNode file)
            {
                if (largest == null || file.Size > largest.Size)
                {
                    largest = file;
                }
            }
            else if (child is FolderNode subFolder)
            {
                var subLargest = FindLargestFile(subFolder);
                if (subLargest != null && (largest == null || subLargest.Size > largest.Size))
                {
                    largest = subLargest;
                }
            }
        }
        return largest;
    }

    private void Sort(string? column)
    {
        if (string.IsNullOrEmpty(column))
            return;

        var view = ChildrenView;
        if (view == null)
            return;

        var direction = ListSortDirection.Ascending;
        if (SortColumn == column)
        {
            direction = SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }

        SortColumn = column;
        SortDirection = direction;

        view.SortDescriptions.Clear();
        view.SortDescriptions.Add(new SortDescription(column, direction));
        view.Refresh();
    }
}
