using ReactiveUI.Fody.Helpers;

namespace FootprintViewerLiteSample.ViewModels;

public class SeriesViewModel : ViewModelBase
{
    public string? Name { get; set; }

    [Reactive]
    public bool IsVisible { get; set; }
}
