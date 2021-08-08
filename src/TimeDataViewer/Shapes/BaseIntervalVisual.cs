using Avalonia.Controls;
using TimeDataViewer.ViewModels;
using Avalonia.Media;
using Avalonia;

namespace TimeDataViewer.Shapes
{
    public abstract class BaseIntervalVisual : BaseVisual
    {
        public abstract BaseIntervalVisual Clone(IntervalViewModel interval);
    }
}
