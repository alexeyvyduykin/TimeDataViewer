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
using TimeDataViewer.ViewModels;
using TimeDataViewer.Models;
using Avalonia.VisualTree;

namespace TimeDataViewer.Views
{
    public class NewIntervalView : Control
    {
        private double _widthX = 0.0;
        //public bool IsChanged = true;           
        private readonly ScaleTransform _scale;
        private IScheduler? _map;
        private IntervalViewModel? _marker;
        private bool _popupIsOpen;

        public NewIntervalView()
        {
          //  PointerEnter += IntervalVisual_PointerEnter;
          //  PointerLeave += IntervalVisual_PointerLeave;

            Initialized += IntervalVisual_Initialized;

            DataContextProperty.Changed.AddClassHandler<NewIntervalView>((d, e) => d.MarkerChanged(e));

            _popupIsOpen = false;

            //    RenderTransform = scale;

            _scale = new ScaleTransform(1, 1);
        }

        public static readonly StyledProperty<Color> BackgroundProperty =
            AvaloniaProperty.Register<NewIntervalView, Color>(nameof(Background), Colors.LightGray);

        public Color Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly StyledProperty<double> HeightYProperty =
            AvaloniaProperty.Register<NewIntervalView, double>(nameof(HeightY), 20.0);

        public double HeightY
        {
            get { return GetValue(HeightYProperty); }
            set { SetValue(HeightYProperty, value); }
        }

        public static readonly StyledProperty<Color> StrokeColorProperty =
            AvaloniaProperty.Register<NewIntervalView, Color>(nameof(StrokeColor), Colors.Black);

        public Color StrokeColor
        {
            get { return GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<NewIntervalView, double>(nameof(StrokeThickness), 1.0);

        public double StrokeThickness
        {
            get { return GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        private void MarkerChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is IntervalViewModel marker)
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

        private IScheduler Map
        {
            get
            {
                if (_map is null)
                {
                    IVisual visual = this;
                    while (visual != null && !(visual is IScheduler))
                    {
                        visual = visual.VisualParent;
                    }

                    _map = visual as IScheduler;
                }

                return _map;
            }
        }


        private void IntervalVisual_Initialized(object? sender, EventArgs e)
        {
            _map = Map;// _marker.Map;

            _map.OnZoomChanged += (s, e) => Update();
            _map.OnSizeChanged += (s, e) => Update();

            InvalidateVisual();
        }

        //private void IntervalVisual_PointerLeave(object? sender, PointerEventArgs e)
        //{
        //    if (_popupIsOpen == true)
        //    {
        //        _map?.HideTooltip();
        //        _popupIsOpen = false;
        //    }

        //    if (_marker is not null)
        //    {
        //        _marker.ZIndex -= 10000;
        //    }

        //    Cursor = new Cursor(StandardCursorType.Arrow);

        //    _scale.ScaleY = 1;
        //}

        //private void IntervalVisual_PointerEnter(object? sender, PointerEventArgs e)
        //{
        //    if (_popupIsOpen == false)
        //    {
        //        var tooltip = Series.Tooltip;
        //        tooltip.DataContext = Series.CreateTooltip(_marker);//  new IntervalTooltipViewModel(_marker);
        //        _map?.ShowTooltip(this, tooltip);
        //        _popupIsOpen = true;
        //    }

        //    if (_marker is not null)
        //    {
        //        _marker.ZIndex += 10000;
        //    }

        //    Cursor = new Cursor(StandardCursorType.Hand);

        //    _scale.ScaleY = 1.5;

        //    // scale.ScaleX = 1;
        //}

        private void Update()
        {
            if (_map is not null && _marker is not null)
            {
                var d1 = _map.FromLocalToAbsolute(new Point2D(_marker.Left, _marker.LocalPosition.Y)).X;
                var d2 = _map.FromLocalToAbsolute(new Point2D(_marker.Right, _marker.LocalPosition.Y)).X;

                _widthX = d2 - d1;
            }

            if (_marker is not null)
            {
                _marker.Offset = new Point2D(-DesiredSize.Width / 2.0, -DesiredSize.Height / 2.0);
            }

            InvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            //  base.Render(drawingContext);

            if (_widthX == 0.0)
                return;

            double thick_half = StrokeThickness / 2.0;

            var p0 = new Point(-_widthX / 2.0, -HeightY / 2.0);
            var p1 = new Point(_widthX / 2.0, HeightY / 2.0);

            var RectBorder = new Rect(
                      new Point(-_widthX / 2.0 + thick_half, -HeightY / 2.0 + thick_half),
                      new Point(_widthX / 2.0 - thick_half, HeightY / 2.0 - thick_half));

            var RectSolid = new Rect(p0, p1);

            var brush = new SolidColorBrush() { Color = Background };
            var pen = new Pen(new SolidColorBrush() { Color = StrokeColor }, StrokeThickness);

            drawingContext.DrawGeometry(brush, null, new RectangleGeometry(RectSolid));
            drawingContext.DrawGeometry(null, pen, new RectangleGeometry(RectBorder));
        }
    }
}
