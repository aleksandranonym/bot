namespace SingleCore.Infrastructure.Services;

using SingleCore.Core.Interfaces;

public sealed class FileLogger : ILogger
{
    private readonly string _logDirectory;
    private readonly string _filePath;

    public FileLogger(string? baseDirectory = null)
    {
        _logDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? Path.Combine(AppContext.BaseDirectory, "logs")
            : baseDirectory;

        _filePath = Path.Combine(_logDirectory, "singlecore.log");
        Directory.CreateDirectory(_logDirectory);
    }

    public void Info(string message)
    {
        Write("INFO", message);
    }

    public void Warning(string message)
    {
        Write("WARNING", message);
    }

    public void Error(string message, Exception? exception = null)
    {
        var fullMessage = exception is null ? message : $"{message} | {exception}";
        Write("ERROR", fullMessage);
    }

    private void Write(string level, string message)
    {
        try
        {
            var text = $"[{DateTime.UtcNow:O}] [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(_filePath, text);
        }
        catch
        {
            // Intentionally swallow logging failures to avoid crashing the app.
        }
    }
}
