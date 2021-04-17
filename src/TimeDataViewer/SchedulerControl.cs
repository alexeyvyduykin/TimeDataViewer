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
using TimeDataViewer.Markers;
using TimeDataViewer;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace TimeDataViewer
{
    public delegate void SelectionChange(RectD Selection, bool ZoomToFit);

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
        private Popup _popup;
        // center 
        private readonly bool _showCenter = true;
        private readonly Pen _centerCrossPen = new Pen(Brushes.Red, 1);
        // mouse center
        private readonly bool _showMouseCenter = true;
        private readonly Pen _mouseCrossPen = new Pen(Brushes.Blue, 1);     

        public SchedulerControl()
        {
            _core = new Core();
            _core.AxisX = new TimeAxis() { CoordType = EAxisCoordType.X, TimePeriodMode = TimePeriod.Month };
            _core.AxisY = new CategoryAxis() { CoordType = EAxisCoordType.Y, IsInversed = true };
            _core.AxisX.IsDynamicLabelEnable = true;
            _core.AxisY.IsDynamicLabelEnable = true;
            _core.OnZoomChanged += (s, e) => ForceUpdateOverlays();
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
            
            _canvas = new Canvas()
            {
                RenderTransform = _schedulerTranslateTransform
            };

            _popup = new Popup()
            {
                //AllowsTransparency = true,
                //PlacementTarget = this,
                PlacementMode = PlacementMode.Pointer,
                IsOpen = false,
            };

            TopLevelForToolTips?.Children.Add(_popup);

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
         
            Series.CollectionChanged += (s, e) => PassingLogicalTree(e);

            ZoomProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.ZoomChanged(e));
            EpochProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.EpochChanged(e));

            OnMousePositionChanged += AxisX.UpdateDynamicLabelPosition;
        }

        public DateTime Epoch0 => Epoch.Date;

        private void PassingLogicalTree(NotifyCollectionChangedEventArgs e)
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

        public void ShowTooltip(Control placementTarget, Control tooltip)
        {
            if (TopLevelForToolTips?.Children.Contains(_popup) == false)
            {
                TopLevelForToolTips?.Children.Add(_popup);
            }

            _popup.PlacementTarget = placementTarget;
            _popup.Child = tooltip;
            _popup.IsOpen = true;
        }

        public void HideTooltip()
        {
            _popup.IsOpen = false;
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
                    var strng = _factory.CreateString(series.Category);                     
                    var ivals = ((IList<Interval>)series.Items).Select(s => _factory.CreateInterval(s, strng, series.IntervalTemplate));

                    _markerPool.Add(strng, ivals);

                    _markers.Add(strng);
                    _markers.AddRange(ivals);

                    minLeft = Math.Min(strng.MinTime(), minLeft);
                    maxRight = Math.Max(strng.MaxTime(), maxRight);                   
                }
            }
          
            var rightDate = Epoch.AddSeconds(maxRight).Date.AddDays(1);
          
            var len = (rightDate - Epoch0).TotalSeconds;
                        
            AutoSetViewportArea(len);

            ForceUpdateOverlays();
        }
      
        private void AutoSetViewportArea(double len)
        {
            var d0 = (Epoch - Epoch0).TotalSeconds;
            int height0 = 100;
            var count = _markerPool.Keys.Count;

            double step = (double)height0 / (double)(count + 1);

            int i = 0;
            foreach (var str in _markerPool.Keys)
            {
                str.SetLocalPosition(0.0, (++i) * step);

                foreach (var ival in str.Intervals)
                {
                    ival.SetLocalPosition(d0 + ival.LocalPosition.X, str.LocalPosition.Y);
                }
            }
                
            _core.SetViewportArea(new RectD(0.0, 0.0, len, height0));            
        }

        public RectD ViewportAreaData => _core.ViewportAreaData;

        public RectD ViewportAreaScreen => _core.ViewportAreaScreen;

        public RectI AbsoluteWindow => _core.WindowAreaZoom;

        public RectI ScreenWindow => _core.Screen;

        public Point2I RenderOffsetAbsolute => _core.RenderOffsetAbsolute;

        public bool IsStarted => _core.IsStarted;

        public TimeAxis AxisX
        {
            get => (TimeAxis)_core.AxisX;
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

        private void ForceUpdateOverlays() => ForceUpdateOverlays(_markers);        

        private void ForceUpdateOverlays(System.Collections.IEnumerable items)
        {
            UpdateMarkersOffset();

            foreach (SchedulerMarker item in items)
            {                    
                item?.ForceUpdateLocalPosition(this);                
            }

            InvalidateVisual();
        }

        private void ZoomChanged(AvaloniaPropertyChangedEventArgs e)
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

        private void EpochChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is DateTime)
            {              
                AxisX.Epoch0 = Epoch0;
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

        public bool IsTestBrush { get; set; } = false;

        public override void Render(DrawingContext context)
        {
            if (IsStarted == false)
                return;

            UpdateBackgroundBrush();
      
            context.FillRectangle(_areaBackground, _core.RenderSize.ToAvaloniaRect());

            DrawEpoch(context);
              
            if (_showCenter == true)
            {
                context.DrawLine(_centerCrossPen, new Point((Bounds.Width / 2) - 5, Bounds.Height / 2), new Point((Bounds.Width / 2) + 5, Bounds.Height / 2));
                context.DrawLine(_centerCrossPen, new Point(Bounds.Width / 2, (Bounds.Height / 2) - 5), new Point(Bounds.Width / 2, (Bounds.Height / 2) + 5));
            }

            if (_showMouseCenter == true)
            {
                context.DrawLine(_mouseCrossPen,
                    new Point(_core.ZoomScreenPosition.X - 5, _core.ZoomScreenPosition.Y),
                    new Point(_core.ZoomScreenPosition.X + 5, _core.ZoomScreenPosition.Y));
                context.DrawLine(_mouseCrossPen,
                    new Point(_core.ZoomScreenPosition.X, _core.ZoomScreenPosition.Y - 5),
                    new Point(_core.ZoomScreenPosition.X, _core.ZoomScreenPosition.Y + 5));
            }

            using (context.PushPreTransform(_schedulerTranslateTransform.Value))
            {
                base.Render(context);
            }          
        }

        private void DrawEpoch(DrawingContext context)
        {
            var d0 = (Epoch - Epoch0).TotalSeconds;
            var p = _core.FromLocalToAbsolute(new Point2D(d0, 0));    
            Pen pen = new Pen(Brushes.Yellow, 2.0);
            context.DrawLine(pen, new Point(p.X + RenderOffsetAbsolute.X, 0.0), new Point(p.X + RenderOffsetAbsolute.X, _core.RenderSize.Height));
        }

        public virtual void Dispose()
        {
            //if (_core.IsStarted == true)
            //{
            _core.OnZoomChanged -= (s, e) => ForceUpdateOverlays();// new SCZoomChanged(ForceUpdateOverlays);

            base.LayoutUpdated -= BaseSchedulerControl_LayoutUpdated;

            //_core.OnMapClose();
            //}
        }
    }

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
