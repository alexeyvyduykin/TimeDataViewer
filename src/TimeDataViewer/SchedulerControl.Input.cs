using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Visuals;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TimeDataViewer.ViewModels;
using TimeDataViewer.Core;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Input.TextInput;

namespace TimeDataViewer
{
    public delegate void MousePositionChangedEventHandler(Point2D point);

    public partial class SchedulerControl
    {
        private Cursor? _cursorBefore;
        private int _onMouseUpTimestamp = 0;
        private Point2D _mousePosition = new();
        private bool _isDragging = false;
        private Point2D _mouseDown;

        public bool IgnoreMarkerOnMouseWheel { get; set; } = true;

        public Point2D MousePosition
        {
            get => _mousePosition;
            protected set
            {
                _mousePosition = value;
                OnMousePositionChanged?.Invoke(_mousePosition);
            }
        }

        private void SchedulerControl_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                var p = e.GetPosition(this);

                _mouseDown = new Point2D(p.X, p.Y);
            }
        }

        private void SchedulerControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_internalModel.IsDragging == true)
            {
                if (_isDragging == true)
                {
                    _onMouseUpTimestamp = (int)e.Timestamp & int.MaxValue;
                    _isDragging = false;

                    if (_cursorBefore == null)
                    {
                        _cursorBefore = new(StandardCursorType.Arrow);
                    }

                    Cursor = _cursorBefore;
                    e.Pointer.Capture(null);
                }
                _internalModel.EndDrag();
                _mouseDown = Point2D.Empty;
            }
            else
            {
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
                {
                    _mouseDown = Point2D.Empty;
                }

                InvalidateVisual();
            }
        }

        private void SchedulerControl_PointerMoved(object? sender, PointerEventArgs e)
        {
            var MouseScreenPosition = e.GetPosition(this);

            _internalModel.ZoomScreenPosition = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            MousePosition = _internalModel.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            // wpf generates to many events if mouse is over some visual and OnMouseUp is fired, wtf, anyway...         
            if (((int)e.Timestamp & int.MaxValue) - _onMouseUpTimestamp < 55)
            {
                return;
            }

            if (_internalModel.IsDragging == false && _mouseDown.IsEmpty == false)
            {
                // cursor has moved beyond drag tolerance
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
                {
                    if (Math.Abs(MouseScreenPosition.X - _mouseDown.X) * 2 >= 2 ||
                        Math.Abs(MouseScreenPosition.Y - _mouseDown.Y) * 2 >= 2)
                    {
                        _internalModel.BeginDrag(_mouseDown);
                    }
                }
            }

            if (_internalModel.IsDragging == true)
            {
                if (_isDragging == false)
                {
                    _isDragging = true;
                    _cursorBefore = Cursor;
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    e.Pointer.Capture(this);
                }

                var mouseCurrent = new Point2D(MouseScreenPosition.X, MouseScreenPosition.Y);

                _internalModel.Drag(mouseCurrent);

                InvalidateVisual();
            }
        }

        //private Point2D _mouseDownPoint;

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(this));
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

        //protected override void OnPointerMoved(PointerEventArgs e)
        //{
        //    base.OnPointerMoved(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(this));
        //}

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

        //protected override void OnPointerEnter(PointerEventArgs e)
        //{
        //    base.OnPointerEnter(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(this));
        //}

        //protected override void OnPointerLeave(PointerEventArgs e)
        //{
        //    base.OnPointerLeave(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(this));
        //}
    }
}
