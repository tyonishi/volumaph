using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using VoluMaph.Core.Color;
using VoluMaph.Core.Model;
using VoluMaph.UI.Commands;

namespace VoluMaph.UI.ViewModels;

 public sealed class VisualizationViewModel : ViewModelBase
{
    private FolderNode? _rootFolder;
    private FileSystemNode? _selectedNode;
    private bool _isSunburstMode;
    private ColorTheme _colorTheme;
    private bool _autoThemeSelection;
    private readonly ObservableCollection<FileSystemNode> _visibleNodes;
    private readonly VisualizationHistory _history;
    private double _zoomLevel = 1.0;
    private double _panX = 0.0;
    private double _panY = 0.0;

    public event EventHandler<FileSystemNode>? SelectionChanged;

    public ObservableCollection<FileSystemNode> VisibleNodes => _visibleNodes;

    public FolderNode? RootFolder
    {
        get => _rootFolder;
        set
        {
            if (_rootFolder != value)
            {
                _rootFolder = value;
                RaisePropertyChanged();
                UpdateVisibleNodes();
                UpdateAutoTheme();
            }
        }
    }

    public FileSystemNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (_selectedNode != value)
            {
                _selectedNode = value;
                RaisePropertyChanged();
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    public bool IsSunburstMode
    {
        get => _isSunburstMode;
        set
        {
            if (_isSunburstMode != value)
            {
                _isSunburstMode = value;
                RaisePropertyChanged();
                UpdateVisibleNodes();
            }
        }
    }

    public ColorTheme ColorTheme
    {
        get => _colorTheme;
        set
        {
            if (_colorTheme != value)
            {
                _colorTheme = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool AutoThemeSelection
    {
        get => _autoThemeSelection;
        set
        {
            if (_autoThemeSelection != value)
            {
                _autoThemeSelection = value;
                RaisePropertyChanged();
                if (value)
                {
                    UpdateAutoTheme();
                }
            }
        }
    }

    public ICommand ToggleVisualizationModeCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand MaxZoomCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand GoForwardCommand { get; }

    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (_zoomLevel != value)
            {
                _zoomLevel = value;
                RaisePropertyChanged();
            }
        }
    }

    public double PanX
    {
        get => _panX;
        set
        {
            if (_panX != value)
            {
                _panX = value;
                RaisePropertyChanged();
            }
        }
    }

    public double PanY
    {
        get => _panY;
        set
        {
            if (_panY != value)
            {
                _panY = value;
                RaisePropertyChanged();
            }
        }
    }

    public VisualizationViewModel()
    {
        _visibleNodes = new ObservableCollection<FileSystemNode>();
        _history = new VisualizationHistory();
        _colorTheme = ColorTheme.Heatmap;
        _autoThemeSelection = false;
        ToggleVisualizationModeCommand = new RelayCommand(_ => ToggleVisualizationMode(), _ => RootFolder != null);
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => true);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => true);
        ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => true);
        MaxZoomCommand = new RelayCommand(_ => MaxZoom(), _ => true);
        GoBackCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
        GoForwardCommand = new RelayCommand(_ => Redo(), _ => CanRedo());

        _history.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationHistory.CanUndo))
            {
                (GoBackCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            if (e.PropertyName == nameof(VisualizationHistory.CanRedo))
            {
                (GoForwardCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        };
    }

    public bool CanUndo() => _history.CanUndo;

    public bool CanRedo() => _history.CanRedo;

    public void Undo()
    {
        var previousState = _history.Undo();
        ApplyState(previousState);
    }

    public void Redo()
    {
        var nextState = _history.Redo();
        ApplyState(nextState);
    }

    private void ApplyState(VisualizationState state)
    {
        ZoomLevel = state.ZoomLevel;
        PanX = state.PanX;
        PanY = state.PanY;
        SelectedNode = state.SelectedNode;
    }

    private void SaveCurrentState()
    {
        _history.AddState(new VisualizationState(ZoomLevel, PanX, PanY, SelectedNode));
    }

    public void ToggleVisualizationMode()
    {
        IsSunburstMode = !IsSunburstMode;
    }

    public void ZoomIn()
    {
        ZoomLevel = Math.Min(20.0, ZoomLevel * 1.2);
        SaveCurrentState();
    }

    public void ZoomOut()
    {
        ZoomLevel = Math.Max(0.1, ZoomLevel / 1.2);
        SaveCurrentState();
    }

    public void ResetZoom()
    {
        ZoomLevel = 1.0;
        PanX = 0.0;
        PanY = 0.0;
        SaveCurrentState();
    }

    public void MaxZoom()
    {
        ZoomLevel = 20.0;
        SaveCurrentState();
    }

    private void UpdateVisibleNodes()
    {
        _visibleNodes.Clear();

        if (RootFolder == null)
        {
            return;
        }

        var children = IsSunburstMode
            ? (IReadOnlyList<FileSystemNode>)new[] { RootFolder }
            : RootFolder.Children.ToList();

        foreach (var child in children)
        {
            _visibleNodes.Add(child);
        }
    }

    private void UpdateAutoTheme()
    {
        if (!AutoThemeSelection || RootFolder == null)
        {
            return;
        }

        var maxDepth = CalculateMaxDepth(RootFolder);
        ColorTheme = maxDepth switch
        {
            <= 3 => ColorTheme.Forest,
            <= 5 => ColorTheme.Ocean,
            <= 7 => ColorTheme.Viridis,
            <= 10 => ColorTheme.Heatmap,
            _ => ColorTheme.Sunset
        };
    }

    private static int CalculateMaxDepth(FileSystemNode node, int currentDepth = 0)
    {
        if (node is FileNode)
        {
            return currentDepth;
        }

        if (node is not FolderNode folder)
        {
            return currentDepth;
        }

        var maxChildDepth = currentDepth;
        foreach (var child in folder.Children)
        {
            var childDepth = CalculateMaxDepth(child, currentDepth + 1);
            if (childDepth > maxChildDepth)
            {
                maxChildDepth = childDepth;
            }
        }

        return maxChildDepth;
    }
}
