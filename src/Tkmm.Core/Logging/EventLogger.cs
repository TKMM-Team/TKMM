using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Tkmm.Core.Models;

namespace Tkmm.Core.Logging;

public class EventLogger(string group) : ILogger
{
    public static event Action<LogLevel, EventId, Exception?, string> OnLog = (_, _, _, _) => { };

    public static ObservableCollection<EventLog> Logs { get; } = [];

    private readonly string _group = group;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Logs.Add(new EventLog(
            logLevel,
            eventId,
            $"[{_group}] {formatter(state, exception).HideUsername()}",
            exception)
        );

        OnLog(logLevel, eventId, exception, formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}