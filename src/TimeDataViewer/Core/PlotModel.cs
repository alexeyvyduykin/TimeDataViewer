#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using TimeDataViewer.Spatial;
using System.Collections.ObjectModel;

namespace TimeDataViewer.Core
{
    public delegate void SizeChangedEventHandler(int width, int height);
    public delegate void WindowChangedEventHandler(RectI window);
    public delegate void ClientViewportChangedEventHandler(RectD viewport);
    public delegate void ViewportChangedEventHandler(RectD viewport);

    public class PlotModel
    {
        private int _width;
        private int _height;
        private readonly TimeAxis _axisX;
        private readonly CategoryAxis _axisY;
        private readonly ObservableCollection<Series> _series;
        private RectI _plotArea;
        private RectI _window;
        private RectD _clientViewport;
        private RectD _viewport = new RectD();  
        private Point2I _windowOffset;        
        private int _zoom = 0;
        private Point2D _dragPoint;
     
        public event EventHandler OnDragChanged;
        public event EventHandler OnZoomChanged;
       
        private event SizeChangedEventHandler OnSizeChanged;        
        private event WindowChangedEventHandler OnWindowChanged;
        private event ClientViewportChangedEventHandler OnClientViewportChanged;
        private event ViewportChangedEventHandler OnViewportChanged;

        public PlotModel() 
        {       
            _axisX = new TimeAxis();
            _axisY = new CategoryAxis();

            _series = new ObservableCollection<Series>();

            MinZoom = 0;
            MaxZoom = 100;
            ZoomScaleX = 1.0; // 30 %        
            ZoomScaleY = 0.0;
            CanDragMap = true;
            MouseWheelZoomEnabled = true;     

            OnSizeChanged += SizeChangedEvent;
            OnWindowChanged += WindowChangedEvent;
            OnClientViewportChanged += ClientViewportChangedEvent;
            OnViewportChanged += ViewportChangedEvent;
        }

        public TimeAxis AxisX => _axisX;

        public CategoryAxis AxisY => _axisY;

        public ObservableCollection<Series> Series => _series;

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

        private void SizeChangedEvent(int width, int height)
        {
            _axisX.UpdateWindow(new RectI(0, 0, width, height));
            _axisY.UpdateWindow(new RectI(0, 0, width, height));
        }

        private void WindowChangedEvent(RectI rect)
        {
            _axisX.UpdateWindow(rect);
            _axisY.UpdateWindow(rect);
        }

        private void ClientViewportChangedEvent(RectD rect)
        {
            _axisX.UpdateClientViewport(rect);
            _axisY.UpdateClientViewport(rect);
        }

        private void ViewportChangedEvent(RectD rect)
        {
            _axisX.UpdateViewport(rect);
            _axisY.UpdateViewport(rect);
        }





        public double ZoomScaleX { get; init; }

        public double ZoomScaleY { get; init; }

        public int MinZoom { get; init; }
       
        public int MaxZoom { get; init; }

        public bool IsDragging { get; set; } = false;

        public bool CanDragMap { get; set; }

        public bool MouseWheelZoomEnabled { get; set; }

        public Point2I ZoomScreenPosition { get; set; }

        public Point2D ZoomPositionLocal => FromScreenToLocal(ZoomScreenPosition);

        public RectI Window
        {
            get
            {
                return _window;
            }
            private set
            {
                _window = value;

                OnWindowChanged?.Invoke(_window);
                //Debug.WriteLine($"Area -> OnWindowChanged -> Count = {OnWindowChanged?.GetInvocationList().Length}");
            }
        }
     
        public RectD ClientViewport
        {
            get
            {
                return _clientViewport;
            }
            private set
            {
                _clientViewport = value;

                OnClientViewportChanged?.Invoke(_clientViewport);
                //Debug.WriteLine($"Area -> OnClientViewportChanged -> Count = {OnClientViewportChanged?.GetInvocationList().Length}");
            }
        }
   
        public RectD Viewport
        {
            get
            {
                return _viewport;
            }
            private set
            {
                _viewport = value;

                OnViewportChanged?.Invoke(_viewport);
                //Debug.WriteLine($"Area -> OnViewportChanged -> Count = {OnViewportChanged?.GetInvocationList().Length}");
            }
        }

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
                    //Debug.WriteLine($"Area -> OnZoomChanged -> Count = {OnZoomChanged?.GetInvocationList().Length}");
                }
            }
        }

        public void UpdateViewport(double x, double y, double width, double height)
        {
            RectD viewport = new RectD(x, y, width, height);
            Viewport = viewport;
            ClientViewport = viewport;
        }

        public void UpdateSize(int width, int height)
        {
            var center = FromScreenToLocal(_width / 2, _height / 2);
    
            _width = width;
            _height = height;

            _plotArea = new RectI(0, 0, _width, _height);

            OnSizeChanged?.Invoke(width, height);
            //Debug.WriteLine($"Area -> OnSizeChanged -> Count = {OnSizeChanged?.GetInvocationList().Length}");

            Window = CreateWindow(_zoom);

            WindowOffset = CreateWindowOffset(center);

            ClientViewport = CreateClientViewport();

            ZoomScreenPosition = FromLocalToScreen(center);
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

                OnDragChanged?.Invoke(this, EventArgs.Empty);
                //Debug.WriteLine($"Area -> OnDragChanged -> Count = {OnDragChanged?.GetInvocationList().Length}");
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

            OnDragChanged?.Invoke(this, EventArgs.Empty);
            //Debug.WriteLine($"Area -> OnDragChanged -> Count = {OnDragChanged?.GetInvocationList().Length}");
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

        private void Zooming(int zoom)
        {
            var posLoc = ZoomPositionLocal;

            Window = CreateWindow(zoom);

            WindowOffset = CreateWindowOffset(posLoc);

            ClientViewport = CreateClientViewport();

            ZoomScreenPosition = FromLocalToScreen(posLoc);
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
