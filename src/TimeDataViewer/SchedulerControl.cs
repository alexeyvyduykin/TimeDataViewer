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
using TimeDataViewer.ViewModels;
using TimeDataViewer;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using TimeDataViewer.Models;
using TimeDataViewer.Core;

namespace TimeDataViewer
{
    public delegate void SelectionChangeEventHandler(RectD Selection, bool ZoomToFit);

    public partial class SchedulerControl : ItemsControl, IScheduler, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private readonly Area _area;
        private readonly Factory _factory;
        private readonly Dictionary<SeriesViewModel, IEnumerable<IntervalViewModel>> _markerPool;
        private readonly ObservableCollection<MarkerViewModel> _markers;
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

        public event EventHandler? OnSizeChanged;
        public event MousePositionChangedEventHandler? OnMousePositionChanged;
        public event EventHandler? OnZoomChanged;

        public SchedulerControl()
        {
            CoreFactory factory = new CoreFactory();

            _area = factory.CreateArea();
          
            _area.OnZoomChanged += _area_OnZoomChanged;

            _factory = new Factory();

            _markers = new ObservableCollection<MarkerViewModel>();
            _markerPool = new Dictionary<SeriesViewModel, IEnumerable<IntervalViewModel>>();

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

            var template = new FuncDataTemplate<MarkerViewModel>((m, s) => new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = new Binding("Shape"),
            });
            
            ItemTemplateProperty.OverrideDefaultValue<SchedulerControl>(template);
     
            var defaultPanel = new FuncTemplate<IPanel>(() => _canvas);
            
            ItemsPanelProperty.OverrideDefaultValue<SchedulerControl>(defaultPanel);

            ClipToBounds = true;
     //       SnapsToDevicePixels = true;
     
            Items = _markers;
     
            LayoutUpdated += SchedulerControl_LayoutUpdated;
         
            Series.CollectionChanged += (s, e) => PassingLogicalTree(e);

            ZoomProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.ZoomChanged(e));
            EpochProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.EpochChanged(e));
            CurrentTimeProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.CurrentTimeChanged(e));

            OnMousePositionChanged += AxisX.UpdateDynamicLabelPosition;
        }

        private void _area_OnZoomChanged(object? sender, EventArgs e)
        {
            OnZoomChanged?.Invoke(this, EventArgs.Empty);

            ForceUpdateOverlays();
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
                    var strng = _factory.CreateSeries(series.Category);                     
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

            _area.UpdateViewport(0.0, 0.0, len, height0);            
        }

        public RectD ViewportAreaData => _area.ViewportData;

        public RectD ViewportAreaScreen => _area.ViewportScreen;

        public RectI AbsoluteWindow => _area.WindowZoom;

        public RectI ScreenWindow => _area.Screen;

        public Point2I RenderOffsetAbsolute => _area.RenderOffsetAbsolute;

        public ITimeAxis AxisX => _area.AxisX;

        public IAxis AxisY => _area.AxisY;
            
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

        private void SchedulerControl_LayoutUpdated(object? sender, EventArgs e)
        {
            _area.UpdateSize((int)Bounds.Width, (int)Bounds.Height);

            OnSizeChanged?.Invoke(this, EventArgs.Empty);
                
            ForceUpdateOverlays();            
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is MarkerViewModel == false)
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

            foreach (MarkerViewModel item in items)
            {                    
                item?.ForceUpdateLocalPosition(this);                
            }

            InvalidateVisual();
        }

        private void ZoomChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is not null && e.NewValue is double value)
            {                           
                value = Math.Min(value, MaxZoom);
                value = Math.Max(value, MinZoom);

                if (_zoom != value)
                {
                    _area.Zoom = (int)Math.Floor(value);

                    if (IsInitialized == true)
                    {
                        ForceUpdateOverlays();
                        InvalidateVisual();
                    }

                    _zoom = value;

                    //RaisePropertyChanged(ZoomProperty, old, value);
                }
            }
        }

        private void EpochChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is DateTime)
            {              
                AxisX.Epoch0 = Epoch0;
            }
        }
        
        private void CurrentTimeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is double)
            {
                AxisX.Epoch0 = Epoch0;
            }
        }

        public int MaxZoom => _area.MaxZoom;  

        public int MinZoom => _area.MinZoom;   
  
        private void UpdateMarkersOffset()
        {
            if (Canvas != null)
            {
                _schedulerTranslateTransform.X = _area.RenderOffsetAbsolute.X;
                _schedulerTranslateTransform.Y = _area.RenderOffsetAbsolute.Y;
            }
        }

        //public bool SetZoomToFitRect(RectD rect)
        //{
        //    int maxZoom = _core.GetMaxZoomToFitRect(rect);
        //    if (maxZoom >= 0)
        //    {
        //        // Position___ = GetCenter(rect);

        //        _core.ZoomScreenPosition = _core.FromLocalToScreen(rect.GetCenter()/*Position___*/);

        //        if (maxZoom > MaxZoom)
        //        {
        //            maxZoom = MaxZoom;
        //        }

        //        if (_core.Zoom != maxZoom)
        //        {
        //            Zoom = maxZoom;
        //        }
        //        else if (maxZoom != MaxZoom)
        //        {
        //            Zoom += 1;
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        public Point2D FromScreenToLocal(int x, int y) => _area.FromScreenToLocal(x, y);        

        public Point2I FromLocalToScreen(Point2D point) => _area.FromLocalToScreen(point);        

        public Point2D FromAbsoluteToLocal(int x, int y) => _area.FromAbsoluteToLocal(x, y);        

        public Point2I FromLocalToAbsolute(Point2D point) => _area.FromLocalToAbsolute(point);
        
        public int TrueHeight => _area.Height;

        public bool IsTestBrush { get; set; } = false;

        public override void Render(DrawingContext context)
        {
            //if (IsStarted == false)
            //    return;

            DrawBackground(context);

            DrawEpoch(context);
              
            if (_showCenter == true)
            {
                context.DrawLine(_centerCrossPen, new Point((Bounds.Width / 2) - 5, Bounds.Height / 2), new Point((Bounds.Width / 2) + 5, Bounds.Height / 2));
                context.DrawLine(_centerCrossPen, new Point(Bounds.Width / 2, (Bounds.Height / 2) - 5), new Point(Bounds.Width / 2, (Bounds.Height / 2) + 5));
            }

            if (_showMouseCenter == true)
            {
                context.DrawLine(_mouseCrossPen,
                    new Point(_area.ZoomScreenPosition.X - 5, _area.ZoomScreenPosition.Y),
                    new Point(_area.ZoomScreenPosition.X + 5, _area.ZoomScreenPosition.Y));
                context.DrawLine(_mouseCrossPen,
                    new Point(_area.ZoomScreenPosition.X, _area.ZoomScreenPosition.Y - 5),
                    new Point(_area.ZoomScreenPosition.X, _area.ZoomScreenPosition.Y + 5));
            }

            using (context.PushPreTransform(_schedulerTranslateTransform.Value))
            {
                base.Render(context);
            }

            DrawCurrentTime(context);
        }

        private void DrawEpoch(DrawingContext context)
        {
            var d0 = (Epoch - Epoch0).TotalSeconds;
            var p = _area.FromLocalToAbsolute(new Point2D(d0, 0));    
            Pen pen = new Pen(Brushes.Yellow, 2.0);
            context.DrawLine(pen, new Point(p.X + RenderOffsetAbsolute.X, 0.0), new Point(p.X + RenderOffsetAbsolute.X, _area.RenderSize.Height));
        }

        private void DrawCurrentTime(DrawingContext context)
        {            
            var d0 = (Epoch - Epoch0).TotalSeconds;
            var p = _area.FromLocalToAbsolute(new Point2D(d0 + CurrentTime, 0));
            Pen pen = new Pen(Brushes.Red, 2.0);
            context.DrawLine(pen, new Point(p.X + RenderOffsetAbsolute.X, 0.0), new Point(p.X + RenderOffsetAbsolute.X, _area.RenderSize.Height));
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
