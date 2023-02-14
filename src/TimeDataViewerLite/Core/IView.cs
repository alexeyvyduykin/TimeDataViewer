using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public interface IView
{
    PlotModel? PlotModel { get; }

    // Gets the coordinates of the client area of the view.
    OxyRect ClientArea { get; }

    // Sets the cursor type.
    void SetCursorType(CursorType cursorType);

    // Hides the zoom rectangle.
    void HideZoomRectangle();

    // Shows the zoom rectangle.
    void ShowZoomRectangle(OxyRect rectangle);
}
