using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Controls;

public class DrawCanvas : Decorator
{
    private Rect? _clip;
    private readonly Pen _selectedPen = new() { Brush = Brushes.Black, Thickness = 4 };
    private readonly IBrush _blackBrush = new SolidColorBrush() { Color = Colors.Black };
    private readonly Dictionary<TimelineSeries, IList<(Rect, BrushMode, bool)>> _dict2 = new();
    private readonly Pen _defaultPen = new() { Brush = Brushes.Black };

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_dict2.Count != 0)
        {
            foreach (var series in _dict2.Keys)
            {
                var brush = series.Brush.ToAvaloniaBrush();

                foreach (var (rect, brushMode, isSelected) in _dict2[series])
                {
                    var pen = (isSelected == true) ? _selectedPen : _defaultPen;

                    if (brushMode == BrushMode.Solid)
                    {
                        context.DrawRectangle(brush, pen, rect);
                    }
                    else if (brushMode == BrushMode.UpLine)
                    {
                        var w = rect.Width;
                        var x0 = rect.Left;
                        var y0 = rect.Top;

                        var dh = rect.Height / 10.0;

                        var r3 = new Rect(x0, y0 + 2 * dh, w, dh);

                        context.DrawRectangle(brush, pen, rect);
                        context.DrawRectangle(_blackBrush, _defaultPen, r3);
                    }
                    else if (brushMode == BrushMode.DownLine)
                    {
                        var w = rect.Width;
                        var x0 = rect.Left;
                        var y0 = rect.Top;

                        var dh = rect.Height / 10.0;

                        var r3 = new Rect(x0, y0 + 7.0 * dh, w, dh);

                        context.DrawRectangle(brush, pen, rect);
                        context.DrawRectangle(_blackBrush, _defaultPen, r3);
                    }
                }
            }
        }
    }

    public void RenderSeries(PlotModel? model)
    {
        if (model != null)
        {
            var series = model.Series.Where(s => s.IsVisible).ToList();

            _dict2.Clear();

            foreach (var s in series.Cast<TimelineSeries>())
            {
                var list = new List<(Rect, BrushMode, bool)>();

                if (s.Rectangles != null)
                {
                    foreach (var (rect, brushMode, isSelected) in s.Rectangles)
                    {
                        var res = CreateClippedRectangle(s.ClippingRect, ToRect(rect));

                        if (res != null)
                        {
                            list.Add(((Rect, BrushMode, bool))(res, (BrushMode)brushMode, isSelected));
                        }
                    }
                }

                _dict2.Add(s, list);
            }

            InvalidateVisual();
        }
    }

    protected Rect? CreateClippedRectangle(OxyRect clippingRectangle, Rect rect)
    {
        if (SetClip(clippingRectangle) == true)
        {
            ResetClip();
            return rect;
        }

        var clippedRect = ClipRect(rect, clippingRectangle);
        if (clippedRect == null)
        {
            return null;
        }

        return clippedRect.Value;
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
