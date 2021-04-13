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
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo.Views
{
    public partial class SchedulerControl : ItemsControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private Core _core;
        private readonly Factory _factory;
        private readonly Dictionary<SchedulerString, IEnumerable<SchedulerInterval>> _markerPool;
        private readonly ObservableCollection<SchedulerMarker> _markers;
        private Canvas _canvas;
        private double _zoom;
        private readonly TranslateTransform _schedulerTranslateTransform = new TranslateTransform();
        private ObservableCollection<Series> _series = new ObservableCollection<Series>();
        //private readonly ScaleTransform _lastScaleTransform = new ScaleTransform();     
        private RectD _selectedArea;                  
        private readonly Brush _emptySchedulerBackground;
        private Brush _areaBackground;     
        private readonly Brush _areaBorderBrush;
        private readonly Pen _borderPen;
        private readonly double _areaBorderThickness;      
        private readonly Pen _selectionPen;
        private readonly Brush _selectedAreaFill;
        // center 
        public bool ShowCenter { get; set; } = true;
        private readonly Pen CenterCrossPen = new Pen(Brushes.Red, 1);
        public bool ShowMouseCenter { get; set; } = true;
        private readonly Pen MouseCrossPen = new Pen(Brushes.Blue, 1);
     
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

            _factory = new Factory();

            _markers = new ObservableCollection<SchedulerMarker>();
            _markerPool = new Dictionary<SchedulerString, IEnumerable<SchedulerInterval>>();

            PointerWheelChanged += SchedulerControl_PointerWheelChanged;
            PointerPressed += SchedulerControl_PointerPressed;
            PointerReleased += SchedulerControl_PointerReleased;
            PointerMoved += SchedulerControl_PointerMoved;

            _emptySchedulerBackground = new SolidColorBrush(Colors.Transparent);
            _areaBackground = new SolidColorBrush(Colors.Transparent);
            _areaBorderBrush = new SolidColorBrush(Colors.Transparent);
            _borderPen = new Pen(_areaBorderBrush, _areaBorderThickness);        
            _areaBorderThickness = 1.0;
            _selectionPen = new Pen(Brushes.Blue, 2.0);
            _selectedAreaFill = new SolidColorBrush(Color.FromArgb(33, Colors.RoyalBlue.R, Colors.RoyalBlue.G, Colors.RoyalBlue.B));
        
            InitBackgrounds();
    
            _canvas = new Canvas()
            {
                RenderTransform = _schedulerTranslateTransform
            };

            IntervalTemplate = new IntervalVisual();

            var style = new Style(x => x.OfType<ItemsControl>().Child().OfType<ContentPresenter>());
            style.Setters.Add(new Setter(Canvas.LeftProperty, new Binding("AbsolutePositionX")));
            style.Setters.Add(new Setter(Canvas.TopProperty, new Binding("AbsolutePositionY")));
            style.Setters.Add(new Setter(Canvas.ZIndexProperty, new Binding("ZIndex")));
            Styles.Add(style);

            var template = new FuncDataTemplate<SchedulerMarker>((m, s) => new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = new Binding("Shape"),
            });
            
            ItemTemplateProperty.OverrideDefaultValue<SchedulerControl>(template);
     
            var defaultPanel = new FuncTemplate<IPanel>(() => _canvas);
            
            ItemsPanelProperty.OverrideDefaultValue<SchedulerControl>(defaultPanel);

            ClipToBounds = true;
            //SnapsToDevicePixels = true;

            Items = _markers;
     
            LayoutUpdated += BaseSchedulerControl_LayoutUpdated;
         
            Series.CollectionChanged += Series_CollectionChanged;

            ZoomProperty.Changed.AddClassHandler<SchedulerControl>(ZoomChanged);
        }
              
        public static readonly DirectProperty<SchedulerControl, ObservableCollection<Series>> SeriesProperty =
            AvaloniaProperty.RegisterDirect<SchedulerControl, ObservableCollection<Series>>(nameof(Series), o => o.Series, (o, v) => o.Series = v);

        [Content]
        public ObservableCollection<Series> Series
        {
            get { return _series; }
            set { SetAndRaise(SeriesProperty, ref _series, value); }
        }


        public static readonly StyledProperty<BaseInterval> IntervalTemplateProperty =   
            AvaloniaProperty.Register<SchedulerControl, BaseInterval>(nameof(IntervalTemplate));

        public BaseInterval IntervalTemplate
        {
            get { return GetValue(IntervalTemplateProperty); }
            set { SetValue(IntervalTemplateProperty, value); }
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
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(this);
                }
                LogicalChildren.AddRange(e.NewItems.OfType<ILogical>());
                VisualChildren.AddRange(e.NewItems.OfType<IVisual>());
            }

            if (e.OldItems is not null)
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


        public void UpdateData()
        {
            _markers.Clear();
            _markerPool.Clear();
            var minLeft = double.MaxValue;
            var maxRight = double.MinValue;

            foreach (var series in Series)
            {
                if (series.Items is not null)
                {
                    var strng = _factory.CreateString();                     
                    var ivals = ((IList<Interval>)series.Items).Select(s => _factory.CreateInterval(s, strng, IntervalTemplate));

                    _markerPool.Add(strng, ivals);

                    _markers.Add(strng);
                    _markers.AddRange(ivals);

                    minLeft = Math.Min(strng.MinTime(), minLeft);
                    maxRight = Math.Max(strng.MaxTime(), maxRight);                   
                }
            }

            AutoSetViewportArea(minLeft, maxRight);

            ForceUpdateOverlays();
        }

        private void AutoSetViewportArea(double min, double max)
        {
            int height0 = 100;
            var count = _markerPool.Keys.Count;

            double step = (double)height0 / (double)(count + 1);

            int i = 0;
            foreach (var str in _markerPool.Keys)
            {              
                str.SetLocalPosition(min, (++i) * step);

                foreach (var ival in str.Intervals)
                {
                    ival.SetLocalPosition(ival.LocalPosition.X, str.LocalPosition.Y);
                }
            }

            if (min != double.MaxValue && max != double.MinValue)
            {
                _core.SetViewportArea(new RectD(min, 0, max - min, height0));
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

        internal Canvas Canvas => _canvas;

        public Panel? TopLevelForToolTips
        {
            get
            {
                IVisual root = this.GetVisualRoot();

                while (root is not null)
                {
                    if (root is Panel panel)
                    {
                        return panel;
                    }

                    if (root.VisualChildren.Count != 0)
                    {
                        root = root.VisualChildren[0];
                    }
                    else
                    {
                        return null;
                    }
                }

                return null;
            }
        }

        private void BaseSchedulerControl_LayoutUpdated(object? sender, EventArgs e)
        {
            _core.UpdateSize((int)base.Bounds/*finalRect*/.Width, (int)base.Bounds/*finalRect*/.Height);

            if (_core.IsStarted == true)
            {
                ForceUpdateOverlays();
            }
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is SchedulerMarker == false)
                    {
                        return;
                    }
                }

                ForceUpdateOverlays(e.NewItems);
            }
            else
            {
                InvalidateVisual();
            }
        }

        private void ForceUpdateOverlays()
        {
            ForceUpdateOverlays(_markers);
        }

        private void ForceUpdateOverlays(System.Collections.IEnumerable items)
        {
            UpdateMarkersOffset();

            foreach (SchedulerMarker item in items)
            {                    
                item?.ForceUpdateLocalPosition(this);                
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
            AvaloniaProperty.Register<SchedulerControl, double>(nameof(Zoom), defaultValue: 0.0, inherits: true, defaultBindingMode: BindingMode.TwoWay/*,validate: OnCoerceZoom*/);

        //public double Zoom
        //{
        //    get { return _zoom; }
        //    set
        //    {
        //        if (value > MaxZoom)
        //        {
        //            value = MaxZoom;
        //        }
        //        if (value < MinZoom)
        //        {
        //            value = MinZoom;
        //        }

        //        if (_zoom != value)
        //        {
        //            var old = _zoom;
               
        //            _core.Zoom = (int)Math.Floor(value);

        //            if (IsInitialized == true)
        //            {
        //                ForceUpdateOverlays();
        //                InvalidateVisual();
        //            }

        //            _zoom = value;

        //            RaisePropertyChanged(ZoomProperty, old, value);
        //        }
        //    }
        //}

        public double Zoom
        {
            get => GetValue(ZoomProperty);            
            set => SetValue(ZoomProperty, value);            
        }

        private void ZoomChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var newValue = (double)e.NewValue;
            //var oldValue = (double)e.OldValue;

            var value = newValue;

            value = Math.Min(value, MaxZoom);
            value = Math.Max(value, MinZoom);

            if (_zoom != value)
            {           
                _core.Zoom = (int)Math.Floor(value);

                if (IsInitialized == true)
                {
                    ForceUpdateOverlays();
                    InvalidateVisual();
                }

                _zoom = value;

                //RaisePropertyChanged(ZoomProperty, old, value);
            }
        }

        public int MaxZoom
        {
            get => _core.MaxZoom;            
            set => _core.MaxZoom = value;            
        }

        public int MinZoom
        {
            get => _core.MinZoom;            
            set => _core.MinZoom = value;            
        }

        // updates markers overlay offset     
        private void UpdateMarkersOffset()
        {
            if (Canvas != null)
            {
                _schedulerTranslateTransform.X = _core.RenderOffsetAbsolute.X;
                _schedulerTranslateTransform.Y = _core.RenderOffsetAbsolute.Y;
            }
        }

        public bool SetZoomToFitRect(RectD rect)
        {
            int maxZoom = _core.GetMaxZoomToFitRect(rect);
            if (maxZoom >= 0)
            {
                // Position___ = GetCenter(rect);

                _core.ZoomScreenPosition = _core.FromLocalToScreen(rect.GetCenter()/*Position___*/);

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

        public Point2D FromScreenToLocal(int x, int y) => _core.FromScreenToLocal(x, y);        

        public Point2I FromLocalToScreen(Point2D point) => _core.FromLocalToScreen(point);        

        public Point2D FromAbsoluteToLocal(int x, int y) => _core.FromAbsoluteToLocal(x, y);        

        public Point2I FromLocalToAbsolute(Point2D point) => _core.FromLocalToAbsolute(point);
        
        public int TrueHeight => _core.TrueHeight;

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
            drawingContext.FillRectangle(_emptySchedulerBackground, _core.RenderSize.ToAvaloniaRect());

            //     var p0 = new Point(WindowArea.Left, WindowArea.Top);// /*_core.*/FromSchedulerPointToLocal(ViewportArea.P0).ToPoint();
            //     var p1 = new Point(WindowArea.Right, WindowArea.Bottom);///*_core.*/FromSchedulerPointToLocal(ViewportArea.P1).ToPoint();

            // scheduler background grid
            drawingContext.FillRectangle(_areaBackground, _core.RenderSize.ToAvaloniaRect());

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
                drawingContext.FillRectangle(_selectedAreaFill, new Rect(x0, y0, x1 - x0, y1 - y0), 5);
                drawingContext.DrawRectangle(_selectionPen, new Rect(x0, y0, x1 - x0, y1 - y0), 5);
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
            drawingContext.DrawRectangle(_borderPen, new Rect(_core.RenderSize.X, _core.RenderSize.Y, _core.RenderSize.Width, _core.RenderSize.Height));

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
            using (drawingContext.PushPreTransform(_schedulerTranslateTransform.Value))
            {
                base.Render(drawingContext);
            }
            //drawingContext.Pop();
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

    internal static class Extensions
    {
        public static bool IsEmpty(this RectD rect)
        {
            return rect.Left == 0.0 && rect.Right == 0.0 && rect.Width == 0.0 && rect.Height == 0.0;
        }

        public static bool IsEmpty(this Point2D point)
        {
            return point.X == 0.0 && point.Y == 0.0;
        }

        public static Point2D GetCenter(this RectD rect)
        {
            return new(rect.X + rect.Width / 2, rect.Y - rect.Height / 2);
        }

        public static void AddRange<T>(this ObservableCollection<T> arr, IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                arr.Add(item);
            }
        }

        public static Rect ToAvaloniaRect(this RectD rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
