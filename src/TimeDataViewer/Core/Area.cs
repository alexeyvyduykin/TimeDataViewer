#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public delegate void SizeChangedEventHandler(int width, int height);
    public delegate void WindowAreaChangedEventHandler(RectI window);
    public delegate void ViewportScreenChangedEventHandler(RectD viewport);
    public delegate void ViewportDataChangedEventHandler(RectD viewport);

    public class Area
    {
        private int _width;
        private int _height;
        private readonly ITimeAxis _axisX;
        private readonly ICategoryAxis _axisY;
        private RectI _windowAreaZoom;
        private RectD _viewportAreaScreen;
        private RectD _viewportAreaData = new RectD();
        private CoreFactory _coreFactory = new CoreFactory();
        private Point2I _renderOffset;        
        private int _zoom = 0;
        private Point2I _dragPoint;
     
        public event EventHandler OnDragChanged;
        public event EventHandler OnZoomChanged;
        public event SizeChangedEventHandler OnSizeChanged;
        public event WindowAreaChangedEventHandler OnWindowAreaChanged;
        public event ViewportScreenChangedEventHandler OnViewportScreenChanged;
        public event ViewportDataChangedEventHandler OnViewportDataChanged;

        public Area() 
        {
            _axisX = _coreFactory.CreateTimeAxis();
            _axisY = _coreFactory.CreateCategoryAxis();

            OnSizeChanged += (w, h) => _axisX.UpdateWindow(new RectI(0, 0, w, h));
            OnWindowAreaChanged += (e) => _axisX.UpdateWindow(e);
            OnViewportScreenChanged += (e) => _axisX.UpdateScreen(e);
            OnViewportDataChanged += (e) => _axisX.UpdateViewport(e);

            OnSizeChanged += (w, h) => _axisY.UpdateWindow(new RectI(0, 0, w, h));
            OnWindowAreaChanged += (e) => _axisY.UpdateWindow(e);
            OnViewportScreenChanged += (e) => _axisY.UpdateScreen(e);
            OnViewportDataChanged += (e) => _axisY.UpdateViewport(e);
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

        public RectD RenderSize => new(RenderOffsetAbsolute.X, RenderOffsetAbsolute.Y, WindowZoom.Width, WindowZoom.Height);

        public RectI Screen => new(0, 0, _width, _height);
        
        public RectI WindowZoom
        {
            get
            {
                return _windowAreaZoom;
            }
            private set
            {
                _windowAreaZoom = value;

                OnWindowAreaChanged?.Invoke(_windowAreaZoom);
            }
        }
     
        public RectD ViewportScreen
        {
            get
            {
                return _viewportAreaScreen;
            }
            private set
            {                    
                _viewportAreaScreen = value;

                OnViewportScreenChanged?.Invoke(_viewportAreaScreen);
            }
        }
   
        public RectD ViewportData
        {
            get
            {
                return _viewportAreaData;
            }
            private set
            {
                _viewportAreaData = value;

                OnViewportDataChanged?.Invoke(_viewportAreaData);
            }
        }
      
        public void UpdateViewport(double x, double y, double width, double height)
        {
            RectD viewport = new RectD(x, y, width, height);
            ViewportData = viewport;
            ViewportScreen = viewport;
        }

        public void UpdateSize(int width, int height)
        {
            var center = FromScreenToLocal(_width / 2, _height / 2);
    
            _width = width;
            _height = height;

            OnSizeChanged?.Invoke(width, height);

            WindowZoom = CreateWindowZoom(_zoom, ZoomScaleX, ZoomScaleY);

            RenderOffsetAbsolute = GetRenderOffset(center);

            ViewportScreen = CreateViewportScreen();

            ZoomScreenPosition = FromLocalToScreen(center);
        }

        public bool IsWindowArea(Point2D point)
        {
            var sc0 = RenderOffsetAbsolute;

            int x0 = Math.Max(sc0.X, 0);
            int y0 = Math.Max(sc0.Y, 0);
            int x1 = Math.Min(WindowZoom.Width + sc0.X, _width);
            int y1 = Math.Min(WindowZoom.Height + sc0.Y, _height);
            var rect = new RectD(x0, y0, Math.Abs(x1 - x0), Math.Abs(y1 - y0));

            return rect.Contains(point);
        }

        public RectD RenderVisibleWindow
        {
            get
            {
                var sc0 = RenderOffsetAbsolute;

                var result = RectI.Intersect(
                    new RectI(0, 0, _width, _height),
                    new RectI(sc0.X, sc0.Y, WindowZoom.Width, WindowZoom.Height));

                return new RectD(result.X, result.Y, result.Width, result.Height);

                //int x0 = Math.Max(sc0.X, 0);
                //int y0 = Math.Max(sc0.Y, 0);
                //int x1 = Math.Min(ZoomingWindowArea.Width + sc0.X, Width__);
                //int y1 = Math.Min(ZoomingWindowArea.Height + sc0.Y, Height__);

                //return new Rect(x0, y0, x1 - x0, y1 - y0);
            }
        }
      
        public Point2I RenderOffsetAbsolute
        {
            get => _renderOffset;            
            set => _renderOffset = RenderOffsetValidate(value);            
        }

        private Point2I RenderOffsetValidate(Point2I offset)
        {
            var x = offset.X;
            var y = offset.Y;

            x = Math.Min(x, 0);
            x = Math.Max(x + WindowZoom.Width, _width) - WindowZoom.Width;

            y = Math.Min(y, 0);
            y = Math.Max(y + WindowZoom.Height, _height) - WindowZoom.Height;

            return new Point2I(x, y);
        }
             
        public Point2D ZoomPositionLocal => FromScreenToLocal(ZoomScreenPosition.X, ZoomScreenPosition.Y);

        public RectI RenderWindowArea => new RectI(RenderOffsetAbsolute.X, RenderOffsetAbsolute.Y, WindowZoom.Width, WindowZoom.Height);

        private bool Zooming(int zm)
        {
            var posLoc = ZoomPositionLocal;

            WindowZoom = CreateWindowZoom(zm, ZoomScaleX, ZoomScaleY);

            RenderOffsetAbsolute = GetRenderOffset(posLoc);

            ViewportScreen = CreateViewportScreen();

            ZoomScreenPosition = FromLocalToScreen(posLoc);

            return true;
        }

        public Point2I GetRenderOffset(Point2D pos)
        {
            var xAbs = AxisX.FromLocalToAbsolute(pos.X);
            var yAbs = AxisY.FromLocalToAbsolute(pos.Y);

            //  var wz = WindowAreaZoom.Width;
            //  var hz = WindowAreaZoom.Height;

            var offsetX = _width / 2 - xAbs;
            var offsetY = _height / 2 - yAbs;

            // var offsetX = wz / 2.0 - this.Width__ / 2.0;
            // var offsetY = hz / 2.0 - this.Height__ / 2.0;

            return new Point2I(offsetX, offsetY);
        }

        private RectI CreateWindowZoom(int zm, double sclX, double sclY)
        {
            var w0 = _width;
            var h0 = _height;

            double stepx = w0 * sclX;
            double stepy = h0 * sclY;

            int w = w0 + (int)(zm * stepx);
            int h = h0 + (int)(zm * stepy);

            return new RectI(0, 0, w, h);
        }

        private RectD CreateViewportScreen()
        {
            RectI Abs = WindowZoom;
            RectD Loc = ViewportData;

            int x00 = -RenderOffsetAbsolute.X;
            int y00 = -RenderOffsetAbsolute.Y;

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
   
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                var zoom = value;

                if(zoom < MinZoom)
                {
                    zoom = MinZoom;
                }

                if(zoom > MaxZoom)
                {
                    zoom = MaxZoom;
                }

                if (_zoom != zoom && IsDragging == false)
                {                                    
                    _zoom = zoom;

                    if (Zooming(_zoom) == true)
                    {
                        OnZoomChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
        
        public void BeginDrag(Point2I pt)
        {
            _dragPoint.X = pt.X - RenderOffsetAbsolute.X;
            _dragPoint.Y = pt.Y - RenderOffsetAbsolute.Y;
            IsDragging = true;
        }

        public void EndDrag()
        {
            IsDragging = false;
            MouseDown = Point2I.Empty;
        }

        public void Drag(Point2I pt)
        {
            //  _renderOffset.X = pt.X - dragPoint.X;
            //  _renderOffset.Y = pt.Y - dragPoint.Y;

            RenderOffsetAbsolute = new Point2I(pt.X - _dragPoint.X, pt.Y - _dragPoint.Y);

            if (IsDragging == true)
            {
                ViewportScreen = CreateViewportScreen();

                OnDragChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Point2D FromScreenToLocal(int x, int y)
        {
            var pLocal = new Point2I(x, y);

            pLocal.OffsetNegative(RenderOffsetAbsolute);

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

            pLocal.Offset(RenderOffsetAbsolute);

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

        public void TimeAsCenter(double xValue)
        {
            var xAbs = AxisX.FromLocalToAbsolute(xValue);
            var offsetX = _width / 2 - xAbs;
            
            RenderOffsetAbsolute = GetRenderOffset(new Point2D(offsetX + RenderOffsetAbsolute.X, RenderOffsetAbsolute.Y));
            
            ViewportScreen = CreateViewportScreen();

            OnDragChanged?.Invoke(this, EventArgs.Empty);
        }

        //public int GetMaxZoomToFitRect(RectD rect)
        //{
        //    int zoom = _minZoom;

        //    if (rect.Height == 0.0 || rect.Width == 0.0)
        //    {
        //        zoom = _maxZoom / 2;
        //    }
        //    else
        //    {
        //        for (int i = (int)zoom; i <= _maxZoom; i++)
        //        {
        //            WindowAreaZoom = CreateWindowAreaZoom(i, _scaleX, _scaleY);

        //            var p0 = new Point2I(
        //                AxisX.FromLocalToAbsolute(rect.Left),
        //                AxisY.FromLocalToAbsolute(rect.Bottom)
        //                );
        //            var p1 = new Point2I(
        //                AxisX.FromLocalToAbsolute(rect.Right),
        //                AxisY.FromLocalToAbsolute(rect.Top)
        //                );

        //            if (((p1.X - p0.X) <= _width + 10) && (p1.Y - p0.Y) <= _height + 10)
        //            {
        //                zoom = i;
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    return zoom;
        //}
    }
}
