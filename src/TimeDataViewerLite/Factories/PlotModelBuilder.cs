using TimeDataViewerLite.Core;

namespace TimeDataViewerLite;

public static class PlotModelBuilder
{
    private static DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public static PlotModel Build(List<string> categories, List<SeriesInfo> seriesInfos, PlotModelState? state = null)
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0,
        };

        var min = seriesInfos.SelectMany(s => s.Items).Min(s => s.Begin);
        var max = seriesInfos.SelectMany(s => s.Items).Max(s => s.End);

        var begin0 = min.Date;

        var beginScenario = (begin0 - _timeOrigin).TotalDays + 1 - 1;
        var endScenario = beginScenario + 3;

        foreach (var series in seriesInfos)
        {
            plotModel.AddSeries(series.Items.ToTimelineItems(categories), series.Brush, series.StackGroup);
        }

        plotModel.UpdateAxisX(begin0, beginScenario, endScenario);
        plotModel.UpdateAxisY(categories);

        plotModel.InvalidateData(state);

        return plotModel;
    }

    public static PlotModel Build()
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0,
        };

        var begin = DateTime.Now;
        var begin0 = begin.Date;

        var beginScenario = (begin0 - _timeOrigin).TotalDays;
        var endScenario = beginScenario + 1;

        // TODO: not using yet
        var begin2 = (begin0 - _timeOrigin).TotalDays + 1;
        var duration2 = 1.0;

        plotModel.UpdateAxisX(begin0, beginScenario, endScenario);
        plotModel.UpdateAxisY(new List<string>());

        plotModel.InvalidateData();

        return plotModel;
    }

    private static List<TimelineItem> ToTimelineItems(this IList<IntervalInfo> intervals, IList<string> categories)
    {
        return intervals
            .Select(s => (Ival: s, Index: categories.IndexOf(s.Category ?? string.Empty)))
            .Where(s => s.Index != -1)
            .Select(s => new TimelineItem()
            {
                Begin = DateTimeAxis.ToDouble(s.Ival.Begin),
                End = DateTimeAxis.ToDouble(s.Ival.End),
                Category = s.Ival.Category,
                CategoryIndex = s.Index,
                BrushMode = s.Ival.BrushMode
            }).ToList();
    }
}
