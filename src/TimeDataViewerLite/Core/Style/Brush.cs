namespace TimeDataViewerLite.Core.Style;

public class Brush
{
    public Brush(Color color, double opacity = 1.0)
    {
        Color = color;
        Opacity = opacity;
    }

    public Color Color { get; set; }

    public double Opacity { get; set; }
}
