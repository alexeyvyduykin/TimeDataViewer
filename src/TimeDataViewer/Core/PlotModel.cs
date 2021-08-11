#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using TimeDataViewer.Spatial;
using System.Collections.ObjectModel;
using System.Linq;

namespace TimeDataViewer.Core
{
    public partial class PlotModel : Model
    {       
        private bool _isDataUpdated;
        private readonly TimeAxis _axisX = new TimeAxis();
        private readonly CategoryAxis _axisY = new CategoryAxis();
        private readonly ObservableCollection<Series> _series;       
        private Point2I _windowOffset;        
        private int _zoom = 0;
        private Point2D _dragPoint;
     
        public event EventHandler OnDragChanged;
        public event EventHandler OnZoomChanged;

        public PlotModel() 
        {
            _series = new ObservableCollection<Series>();

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
        
        /// <summary>
        /// Gets the total width of the plot (in device units).
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Gets the total height of the plot (in device units).
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Gets the plot area. This area is used to draw the series (not including axes or legends).
        /// </summary>
        public RectD PlotArea { get; private set; }

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
            xaxis = _axisX;
            yaxis = _axisY;
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

        public override IEnumerable<UIElement> GetElements()
        {
            throw new Exception();
            //foreach (var axis in Axes.Reverse().Where(a => a.IsAxisVisible && a.Layer == AxisLayer.AboveSeries))
            //{
            //    yield return axis;
            //}

            //foreach (var s in Series.Reverse().Where(s => s.IsVisible))
            //{
            //    yield return s;
            //}

            //foreach (var axis in Axes.Reverse().Where(a => a.IsAxisVisible && a.Layer == AxisLayer.BelowSeries))
            //{
            //    yield return axis;
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
            var center = FromScreenToLocal((int)Width / 2, (int)Height / 2);
    
            Width = width;
            Height = height;

            PlotArea = new RectD(0, 0, Width, Height);

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

            var offsetX = Width / 2 - xAbs;
            var offsetY = Height / 2 - yAbs;

            return new Point2I((int)offsetX, (int)offsetY);
        }

        private Point2I WindowOffsetValidate(Point2I offset)
        {
            var xOffset = offset.X;
            var yOffset = offset.Y;

            xOffset = Math.Min(xOffset, 0);
            xOffset = Math.Max(xOffset + Window.Width, (int)Width) - Window.Width;

            yOffset = Math.Min(yOffset, 0);
            yOffset = Math.Max(yOffset + Window.Height, (int)Height) - Window.Height;

            return new Point2I(xOffset, yOffset);
        }

        private RectI CreateWindow(int zoom)
        {                                   
            int w = (int)Width * (1 + (int)(zoom * ZoomScaleX));
            int h = (int)Height * (1 + (int)(zoom * ZoomScaleY));

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

            double w = Width * Loc.Width / Abs.Width;
            double h = Height * Loc.Height / Abs.Height;

            if (w < 0 || h < 0)
            {
                throw new Exception();
            }

            return new RectD(left, bottom, w, h);
        }
    }
}
