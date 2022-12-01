using System;
using Avalonia.Controls;
using Avalonia.Input;
using TimeDataViewer.Core;
using TimeDataViewer.Extensions;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Controls;

public partial class TimelineControl
{
    private ScreenPoint _mouseDownPoint;
    private bool _isPressed = false;
    private Core.Series? _currentSeries;
    private ScreenPoint _previousPosition;
    private bool _isPanEnabled;
    private OxyRect _zoomRectangle;
    private bool _isZoomEnabled;

    protected Core.Axis? XAxis { get; set; }

    protected Core.Axis? YAxis { get; set; }

    protected DataPoint InverseTransform(double x, double y)
    {
        if (XAxis != null)
        {
            return XAxis.InverseTransform(x, y, YAxis!);
        }

        if (YAxis != null)
        {
            return new DataPoint(0, YAxis.InverseTransform(y));
        }

        return new DataPoint();
    }

    protected void AssignAxes()
    {
        Core.Axis? xAxis = null;
        Core.Axis? yAxis = null;

        ActualModel?.GetAxesFromPoint(out xAxis, out yAxis);

        XAxis = xAxis;
        YAxis = yAxis;
    }

    protected static TrackerHitResult? GetNearestHit(Core.Series? series, ScreenPoint point, bool snap, bool pointsOnly)
    {
        if (series == null)
        {
            return null;
        }

        // Check data points only
        if (snap || pointsOnly)
        {
            var result = series.GetNearestPoint(point, false);
            if (result != null)
            {
                if (result.Position.DistanceTo(point) < 20)
                {
                    return result;
                }
            }
        }

        // Check between data points (if possible)
        if (!pointsOnly)
        {
            var result = series.GetNearestPoint(point, true);
            return result;
        }

        return null;
    }

    protected static TrackerHitResult? GetNearestHit(Core.Series? series, ScreenPoint point)
    {
        if (series == null)
        {
            return null;
        }

        var result = series.GetNearestPoint(point, false);
        if (result != null)
        {
            if (result.Position.DistanceTo(point) < 20)
            {
                return result;
            }
        }

        return null;
    }

    private CursorType GetZoomCursor()
    {
        if (XAxis == null)
        {
            return CursorType.ZoomVertical;
        }

        if (YAxis == null)
        {
            return CursorType.ZoomHorizontal;
        }

        return CursorType.ZoomRectangle;
    }

    private void _panel_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (_basePanel == null)
        {
            return;
        }

        AssignAxes();

        var position = e.GetPosition(_basePanel).ToScreenPoint();
        var delta = (int)(e.Delta.Y + e.Delta.X) * 120;
        var factor = 1;
        var step = delta * 0.001 * factor;

        var isZoomEnabled = (XAxis != null && XAxis.IsZoomEnabled) || (YAxis != null && YAxis.IsZoomEnabled);

        if (isZoomEnabled == false)
        {
            return;
        }

        var current = InverseTransform(position.X, position.Y);

        var scale = 1 + step;

        // make sure the zoom factor is not negative
        if (scale < 0.1)
        {
            scale = 0.1;
        }

        XAxis?.ZoomAt(scale, current.X);

        YAxis?.ZoomAt(scale, current.Y);

        InvalidatePlot(false);
    }

    private void _panel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_basePanel == null)
        {
            return;
        }

        Focus();
        e.Pointer.Capture(_basePanel);

        // store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
        _mouseDownPoint = e.GetPosition(_basePanel).ToScreenPoint();

        var changedButton = e.GetCurrentPoint(_basePanel).Properties.PointerUpdateKind;

        _ = changedButton switch
        {
            PointerUpdateKind.LeftButtonPressed => OnLeftButtonMouseDown(e),
            PointerUpdateKind.MiddleButtonPressed => OnMiddleButtonMouseDown(e),
            PointerUpdateKind.RightButtonPressed => OnRightButtonMouseDown(e),
            _ => false
        };
    }

    private bool OnLeftButtonMouseDown(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(_basePanel).ToScreenPoint();

        if (ActualModel == null)
        {
            return false;
        }

        foreach (var item in ActualModel.Series)
        {
            if (item is Core.TimelineSeries series)
            {
                series.ResetSelecIndex();
            }
        }

        if (ActualModel.PlotArea.Contains(position.X, position.Y) == false)
        {
            return false;
        }

        var currentSeries = ActualModel.GetSeriesFromPoint(position);

        var result = GetNearestHit(currentSeries, position);

        if (result != null)
        {
            if (currentSeries is Core.TimelineSeries series)
            {
                series.SelectIndex((int)result.Index);

                InvalidatePlot(false);

                OnSelectedInterval(result);
            }
        }

        return true;
    }

    private bool OnRightButtonMouseDown(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(_basePanel).ToScreenPoint();

        _previousPosition = position;

        _isPanEnabled = (XAxis != null && XAxis.IsPanEnabled)
                     || (YAxis != null && YAxis.IsPanEnabled);

        if (_isPanEnabled == true)
        {
            SetCursorType(CursorType.Pan);
            HideTracker();
        }

        return _isPanEnabled;
    }

    private bool OnMiddleButtonMouseDown(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(_basePanel).ToScreenPoint();

        _isZoomEnabled = (XAxis != null && XAxis.IsZoomEnabled)
                      || (YAxis != null && YAxis.IsZoomEnabled);

        if (_isZoomEnabled == true)
        {
            _zoomRectangle = new OxyRect(position.X, position.Y, 0, 0);

            ShowZoomRectangle(_zoomRectangle);

            SetCursorType(GetZoomCursor());

            HideTracker();
        }

        return _isZoomEnabled;
    }

    private void _panel_PointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_basePanel == null || ActualModel == null)
        {
            return;
        }

        if (_isPanEnabled == true)
        {
            OnRightButtonPressedMouseMoved(e);
            return;
        }

        if (_isZoomEnabled == true)
        {
            OnLeftButtonPressedMouseMoved(e);
            return;
        }

        var position = e.GetPosition(_basePanel).ToScreenPoint();
        var snap = true;
        var pointsOnly = false;

        _currentSeries = ActualModel.GetSeriesFromPoint(position, 20);

        if (_currentSeries == null)
        {
            HideTracker();

            return;
        }

        if (ActualModel.PlotArea.Contains(position.X, position.Y) == false)
        {
            return;
        }

        var result = GetNearestHit(_currentSeries, position, snap, pointsOnly);
        if (result != null)
        {
            result.PlotModel = ActualModel;
            ShowTracker(result);
        }
    }

    private void OnLeftButtonPressedMouseMoved(PointerEventArgs e)
    {
        if (ActualModel == null)
        {
            return;
        }

        var position = e.GetPosition(_basePanel).ToScreenPoint();

        var plotArea = ActualModel.PlotArea;

        var x = Math.Min(_mouseDownPoint.X, position.X);
        var w = Math.Abs(_mouseDownPoint.X - position.X);
        var y = Math.Min(_mouseDownPoint.Y, position.Y);
        var h = Math.Abs(_mouseDownPoint.Y - position.Y);

        if (XAxis == null || XAxis.IsZoomEnabled == false)
        {
            x = plotArea.Left;
            w = plotArea.Width;
        }

        if (YAxis == null || YAxis.IsZoomEnabled == false)
        {
            y = plotArea.Top;
            h = plotArea.Height;
        }

        _zoomRectangle = new OxyRect(x, y, w, h);

        ShowZoomRectangle(_zoomRectangle);

        return;
    }

    private void OnRightButtonPressedMouseMoved(PointerEventArgs e)
    {
        var position = e.GetPosition(_basePanel).ToScreenPoint();

        XAxis?.Pan(_previousPosition, position);
        YAxis?.Pan(_previousPosition, position);

        InvalidatePlot(false);

        _previousPosition = position;
    }

    private void _panel_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_basePanel == null)
        {
            return;
        }

        e.Pointer.Capture(null);

        if (_isPanEnabled == true)
        {
            OnRightButtonPressedMouseReleased(e);
            return;
        }

        if (_isZoomEnabled == true)
        {
            OnLeftButtonPressedMouseReleased(e);
            return;
        }

        // Open the context menu
        var p = e.GetPosition(_basePanel).ToScreenPoint();
        var d = p.DistanceTo(_mouseDownPoint);

        if (ContextMenu != null)
        {
            if (Math.Abs(d) < 1e-8 && ((PointerReleasedEventArgs)e).InitialPressMouseButton == MouseButton.Right)
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

    private void OnRightButtonPressedMouseReleased(PointerReleasedEventArgs e)
    {
        SetCursorType(CursorType.Default);

        _isPanEnabled = false;
    }

    private void OnLeftButtonPressedMouseReleased(PointerReleasedEventArgs e)
    {
        SetCursorType(CursorType.Default);

        HideZoomRectangle();

        if (_zoomRectangle.Width > 10 && _zoomRectangle.Height > 10)
        {
            var p0 = InverseTransform(_zoomRectangle.Left, _zoomRectangle.Top);
            var p1 = InverseTransform(_zoomRectangle.Right, _zoomRectangle.Bottom);

            XAxis?.Zoom(p0.X, p1.X);
            YAxis?.Zoom(p0.Y, p1.Y);

            InvalidatePlot();
        }

        _isZoomEnabled = false;
    }

    private void _panelX_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        Focus();
        e.Pointer.Capture(_axisXPanel);

        if (ActualModel == null)
        {
            return;
        }

        var point = e.GetPosition(_axisXPanel).ToScreenPoint();

        foreach (var a in ActualModel.Axises)
        {
            if (a.IsHorizontal() == true && _slider != null)
            {
                var value = a.InverseTransform(point.X);

                _slider.IsTracking = false;
                _slider.CurrentValue = _timeOrigin.AddDays(value - 1);
                _slider.IsTracking = true;

                _isPressed = true;

                // TODO: update only slider
                UpdateSlider();
                Draw();
            }
        }
    }

    private void _panelX_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isPressed = false;
    }

    private void _panelX_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPressed == true)
        {
            base.OnPointerMoved(e);

            e.Pointer.Capture(_axisXPanel);

            if (ActualModel == null)
            {
                return;
            }

            var point = e.GetPosition(_axisXPanel).ToScreenPoint();

            foreach (var axis in ActualModel.Axises)
            {
                if (axis.IsHorizontal() == true && _slider != null)
                {
                    var value = axis.InverseTransform(point.X);

                    _slider.IsTracking = false;
                    _slider.CurrentValue = _timeOrigin.AddDays(value - 1);
                    _slider.IsTracking = true;

                    // TODO: update only slider
                    UpdateSlider();
                    Draw();
                }
            }
        }
    }
}
