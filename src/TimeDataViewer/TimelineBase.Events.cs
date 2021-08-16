using System;
using Avalonia.Controls;
using Avalonia.Input;
using TimeDataViewer.Spatial;

namespace TimeDataViewer
{
    public partial class TimelineBase
    {
        private ScreenPoint _mouseDownPoint;

        //protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        //{
        //    base.OnPointerWheelChanged(e);

        //    if (e.Handled || !IsMouseWheelEnabled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(this));
        //}

        private void _panel_PointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            if (e.Handled /*|| !IsMouseWheelEnabled*/)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(_panel/*this*/));
        }

        //protected override void OnPointerPressed(PointerPressedEventArgs e)
        //{
        //    base.OnPointerPressed(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    Focus();
        //    e.Pointer.Capture(this);

        //    // store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
        //    _mouseDownPoint = e.GetPosition(this).ToScreenPoint();

        //    e.Handled = ActualController.HandleMouseDown(this, e.ToMouseDownEventArgs(this));
        //}

        private void _panel_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Handled)
            {
                return;
            }

            Focus();
            e.Pointer.Capture(_panel/*this*/);

            // store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
            _mouseDownPoint = e.GetPosition(_panel/*this*/).ToScreenPoint();

            e.Handled = ActualController.HandleMouseDown(this, e.ToMouseDownEventArgs(_panel/*this*/));
        }

        //protected override void OnPointerMoved(PointerEventArgs e)
        //{
        //    base.OnPointerMoved(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(this));
        //}

        private void _panel_PointerMoved(object sender, PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(_panel/*this*/));
        }

        //protected override void OnPointerReleased(PointerReleasedEventArgs e)
        //{
        //    base.OnPointerReleased(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    var releasedArgs = (PointerReleasedEventArgs)e;

        //    e.Pointer.Capture(null);

        //    e.Handled = ActualController.HandleMouseUp(this, releasedArgs.ToMouseReleasedEventArgs(this));

        //    // Open the context menu
        //    var p = e.GetPosition(this).ToScreenPoint();
        //    var d = p.DistanceTo(_mouseDownPoint);

        //    if (ContextMenu != null)
        //    {
        //        if (Math.Abs(d) < 1e-8 && releasedArgs.InitialPressMouseButton == MouseButton.Right)
        //        {
        //            ContextMenu.DataContext = DataContext;
        //            ContextMenu.IsVisible = true;
        //        }
        //        else
        //        {
        //            ContextMenu.IsVisible = false;
        //        }
        //    }
        //}

        private void _panel_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.Handled)
            {
                return;
            }

            var releasedArgs = (PointerReleasedEventArgs)e;

            e.Pointer.Capture(null);

            e.Handled = ActualController.HandleMouseUp(this, releasedArgs.ToMouseReleasedEventArgs(_panel/*this*/));

            // Open the context menu
            var p = e.GetPosition(_panel/*this*/).ToScreenPoint();
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

        //protected override void OnPointerEnter(PointerEventArgs e)
        //{
        //    base.OnPointerEnter(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(this));
        //}

        private void _panel_PointerEnter(object sender, PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(_panel/*this*/));
        }

        //protected override void OnPointerLeave(PointerEventArgs e)
        //{
        //    base.OnPointerLeave(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(this));
        //}

        private void _panel_PointerLeave(object sender, PointerEventArgs e)
        {
            base.OnPointerLeave(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(_panel/*this*/));
        }
    }
}
