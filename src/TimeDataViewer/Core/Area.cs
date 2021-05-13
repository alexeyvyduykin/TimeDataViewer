#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public delegate void SizeChangedEventHandler(int width, int height);
    public delegate void WindowChangedEventHandler(RectI window);
    public delegate void ClientViewportChangedEventHandler(RectD viewport);
    public delegate void ViewportChangedEventHandler(RectD viewport);

    public class Area
    {
        private int _width;
        private int _height;
        private readonly ITimeAxis _axisX;
        private readonly ICategoryAxis _axisY;
        private RectI _window;
        private RectD _clientViewport;
        private RectD _viewport = new RectD();
        private readonly CoreFactory _coreFactory;
        private Point2I _windowOffset;        
        private int _zoom = 0;
        private Point2I _dragPoint;
     
        public event EventHandler OnDragChanged;
        public event EventHandler OnZoomChanged;
        public event SizeChangedEventHandler OnSizeChanged;
        public event WindowChangedEventHandler OnWindowChanged;
        public event ClientViewportChangedEventHandler OnClientViewportChanged;
        public event ViewportChangedEventHandler OnViewportChanged;

        public Area() 
        {
            _coreFactory = new CoreFactory();
            _axisX = _coreFactory.CreateTimeAxis();
            _axisY = _coreFactory.CreateCategoryAxis();

            OnSizeChanged += (w, h) => _axisX.UpdateWindow(new RectI(0, 0, w, h));
            OnWindowChanged += (e) => _axisX.UpdateWindow(e);
            OnClientViewportChanged += (e) => _axisX.UpdateClientViewport(e);
            OnViewportChanged += (e) => _axisX.UpdateViewport(e);

            OnSizeChanged += (w, h) => _axisY.UpdateWindow(new RectI(0, 0, w, h));
            OnWindowChanged += (e) => _axisY.UpdateWindow(e);
            OnClientViewportChanged += (e) => _axisY.UpdateClientViewport(e);
            OnViewportChanged += (e) => _axisY.UpdateViewport(e);
        }

        public int Width => _width;

        public int Height => _height;

        public ITimeAxis AxisX => _axisX;            
        
        public ICategoryAxis AxisY => _axisY;

        public double ZoomScaleX { get; init; }

        public double ZoomScaleY { get; init; }

        public int MinZoom { get; init; }
       
        public int MaxZoom { get; init; }

        public Point2I MouseDown { get; set; }

        public Point2I MouseCurrent { get; set; }

        public bool IsDragging { get; set; } = false;

        public bool CanDragMap { get; set; }

        public bool MouseWheelZoomEnabled { get; set; }

        public Point2I ZoomScreenPosition { get; set; }

        public RectI Screen => new(0, 0, _width, _height);

        public Point2D ZoomPositionLocal => FromScreenToLocal(ZoomScreenPosition.X, ZoomScreenPosition.Y);

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

            OnSizeChanged?.Invoke(width, height);

            Window = CreateWindow(_zoom);

            WindowOffset = CreateWindowOffset(center);

            ClientViewport = CreateClientViewport();

            ZoomScreenPosition = FromLocalToScreen(center);
        }

        public void BeginDrag(Point2I pt)
        {
            _dragPoint.X = pt.X - WindowOffset.X;
            _dragPoint.Y = pt.Y - WindowOffset.Y;
            IsDragging = true;
        }

        public void EndDrag()
        {
            IsDragging = false;
            MouseDown = Point2I.Empty;
        }

        public void Drag(Point2I pt)
        {            
            if (IsDragging == true)
            {
                WindowOffset = new Point2I(pt.X - _dragPoint.X, pt.Y - _dragPoint.Y);

                ClientViewport = CreateClientViewport();

                OnDragChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Point2D FromScreenToLocal(int x, int y)
        {
            var pLocal = new Point2I(x, y);

            pLocal.OffsetNegative(WindowOffset);

            return new Point2D(AxisX.FromAbsoluteToLocal(pLocal.X), AxisY.FromAbsoluteToLocal(pLocal.Y));

            //   return Provider.Projection.FromPixelToSchedulerPoint(pLocal, Zoom);
        }

        public Point2D FromAbsoluteToLocal(int x, int y)
        {
            var pLocal = new Point2I(x, y);

            return new Point2D(AxisX.FromAbsoluteToLocal(pLocal.X), AxisY.FromAbsoluteToLocal(pLocal.Y));
        }

        public Point2I FromLocalToScreen(Point2D shedulerPoint)
        {
            // Point2I pLocal = Provider.Projection.FromSchedulerPointToPixel(shedulerPoint, Zoom);

            var pLocal = new Point2I(AxisX.FromLocalToAbsolute(shedulerPoint.X), AxisY.FromLocalToAbsolute(shedulerPoint.Y));

            pLocal.Offset(WindowOffset);

            return new Point2I(pLocal.X, pLocal.Y);
        }

        public Point2I FromLocalToAbsolute(Point2D shedulerPoint)
        {
            // Point2I pLocal = Provider.Projection.FromSchedulerPointToPixel(shedulerPoint, Zoom);

            var pLocal = new Point2I(AxisX.FromLocalToAbsolute(shedulerPoint.X), AxisY.FromLocalToAbsolute(shedulerPoint.Y));

            return new Point2I(pLocal.X, pLocal.Y);
        }
         
        public double GetTimeCenter()
        {
            var local = FromScreenToLocal(Width / 2, Height / 2);
            return local.X;
        }

        public void DragToTime(double xValue)
        {
            //var xAbs = AxisX.FromLocalToAbsolute(xValue);
            //var offsetX = _width / 2 - xAbs;
            
            WindowOffset = CreateWindowOffset(new Point2D(xValue, 0.0));
          //  WindowOffset = new Point2I(WindowOffset.X + _width / 2, WindowOffset.Y);
            
            ClientViewport = CreateClientViewport();

            OnDragChanged?.Invoke(this, EventArgs.Empty);
        }

        private Point2I CreateWindowOffset(Point2D pos)
        {
            var xAbs = AxisX.FromLocalToAbsolute(pos.X);
            var yAbs = AxisY.FromLocalToAbsolute(pos.Y);

            var offsetX = _width / 2 - xAbs;
            var offsetY = _height / 2 - yAbs;

            return new Point2I(offsetX, offsetY);
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
