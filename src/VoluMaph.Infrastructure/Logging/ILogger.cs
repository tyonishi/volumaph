namespace VoluMaph.Infrastructure.Logging;

/// <summary>
/// Interface for logging operations.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Info(string message);

    /// <summary>
    /// Logs an error message with optional exception details.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="ex">The optional exception that caused the error.</param>
    void Error(string message, Exception? ex = null);
}
