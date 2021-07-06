using Avalonia.Controls;
using Timeline.Models;
using Avalonia.Media;
using Avalonia;

namespace Timeline.Shapes
{
    public abstract class BaseIntervalVisual : BaseVisual
    {
        //public static readonly StyledProperty<Series> SeriesProperty =    
        //    AvaloniaProperty.Register<IntervalVisual, Series>(nameof(Series));

        //public Series Series
        //{
        //    get { return GetValue(SeriesProperty); }
        //    set { SetValue(SeriesProperty, value); }
        //}

        public abstract BaseIntervalVisual Clone(IInterval interval);
    }
}
