using System;
using Avalonia.Controls;
using Avalonia.Input;
using TimeDataViewer;
using TimeDataViewer.Spatial;

namespace FootprintViewerDemo.Controls;

public partial class TimelineControl
{
    private ScreenPoint _mouseDownPoint;

    private void _panel_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (e.Handled || _basePanel == null)
        {
            return;
        }

        e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(_basePanel));
    }

    private void _panel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled || _basePanel == null)
        {
            return;
        }

        Focus();
        e.Pointer.Capture(_basePanel);

        // store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
        _mouseDownPoint = e.GetPosition(_basePanel).ToScreenPoint();

        e.Handled = ActualController.HandleMouseDown(this, e.ToMouseDownEventArgs(_basePanel));
    }

    private void _panel_PointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Handled || _basePanel == null)
        {
            return;
        }

        e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(_basePanel));
    }

    private void _panel_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Handled || _basePanel == null)
        {
            return;
        }

        var releasedArgs = (PointerReleasedEventArgs)e;

        e.Pointer.Capture(null);

        e.Handled = ActualController.HandleMouseUp(this, releasedArgs.ToMouseReleasedEventArgs(_basePanel));

        // Open the context menu
        var p = e.GetPosition(_basePanel).ToScreenPoint();
        var d = p.DistanceTo(_mouseDownPoint);

        if (ContextMenu != null)
        {
            if (Math.Abs(d) < 1e-8 && releasedArgs.InitialPressMouseButton == MouseButton.Right)
            {
                ContextMenu.DataContext = DataContext;
                ContextMenu.IsVisible = true;
            }
            else
            {
                ContextMenu.IsVisible = false;
            }
        }
    }

    private void _panel_PointerEnter(object? sender, PointerEventArgs e)
    {
        base.OnPointerEnter(e);
        if (e.Handled || _basePanel == null)
        {
            return;
        }

        e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(_basePanel));
    }

    private void _panel_PointerLeave(object? sender, PointerEventArgs e)
    {
        base.OnPointerLeave(e);
        if (e.Handled || _basePanel == null)
        {
            return;
        }

        e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(_basePanel));
    }
}
