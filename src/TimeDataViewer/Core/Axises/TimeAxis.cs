#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Timeline.Spatial;
using System.Globalization;
using Timeline.ViewModels;

namespace Timeline.Core
{
    public enum TimePeriod
    {
        Hour,
        Day,
        Week,
        Month,
        Year,
    }

    public class TimeAxis : BaseAxis, ITimeAxis
    {
        private readonly Area _area;
        private AxisLabelPosition? _dynamicLabel;
        private string _minLabel;
        private string _maxLabel;
        private IList<AxisLabelPosition> _labels;
        
        public event EventHandler OnBoundChanged;

        public TimeAxis(Area area) 
        {
            _area = area;
               _labels = new List<AxisLabelPosition>();
        }

        public IDictionary<TimePeriod, string>? LabelFormatPool { get; init; }

        public IDictionary<TimePeriod, double>? LabelDeltaPool { get; init; }

        public IList<AxisLabelPosition> Labels => _labels;

        public string MinLabel => _minLabel;

        public string MaxLabel => _maxLabel;

        public TimePeriod TimePeriodMode { get; set; }

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
   
        private IList<AxisLabelPosition> CreateLabels(DateTime begin)
        {
            var labs = new List<AxisLabelPosition>();

            if ((MaxClientValue - MinClientValue) == 0.0)
            {
                return labs;
            }

            if (LabelDeltaPool == null || LabelDeltaPool.ContainsKey(TimePeriodMode) == false || 
                LabelFormatPool == null || LabelFormatPool.ContainsKey(TimePeriodMode) == false)
            {
                return labs;
            }

            double delta = LabelDeltaPool[TimePeriodMode];

            int fl = (int)Math.Floor(MinClientValue / delta);

            double value = fl * delta;

            if (value < MinClientValue)
            {
                value += delta;
            }
            
            while (value <= MaxClientValue)
            {
                labs.Add(new AxisLabelPosition()
                {
                    Label = string.Format(CultureInfo.InvariantCulture, LabelFormatPool[TimePeriodMode], begin.AddSeconds(value)),
                    Value = value
                });

                value += delta;
            }

            _labels = new List<AxisLabelPosition>(labs);
 

            return labs;
        }

        private string CreateMinMaxLabel(DateTime begin, double value)
        {
            if ((MaxClientValue - MinClientValue) == 0.0)
            {
                return string.Empty;
            }

            return begin.AddSeconds(value).ToString(@"dd/MMM/yyyy", CultureInfo.InvariantCulture);
        }

        public void UpdateStaticLabels(DateTime begin)
        {
            _labels = CreateLabels(begin);
     //       MinValue = MinClientValue;
     //       MaxValue = MaxClientValue;
            _minLabel = CreateMinMaxLabel(begin, MinClientValue);
            _maxLabel = CreateMinMaxLabel(begin, MaxClientValue); 
        }

        public void UpdateDynamicLabelPosition(DateTime begin, Point2D point)
        {
            if (Type == AxisType.Y)
            {
                _dynamicLabel = new AxisLabelPosition()
                {
                    Label = string.Format("{0:HH:mm:ss}", begin.AddSeconds(point.Y)),
                    Value = point.Y
                };
            }
            else if (Type == AxisType.X)
            {
                _dynamicLabel = new AxisLabelPosition()
                {
                    Label = string.Format("{0:HH:mm:ss}", begin.AddSeconds(point.X)),
                    Value = point.X
                };
            }

            OnBoundChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
