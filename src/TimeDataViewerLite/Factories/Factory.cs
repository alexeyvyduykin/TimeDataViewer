using System.Globalization;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite;

public static class Factory
{
    public static DateTimeAxis CreateAxisX(DateTime epoch, double begin, double end)
    {
        var axis = new DateTimeAxis()
        {
            Position = AxisPosition.Top,
            IntervalType = DateTimeIntervalType.Auto,
            CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek,
            FirstDayOfWeek = DayOfWeek.Monday,
            MinorIntervalType = DateTimeIntervalType.Auto,
            Minimum = DateTimeAxis.ToDouble(epoch),
            AxisDistance = 0.0,
            AxisTickToLabelDistance = 4.0,
            IntervalLength = 60.0,
            IsPanEnabled = true,
            IsAxisVisible = true,
            IsZoomEnabled = true,         
            MajorTickSize = 7.0,          
            MinorTickSize = 4.0,           
            MinimumRange = 0.0,
            MaximumRange = double.PositiveInfinity,
            StringFormat = null
        };

        axis.SetAvailableRange(begin, end);

        return axis;
    }

    public static CategoryAxis CreateAxisY(IEnumerable<string> labels)
    {
        var axis = new CategoryAxis()
        {
            Position = AxisPosition.Left,
            IsZoomEnabled = false,
            IsPanEnabled = false,
            IsTickCentered = false,
            GapWidth = 1.0,
            SourceLabels = new List<string>(labels)
        };

        var count = labels.Count();
        var min = -0.5;
        var max = min + count;

        axis.SetAvailableRange(min, max);

        return axis;
    }
}
