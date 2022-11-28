using Avalonia.Controls;
using Avalonia.Media;
using TimeDataViewer;
using TimeDataViewer.Core;

namespace FootprintViewerDemo.Controls;

internal class Renderer
{
    private readonly TimelineControl _control;
    private readonly Pen _minorPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _majorPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _minorTickPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _majorTickPen = new() { Brush = Brush.Parse("#2A2A2A") };

    public Renderer(TimelineControl control)
    {
        _control = control;
    }

    public Canvas? BackCanvas { get; set; }

    public DrawCanvas? DrawCanvas { get; set; }

    public Canvas? FrontCanvas { get; set; }

    public Canvas? AxisXCanvas { get; set; }

    public Canvas? OvarlayCanvas { get; set; }

    public void Draw()
    {
        if (BackCanvas == null || FrontCanvas == null || AxisXCanvas == null || DrawCanvas == null)
        {
            return;
        }

        // Clear the canvas
        BackCanvas.Children.Clear();
        AxisXCanvas.Children.Clear();
        FrontCanvas.Children.Clear();

        RenderBack(BackCanvas);
        RenderSeries(DrawCanvas);
        RenderAxisX(AxisXCanvas, BackCanvas);
        RenderSlider(AxisXCanvas, FrontCanvas);
    }

    private void RenderBack(Canvas backCanvas)
    {
        ((IPlotModel?)_control.ActualModel)?.Render(backCanvas.Bounds.Width, backCanvas.Bounds.Height);
    }

    private void RenderSeries(DrawCanvas drawCanvas)
    {
        drawCanvas.RenderSeries(_control.ActualModel);
    }

    private void RenderAxisX(Canvas axisXCanvas, Canvas backCanvas)
    {
        if (_control.ActualModel != null)
        {
            var rcAxis = new CanvasRenderContext(axisXCanvas);
            var rcPlotBack = new CanvasRenderContext(backCanvas);

            foreach (var item in _control.ActualModel.Axises)
            {
                if (item.IsHorizontal() == true)
                {
                    RenderAxis(item, rcAxis, rcPlotBack);
                }
            }
        }
    }

    private void RenderSlider(Canvas axisXCanvas, Canvas frontCanvas)
    {
        var rcAxis = new CanvasRenderContext(axisXCanvas);
        var rcPlotFront = new CanvasRenderContext(frontCanvas);

        if (_control.ActualModel != null)
        {
            _control.Slider?.UpdateMinMax(_control.ActualModel);
        }

        _control.Slider?.Render(rcAxis, rcPlotFront);
    }

    private void RenderAxis(TimeDataViewer.Core.Axis? internalAxis, CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        if (internalAxis == null)
        {
            return;
        }

        var labels = internalAxis.MyLabels;
        var minorSegments = internalAxis.MyMinorSegments;
        var minorTickSegments = internalAxis.MyMinorTickSegments;
        var majorSegments = internalAxis.MyMajorSegments;
        var majorTickSegments = internalAxis.MyMajorTickSegments;

        if (_minorPen != null)
        {
            contextPlot.DrawLineSegments(minorSegments, _minorPen);
        }

        if (_minorTickPen != null)
        {
            contextAxis.DrawLineSegments(minorTickSegments, _minorTickPen);
        }

        foreach (var (pt, text, ha, va) in labels)
        {
            var label = _control.DefaultLabelTemplate.Build(new ContentControl());

            if (label.Control is TextBlock textBlock)
            {
                textBlock.Text = text;
                textBlock.HorizontalAlignment = ha.ToAvalonia();
                textBlock.VerticalAlignment = va.ToAvalonia();
                contextAxis.DrawMathText(pt, textBlock);
            }
        }

        if (_majorPen != null)
        {
            contextPlot.DrawLineSegments(majorSegments, _majorPen);
        }

        if (_majorTickPen != null)
        {
            contextAxis.DrawLineSegments(majorTickSegments, _majorTickPen);
        }

        //if(MinMaxBrush != null && InternalAxis.IsHorizontal() == true)
        //{
        //    var rect0 = InternalAxis.LeftRect;
        //    var rect1 = InternalAxis.RightRect;

        //    if (rect0.Width != 0)
        //    {
        //      //  contextPlot.DrawRectangle(rect0, MinMaxBrush, null);
        //      //  contextPlot.DrawLine(rect0.Right, rect0.Top, rect0.Right, rect0.Bottom, BlackPen);
        //    }

        //    if (rect1.Width != 0)
        //    {
        //      //  contextPlot.DrawRectangle(rect1, MinMaxBrush, null);
        //      //  contextPlot.DrawLine(rect1.Left, rect1.Top, rect1.Left, rect1.Bottom, BlackPen);
        //    }
        //}
    }
}
