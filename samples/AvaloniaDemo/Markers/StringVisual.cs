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

namespace AvaloniaDemo.Markers
{
    //public class StringVisual : Control
    //{
    //    private readonly SchedulerString _marker;
    //    private SchedulerControl? _map;      
    //    private double _widthX = 0.0;
    //    private double _heightY = 0.0;

    //    public StringVisual(SchedulerString marker)
    //    {
    //        _marker = marker;
    //        _marker.ZIndex = 30;

    //        LayoutUpdated += StringVisual_LayoutUpdated;
    //        Initialized += StringVisual_Initialized;
            
    //        _heightY = 5;
    //        RenderTransform = new ScaleTransform(1, 1);
    //    }

    //    private void StringVisual_Initialized(object? sender, EventArgs e)
    //    {
    //        _map = _marker.Map;

    //        _map.OnSchedulerZoomChanged += Map_OnMapZoomChanged;
    //        _map.LayoutUpdated += Map_LayoutUpdated;

    //        UpdateWidthString();

    //        base.InvalidateVisual();     
    //    }

    //    private void Map_LayoutUpdated(object? sender, EventArgs e)
    //    {
    //        UpdateWidthString();

    //        base.InvalidateVisual();
    //    }

    //    private void StringVisual_LayoutUpdated(object? sender, EventArgs e)
    //    {
    //        _marker.Offset = new Point2D(-Bounds.Width / 2, -Bounds.Height / 2);
    //    }

    //    private void UpdateWidthString()
    //    {
    //        var left = _map.AbsoluteWindow.Left;
    //        var right = _map.AbsoluteWindow.Right;

    //        _widthX = right - left;
    //    }

    //    private void Map_OnMapZoomChanged()
    //    {
    //        UpdateWidthString();

    //        base.InvalidateVisual();
    //    }

    //    public override void Render(DrawingContext drawingContext)
    //    {
    //        drawingContext.FillRectangle(Brushes.Black,
    //            new Rect(new Point(0, -_heightY / 2.4), new Point(_widthX, _heightY / 2.4)));
    //    }

    //}
}
