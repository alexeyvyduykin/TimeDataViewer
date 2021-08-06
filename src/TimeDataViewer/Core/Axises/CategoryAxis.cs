#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Core
{
    public class CategoryAxis : Axis
    {
        private AxisInfo? _axisInfo;
        private bool _dirty = true; 
        private readonly Dictionary<string, Point2D> _targetMarkers;

        public CategoryAxis()
        {
            _targetMarkers = new Dictionary<string, Point2D>();
            Header = "Y";
            Position = AxisPosition.Left;
            HasInversion = false;
            IsDynamicLabelEnable = true; 
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

        private IList<AxisLabelPosition> CreateLabels()
        {
            var list = new List<AxisLabelPosition>();

            foreach (var item in _targetMarkers)
            {
                list.Add(new AxisLabelPosition()
                {
                    Value = IsHorizontal() ? item.Value.X : item.Value.Y,
                    Label = item.Key,
                });
            }

            return list;
        }

        //public override void UpdateFollowLabelPosition(MarkerViewModel marker)
        //{
        //    if (_targetMarkers.ContainsKey(marker.Name) == false)
        //    {
        //        _targetMarkers.Add(marker.Name, new Point2D());
        //    }

        //    _targetMarkers[marker.Name] = marker.LocalPosition;

        //    _dirty = true;

        //    Invalidate();
        //}

        private AxisInfo CreateAxisInfo()
        {
            return new AxisInfo()
            {
                Labels = CreateLabels(),
                Position = Position,
                MinValue = MinClientValue,
                MaxValue = MaxClientValue,                                 
            };
        }

        public override AxisInfo AxisInfo
        {
            get
            {
                if (_dirty == true || _axisInfo == null)
                {
                    _axisInfo = CreateAxisInfo();
                    _dirty = false;
                }

                return _axisInfo;
            }
        }
    }
}
