#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Core
{
    //public delegate void DragChangedEventHandler();
    //public delegate void SCZoomChanged();
    public delegate void SizeChangedEventHandler(int width, int height);
    public delegate void WindowAreaChangedEventHandler(RectI window);
    public delegate void ViewportScreenChangedEventHandler(RectD viewport);
    public delegate void ViewportDataChangedEventHandler(RectD viewport);

    public delegate void SCTargetMarkerPositionChanged(MarkerViewModel marker);
    public delegate void SchedulerTypeChanged(BaseTimeSchedulerProvider type);
    public delegate void SCPositionChanged(Point2D point);

    public delegate void SCAxisLengthChanged(int length);
    //public delegate void AxisChanged();
    public delegate void AxisRect3Changed(int w, int h);


    public delegate void SchedulerWindowChanged(RectI window);
    public delegate void SchedulerViewportChanged(RectD viewport);

    public delegate void AxisShapeChanged(AxisInfo info);
}
