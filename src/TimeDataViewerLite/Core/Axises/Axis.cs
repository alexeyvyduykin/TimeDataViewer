using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public abstract partial class Axis
{
    protected double _offset;
    protected double _scale;
    // Gets or sets the absolute maximum. This is only used for the UI control.
    // It will not be possible to zoom/pan beyond this limit. The default value is <c>double.MaxValue</c>.    
    private double _absoluteMaximum = double.MaxValue;
    // Gets or sets the absolute minimum. This is only used for the UI control.
    // It will not be possible to zoom/pan beyond this limit. The default value is <c>double.MinValue</c>.     
    private double _absoluteMinimum = double.MinValue;

    // Gets or sets the current view's maximum. This value is used when the user zooms or pans.
    protected double? _viewMaximum;

    // Gets or sets the current view's minimum. This value is used when the user zooms or pans.
    protected double? _viewMinimum;

    // Gets or sets the minimum value of the axis.
    public double? Minimum { get; set; }

    // Gets or sets the maximum value of the axis.                                                        
    public double? Maximum { get; set; }

    public double? ActualMinorStep { get; protected set; } = null;

    public double? ActualMajorStep { get; protected set; } = null;

    // Gets or sets the actual maximum value of the axis.
    public double ActualMaximum { get; protected set; } = 100;

    // Gets or sets the actual minimum value of the axis.
    public double ActualMinimum { get; protected set; } = 0;

    public string? ActualStringFormat { get; protected set; }

    // Gets or sets the distance from the end of the tick lines to the labels. The default value is <c>4</c>.
    public double AxisTickToLabelDistance { get; set; } = 4;

    // Gets or sets the distance between the plot area and the axis. The default value is <c>0</c>.
    public double AxisDistance { get; set; } = 0;

    // Gets or sets the maximum value of the data displayed on this axis.
    public double? DataMaximum { get; protected set; }

    // Gets or sets the minimum value of the data displayed on this axis.
    public double? DataMinimum { get; protected set; }

    // Gets or sets the maximum length (screen space) of the intervals. The available length of the axis will be divided by this length to get the approximate number of major intervals on the axis. The default value is <c>60</c>.
    public double IntervalLength { get; set; } = 60;

    public bool IsAxisVisible { get; set; } = true;

    public bool IsPanEnabled { get; set; } = true;

    public bool IsZoomEnabled { get; set; } = true;

    // Gets or sets the interval between minor ticks. The default value is <c>double.NaN</c>.
    public double? MinorStep { get; set; } = null;

    public double? MajorStep { get; set; } = null;

    public double MajorTickSize { get; set; } = 7;

    // Gets or sets the maximum range of the axis. Setting this property ensures that <c>ActualMaximum-ActualMinimum &lt; MaximumRange</c>. The default value is <c>double.PositiveInfinity</c>.
    public double MaximumRange { get; set; } = double.PositiveInfinity;

    // Gets or sets the minimum value for the interval between major ticks. The default value is <c>0</c>.
    public double MinimumMajorStep { get; set; } = 0;

    // Gets or sets the minimum value for the interval between minor ticks. The default value is <c>0</c>.
    public double MinimumMinorStep { get; set; } = 0;

    // Gets or sets the minimum range of the axis. Setting this property ensures that <c>ActualMaximum-ActualMinimum > MinimumRange</c>. The default value is <c>0</c>.
    public double MinimumRange { get; set; } = 0;

    // Gets or sets the size of the minor ticks. The default value is <c>4</c>.
    public double MinorTickSize { get; set; } = 4;

    // Gets the offset. This is used to transform between data and screen coordinates.
    public double Offset => _offset;

    public AxisPosition Position { get; set; } = AxisPosition.Left;

    // Gets the scaling factor of the axis. This is used to transform between data and screen coordinates.
    public double Scale => _scale;

    // Gets or sets the screen coordinate of the maximum end of the axis.
    public ScreenPoint ScreenMax { get; protected set; }

    // Gets or sets the screen coordinate of the minimum end of the axis.
    public ScreenPoint ScreenMin { get; protected set; }

    // Gets or sets the string format used for formatting the axis values. The default value is <c>null</c>.
    public string? StringFormat { get; set; }

    public void SetAvailableRange(double min, double max)
    {
        _absoluteMinimum = min;
        _absoluteMaximum = max;
    }

    public abstract void UpdateRenderInfo(PlotModel plot);

    public abstract string ToLabel(double x);

    internal abstract void UpdateFromSeries(Series[] series);

    // Inverse transform the specified screen point.
    public DataPoint InverseTransform(double x, double y, Axis yaxis)
    {
        return new DataPoint(InverseTransform(x), yaxis != null ? yaxis.InverseTransform(y) : 0);
    }

    // Inverse transforms the specified screen coordinate. This method can only be used with non-polar coordinate systems.
    public double InverseTransform(double sx) => (sx / _scale) + _offset;

    // Transforms the specified coordinate to screen coordinates. This method can only be used with non-polar coordinate systems.
    public double Transform(double x) => (x - _offset) * _scale;

    public bool IsHorizontal() => Position == AxisPosition.Top || Position == AxisPosition.Bottom;

    public virtual void Pan(ScreenPoint previousPoint, ScreenPoint currentPoint)
    {
        if (IsPanEnabled == false)
        {
            return;
        }

        bool isHorizontal = IsHorizontal();

        double dsx = isHorizontal ? currentPoint.X - previousPoint.X : currentPoint.Y - previousPoint.Y;
        Pan(dsx);
    }

    public virtual void Pan(double delta)
    {
        if (IsPanEnabled == false)
        {
            return;
        }

        double dx = delta / Scale;

        double newMinimum = ActualMinimum - dx;
        double newMaximum = ActualMaximum - dx;

        if (newMinimum < _absoluteMinimum)
        {
            newMinimum = _absoluteMinimum;
            newMaximum = Math.Min(newMinimum + ActualMaximum - ActualMinimum, _absoluteMaximum);
        }

        if (newMaximum > _absoluteMaximum)
        {
            newMaximum = _absoluteMaximum;
            newMinimum = Math.Max(newMaximum - (ActualMaximum - ActualMinimum), _absoluteMinimum);
        }

        _viewMinimum = newMinimum;
        _viewMaximum = newMaximum;

        UpdateActualMaxMin();
    }

    // Transforms the specified point to screen coordinates.
    public ScreenPoint Transform(double x, double y, Axis yaxis)
    {
        if (yaxis == null)
        {
            throw new NullReferenceException("Y axis should not be null when transforming.");
        }

        return new ScreenPoint(Transform(x), yaxis.Transform(y));
    }

    public virtual void Zoom(double newScale)
    {
        double sx1 = Transform(ActualMaximum);
        double sx0 = Transform(ActualMinimum);

        double sgn = Math.Sign(_scale);
        double mid = (ActualMaximum + ActualMinimum) / 2;

        double dx = (_offset - mid) * _scale;
        var newOffset = (dx / (sgn * newScale)) + mid;

        _scale = sgn * newScale;
        _offset = newOffset;

        double newMaximum = InverseTransform(sx1);
        double newMinimum = InverseTransform(sx0);

        if (newMinimum < _absoluteMinimum
            && newMaximum > _absoluteMaximum)
        {
            newMinimum = _absoluteMinimum;
            newMaximum = _absoluteMaximum;
        }
        else
        {
            if (newMinimum < _absoluteMinimum)
            {
                double d = newMaximum - newMinimum;
                newMinimum = _absoluteMinimum;
                newMaximum = _absoluteMinimum + d;
                if (newMaximum > _absoluteMaximum)
                {
                    newMaximum = _absoluteMaximum;
                }
            }
            else if (newMaximum > _absoluteMaximum)
            {
                double d = newMaximum - newMinimum;
                newMaximum = _absoluteMaximum;
                newMinimum = _absoluteMaximum - d;
                if (newMinimum < _absoluteMaximum)
                {
                    newMinimum = _absoluteMinimum;
                }
            }
        }

        _viewMaximum = newMaximum;
        _viewMinimum = newMinimum;

        UpdateActualMaxMin();
    }

    // Zooms the axis to the range [x0,x1].
    public virtual void Zoom(double x0, double x1)
    {
        if (IsZoomEnabled == false)
        {
            return;
        }

        _viewMinimum = Math.Max(Math.Min(x0, x1), _absoluteMinimum);
        _viewMaximum = Math.Min(Math.Max(x0, x1), _absoluteMaximum);

        UpdateActualMaxMin();
    }

    public virtual void ZoomAt(double factor, double x)
    {
        if (IsZoomEnabled == false)
        {
            return;
        }

        double dx0 = (ActualMinimum - x) * _scale;
        double dx1 = (ActualMaximum - x) * _scale;

        _scale *= factor;

        double newMinimum = (dx0 / _scale) + x;
        double newMaximum = (dx1 / _scale) + x;

        if (newMaximum - newMinimum > MaximumRange)
        {
            var mid = (newMinimum + newMaximum) * 0.5;
            newMaximum = mid + (MaximumRange * 0.5);
            newMinimum = mid - (MaximumRange * 0.5);
        }

        if (newMaximum - newMinimum < MinimumRange)
        {
            var mid = (newMinimum + newMaximum) * 0.5;
            newMaximum = mid + (MinimumRange * 0.5);
            newMinimum = mid - (MinimumRange * 0.5);
        }

        newMinimum = Math.Max(newMinimum, _absoluteMinimum);
        newMaximum = Math.Min(newMaximum, _absoluteMaximum);

        _viewMinimum = newMinimum;
        _viewMaximum = newMaximum;

        UpdateActualMaxMin();
    }

    // Zooms the axis with the specified zoom factor at the center of the axis.
    public virtual void ZoomAtCenter(double factor)
    {
        double sx = (Transform(ActualMaximum) + Transform(ActualMinimum)) * 0.5;
        var x = InverseTransform(sx);
        ZoomAt(factor, x);
    }

    // Modifies the data range of the axis [DataMinimum,DataMaximum] to includes the specified value.
    public virtual void Include(double value)
    {
        DataMinimum = Math.Min(DataMinimum ?? double.MaxValue, value);
        DataMaximum = Math.Max(DataMaximum ?? double.MinValue, value);
    }

    internal virtual void ResetDataMaxMin()
    {
        DataMaximum = null;
        DataMinimum = null;
    }

    /// <summary>
    /// Updates the <see cref="ActualMaximum" /> and <see cref="ActualMinimum" /> values.
    /// </summary>
    /// <remarks>If the user has zoomed/panned the axis, the internal ViewMaximum/ViewMinimum
    /// values will be used. If Maximum or Minimum have been set, these values will be used. Otherwise the maximum and minimum values
    /// of the series will be used, including the 'padding'.</remarks>
    internal virtual void UpdateActualMaxMin()
    {
        if (_viewMaximum != null)
        {
            // The user has zoomed/panned the axis, use the ViewMaximum value.
            ActualMaximum = (double)_viewMaximum;
        }
        else if (Maximum != null)
        {
            // The Maximum value has been set
            ActualMaximum = (double)Maximum;
        }
        else
        {
            // Calculate the actual maximum, including padding
            ActualMaximum = DataMaximum ?? 100;
        }

        if (_viewMinimum != null)
        {
            ActualMinimum = (double)_viewMinimum;
        }
        else if (Minimum != null)
        {
            ActualMinimum = (double)Minimum;
        }
        else
        {
            ActualMinimum = DataMinimum ?? 0;
        }

        if (_absoluteMaximum - _absoluteMinimum < MinimumRange)
        {
            throw new InvalidOperationException("MinimumRange should be larger than AbsoluteMaximum-AbsoluteMinimum.");
        }

        // Coerce the minimum range
        if (ActualMaximum - ActualMinimum < MinimumRange)
        {
            if (ActualMinimum + MinimumRange < _absoluteMaximum)
            {
                var average = (ActualMaximum + ActualMinimum) * 0.5;
                var delta = MinimumRange / 2;
                ActualMinimum = average - delta;
                ActualMaximum = average + delta;

                if (ActualMinimum < _absoluteMinimum)
                {
                    var diff = _absoluteMinimum - ActualMinimum;
                    ActualMinimum = _absoluteMinimum;
                    ActualMaximum += diff;
                }

                if (ActualMaximum > _absoluteMaximum)
                {
                    var diff = _absoluteMaximum - ActualMaximum;
                    ActualMaximum = _absoluteMaximum;
                    ActualMinimum += diff;
                }
            }
            else
            {
                if (_absoluteMaximum - MinimumRange > _absoluteMinimum)
                {
                    ActualMinimum = _absoluteMaximum - MinimumRange;
                    ActualMaximum = _absoluteMaximum;
                }
                else
                {
                    ActualMaximum = _absoluteMaximum;
                    ActualMinimum = _absoluteMinimum;
                }
            }
        }

        // Coerce the maximum range
        if (ActualMaximum - ActualMinimum > MaximumRange)
        {
            if (ActualMinimum + MaximumRange < _absoluteMaximum)
            {
                var average = (ActualMaximum + ActualMinimum) * 0.5;
                var delta = MaximumRange / 2;
                ActualMinimum = average - delta;
                ActualMaximum = average + delta;

                if (ActualMinimum < _absoluteMinimum)
                {
                    var diff = _absoluteMinimum - ActualMinimum;
                    ActualMinimum = _absoluteMinimum;
                    ActualMaximum += diff;
                }

                if (ActualMaximum > _absoluteMaximum)
                {
                    var diff = _absoluteMaximum - ActualMaximum;
                    ActualMaximum = _absoluteMaximum;
                    ActualMinimum += diff;
                }
            }
            else
            {
                if (_absoluteMaximum - MaximumRange > _absoluteMinimum)
                {
                    ActualMinimum = _absoluteMaximum - MaximumRange;
                    ActualMaximum = _absoluteMaximum;
                }
                else
                {
                    ActualMaximum = _absoluteMaximum;
                    ActualMinimum = _absoluteMinimum;
                }
            }
        }

        // Coerce the absolute maximum/minimum
        if (_absoluteMaximum <= _absoluteMinimum)
        {
            throw new InvalidOperationException("AbsoluteMaximum should be larger than AbsoluteMinimum.");
        }

        if (ActualMaximum <= ActualMinimum)
        {
            ActualMaximum = ActualMinimum + 100;
        }

        ActualMinimum = ToRange(ActualMinimum, _absoluteMinimum, _absoluteMaximum);

        ActualMaximum = ToRange(ActualMaximum, _absoluteMinimum, _absoluteMaximum);
    }

    protected virtual double CalculateMinorInterval(double majorInterval)
    {
        return AxisUtilities.CalculateMinorInterval(majorInterval);
    }

    private static double ToRange(double value, double min, double max)
    {
        var res = Math.Max(value, Math.Min(min, max));

        return Math.Min(res, Math.Max(min, max));
    }

    /// <summary>
    /// Calculates the actual interval.
    /// </summary>
    /// <param name="availableSize">Size of the available area.</param>
    /// <param name="maxIntervalSize">Maximum length of the intervals.</param>
    /// <returns>The calculate actual interval.</returns>
    protected virtual double CalculateActualInterval(double availableSize, double maxIntervalSize)
    {
        return CalculateActualInterval(availableSize, maxIntervalSize, ActualMaximum - ActualMinimum);
    }

    /// <summary>
    /// Returns the actual interval to use to determine which values are displayed in the axis.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <param name="maxIntervalSize">The maximum interval size.</param>
    /// <param name="range">The range.</param>
    /// <returns>Actual interval to use to determine which values are displayed in the axis.</returns>
    protected static double CalculateActualInterval(double availableSize, double maxIntervalSize, double range)
    {
        if (availableSize <= 0)
        {
            return maxIntervalSize;
        }

        if (Math.Abs(maxIntervalSize) < double.Epsilon)
        {
            throw new ArgumentException("Maximum interval size cannot be zero.", nameof(maxIntervalSize));
        }

        if (Math.Abs(range) < double.Epsilon)
        {
            throw new ArgumentException("Range cannot be zero.", nameof(range));
        }

        static double exponent(double x) => Math.Ceiling(Math.Log(x, 10));
        static double mantissa(double x) => x / Math.Pow(10, exponent(x) - 1);

        // reduce intervals for horizontal axis.
        // double maxIntervals = Orientation == AxisOrientation.x ? MaximumAxisIntervalsPer200Pixels * 0.8 : MaximumAxisIntervalsPer200Pixels;
        // real maximum interval count
        double maxIntervalCount = availableSize / maxIntervalSize;

        range = Math.Abs(range);
        double interval = Math.Pow(10, exponent(range));
        double intervalCandidate = interval;

        // Function to remove 'double precision noise'
        // TODO: can this be improved
        static double removeNoise(double x) => double.Parse(x.ToString("e14"));

        // decrease interval until interval count becomes less than maxIntervalCount
        while (true)
        {
            var m = (int)mantissa(intervalCandidate);
            if (m == 5)
            {
                // reduce 5 to 2
                intervalCandidate = removeNoise(intervalCandidate / 2.5);
            }
            else if (m == 2 || m == 1 || m == 10)
            {
                // reduce 2 to 1, 10 to 5, 1 to 0.5
                intervalCandidate = removeNoise(intervalCandidate / 2.0);
            }
            else
            {
                intervalCandidate = removeNoise(intervalCandidate / 2.0);
            }

            if (range / intervalCandidate > maxIntervalCount)
            {
                break;
            }

            if (double.IsNaN(intervalCandidate) || double.IsInfinity(intervalCandidate))
            {
                break;
            }

            interval = intervalCandidate;
        }

        return interval;
    }
}
