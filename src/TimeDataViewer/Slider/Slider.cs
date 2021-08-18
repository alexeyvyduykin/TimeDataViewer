using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using TimeDataViewer.Spatial;
using A = Avalonia.Layout;

namespace TimeDataViewer
{
    public class Slider : Control
    {
        public static readonly StyledProperty<double> CurrentPositionProperty = 
            AvaloniaProperty.Register<Slider, double>(nameof(CurrentPosition), 0.0);
        public static readonly StyledProperty<double> BeginProperty = 
            AvaloniaProperty.Register<Slider, double>(nameof(Begin), 0.0);
        public static readonly StyledProperty<double> DurationProperty = 
            AvaloniaProperty.Register<Slider, double>(nameof(Duration), 0.0);
        public static readonly StyledProperty<IBrush> MinMaxBrushProperty =   
            AvaloniaProperty.Register<Slider, IBrush>(nameof(MinMaxBrush), new SolidColorBrush());

        private OxyRect _leftRect;
        private OxyRect _rightRect;

        static Slider()
        {
            ClipToBoundsProperty.OverrideDefaultValue<Slider>(false);
            DurationProperty.Changed.AddClassHandler<Slider>(AppearanceChanged);
            MinMaxBrushProperty.Changed.AddClassHandler<Slider>(AppearanceChanged);
        }
        
        public IBrush MinMaxBrush
        {
            get
            {
                return GetValue(MinMaxBrushProperty);
            }

            set
            {
                SetValue(MinMaxBrushProperty, value);
            }
        }

        private Pen BlackPen { get; set; } = new Pen() { Brush = Brushes.Black, Thickness = 1 };
        
        public double CurrentPosition
        {
            get
            {
                return GetValue(CurrentPositionProperty);
            }

            set
            {
                SetValue(CurrentPositionProperty, value);
            }
        }

        public double Begin
        {
            get
            {
                return GetValue(BeginProperty);
            }

            set
            {
                SetValue(BeginProperty, value);
            }
        }

        public double Duration
        {
            get
            {
                return GetValue(DurationProperty);
            }

            set
            {
                SetValue(DurationProperty, value);
            }
        }
        
        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Slider)d).OnVisualChanged();
        }

        protected void OnVisualChanged()
        {
            if (Parent is Core.IPlotView pc)
            {
                pc.InvalidatePlot(false);
            }
        }

        public void UpdateMinMax(Core.PlotModel plotModel)
        {
            _leftRect = new OxyRect();
            _rightRect = new OxyRect();

            if (plotModel == null)
            {
                return;
            }
            
            plotModel.GetAxesFromPoint(out Core.Axis axisX, out _);

            if (axisX == null)
            {
                return;
            }
                
            double h = plotModel.PlotArea.Height;

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (Core.XYAxisSeries item in plotModel.Series)
            {
                if (item.IsVisible == true)
                {
                    min = Math.Min(min, item.MinX);
                    max = Math.Max(max, item.MaxX);
                }
            }

            var p0 = axisX.Transform(axisX.AbsoluteMinimum);
            var p1 = axisX.Transform(Begin);
            var p2 = axisX.Transform(Begin + Duration);
            var p3 = axisX.Transform(axisX.AbsoluteMaximum);

            var w0 = p1 - p0;
            var w1 = p3 - p2;

            if (w0 > 0)
            {
                _leftRect = new OxyRect(p0, 0, p1 - p0, h);
            }

            if (w1 > 0)
            {
                _rightRect = new OxyRect(p2, 0, p3 - p2, h);
            }
        }

        public virtual void Render(CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
        {
            if (MinMaxBrush != null)
            {
                if (_leftRect.Width != 0)
                {
                    contextPlot.DrawRectangle(_leftRect, MinMaxBrush, null);
                    contextPlot.DrawLine(_leftRect.Right, _leftRect.Top, _leftRect.Right, _leftRect.Bottom, BlackPen);
                }

                if (_rightRect.Width != 0)
                {
                    contextPlot.DrawRectangle(_rightRect, MinMaxBrush, null);
                    contextPlot.DrawLine(_rightRect.Left, _rightRect.Top, _rightRect.Left, _rightRect.Bottom, BlackPen);
                }
            }
        }
    }
}
