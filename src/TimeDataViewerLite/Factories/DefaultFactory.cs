using System.Globalization;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Factories;

internal sealed class DefaultFactory : IFactory
{
    public DateTimeAxis CreateAxisX()
    {
        var axis = new DateTimeAxis()
        {
            Position = AxisPosition.Top,
            IntervalType = DateTimeIntervalType.Auto,
            CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek,
            FirstDayOfWeek = DayOfWeek.Monday,
            MinorIntervalType = DateTimeIntervalType.Auto,
            //Minimum = DateTimeAxis.ToDouble(epoch),
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

        return axis;
    }

    public CategoryAxis CreateAxisY()
    {
        var axis = new CategoryAxis()
        {
            Position = AxisPosition.Left,
            IsZoomEnabled = false,
            IsPanEnabled = false,
            IsTickCentered = false,
            GapWidth = 1.0,
        };

        return axis;
    }

    public TimelineSeries CreateSeries(PlotModel parent)
    {
        return new TimelineSeries(parent)
        {
            BarWidth = 0.5,
            IsVisible = true,
        };
    }
}
