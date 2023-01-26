using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public sealed class PlotModel : Model, IPlotModel
{
    // The plot view that renders this plot.    
    private WeakReference _plotViewReference;
    // Flags if the data has been updated.  
    private bool _isDataUpdated;
    private DateTimeAxis? _axisX;
    private CategoryAxis? _axisY;

    public PlotModel()
    {
        Series = new ElementCollection<Series>(this);
    }

    public IPlotView? PlotView => (_plotViewReference != null) ? (IPlotView?)_plotViewReference.Target : null;

    public CategoryAxis AxisY => _axisY!;

    public DateTimeAxis AxisX => _axisX!;

    public ElementCollection<Series> Series { get; private set; }

    // Gets the total width of the plot (in device units).       
    public double Width { get; private set; }

    // Gets the total height of the plot (in device units).      
    public double Height { get; private set; }

    // Gets the plot area. This area is used to draw the series (not including axes or legends).
    public OxyRect PlotArea { get; private set; }

    public double PlotMarginLeft { get; set; }

    public double PlotMarginTop { get; set; }

    public double PlotMarginRight { get; set; }

    public double PlotMarginBottom { get; set; }

    void IPlotModel.AttachPlotView(IPlotView plotView)
    {
        var currentPlotView = PlotView;
        if (ReferenceEquals(currentPlotView, null) == false
            && ReferenceEquals(plotView, null) == false
            && ReferenceEquals(currentPlotView, plotView) == false)
        {
            throw new InvalidOperationException("This PlotModel is already in use by some other PlotView control.");
        }

        _plotViewReference = (plotView == null) ? null : new WeakReference(plotView);
    }

    public Series GetSeriesFromPoint(ScreenPoint point, double limit = 100)
    {
        double mindist = double.MaxValue;
        Series nearestSeries = null;
        foreach (var series in Series.Reverse().Where(s => s.IsVisible))
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

    public void AddAxisX(DateTimeAxis axis) => _axisX = axis;

    public void AddAxisY(CategoryAxis axis) => _axisY = axis;

    /// <summary>
    /// Updates all axes and series.
    /// 0. Updates the owner PlotModel of all plot items (axes, series and annotations)
    /// 1. Updates the data of each Series (only if updateData==<c>true</c>).
    /// 2. Ensure that all series have axes assigned.
    /// 3. Updates the max and min of the axes.
    /// </summary>
    /// <param name="updateData">if set to <c>true</c> , all data collections will be updated.</param>
    void IPlotModel.Update(bool updateData)
    {
        lock (SyncRoot)
        {
            try
            {
                // Updates the default axes
                EnsureDefaultAxes();

                var visibleSeries = Series.Where(s => s.IsVisible).ToArray();

                // Update data of the series
                if (updateData || _isDataUpdated == false)
                {
                    foreach (var s in visibleSeries)
                    {
                        s.UpdateData();
                    }

                    _isDataUpdated = true;
                }

                // Updates axes with information from the series
                // This is used by the category axis that need to know the number of series using the axis.
                AxisX.UpdateFromSeries(visibleSeries);
                AxisX.ResetCurrentValues();

                AxisY.UpdateFromSeries(visibleSeries);
                AxisY.ResetCurrentValues();

                // Update valid data of the series
                // This must be done after the axes are updated from series!
                if (updateData)
                {
                    foreach (var s in visibleSeries)
                    {
                        s.UpdateValidData();
                    }
                }

                // Update the max and min of the axes
                UpdateMaxMin(updateData);

            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }

    /// <summary>
    /// Gets all elements of the model, top-level elements first.
    /// </summary>
    /// <returns>
    /// An enumerator of the elements.
    /// </returns>
    public override IEnumerable<UIElement> GetElements()
    {
        foreach (var s in Series.Reverse().Where(s => s.IsVisible))
        {
            yield return s;
        }

        foreach (var axis in new Axis[] { AxisX, AxisY }.Reverse().Where(a => a.IsAxisVisible))
        {
            yield return axis;
        }
    }

    /// <summary>
    /// Finds and sets the default horizontal and vertical axes (the first horizontal/vertical axes in the Axes collection).
    /// </summary>
    private void EnsureDefaultAxes()
    {
        // Update the axes of series without axes defined
        foreach (var s in Series)
        {
            if (s.IsVisible && s.AreAxesRequired())
            {
                s.EnsureAxes();
            }
        }
    }

    private void UpdateMaxMin(bool isDataUpdated)
    {
        if (isDataUpdated)
        {
            AxisX.ResetDataMaxMin();
            AxisY.ResetDataMaxMin();

            // data has been updated, so we need to calculate the max/min of the series again
            foreach (var s in Series.Where(s => s.IsVisible))
            {
                s.UpdateMaxMin();
            }
        }

        foreach (var s in Series.Where(s => s.IsVisible))
        {
            s.UpdateAxisMaxMin();
        }

        AxisX.UpdateActualMaxMin();
        AxisY.UpdateActualMaxMin();
    }

    // Renders the plot with the specified rendering context.
    void IPlotModel.Render(double width, double height)
    {
        RenderOverride(width, height);
    }

    // Renders the plot with the specified rendering context.
    private void RenderOverride(double width, double height)
    {
        lock (SyncRoot)
        {
            try
            {
                Width = width;
                Height = height;

                PlotArea = new OxyRect(0, 0, Width, Height);

                AxisX.UpdateTransform(PlotArea);
                AxisY.UpdateTransform(PlotArea);

                AxisX.UpdateIntervals(PlotArea);
                AxisY.UpdateIntervals(PlotArea);

                AxisX.ResetCurrentValues();
                AxisY.ResetCurrentValues();

                RenderSeries();

                AxisX.MyOnRender(this);
                AxisY.MyOnRender(this);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }

    private void RenderSeries()
    {
        foreach (var s in Series.Where(s => s.IsVisible))
        {
            s.Render();
            s.MyOnRender();
        }
    }
}
