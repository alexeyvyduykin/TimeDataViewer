using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public class TrackerHitResult
{
    // Gets or sets the nearest or interpolated data point.
    public DataPoint DataPoint { get; set; }

    // Gets or sets the source item of the point.
    // If the current point is from an ItemsSource and is not interpolated, this property will contain the item.
    public object? Item { get; set; }

    // Gets or sets the index for the Item.
    public double Index { get; set; }

    public PlotModel? PlotModel { get; set; }

    // Gets or sets the position in screen coordinates.
    public ScreenPoint Position { get; set; }

    public Series? Series { get; set; }

    public string? Text { get; set; }

    public Axis? XAxis => Series is TimelineSeries xyas ? xyas.PlotModel.AxisX : null;

    public Axis? YAxis => Series is TimelineSeries xyas ? xyas.PlotModel.AxisY : null;

    public override string ToString()
    {
        return Text != null ? Text.Trim() : string.Empty;
    }
}
