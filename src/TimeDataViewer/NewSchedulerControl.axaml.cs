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

        private Area _core;
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
   
        public event EventHandler OnLayoutUpdated
        {
            add
            {
                base.LayoutUpdated += value;
            }
            remove
            {
                base.LayoutUpdated -= value;
            }
        }

        public NewSchedulerControl()
        {
            AvaloniaXamlLoader.Load(this);

            Core.CoreFactory factory = new Core.CoreFactory();

            _core = new Area();
            _core.AxisX.IsDynamicLabelEnable = true;
            _core.AxisY.IsDynamicLabelEnable = true;
            _core.OnZoomChanged += (s, e) => ForceUpdateOverlays();
            _core.CanDragMap = true;
            _core.MouseWheelZoomEnabled = true;
            //_core.Zoom = (int)(ZoomProperty.GetDefaultValue(typeof(SchedulerControl)));

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

            LayoutUpdated += BaseSchedulerControl_LayoutUpdated;

        //    ZoomProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.ZoomChanged(e));
        //    EpochProperty.Changed.AddClassHandler<SchedulerControl>((d, e) => d.EpochChanged(e));

        //    OnMousePositionChanged += AxisX.UpdateDynamicLabelPosition;

            _epoch = DateTime.Now;
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

            _core.SetViewportArea(new RectD(0.0, 0.0, len, height0));

            ForceUpdateOverlays();
        }

        public RectD ViewportAreaData => _core.ViewportAreaData;

        public RectD ViewportAreaScreen => _core.ViewportAreaScreen;

        public RectI AbsoluteWindow => _core.WindowAreaZoom;

        public RectI ScreenWindow => _core.Screen;

        public Point2I RenderOffsetAbsolute => _core.RenderOffsetAbsolute;

        //public bool IsStarted => _core.IsStarted;

        public ITimeAxis AxisX => (ITimeAxis)_core.AxisX;
                  
        public IAxis AxisY => _core.AxisY;
                  
        internal Canvas Canvas => _canvas;

        private void BaseSchedulerControl_LayoutUpdated(object? sender, EventArgs e)
        {
            _core.UpdateSize((int)base.Bounds/*finalRect*/.Width, (int)base.Bounds/*finalRect*/.Height);

            //if (_core.IsStarted == true)
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

        public Point2D FromScreenToLocal(int x, int y) => _core.FromScreenToLocal(x, y);

        public Point2I FromLocalToScreen(Point2D point) => _core.FromLocalToScreen(point);

        public Point2D FromAbsoluteToLocal(int x, int y) => _core.FromAbsoluteToLocal(x, y);

        public Point2I FromLocalToAbsolute(Point2D point) => _core.FromLocalToAbsolute(point);

        public int TrueHeight => _core.TrueHeight;

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
