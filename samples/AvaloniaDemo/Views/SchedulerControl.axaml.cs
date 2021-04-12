#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AvaloniaDemo.Markers;
using AvaloniaDemo.Models;
using TimeDataViewer;
using TimeDataViewer.Spatial;

namespace AvaloniaDemo.Views
{
    public partial class SchedulerControl : ItemsControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        internal Core _core;
        private Canvas _schedulerCanvas;
        private double _zoom;
        private readonly ScaleTransform lastScaleTransform = new ScaleTransform();
        private bool isDragging = false;
        private bool isSelected = false;
        private Point2D selectionStart;
        private Point2D selectionEnd;
        // current selected area in scheduler     
        private RectD _selectedArea;
        // lets you zoom by MouseWheel even when pointer is in area of marker       
        public bool IgnoreMarkerOnMouseWheel = true;// false;
        private Cursor cursorBefore = new Cursor(StandardCursorType.Arrow);
        private Point2D _schedulerMousePosition = new Point2D();
        private int onMouseUpTimestamp = 0;
        // if true, selects area just by holding mouse and moving     
        public bool DisableAltForSelection = false;
        // control
        private readonly Brush EmptySchedulerBackground = new SolidColorBrush(Colors.Transparent);
        // background
        public Brush AreaBackground { get; set; } = new SolidColorBrush(Colors.Transparent);
        // border
        private Brush _areaBorderBrush = new SolidColorBrush(Colors.Transparent);
        private double _areaBorderThickness = 1.0;
        private Pen _borderPen = null;
        // selections
        private readonly Pen SelectionPen = new Pen(Brushes.Blue, 2.0);
        private readonly Brush SelectedAreaFill = new SolidColorBrush(Color.FromArgb(33, Colors.RoyalBlue.R, Colors.RoyalBlue.G, Colors.RoyalBlue.B));
        // center 
        public bool ShowCenter { get; set; } = true;
        private readonly Pen CenterCrossPen = new Pen(Brushes.Red, 1);
        public bool ShowMouseCenter { get; set; } = true;
        private readonly Pen MouseCrossPen = new Pen(Brushes.Blue, 1);
        private ObservableCollection<SchedulerMarker> Markers = new ObservableCollection<SchedulerMarker>();
        internal readonly TranslateTransform SchedulerTranslateTransform = new TranslateTransform();

        public SchedulerControl()
        {
            _core = new Core();
            _core.AxisX = new TimeAxis() { CoordType = EAxisCoordType.X, TimePeriodMode = TimePeriod.Month };
            _core.AxisY = new CategoryAxis() { CoordType = EAxisCoordType.Y, IsInversed = true };
            _core.AxisX.IsDynamicLabelEnable = true;
            _core.AxisY.IsDynamicLabelEnable = true;
            _core.OnZoomChanged += new SCZoomChanged(ForceUpdateOverlays);
            _core.CanDragMap = true;
            _core.MouseWheelZoomEnabled = true;
            _core.Zoom = (int)(ZoomProperty.GetDefaultValue(typeof(SchedulerControl)));

            _schedulerCanvas = null;

            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _schedulerCanvas = new Canvas();
            _schedulerCanvas.Classes.Add("h1");

            _schedulerCanvas.RenderTransform = SchedulerTranslateTransform;

            FuncTemplate<IPanel> DefaultPanel = new FuncTemplate<IPanel>(() => _schedulerCanvas);

            ItemsPanelProperty.OverrideDefaultValue<SchedulerControl>(DefaultPanel);


            base.ClipToBounds = true;
            //  base.SnapsToDevicePixels = true;

            // by default its internal property, feel free to use your own
            //  if (base.Items == null)
            //  {
            base.Items = Markers;
            //  }

            //  _scheduler.Items = Markers;

            base.LayoutUpdated += BaseSchedulerControl_LayoutUpdated;

            InitBackgrounds();

            Series.CollectionChanged += Series_CollectionChanged;
        }
        
        private void Series_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Series series in Series)
            {
                series.Map = this;
            }

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


        private ObservableCollection<Series> _series = new ObservableCollection<Series>();

        public static readonly DirectProperty<SchedulerControl, ObservableCollection<Series>> SeriesProperty =
            AvaloniaProperty.RegisterDirect<SchedulerControl, ObservableCollection<Series>>(nameof(Series), o => o.Series, (o, v) => o.Series = v);

        [Content]
        public ObservableCollection<Series> Series
        {
            get { return _series; }
            set { SetAndRaise(SeriesProperty, ref _series, value); }
        }

        private double _minLeft_ = double.MaxValue;
        private double _maxRight_ = double.MinValue;
        private readonly IDictionary<SchedulerString, IList<SchedulerInterval>> _markers_ = new Dictionary<SchedulerString, IList<SchedulerInterval>>();

        public void UpdateData()
        {
            Markers.Clear();
            //IEnumerable<SchedulerMarker> markers = new List<SchedulerMarker>();

            _markers_.Clear();

            foreach (Series series in Series)
            {
                if (series.Items is not null)
                {
                    var markerStr = new SchedulerString("String");
                    markerStr.Shape = new StringVisual(markerStr);

                    List<SchedulerInterval> ivalMarkers = new List<SchedulerInterval>();

                    foreach (Interval ival in series.Items)
                    {
                        var markerIval = new SchedulerInterval(ival.Left, ival.Right);
                        markerIval.String = markerStr;
                        markerIval.Shape = new IntervalVisual(markerIval);
                        ivalMarkers.Add(markerIval);
                    }
                    AddIntervals(ivalMarkers, markerStr);
                }
            }

            //Markers = new ObservableCollection<SchedulerMarker>(markers);

            ForceUpdateOverlays(Markers);
        }

        public void AddIntervals(IEnumerable<SchedulerInterval> ivals, SchedulerString str)
        {
            if (str == null)
                return;

            if (_markers_.ContainsKey(str) == false)
            {
                _markers_.Add(str, new List<SchedulerInterval>());

                if (_core.AxisY is CategoryAxis)
                {
                    (str as SchedulerTargetMarker).OnTargetMarkerPositionChanged += AxisY.UpdateFollowLabelPosition;
                }

                AddMarkers(new List<SchedulerString>() { str });
            }

            if (ivals == null)
                return;

            foreach (var item in ivals)
            {
                str.Intervals.Add(item);
                item.String = str;

                _minLeft_ = Math.Min(item.Left, _minLeft_);
                _maxRight_ = Math.Max(item.Right, _maxRight_);

                _markers_[str].Add(item);
            }

            AutoSetViewportArea();

            AddMarkers(ivals);
        }

        private void AutoSetViewportArea()
        {
            int height0 = 100;

            var count = _markers_.Keys.Count;

            double step = (double)height0 / (double)(count + 1);
            int i = 0;
            foreach (var str in _markers_.Keys)
            {
                str.SetLocalPosition(_minLeft_, (++i) * step);

                foreach (var ival in str.Intervals)
                {
                    ival.SetLocalPosition(ival.LocalPosition.X, str.LocalPosition.Y);
                }
            }

            if (_minLeft_ != Double.MaxValue && _maxRight_ != Double.MinValue)
            {
                _core.SetViewportArea(new RectD(_minLeft_, 0, _maxRight_ - _minLeft_, height0));
            }
        }


        public RectD ViewportAreaData => _core.ViewportAreaData;

        public RectD ViewportAreaScreen => _core.ViewportAreaScreen;

        public RectI AbsoluteWindow => _core.WindowAreaZoom;

        public RectI ScreenWindow => _core.Screen;

        public Point2I RenderOffsetAbsolute => _core.RenderOffsetAbsolute;

        public bool IsStarted => _core.IsStarted;

        public BaseAxis AxisX
        {
            get => _core.AxisX;
            set => _core.AxisX = value;
        }

        public BaseAxis AxisY
        {
            get => _core.AxisY;
            set => _core.AxisY = value;
        }


        internal void AddMarkers(IEnumerable<SchedulerMarker> markers)
        {
            foreach (var item in markers)
            {
                Markers.Add(item);
            }

            ForceUpdateOverlays(markers);
        }

        internal Canvas SchedulerCanvas
        {
            get
            {

                //  return null;      
                if (_schedulerCanvas == null)
                {
                    if (this.VisualChildren.Count > 0)
                    {
                        var border = this.VisualChildren[0] as ContentPresenter;// Border;                    
                        var/*ItemsPresenter*/ items = border.Child;// as ItemsPresenter;
                        AvaloniaObject target = null;
                        foreach (var item in items.GetVisualChildren())
                        {
                            target = item as AvaloniaObject;
                            break;
                        }
                        _schedulerCanvas = target as Canvas;

                        _schedulerCanvas.RenderTransform = SchedulerTranslateTransform;


                        //Border border = VisualTreeHelper.GetChild(this, 0) as Border;
                        //ItemsPresenter items = border.Child as ItemsPresenter;
                        //DependencyObject target = VisualTreeHelper.GetChild(items, 0);
                        //_schedulerCanvas = target as Canvas;

                        //_schedulerCanvas.RenderTransform = SchedulerTranslateTransform;
                    }
                }

                return _schedulerCanvas;
            }
        }

        public Panel TopLevelForToolTips
        {
            get
            {
                IVisual root = this.GetVisualRoot();

                while (root != null)
                {
                    if (root is Panel == true)
                        return root as Panel;

                    if (root.VisualChildren.Count != 0)
                        root = root.VisualChildren[0];
                    else
                        return null;
                }

                return null;
            }
        }

        private void BaseSchedulerControl_LayoutUpdated(object sender, EventArgs e)
        {
            Debug.WriteLine(string.Format("SizeChanged : BaseSchedulerControl"));


            // ????????????????????????????????????????????????????????? base.Bounds or e.Width/e.Height

            _core.UpdateSize((int)base.Bounds/*finalRect*/.Width, (int)base.Bounds/*finalRect*/.Height);

            if (_core.IsStarted == true)
            {
                ForceUpdateOverlays();
            }
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is SchedulerMarker == false)
                        return;
                }

                ForceUpdateOverlays(e.NewItems);
            }
            else
            {
                InvalidateVisual();
            }
        }

        void ForceUpdateOverlays()
        {
            ForceUpdateOverlays(Markers);
        }

        void ForceUpdateOverlays(System.Collections.IEnumerable items)
        {
            //   using (Dispatcher.DisableProcessing())
            {
                //----------
                //foreach (var item in items)
                //{
                //    if ((item is SCSchedulerMarker) == false)
                //        return;
                //}

                //----------
                UpdateMarkersOffset();

                foreach (SchedulerMarker i in items)
                {
                    if (i != null)
                    {
                        i.ForceUpdateLocalPosition(this);

                        //if (i is IShapable)
                        //{
                        //    RegenerateShape(i as IShapable);
                        //}
                    }
                }
            }

            InvalidateVisual();
        }

        private static double OnCoerceZoom(AvaloniaObject o, double value)
        {
            SchedulerControl scheduler = o as SchedulerControl;
            if (scheduler != null)
            {
                if (value > scheduler.MaxZoom)
                {
                    value = scheduler.MaxZoom;
                }
                if (value < scheduler.MinZoom)
                {
                    value = scheduler.MinZoom;
                }

                return value;
            }
            else
            {
                return value;
            }
        }

        public static readonly StyledProperty<double> ZoomProperty =
    AvaloniaProperty.Register<SchedulerControl, double>(
        nameof(Zoom),
        defaultValue: 0.0,
        inherits: true,
        defaultBindingMode: BindingMode.TwoWay/*,
        validate: OnCoerceZoom*/);

        public double Zoom
        {
            get { return _zoom; }
            set
            {
                if (value > MaxZoom)
                {
                    value = MaxZoom;
                }
                if (value < MinZoom)
                {
                    value = MinZoom;
                }

                if (_zoom != value)
                {
                    var old = _zoom;
                    Debug.WriteLine("Zoom: " + Zoom + " -> " + value);

                    _core.Zoom = (int)Math.Floor(value);

                    if (IsInitialized == true)
                    {
                        ForceUpdateOverlays();
                        InvalidateVisual();
                    }

                    _zoom = value;

                    RaisePropertyChanged(ZoomProperty, old, value);
                }
            }
        }

        public int MaxZoom
        {
            get
            {
                return _core.MaxZoom;
            }
            set
            {
                _core.MaxZoom = value;
            }
        }

        public int MinZoom
        {
            get
            {
                return _core.MinZoom;
            }
            set
            {
                _core.MinZoom = value;
            }
        }

        // updates markers overlay offset     
        private void UpdateMarkersOffset()
        {
            if (SchedulerCanvas != null)
            {
                SchedulerTranslateTransform.X = _core.RenderOffsetAbsolute.X;
                SchedulerTranslateTransform.Y = _core.RenderOffsetAbsolute.Y;
            }
        }

        private Point2D GetCenter(RectD rect)
        {
            Point2D center = new Point2D(rect.X + rect.Width / 2, rect.Y - rect.Height / 2);
            return center;
        }

        public bool SetZoomToFitRect(RectD rect)
        {
            int maxZoom = _core.GetMaxZoomToFitRect(rect);
            if (maxZoom >= 0)
            {
                // Position___ = GetCenter(rect);

                _core.ZoomScreenPosition = _core.FromLocalToScreen(GetCenter(rect)/*Position___*/);

                if (maxZoom > MaxZoom)
                {
                    maxZoom = MaxZoom;
                }

                if (_core.Zoom != maxZoom)
                {
                    Zoom = maxZoom;
                }
                else if (maxZoom != MaxZoom)
                {
                    Zoom += 1;
                }

                return true;
            }

            return false;
        }

        public Point2D FromScreenToLocal(int x, int y)
        {
            return _core.FromScreenToLocal(x, y);
        }

        public Point2I FromLocalToScreen(Point2D point)
        {
            return _core.FromLocalToScreen(point);
        }

        public Point2D FromAbsoluteToLocal(int x, int y)
        {
            return _core.FromAbsoluteToLocal(x, y);
        }

        public Point2I FromLocalToAbsolute(Point2D point)
        {
            return _core.FromLocalToAbsolute(point);
        }

        public int TrueHeight
        {
            get
            {
                return _core.TrueHeight;
            }
        }

        public RectD SelectedArea
        {
            get
            {
                return _selectedArea;
            }
            set
            {
                _selectedArea = value;
                InvalidateVisual();
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            if ((/*IsMouseDirectlyOver ||*/ IgnoreMarkerOnMouseWheel) && _core.IsDragging == false)
            {

                if (e.Delta.Y > 0)
                {
                    Zoom = ((int)Zoom) + 1;
                }
                else
                {
                    Zoom = ((int)(Zoom + 0.99)) - 1;
                }

                var ps = (this as Visual).PointToScreen(new Point(_core.ZoomScreenPosition.X, _core.ZoomScreenPosition.Y));

                Stuff.SetCursorPos((int)ps.X, (int)ps.Y);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                Point p = e.GetPosition(this);

                _core.MouseDown.X = (int)p.X;
                _core.MouseDown.Y = (int)p.Y;

                base.InvalidateVisual();
            }
            else if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            {
                if (isSelected == false)
                {
                    Point p = e.GetPosition(this);
                    isSelected = true;
                    SelectedArea = default;// RectD.Empty;
                    selectionEnd = default;//Point2D.Empty;
                    selectionStart = FromScreenToLocal((int)p.X, (int)p.Y);

                    Debug.WriteLine("Selected = true, Start = " + selectionStart);
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (isSelected == true)
            {
                isSelected = false;
            }

            if (_core.IsDragging == true)
            {
                if (isDragging == true)
                {
                    onMouseUpTimestamp = (int)e.Timestamp & Int32.MaxValue;
                    isDragging = false;
                    Debug.WriteLine("IsDragging = " + isDragging);
                    base.Cursor = cursorBefore;
                    //Mouse.Capture(null);
                    e.Pointer.Capture(null);
                }
                _core.EndDrag();
            }
            else
            {
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true/*ChangedButton == MouseButton.Right*/)
                {
                    _core.MouseDown = default;//Point2I.Empty;
                }

                if (selectionEnd.IsEmpty() == false && selectionStart.IsEmpty() == false)
                {
                    bool zoomtofit = false;

                    if (SelectedArea.IsEmpty() == false && e.KeyModifiers == KeyModifiers.Shift /* Keyboard.Modifiers == ModifierKeys.Shift*/)
                    {
                        zoomtofit = SetZoomToFitRect(SelectedArea);
                    }

                    if (OnSchedulerSelectionChanged != null)
                    {
                        OnSchedulerSelectionChanged(SelectedArea, true/*zoomtofit*/);
                    }

                    SelectedArea = default;// RectD.Empty;
                    selectionEnd = default;// Point2D.Empty;

                    //   _core.UpdateBounds__();
                }
                else
                {
                    InvalidateVisual();
                }
            }
        }

        public Point2D SchedulerMousePosition
        {
            get
            {
                return _schedulerMousePosition;
            }
            protected set
            {
                _schedulerMousePosition = value;

                if (OnMousePositionChanged != null)
                    OnMousePositionChanged(_schedulerMousePosition);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            Point MouseScreenPosition = e.GetPosition(this);

            //if (_core.IsWindowArea(MouseAbsolutePosition) == true)
            {
                _core.ZoomScreenPosition = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);
                //  base.InvalidateVisual();
            }

            SchedulerMousePosition = _core.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            //  _core.AxisX.IsDynamicLabelEnable = true;
            //    _core.AxisX.UpdateDynamicLabelPosition(SchedulerMousePosition);
            //   _core.AxisY.IsDynamicLabelEnable = true;
            //    _core.AxisY.UpdateDynamicLabelPosition(SchedulerMousePosition);

            // wpf generates to many events if mouse is over some visual and OnMouseUp is fired, wtf, anyway...         
            if (((int)e.Timestamp & Int32.MaxValue) - onMouseUpTimestamp < 55)
            {
                Debug.WriteLine("OnMouseMove skipped: " + (((int)e.Timestamp & Int32.MaxValue) - onMouseUpTimestamp) + "ms");
                return;
            }

            if (_core.IsDragging == false && _core.MouseDown.IsEmpty == false)
            {
                // cursor has moved beyond drag tolerance
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
                {
                    if (Math.Abs(MouseScreenPosition.X - _core.MouseDown.X) * 2 >= 2/*SystemParameters.MinimumHorizontalDragDistance*/ ||
                        Math.Abs(MouseScreenPosition.Y - _core.MouseDown.Y) * 2 >= 2/*SystemParameters.MinimumVerticalDragDistance*/)
                    {
                        _core.BeginDrag(_core.MouseDown);
                    }
                }

            }

            if (_core.IsDragging == true)
            {
                if (isDragging == false)
                {
                    isDragging = true;
                    Debug.WriteLine("IsDragging = " + isDragging);
                    cursorBefore = base.Cursor;
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);// Cursors.SizeWE;// SizeAll;
                    e.Pointer.Capture(this);
                    //Mouse.Capture(this);
                }

                _core.MouseCurrent.X = (int)MouseScreenPosition.X;
                _core.MouseCurrent.Y = (int)MouseScreenPosition.Y;

                _core.Drag(_core.MouseCurrent);

                UpdateMarkersOffset();

                base.InvalidateVisual();
            }
            else
            {
                if (isSelected && selectionStart.IsEmpty() == false &&
                    (e.KeyModifiers == KeyModifiers.Shift /*Keyboard.Modifiers == ModifierKeys.Shift*/ ||
                    e.KeyModifiers == KeyModifiers.Alt /*Keyboard.Modifiers == ModifierKeys.Alt*/ ||
                    DisableAltForSelection))
                {
                    selectionEnd = _core.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);
                    {
                        Point2D p1 = selectionStart;
                        Point2D p2 = selectionEnd;

                        //double x1 = Math.Min(p1.X, p2.X);
                        //double y1 = Math.Max(p1.Y, p2.Y);
                        //double x2 = Math.Max(p1.X, p2.X);
                        //double y2 = Math.Min(p1.Y, p2.Y);

                        //SelectedArea = new SCSchedulerRect(x1, y1, x2 - x1, y1 - y2);


                        double x0 = Math.Min(p1.X, p2.X);
                        double y0 = Math.Min(p1.Y, p2.Y);
                        double x1 = Math.Max(p1.X, p2.X);
                        double y1 = Math.Max(p1.Y, p2.Y);

                        SelectedArea = new RectD(x0, y0, x1 - x0, y1 - y0);
                    }
                }
            }
        }

        // occurs when mouse selection is changed       
        public event SelectionChange OnSchedulerSelectionChanged;

        public event SCPositionChanged OnMousePositionChanged;

        public event SCDragChanged OnSchedulerDragChanged
        {
            add
            {
                _core.OnDragChanged += value;
            }
            remove
            {
                _core.OnDragChanged -= value;
            }
        }

        public event SCZoomChanged OnSchedulerZoomChanged
        {
            add
            {
                _core.OnZoomChanged += value;
            }
            remove
            {
                _core.OnZoomChanged -= value;
            }
        }

        public Brush AreaBorderBrush
        {
            get
            {
                return _areaBorderBrush;
            }
            set
            {
                _areaBorderBrush = value;
                _borderPen = new Pen(_areaBorderBrush, _areaBorderThickness);
            }
        }

        public double AreaBorderThickness
        {
            get
            {
                return _areaBorderThickness;
            }
            set
            {
                _areaBorderThickness = value;
                _borderPen = new Pen(_areaBorderBrush, _areaBorderThickness);
            }
        }

        Pen BorderPen
        {
            get
            {
                if (_borderPen == null)
                    _borderPen = new Pen(AreaBorderBrush, AreaBorderThickness);
                return _borderPen;
            }
        }

#if DEBUG
        Pen RenderOffsetPen = new Pen(Brushes.Blue, 1.0) { DashStyle = new DashStyle(new double[] { 25, 10 }, 0) };
        Pen GlobalFramePen = new Pen(Brushes.Blue, 5.0);
        Pen LocalFramePen = new Pen(Brushes.Green, 5.0);
#endif
        public bool IsTestBrush { get; set; } = false;

        public override void Render(DrawingContext drawingContext)
        {
            if (IsStarted == false)
                return;

            SchedulerBase_OnMapZoomChanged();


            // control
            drawingContext.FillRectangle(EmptySchedulerBackground, new Rect(_core.RenderSize.X, _core.RenderSize.Y, _core.RenderSize.Width, _core.RenderSize.Height));

            //     var p0 = new Point(WindowArea.Left, WindowArea.Top);// /*_core.*/FromSchedulerPointToLocal(ViewportArea.P0).ToPoint();
            //     var p1 = new Point(WindowArea.Right, WindowArea.Bottom);///*_core.*/FromSchedulerPointToLocal(ViewportArea.P1).ToPoint();

            // scheduler background grid
            drawingContext.FillRectangle(AreaBackground, new Rect(_core.RenderSize.X, _core.RenderSize.Y, _core.RenderSize.Width, _core.RenderSize.Height));

#if DEBUG
            if (IsTestBrush == true)
            {

                // origin window => top left scheduler point
                var renderOffset = _core.RenderSize.TopLeft;// p0;// new Point(WindowArea.Left, WindowArea.Top);///*_core.*/FromSchedulerPointToLocal(ViewportArea.P0).ToPoint();
                drawingContext.DrawLine(RenderOffsetPen, new Point(0, 0), new Point(renderOffset.X, renderOffset.Y));

                // global(window) axises
                drawingContext.DrawLine(GlobalFramePen, new Point(0, 0), new Point(0, 50));
                drawingContext.DrawLine(GlobalFramePen, new Point(0, 0), new Point(50, 0));

                //drawingContext.PushTransform(_core.ToViewportInPixels);
                //{
                //    // local axises
                //    drawingContext.DrawLine(LocalFramePen, new Point(), new Point(_core.ZoomingWindowArea.Height / 8.0, 0.0));
                //    drawingContext.DrawLine(LocalFramePen, new Point(), new Point(0, _core.ZoomingWindowArea.Height / 8.0));
                //}
                //drawingContext.Pop();
            }

#endif

            // selection
            if (SelectedArea.IsEmpty() == false)
            {
                Point2I p00 = FromLocalToScreen(SelectedArea.TopLeft/*P0*/);
                Point2I p11 = FromLocalToScreen(SelectedArea.BottomRight/*P1*/);

                int x0 = Math.Min(p00.X, p11.X);
                int y0 = Math.Min(p00.Y, p11.Y);
                int x1 = Math.Max(p00.X, p11.X);
                int y1 = Math.Max(p00.Y, p11.Y);

                //  int x1 = p00.X;
                //   int y1 = p00.Y;
                //  int x2 = p11.X;
                //  int y2 = p11.Y;
                drawingContext.FillRectangle(SelectedAreaFill, new Rect(x0, y0, x1 - x0, y1 - y0), 5);
                drawingContext.DrawRectangle(SelectionPen, new Rect(x0, y0, x1 - x0, y1 - y0), 5);
            }

            // center
            if (ShowCenter == true)
            {
                drawingContext.DrawLine(CenterCrossPen, new Point((base.Bounds.Width/*ActualWidth*/ / 2) - 5, base.Bounds.Height/*ActualHeight*/ / 2), new Point((base.Bounds.Width/*ActualWidth*/ / 2) + 5, base.Bounds.Height/*ActualHeight*/ / 2));
                drawingContext.DrawLine(CenterCrossPen, new Point(base.Bounds.Width/*ActualWidth*/ / 2, (base.Bounds.Height/*ActualHeight*/ / 2) - 5), new Point(base.Bounds.Width/*ActualWidth*/ / 2, (base.Bounds.Height/*ActualHeight*/ / 2) + 5));
            }

            if (ShowMouseCenter == true)
            {
                drawingContext.DrawLine(MouseCrossPen,
                    new Point(_core.ZoomScreenPosition.X - 5, _core.ZoomScreenPosition.Y),
                    new Point(_core.ZoomScreenPosition.X + 5, _core.ZoomScreenPosition.Y));
                drawingContext.DrawLine(MouseCrossPen,
                    new Point(_core.ZoomScreenPosition.X, _core.ZoomScreenPosition.Y - 5),
                    new Point(_core.ZoomScreenPosition.X, _core.ZoomScreenPosition.Y + 5));

                Debug.WriteLine(string.Format("MouseScreenPosition: x={0}, y={1}", _core.ZoomScreenPosition.X, _core.ZoomScreenPosition.Y));
            }

            // border
            drawingContext.DrawRectangle(BorderPen, new Rect(_core.RenderSize.X, _core.RenderSize.Y, _core.RenderSize.Width, _core.RenderSize.Height));

#if DEBUG  
            if (IsTestBrush == true)
            {
                double mark = 10.0;
                Pen borderPen = new Pen(Brushes.Black, 2.0);
                drawingContext.DrawRectangle(borderPen, base.Bounds/*RenderSize*/);
                drawingContext.DrawLine(borderPen, new Point(0, 0), new Point(mark, mark));
                drawingContext.DrawLine(borderPen, new Point(base.Bounds/*RenderSize*/.Width, 0), new Point(base.Bounds/*RenderSize*/.Width - mark, mark));
                drawingContext.DrawLine(borderPen, new Point(base.Bounds/*RenderSize*/.Width, base.Bounds/*RenderSize*/.Height), new Point(base.Bounds/*RenderSize*/.Width - mark, base.Bounds/*RenderSize*/.Height - mark));
                drawingContext.DrawLine(borderPen, new Point(0, base.Bounds/*RenderSize*/.Height), new Point(mark, base.Bounds/*RenderSize*/.Height - mark));
            }
#endif

            //var _p0 = _core.FromLocalToAbsolute(_core.ViewportAreaScreen.P0);
            //var _p1 = _core.FromLocalToAbsolute(_core.ViewportAreaScreen.P1);
            //drawingContext.DrawRectangle(null, new Pen(Brushes.Orange, 10.0),
            //    new Rect(new Point(_p0.X, _p0.Y), new Point(_p1.X, _p1.Y)));


            //   drawingContext.DrawRectangle(null, new Pen(Brushes.Orange, 10.0), _core.RenderVisibleWindow);

            //drawingContext.PushTransform(SchedulerTranslateTransform);
            using (drawingContext.PushPreTransform(SchedulerTranslateTransform.Value))
            {
                base.Render(drawingContext);
            }
            //drawingContext.Pop();
        }

        void DrawMarker(DrawingContext drawingContext, Point2I point, Brush brush)
        {
            //  drawingContext.DrawEllipse(brush, new Pen(Brushes.Black, 1.0), new Point(point.X, point.Y), 5.0, 5.0);
            EllipseGeometry ellipseGeometry = new EllipseGeometry(new Rect(point.X, point.Y, 5.0, 5.0));
            drawingContext.DrawGeometry(brush, new Pen(Brushes.Black, 1.0), ellipseGeometry);
        }

        public virtual void Dispose()
        {
            //if (_core.IsStarted == true)
            //{
            _core.OnZoomChanged -= new SCZoomChanged(ForceUpdateOverlays);

            base.LayoutUpdated -= BaseSchedulerControl_LayoutUpdated;

            //_core.OnMapClose();
            //}
        }
    }

    public delegate void SelectionChange(RectD Selection, bool ZoomToFit);

    internal class Stuff
    {
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);
    }

    internal static class SpatialExtensions
    {
        public static bool IsEmpty(this RectD rect)
        {
            return rect.Left == 0.0 && rect.Right == 0.0 && rect.Width == 0.0 && rect.Height == 0.0;
        }

        public static bool IsEmpty(this Point2D point)
        {
            return point.X == 0.0 && point.Y == 0.0;
        }
    }
}
