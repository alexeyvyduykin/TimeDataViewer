using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public abstract class Series
{
    protected PlotModel _plotModel;

    public Series(PlotModel parent)
    {
        _plotModel = parent;
    }

    public PlotModel PlotModel => _plotModel;

    public bool IsVisible { get; set; } = true;

    public string TrackerFormatString { get; set; } = string.Empty;

    // Gets or sets the key for the tracker to use on this series. The default is <c>null</c>.                                                                                                                                                                         
    // This key may be used by the plot view to show a custom tracker for the series.                                                     
    public string? TrackerKey { get; set; }

    // Renders the series on the specified render context.
    public abstract void Render();

    // Updates the maximum and minimum values of the axes used by this series.    
    protected internal abstract void UpdateAxisMaxMin();

    // Updates the maximum and minimum values of the series.      
    protected internal abstract void UpdateMaxMin();

    public virtual TrackerHitResult? GetNearestPoint(ScreenPoint point, bool interpolate)
    {
        return null;
    }
}
