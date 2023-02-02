using Avalonia.Controls;
using Avalonia.Input;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Extensions;
using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Controls;

public partial class TimelineControl
{
    private ScreenPoint _mouseDownPoint;
    private Series? _currentSeries;
    private ScreenPoint _previousPosition;
    private bool _isPanEnabled;
    private OxyRect _zoomRectangle;
    private bool _isZoomEnabled;
    private Axis? _xAxis;
    private Axis? _yAxis;

    protected DataPoint InverseTransform(double x, double y)
    {
        if (_xAxis != null)
        {
            return _xAxis.InverseTransform(x, y, _yAxis!);
        }

        if (_yAxis != null)
        {
            return new DataPoint(0, _yAxis.InverseTransform(y));
        }

        return new DataPoint();
    }

    protected static TrackerHitResult? GetNearestHit(Series? series, ScreenPoint point, bool snap, bool pointsOnly)
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

    protected static TrackerHitResult? GetNearestHit(Series? series, ScreenPoint point)
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
        if (_xAxis == null)
        {
            return CursorType.ZoomVertical;
        }

        if (_yAxis == null)
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

        _xAxis = ActualModel?.AxisX;
        _yAxis = ActualModel?.AxisY;

        var position = e.GetPosition(_basePanel).ToScreenPoint();
        var delta = (int)(e.Delta.Y + e.Delta.X) * 120;
        var factor = 1;
        var step = delta * 0.001 * factor;

        var isZoomEnabled = (_xAxis != null && _xAxis.IsZoomEnabled) || (_yAxis != null && _yAxis.IsZoomEnabled);

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

        _xAxis?.ZoomAt(scale, current.X);

        _yAxis?.ZoomAt(scale, current.Y);

        InvalidatePlot();
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
            if (item is TimelineSeries series)
            {
                series.ResetSelectIndex();
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
            if (currentSeries is TimelineSeries series)
            {
                series.SelectIndex((int)result.Index);

                InvalidatePlot();

                OnSelectedInterval(result);
            }
        }

        return true;
    }

    private bool OnRightButtonMouseDown(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(_basePanel).ToScreenPoint();

        _previousPosition = position;

        _isPanEnabled = (_xAxis != null && _xAxis.IsPanEnabled)
                     || (_yAxis != null && _yAxis.IsPanEnabled);

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

        _isZoomEnabled = (_xAxis != null && _xAxis.IsZoomEnabled)
                      || (_yAxis != null && _yAxis.IsZoomEnabled);

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

        if (_xAxis == null || _xAxis.IsZoomEnabled == false)
        {
            x = plotArea.Left;
            w = plotArea.Width;
        }

        if (_yAxis == null || _yAxis.IsZoomEnabled == false)
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

        _xAxis?.Pan(_previousPosition, position);
        _yAxis?.Pan(_previousPosition, position);

        InvalidatePlot();

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

            _xAxis?.Zoom(p0.X, p1.X);
            _yAxis?.Zoom(p0.Y, p1.Y);

            InvalidatePlot();
        }

        _isZoomEnabled = false;
    }
}
