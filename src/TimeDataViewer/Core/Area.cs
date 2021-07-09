#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Timeline.Spatial;

namespace Timeline.Core
{
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
        private Point2D _dragPoint;
     
        public event EventHandler OnDragChanged;
        public event EventHandler OnZoomChanged;
       
        public Area() 
        {
            _coreFactory = new CoreFactory();

            _axisX = _coreFactory.CreateTimeAxis(this);
            _axisY = _coreFactory.CreateCategoryAxis(this);
        }

        public int Width => _width;

        public int Height => _height;

        public ITimeAxis AxisX => _axisX;            
        
        public ICategoryAxis AxisY => _axisY;

        public double ZoomScaleX { get; init; }

        public double ZoomScaleY { get; init; }

        public int MinZoom { get; init; }
       
        public int MaxZoom { get; init; }

        public bool IsDragging { get; set; } = false;

        public bool CanDragMap { get; set; }

        public bool MouseWheelZoomEnabled { get; set; }

        public Point2I ZoomScreenPosition { get; set; }

        public RectI Screen => new(0, 0, _width, _height);

        public Point2D ZoomPositionLocal => FromScreenToLocal(ZoomScreenPosition);

        public RectI Window => _window;
     
        public RectD ClientViewport => _clientViewport;
   
        public RectD Viewport => _viewport;

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

        public void WindowUpdated(int width, int height, int zoom)
        {
            _width = width;
            _height = height;

            int w = width * (1 + (int)(zoom * ZoomScaleX));
            int h = height * (1 + (int)(zoom * ZoomScaleY));

            _window = new RectI(0, 0, w, h);

            _axisX.UpdateWindow(_window);
            _axisY.UpdateWindow(_window);
        }

        public void ClientViewportUpdated(Point2I offset, RectI window, RectD viewport)
        {              
            int x00 = -offset.X;
            int y00 = -offset.Y;

            double bottom = y00 * viewport.Height / window.Height + viewport.Y;
            double left = x00 * viewport.Width / window.Width + viewport.X;

            double w = _width * viewport.Width / window.Width;
            double h = _height * viewport.Height / window.Height;

            if (w < 0 || h < 0)
            {
                throw new Exception();
            }

            _clientViewport = new RectD(left, bottom, w, h);

            _axisX.UpdateClientViewport(_clientViewport);
            _axisY.UpdateClientViewport(_clientViewport);
        }

        public void ViewportUpdated(double x, double y, double width, double height)
        {
            RectD viewport = new RectD(x, y, width, height);
            _viewport = viewport;
            _clientViewport = viewport;

            _axisX.UpdateViewport(_viewport);
            _axisY.UpdateViewport(_viewport);

            _axisX.UpdateClientViewport(_clientViewport);
            _axisY.UpdateClientViewport(_clientViewport);
        }

        public void SizeUpdated(int width, int height)
        {
            var oldCenter = FromScreenToLocal(Width / 2, Height / 2);

            WindowUpdated(width, height, _zoom);
     
            WindowOffset = CreateWindowOffset(oldCenter);

            ClientViewportUpdated(WindowOffset, Window, Viewport);

            ZoomScreenPosition = FromLocalToScreen(oldCenter);
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
             
                ClientViewportUpdated(WindowOffset, Window, Viewport);

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
            var pLocal = new Point2I(AxisX.FromLocalToAbsolute(x), AxisY.FromLocalToAbsolute(y));

            pLocal.Offset(WindowOffset);

            return new Point2I(pLocal.X, pLocal.Y);
        }

        public Point2I FromLocalToAbsolute(double x, double y)
        {
            var pLocal = new Point2I(AxisX.FromLocalToAbsolute(x), AxisY.FromLocalToAbsolute(y));

            return new Point2I(pLocal.X, pLocal.Y);
        }

        public void DragToTime(double xValue)
        {            
            WindowOffset = CreateWindowOffset(new Point2D(xValue, 0.0));
            
            ClientViewportUpdated(WindowOffset, Window, Viewport);
        
            OnDragChanged?.Invoke(this, EventArgs.Empty);
            //Debug.WriteLine($"Area -> OnDragChanged -> Count = {OnDragChanged?.GetInvocationList().Length}");
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

            WindowUpdated(Width, Height, zoom);

            WindowOffset = CreateWindowOffset(posLoc);
           
            ClientViewportUpdated(WindowOffset, Window, Viewport);

            ZoomScreenPosition = FromLocalToScreen(posLoc);
        }
    }
}
