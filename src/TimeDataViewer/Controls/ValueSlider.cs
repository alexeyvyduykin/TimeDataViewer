using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using TimeDataViewer.Extensions;
using TimeDataViewer.RenderContext;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Controls;

public class ValueSlider : Control
{
    public static readonly StyledProperty<DateTime> CurrentValueProperty =
        AvaloniaProperty.Register<ValueSlider, DateTime>(nameof(CurrentValue));
    public static readonly StyledProperty<double> BeginProperty =
        AvaloniaProperty.Register<ValueSlider, double>(nameof(Begin), 0.0);
    public static readonly StyledProperty<double> DurationProperty =
        AvaloniaProperty.Register<ValueSlider, double>(nameof(Duration), 0.0);
    public static readonly StyledProperty<IBrush> InactiveRangeBrushProperty =
        AvaloniaProperty.Register<ValueSlider, IBrush>(nameof(InactiveRangeBrush), new SolidColorBrush());
    public static readonly StyledProperty<IBrush> SliderBrushProperty =
        AvaloniaProperty.Register<ValueSlider, IBrush>(nameof(SliderBrush), new SolidColorBrush());
    public static readonly StyledProperty<ControlTemplate> DefaultLabelTemplateProperty =
        AvaloniaProperty.Register<ValueSlider, ControlTemplate>(nameof(DefaultLabelTemplate));
    public static readonly StyledProperty<bool> IsTrackingProperty =
        AvaloniaProperty.Register<ValueSlider, bool>(nameof(IsTracking), true);

    private OxyRect _leftRect;
    private OxyRect _rightRect;
    private (Point p0, Point p1) _plotSlider;
    private (Point p0, Point p1) _axisSlider;
    private string? _label;
    private ScreenPoint _labelPoint;
    private static DateTime TimeOrigin { get; } = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    private Core.Axis? _axisX;

    static ValueSlider()
    {
        ClipToBoundsProperty.OverrideDefaultValue<ValueSlider>(false);
        BeginProperty.Changed.AddClassHandler<ValueSlider>(AppearanceChanged);
        DurationProperty.Changed.AddClassHandler<ValueSlider>(AppearanceChanged);
        CurrentValueProperty.Changed.AddClassHandler<ValueSlider>(CurrentValueChanged);
        InactiveRangeBrushProperty.Changed.AddClassHandler<ValueSlider>(AppearanceChanged);
        SliderBrushProperty.Changed.AddClassHandler<ValueSlider>(AppearanceChanged);
    }

    public bool IsTracking
    {
        get
        {
            return GetValue(IsTrackingProperty);
        }

        set
        {
            SetValue(IsTrackingProperty, value);
        }
    }

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

    public IBrush InactiveRangeBrush
    {
        get
        {
            return GetValue(InactiveRangeBrushProperty);
        }

        set
        {
            SetValue(InactiveRangeBrushProperty, value);
        }
    }

    public IBrush SliderBrush
    {
        get
        {
            return GetValue(SliderBrushProperty);
        }

        set
        {
            SetValue(SliderBrushProperty, value);
        }
    }

    private Pen BlackPen { get; set; } = new Pen() { Brush = Brushes.Black, Thickness = 1 };

    public DateTime CurrentValue
    {
        get
        {
            return GetValue(CurrentValueProperty);
        }

        set
        {
            SetValue(CurrentValueProperty, value);
        }
    }

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

    protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        ((ValueSlider)d).OnVisualChanged();
    }

    protected static void CurrentValueChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        if (((ValueSlider)d).IsTracking == true)
        {
            var axisX = ((ValueSlider)d)._axisX;

            if (axisX != null)
            {
                var newSliderValue = (DateTime)e.NewValue;

                var xc = axisX.ScreenMin.X + (axisX.ScreenMax.X - axisX.ScreenMin.X) / 2.0;

                var tnew = (newSliderValue - TimeOrigin).TotalDays + 1;
                var xnew = axisX.Transform(tnew);

                axisX.Pan(-xnew + xc);
            }
        }
        ((ValueSlider)d).OnVisualChanged();
    }

    protected void OnVisualChanged()
    {
        if (Parent is Core.IPlotView pc)
        {
            pc.InvalidatePlot(false);
        }
    }

    public void UpdateMinMax(Core.PlotModel plotModel)
    {
        _leftRect = new OxyRect();
        _rightRect = new OxyRect();

        if (plotModel == null)
        {
            return;
        }

        plotModel.GetAxesFromPoint(out var axisX, out _);

        _axisX = axisX;

        if (axisX == null)
        {
            return;
        }

        var plotHeight = plotModel.PlotArea.Height;
        // HACK: Axis height bound make as property
        double axisHeight = 30;//axisX.DesiredSize.Height;

        var min = double.MaxValue;
        var max = double.MinValue;

        foreach (Core.XYAxisSeries item in plotModel.Series)
        {
            if (item.IsVisible == true)
            {
                min = Math.Min(min, item.MinX);
                max = Math.Max(max, item.MaxX);
            }
        }

        var p0 = axisX.Transform(axisX.AbsoluteMinimum);
        var p1 = axisX.Transform(Begin);
        var p2 = axisX.Transform(Begin + Duration);
        var p3 = axisX.Transform(axisX.AbsoluteMaximum);

        var t0 = (CurrentValue - TimeOrigin).TotalDays + 1;//Begin + CurrentValue;
        var x0 = axisX.Transform(t0);
        _plotSlider = (new Point(x0, 0), new Point(x0, plotHeight));
        _axisSlider = (new Point(x0, 15), new Point(x0, axisHeight));

        _label = axisX.FormatValue(t0);
        _labelPoint = new ScreenPoint(x0, 15);

        var w0 = p1 - p0;
        var w1 = p3 - p2;

        if (w0 > 0)
        {
            _leftRect = new OxyRect(p0, 0, p1 - p0, plotHeight);
        }

        if (w1 > 0)
        {
            _rightRect = new OxyRect(p2, 0, p3 - p2, plotHeight);
        }
    }

    public virtual void Render(CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        if (InactiveRangeBrush != null)
        {
            if (_leftRect.Width != 0)
            {
                contextPlot.DrawRectangle(_leftRect, InactiveRangeBrush, null);
                contextPlot.DrawLine(_leftRect.Right, _leftRect.Top, _leftRect.Right, _leftRect.Bottom, BlackPen);
            }

            if (_rightRect.Width != 0)
            {
                contextPlot.DrawRectangle(_rightRect, InactiveRangeBrush, null);
                contextPlot.DrawLine(_rightRect.Left, _rightRect.Top, _rightRect.Left, _rightRect.Bottom, BlackPen);
            }
        }

        if (SliderBrush != null)
        {
            var sliderPen = new Pen() { Brush = SliderBrush, Thickness = 2 };

            contextPlot.DrawLine(_plotSlider.p0, _plotSlider.p1, sliderPen);

            var label = DefaultLabelTemplate.Build(new ContentControl());

            if (label.Control is TextBlock textBlock)
            {
                textBlock.Text = _label;
                contextAxis.DrawMathText(_labelPoint, textBlock);
            }

            contextAxis.DrawLine(_axisSlider.p0, _axisSlider.p1, sliderPen);
        }
    }
}
