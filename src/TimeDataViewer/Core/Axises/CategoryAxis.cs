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

            return Math.Clamp(MinValue + value, MinValue, MaxValue);
        }

        public override int FromLocalToAbsolute(double value)
        {
            int pixel = (int)((value - MinValue) * (MaxPixel - MinPixel) / (MaxValue - MinValue));

            return Math.Clamp(/*MinPixel +*/ pixel, MinPixel, MaxPixel);
        }

        public override void UpdateWindow(RectI window)
        {                   
            MinPixel = 0;                    
            MaxPixel = window.Height;
        }

        public override void UpdateViewport(RectD viewport)
        {                    
            MinValue = viewport.Bottom;                    
            MaxValue = viewport.Top;
        }

        public override void UpdateClientViewport(RectD clientViewport)
        {                    
            MinClientValue = clientViewport.Bottom;                    
            MaxClientValue = clientViewport.Top;
        }

        private IList<AxisLabelPosition> CreateLabels()
        {
            var list = new List<AxisLabelPosition>();

            foreach (var item in _targetMarkers)
            {
                list.Add(new AxisLabelPosition()
                {
                    Value = item.Value.Y,
                    Label = item.Key,
                });
            }

            return list;
        }
    }
}
