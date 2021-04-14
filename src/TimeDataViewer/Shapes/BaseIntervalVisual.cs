using Avalonia.Controls;
using TimeDataViewer.Markers;

namespace TimeDataViewer.Shapes
{
    public abstract class BaseIntervalVisual : Control
    {
        public abstract BaseIntervalVisual Clone(SchedulerInterval marker);
    }
}
