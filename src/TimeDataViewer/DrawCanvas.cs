using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TimeDataViewer.Spatial;

namespace TimeDataViewer
{
    public class DrawCanvas : Canvas
    {
        private Rect? _clip;
        private IBrush _brush;
        private Pen _pen;
        private List<Rect> _rects = new List<Rect>();
        private Dictionary<TimelineSeries, IList<Rect>> _dict = new Dictionary<TimelineSeries, IList<Rect>>();

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (_dict.Count != 0)
            {
                foreach (var series in _dict.Keys)
                {
                    _brush = series.FillBrush;
                    _pen = new Pen() { Brush = series.StrokeBrush };

                    foreach (var item in _dict[series])
                    {
                        context.DrawRectangle(_brush, _pen, item);
                    }
                }
            }
            else
            {
                foreach (var item in _rects)
                {
                    context.DrawRectangle(_brush, _pen, item);
                }
            }
        }

        public void RenderSeries(IEnumerable<Series> series)
        {
            _dict.Clear();

            foreach (var s in series)
            {
                List<Rect> list = new List<Rect>();

                var innserSeries = ((Core.TimelineSeries)s.InternalSeries);

                foreach (var item in innserSeries.MyRectList)
                {
                    CreateClippedRectangle(innserSeries.MyClippingRect, ToRect(item), list);
                }

                _dict.Add((TimelineSeries)s, list);
            }

            InvalidateVisual();
        }

        public void RenderIntervals(OxyRect clippingRectangle, IEnumerable<OxyRect> rects, IBrush fill, IBrush stroke)
        {
            _brush = fill;
            _pen = new Pen() { Brush = stroke };
            _rects.Clear();

            foreach (var item in rects)
            {
                CreateClippedRectangle(clippingRectangle, ToRect(item));
            }

            InvalidateVisual();
        }

        protected void CreateClippedRectangle(OxyRect clippingRectangle, Rect rect, IList<Rect> list)
        {
            if (SetClip(clippingRectangle))
            {
                list.Add(rect);
                ResetClip();
                return;
            }

            var clippedRect = ClipRect(rect, clippingRectangle);
            if (clippedRect == null)
            {
                return;
            }

            list.Add(clippedRect.Value);
        }

        protected void CreateClippedRectangle(OxyRect clippingRectangle, Rect rect)
        {
            // if (SetClip(clippingRectangle))
            {
                _rects.Add(rect);
                ResetClip();
                return;
            }

            var clippedRect = ClipRect(rect, clippingRectangle);
            if (clippedRect == null)
            {
                return;
            }

            _rects.Add(clippedRect.Value);
        }

        protected bool SetClip(OxyRect clippingRect)
        {
            _clip = ToRect(clippingRect);
            return false;//true;
        }

        protected static Rect? ClipRect(Rect rect, OxyRect clippingRectangle)
        {
            if (rect.Right < clippingRectangle.Left)
            {
                return null;
            }

            if (rect.Left > clippingRectangle.Right)
            {
                return null;
            }

            if (rect.Top > clippingRectangle.Bottom)
            {
                return null;
            }

            if (rect.Bottom < clippingRectangle.Top)
            {
                return null;
            }

            var width = rect.Width;
            var left = rect.Left;
            var top = rect.Top;
            var height = rect.Height;

            if (left + width > clippingRectangle.Right)
            {
                width = clippingRectangle.Right - left;
            }

            if (left < clippingRectangle.Left)
            {
                width = rect.Right - clippingRectangle.Left;
                left = clippingRectangle.Left;
            }

            if (top < clippingRectangle.Top)
            {
                height = rect.Bottom - clippingRectangle.Top;
                top = clippingRectangle.Top;
            }

            if (top + height > clippingRectangle.Bottom)
            {
                height = clippingRectangle.Bottom - top;
            }

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return null;
            }

            return new Rect(left, top, width, height);
        }

        protected void ResetClip()
        {
            _clip = null;
        }

        protected static Rect ToRect(OxyRect r)
        {
            return new Rect(r.Left, r.Top, r.Width, r.Height);
        }
    }
}
