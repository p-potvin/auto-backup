using System.Text;

namespace AutoBackup.Services;

/// <summary>
/// Appends structured log entries to a file and keeps a bounded in-memory buffer
/// for display in the UI.  All entries include a CorrelationId for traceability.
/// </summary>
public sealed class LoggingService : IDisposable
{
    private const int DefaultMaxLines = 2000;

    private readonly string _logFilePath;
    private readonly int _maxLines;
    private readonly Queue<string> _buffer = new();
    private readonly object _lock = new();
    private StreamWriter? _writer;

    public event EventHandler<string>? LineAdded;

    public LoggingService(string logFilePath, int maxLines = DefaultMaxLines)
    {
        _logFilePath = logFilePath;
        _maxLines = maxLines;

        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        _writer = new StreamWriter(logFilePath, append: true, Encoding.UTF8) { AutoFlush = true };
    }

    public void Info(string correlationId, string message)
        => Write("INFO ", correlationId, message);

    public void Warn(string correlationId, string message)
        => Write("WARN ", correlationId, message);

    public void Error(string correlationId, string message, Exception? ex = null)
    {
        Write("ERROR", correlationId, message);
        if (ex is not null)
            Write("ERROR", correlationId, ex.ToString());
    }

    /// <summary>Returns a snapshot of the recent log lines held in memory.</summary>
    public IReadOnlyList<string> GetRecentLines()
    {
        lock (_lock)
            return _buffer.ToArray();
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _writer = null;
    }

    // -------------------------------------------------------------------------

    private void Write(string level, string correlationId, string message)
    {
        var line = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] [{level}] [{correlationId}] {message}";

        lock (_lock)
        {
            _buffer.Enqueue(line);
            while (_buffer.Count > _maxLines)
                _buffer.Dequeue();

            _writer?.WriteLine(line);
        }

        LineAdded?.Invoke(this, line);
    }
}
