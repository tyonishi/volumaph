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

namespace VoluMaph.UI
{
    public partial class MainWindow : Window
    {
        private DataGrid? _mainDataGrid;
        private MainViewModel? _viewModel;
        private Services.IDialogService? _dialogService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // Wire up controls by name (fields generated from XAML x:Name)
            var details = DetailsPane;
            _mainDataGrid = details?.MainDataGrid;
            if (details != null)
            {
                details.SortingRequested += MainDataGrid_Sorting;
            }

            var folderTree = FolderTree;
            if (folderTree != null)
            {
                folderTree.SelectedItemChanged += OnFolderTreeSelectedItemChanged;
            }

            InitializeEventHandlers();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // Unsubscribe events to prevent leaks
            if (_viewModel != null)
            {
                _viewModel.ShowToastRequested -= ShowToast;
                _viewModel.ScreenshotRequested -= OnScreenshotRequested;
            }

            if (DetailsPane != null)
            {
                DetailsPane.SortingRequested -= MainDataGrid_Sorting;
            }

            if (FolderTree != null)
            {
                FolderTree.SelectedItemChanged -= OnFolderTreeSelectedItemChanged;
            }

            if (MainGrid != null)
            {
                MainGrid.Drop -= Grid_Drop;
                MainGrid.DragOver -= Grid_DragOver;
            }
        }

        private void InitializeEventHandlers()
        {
            if (MainGrid != null)
            {
                MainGrid.Drop += Grid_Drop;
                MainGrid.DragOver += Grid_DragOver;
            }
        }

        public void ShowToast(string message, string icon = "\xE8FB", Brush? iconColor = null)
        {
            if (ToastCanvas == null)
                return;

            var toast = new ToastNotification
            {
                Message = message,
                Icon = icon,
                IconColor = iconColor ?? new SolidColorBrush(Colors.Green)
            };

            Canvas.SetLeft(toast, (ToastCanvas.ActualWidth - 320) / 2);
            Canvas.SetTop(toast, 16);
            ToastCanvas.Children.Add(toast);
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

            _viewModel = new MainViewModel(scanner, analyzer, extensionAnalyzer, duplicateDetector, settingsProvider, logger);
            _viewModel.ShowToastRequested += ShowToast;
            _viewModel.ScreenshotRequested += OnScreenshotRequested;

            // dialog service used by views to show file dialogs (keeps dialog code centralized)
            _dialogService = new Services.DialogService();
            if (_viewModel != null)
            {
                _viewModel.DialogService = _dialogService;
            }

            DataContext = _viewModel; // set DataContext to viewmodel field

        }

        private void OnScreenshotRequested()
        {
            var defaultName = $"VoluMaph_Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var path = _dialogService?.ShowSaveFile(defaultName, "PNG Files (*.png)|*.png|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                CaptureScreenshot(path);
            }
        }

        private void OnFolderTreeSelectedItemChanged(FileSystemNode? node)
        {
            if (_viewModel != null && node != null && !_viewModel.IsScanJustCompleted)
            {
                _viewModel.SelectedNode = node;
            }
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
                if (Directory.Exists(path) && _viewModel != null)
                {
                    await _viewModel.ScanFolderAsync(path);
                }
            }
        }


        private void MainDataGrid_Sorting(object? sender, DataGridSortingEventArgs e)
        {
            if (_viewModel != null)
            {
                e.Handled = true;
                var column = e.Column.SortMemberPath;
                if (string.IsNullOrEmpty(column))
                    return;

                _viewModel.SortColumn = column;
                _viewModel.SortDirection = _viewModel.SortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                var view = _viewModel.ChildrenView;
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(column, _viewModel.SortDirection));
                e.Column.SortDirection = _viewModel.SortDirection;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CommandBar?.FocusSearch();
                e.Handled = true;
            }
        }

        private void CaptureScreenshot(string filePath)
        {
            try
            {
                // Prefer ActualWidth/ActualHeight when available (accounts for layout)
                var width = ActualWidth > 0 ? ActualWidth : Width;
                var height = ActualHeight > 0 ? ActualHeight : Height;
                if (double.IsNaN(width) || double.IsNaN(height) || width <= 0 || height <= 0)
                {
                    width = Width;
                    height = Height;
                }

                var size = new Size(width, height);

                // Ensure layout is up-to-date before rendering
                UpdateLayout();

                var dpi = VisualTreeHelper.GetDpi(this);
                var pixelWidth = Math.Max(1, (int)Math.Round(size.Width * dpi.DpiScaleX));
                var pixelHeight = Math.Max(1, (int)Math.Round(size.Height * dpi.DpiScaleY));

                var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);

                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    context.DrawRectangle(new VisualBrush(this), null, new Rect(new Point(), size));
                }

                bitmap.Render(visual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // Ensure target directory exists
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(fileStream);
                }

                if (_viewModel != null)
                {
                    _viewModel.StatusMessage = $"Screenshot saved to {Path.GetFileName(filePath)}";
                }

                // Show a toast to confirm
                ShowToast($"Screenshot saved to {Path.GetFileName(filePath)}", "\xE8FB", new SolidColorBrush(Colors.Green));
            }
            catch (Exception ex)
            {
                if (_viewModel != null)
                {
                    _viewModel.StatusMessage = "Failed to save screenshot.";
                }

                // Also surface a toast so user sees failure
                ShowToast("Failed to save screenshot.", "\xE7BA", new SolidColorBrush(Colors.Red));
            }
        }

    }
}
