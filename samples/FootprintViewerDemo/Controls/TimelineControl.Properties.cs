using Avalonia;
using Avalonia.Markup.Xaml.Templates;
using TimeDataViewer;

namespace FootprintViewerDemo.Controls;

public partial class TimelineControl
{
    static TimelineControl()
    {
        PaddingProperty.OverrideDefaultValue<TimelineControl>(new Thickness(8));
        PaddingProperty.Changed.AddClassHandler<TimelineControl>(AppearanceChanged);

        SliderProperty.Changed.AddClassHandler<TimelineControl>(SliderChanged);
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


    public static readonly StyledProperty<Slider> SliderProperty =
        AvaloniaProperty.Register<TimelineControl, Slider>(nameof(Slider));

    public Slider Slider
    {
        get
        {
            return GetValue(SliderProperty);
        }

        set
        {
            SetValue(SliderProperty, value);
        }
    }
}
