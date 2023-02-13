using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Factories;

public interface IFactory
{
    DateTimeAxis CreateAxisX();

    CategoryAxis CreateAxisY();

    TimelineSeries CreateSeries(PlotModel parent);
}
