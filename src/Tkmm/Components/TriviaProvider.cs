using CommunityToolkit.Mvvm.ComponentModel;

namespace Tkmm.Components;

public sealed partial class TriviaProvider : ObservableObject
{
    private static readonly string[] _source = GetSource();
    
    public static readonly TriviaProvider Instance = new();
    
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Timer _timer;
    private List<string> _current = [.._source];
    
    [ObservableProperty]
    private string _message = string.Empty;

    public TriviaProvider()
    {
        _timer = new Timer(_ => {
            if (_current.Count is 0) {
                _current = [.. _source.Where(x => x != Message)];
            }
            
            int index = Random.Shared.Next(0, _current.Count - 1);
            Message = _current[index];
            _current.RemoveAt(index);
        });

        _timer.Change(0, 5000);
    }

    private static string[] GetSource()
    {
        using Stream? stream = typeof(TriviaProvider).Assembly.GetManifestResourceStream("Tkmm.Resources.Trivia.trivia.txt");
        if (stream is null) {
            return [];
        }

        List<string> lines = [];
        using StreamReader reader = new(stream);
        while (reader.ReadLine() is string line) {
            lines.Add(line);
        }
        
        return lines.ToArray();
    }
}