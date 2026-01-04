using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using VoluMaph.Core.Model;
using VoluMaph.UI.Commands;

namespace VoluMaph.UI.ViewModels;

public sealed class VisualizationViewModel : ViewModelBase
{
    private FolderNode? _rootFolder;
    private FileSystemNode? _selectedNode;
    private bool _isSunburstMode;
    private readonly ObservableCollection<FileSystemNode> _visibleNodes;

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

    public ICommand ToggleVisualizationModeCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }

    public VisualizationViewModel()
    {
        _visibleNodes = new ObservableCollection<FileSystemNode>();
        ToggleVisualizationModeCommand = new RelayCommand(ToggleVisualizationMode, () => RootFolder != null);
        ZoomInCommand = new RelayCommand(ZoomIn, () => true);
        ZoomOutCommand = new RelayCommand(ZoomOut, () => true);
        ResetZoomCommand = new RelayCommand(ResetZoom, () => true);
    }

    public void ToggleVisualizationMode()
    {
        IsSunburstMode = !IsSunburstMode;
    }

    public void ZoomIn()
    {
    }

    public void ZoomOut()
    {
    }

    public void ResetZoom()
    {
    }

    private void UpdateVisibleNodes()
    {
        _visibleNodes.Clear();

        if (RootFolder == null)
        {
            return;
        }

        var children = IsSunburstMode
            ? new[] { RootFolder }
            : RootFolder.Children.ToList();

        foreach (var child in children)
        {
            _visibleNodes.Add(child);
        }
    }
}
