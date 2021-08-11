using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Path = Avalonia.Controls.Shapes.Path;
using Avalonia.Media.Immutable;
using TimeDataViewer.Spatial;

namespace TimeDataViewer
{
    public class DrawCanvas : Canvas
    {
        private Rect? _clip;
        private IBrush _brush;
        private Pen _pen;
        private List<Rect> _rects = new List<Rect>();

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            foreach (var item in _rects)
            {
                context.DrawRectangle(_brush, _pen, item);
            }
        }

        public void CreateRenderIntervals(RectD clippingRectangle, IEnumerable<RectD> rects, IBrush fill, IBrush stroke)
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

        protected void CreateClippedRectangle(RectD clippingRectangle, Rect rect)
        {
            //if (SetClip(clippingRectangle))
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

        protected bool SetClip(RectD clippingRect)
        {
            _clip = ToRect(clippingRect);
            return false;// true;
        }

        protected static Rect? ClipRect(Rect rect, RectD clippingRectangle)
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

        protected static Rect ToRect(RectD r)
        {
            return new Rect(r.Left, r.Top, r.Width, r.Height);
        }
    }
}
