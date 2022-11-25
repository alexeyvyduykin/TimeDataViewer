using System.Globalization;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using TimeDataViewer;

namespace FootprintViewerDemo.Controls;

public partial class TimelineControl
{
    static TimelineControl()
    {
        PaddingProperty.OverrideDefaultValue<TimelineControl>(new Thickness(8));
        PaddingProperty.Changed.AddClassHandler<TimelineControl>(AppearanceChanged);
        CultureProperty.Changed.AddClassHandler<TimelineControl>(AppearanceChanged);
        DefaultFontProperty.Changed.AddClassHandler<TimelineControl>(AppearanceChanged);
        DefaultFontSizeProperty.Changed.AddClassHandler<TimelineControl>(AppearanceChanged);
        InvalidateFlagProperty.Changed.AddClassHandler<TimelineControl>((s, e) => s.InvalidateFlagChanged());

        SliderProperty.Changed.AddClassHandler<TimelineControl>(SliderChanged);
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

    public static readonly StyledProperty<Cursor> PanCursorProperty =
        AvaloniaProperty.Register<TimelineControl, Cursor>(nameof(PanCursor), new Cursor(StandardCursorType.Hand));

    public Cursor PanCursor
    {
        get
        {
            return GetValue(PanCursorProperty);
        }

        set
        {
            SetValue(PanCursorProperty, value);
        }
    }

    public static readonly StyledProperty<Cursor> PanHorizontalCursorProperty =
        AvaloniaProperty.Register<TimelineControl, Cursor>(nameof(PanHorizontalCursor), new Cursor(StandardCursorType.SizeWestEast));

    public Cursor PanHorizontalCursor
    {
        get
        {
            return GetValue(PanHorizontalCursorProperty);
        }

        set
        {
            SetValue(PanHorizontalCursorProperty, value);
        }
    }

    public static readonly StyledProperty<Cursor> ZoomHorizontalCursorProperty =
        AvaloniaProperty.Register<TimelineControl, Cursor>(nameof(ZoomHorizontalCursor), new Cursor(StandardCursorType.SizeWestEast));

    public Cursor ZoomHorizontalCursor
    {
        get
        {
            return GetValue(ZoomHorizontalCursorProperty);
        }

        set
        {
            SetValue(ZoomHorizontalCursorProperty, value);
        }
    }

    public static readonly StyledProperty<Cursor> ZoomRectangleCursorProperty =
        AvaloniaProperty.Register<TimelineControl, Cursor>(nameof(ZoomRectangleCursor), new Cursor(StandardCursorType.SizeAll));

    public Cursor ZoomRectangleCursor
    {
        get
        {
            return GetValue(ZoomRectangleCursorProperty);
        }

        set
        {
            SetValue(ZoomRectangleCursorProperty, value);
        }
    }

    public static readonly StyledProperty<Cursor> ZoomVerticalCursorProperty =
        AvaloniaProperty.Register<TimelineControl, Cursor>(nameof(ZoomVerticalCursor), new Cursor(StandardCursorType.SizeNorthSouth));

    public Cursor ZoomVerticalCursor
    {
        get
        {
            return GetValue(ZoomVerticalCursorProperty);
        }

        set
        {
            SetValue(ZoomVerticalCursorProperty, value);
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

    public static readonly StyledProperty<CultureInfo> CultureProperty =
        AvaloniaProperty.Register<TimelineControl, CultureInfo>(nameof(Culture));

    public CultureInfo Culture
    {
        get
        {
            return GetValue(CultureProperty);
        }

        set
        {
            SetValue(CultureProperty, value);
        }
    }

    public static readonly StyledProperty<string> DefaultFontProperty =
        AvaloniaProperty.Register<TimelineControl, string>(nameof(DefaultFont), "Segoe UI");

    public string DefaultFont
    {
        get
        {
            return GetValue(DefaultFontProperty);
        }

        set
        {
            SetValue(DefaultFontProperty, value);
        }
    }

    public static readonly StyledProperty<double> DefaultFontSizeProperty =
        AvaloniaProperty.Register<TimelineControl, double>(nameof(DefaultFontSize), 12d);

    public double DefaultFontSize
    {
        get
        {
            return GetValue(DefaultFontSizeProperty);
        }

        set
        {
            SetValue(DefaultFontSizeProperty, value);
        }
    }

    public static readonly StyledProperty<int> InvalidateFlagProperty =
        AvaloniaProperty.Register<TimelineControl, int>(nameof(InvalidateFlag), 0);

    // Gets or sets the refresh flag (an integer value). When the flag is changed, the Plot will be refreshed.
    public int InvalidateFlag
    {
        get
        {
            return GetValue(InvalidateFlagProperty);
        }

        set
        {
            SetValue(InvalidateFlagProperty, value);
        }
    }

    // Invalidates the Plot control/view when the <see cref="InvalidateFlag" /> property is changed.
    private void InvalidateFlagChanged()
    {
        InvalidatePlot();
    }
}
