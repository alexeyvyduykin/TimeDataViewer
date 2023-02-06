using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public sealed class PlotModel : Model, IPlotModel
{
    // The plot view that renders this plot.    
    private WeakReference? _plotViewReference;
    private DateTimeAxis? _axisX;
    private CategoryAxis? _axisY;
    private List<Series> _series = new();

    public PlotModel() { }

    public IPlotView? PlotView => (_plotViewReference != null) ? (IPlotView?)_plotViewReference.Target : null;

    public CategoryAxis AxisY => _axisY!;

    public DateTimeAxis AxisX => _axisX!;

    public IReadOnlyCollection<Series> Series => _series;

    // Gets the plot area. This area is used to draw the series (not including axes or legends).
    public OxyRect PlotArea { get; private set; }

    public double PlotMarginLeft { get; set; }

    public double PlotMarginTop { get; set; }

    public double PlotMarginRight { get; set; }

    public double PlotMarginBottom { get; set; }

    void IPlotModel.AttachPlotView(IPlotView plotView)
    {
        var currentPlotView = PlotView;
        if (currentPlotView is null == false
            && plotView is null == false
            && ReferenceEquals(currentPlotView, plotView) == false)
        {
            throw new InvalidOperationException("This PlotModel is already in use by some other PlotView control.");
        }

        _plotViewReference = (plotView == null) ? null : new WeakReference(plotView);
    }

    public Series? GetSeriesFromPoint(ScreenPoint point, double limit = 100)
    {
        double mindist = double.MaxValue;
        Series? nearestSeries = null;
        foreach (var series in _series.Reverse<Series>().Where(s => s.IsVisible))
        {
            var thr = series.GetNearestPoint(point, true) ?? series.GetNearestPoint(point, false);

            if (thr == null)
            {
                continue;
            }

            // find distance to this point on the screen
            double dist = point.DistanceTo(thr.Position);
            if (dist < mindist)
            {
                nearestSeries = series;
                mindist = dist;
            }
        }

        if (mindist < limit)
        {
            return nearestSeries;
        }

        return null;
    }

    public void AddAxisX(DateTimeAxis axis)
    {
        _axisX = axis;

        if (_axisY != null && _series.Count != 0)
        {
            DataChanged();
        }
    }

    public void AddAxisY(CategoryAxis axis)
    {
        _axisY = axis;

        if (_axisX != null && _series.Count != 0)
        {
            DataChanged();
        }
    }

    public void AddSeries(IEnumerable<Series> series)
    {
        _series = new(series);

        if (_axisX != null && _axisY != null)
        {
            DataChanged();
        }
    }

    private void DataChanged()
    {
        var visibleSeries = _series.Where(s => s.IsVisible).ToArray();

        AxisX.UpdateFromSeries(_series.ToArray());
        AxisY.UpdateFromSeries(visibleSeries);

        AxisX.ResetDataMaxMin();
        AxisY.ResetDataMaxMin();

        foreach (var s in visibleSeries)
        {
            s.UpdateAxisMaxMin();
        }

        AxisX.UpdateActualMaxMin();
        AxisY.UpdateActualMaxMin();
    }

    // Renders the plot with the specified rendering context.
    public void UpdateRenderInfo(double width, double height)
    {
        lock (SyncRoot)
        {
            try
            {
                PlotArea = new OxyRect(0, 0, width, height);

                AxisX.UpdateAll(this);
                AxisY.UpdateAll(this);

                _series.ForEach(s => s.UpdateRenderInfo());
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }

    public void PanUp()
    {
        AxisY.PanUp();

        PlotView?.InvalidatePlot();
    }

    public void PanDown()
    {
        AxisY.PanDown();

        PlotView?.InvalidatePlot();
    }

    public void ZoomToCount(int count)
    {
        AxisY.ZoomToCategoryCount(count);

        PlotView?.InvalidatePlot();
    }
}
