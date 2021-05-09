#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Core
{
    public class CategoryAxis : BaseAxis, ICategoryAxis
    {
        private AxisInfo? _axisInfo;
        private bool _dirty = true; 
        private readonly Dictionary<string, Point2D> _targetMarkers;

        public CategoryAxis()
        {
            _targetMarkers = new Dictionary<string, Point2D>();          
        }

        public override double FromAbsoluteToLocal(int pixel)
        {
            double value = (MaxValue - MinValue) * pixel / (MaxPixel - MinPixel);

            if (HasInversion == true)
            {                
                value = (MaxValue - MinValue) - value;
            }

            var res = MinValue + value;

            res = MathHelper.MathHelper.Clip(res, MinValue, MaxValue);

            return res;
        }

        public override int FromLocalToAbsolute(double value)
        {
            int pixel = (int)((value - MinValue) * (MaxPixel - MinPixel) / (MaxValue - MinValue));

            if (HasInversion == true)
            {
                pixel = (MaxPixel - MinPixel) - pixel;
            }

            var res = /*MinPixel +*/ pixel;

            res = MathHelper.MathHelper.Clip(res, MinPixel, MaxPixel);

            return res;
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

            Invalidate();
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

            Invalidate();
        }

        public override void UpdateScreen(RectD screen)
        {
            switch (Type)
            {
                case AxisType.X:
                    MinScreenValue = screen.Left;
                    MaxScreenValue = screen.Right;
                    break;
                case AxisType.Y:
                    MinScreenValue = screen.Bottom;
                    MaxScreenValue = screen.Top;
                    break;
                default:
                    break;
            }

            _dirty = true;

            Invalidate();
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
                Type = Type,
                MinValue = MinScreenValue,
                MaxValue = MaxScreenValue,                                 
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
