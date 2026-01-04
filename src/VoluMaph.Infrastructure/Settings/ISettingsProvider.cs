using System.Threading;
using System.Threading.Tasks;

namespace VoluMaph.Infrastructure.Settings;

/// <summary>
/// Interface for loading and saving application settings.
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    /// Loads application settings synchronously.
    /// </summary>
    /// <returns>The application settings.</returns>
    AppSettings Load();

    /// <summary>
    /// Saves application settings synchronously.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    void Save(AppSettings settings);

    /// <summary>
    /// Loads application settings asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
    /// <returns>A task representing the asynchronous load operation, containing the settings.</returns>
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves application settings asynchronously.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
