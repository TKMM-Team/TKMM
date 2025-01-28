using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Tkmm.Core.Models;

public sealed partial class EventLog(LogLevel logLevel, EventId eventId, string content, Exception? exception) : ObservableObject
{
    [ObservableProperty]
    private LogLevel _logLevel = logLevel;

    [ObservableProperty]
    private EventId _eventId = eventId;

    [ObservableProperty]
    private DateTime _date = DateTime.UtcNow;

    [ObservableProperty]
    private string _content = content;

    [ObservableProperty]
    private Exception? _exception = exception;

    public string ToMarkdown()
    {
        return $"""
            `[{EventId.Id}: {LogLevel}]`
                 {Content}
            """;
    }

    public override string ToString()
    {
        return $"""
            [{EventId.Id,2}: {LogLevel,-12}]
                 {Content}
            """;
    }
}