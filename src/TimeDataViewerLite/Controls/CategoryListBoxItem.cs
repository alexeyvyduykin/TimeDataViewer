using ReactiveUI;

namespace TimeDataViewerLite.Controls;

public class CategoryListBoxItem : ReactiveObject
{
    private string? _text;

    public string? Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
}
