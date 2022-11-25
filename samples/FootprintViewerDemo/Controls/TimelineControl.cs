using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FootprintViewerDemo.Models;
using FootprintViewerDemo.ViewModels;
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

    private Panel? _basePanel;
    private Panel? _axisXPanel;
    private Canvas? _backCanvas;
    private DrawCanvas? _drawCanvas;
    private Canvas? _frontCanvas;
    private Canvas? _axisXCanvas;
    private Canvas? _ovarlayCanvas;
    private ContentControl? _zoomControl;

   // private Canvas _canvasBack;
  //  private Canvas _canvasFront;
   // private Canvas _canvasX;
   // private DrawCanvas _drawCanvas;
  //  private Panel? _panel;
  //  protected Panel? _panelX;
    // Invalidation flag (0: no update, 1: update visual elements).  
    private int _isPlotInvalidated;
  //  private Canvas _overlays;
  //  private ContentControl _zoomControl;
    private readonly ObservableCollection<TrackerDefinition> _trackerDefinitions;
    private IControl? _currentTracker;

    private readonly PlotModel _internalModel;
    private readonly IPlotController _defaultController;

    private readonly ObservableCollection<TimeDataViewer.Axis> _axises;
    private readonly ObservableCollection<TimeDataViewer.Series> _series;

    public TimelineControl()
    {
        DisconnectCanvasWhileUpdating = true;
        _trackerDefinitions = new ObservableCollection<TrackerDefinition>();
        this.GetObservable(TransformedBoundsProperty).Subscribe(bounds => OnSizeChanged(this, bounds?.Bounds.Size ?? new Size()));

        _series = new ObservableCollection<TimeDataViewer.Series>();
        _axises = new ObservableCollection<TimeDataViewer.Axis>();

        _series.CollectionChanged += OnSeriesChanged;
        _axises.CollectionChanged += OnAxesChanged;

        _defaultController = new PlotController();
        _internalModel = new PlotModel();
        ((IPlotModel)_internalModel).AttachPlotView(this);

        InitData();
    }

    public Collection<TimeDataViewer.Axis> Axises => _axises;

    [Content]
    public Collection<TimeDataViewer.Series> Series => _series;

    private void InitData()
    {
        var labels = new ObservableCollection<ItemViewModel>()
            {
                new() { Label = "Satellite1" },
                new() { Label = "Satellite2" },
                new() { Label = "Satellite3" },
                new() { Label = "Satellite4" },
                new() { Label = "Satellite5" }
            };

        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var path = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "Footprints.json"));

        var serializer = new FootprintSerializer(path);
        var res = Task.Run(async () => await serializer.GetValuesAsync()).Result;
        var footprints = res.Cast<Footprint>().ToList();

        var min = footprints.Select(s => s.Begin).Min();
        var max = footprints.Select(s => s.Begin).Max();

        var satellites = footprints.Select(s => s.SatelliteName!).Distinct().ToList() ?? new List<string>();

        var epoch = min.Date;

        var beginScenario = 0.0;// Selected.BeginScenario;
        var endScenario = 2 * 86400.0;// Selected.EndScenario;

        var begin = 0.0; //Selected.Begin;
        var duration = 2 * 86400.0;//Selected.Duration;

        var axises = new List<TimeDataViewer.CategoryAxis>();

        _axises.Add(new TimeDataViewer.CategoryAxis()
        {
            Position = AxisPosition.Left,
            AbsoluteMinimum = -0.5,
            AbsoluteMaximum = 4.5,
            IsZoomEnabled = false,
            Items = labels,
            LabelField = "Label"
        });

        _axises.Add(new TimeDataViewer.DateTimeAxis()
        {
            Position = AxisPosition.Top,
            IntervalType = DateTimeIntervalType.Auto,
            AbsoluteMinimum = beginScenario,
            AbsoluteMaximum = endScenario,
            FirstDateTime = epoch,
        });

        var items1 = footprints.Where(s => Equals(s.SatelliteName, "Satellite1")).Select(s => CreateInterval(s, epoch)).ToList();
        var items2 = footprints.Where(s => Equals(s.SatelliteName, "Satellite2")).Select(s => CreateInterval(s, epoch)).ToList();
        var items3 = footprints.Where(s => Equals(s.SatelliteName, "Satellite3")).Select(s => CreateInterval(s, epoch)).ToList();
        var items4 = footprints.Where(s => Equals(s.SatelliteName, "Satellite4")).Select(s => CreateInterval(s, epoch)).ToList();
        var items5 = footprints.Where(s => Equals(s.SatelliteName, "Satellite5")).Select(s => CreateInterval(s, epoch)).ToList();

        _series.Add(new TimeDataViewer.TimelineSeries()
        {
            BarWidth = 0.5,
            FillBrush = Brushes.LightCoral,
            Items = items1,
            CategoryField = "Category",
            BeginField = "BeginTime",
            EndField = "EndTime"
        });

        _series.Add(new TimeDataViewer.TimelineSeries()
        {
            BarWidth = 0.5,
            FillBrush = Brushes.Green,
            Items = items2,
            CategoryField = "Category",
            BeginField = "BeginTime",
            EndField = "EndTime"
        });

        _series.Add(new TimeDataViewer.TimelineSeries()
        {
            BarWidth = 0.5,
            FillBrush = Brushes.Blue,
            Items = items3,
            CategoryField = "Category",
            BeginField = "BeginTime",
            EndField = "EndTime"
        });

        _series.Add(new TimeDataViewer.TimelineSeries()
        {
            BarWidth = 0.5,
            FillBrush = Brushes.Red,
            Items = items4,
            CategoryField = "Category",
            BeginField = "BeginTime",
            EndField = "EndTime"
        });

        //_series.Add(new TimeDataViewer.TimelineSeries()
        //{
        //    BarWidth = 0.5,
        //    FillBrush = Brushes.Yellow,
        //    Items = items5,
        //    CategoryField = "Category",
        //    BeginField = "BeginTime",
        //    EndField = "EndTime"
        //});

        Slider = new TimeDataViewer.Slider() 
        {        
            Begin = begin,        
            Duration = duration
        };
    }

    private Interval CreateInterval(Footprint footprint, DateTime epoch)
    {
        var secs = footprint.Begin.TimeOfDay.TotalSeconds;

        var date = epoch.Date;

        return new Interval()
        {
            Category = footprint.SatelliteName,
            BeginTime = date.AddSeconds(secs),
            EndTime = date.AddSeconds(secs + footprint.Duration)
        };
    }

    // Gets or sets a value indicating whether to disconnect the canvas while updating.
    public bool DisconnectCanvasWhileUpdating { get; set; }

    // Gets the actual model in the view.
    Model IView.ActualModel => ActualModel;

    public PlotModel ActualModel => _internalModel;

    public IPlotController ActualController => _defaultController;

    IController IView.ActualController => ActualController;

    // Gets the coordinates of the client area of the view.
    public OxyRect ClientArea => new OxyRect(0, 0, Bounds.Width, Bounds.Height);

    public ObservableCollection<TrackerDefinition> TrackerDefinitions => _trackerDefinitions;

    public void HideTracker()
    {
        if (_currentTracker != null)
        {
            _ovarlayCanvas?.Children.Remove(_currentTracker);
            _currentTracker = null;
        }
    }

    public void HideZoomRectangle()
    {
        _zoomControl.IsVisible = false;
    }

    //public void PanAllAxes(Vector delta)
    //{
    //    if (ActualModel != null)
    //    {
    //        ActualModel.PanAllAxes(delta.X, delta.Y);
    //    }

    //    InvalidatePlot(false);
    //}

    //public void ZoomAllAxes(double factor)
    //{
    //    if (ActualModel != null)
    //    {
    //        ActualModel.ZoomAllAxes(factor);
    //    }

    //    InvalidatePlot(false);
    //}

    //public void ResetAllAxes()
    //{
    //    if (ActualModel != null)
    //    {
    //        ActualModel.ResetAllAxes();
    //    }

    //    InvalidatePlot(false);
    //}

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
    }

    public void SetCursorType(CursorType cursorType)
    {
        switch (cursorType)
        {
            case CursorType.Pan:
                Cursor = PanCursor;
                break;
            case CursorType.PanHorizontal:
                Cursor = PanHorizontalCursor;
                break;
            case CursorType.ZoomRectangle:
                Cursor = ZoomRectangleCursor;
                break;
            case CursorType.ZoomHorizontal:
                Cursor = ZoomHorizontalCursor;
                break;
            case CursorType.ZoomVertical:
                Cursor = ZoomVerticalCursor;
                break;
            default:
                Cursor = Cursor.Default;
                break;
        }
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
            _ovarlayCanvas.Children.Add(tracker.Control);
            _currentTracker = tracker.Control;
        }

        if (_currentTracker != null)
        {
            _currentTracker.DataContext = trackerHitResult;
        }
    }

    public void ShowZoomRectangle(OxyRect r)
    {
        _zoomControl.Width = r.Width;
        _zoomControl.Height = r.Height;
        Canvas.SetLeft(_zoomControl, r.Left);
        Canvas.SetTop(_zoomControl, r.Top);
        _zoomControl.Template = ZoomRectangleTemplate;
        _zoomControl.IsVisible = true;
    }

    // Stores text on the clipboard.
    public async void SetClipboardText(string text)
    {
        await AvaloniaLocator.Current.GetService<IClipboard>().SetTextAsync(text);
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
        SynchronizeProperties();
        SynchronizeSeries();
        SynchronizeAxes();

        if (ActualModel != null)
        {
            ((IPlotModel)ActualModel).Update(updateData);
        }

        //UpdateSlider();
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
    private void OnSizeChanged(object sender, Size size)
    {
        if (size.Height > 0 && size.Width > 0)
        {
            InvalidatePlot(false);
        }
    }

    // Gets the relevant parent.
    private Control GetRelevantParent<T>(IVisual obj) where T : Control
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

        return (Control)container;
    }

    private void UpdateVisuals()
    {
        if (_backCanvas == null || _frontCanvas == null || _basePanel == null)
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
        foreach (var item in Axises)
        {
            if (item.InternalAxis.IsHorizontal() == true)
            {
                item.Render(contextAxis, contextPlot);
            }
        }
    }

    protected void RenderSeries(Canvas canvasPlot, DrawCanvas drawCanvas)
    {
        drawCanvas.RenderSeries(Series.Where(s => s.IsVisible).ToList());
    }

    private bool _isPressed = false;

    private void _panelX_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPressed = false;
    }

    private void _panelX_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressed == true)
        {
            base.OnPointerMoved(e);
            if (e.Handled)
            {
                return;
            }

            e.Pointer.Capture(_axisXPanel);

            var point = e.GetPosition(_axisXPanel).ToScreenPoint();

            foreach (var a in Axises)
            {
                if (a.InternalAxis.IsHorizontal() == true && a is TimeDataViewer.DateTimeAxis axis)
                {
                    var value = axis.InternalAxis.InverseTransform(point.X);

                    DateTime TimeOrigin = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
                    Slider.IsTracking = false;
                    Slider.CurrentValue = TimeOrigin.AddDays(value - 1);
                    Slider.IsTracking = true;
                }
            }
        }
    }

    public void SliderTo(double value)
    {
        foreach (var a in Axises)
        {
            if (a.InternalAxis.IsHorizontal() == true)
            {
                DateTime TimeOrigin = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
                Slider.IsTracking = false;
                Slider.CurrentValue = TimeOrigin.AddDays(value - 1);
                Slider.IsTracking = true;
            }
        }
    }

    private void _panelX_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled)
        {
            return;
        }

        Focus();
        e.Pointer.Capture(_axisXPanel);

        var point = e.GetPosition(_axisXPanel).ToScreenPoint();

        foreach (var a in Axises)
        {
            if (a.InternalAxis.IsHorizontal() == true && a is TimeDataViewer.DateTimeAxis axis)
            {
                var value = axis.InternalAxis.InverseTransform(point.X);

                DateTime TimeOrigin = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
                Slider.IsTracking = false;
                Slider.CurrentValue = TimeOrigin.AddDays(value - 1);
                Slider.IsTracking = true;

                _isPressed = true;
            }
        }
    }

    protected void RenderSlider(CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        // TODO: Remove update method from render and replace to UpdateModel (present correct not work) 
        UpdateSlider();
        Slider.Render(contextAxis, contextPlot);
    }

    // Called when the visual appearance is changed.     
    protected void OnAppearanceChanged()
    {
        InvalidatePlot(false);
    }

    // Called when the visual appearance is changed.
    private static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        ((TimelineControl)d).OnAppearanceChanged();
    }

    private static void SliderChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        ((TimelineControl)d).SyncLogicalTree(e);
    }

    private void OnAxesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncLogicalTree(e);
    }

    private void OnSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncLogicalTree(e);
    }

    private void SyncLogicalTree(NotifyCollectionChangedEventArgs e)
    {
        // In order to get DataContext and binding to work with the series, axes and annotations
        // we add the items to the logical tree
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems.OfType<ISetLogicalParent>())
            {
                item.SetParent(this);
            }
            LogicalChildren.AddRange(e.NewItems.OfType<ILogical>());
            VisualChildren.AddRange(e.NewItems.OfType<IVisual>());
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems.OfType<ISetLogicalParent>())
            {
                item.SetParent(null);
            }
            foreach (var item in e.OldItems)
            {
                LogicalChildren.Remove((ILogical)item);
                VisualChildren.Remove((IVisual)item);
            }
        }
    }

    private void SyncLogicalTree(AvaloniaPropertyChangedEventArgs e)
    {
        // In order to get DataContext and binding to work with the series, axes and annotations
        // we add the items to the logical tree
        if (e.NewValue != null)
        {
            ((ISetLogicalParent)e.NewValue).SetParent(this);

            LogicalChildren.Add((ILogical)e.NewValue);
            VisualChildren.Add((IVisual)e.NewValue);
        }

        if (e.OldValue != null)
        {
            ((ISetLogicalParent)e.OldValue).SetParent(null);

            LogicalChildren.Remove((ILogical)e.OldValue);
            VisualChildren.Remove((IVisual)e.OldValue);
        }
    }

    // Synchronize properties in the internal Plot model
    private void SynchronizeProperties()
    {
        var m = _internalModel;

        m.PlotMarginLeft = PlotMargins.Left;
        m.PlotMarginTop = PlotMargins.Top;
        m.PlotMarginRight = PlotMargins.Right;
        m.PlotMarginBottom = PlotMargins.Bottom;

        //     m.Padding = Padding.ToOxyThickness();

        //  m.DefaultColors = DefaultColors.Select(c => c.ToOxyColor()).ToArray();

        //   m.AxisTierDistance = AxisTierDistance;
    }

    // Synchronizes the axes in the internal model.      
    private void SynchronizeAxes()
    {
        _internalModel.Axises.Clear();
        foreach (var a in Axises)
        {
            _internalModel.Axises.Add(a.CreateModel());
        }
    }

    // Synchronizes the series in the internal model.       
    private void SynchronizeSeries()
    {
        _internalModel.Series.Clear();
        foreach (var s in Series)
        {
            _internalModel.Series.Add(s.CreateModel());
        }
    }

    private void UpdateSlider()
    {
        Slider?.UpdateMinMax(ActualModel);
    }
}
