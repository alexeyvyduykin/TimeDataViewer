using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public class LabelInfo
{
    public ScreenPoint Position { get; set; }

    public string? Text { get; set; }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public VerticalAlignment VerticalAlignment { get; set; }
}

public partial class DateTimeAxis
{
    private IList<double> _majorLabelValues = new List<double>();
    private IList<double> _majorTickValues = new List<double>();
    private IList<double> _minorTickValues = new List<double>();

    private IList<ScreenPoint>? _minorGridLines;
    private IList<ScreenPoint>? _minorTicks;
    private IList<ScreenPoint>? _majorGridLines;
    private IList<ScreenPoint>? _majorTicks;
    private IList<LabelInfo>? _labelInfos;

    public IList<ScreenPoint>? MinorGridLines => _minorGridLines;

    public IList<ScreenPoint>? MinorTicks => _minorTicks;

    public IList<ScreenPoint>? MajorGridLines => _majorGridLines;

    public IList<ScreenPoint>? MajorTicks => _majorTicks;

    public IList<LabelInfo>? LabelInfos => _labelInfos;

    public override void UpdateRenderInfo(PlotModel plot)
    {
        _minorTickValues = CreateDateTimeTickValues(ActualMinimum, ActualMaximum, (double)ActualMinorStep!, _actualMinorIntervalType, (int)FirstDayOfWeek);

        _majorTickValues = CreateDateTimeTickValues(ActualMinimum, ActualMaximum, (double)ActualMajorStep!, _actualIntervalType, (int)FirstDayOfWeek);

        _majorLabelValues = _majorTickValues;

        (_minorGridLines, _minorTicks) = RenderMinorItems(plot);
        (_majorGridLines, _majorTicks, _labelInfos) = RenderMajorItems(plot);
    }

    // Snaps v to value if it is within the specified distance.
    private static void SnapTo(double target, ref double v, double eps = 0.5)
    {
        if (v > target - eps && v < target + eps)
        {
            v = target;
        }
    }

    private (List<ScreenPoint> gridLines, List<ScreenPoint> ticks, List<LabelInfo> infos) RenderMajorItems(PlotModel plot)
    {
        // Axis position (x or y screen coordinate)
        double axisPosition = plot.PlotMarginTop - AxisDistance;

        double eps = (double)ActualMinorStep! * 1e-3;

        List<ScreenPoint> gridLines = new();
        List<ScreenPoint> ticks = new();
        List<LabelInfo> infos = new();

        var a0 = 0;
        var a1 = -MajorTickSize;

        var dontRenderZero = false;

        foreach (double value in _majorTickValues)
        {
            if (value < ActualMinimum - eps || value > ActualMaximum + eps)
            {
                continue;
            }

            if (dontRenderZero && Math.Abs(value) < eps)
            {
                continue;
            }

            double transformedValue = Transform(value);

            SnapTo(plot.PlotArea.Left, ref transformedValue);
            SnapTo(plot.PlotArea.Right, ref transformedValue);

            gridLines.Add(new ScreenPoint(transformedValue, plot.PlotArea.Top));
            gridLines.Add(new ScreenPoint(transformedValue, plot.PlotArea.Bottom));

            if (MajorTickSize > 0)
            {
                ticks.Add(new ScreenPoint(transformedValue, axisPosition + a0));
                ticks.Add(new ScreenPoint(transformedValue, axisPosition + a1));
            }
        }

        // Render the axis labels (numbers or category names)
        foreach (double value in _majorLabelValues)
        {
            if (value < ActualMinimum - eps || value > ActualMaximum + eps)
            {
                continue;
            }

            if (dontRenderZero && Math.Abs(value) < eps)
            {
                continue;
            }

            double transformedValue = Transform(value);

            SnapTo(plot.PlotArea.Left, ref transformedValue);
            SnapTo(plot.PlotArea.Right, ref transformedValue);

            var pt = new ScreenPoint(transformedValue, axisPosition + a1 - AxisTickToLabelDistance);

            GetRotatedAlignments(0, out var ha, out var va);

            infos.Add(new LabelInfo()
            {
                Position = pt,
                Text = ToLabel(value),
                HorizontalAlignment = ha,
                VerticalAlignment = va
            });
        }

        return (gridLines, ticks, infos);
    }

    private (List<ScreenPoint> gridLines, List<ScreenPoint> ticks) RenderMinorItems(PlotModel plot)
    {
        // Axis position (x or y screen coordinate)
        double axisPosition = plot.PlotMarginTop - AxisDistance;

        double eps = (double)ActualMinorStep! * 1e-3;

        List<ScreenPoint> gridLines = new();
        List<ScreenPoint> ticks = new();

        var a0 = 0;
        var a1 = -MinorTickSize;

        foreach (double value in _minorTickValues)
        {
            if (value < ActualMinimum - eps || value > ActualMaximum + eps)
            {
                continue;
            }

            if (_majorTickValues.Contains(value))
            {
                continue;
            }

            double transformedValue = Transform(value);

            SnapTo(plot.PlotArea.Left, ref transformedValue);
            SnapTo(plot.PlotArea.Right, ref transformedValue);

            // Draw the minor grid line                                           
            gridLines.Add(new ScreenPoint(transformedValue, plot.PlotArea.Top));
            gridLines.Add(new ScreenPoint(transformedValue, plot.PlotArea.Bottom));

            // Draw the minor tick
            if (MinorTickSize > 0)
            {
                ticks.Add(new ScreenPoint(transformedValue, axisPosition + a0));
                ticks.Add(new ScreenPoint(transformedValue, axisPosition + a1));
            }
        }

        return (gridLines, ticks);
    }

    /// <summary>
    /// Gets the alignments given the specified rotation angle.
    /// </summary>
    /// <param name="boxAngle">The angle of a box to rotate (usually it is label angle).</param>
    /// <param name="axisAngle">
    /// The axis angle, the original angle belongs to. The Top axis should have 0, next angles are computed clockwise. 
    /// The angle should be in [-180, 180). (T, R, B, L) is (0, 90, -180, -90). 
    /// </param>
    /// <param name="ha">Horizontal alignment.</param>
    /// <param name="va">Vertical alignment.</param>
    /// <remarks>
    /// This method is supposed to compute the alignment of the labels that are put near axis. 
    /// Because such labels can have different angles, and the axis can have different angles as well,
    /// computing the alignment is not straightforward.
    /// </remarks>       
    private static void GetRotatedAlignments(double axisAngle, out HorizontalAlignment ha, out VerticalAlignment va)
    {
        const double AngleTolerance = 10.0;

        // The axis angle if it would have been turned on 180 and leave it in [-180, 180)
        double flippedAxisAngle = ((axisAngle + 360.0) % 360.0) - 180.0;

        // When the box (assuming the axis and box have the same angle) box starts to turn clockwise near the axis
        // It leans on the right until it gets to 180 rotation, when it is started to lean on the left.
        // In real computation we need to compute this in relation with axisAngle
        // So if axisAngle <= boxAngle < (axisAngle + 180), we align Right, else - left.
        // The check looks inverted because flippedAxisAngle has the opposite sign.
        ha = 0.0 >= Math.Min(axisAngle, flippedAxisAngle) && 0.0 < Math.Max(axisAngle, flippedAxisAngle) ? HorizontalAlignment.Left : HorizontalAlignment.Right;

        // If axisAngle was < 0, we need to shift the previous computation on 180.
        if (axisAngle < 0)
        {
            ha = (HorizontalAlignment)((int)ha * -1);
        }

        va = VerticalAlignment.Middle;

        // If the angle almost the same as axisAngle (or axisAngle + 180) - set horizontal alignment to Center
        if (Math.Abs(0.0 - flippedAxisAngle) < AngleTolerance || Math.Abs(0.0 - axisAngle) < AngleTolerance)
        {
            ha = HorizontalAlignment.Center;
        }

        // And vertical alignment according to whether it is near to axisAngle or flippedAxisAngle
        if (Math.Abs(0.0 - axisAngle) < AngleTolerance)
        {
            va = VerticalAlignment.Bottom;
        }

        if (Math.Abs(0.0 - flippedAxisAngle) < AngleTolerance)
        {
            va = VerticalAlignment.Top;
        }
    }

    private static IList<double> CreateDateTickValues(double min, double max, double step, DateTimeIntervalType intervalType, int firstDayOfWeek)
    {
        var values = new List<double>();
        var start = ToDateTime(min);
        if (start.Ticks == 0)
        {
            // Invalid start time
            return values;
        }

        switch (intervalType)
        {
            case DateTimeIntervalType.Weeks:

                // make sure the first tick is at the 1st day of a week
                start = start.AddDays(-(int)start.DayOfWeek + firstDayOfWeek);
                break;
            case DateTimeIntervalType.Months:

                // make sure the first tick is at the 1st of a month
                start = new DateTime(start.Year, start.Month, 1);
                break;
            case DateTimeIntervalType.Years:

                // make sure the first tick is at Jan 1st
                start = new DateTime(start.Year, 1, 1);
                break;
        }

        // Adds a tick to the end time to make sure the end DateTime is included.
        var end = ToDateTime(max).AddTicks(1);
        if (end.Ticks == 0)
        {
            // Invalid end time
            return values;
        }

        var current = start;
        double eps = step * 1e-3;
        var minDateTime = ToDateTime(min - eps);
        var maxDateTime = ToDateTime(max + eps);

        if (minDateTime.Ticks == 0 || maxDateTime.Ticks == 0)
        {
            // Invalid min/max time
            return values;
        }

        while (current < end)
        {
            if (current > minDateTime && current < maxDateTime)
            {
                values.Add(ToDouble(current));
            }

            try
            {
                current = intervalType switch
                {
                    DateTimeIntervalType.Months => current.AddMonths((int)Math.Ceiling(step)),
                    DateTimeIntervalType.Years => current.AddYears((int)Math.Ceiling(step)),
                    _ => current.AddDays(step),
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                // AddMonths/AddYears/AddDays can throw an exception
                // We could test this by comparing to MaxDayValue/MinDayValue, but it is easier to catch the exception...
                break;
            }
        }

        return values;
    }

    private static IList<double> CreateDateTimeTickValues(double min, double max, double interval, DateTimeIntervalType intervalType, int firstDayOfWeek)
    {
        // If the step size is more than 7 days (e.g. months or years) we use a specialized tick generation method that adds tick values with uneven spacing...
        if (intervalType > DateTimeIntervalType.Days)
        {
            return CreateDateTickValues(min, max, interval, intervalType, firstDayOfWeek);
        }

        // For shorter step sizes we use the method from Axis
        return CreateTickValues(min, max, interval);
    }

    // Creates tick values at the specified interval.
    private static IList<double> CreateTickValues(double from, double to, double step, int maxTicks = 1000)
    {
        return AxisUtilities.CreateTickValues(from, to, step, maxTicks);
    }
}
