using Avalonia.Media;

namespace TimeDataViewerLite;

public static class BrushExtensions
{
    public static IBrush ToAvaloniaBrush(this Core.Style.Brush brush)
    {
        return new SolidColorBrush()
        {
            Color = Color.FromRgb((byte)brush.Color.R, (byte)brush.Color.G, (byte)brush.Color.B),
            Opacity = brush.Opacity,
        };
    }
}
