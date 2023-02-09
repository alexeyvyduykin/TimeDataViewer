using System.Globalization;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Core.Style;

namespace TimeDataViewerLite;

public static class Factory
{
    public static PlotModel CreatePlotModel(DateTime epoch, double begin, double end,
        IList<string> categories,
        IDictionary<string, Color> colors,
        Dictionary<string, List<Interval>> windows,
        Dictionary<string, List<Interval>> intervals)
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0,
        };

        var series = new List<Series>();

        foreach (var key in windows.Keys)
        {
            var list1 = new List<Interval>();
            var list2 = new List<Interval>();

            foreach (var label in categories)
            {
                list1.AddRange(windows[key].Where(s => Equals(s.Category, label)));
                list2.AddRange(intervals[key].Where(s => Equals(s.Category, label)));
            }

            series.Add(CreateSeries(plotModel, new Brush(colors[key], 0.35), list1, categories, key));
            series.Add(CreateSeries(plotModel, new Brush(colors[key]), list2, categories, key));
        }

        plotModel.AddAxisX(CreateAxisX(epoch, begin, end));
        plotModel.AddAxisY(CreateAxisY(categories));
        plotModel.AddSeries(series);

        return plotModel;
    }

    public static Series CreateSeries(PlotModel parent, Brush brush, IList<Interval> intervals, IList<string> labels, string stackGroup = "")
    {
        var list = new List<TimelineItem>();

        foreach (var item in intervals)
        {
            var category = item.Category ?? throw new Exception();

            list.Add(new TimelineItem()
            {
                Begin = DateTimeAxis.ToDouble(item.Begin),
                End = DateTimeAxis.ToDouble(item.End),
                Category = item.Category,
                CategoryIndex = labels.IndexOf(category)
            });
        }

        return new TimelineSeries(parent)
        {
            BarWidth = 0.5,
            Brush = brush,
            Items = list,
            IsVisible = true,
            StackGroup = stackGroup,
            TrackerKey = intervals.FirstOrDefault()?.Category ?? string.Empty,
        };
    }

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
            // IsZoomEnabled = false,
            IsZoomEnabled = true,
            // IsPanEnabled = false,
            IsPanEnabled = true,
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
