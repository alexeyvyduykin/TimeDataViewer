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
using Avalonia.VisualTree;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;
using TimeDataViewer.Models;

namespace TimeDataViewer.Views
{
    public class NewSeriesView : Control
    {
        private SeriesViewModel _marker;
        private IScheduler? _map;
        private double _widthX = 0.0;

        public NewSeriesView()
        {
            Initialized += StringVisual_Initialized;

            DataContextProperty.Changed.AddClassHandler<NewSeriesView>((d, e) => d.MarkerChanged(e));

            RenderTransform = new ScaleTransform(1, 1);
        }

        public static readonly StyledProperty<double> HeightYProperty =
            AvaloniaProperty.Register<NewSeriesView, double>(nameof(HeightY), 2.0);

        public double HeightY
        {
            get { return GetValue(HeightYProperty); }
            set { SetValue(HeightYProperty, value); }
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

        private void StringVisual_Initialized(object? sender, EventArgs e)
        {
            _map = Map;// _marker.Map;

            _map.OnZoomChanged += (s, e) => Update(s, e);
            _map.OnLayoutUpdated += (s, e) => Update(s, e);

            base.InvalidateVisual();
        }

        private void MarkerChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is SeriesViewModel series)
            {
                //if(e.OldValue is not null && e.OldValue is SchedulerInterval oldMarker)
                //{
                //    _map.OnSchedulerZoomChanged -= Map_OnMapZoomChanged;
                //    _map.LayoutUpdated -= Map_LayoutUpdated;
                //}

                _marker = series;
                _marker.ZIndex = 30;

                //_map = _marker.Map;
                //_map.OnSchedulerZoomChanged += Map_OnMapZoomChanged;
                //_map.LayoutUpdated += Map_LayoutUpdated;
            }
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
