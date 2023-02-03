using ReactiveUI.Fody.Helpers;

namespace FootprintViewerLiteSample.ViewModels;

public class ItemViewModel : ViewModelBase
{
    public string? Label { get; set; }

    [Reactive]
    public double Height { get; set; }
}
