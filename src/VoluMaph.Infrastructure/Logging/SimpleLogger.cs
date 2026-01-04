using System;
using System.IO;

namespace VoluMaph.Infrastructure.Logging;

/// <summary>
/// Simple logger implementation that writes to file.
/// </summary>
public sealed class SimpleLogger : ILogger
{
    private readonly string _logFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleLogger"/> class.
    /// </summary>
    /// <param name="logFilePath">The file path to write logs to.</param>
    public SimpleLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message) => Write("INFO", message);

    /// <summary>
    /// Logs an error message with optional exception details.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="ex">The optional exception that caused the error.</param>
    public void Error(string message, Exception? ex = null)
        => Write("ERROR", $"{message} {ex}");

    /// <summary>
    /// Writes a log message to the file.
    /// </summary>
    /// <param name="level">The log level (INFO, ERROR, etc.).</param>
    /// <param name="message">The message to log.</param>
    private void Write(string level, string message)
    {
        var line = $"{DateTime.Now:O} [{level}] {message}";
        File.AppendAllLines(_logFilePath, new[] { line });
    }
}
