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
        
            return Math.Clamp(MinValue + value, MinValue, MaxValue);
        }

        public override int FromLocalToAbsolute(double value)
        {
            int pixel = (int)((value - MinValue) * (MaxPixel - MinPixel) / (MaxValue - MinValue));
        
            return Math.Clamp(/*MinPixel +*/ pixel, MinPixel, MaxPixel);
        }

        public override void UpdateViewport(RectD viewport)
        {                    
            MinValue = viewport.Left;                    
            MaxValue = viewport.Right;
        }

        public override void UpdateClientViewport(RectD clientViewport)
        {                    
            MinClientValue = clientViewport.Left;                    
            MaxClientValue = clientViewport.Right;
        }

        public override void UpdateWindow(RectI window)
        {                    
            MinPixel = 0;                    
            MaxPixel = window.Width;
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
       
            return labs;
        }

        private string CreateLabel(DateTime begin, double value)
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
            _minLabel = CreateLabel(begin, MinClientValue);
            _maxLabel = CreateLabel(begin, MaxClientValue);

            OnBoundChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDynamicLabelPosition(DateTime begin, Point2D point)
        {
            _dynamicLabel = new AxisLabelPosition()
            {
                Label = string.Format("{0:HH:mm:ss}", begin.AddSeconds(point.X)),
                Value = point.X
            };

            OnBoundChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
