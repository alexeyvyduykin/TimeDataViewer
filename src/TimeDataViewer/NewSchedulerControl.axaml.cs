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
    public partial class NewSchedulerControl : ItemsControl, IScheduler, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private Area _area;
        private readonly Factory _factory;
      //  private readonly Dictionary<SeriesViewModel, IEnumerable<IntervalViewModel>> _markerPool;
      //  private readonly ObservableCollection<MarkerViewModel> _markers;
        private Canvas _canvas;
        private double _zoom;
        private readonly TranslateTransform _schedulerTranslateTransform = new TranslateTransform();
        private ObservableCollection<Series> _series = new ObservableCollection<Series>();   
        // center 
        private readonly bool _showCenter = true;
        private readonly Pen _centerCrossPen = new Pen(Brushes.Red, 1);
        // mouse center
        private readonly bool _showMouseCenter = true;
        private readonly Pen _mouseCrossPen = new Pen(Brushes.Blue, 1);
        private DateTime _epoch;

        public event EventHandler OnZoomChanged;
        public event EventHandler OnSizeChanged;

        public NewSchedulerControl()
        {
            AvaloniaXamlLoader.Load(this);

            CoreFactory factory = new Core.CoreFactory();

            _area = factory.CreateArea();
            _area.OnZoomChanged += _area_OnZoomChanged;       

            _factory = new Factory();

            //  _markers = new ObservableCollection<MarkerViewModel>();
            //  _markerPool = new Dictionary<SeriesViewModel, IEnumerable<IntervalViewModel>>();

            _canvas = new Canvas()
            {
                RenderTransform = _schedulerTranslateTransform
            };

            ClipToBounds = true;
            //       SnapsToDevicePixels = true;

          //  Items = _markers;

            LayoutUpdated += SchedulerControl_LayoutUpdated;

        //    ZoomProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.ZoomChanged(e));
        //    EpochProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.EpochChanged(e));

        //    OnMousePositionChanged += AxisX.UpdateDynamicLabelPosition;

            _epoch = DateTime.Now;
        }

        private void _area_OnZoomChanged(object? sender, EventArgs e)
        {
            OnZoomChanged?.Invoke(this, EventArgs.Empty);
            ForceUpdateOverlays();
        }

        public DateTime Epoch0 => _epoch.Date;

        public DateTime Epoch => _epoch;

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);

            if (Items != null)
            {
                UpdateData();
            }
        }

        public void UpdateData()
        {
            var minLeft = double.MaxValue;
            var maxRight = double.MinValue;

            List<SeriesViewModel> seriesList = new List<SeriesViewModel>();

            foreach (var item in Items)
            {
                if (item is IntervalViewModel ival)
                {

                }
                else if (item is SeriesViewModel series)
                {
                    seriesList.Add(series);

                    minLeft = Math.Min(series.MinTime(), minLeft);
                    maxRight = Math.Max(series.MaxTime(), maxRight);
                }
                else
                {
                    throw new Exception();
                }
            }

            var rightDate = Epoch.AddSeconds(maxRight).Date.AddDays(1);

            var len = (rightDate - Epoch0).TotalSeconds;
           
            var d0 = (Epoch - Epoch0).TotalSeconds;
            int height0 = 100;        
            double step = (double)height0 / (double)(seriesList.Count + 1);

            int i = 0;
            foreach (var str in seriesList)
            {
                str.SetLocalPosition(0.0, (++i) * step);

                foreach (var ival in str.Intervals)
                {
                    ival.SetLocalPosition(d0 + ival.LocalPosition.X, str.LocalPosition.Y);
                }
            }

            _area.UpdateViewport(0.0, 0.0, len, height0);

            ForceUpdateOverlays();
        }

        public RectD ViewportArea => _area.Viewport;

        public RectD ClientViewportArea => _area.ClientViewport;

        public RectI AbsoluteWindow => _area.Window;

        public RectI ScreenWindow => _area.Screen;

        public Point2I WindowOffset => _area.WindowOffset;

        public ITimeAxis AxisX => _area.AxisX;
                  
        public IAxis AxisY => _area.AxisY;
                  
        internal Canvas Canvas => _canvas;

        private void SchedulerControl_LayoutUpdated(object? sender, EventArgs e)
        {
            _area.UpdateSize((int)Bounds.Width, (int)Bounds.Height);

            OnSizeChanged?.Invoke(this, EventArgs.Empty);
                
            ForceUpdateOverlays();            
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add)
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

        private void ForceUpdateOverlays() => ForceUpdateOverlays(Items/*_markers*/);

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
            var newValue = (double)e.NewValue;
            //var oldValue = (double)e.OldValue;

            var value = newValue;

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

        private void EpochChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is DateTime)
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
                _schedulerTranslateTransform.X = _area.WindowOffset.X;
                _schedulerTranslateTransform.Y = _area.WindowOffset.Y;
            }
        }

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

      //      DrawBackground(context);

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
        }

        private void DrawEpoch(DrawingContext context)
        {
            var d0 = (Epoch - Epoch0).TotalSeconds;
            var p = _area.FromLocalToAbsolute(new Point2D(d0, 0));
            Pen pen = new Pen(Brushes.Yellow, 2.0);
            context.DrawLine(pen, new Point(p.X + WindowOffset.X, 0.0), new Point(p.X + WindowOffset.X, _area.Window.Height));
        }

        public void ShowTooltip(Control placementTarget, Control tooltip)
        {
            throw new NotImplementedException();
        }

        public void HideTooltip()
        {
            throw new NotImplementedException();
        }
    }
}
