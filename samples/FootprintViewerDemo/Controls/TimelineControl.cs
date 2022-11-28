using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TimeDataViewer;
using TimeDataViewer.Core;
using TimeDataViewer.Spatial;

namespace FootprintViewerDemo.Controls;

public partial class TimelineControl : TemplatedControl, IPlotView
{
    private const string PART_BasePanel = "PART_BasePanel";
    private const string PART_AxisXPanel = "PART_AxisXPanel";
    private const string PART_BackCanvas = "PART_BackCanvas";
    private const string PART_DrawCanvas = "PART_DrawCanvas";
    private const string PART_FrontCanvas = "PART_FrontCanvas";
    private const string PART_AxisXCanvas = "PART_AxisXCanvas";
    private const string PART_OverlayCanvas = "PART_OverlayCanvas";
    private const string PART_ZoomControl = "PART_ZoomControl";
    private const string PART_TimelineSlider = "PART_TimelineSlider";
    private Panel? _basePanel;
    private Panel? _axisXPanel;
    private Canvas? _backCanvas;
    private DrawCanvas? _drawCanvas;
    private Canvas? _frontCanvas;
    private Canvas? _axisXCanvas;
    private Canvas? _ovarlayCanvas;
    private ContentControl? _zoomControl;
    private TimeDataViewer.Slider? _slider;
    // Invalidation flag (0: no update, 1: update visual elements).  
    private int _isPlotInvalidated;
    private readonly ObservableCollection<TrackerDefinition> _trackerDefinitions;
    private IControl? _currentTracker;
    private PlotModel? _plotModel;
    private readonly IPlotController _defaultController;
    private readonly Pen _minorPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _majorPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _minorTickPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _majorTickPen = new() { Brush = Brush.Parse("#2A2A2A") };

    public TimelineControl()
    {
        DisconnectCanvasWhileUpdating = false;// true;
        _trackerDefinitions = new ObservableCollection<TrackerDefinition>();
        this.GetObservable(TransformedBoundsProperty).Subscribe(bounds => OnSizeChanged(this, bounds?.Bounds.Size ?? new Size()));

        _defaultController = new PlotController();
    }

    // Gets or sets a value indicating whether to disconnect the canvas while updating.
    public bool DisconnectCanvasWhileUpdating { get; set; }

    public PlotModel? ActualModel => _plotModel;

    public IController? ActualController => _defaultController;

    // Gets the coordinates of the client area of the view.
    public OxyRect ClientArea => new(0, 0, Bounds.Width, Bounds.Height);

    public ObservableCollection<TrackerDefinition> TrackerDefinitions => _trackerDefinitions;

    public void HideTracker()
    {
        if (_currentTracker != null)
        {
            _ovarlayCanvas?.Children.Remove(_currentTracker);
            _currentTracker = null;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is PlotModel plotModel)
        {
            _plotModel = plotModel;

            ((IPlotModel)_plotModel).AttachPlotView(this);

            InvalidatePlot(false);
            InvalidatePlot(true);
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
    public void InvalidatePlot(bool updateData = true)
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        UpdateModel(updateData);

        if (Interlocked.CompareExchange(ref _isPlotInvalidated, 1, 0) == 0)
        {
            // Invalidate the arrange state for the element.
            // After the invalidation, the element will have its layout updated,
            // which will occur asynchronously unless subsequently forced by UpdateLayout.
            BeginInvoke(InvalidateArrange);
            BeginInvoke(InvalidateVisual);
        }
    }

    // When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass)
    // call <see cref="M:System.Windows.Controls.Control.ApplyTemplate" /> . In simplest terms, this means the method is called 
    // just before a UI element displays in an application. For more information, see Remarks.
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _basePanel = e.NameScope.Find<Panel>(PART_BasePanel);
        _axisXPanel = e.NameScope.Find<Panel>(PART_AxisXPanel);

        if (_basePanel == null || _axisXPanel == null)
        {
            return;
        }

        _basePanel.PointerEnter += _panel_PointerEnter;
        _basePanel.PointerLeave += _panel_PointerLeave;
        _basePanel.PointerWheelChanged += _panel_PointerWheelChanged;
        _basePanel.PointerPressed += _panel_PointerPressed;
        _basePanel.PointerMoved += _panel_PointerMoved;
        _basePanel.PointerReleased += _panel_PointerReleased;

        _axisXPanel.PointerPressed += _panelX_PointerPressed;
        _axisXPanel.PointerMoved += _panelX_PointerMoved;
        _axisXPanel.PointerReleased += _panelX_PointerReleased;

        _backCanvas = e.NameScope.Find<Canvas>(PART_BackCanvas);
        _drawCanvas = e.NameScope.Find<DrawCanvas>(PART_DrawCanvas);
        _frontCanvas = e.NameScope.Find<Canvas>(PART_FrontCanvas);

        _axisXCanvas = e.NameScope.Find<Canvas>(PART_AxisXCanvas);
        _ovarlayCanvas = e.NameScope.Find<Canvas>(PART_OverlayCanvas);

        _zoomControl = e.NameScope.Find<ContentControl>(PART_ZoomControl);

        _slider = e.NameScope.Find<TimeDataViewer.Slider>(PART_TimelineSlider);
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

    public void ShowTracker(TrackerHitResult trackerHitResult)
    {
        if (trackerHitResult == null)
        {
            HideTracker();
            return;
        }

        var trackerTemplate = DefaultTrackerTemplate;
        if (trackerHitResult.Series != null && !string.IsNullOrEmpty(trackerHitResult.Series.TrackerKey))
        {
            var match = TrackerDefinitions.FirstOrDefault(t => t.TrackerKey == trackerHitResult.Series.TrackerKey);
            if (match != null)
            {
                trackerTemplate = match.TrackerTemplate;
            }
        }

        if (trackerTemplate == null)
        {
            HideTracker();
            return;
        }

        var tracker = trackerTemplate.Build(new ContentControl());

        // ReSharper disable once RedundantNameQualifier
        if (!object.ReferenceEquals(tracker, _currentTracker))
        {
            HideTracker();
            _ovarlayCanvas?.Children.Add(tracker.Control);
            _currentTracker = tracker.Control;
        }

        if (_currentTracker != null)
        {
            _currentTracker.DataContext = trackerHitResult;
        }
    }

    public void ShowZoomRectangle(OxyRect r)
    {
        if (_zoomControl != null)
        {
            _zoomControl.Width = r.Width;
            _zoomControl.Height = r.Height;
            Canvas.SetLeft(_zoomControl, r.Left);
            Canvas.SetTop(_zoomControl, r.Top);
            _zoomControl.Template = ZoomRectangleTemplate;
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
                UpdateVisuals();
            }
        }

        return actualSize;
    }

    // Updates the model. If Model==<c>null</c>, an internal model will be created.
    // The ActualModel.Update will be called (updates all series data).
    protected void UpdateModel(bool updateData = true)
    {
        if (ActualModel != null)
        {
            ((IPlotModel)ActualModel).Update(updateData);
        }

        UpdateSlider();
    }

    // Determines whether the plot is currently visible to the user.
    protected bool IsVisibleToUser()
    {
        return IsUserVisible(this);
    }

    // Determines whether the specified element is currently visible to the user.
    private static bool IsUserVisible(Control element)
    {
        return element.IsEffectivelyVisible && element.TransformedBounds.HasValue;
    }

    // Called when the size of the control is changed.
    private void OnSizeChanged(object _, Size size)
    {
        if (size.Height > 0 && size.Width > 0)
        {
            InvalidatePlot(false);
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

    private void UpdateVisuals()
    {
        if (_backCanvas == null || _frontCanvas == null || _basePanel == null || _axisXCanvas == null || _drawCanvas == null)
        {
            return;
        }

        if (IsVisibleToUser() == false)
        {
            return;
        }

        // Clear the canvas
        _backCanvas.Children.Clear();
        _axisXCanvas.Children.Clear();
        _frontCanvas.Children.Clear();

        if (ActualModel != null)
        {
            var rcAxis = new CanvasRenderContext(_axisXCanvas);
            var rcPlotBack = new CanvasRenderContext(_backCanvas);
            var rcPlotFront = new CanvasRenderContext(_frontCanvas);

            if (DisconnectCanvasWhileUpdating)
            {
                // TODO: profile... not sure if this makes any difference
                var idx0 = _basePanel.Children.IndexOf(_backCanvas);
                if (idx0 != -1)
                {
                    _basePanel.Children.RemoveAt(idx0);
                }

                var idx1 = _basePanel.Children.IndexOf(_frontCanvas);
                if (idx1 != -1)
                {
                    _basePanel.Children.RemoveAt(idx1);
                }


                ((IPlotModel)ActualModel).Render(_backCanvas.Bounds.Width, _backCanvas.Bounds.Height);
                RenderSeries(_backCanvas, _drawCanvas);
                RenderAxisX(rcAxis, rcPlotBack);
                RenderSlider(rcAxis, rcPlotFront);

                // reinsert the canvas again
                if (idx1 != -1)
                {
                    _basePanel.Children.Insert(idx1, _frontCanvas);
                }

                if (idx0 != -1)
                {
                    _basePanel.Children.Insert(idx0, _backCanvas);
                }
            }
            else
            {
                ((IPlotModel)ActualModel).Render(_backCanvas.Bounds.Width, _backCanvas.Bounds.Height);
                RenderSeries(_backCanvas, _drawCanvas);
                RenderAxisX(rcAxis, rcPlotBack);
                RenderSlider(rcAxis, rcPlotFront);
            }
        }
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

    protected void RenderAxisX(CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        if (_plotModel != null)
        {
            foreach (var item in _plotModel.Axises)
            {
                if (item.IsHorizontal() == true)
                {
                    RenderAxis(item, contextAxis, contextPlot);
                }
            }
        }
    }

    private void RenderAxis(TimeDataViewer.Core.Axis? internalAxis, CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        if (internalAxis == null)
        {
            return;
        }

        var labels = internalAxis.MyLabels;
        var minorSegments = internalAxis.MyMinorSegments;
        var minorTickSegments = internalAxis.MyMinorTickSegments;
        var majorSegments = internalAxis.MyMajorSegments;
        var majorTickSegments = internalAxis.MyMajorTickSegments;

        if (_minorPen != null)
        {
            contextPlot.DrawLineSegments(minorSegments, _minorPen);
        }

        if (_minorTickPen != null)
        {
            contextAxis.DrawLineSegments(minorTickSegments, _minorTickPen);
        }

        foreach (var (pt, text, ha, va) in labels)
        {
            var label = DefaultLabelTemplate.Build(new ContentControl());

            if (label.Control is TextBlock textBlock)
            {
                textBlock.Text = text;
                textBlock.HorizontalAlignment = ha.ToAvalonia();
                textBlock.VerticalAlignment = va.ToAvalonia();
                contextAxis.DrawMathText(pt, textBlock);
            }
        }

        if (_majorPen != null)
        {
            contextPlot.DrawLineSegments(majorSegments, _majorPen);
        }

        if (_majorTickPen != null)
        {
            contextAxis.DrawLineSegments(majorTickSegments, _majorTickPen);
        }

        //if(MinMaxBrush != null && InternalAxis.IsHorizontal() == true)
        //{
        //    var rect0 = InternalAxis.LeftRect;
        //    var rect1 = InternalAxis.RightRect;

        //    if (rect0.Width != 0)
        //    {
        //      //  contextPlot.DrawRectangle(rect0, MinMaxBrush, null);
        //      //  contextPlot.DrawLine(rect0.Right, rect0.Top, rect0.Right, rect0.Bottom, BlackPen);
        //    }

        //    if (rect1.Width != 0)
        //    {
        //      //  contextPlot.DrawRectangle(rect1, MinMaxBrush, null);
        //      //  contextPlot.DrawLine(rect1.Left, rect1.Top, rect1.Left, rect1.Bottom, BlackPen);
        //    }
        //}
    }

    protected void RenderSeries(Canvas _, DrawCanvas drawCanvas)
    {
        if (_plotModel != null)
        {
            drawCanvas.RenderSeries(_plotModel.Series.Where(s => s.IsVisible).ToList());
        }
    }

    public void SliderTo(double value)
    {
        if (_plotModel != null)
        {
            foreach (var a in _plotModel.Axises)
            {
                if (a.IsHorizontal() == true && _slider != null)
                {
                    DateTime TimeOrigin = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
                    _slider.IsTracking = false;
                    _slider.CurrentValue = TimeOrigin.AddDays(value - 1);
                    _slider.IsTracking = true;
                }
            }
        }
    }

    protected void RenderSlider(CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        // TODO: Remove update method from render and replace to UpdateModel (present correct not work) 
        UpdateSlider();
        _slider?.Render(contextAxis, contextPlot);
    }

    private void UpdateSlider()
    {
        if (ActualModel != null)
        {
            _slider?.UpdateMinMax(ActualModel);
        }
    }
}
