using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace VoluMaph.Infrastructure.Settings;

/// <summary>
/// JSON-based implementation of settings provider.
/// </summary>
public sealed class JsonSettingsProvider : ISettingsProvider
{
    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSettingsProvider"/> class.
    /// </summary>
    /// <param name="filePath">The file path to read/write settings from/to.</param>
    public JsonSettingsProvider(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Loads application settings synchronously.
    /// </summary>
    /// <returns>The application settings.</returns>
    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
            return new AppSettings();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<AppSettings>(json)
               ?? new AppSettings();
    }

    /// <summary>
    /// Saves application settings synchronously.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Loads application settings asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
    /// <returns>A task representing the asynchronous load operation, containing settings.</returns>
    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
            return new AppSettings();

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (Exception)
        {
            // Return default settings if loading fails
            return new AppSettings();
        }
    }

    /// <summary>
    /// Saves application settings asynchronously.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(_filePath, json, cancellationToken).ConfigureAwait(false);
    }
}
