#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Timeline.Spatial;
using Timeline.ViewModels;

namespace Timeline.Core
{
    public class CategoryAxis : BaseAxis, ICategoryAxis
    {
        private readonly Area _area;
        private readonly Dictionary<string, Point2D> _targetMarkers;

        public CategoryAxis(Area area)
        {
            _area = area;
            _targetMarkers = new Dictionary<string, Point2D>();          
        }

        public override double FromAbsoluteToLocal(int pixel)
        {
            double value = (MaxValue - MinValue) * pixel / (MaxPixel - MinPixel);

            if (HasInversion == true)
            {                
                value = (MaxValue - MinValue) - value;
            }

            return Math.Clamp(MinValue + value, MinValue, MaxValue);
        }

        public override int FromLocalToAbsolute(double value)
        {
            int pixel = (int)((value - MinValue) * (MaxPixel - MinPixel) / (MaxValue - MinValue));

            if (HasInversion == true)
            {
                pixel = (MaxPixel - MinPixel) - pixel;
            }

            return Math.Clamp(/*MinPixel +*/ pixel, MinPixel, MaxPixel);
        }

        public override void UpdateWindow(RectI window)
        {
            switch (Type)
            {
                case AxisType.X:
                    MinPixel = 0;
                    MaxPixel = window.Width;
                    break;
                case AxisType.Y:
                    MinPixel = 0;
                    MaxPixel = window.Height;
                    break;
                default:
                    break;
            }
        }

        public override void UpdateViewport(RectD viewport)
        {
            switch (Type)
            {
                case AxisType.X:
                    MinValue = viewport.Left;
                    MaxValue = viewport.Right;
                    break;
                case AxisType.Y:
                    MinValue = viewport.Bottom;
                    MaxValue = viewport.Top;
                    break;
                default:
                    break;
            }
        }

        public override void UpdateClientViewport(RectD clientViewport)
        {
            switch (Type)
            {
                case AxisType.X:
                    MinClientValue = clientViewport.Left;
                    MaxClientValue = clientViewport.Right;
                    break;
                case AxisType.Y:
                    MinClientValue = clientViewport.Bottom;
                    MaxClientValue = clientViewport.Top;
                    break;
                default:
                    break;
            }
        }

        private IList<AxisLabelPosition> CreateLabels()
        {
            var list = new List<AxisLabelPosition>();

            foreach (var item in _targetMarkers)
            {
                list.Add(new AxisLabelPosition()
                {
                    Value = (Type == AxisType.X) ? item.Value.X : item.Value.Y,
                    Label = item.Key,
                });
            }

            return list;
        }
    }
}
