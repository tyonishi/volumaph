using VoluMaph.Core.Model;

namespace VoluMaph.UI.ViewModels;

public sealed record VisualizationState(double ZoomLevel, double PanX, double PanY, FileSystemNode? SelectedNode);
