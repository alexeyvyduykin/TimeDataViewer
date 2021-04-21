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
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Shapes
{
    public class StringVisual : Control
    {
        private readonly SeriesViewModel _marker;
        private SchedulerControl? _map;      
        private double _widthX = 0.0;
      
        public StringVisual(SeriesViewModel marker)
        {
            _marker = marker;
            _marker.ZIndex = 30;
      
            Initialized += StringVisual_Initialized;
      
            RenderTransform = new ScaleTransform(1, 1);
        }

        public static readonly StyledProperty<double> HeightYProperty =    
            AvaloniaProperty.Register<IntervalVisual, double>(nameof(HeightY), 2.0);

        public double HeightY
        {
            get { return GetValue(HeightYProperty); }
            set { SetValue(HeightYProperty, value); }
        }

        private void StringVisual_Initialized(object? sender, EventArgs e)
        {     
            _map = _marker.Map;

            _map.OnZoomChanged += (s, e) => Update(s, e);
            _map.LayoutUpdated += (s, e) => Update(s, e);
       
            base.InvalidateVisual();     
        }

        protected void Update(object? sender, EventArgs e)
        {
            if (_map is not null)
            {
                var left = _map.AbsoluteWindow.Left;
                var right = _map.AbsoluteWindow.Right;

                _widthX = right - left;
            }

            if (_marker is not null)
            {
                _marker.Offset = new Point2D(-Bounds.Width / 2.0, -Bounds.Height / 2.0);
            }

            base.InvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            drawingContext.FillRectangle(Brushes.Black,
                new Rect(new Point(0, -HeightY / 2.0), new Point(_widthX, HeightY / 2.0)));
        }

    }
}
