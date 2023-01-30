using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public abstract class Series : PlotElement
{
    public bool IsVisible { get; set; } = true;

    public string? TrackerFormatString { get; set; }

    // Gets or sets the key for the tracker to use on this series. The default is <c>null</c>.                                                                                                                                                                         
    // This key may be used by the plot view to show a custom tracker for the series.                                                     
    public string? TrackerKey { get; set; }

    // Renders the series on the specified render context.
    public abstract void Render();

    // Updates the maximum and minimum values of the axes used by this series.    
    protected internal abstract void UpdateAxisMaxMin();

    // Updates the data of the series.    
    protected internal abstract void UpdateData();

    // Updates the valid data of the series.      
    protected internal abstract void UpdateValidData();

    // Updates the maximum and minimum values of the series.      
    protected internal abstract void UpdateMaxMin();

    public virtual TrackerHitResult? GetNearestPoint(ScreenPoint point, bool interpolate)
    {
        return null;
    }

    protected override HitTestResult? HitTestOverride(HitTestArguments args)
    {
        var thr = GetNearestPoint(args.Point, true) ?? GetNearestPoint(args.Point, false);

        if (thr != null)
        {
            double distance = thr.Position.DistanceTo(args.Point);
            if (distance > args.Tolerance)
            {
                return null;
            }

            return new HitTestResult(this, thr.Position, thr.Item, thr.Index);
        }

        return null;
    }
}
