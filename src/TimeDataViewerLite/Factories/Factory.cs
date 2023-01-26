using System.Globalization;

namespace TimeDataViewerLite;

public static class Factory
{
    public static Core.Axis CreateAxisX(DateTime epoch, double begin, double end)
    {
        return new Core.DateTimeAxis()
        {
            Position = Core.AxisPosition.Top,
            IntervalType = Core.DateTimeIntervalType.Auto,
            AbsoluteMinimum = begin,
            AbsoluteMaximum = end,
            CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek,
            FirstDayOfWeek = DayOfWeek.Monday,
            MinorIntervalType = Core.DateTimeIntervalType.Auto,
            Minimum = Core.DateTimeAxis.ToDouble(epoch),
            AxisDistance = 0.0,
            AxisTickToLabelDistance = 4.0,
            ExtraGridlines = null,
            IntervalLength = 60.0,
            IsPanEnabled = true,
            IsAxisVisible = true,
            IsZoomEnabled = true,
            Key = null,
            MajorStep = double.NaN,
            MajorTickSize = 7.0,
            MinorStep = double.NaN,
            MinorTickSize = 4.0,
            Maximum = double.NaN,
            MinimumRange = 0.0,
            MaximumRange = double.PositiveInfinity,
            StringFormat = null
        };
    }

    public static Core.Axis CreateAxisY(IEnumerable<object> labels, Func<object, string> func)
    {
        var axisY = new Core.CategoryAxis()
        {
            Position = Core.AxisPosition.Left,
            AbsoluteMinimum = -0.5,
            AbsoluteMaximum = 4.5,
            IsZoomEnabled = false,
            LabelField = "Label",
            IsTickCentered = false,
            GapWidth = 1.0,
            ItemsSource = labels
        };

        axisY.Labels.Clear();
        axisY.Labels.AddRange(labels.Select(func));

        return axisY;
    }
}
