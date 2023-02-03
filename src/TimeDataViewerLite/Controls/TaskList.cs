using ReactiveUI;

namespace TimeDataViewerLite.Controls;

public class TaskListItem : ReactiveObject
{
    private double _height;
    private string? _text;

    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }

    public string? Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
}
