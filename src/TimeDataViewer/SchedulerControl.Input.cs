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
    public partial class SchedulerControl
    {
        private bool _isSelected = false;
        private Point2D _selectionStart;
        private Point2D _selectionEnd;
        private Cursor? _cursorBefore = new(StandardCursorType.Arrow);
        private int _onMouseUpTimestamp = 0;
        private Point2D _mousePosition = new();
        private bool _disableAltForSelection = false;
        private bool _isDragging = false;

        public event SCPositionChanged? OnMousePositionChanged;  
        public event EventHandler OnSchedulerDragChanged
        {
            add
            {
                _area.OnDragChanged += value;
            }
            remove
            {
                _area.OnDragChanged -= value;
            }
        }
        public event EventHandler OnZoomChanged
        {
            add
            {
                _area.OnZoomChanged += value;
            }
            remove
            {
                _area.OnZoomChanged -= value;
            }
        }

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

        private void SchedulerControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (IgnoreMarkerOnMouseWheel == true && _area.IsDragging == false)
            {
                Zoom = (e.Delta.Y > 0) ? ((int)Zoom) + 1 : ((int)(Zoom + 0.99)) - 1;
                
                var ps = (this as Visual).PointToScreen(new Point(_area.ZoomScreenPosition.X, _area.ZoomScreenPosition.Y));

                Stuff.SetCursorPos((int)ps.X, (int)ps.Y);
            }
        }

        private void SchedulerControl_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                var p = e.GetPosition(this);

                _area.MouseDown = new Point2I((int)p.X, (int)p.Y);
            
                base.InvalidateVisual();
            }
            else if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            {
                if (_isSelected == false)
                {
                    var p = e.GetPosition(this);
                    _isSelected = true;                  
                    _selectionEnd = default;//Point2D.Empty;
                    _selectionStart = FromScreenToLocal((int)p.X, (int)p.Y);
                }
            }
        }

        private void SchedulerControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isSelected == true)
            {
                _isSelected = false;
            }

            if (_area.IsDragging == true)
            {
                if (_isDragging == true)
                {
                    _onMouseUpTimestamp = (int)e.Timestamp & int.MaxValue;
                    _isDragging = false;            
                    base.Cursor = _cursorBefore;               
                    e.Pointer.Capture(null);
                }
                _area.EndDrag();
            }
            else
            {
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true/*ChangedButton == MouseButton.Right*/)
                {
                    _area.MouseDown = default;//Point2I.Empty;
                }

                if (_selectionEnd.IsEmpty() == false && _selectionStart.IsEmpty() == false)
                {                                 
                    _selectionEnd = default;// Point2D.Empty;               
                }
                else
                {
                    InvalidateVisual();
                }
            }
        }

        private void SchedulerControl_PointerMoved(object? sender, PointerEventArgs e)
        {
            var MouseScreenPosition = e.GetPosition(this);

            //if (_area.IsWindowArea(MouseAbsolutePosition) == true)
            {
                _area.ZoomScreenPosition = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);
                //  base.InvalidateVisual();
            }

            MousePosition = _area.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            // wpf generates to many events if mouse is over some visual and OnMouseUp is fired, wtf, anyway...         
            if (((int)e.Timestamp & int.MaxValue) - _onMouseUpTimestamp < 55)
            {              
                return;
            }

            if (_area.IsDragging == false && _area.MouseDown.IsEmpty == false)
            {
                // cursor has moved beyond drag tolerance
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
                {
                    if (Math.Abs(MouseScreenPosition.X - _area.MouseDown.X) * 2 >= 2/*SystemParameters.MinimumHorizontalDragDistance*/ ||
                        Math.Abs(MouseScreenPosition.Y - _area.MouseDown.Y) * 2 >= 2/*SystemParameters.MinimumVerticalDragDistance*/)
                    {
                        _area.BeginDrag(_area.MouseDown);
                    }
                }

            }

            if (_area.IsDragging == true)
            {
                if (_isDragging == false)
                {
                    _isDragging = true;           
                    _cursorBefore = base.Cursor;
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);// Cursors.SizeWE;// SizeAll;
                    e.Pointer.Capture(this);
                    //Mouse.Capture(this);
                }

                _area.MouseCurrent = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);
               
                _area.Drag(_area.MouseCurrent);

                UpdateMarkersOffset();

                base.InvalidateVisual();
            }
            else
            {
                if (_isSelected && _selectionStart.IsEmpty() == false &&
                    (e.KeyModifiers == KeyModifiers.Shift /*Keyboard.Modifiers == ModifierKeys.Shift*/ ||
                    e.KeyModifiers == KeyModifiers.Alt /*Keyboard.Modifiers == ModifierKeys.Alt*/ ||
                    _disableAltForSelection))
                {
                    _selectionEnd = _area.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);
                }
            }
        }
    }
}
