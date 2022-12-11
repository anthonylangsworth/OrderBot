// Copied from https://stackoverflow.com/questions/52707702/how-do-you-mock-ilogger-loginformation

using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test;

public class FakeLogger<T> : ILogger, ILogger<T>
{
    public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

    public IEnumerable<LogEntry> InformationEntries =>
        LogEntries.Where(e => e.LogLevel == LogLevel.Information);

    public IEnumerable<LogEntry> WarningEntries =>
        LogEntries.Where(e => e.LogLevel == LogLevel.Warning);

    public IEnumerable<LogEntry> ErrorEntries =>
        LogEntries.Where(e => e.LogLevel == LogLevel.Error);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogEntries.Add(new LogEntry(logLevel, eventId, state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return new LoggingScope();
    }

    public class LoggingScope : IDisposable
    {
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

public record LogEntry : IEquatable<LogEntry>
{
    public LogEntry(LogLevel logLevel, EventId eventId, object? state, Exception? exception)
    {
        LogLevel = logLevel;
        EventId = eventId;
        State = state;
        Exception = exception;
    }

    public LogLevel LogLevel { get; }
    public EventId EventId { get; }
    public object? State { get; }
    public Exception? Exception { get; }
    public string Message => State?.ToString() ?? string.Empty;
}

public class LogEntryEqualityComparer : IEqualityComparer<LogEntry>
{
    public bool Equals(LogEntry? x, LogEntry? y)
    {
        return (x == null && y == null)
            || (x != null && y != null
            && x.LogLevel == y.LogLevel
            && EqualityComparer<EventId>.Default.Equals(x.EventId, x.EventId)
            && EqualityComparer<Exception>.Default.Equals(x.Exception, y.Exception)
            && string.Equals(x.Message, y.Message));
    }

    public int GetHashCode([DisallowNull] LogEntry obj)
    {
        throw new NotImplementedException();
    }
}
