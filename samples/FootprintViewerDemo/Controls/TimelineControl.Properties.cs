using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;

namespace FootprintViewerDemo.Controls;

public partial class TimelineControl
{
    static TimelineControl()
    {
        BeginProperty.Changed.AddClassHandler<TimelineControl>(BeginChanged);
        DurationProperty.Changed.AddClassHandler<TimelineControl>(DurationChanged);
    }

    protected static void BeginChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (((TimelineControl)d)._slider != null && e.NewValue is double begin)
        {
            ((TimelineControl)d)._slider!.Begin = begin;
        }
    }

    protected static void DurationChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (((TimelineControl)d)._slider != null && e.NewValue is double duration)
        {
            ((TimelineControl)d)._slider!.Duration = duration;
        }
    }

    public static readonly StyledProperty<double> BeginProperty =
        AvaloniaProperty.Register<TimelineControl, double>(nameof(Begin));

    public double Begin
    {
        get
        {
            return GetValue(BeginProperty);
        }

        set
        {
            SetValue(BeginProperty, value);
        }
    }

    public static readonly StyledProperty<double> DurationProperty =
        AvaloniaProperty.Register<TimelineControl, double>(nameof(Duration));

    public double Duration
    {
        get
        {
            return GetValue(DurationProperty);
        }

        set
        {
            SetValue(DurationProperty, value);
        }
    }

    public static readonly StyledProperty<ControlTemplate> DefaultLabelTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, ControlTemplate>(nameof(DefaultLabelTemplate));

    public ControlTemplate DefaultLabelTemplate
    {
        get
        {
            return GetValue(DefaultLabelTemplateProperty);
        }

        set
        {
            SetValue(DefaultLabelTemplateProperty, value);
        }
    }

    public static readonly StyledProperty<IDataTemplate> TrackerTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, IDataTemplate>(nameof(TrackerTemplate));

    public IDataTemplate TrackerTemplate
    {
        get
        {
            return GetValue(TrackerTemplateProperty);
        }

        set
        {
            SetValue(TrackerTemplateProperty, value);
        }
    }

    public static readonly StyledProperty<ControlTemplate> ZoomRectangleTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, ControlTemplate>(nameof(ZoomRectangleTemplate));

    public ControlTemplate ZoomRectangleTemplate
    {
        get
        {
            return GetValue(ZoomRectangleTemplateProperty);
        }

        set
        {
            SetValue(ZoomRectangleTemplateProperty, value);
        }
    }
}
