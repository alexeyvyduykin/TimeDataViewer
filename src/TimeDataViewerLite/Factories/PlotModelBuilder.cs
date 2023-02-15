using TimeDataViewerLite.Core;

namespace TimeDataViewerLite;

public static class PlotModelBuilder
{
    private static DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public static PlotModel Build(DateTime begin0, double begin, double end, List<string> categories, List<SeriesInfo> seriesInfos)
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0,
        };

        // TODO: not using yet
        var begin2 = (begin0 - _timeOrigin).TotalDays + 1;
        var duration2 = 1.0;

        foreach (var series in seriesInfos)
        {
            plotModel.AddSeries(series.Converter.Invoke(categories), series.Brush, series.StackGroup);
        }

        plotModel.UpdateAxisX(begin0, begin, end);
        plotModel.UpdateAxisY(categories);

        plotModel.InvalidateData();

        return plotModel;
    }
}
