#nullable enable
using System;
using System.Collections;
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
using Avalonia.Threading;
using Avalonia.LogicalTree;
using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Timeline.ViewModels;
using Timeline;
using Timeline.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Timeline.Models;
using Timeline.Core;
using Avalonia.Controls.Generators;
using System.Threading.Tasks;
using Timeline.Views;
using Avalonia.Collections;

namespace Timeline
{
    public delegate void SelectionChangeEventHandler(RectD Selection, bool ZoomToFit);

    public partial class TimelineControl : ItemsControl, IStyleable, ITimeline
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private readonly Area _area; 
        private readonly ITimeAxis _axisX;
        private readonly Canvas _canvas;
        private ObservableCollection<IMarker> _markers;
        private readonly TranslateTransform _schedulerTranslateTransform;
        private ObservableCollection<Series> _series;
        private IList<ISeries> _seriesViewModels;    
        private readonly Popup _popup;
        // center 
        private readonly bool _showCenter = true;
        private readonly Pen _centerCrossPen = new(Brushes.Red, 1);
        // mouse center
        private readonly bool _showMouseCenter = true;
        private readonly Pen _mouseCrossPen = new(Brushes.Blue, 1);

        public event EventHandler? OnSizeChanged;
        public event MousePositionChangedEventHandler? OnMousePositionChanged;
        public event EventHandler? OnZoomChanged;
        public event EventHandler? OnDragChanged;

        public TimelineControl()
        {
            CoreFactory factory = new CoreFactory();

            _area = factory.CreateArea();
            _axisX = _area.AxisX;
           
            _series = new ObservableCollection<Series>();
            _schedulerTranslateTransform = new TranslateTransform();

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
            style.Setters.Add(new Setter(Canvas.LeftProperty, new Binding(nameof(MarkerViewModel.AbsolutePositionX))));
            style.Setters.Add(new Setter(Canvas.TopProperty, new Binding(nameof(MarkerViewModel.AbsolutePositionY))));
            style.Setters.Add(new Setter(Canvas.ZIndexProperty, new Binding(nameof(MarkerViewModel.ZIndex))));
            Styles.Add(style);

            ItemTemplate = new CustomItemTemplate();

            ItemsPanel = new FuncTemplate<IPanel>(() => _canvas);

            ClipToBounds = true;
            //       SnapsToDevicePixels = true;

            //       LayoutUpdated += TimelineControl_LayoutUpdated;    
            //TransformedBoundsProperty.Changed.AddClassHandler<TimelineControl>((d, e) => d.SizeChanged(e));

            Series.CollectionChanged += (s, e) => PassingLogicalTree(e);
            Series.CollectionChanged += (s, e) => Series_CollectionChanged(s, e);

            ZoomProperty.Changed.AddClassHandler<TimelineControl>((d, e) => d.ZoomChanged(e));          
            CurrentTimeProperty.Changed.AddClassHandler<TimelineControl>((d, e) => d.CurrentTimeChanged(e));

            _markers = new ObservableCollection<IMarker>();

            SetViewport();

            Items = _markers;
        }

        public DateTime Begin0 => Begin.Date;

        public RectD ViewportArea => _area.Viewport;

        public RectD ClientViewportArea => _area.ClientViewport;

        public RectI AbsoluteWindow => _area.Window;

        public RectI Screen => _area.Screen;

        public int MaxZoom => _area.MaxZoom;

        public int MinZoom => _area.MinZoom;

        public Point2I WindowOffset => _area.WindowOffset;

        public ITimeAxis AxisX => _area.AxisX;

        public ICategoryAxis AxisY => _area.AxisY;

        private Canvas Canvas => _canvas;

        private void TimelineControl_OnMousePositionChanged(Point2D point)
        {
            AxisX.UpdateDynamicLabelPosition(Begin0, point);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            PointerWheelChanged += TimelineControl_PointerWheelChanged;
            PointerPressed += TimelineControl_PointerPressed;
            PointerReleased += TimelineControl_PointerReleased;
            PointerMoved += TimelineControl_PointerMoved;

            OnMousePositionChanged += TimelineControl_OnMousePositionChanged;
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            PointerWheelChanged -= TimelineControl_PointerWheelChanged;
            PointerPressed -= TimelineControl_PointerPressed;
            PointerReleased -= TimelineControl_PointerReleased;
            PointerMoved -= TimelineControl_PointerMoved;

            OnMousePositionChanged -= TimelineControl_OnMousePositionChanged;
        }

        private void Series_CollectionChanged(object? s, NotifyCollectionChangedEventArgs e)
        {           
            foreach (var series in Series)
            {
                series.OnInvalidateData += SeriesInvalidateDataEvent;                              
            }
        }
    
        private void UpdateProperties()
        {
            Window = _area.Window;
            Viewport = _area.Viewport;
            ClientViewport = _area.ClientViewport;
        }

        private void UpdateViewport()
        {
            var maxRight = _seriesViewModels.Max(s => s.MaxTime());

            var rightDate = Begin.AddSeconds(maxRight).Date.AddDays(1);

            Duration = (rightDate - Begin0).TotalSeconds;

            SetViewport();
            
            UpdateProperties();
        }

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
        
        private void SetViewport()
        {
            var d0 = (Begin - Begin0).TotalSeconds;
            var end = Begin0.AddSeconds(d0 + Duration + 86399.0).Date;
            var len = (end - Begin0).TotalSeconds;

            if (_seriesViewModels is not null)
            {
                var count = _seriesViewModels.Count;
                double step = 1.0 / (count + 1);

                int i = 0;
                foreach (var item in _seriesViewModels)
                {
                    if (item is not null)
                    {
                        var series = item;

                        var seriesLocalPostion = new Point2D(0.0, (++i) * step);
                        var seriesAbsolutePostion = _area.FromLocalToAbsolute(seriesLocalPostion);

                        series.LocalPosition = seriesLocalPostion;
                        series.AbsolutePositionX = seriesAbsolutePostion.X;
                        series.AbsolutePositionY = seriesAbsolutePostion.Y;

                        foreach (var ival in series.Intervals)
                        {
                            var intervalLocalPosition = new Point2D(d0 + ival.Left + (ival.Right - ival.Left) / 2.0, series.LocalPosition.Y);
                            var intervalAbsolutePostion = _area.FromLocalToAbsolute(intervalLocalPosition);

                            ival.LocalPosition = intervalLocalPosition;
                            ival.AbsolutePositionX = intervalAbsolutePostion.X;
                            ival.AbsolutePositionY = intervalAbsolutePostion.Y;
                        }
                    }
                }
            }

            _area.ViewportUpdated(0.0, 0.0, len, 1.0);            
        }

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

        protected override Size ArrangeOverride(Size finalSize)
        {
            var actualSize = base.ArrangeOverride(finalSize);
            if (actualSize.Width > 0 && actualSize.Height > 0)
            {
                _area.SizeUpdated((int)Bounds.Width, (int)Bounds.Height);
                //Debug.WriteLine($"TimelineControl -> OnSizeChanged -> Count = {OnSizeChanged?.GetInvocationList().Length}");

                ForceUpdateOverlays();

                UpdateProperties();

                OnSizeChanged?.Invoke(this, EventArgs.Empty);
            }

            return actualSize;
        }

        private void SizeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            _area.SizeUpdated((int)Bounds.Width, (int)Bounds.Height);
                        
            ForceUpdateOverlays();

            UpdateProperties();
                        
            OnSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        //private void TimelineControl_LayoutUpdated(object? sender, EventArgs e)
        //{
        //    _area.SizeUpdated((int)Bounds.Width, (int)Bounds.Height);

        //    OnSizeChanged?.Invoke(this, EventArgs.Empty);

        //    ForceUpdateOverlays();

        //    UpdateProperties();
        //}

        private void ForceUpdateOverlays() => ForceUpdateOverlays(Items);        

        private void ForceUpdateOverlays(IEnumerable items)
        {
            UpdateMarkersOffset();

            foreach (IMarker item in items)
            {
                var p = _area.FromLocalToAbsolute(item.LocalPosition);

                item.AbsolutePositionX = p.X;
                item.AbsolutePositionY = p.Y;
            }

            InvalidateVisual();
        }

        private void ZoomChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is not null && e.NewValue is double value)
            {
                _area.ZoomUpdated((int)value);

                if (IsInitialized == true)
                {
                    ForceUpdateOverlays();
                }

                UpdateProperties();

                OnZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void CurrentTimeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is double)
            {
                var xValue = (Begin - Begin0).TotalSeconds + CurrentTime;
               
                _area.DragToTime(xValue);

                UpdateMarkersOffset();

                UpdateProperties();

                OnDragChanged?.Invoke(this, EventArgs.Empty);
                
                InvalidateVisual();
            }
        }
  
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
         
        public override void Render(DrawingContext context)
        {
            //SeriesValidate();

            //if (_dirtyItems == false)
            {
                DrawBackground(context);
            }
           // else
            {
               // context.FillRectangle(Brushes.LightBlue, new Rect(0, 0, Bounds.Width, Bounds.Height));
            }

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
            var d0 = (Begin - Begin0).TotalSeconds;
            var p = _area.FromLocalToAbsolute(d0, 0.0);    
            Pen pen = new Pen(Brushes.Yellow, 2.0);
            context.DrawLine(pen, new Point(p.X + WindowOffset.X, 0.0), new Point(p.X + WindowOffset.X, _area.Window.Height));
        }

        private void DrawCurrentTime(DrawingContext context)
        {            
            var d0 = (Begin - Begin0).TotalSeconds;
            var p = _area.FromLocalToAbsolute(d0 + CurrentTime, 0.0);
            Pen pen = new Pen(Brushes.Red, 2.0);
            context.DrawLine(pen, new Point(p.X + WindowOffset.X, 0.0), new Point(p.X + WindowOffset.X, _area.Window.Height));
        }        
    }

    //internal class Stuff
    //{
    //    [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
    //    [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
    //    public static extern bool SetCursorPos(int X, int Y);
    //}
}
