using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using FootprintViewerDemo.Models;
using FootprintViewerDemo.ViewModels;
using TimeDataViewer;
using TimeDataViewer.Core;
using TimeDataViewer.Spatial;

namespace FootprintViewerDemo.Controls;

//public class TimelineControl : TemplatedControl, IPlotView
//{
//    private const string PART_BasePanel = "PART_BasePanel";
//    private const string PART_AxisXPanel = "PART_AxisXPanel";
//    private const string PART_BackCanvas = "PART_BackCanvas";
//    private const string PART_DrawCanvas = "PART_DrawCanvas";
//    private const string PART_FrontCanvas = "PART_FrontCanvas";
//    private const string PART_AxisXCanvas = "PART_AxisXCanvas";
//    private const string PART_OverlayCanvas = "PART_OverlayCanvas";
//    private const string PART_ZoomControl = "PART_ZoomControl";

//    private Panel? _basePanel;
//    private Panel? _axisXPanel;
//    private Canvas? _backCanvas;
//    private DrawCanvas? _drawCanvas;
//    private Canvas? _frontCanvas;
//    private Canvas? _axisXCanvas;
//    private Canvas? _ovarlayCanvas;
//    private ContentControl? _zoomControl;

//    private ScreenPoint _downMousePosition;
//    private bool _isPressed = false;

//    private readonly PlotModel _internalModel;
//    private readonly IPlotController _defaultController;

//    private readonly ObservableCollection<TimeDataViewer.Series> _series = new();
//    private readonly ObservableCollection<TimeDataViewer.Axis> _axises = new();

//    // Invalidation flag (0: no update, 1: update visual elements).  
//    private int _isPlotInvalidated;

//    public TimelineControl()
//    {
//        _defaultController = new PlotController();
//        _internalModel = new PlotModel();
//        ((IPlotModel)_internalModel).AttachPlotView(this);

//        _series.CollectionChanged += OnSeriesChanged;
//        _axises.CollectionChanged += OnAxesChanged;

//        var labels = new ObservableCollection<ItemViewModel>()
//        {
//            new() { Label = "Satellite1" },
//            new() { Label = "Satellite2" },
//            new() { Label = "Satellite3" },
//            new() { Label = "Satellite4" },
//            new() { Label = "Satellite5" }
//        };

//        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
//        var path = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "Footprints.json"));

//        var serializer = new FootprintSerializer(path);
//        var res = Task.Run(async () => await serializer.GetValuesAsync()).Result;
//        var footprints = res.Cast<Footprint>().ToList();

//        var min = footprints.Select(s => s.Begin).Min();
//        var max = footprints.Select(s => s.Begin).Max();

//        var satellites = footprints.Select(s => s.SatelliteName!).Distinct().ToList() ?? new List<string>();

//        var epoch = min.Date;

//        var beginScenario = 0.0;// Selected.BeginScenario;
//        var endScenario = 2 * 86400.0;// Selected.EndScenario;

//        var begin = 0.0; //Selected.Begin;
//        var duration = 2 * 86400.0;//Selected.Duration;

//        _axises.Add(new TimeDataViewer.CategoryAxis()
//        {
//            Position = AxisPosition.Left,
//            AbsoluteMinimum = -0.5,
//            AbsoluteMaximum = 4.5,
//            IsZoomEnabled = false,
//            Items = labels,
//            LabelField = "Label"
//        });

//        _axises.Add(new TimeDataViewer.DateTimeAxis()
//        {
//            Position = AxisPosition.Top,
//            IntervalType = DateTimeIntervalType.Auto,
//            AbsoluteMinimum = beginScenario,
//            AbsoluteMaximum = endScenario,
//            FirstDateTime = epoch,
//        });

//        var items1 = footprints.Where(s => Equals(s.SatelliteName, "Satellite1")).Select(s => CreateInterval(s, epoch)).ToList();
//        var items2 = footprints.Where(s => Equals(s.SatelliteName, "Satellite2")).Select(s => CreateInterval(s, epoch)).ToList();
//        var items3 = footprints.Where(s => Equals(s.SatelliteName, "Satellite3")).Select(s => CreateInterval(s, epoch)).ToList();
//        var items4 = footprints.Where(s => Equals(s.SatelliteName, "Satellite4")).Select(s => CreateInterval(s, epoch)).ToList();
//        var items5 = footprints.Where(s => Equals(s.SatelliteName, "Satellite5")).Select(s => CreateInterval(s, epoch)).ToList();

//        _series.Add(new TimeDataViewer.TimelineSeries()
//        {
//            BarWidth = 0.5,
//            FillBrush = Brushes.LightCoral,
//            Items = items1,
//            CategoryField = "Category",
//            BeginField = "BeginTime",
//            EndField = "EndTime"
//        });

//        _series.Add(new TimeDataViewer.TimelineSeries()
//        {
//            BarWidth = 0.5,
//            FillBrush = Brushes.Green,
//            Items = items2,
//            CategoryField = "Category",
//            BeginField = "BeginTime",
//            EndField = "EndTime"
//        });

//        _series.Add(new TimeDataViewer.TimelineSeries()
//        {
//            BarWidth = 0.5,
//            FillBrush = Brushes.Blue,
//            Items = items3,
//            CategoryField = "Category",
//            BeginField = "BeginTime",
//            EndField = "EndTime"
//        });

//        _series.Add(new TimeDataViewer.TimelineSeries()
//        {
//            BarWidth = 0.5,
//            FillBrush = Brushes.Red,
//            Items = items4,
//            CategoryField = "Category",
//            BeginField = "BeginTime",
//            EndField = "EndTime"
//        });

//        _series.Add(new TimeDataViewer.TimelineSeries()
//        {
//            BarWidth = 0.5,
//            FillBrush = Brushes.Yellow,
//            Items = items5,
//            CategoryField = "Category",
//            BeginField = "BeginTime",
//            EndField = "EndTime"
//        });

//        this.GetObservable(TransformedBoundsProperty).Subscribe(bounds => OnSizeChanged(this, bounds?.Bounds.Size ?? new Size()));

//        OnAppearanceChanged();
//    }

//    private Interval CreateInterval(Footprint footprint, DateTime epoch)
//    {
//        var secs = footprint.Begin.TimeOfDay.TotalSeconds;

//        var date = epoch.Date;

//        return new Interval()
//        {
//            Category = footprint.SatelliteName,
//            BeginTime = date.AddSeconds(secs),
//            EndTime = date.AddSeconds(secs + footprint.Duration)
//        };
//    }


//    public PlotModel ActualModel => _internalModel;

//    public IPlotController ActualController => _defaultController;

//    // Gets the coordinates of the client area of the view.
//    public OxyRect ClientArea => new(0, 0, Bounds.Width, Bounds.Height);

//    Model IView.ActualModel => ActualModel;

//    IController IView.ActualController => ActualController;

//    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
//    {
//        base.OnApplyTemplate(e);

//        _basePanel = e.NameScope.Find<Panel>(PART_BasePanel);
//        _axisXPanel = e.NameScope.Find<Panel>(PART_AxisXPanel);

//        if (_basePanel == null || _axisXPanel == null)
//        {
//            return;
//        }

//        _basePanel.PointerEnter += _basePanel_PointerEnter;
//        _basePanel.PointerLeave += _basePanel_PointerLeave;
//        _basePanel.PointerWheelChanged += _basePanel_PointerWheelChanged;
//        _basePanel.PointerPressed += _basePanel_PointerPressed;
//        _basePanel.PointerMoved += _basePanel_PointerMoved;
//        _basePanel.PointerReleased += _basePanel_PointerReleased;

//        _axisXPanel.PointerPressed += _axisXPanel_PointerPressed;
//        _axisXPanel.PointerMoved += _axisXPanel_PointerMoved;
//        _axisXPanel.PointerReleased += _axisXPanel_PointerReleased;

//        _backCanvas = e.NameScope.Find<Canvas>(PART_BackCanvas);
//        _drawCanvas = e.NameScope.Find<DrawCanvas>(PART_DrawCanvas);
//        _frontCanvas = e.NameScope.Find<Canvas>(PART_FrontCanvas);

//        _axisXCanvas = e.NameScope.Find<Canvas>(PART_AxisXCanvas);
//        _ovarlayCanvas = e.NameScope.Find<Canvas>(PART_OverlayCanvas);

//        _zoomControl = e.NameScope.Find<ContentControl>(PART_ZoomControl);
//    }

//    public void HideTracker()
//    {
//        //if (_currentTracker != null)
//        //{
//        //    _overlays.Children.Remove(_currentTracker);
//        //    _currentTracker = null;
//        //}
//    }

//    public void InvalidatePlot(bool updateData = true)
//    {
//        if (Width <= 0 || Height <= 0)
//        {
//            return;
//        }

//        UpdateModel(updateData);

//        if (Interlocked.CompareExchange(ref _isPlotInvalidated, 1, 0) == 0)
//        {
//            // Invalidate the arrange state for the element.
//            // After the invalidation, the element will have its layout updated,
//            // which will occur asynchronously unless subsequently forced by UpdateLayout.
//            BeginInvoke(InvalidateArrange);
//            BeginInvoke(InvalidateVisual);
//        }
//    }

//    protected virtual void UpdateModel(bool updateData = true)
//    {
//        SynchronizeProperties();
//        SynchronizeSeries();
//        SynchronizeAxes();

//        if (ActualModel != null)
//        {
//            ((IPlotModel)ActualModel).Update(updateData);
//        }
//    }

//    // Synchronizes the axes in the internal model.      
//    private void SynchronizeAxes()
//    {
//        _internalModel.Axises.Clear();
//        foreach (var a in _axises)
//        {
//            _internalModel.Axises.Add(a.CreateModel());
//        }
//    }

//    // Synchronizes the series in the internal model.       
//    private void SynchronizeSeries()
//    {
//        _internalModel.Series.Clear();
//        foreach (var s in _series)
//        {
//            _internalModel.Series.Add(s.CreateModel());
//        }
//    }

//    // Synchronize properties in the internal Plot model
//    private void SynchronizeProperties()
//    {
//        var m = _internalModel;

//        m.PlotMarginLeft = 0;
//        m.PlotMarginTop = 0;
//        m.PlotMarginRight = 0;
//        m.PlotMarginBottom = 0;

//        //     m.Padding = Padding.ToOxyThickness();

//        //  m.DefaultColors = DefaultColors.Select(c => c.ToOxyColor()).ToArray();

//        //   m.AxisTierDistance = AxisTierDistance;
//    }

//    // Called when the visual appearance is changed.     
//    protected void OnAppearanceChanged()
//    {
//        InvalidatePlot(false);
//    }

//    // Called when the visual appearance is changed.
//    private static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
//    {
//        ((TimelineControl)d).OnAppearanceChanged();
//    }

//    public void SetCursorType(CursorType cursorType)
//    {
//        Cursor = cursorType switch
//        {
//            CursorType.Pan => new Cursor(StandardCursorType.Hand),
//            CursorType.PanHorizontal => new Cursor(StandardCursorType.SizeWestEast),
//            CursorType.ZoomRectangle => new Cursor(StandardCursorType.SizeAll),
//            CursorType.ZoomHorizontal => new Cursor(StandardCursorType.SizeWestEast),
//            CursorType.ZoomVertical => new Cursor(StandardCursorType.SizeNorthSouth),
//            _ => Cursor.Default,
//        };
//    }

//    public void ShowTracker(TrackerHitResult trackerHitResult)
//    {
//        //if (trackerHitResult == null)
//        //{
//        //    HideTracker();
//        //    return;
//        //}

//        //var trackerTemplate = DefaultTrackerTemplate;
//        //if (trackerHitResult.Series != null && !string.IsNullOrEmpty(trackerHitResult.Series.TrackerKey))
//        //{
//        //    var match = TrackerDefinitions.FirstOrDefault(t => t.TrackerKey == trackerHitResult.Series.TrackerKey);
//        //    if (match != null)
//        //    {
//        //        trackerTemplate = match.TrackerTemplate;
//        //    }
//        //}

//        //if (trackerTemplate == null)
//        //{
//        //    HideTracker();
//        //    return;
//        //}

//        //var tracker = trackerTemplate.Build(new ContentControl());

//        //// ReSharper disable once RedundantNameQualifier
//        //if (!object.ReferenceEquals(tracker, _currentTracker))
//        //{
//        //    HideTracker();
//        //    _overlays.Children.Add(tracker.Control);
//        //    _currentTracker = tracker.Control;
//        //}

//        //if (_currentTracker != null)
//        //{
//        //    _currentTracker.DataContext = trackerHitResult;
//        //}
//    }

//    // Stores text on the clipboard.
//    public async void SetClipboardText(string text)
//    {
//        await AvaloniaLocator.Current.GetService<IClipboard>()!.SetTextAsync(text);
//    }

//    public void HideZoomRectangle()
//    {
//        //_zoomControl.IsVisible = false;
//    }

//    public void ShowZoomRectangle(OxyRect r)
//    {
//        //_zoomControl.Width = r.Width;
//        //_zoomControl.Height = r.Height;
//        //Canvas.SetLeft(_zoomControl, r.Left);
//        //Canvas.SetTop(_zoomControl, r.Top);
//        //_zoomControl.Template = ZoomRectangleTemplate;
//        //_zoomControl.IsVisible = true;
//    }


//    private void OnAxesChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        SyncLogicalTree(e);
//    }

//    private void OnSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        SyncLogicalTree(e);
//    }

//    private void SyncLogicalTree(NotifyCollectionChangedEventArgs e)
//    {
//        // In order to get DataContext and binding to work with the series, axes and annotations
//        // we add the items to the logical tree
//        if (e.NewItems != null)
//        {
//            foreach (var item in e.NewItems.OfType<ISetLogicalParent>())
//            {
//                item.SetParent(this);
//            }
//            LogicalChildren.AddRange(e.NewItems.OfType<ILogical>());
//            VisualChildren.AddRange(e.NewItems.OfType<IVisual>());
//        }

//        if (e.OldItems != null)
//        {
//            foreach (var item in e.OldItems.OfType<ISetLogicalParent>())
//            {
//                item.SetParent(null);
//            }
//            foreach (var item in e.OldItems)
//            {
//                LogicalChildren.Remove((ILogical)item);
//                VisualChildren.Remove((IVisual)item);
//            }
//        }
//    }


//    // Invokes the specified action on the dispatcher, if necessary.
//    private static void BeginInvoke(Action action)
//    {
//        if (Dispatcher.UIThread.CheckAccess())
//        {
//            Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Loaded);
//        }
//        else
//        {
//            action?.Invoke();
//        }
//    }

//    // Called when the size of the control is changed.
//    private void OnSizeChanged(object sender, Size size)
//    {
//        if (size.Height > 0 && size.Width > 0)
//        {
//            InvalidatePlot(false);
//        }
//    }

//    // Provides the behavior for the Arrange pass of Silverlight layout.
//    // Classes can override this method to define their own Arrange pass behavior.
//    protected override Size ArrangeOverride(Size finalSize)
//    {
//        var actualSize = base.ArrangeOverride(finalSize);
//        if (actualSize.Width > 0 && actualSize.Height > 0)
//        {
//            if (Interlocked.CompareExchange(ref _isPlotInvalidated, 0, 1) == 1)
//            {
//                UpdateVisuals();
//            }
//        }

//        return actualSize;
//    }

//    private void UpdateVisuals()
//    {
//        if (_backCanvas == null || _axisXCanvas == null || _frontCanvas == null || _drawCanvas == null)
//        {
//            return;
//        }

//        if (IsVisibleToUser() == false)
//        {
//            return;
//        }

//        // Clear the canvas
//        _backCanvas.Children.Clear();
//        _axisXCanvas.Children.Clear();
//        _frontCanvas.Children.Clear();

//        if (ActualModel != null)
//        {
//            var rcAxis = new CanvasRenderContext(_axisXCanvas);
//            var rcPlotBack = new CanvasRenderContext(_backCanvas);
//            //var rcPlotFront = new CanvasRenderContext(_frontCanvas);

//            //if (DisconnectCanvasWhileUpdating)
//            //{
//            //    // TODO: profile... not sure if this makes any difference
//            //    var idx0 = _panel.Children.IndexOf(_backCanvas);
//            //    if (idx0 != -1)
//            //    {
//            //        _panel.Children.RemoveAt(idx0);
//            //    }

//            //    var idx1 = _panel.Children.IndexOf(_canvasFront);
//            //    if (idx1 != -1)
//            //    {
//            //        _panel.Children.RemoveAt(idx1);
//            //    }


//            //    ((IPlotModel)ActualModel).Render(_backCanvas.Bounds.Width, _backCanvas.Bounds.Height);
//            //    RenderSeries(_backCanvas, _drawCanvas);
//            //    RenderAxisX(rcAxis, rcPlotBack);
//            //    RenderSlider(rcAxis, rcPlotFront);

//            //    // reinsert the canvas again
//            //    if (idx1 != -1)
//            //    {
//            //        _panel.Children.Insert(idx1, _canvasFront);
//            //    }

//            //    if (idx0 != -1)
//            //    {
//            //        _panel.Children.Insert(idx0, _backCanvas);
//            //    }
//            //}
//            //else
//            {
//                ((IPlotModel)ActualModel).Render(_backCanvas.Bounds.Width, _backCanvas.Bounds.Height);
//                RenderSeries(_backCanvas, _drawCanvas);
//                RenderAxisX(rcAxis, rcPlotBack);
//                //RenderSlider(rcAxis, rcPlotFront);
//            }
//        }
//    }

//    protected void RenderAxisX(CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
//    {
//        foreach (var item in _axises)
//        {
//            if (item.InternalAxis.IsHorizontal() == true)
//            {
//                item.Render(contextAxis, contextPlot);
//            }
//        }
//    }

//    protected void RenderSeries(Canvas canvasPlot, DrawCanvas drawCanvas)
//    {
//        drawCanvas.RenderSeries(_series.Where(s => s.IsVisible).ToList());
//    }

//    // Determines whether the plot is currently visible to the user.
//    protected bool IsVisibleToUser()
//    {
//        return IsUserVisible(this);
//    }

//    // Determines whether the specified element is currently visible to the user.
//    private static bool IsUserVisible(Control element)
//    {
//        return element.IsEffectivelyVisible && element.TransformedBounds.HasValue;
//    }

//    private void _axisXPanel_PointerReleased(object? sender, PointerReleasedEventArgs e)
//    {
//        //_isPressed = false;
//    }

//    private void _axisXPanel_PointerMoved(object? sender, PointerEventArgs e)
//    {
//        //if (_isPressed == true)
//        //{
//        //    base.OnPointerMoved(e);
//        //    if (e.Handled)
//        //    {
//        //        return;
//        //    }

//        //    e.Pointer.Capture(_panelX);

//        //    var point = e.GetPosition(_panelX).ToScreenPoint();

//        //    foreach (var a in Axises)
//        //    {
//        //        if (a.InternalAxis.IsHorizontal() == true && a is DateTimeAxis axis)
//        //        {
//        //            var value = axis.InternalAxis.InverseTransform(point.X);

//        //            DateTime TimeOrigin = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
//        //            Slider.IsTracking = false;
//        //            Slider.CurrentValue = TimeOrigin.AddDays(value - 1);
//        //            Slider.IsTracking = true;
//        //        }
//        //    }
//        //}
//    }

//    private void _axisXPanel_PointerPressed(object? sender, PointerPressedEventArgs e)
//    {
//        //base.OnPointerPressed(e);
//        //if (e.Handled)
//        //{
//        //    return;
//        //}

//        //Focus();
//        //e.Pointer.Capture(_panelX);

//        //var point = e.GetPosition(_panelX).ToScreenPoint();

//        //foreach (var a in Axises)
//        //{
//        //    if (a.InternalAxis.IsHorizontal() == true && a is DateTimeAxis axis)
//        //    {
//        //        var value = axis.InternalAxis.InverseTransform(point.X);

//        //        DateTime TimeOrigin = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
//        //        Slider.IsTracking = false;
//        //        Slider.CurrentValue = TimeOrigin.AddDays(value - 1);
//        //        Slider.IsTracking = true;

//        //        _isPressed = true;
//        //    }
//        //}
//    }

//    private void _basePanel_PointerReleased(object? sender, PointerReleasedEventArgs e)
//    {
//        //base.OnPointerReleased(e);
//        //if (e.Handled || _panel == null)
//        //{
//        //    return;
//        //}

//        //var releasedArgs = (PointerReleasedEventArgs)e;

//        //e.Pointer.Capture(null);

//        //e.Handled = ActualController.HandleMouseUp(this, releasedArgs.ToMouseReleasedEventArgs(_panel));

//        //// Open the context menu
//        //var p = e.GetPosition(_panel).ToScreenPoint();
//        //var d = p.DistanceTo(_mouseDownPoint);

//        //if (ContextMenu != null)
//        //{
//        //    if (Math.Abs(d) < 1e-8 && releasedArgs.InitialPressMouseButton == MouseButton.Right)
//        //    {
//        //        ContextMenu.DataContext = DataContext;
//        //        ContextMenu.IsVisible = true;
//        //    }
//        //    else
//        //    {
//        //        ContextMenu.IsVisible = false;
//        //    }
//        //}
//    }

//    private void _basePanel_PointerMoved(object? sender, PointerEventArgs e)
//    {
//        //base.OnPointerMoved(e);
//        //if (e.Handled || _panel == null)
//        //{
//        //    return;
//        //}

//        //e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(_panel));
//    }

//    private void _basePanel_PointerPressed(object? sender, PointerPressedEventArgs e)
//    {
//        //base.OnPointerPressed(e);
//        //if (e.Handled || _panel == null)
//        //{
//        //    return;
//        //}

//        //Focus();
//        //e.Pointer.Capture(_panel);

//        //// store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
//        //_mouseDownPoint = e.GetPosition(_panel).ToScreenPoint();

//        //e.Handled = ActualController.HandleMouseDown(this, e.ToMouseDownEventArgs(_panel));
//    }

//    private void _basePanel_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
//    {
//        //base.OnPointerWheelChanged(e);

//        //if (e.Handled || _panel == null)
//        //{
//        //    return;
//        //}

//        //e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(_panel));
//    }

//    private void _basePanel_PointerLeave(object? sender, PointerEventArgs e)
//    {
//        //base.OnPointerLeave(e);
//        //if (e.Handled || _panel == null)
//        //{
//        //    return;
//        //}

//        //e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(_panel));
//    }

//    private void _basePanel_PointerEnter(object? sender, PointerEventArgs e)
//    {
//        //base.OnPointerEnter(e);
//        //if (e.Handled || _panel == null)
//        //{
//        //    return;
//        //}

//        //e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(_panel));
//    }
//}
