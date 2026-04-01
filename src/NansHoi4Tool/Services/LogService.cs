using Serilog;
using ILogger = Serilog.ILogger;

namespace NansHoi4Tool.Services;

public class LogService : ILogService
{
    private readonly ILogger _logger;

    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NansHoi4Tool", "Logs");

    public LogService()
    {
        Directory.CreateDirectory(LogDir);
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(LogDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .WriteTo.Debug()
            .CreateLogger();
    }

    public void Info(string message) => _logger.Information(message);
    public void Warning(string message) => _logger.Warning(message);
    public void Error(string message, Exception? ex = null)
    {
        if (ex != null) _logger.Error(ex, message);
        else _logger.Error(message);
    }
    public void Debug(string message) => _logger.Debug(message);
}
