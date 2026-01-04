using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using VoluMaph.Infrastructure.Logging;
using VoluMaph.Infrastructure.Settings;

namespace VoluMaph.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private string? _settingsPath;
    private JsonSettingsProvider? _settingsProvider;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoluMaph",
            "settings.json");

        var settingsDirectory = Path.GetDirectoryName(_settingsPath);
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

        _settingsProvider = new JsonSettingsProvider(_settingsPath);
        var settings = _settingsProvider.Load();

        ApplyTheme(settings.IsDarkTheme);

        SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
    }

    private void SystemParameters_StaticPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "HighContrast")
        {
            if (_settingsProvider != null)
            {
                var settings = _settingsProvider.Load();
                ApplyTheme(settings.IsDarkTheme);
            }
        }
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        Resources.MergedDictionaries.Clear();

        ResourceDictionary designSystem = new() { Source = new Uri("/Themes/DesignSystem.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(designSystem);

        ResourceDictionary spacing = new() { Source = new Uri("/Themes/Spacing.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(spacing);

        ResourceDictionary typography = new() { Source = new Uri("/Themes/Typography.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(typography);

        ResourceDictionary animations = new() { Source = new Uri("/Themes/Animations/MicroInteractions.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(animations);

        var isHighContrast = SystemParameters.HighContrast;
        var themeUri = isHighContrast
            ? new Uri("/Themes/HighContrastTheme.xaml", UriKind.Relative)
            : new Uri(isDarkTheme ? "/Themes/DarkTheme.xaml" : "/Themes/LightTheme.xaml", UriKind.Relative);

        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });

        ResourceDictionary buttons = new() { Source = new Uri("/Themes/Styles/Buttons.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(buttons);

        ResourceDictionary cards = new() { Source = new Uri("/Themes/Styles/Cards.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(cards);

        ResourceDictionary comboBox = new() { Source = new Uri("/Themes/Styles/ComboBox.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(comboBox);

        ResourceDictionary dataGrid = new() { Source = new Uri("/Themes/Styles/DataGrid.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(dataGrid);

        ResourceDictionary expander = new() { Source = new Uri("/Themes/Styles/Expander.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(expander);

        ResourceDictionary progressBar = new() { Source = new Uri("/Themes/Styles/ProgressBar.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(progressBar);

        ResourceDictionary textBox = new() { Source = new Uri("/Themes/Styles/TextBox.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(textBox);

        ResourceDictionary treeView = new() { Source = new Uri("/Themes/Styles/TreeView.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(treeView);
    }
}

 

