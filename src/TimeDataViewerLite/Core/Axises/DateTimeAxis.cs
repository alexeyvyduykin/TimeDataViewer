using System.Globalization;
using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public sealed partial class DateTimeAxis : Axis
{
    private static readonly double[] _goodIntervals = BuildGoodIntervals();
    private static readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    private static readonly double _maxDayValue = (DateTime.MaxValue - _timeOrigin).TotalDays;
    private static readonly double _minDayValue = (DateTime.MinValue - _timeOrigin).TotalDays;
    private DateTimeIntervalType _actualIntervalType;
    private DateTimeIntervalType _actualMinorIntervalType;

    public DateTimeAxis()
    {
        Position = AxisPosition.Bottom;
        IntervalType = DateTimeIntervalType.Auto;
        FirstDayOfWeek = DayOfWeek.Monday;
        CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek;
    }

    private static double[] BuildGoodIntervals()
    {
        const double Year = 365.25;
        const double Month = 30.5;
        const double Week = 7;
        const double Day = 1.0;
        const double Hour = Day / 24;
        const double Minute = Hour / 60;
        const double Second = Minute / 60;
        const double MilliSecond = Second / 1000;

        return new[]
        {
            MilliSecond, 2 * MilliSecond, 10 * MilliSecond, 100 * MilliSecond,
            Second, 2 * Second, 5 * Second, 10 * Second, 30 * Second, Minute, 2 * Minute,
            5 * Minute, 10 * Minute, 30 * Minute, Hour, 4 * Hour, 8 * Hour, 12 * Hour, Day,
            2 * Day, 5 * Day, Week, 2 * Week, Month, 2 * Month, 3 * Month, 4 * Month,
            6 * Month, Year
        };
    }

    public CalendarWeekRule CalendarWeekRule { get; set; }

    public DayOfWeek FirstDayOfWeek { get; set; }

    public DateTimeIntervalType IntervalType { get; set; }

    public DateTimeIntervalType MinorIntervalType { get; set; }

    internal override void UpdateFromSeries(Series[] series)
    {

    }

    /// <summary>
    /// Converts a numeric representation of the date (number of days after the time origin) to a DateTime structure.
    /// </summary>
    /// <param name="value">The number of days after the time origin.</param>
    /// <returns>A <see cref="DateTime" /> structure. Ticks = 0 if the value is invalid.</returns>
    public static DateTime ToDateTime(double value)
    {
        if (double.IsNaN(value) || value < _minDayValue || value > _maxDayValue)
        {
            return new DateTime();
        }

        return _timeOrigin.AddDays(value - 1);
    }

    public static double ToDouble(DateTime value) => (value - _timeOrigin).TotalDays + 1;

    public override string ToLabel(double x)
    {
        var time = ToDateTime(x);

        var fmt = ActualStringFormat;
        if (fmt == null)
        {
            return time.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }

        int week = GetWeek(time);
        fmt = fmt.Replace("ww", week.ToString("00"));
        fmt = fmt.Replace("w", week.ToString(CultureInfo.InvariantCulture));
        fmt = string.Concat("{0:", fmt, "}");
        return string.Format(CultureInfo.CurrentCulture, fmt, time);
    }

    protected override double CalculateActualInterval(double availableSize, double maxIntervalSize)
    {
        double range = Math.Abs(ActualMinimum - ActualMaximum);

        double interval = _goodIntervals[0];

        int maxNumberOfIntervals = Math.Max((int)(availableSize / maxIntervalSize), 2);

        while (true)
        {
            if (range / interval < maxNumberOfIntervals)
            {
                break;
            }

            double nextInterval = _goodIntervals.FirstOrDefault(i => i > interval);
            if (Math.Abs(nextInterval) <= double.Epsilon)
            {
                nextInterval = interval * 2;
            }

            interval = nextInterval;
        }

        _actualIntervalType = IntervalType;
        _actualMinorIntervalType = MinorIntervalType;

        if (IntervalType == DateTimeIntervalType.Auto)
        {
            _actualIntervalType = DateTimeIntervalType.Milliseconds;

            if (interval >= 1.0 / 24 / 60 / 60)
            {
                _actualIntervalType = DateTimeIntervalType.Seconds;
            }

            if (interval >= 1.0 / 24 / 60)
            {
                _actualIntervalType = DateTimeIntervalType.Minutes;
            }

            if (interval >= 1.0 / 24)
            {
                _actualIntervalType = DateTimeIntervalType.Hours;
            }

            if (interval >= 1)
            {
                _actualIntervalType = DateTimeIntervalType.Days;
            }

            if (interval >= 30)
            {
                _actualIntervalType = DateTimeIntervalType.Months;
            }

            if (range >= 365.25)
            {
                _actualIntervalType = DateTimeIntervalType.Years;
            }
        }

        if (_actualIntervalType == DateTimeIntervalType.Months)
        {
            double monthsRange = range / 30.5;
            interval = CalculateActualInterval(availableSize, maxIntervalSize, monthsRange);
        }

        if (_actualIntervalType == DateTimeIntervalType.Years)
        {
            double yearsRange = range / 365.25;
            interval = CalculateActualInterval(availableSize, maxIntervalSize, yearsRange);
        }

        if (_actualMinorIntervalType == DateTimeIntervalType.Auto)
        {
            _actualMinorIntervalType = _actualIntervalType switch
            {
                DateTimeIntervalType.Years => DateTimeIntervalType.Months,
                DateTimeIntervalType.Months => DateTimeIntervalType.Days,
                DateTimeIntervalType.Weeks => DateTimeIntervalType.Days,
                DateTimeIntervalType.Days => DateTimeIntervalType.Hours,
                DateTimeIntervalType.Hours => DateTimeIntervalType.Minutes,
                _ => DateTimeIntervalType.Days,
            };
        }

        return interval;
    }

    private int GetWeek(DateTime date)
    {
        return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule, FirstDayOfWeek);
    }

    public void UpdateAll(PlotModel plot)
    {
        UpdateTransform(plot.PlotArea);

        UpdateIntervals(plot.PlotArea);

        UpdateRenderInfo(plot);
    }

    private void UpdateTransform(OxyRect bounds)
    {
        ScreenMin = new ScreenPoint(bounds.Left, bounds.Top);
        ScreenMax = new ScreenPoint(bounds.Right, bounds.Bottom);

        ScreenMin = new ScreenPoint(bounds.Left, bounds.Right);
        ScreenMax = new ScreenPoint(bounds.Right, bounds.Left);

        if (ActualMaximum - ActualMinimum < double.Epsilon)
        {
            ActualMaximum = ActualMinimum + 1;
        }

        double da = bounds.Left - bounds.Right;
        double range = ActualMaximum - ActualMinimum;

        if (Math.Abs(da) > double.Epsilon)
        {
            _offset = (bounds.Left / da * ActualMaximum) - (bounds.Right / da * ActualMinimum);
        }
        else
        {
            _offset = 0;
        }

        if (Math.Abs(range) > double.Epsilon)
        {
            _scale = (bounds.Right - bounds.Left) / range;
        }
        else
        {
            _scale = 1;
        }
    }

    private void UpdateIntervals(OxyRect plotArea)
    {
        var actualMajorStep = MajorStep ?? CalculateActualInterval(plotArea.Width, IntervalLength);
        var actualMinorStep = MinorStep ?? CalculateMinorInterval(actualMajorStep);

        ActualMinorStep = Math.Max(actualMinorStep, MinimumMinorStep);
        ActualMajorStep = Math.Max(actualMajorStep, MinimumMajorStep);

        ActualStringFormat = StringFormat;

        ActualMinorStep = _actualIntervalType switch
        {
            DateTimeIntervalType.Years => 31,
            DateTimeIntervalType.Weeks => 1,
            DateTimeIntervalType.Days => ActualMajorStep,
            DateTimeIntervalType.Hours => ActualMajorStep,
            DateTimeIntervalType.Minutes => ActualMajorStep,
            DateTimeIntervalType.Seconds => ActualMajorStep,
            DateTimeIntervalType.Milliseconds => ActualMajorStep,
            _ => ActualMinorStep
        };

        ActualMajorStep = _actualIntervalType switch
        {
            DateTimeIntervalType.Weeks => 7,
            _ => ActualMajorStep
        };

        _actualMinorIntervalType = _actualIntervalType switch
        {
            DateTimeIntervalType.Years => DateTimeIntervalType.Years,
            DateTimeIntervalType.Months => DateTimeIntervalType.Months,
            DateTimeIntervalType.Weeks => DateTimeIntervalType.Days,
            _ => _actualMinorIntervalType
        };

        ActualStringFormat = _actualIntervalType switch
        {
            DateTimeIntervalType.Years => "yyyy",
            DateTimeIntervalType.Months => "yyyy-MM-dd",
            DateTimeIntervalType.Weeks => "yyyy/ww",
            DateTimeIntervalType.Days => "yyyy-MM-dd",
            DateTimeIntervalType.Hours => "HH:mm",
            DateTimeIntervalType.Minutes => "HH:mm",
            DateTimeIntervalType.Seconds => "HH:mm:ss",
            DateTimeIntervalType.Milliseconds => "HH:mm:ss.fff",
            _ => throw new Exception()
        };
    }
}
