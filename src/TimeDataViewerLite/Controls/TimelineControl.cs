using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Controls;

public partial class TimelineControl : TemplatedControl, IPlotView
{
    private const string PART_BasePanel = "PART_BasePanel";
    private const string PART_AxisXPanel = "PART_AxisXPanel";
    private const string PART_BackCanvas = "PART_BackCanvas";
    private const string PART_DrawCanvas = "PART_DrawCanvas";
    private const string PART_FrontCanvas = "PART_FrontCanvas";
    private const string PART_AxisXCanvas = "PART_AxisXCanvas";
    private const string PART_ZoomControl = "PART_ZoomControl";
    private const string PART_Tracker = "PART_Tracker";
    private Panel? _basePanel;
    private Panel? _axisXPanel;
    private ContentControl? _zoomControl;
    private TrackerControl? _tracker;
    // Invalidation flag (0: no update, 1: update visual elements).  
    private int _isPlotInvalidated;
    private PlotModel? _plotModel;

    public TimelineControl()
    {
        this.GetObservable(TransformedBoundsProperty).Subscribe(bounds => OnSizeChanged(this, bounds?.Bounds.Size ?? new Size()));
    }

    public PlotModel? ActualModel => _plotModel;

    //public IController? ActualController { get; }

    public OxyRect ClientArea => new(0, 0, Bounds.Width, Bounds.Height);

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is PlotModel plotModel)
        {
            _plotModel = plotModel;

            ((IPlotModel)_plotModel).AttachPlotView(this);

            InvalidatePlot();
        }
    }

    public void HideZoomRectangle()
    {
        if (_zoomControl != null)
        {
            _zoomControl.IsVisible = false;
        }
    }

    // Invalidate the PlotView (not blocking the UI thread)
    public void InvalidatePlot()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _isPlotInvalidated, 1, 0) == 0)
        {
            // Invalidate the arrange state for the element.
            // After the invalidation, the element will have its layout updated,
            // which will occur asynchronously unless subsequently forced by UpdateLayout.
            BeginInvoke(InvalidateArrange);
            BeginInvoke(InvalidateVisual);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _basePanel = e.NameScope.Find<Panel>(PART_BasePanel);
        _axisXPanel = e.NameScope.Find<Panel>(PART_AxisXPanel);

        if (_basePanel == null || _axisXPanel == null)
        {
            return;
        }

        _basePanel.PointerWheelChanged += _panel_PointerWheelChanged;
        _basePanel.PointerPressed += _panel_PointerPressed;
        _basePanel.PointerMoved += _panel_PointerMoved;
        _basePanel.PointerReleased += _panel_PointerReleased;

        _backCanvas = e.NameScope.Find<Canvas>(PART_BackCanvas);
        _drawCanvas = e.NameScope.Find<DrawCanvas>(PART_DrawCanvas);
        _frontCanvas = e.NameScope.Find<Canvas>(PART_FrontCanvas);
        _axisXCanvas = e.NameScope.Find<Canvas>(PART_AxisXCanvas);

        _zoomControl = e.NameScope.Find<ContentControl>(PART_ZoomControl);

        _tracker = e.NameScope.Find<TrackerControl>(PART_Tracker);
    }

    public void SetCursorType(CursorType cursorType)
    {
        Cursor = cursorType switch
        {
            CursorType.Pan => new Cursor(StandardCursorType.Hand),
            CursorType.PanHorizontal => new Cursor(StandardCursorType.SizeWestEast),
            CursorType.ZoomRectangle => new Cursor(StandardCursorType.SizeAll),
            CursorType.ZoomHorizontal => new Cursor(StandardCursorType.SizeWestEast),
            CursorType.ZoomVertical => new Cursor(StandardCursorType.SizeNorthSouth),
            _ => Cursor.Default,
        };
    }

    public void ShowTracker(TrackerHitResult? trackerHitResult) => _tracker?.Show(trackerHitResult);

    public void HideTracker() => _tracker?.Hide();

    public void ShowZoomRectangle(OxyRect r)
    {
        if (_zoomControl != null)
        {
            _zoomControl.Width = r.Width;
            _zoomControl.Height = r.Height;
            Canvas.SetLeft(_zoomControl, r.Left);
            Canvas.SetTop(_zoomControl, r.Top);
            _zoomControl.IsVisible = true;
        }
    }

    // Stores text on the clipboard.
    public async void SetClipboardText(string text)
    {
        await AvaloniaLocator.Current.GetService<IClipboard>()!.SetTextAsync(text);
    }

    // Provides the behavior for the Arrange pass of Silverlight layout.
    // Classes can override this method to define their own Arrange pass behavior.
    protected override Size ArrangeOverride(Size finalSize)
    {
        var actualSize = base.ArrangeOverride(finalSize);
        if (actualSize.Width > 0 && actualSize.Height > 0)
        {
            if (Interlocked.CompareExchange(ref _isPlotInvalidated, 0, 1) == 1)
            {
                Draw();
            }
        }

        return actualSize;
    }

    // Called when the size of the control is changed.
    private void OnSizeChanged(object _, Size size)
    {
        if (size.Height > 0 && size.Width > 0)
        {
            InvalidatePlot();
        }
    }

    // Gets the relevant parent.
    private Control? GetRelevantParent<T>(IVisual obj) where T : Control
    {
        var container = obj.VisualParent;

        if (container is ContentPresenter contentPresenter)
        {
            container = GetRelevantParent<T>(contentPresenter);
        }

        if (container is Panel panel)
        {
            container = GetRelevantParent<ScrollViewer>(panel);
        }

        if ((container is T) == false && (container != null))
        {
            container = GetRelevantParent<T>(container);
        }

        return (Control?)container;
    }

    // Invokes the specified action on the dispatcher, if necessary.
    private static void BeginInvoke(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Loaded);
        }
        else
        {
            action?.Invoke();
        }
    }
}
