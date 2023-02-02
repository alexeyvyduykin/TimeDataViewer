namespace TimeDataViewerLite.Core;

public interface IPlotModel
{
    void UpdateRenderInfo(double width, double height);

    void AttachPlotView(IPlotView plotView);
}
