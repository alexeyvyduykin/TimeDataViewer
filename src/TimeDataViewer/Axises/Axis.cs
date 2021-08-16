using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TimeDataViewer
{
    public abstract partial class Axis : Control
    {
        public Core.Axis InternalAxis { get; protected set; }
        protected Pen AxislinePen { get; set; }
        protected Pen ExtraPen { get; set; }
        protected Pen MajorPen { get; set; }
        protected Pen MajorTickPen { get; set; }
        protected Pen MinorPen { get; set; }
        protected Pen MinorTickPen { get; set; }
        protected Pen ZeroPen { get; set; }

        protected Axis()
        {
            MinorPen = new Pen() { Brush = new SolidColorBrush() { Color = Color.FromArgb(0x20, 0, 0, 0x00) }, Thickness = 1, DashStyle = DashStyle.Dot };
            MajorPen = new Pen() { Brush = new SolidColorBrush() { Color = Color.FromArgb(0x40, 0, 0, 0) }, Thickness = 1 };
            MinorTickPen = new Pen() { Brush = Brushes.Black, Thickness = 1 };
            MajorTickPen = new Pen() { Brush = Brushes.Black, Thickness = 1 };
            ZeroPen = new Pen() { Brush = Brushes.Black, Thickness = 1 };
            ExtraPen = new Pen() { Brush = Brushes.Black, Thickness = 1, DashStyle = DashStyle.Dot };
            AxislinePen = new Pen() { Brush = Brushes.Black, Thickness = 1 };
        }

        public abstract Core.Axis CreateModel();

        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Axis)d).OnVisualChanged();
        }

        protected static void DataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Axis)d).OnDataChanged();
        }

        protected void OnDataChanged()
        {
            if (Parent is Core.IPlotView pc)
            {
                pc.InvalidatePlot();
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.OwnerType == GetType())
            {
                if (Parent is Core.IPlotView plot)
                {
                    plot.InvalidatePlot();
                }
            }
        }

        protected void OnVisualChanged()
        {
            var pc = Parent as Core.IPlotView;
            if (pc != null)
            {
                pc.InvalidatePlot(false);
            }
        }

        protected virtual void SynchronizeProperties()
        {
            var a = InternalAxis;
            a.AbsoluteMaximum = AbsoluteMaximum;
            a.AbsoluteMinimum = AbsoluteMinimum;
            a.AxisDistance = AxisDistance;
            a.AxisTickToLabelDistance = AxisTickToLabelDistance;
            a.ExtraGridlines = ExtraGridlines;
            a.IntervalLength = IntervalLength;
            a.IsPanEnabled = IsPanEnabled;
            a.IsAxisVisible = IsAxisVisible;
            a.IsZoomEnabled = IsZoomEnabled;
            a.Key = Key;
            a.MajorStep = MajorStep;
            a.MajorTickSize = MajorTickSize;
            a.MinorStep = MinorStep;
            a.MinorTickSize = MinorTickSize;
            a.Minimum = Minimum;
            a.Maximum = Maximum;
            a.MinimumRange = MinimumRange;
            a.MaximumRange = MaximumRange;
            //   a.MinimumPadding = MinimumPadding;
            //   a.MaximumPadding = MaximumPadding;
            a.Position = Position;
            a.StringFormat = StringFormat;
            //a.ToolTip = ToolTip.GetTip(this) != null ? ToolTip.GetTip(this).ToString() : null;
        }

        public virtual void MyRender(Canvas canvasAxis, Canvas canvasPlot)
        {
            if (InternalAxis == null)
            {
                return;
            }

            var rcAxis = new CanvasRenderContext(canvasAxis);
            var rcPlot = new CanvasRenderContext(canvasPlot);

            var labels = InternalAxis.MyLabels;
            var minorSegments = InternalAxis.MyMinorSegments;
            var minorTickSegments = InternalAxis.MyMinorTickSegments;
            var majorSegments = InternalAxis.MyMajorSegments;
            var majorTickSegments = InternalAxis.MyMajorTickSegments;

            if (MinorPen != null)
            {
                rcPlot.DrawLineSegments(minorSegments, MinorPen);
            }

            if (MinorTickPen != null)
            {
                rcAxis.DrawLineSegments(minorTickSegments, MinorTickPen);
            }

            foreach (var (pt, text, ha, va) in labels)
            {
                rcAxis.DrawMathText(pt, text, ha, va);
            }

            if (MajorPen != null)
            {
                rcPlot.DrawLineSegments(majorSegments, MajorPen);
            }

            if (MajorTickPen != null)
            {
                rcAxis.DrawLineSegments(majorTickSegments, MajorTickPen);
            }
        }
    }
}
