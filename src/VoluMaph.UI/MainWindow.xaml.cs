using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VoluMaph.Core.Analysis;
using VoluMaph.Core.Model;
using VoluMaph.Core.Scanning;
using VoluMaph.Infrastructure.Logging;
using VoluMaph.Infrastructure.Settings;
using VoluMaph.UI.Controls;
using VoluMaph.UI.ViewModels;

namespace VoluMaph.UI;

public partial class MainWindow : Window
{
    private DataGrid? _mainDataGrid { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        InitializeViewModel();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _mainDataGrid = FindName("MainDataGrid") as DataGrid;
        InitializeEventHandlers();
    }

    private void InitializeEventHandlers()
    {
        var mainGrid = FindName("MainGrid") as Grid;
        if (mainGrid != null)
        {
            mainGrid.Drop += Grid_Drop;
            mainGrid.DragOver += Grid_DragOver;
        }
    }

    public void ShowToast(string message, string icon = "\xE8FB", Brush? iconColor = null)
    {
        var toastCanvas = FindName("ToastCanvas") as Canvas;
        if (toastCanvas == null)
            return;

        var toast = new ToastNotification
        {
            Message = message,
            Icon = icon,
            IconColor = iconColor ?? new SolidColorBrush(Colors.Green)
        };

        Canvas.SetLeft(toast, (toastCanvas.ActualWidth - 320) / 2);
        Canvas.SetTop(toast, 16);
        toastCanvas.Children.Add(toast);
    }

    private void InitializeViewModel()
    {
        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoluMaph",
            "settings.json");

        var settingsDirectory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrEmpty(settingsDirectory) && !Directory.Exists(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoluMaph",
            "logs",
            $"{DateTime.Now:yyyyMMdd}.log");

        var logDirectory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var scanner = new DirectoryScanner();
        var analyzer = new FolderAnalyzer();
        var extensionAnalyzer = new ExtensionAnalyzer();
        var duplicateDetector = new DuplicateDetector();
        var settingsProvider = new JsonSettingsProvider(settingsPath);
        var logger = new SimpleLogger(logPath);

        var viewModel = new MainViewModel(scanner, analyzer, extensionAnalyzer, duplicateDetector, settingsProvider, logger);
        viewModel.ShowToastRequested += ShowToast;
        DataContext = viewModel;
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel && e.NewValue is FileSystemNode node && !viewModel.IsScanJustCompleted)
        {
            viewModel.SelectedNode = node;
        }
        // Do not set SelectedNode when scan is just completed to avoid overriding SelectedNode = root
    }

    private void Grid_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void Grid_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            var path = files[0];
            if (Directory.Exists(path) && DataContext is MainViewModel viewModel)
            {
                await viewModel.ScanFolderAsync(path);
            }
        }
    }

    private void ToggleColumns_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Items.Count == 0 && DataContext is MainViewModel viewModel)
        {
            menuItem.Items.Clear();
            foreach (var column in viewModel.ColumnDefinitions)
            {
                var checkbox = new CheckBox
                {
                    Content = column.Header,
                    IsChecked = column.IsVisible,
                    Margin = new Thickness(4)
                };
                checkbox.Checked += (s, args) => ToggleColumnVisibility(column.Header, true);
                checkbox.Unchecked += (s, args) => ToggleColumnVisibility(column.Header, false);
                menuItem.Items.Add(checkbox);
            }
        }
    }

    private void ToggleColumnVisibility(string header, bool isVisible)
    {
        if (_mainDataGrid != null)
        {
            foreach (var column in _mainDataGrid.Columns)
            {
                if (column.Header.ToString() == header)
                {
                    column.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                    break;
                }
            }
        }
    }

    private void MainDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            e.Handled = true;
            var column = e.Column.SortMemberPath;
            if (string.IsNullOrEmpty(column))
                return;

            viewModel.SortColumn = column;
            viewModel.SortDirection = viewModel.SortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            var view = viewModel.ChildrenView;
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(column, viewModel.SortDirection));
            e.Column.SortDirection = viewModel.SortDirection;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            var searchTextBox = FindName("SearchTextBox") as TextBox;
            searchTextBox?.Focus();
            e.Handled = true;
        }
    }



    private void Screenshot_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*",
            DefaultExt = "png",
            FileName = $"VoluMaph_Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        };

        if (dialog.ShowDialog() == true)
        {
            CaptureScreenshot(dialog.FileName);
        }
    }

    private void CaptureScreenshot(string filePath)
    {
        try
        {
            var size = new Size(Width, Height);
            var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Bgra32);

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(this), null, new Rect(new Point(), size));
            }

            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.StatusMessage = $"Screenshot saved to {Path.GetFileName(filePath)}";
            }
        }
        catch (Exception)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.StatusMessage = "Failed to save screenshot.";
            }
        }
    }
}
