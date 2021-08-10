﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using TimeDataViewer.Spatial;
using System.Collections.ObjectModel;
using System.Linq;

namespace TimeDataViewer.Core
{
    public partial class PlotModel
    {
        private readonly object _syncRoot = new object(); 
        private bool _isDataUpdated;
        private int _width;
        private int _height;
        private readonly TimeAxis _axisX = new TimeAxis();
        private readonly CategoryAxis _axisY = new CategoryAxis();
        private readonly ObservableCollection<Series> _series = new ObservableCollection<Series>();
        private RectI _plotArea;
        private Point2I _windowOffset;        
        private int _zoom = 0;
        private Point2D _dragPoint;
     
        public event EventHandler OnDragChanged;
        public event EventHandler OnZoomChanged;

        public PlotModel() 
        {       
            MinZoom = 0;
            MaxZoom = 100;
            ZoomScaleX = 1.0; // 30 %        
            ZoomScaleY = 0.0;
            CanDragMap = true;
            MouseWheelZoomEnabled = true;     
        }

        public DateTime Epoch { get; set; }
        
        public TimeAxis AxisX => _axisX;

        public CategoryAxis AxisY => _axisY;

        public ObservableCollection<Series> Series => _series;
        
        public object SyncRoot => _syncRoot;

        /// <summary>
        /// Gets the total width of the plot (in device units).
        /// </summary>
        public double Width => _width;

        /// <summary>
        /// Gets the total height of the plot (in device units).
        /// </summary>
        public double Height => _height;
   
        /// <summary>
        /// Gets the plot area. This area is used to draw the series (not including axes or legends).
        /// </summary>
        public RectI PlotArea => _plotArea;

        public double ZoomScaleX { get; init; }

        public double ZoomScaleY { get; init; }

        public int MinZoom { get; init; }
       
        public int MaxZoom { get; init; }

        public bool IsDragging { get; set; } = false;

        public bool CanDragMap { get; set; }

        public bool MouseWheelZoomEnabled { get; set; }

        public Point2I ZoomScreenPosition { get; set; }

        public Point2D ZoomPositionLocal => FromScreenToLocal(ZoomScreenPosition);

        public RectI Window { get; private set; }
     
        public RectD ClientViewport { get; private set; }
   
        public RectD Viewport { get; private set; }

        public Point2I WindowOffset
        {
            get => _windowOffset;
            set
            {
                var offset = value;
                _windowOffset = WindowOffsetValidate(offset);
            }
        }

        public int Zoom
        {
            get => _zoom;            
            set
            {              
                var zoom = Math.Clamp(value, MinZoom, MaxZoom);

                if (_zoom != zoom && IsDragging == false)
                {
                    _zoom = zoom;

                    Zooming(_zoom);
                   
                    OnZoomChanged?.Invoke(this, EventArgs.Empty);                   
                }
            }
        }

        // Gets the first axes that covers the area of the specified point.
        public void GetAxesFromPoint(Point2D pt, out Axis xaxis, out Axis yaxis)
        {
            throw new Exception();
            //xaxis = yaxis = null;

            //// Get the axis position of the given point. Using null if the point is inside the plot area.
            //AxisPosition? position = null;
            //double plotAreaValue = 0;
            //if (pt.X < PlotArea.Left)
            //{
            //    position = AxisPosition.Left;
            //    plotAreaValue = PlotArea.Left;
            //}

            //if (pt.X > PlotArea.Right)
            //{
            //    position = AxisPosition.Right;
            //    plotAreaValue = PlotArea.Right;
            //}

            //if (pt.Y < PlotArea.Top)
            //{
            //    position = AxisPosition.Top;
            //    plotAreaValue = PlotArea.Top;
            //}

            //if (pt.Y > PlotArea.Bottom)
            //{
            //    position = AxisPosition.Bottom;
            //    plotAreaValue = PlotArea.Bottom;
            //}

            //foreach (var axis in Axes)
            //{
            //    if (!axis.IsAxisVisible)
            //    {
            //        continue;
            //    }

            //    //if (axis is IColorAxis)
            //    //{
            //    //    continue;
            //    //}

            //    //if (axis is MagnitudeAxis)
            //    //{
            //    //    xaxis = axis;
            //    //    continue;
            //    //}

            //    //if (axis is AngleAxis)
            //    //{
            //    //    yaxis = axis;
            //    //    continue;
            //    //}

            //    double x = double.NaN;
            //    if (axis.IsHorizontal())
            //    {
            //        x = axis.InverseTransform(pt.X);
            //    }

            //    if (axis.IsVertical())
            //    {
            //        x = axis.InverseTransform(pt.Y);
            //    }

            //    if (x >= axis.ActualMinimum && x <= axis.ActualMaximum)
            //    {
            //        if (position == null)
            //        {
            //            if (axis.IsHorizontal())
            //            {
            //                if (xaxis == null)
            //                {
            //                    xaxis = axis;
            //                }
            //            }
            //            else if (axis.IsVertical())
            //            {
            //                if (yaxis == null)
            //                {
            //                    yaxis = axis;
            //                }
            //            }
            //        }
            //        else if (position == axis.Position)
            //        {
            //            // Choose right tier
            //            double positionTierMinShift = 0;// axis.PositionTierMinShift;
            //            double positionTierMaxShift = 0;// axis.PositionTierMaxShift;

            //            double posValue = axis.IsHorizontal() ? pt.Y : pt.X;
            //            bool isLeftOrTop = position == AxisPosition.Top || position == AxisPosition.Left;
            //            if ((posValue >= plotAreaValue + positionTierMinShift
            //                 && posValue < plotAreaValue + positionTierMaxShift && !isLeftOrTop)
            //                ||
            //                (posValue <= plotAreaValue - positionTierMinShift
            //                 && posValue > plotAreaValue - positionTierMaxShift && isLeftOrTop))
            //            {
            //                if (axis.IsHorizontal())
            //                {
            //                    if (xaxis == null)
            //                    {
            //                        xaxis = axis;
            //                    }
            //                }
            //                else if (axis.IsVertical())
            //                {
            //                    if (yaxis == null)
            //                    {
            //                        yaxis = axis;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void ResetAllAxes()
        {
            throw new Exception();
            //foreach (var a in Axes)
            //{
            //    a.Reset();
            //}
        }

        public void PanAllAxes(double dx, double dy)
        {
            throw new Exception();
            //foreach (var a in Axes)
            //{
            //    a.Pan(a.IsHorizontal() ? dx : dy);
            //}
        }
        
        public void ZoomAllAxes(double factor)
        {
            throw new Exception();
            //foreach (var a in Axes)
            //{
            //    a.ZoomAtCenter(factor);
            //}
        }

        public void Update(bool updateData)
        {
            lock (SyncRoot)
            {
                try
                {
                    // Updates the default axes
                    //EnsureDefaultAxes();

                    var visibleSeries = Series./*Where(s => s.IsVisible).*/ToArray();

                    // Update data of the series
                    if (updateData || _isDataUpdated == false)
                    {
                        foreach (var s in visibleSeries)
                        {
                            s.UpdateData();
                        }

                        UpdateViewport();

                        _isDataUpdated = true;
                    }

                    // Updates axes with information from the series
                    // This is used by the category axis that need to know the number of series using the axis.
                    //foreach (var a in Axes)
                    //{
                    //    a.UpdateFromSeries(visibleSeries);
                    //    a.ResetCurrentValues();
                    //}

                    // Update valid data of the series
                    // This must be done after the axes are updated from series!
                    if (updateData)
                    {
                        foreach (var s in visibleSeries)
                        {
                            //s.UpdateValidData();
                        }
                    }

                    // Update the max and min of the axes
                    //UpdateMaxMin(updateData);
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }
        }

        public void UpdateViewport(double x, double y, double width, double height)
        {
            RectD viewport = new RectD(x, y, width, height);
            Viewport = viewport;
            _axisX.UpdateViewport(Viewport);
            _axisY.UpdateViewport(Viewport);
            ClientViewport = viewport;
            _axisX.UpdateClientViewport(ClientViewport);
            _axisY.UpdateClientViewport(ClientViewport);
        }

        public void UpdateSize(int width, int height)
        {
            var center = FromScreenToLocal(_width / 2, _height / 2);
    
            _width = width;
            _height = height;

            _plotArea = new RectI(0, 0, _width, _height);

            Window = CreateWindow(_zoom);
            _axisX.UpdateWindow(Window);
            _axisY.UpdateWindow(Window);

            WindowOffset = CreateWindowOffset(center);

            ClientViewport = CreateClientViewport();
            _axisX.UpdateClientViewport(ClientViewport);
            _axisY.UpdateClientViewport(ClientViewport);

            ZoomScreenPosition = FromLocalToScreen(center);
        }

        private void Zooming(int zoom)
        {
            var posLoc = ZoomPositionLocal;

            Window = CreateWindow(zoom);
            _axisX.UpdateWindow(Window);
            _axisY.UpdateWindow(Window);

            WindowOffset = CreateWindowOffset(posLoc);

            ClientViewport = CreateClientViewport();
            _axisX.UpdateClientViewport(ClientViewport);
            _axisY.UpdateClientViewport(ClientViewport);

            ZoomScreenPosition = FromLocalToScreen(posLoc);
        }

        public void BeginDrag(Point2D pt)
        {
            _dragPoint = new Point2D(pt.X - WindowOffset.X, pt.Y - WindowOffset.Y);
            IsDragging = true;
        }

        public void EndDrag()
        {
            IsDragging = false;     
        }

        public void Drag(Point2D point)
        {            
            if (IsDragging == true)
            {
                WindowOffset = new Point2I((int)(point.X - _dragPoint.X), (int)(point.Y - _dragPoint.Y));

                ClientViewport = CreateClientViewport();
                _axisX.UpdateClientViewport(ClientViewport);
                _axisY.UpdateClientViewport(ClientViewport);

                OnDragChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateViewport()
        {
            var maxRight = _series.Max(s => ((TimelineSeries)s).MaxTime());

            var rightDate = Epoch.AddSeconds(maxRight).Date.AddDays(1);

            var len = (rightDate - Epoch.Date).TotalSeconds;

            AutoSetViewportArea();

            UpdateViewport(0.0, 0.0, len, 1.0);
        }

        private void AutoSetViewportArea()
        {
            var d0 = (Epoch - Epoch.Date).TotalSeconds;
            var count = _series.Count;
            double step = 1.0 / (count + 1);

            int i = 0;
            foreach (TimelineSeries item in _series)
            {
                if (item is not null)
                {
                    var series = item;
                    var seriesLocalPostion = new Point2D(0.0, (++i) * step);

                    foreach (var ival in series.Items)
                    {                      
                        ival.LocalPosition = 
                            new Point2D(d0 + ival.Begin + (ival.End - ival.Begin) / 2.0, seriesLocalPostion.Y);                        
                    }
                }
            }
        }

        public Point2D FromScreenToLocal(Point2I point)
        {
            return FromScreenToLocal(point.X, point.Y);
        }

        public Point2D FromAbsoluteToLocal(Point2I point)
        {       
            return FromAbsoluteToLocal(point.X, point.Y);
        }

        public Point2D FromScreenToLocal(int x, int y)
        {
            var pLocal = new Point2I(x, y);

            pLocal.OffsetNegative(WindowOffset);

            return new Point2D(AxisX.FromAbsoluteToLocal(pLocal.X), AxisY.FromAbsoluteToLocal(pLocal.Y));
        }

        public Point2D FromAbsoluteToLocal(int x, int y)
        {
            var pLocal = new Point2I(x, y);

            return new Point2D(AxisX.FromAbsoluteToLocal(pLocal.X), AxisY.FromAbsoluteToLocal(pLocal.Y));
        }

        public Point2I FromLocalToScreen(Point2D point)
        {
            return FromLocalToScreen(point.X, point.Y);
        }

        public Point2I FromLocalToAbsolute(Point2D point)
        {
            return FromLocalToAbsolute(point.X, point.Y);
        }

        public Point2I FromLocalToScreen(double x, double y)
        {
            var pLocal = new Point2I((int)AxisX.FromLocalToAbsolute(x), (int)AxisY.FromLocalToAbsolute(y));

            pLocal.Offset(WindowOffset);

            return new Point2I(pLocal.X, pLocal.Y);
        }

        public Point2I FromLocalToAbsolute(double x, double y)
        {
            var pLocal = new Point2I((int)AxisX.FromLocalToAbsolute(x), (int)AxisY.FromLocalToAbsolute(y));

            return new Point2I(pLocal.X, pLocal.Y);
        }

        public void DragToTime(double xValue)
        {            
            WindowOffset = CreateWindowOffset(new Point2D(xValue, 0.0));
            
            ClientViewport = CreateClientViewport();
            _axisX.UpdateClientViewport(ClientViewport);
            _axisY.UpdateClientViewport(ClientViewport);

            OnDragChanged?.Invoke(this, EventArgs.Empty);
        }

        private Point2I CreateWindowOffset(Point2D pos)
        {
            var xAbs = AxisX.FromLocalToAbsolute(pos.X);
            var yAbs = AxisY.FromLocalToAbsolute(pos.Y);

            var offsetX = _width / 2 - xAbs;
            var offsetY = _height / 2 - yAbs;

            return new Point2I((int)offsetX, (int)offsetY);
        }

        private Point2I WindowOffsetValidate(Point2I offset)
        {
            var xOffset = offset.X;
            var yOffset = offset.Y;

            xOffset = Math.Min(xOffset, 0);
            xOffset = Math.Max(xOffset + Window.Width, _width) - Window.Width;

            yOffset = Math.Min(yOffset, 0);
            yOffset = Math.Max(yOffset + Window.Height, _height) - Window.Height;

            return new Point2I(xOffset, yOffset);
        }

        private RectI CreateWindow(int zoom)
        {                                   
            int w = _width * (1 + (int)(zoom * ZoomScaleX));
            int h = _height * (1 + (int)(zoom * ZoomScaleY));

            return new RectI(0, 0, w, h);
        }

        private RectD CreateClientViewport()
        {
            RectI Abs = Window;
            RectD Loc = Viewport;

            int x00 = -WindowOffset.X;
            int y00 = -WindowOffset.Y;

            double bottom = y00 * Loc.Height / Abs.Height + Loc.Y;
            double left = x00 * Loc.Width / Abs.Width + Loc.X;

            double w = _width * Loc.Width / Abs.Width;
            double h = _height * Loc.Height / Abs.Height;

            if (w < 0 || h < 0)
            {
                throw new Exception();
            }

            return new RectD(left, bottom, w, h);
        }
    }
}
