using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Controls;

public class DrawCanvas : Canvas
{
    private Rect? _clip;
    private readonly Pen _selectedPen = new() { Brush = Brushes.Black, Thickness = 4 };
    private readonly Dictionary<TimelineSeries, IList<Rect>> _dict = new();
    private readonly Dictionary<TimelineSeries, IList<Rect>> _selectedDict = new();
    private readonly Dictionary<Core.TimelineSeries, IList<(Rect, bool)>> _dict2 = new();

    private readonly Pen _defaultPen = new() { Brush = Brushes.Black };
    private readonly IBrush _defaultBrush = new SolidColorBrush(Colors.Red);
    private readonly List<IBrush> _brushes = new();

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_dict.Count != 0)
        {
            foreach (var series in _dict.Keys)
            {
                var pen = new Pen() { Brush = series.StrokeBrush };

                foreach (var item in _dict[series])
                {
                    context.DrawRectangle(series.FillBrush, pen, item);
                }
            }
        }

        if (_selectedDict.Count != 0)
        {
            foreach (var series in _selectedDict.Keys)
            {
                foreach (var item in _selectedDict[series])
                {
                    context.DrawRectangle(series.FillBrush, _selectedPen, item);
                }
            }
        }

        if (_dict2.Count != 0)
        {
            int index = 0;
            int count = _brushes.Count;

            foreach (var series in _dict2.Keys)
            {
                var brush = (index < count) ? _brushes[index++] : _defaultBrush;

                foreach (var (rect, isSelected) in _dict2[series])
                {
                    if (isSelected == false)
                    {
                        context.DrawRectangle(brush, _defaultPen, rect);
                    }
                    else
                    {
                        context.DrawRectangle(brush, _selectedPen, rect);
                    }
                }
            }
        }
    }

    public void RenderSeries(PlotModel? model, IList<IBrush> brushes)
    {
        _brushes.Clear();
        _brushes.AddRange(brushes);

        if (model != null)
        {
            RenderSeries(model.Series.Where(s => s.IsVisible).ToList());
        }
    }

    public void RenderSeries(IEnumerable<Series> series)
    {
        _dict.Clear();
        _selectedDict.Clear();

        foreach (var s in series)
        {
            var list1 = new List<Rect>();
            var list2 = new List<Rect>();

            var innserSeries = (Core.TimelineSeries)s.InternalSeries;

            foreach (var (rect, isSelected) in innserSeries.Rectangles)
            {
                if (isSelected == false)
                {
                    CreateClippedRectangle(innserSeries.MyClippingRect, ToRect(rect), list1);
                }
                else
                {
                    CreateClippedRectangle(innserSeries.MyClippingRect, ToRect(rect), list2);
                }
            }

            _dict.Add((TimelineSeries)s, list1);

            _selectedDict.Add((TimelineSeries)s, list2);
        }

        InvalidateVisual();
    }

    public void RenderSeries(IEnumerable<Core.Series> series)
    {
        _dict2.Clear();

        foreach (var s in series.Cast<Core.TimelineSeries>())
        {
            var list = new List<(Rect, bool)>();

            foreach (var (rect, isSelected) in s.Rectangles)
            {
                var res = CreateClippedRectangle(s.MyClippingRect, ToRect(rect));

                if (res != null)
                {
                    list.Add(((Rect, bool))(res, isSelected));
                }
            }

            _dict2.Add(s, list);
        }

        InvalidateVisual();
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
