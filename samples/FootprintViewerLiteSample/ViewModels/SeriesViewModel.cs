using ReactiveUI.Fody.Helpers;

namespace FootprintViewerLiteSample.ViewModels;

public class SeriesViewModel : ViewModelBase
{
    public string Name { get; set; } = string.Empty;

    [Reactive]
    public bool IsVisible { get; set; }
}
