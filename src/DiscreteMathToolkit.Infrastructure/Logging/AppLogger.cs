using Serilog;
using Serilog.Events;

namespace DiscreteMathToolkit.Infrastructure.Logging;

public interface IAppLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
    void Debug(string message);
}

public sealed class SerilogAppLogger : IAppLogger, IDisposable
{
    private readonly ILogger _logger;

    public SerilogAppLogger(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDirectory, "dmt-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public void Info(string message) => _logger.Information(message);
    public void Warn(string message) => _logger.Warning(message);
    public void Error(string message, Exception? ex = null) => _logger.Error(ex, message);
    public void Debug(string message) => _logger.Debug(message);

    public void Dispose() => (_logger as IDisposable)?.Dispose();

    public static string DefaultLogDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DiscreteMathToolkit", "logs");
}

/// <summary>Null logger for tests and headless contexts.</summary>
public sealed class NullAppLogger : IAppLogger
{
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message, Exception? ex = null) { }
    public void Debug(string message) { }
}
