using Avalonia;
using Avalonia.Markup.Xaml.Templates;

namespace FootprintViewerDemo.Controls;

public partial class TimelineControl
{
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

    public static readonly StyledProperty<ControlTemplate> DefaultTrackerTemplateProperty =
        AvaloniaProperty.Register<TimelineControl, ControlTemplate>(nameof(DefaultTrackerTemplate));

    public ControlTemplate DefaultTrackerTemplate
    {
        get
        {
            return GetValue(DefaultTrackerTemplateProperty);
        }

        set
        {
            SetValue(DefaultTrackerTemplateProperty, value);
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
