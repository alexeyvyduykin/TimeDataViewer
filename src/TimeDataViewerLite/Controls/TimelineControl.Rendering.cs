﻿using Avalonia.Controls;
using Avalonia.Media;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Extensions;
using TimeDataViewerLite.RenderContext;
using TimeDataViewerLite.Spatial;

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

        PlotModel?.UpdateRenderInfo(_backCanvas.Bounds.Width, _backCanvas.Bounds.Height);

        _drawCanvas.RenderSeries(PlotModel);

        RenderAxisX(_axisXCanvas, _backCanvas);
    }

    private void RenderAxisX(Canvas axisXCanvas, Canvas backCanvas)
    {
        if (PlotModel != null)
        {
            var rcAxis = new CanvasRenderContext(axisXCanvas);
            var rcPlotBack = new CanvasRenderContext(backCanvas);

            RenderAxis(PlotModel.AxisX, rcAxis, rcPlotBack);
        }
    }

    private void RenderAxis(DateTimeAxis? axis, CanvasRenderContext contextAxis, CanvasRenderContext contextPlot)
    {
        if (axis == null)
        {
            return;
        }

        if (axis.MinorGridLines is List<ScreenPoint> minorGridLines)
        {
            contextPlot.DrawLineSegments(minorGridLines, _minorPen);
        }

        if (axis.MinorTicks is List<ScreenPoint> minorTicks)
        {
            contextAxis.DrawLineSegments(minorTicks, _minorTickPen);
        }

        if (axis.LabelInfos is List<LabelInfo> infos)
        {
            foreach (var item in infos)
            {
                var label = DefaultLabelTemplate.Build(new ContentControl());

                if (label?.Result is TextBlock textBlock)
                {
                    textBlock.Text = item.Text;
                    textBlock.HorizontalAlignment = item.HorizontalAlignment.ToAvalonia();
                    textBlock.VerticalAlignment = item.VerticalAlignment.ToAvalonia();
                    contextAxis.DrawMathText(item.Position, textBlock);
                }
            }
        }

        if (axis.MajorGridLines is List<ScreenPoint> majorGridLines)
        {
            contextPlot.DrawLineSegments(majorGridLines, _majorPen);
        }

        if (axis.MajorTicks is List<ScreenPoint> majorTicks)
        {
            contextAxis.DrawLineSegments(majorTicks, _majorTickPen);
        }
    }
}
