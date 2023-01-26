using Avalonia.Controls;
using Avalonia.Media;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Extensions;
using TimeDataViewerLite.RenderContext;

namespace TimeDataViewerLite.Controls;

public partial class TimelineControl
{
    private readonly Pen _minorPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _majorPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _minorTickPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private readonly Pen _majorTickPen = new() { Brush = Brush.Parse("#2A2A2A") };
    private Canvas? _backCanvas;
    private DrawCanvas? _drawCanvas;
    private Canvas? _frontCanvas;
    private Canvas? _axisXCanvas;

    public void Draw()
    {
        if (_backCanvas == null || _frontCanvas == null || _axisXCanvas == null || _drawCanvas == null)
        {
            return;
        }

        // Clear the canvas
        _backCanvas.Children.Clear();
        _axisXCanvas.Children.Clear();
        _frontCanvas.Children.Clear();

        RenderBack(_backCanvas);
        RenderSeries(_drawCanvas);
        RenderAxisX(_axisXCanvas, _backCanvas);
    }

    private void RenderBack(Canvas backCanvas)
    {
        ((IPlotModel?)ActualModel)?.Render(backCanvas.Bounds.Width, backCanvas.Bounds.Height);
    }

    private void RenderSeries(DrawCanvas drawCanvas)
    {
        drawCanvas.RenderSeries(ActualModel, SeriesBrushes);
    }

    private void RenderAxisX(Canvas axisXCanvas, Canvas backCanvas)
    {
        if (ActualModel != null)
        {
            var rcAxis = new CanvasRenderContext(axisXCanvas);
            var rcPlotBack = new CanvasRenderContext(backCanvas);
                           
            RenderAxis(ActualModel.AxisX, rcAxis, rcPlotBack);
        }
    }

    private void RenderAxis(Axis? internalAxis, CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
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
            var label = DefaultLabelTemplate.Build(new ContentControl());

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
