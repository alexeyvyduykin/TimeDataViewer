using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using System.Windows.Input;
using Avalonia.Media;
using System.Globalization;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.Specialized;
using Avalonia.Controls.Shapes;
using TimeDataViewer.Spatial;
using TimeDataViewer.Markers;

namespace TimeDataViewer.Shapes
{
    public class IntervalVisual : BaseIntervalVisual
    {
        private double _widthX = 0.0;
        private double _heightY = 0.0; 
        //public bool IsChanged = true;       
        private readonly Brush _foreground; 
        private readonly Pen _stroke;
        private readonly ScaleTransform _scale;
        private SchedulerControl? _map;
        private SchedulerInterval? _marker;
        //   public readonly Popup Popup = new Popup();
        //   public readonly IntervalTooltip Tooltip;// = new IntervalTooltip();

        public IntervalVisual()
        {
            //Tooltip = new IntervalTooltip()
            //{
            //    DataContext = new IntervalTooltipViewModel(m),
            //};

            // Popup.AllowsTransparency = true;
            //      Popup.PlacementTarget = this;
            //      Popup.PlacementMode = PlacementMode.Pointer;
            //      Popup.Child = Tooltip;
            //      Popup.Child.Opacity = 0.777;

            PointerEnter += IntervalVisual_PointerEnter;
            PointerLeave += IntervalVisual_PointerLeave;

            LayoutUpdated += IntervalVisual_LayoutUpdated;
            Initialized += IntervalVisual_Initialized;

            DataContextProperty.Changed.AddClassHandler<IntervalVisual>(MarkerChanged);

            //    RenderTransform = scale;
            
            _scale = new ScaleTransform(1, 1);
            _stroke = new Pen(Brushes.Black, 1.0);
            _foreground = new SolidColorBrush() { Color = Colors.White };
         
            _heightY = 20;
        }

        public static readonly StyledProperty<Color> BackgroundProperty =    
            AvaloniaProperty.Register<IntervalVisual, Color>(nameof(Background), Colors.LightGray);

        public Color Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private void MarkerChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is SchedulerInterval marker)
            {
                //if(e.OldValue is not null && e.OldValue is SchedulerInterval oldMarker)
                //{
                //    _map.OnSchedulerZoomChanged -= Map_OnMapZoomChanged;
                //    _map.LayoutUpdated -= Map_LayoutUpdated;
                //}
                
                _marker = marker;
                _marker.ZIndex = 100;

                //_map = _marker.Map;
                //_map.OnSchedulerZoomChanged += Map_OnMapZoomChanged;
                //_map.LayoutUpdated += Map_LayoutUpdated;
            }
        }

        private void IntervalVisual_Initialized(object? sender, EventArgs e)
        {
            _map = _marker.Map;
            _map.OnSchedulerZoomChanged += Map_OnMapZoomChanged;
            _map.LayoutUpdated += Map_LayoutUpdated;


            UpdateInterval();
            InvalidateVisual();
        }

        private void Map_LayoutUpdated(object? sender, EventArgs e)
        {
            UpdateInterval();
            InvalidateVisual();          
        }

        private void IntervalVisual_LayoutUpdated(object? sender, EventArgs e)
        {
            if (_marker is not null)
            {
                _marker.Offset = new Point2D(-DesiredSize.Width / 2, -DesiredSize.Height / 2);
            }
        }

        private void IntervalVisual_PointerLeave(object? sender, PointerEventArgs e)
        {
            //if (Popup.IsOpen)
            //{
            //    Popup.IsOpen = false;
            //}

            if (_marker is not null)
            {
                _marker.ZIndex -= 10000;
            }

            Cursor = new Cursor(StandardCursorType.Arrow);

            _scale.ScaleY = 1;
            //  scale.ScaleX = 1;
        }

        private void IntervalVisual_PointerEnter(object? sender, PointerEventArgs e)
        {
            //if (Popup.IsOpen == false)
            //{
            //    Popup.IsOpen = true;

            //    // Popup.InvalidateVisual();
            //}

            if (_marker is not null)
            {
                _marker.ZIndex += 10000;
            }
            
            Cursor = new Cursor(StandardCursorType.Hand);

            _scale.ScaleY = 1.5;
   
            // scale.ScaleX = 1;
        }

        private void Map_OnMapZoomChanged()
        {
            UpdateInterval();


            InvalidateVisual();
            //  UpdateVisual(true);
        }

        private void UpdateInterval()
        {
            //   var d1 = Map.FromLocalValueToPixelX(Marker.Left);
            //   var d2 = Map.FromLocalValueToPixelX(Marker.Right);
            if (_map is not null && _marker is not null)
            {
                var d1 = _map.FromLocalToAbsolute(new Point2D(_marker.Left, _marker.LocalPosition.Y)).X;
                var d2 = _map.FromLocalToAbsolute(new Point2D(_marker.Right, _marker.LocalPosition.Y)).X;

                //    var d1 = Map.FromSchedulerPointToLocal(new SCSchedulerPoint(Marker.Left, Marker.Position.Y)).X;
                //    var d2 = Map.FromSchedulerPointToLocal(new SCSchedulerPoint(Marker.Right, Marker.Position.Y)).X;

                _widthX = d2 - d1;
            }
        }
        
        public override void Render(DrawingContext drawingContext)
        {
            //  base.Render(drawingContext);

            if (_widthX == 0.0)
                return;

            double thick_half = _stroke.Thickness / 2.0;

            var p0 = new Point(-_widthX / 2.0, -_heightY / 2.0);
            var p1 = new Point(_widthX / 2.0, _heightY / 2.0);

            var RectBorder = new Rect(
                      new Point(-_widthX / 2.0 + thick_half, -_heightY / 2.0 + thick_half),
                      new Point(_widthX / 2.0 - thick_half, _heightY / 2.0 - thick_half));

            var RectSolid = new Rect(p0, p1);
                        
            var brush = new SolidColorBrush() { Color = Background };

            drawingContext.DrawGeometry(brush, null, new RectangleGeometry(RectSolid));                      
            drawingContext.DrawGeometry(null, _stroke, new RectangleGeometry(RectBorder));                            
        }

        public override BaseIntervalVisual Clone(SchedulerInterval marker)
        {
            return new IntervalVisual() 
            {
                DataContext = marker,
                Background = Background,
            };
        }
    }
}
