#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;
using System.Globalization;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Core
{
    public enum TimePeriod
    {
        Hour,
        Day,
        Week,
        Month,
        Year,
    }

    public class TimeAxis : Axis
    {
        private AxisLabelPosition? _dynamicLabel;
        private DateTime _epoch0 = DateTime.MinValue;
        
        public TimeAxis() 
        {
            Header = "X";
            Position = AxisPosition.Bottom;            
            IsDynamicLabelEnable = true;
            TimePeriodMode = TimePeriod.Month;
            LabelFormatPool = new Dictionary<TimePeriod, string>()
                {
                    { TimePeriod.Hour, @"{0:HH:mm}" },
                    { TimePeriod.Day, @"{0:HH:mm}" },
                    { TimePeriod.Week, @"{0:dd/MMM}" },
                    { TimePeriod.Month, @"{0:dd}" },
                    { TimePeriod.Year, @"{0:dd/MMM}" },
                };
            LabelDeltaPool = new Dictionary<TimePeriod, double>()
                {
                    { TimePeriod.Hour, 60.0 * 5 },
                    { TimePeriod.Day, 3600.0 * 2 },
                    { TimePeriod.Week, 86400.0 },
                    { TimePeriod.Month, 86400.0 },
                    { TimePeriod.Year, 86400.0 * 12 },
                };
        }

        public IDictionary<TimePeriod, string>? LabelFormatPool { get; init; }

        public IDictionary<TimePeriod, double>? LabelDeltaPool { get; init; }

        public DateTime Epoch0 
        { 
            get => _epoch0; 
            set 
            {
                _epoch0 = value;
                Invalidate();
            }
        }

        public TimePeriod TimePeriodMode { get; set; }
   
        private IList<AxisLabelPosition> CreateLabels()
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
                    Label = string.Format(CultureInfo.InvariantCulture, LabelFormatPool[TimePeriodMode], Epoch0.AddSeconds(value)),
                    Value = value
                });

                value += delta;
            }

            return labs;
        }

        private string CreateMinMaxLabel(double value)
        {
            if ((MaxClientValue - MinClientValue) == 0.0)
            {
                return string.Empty;
            }

            return Epoch0.AddSeconds(value).ToString(@"dd/MMM/yyyy", CultureInfo.InvariantCulture);
        }

        public void UpdateDynamicLabelPosition(Point2D point)
        {
            if (IsVertical() == true)
            {
                _dynamicLabel = new AxisLabelPosition()
                {
                    Label = string.Format("{0:HH:mm:ss}", Epoch0.AddSeconds(point.Y)),
                    Value = point.Y
                };
            }
            else
            {
                _dynamicLabel = new AxisLabelPosition()
                {
                    Label = string.Format("{0:HH:mm:ss}", Epoch0.AddSeconds(point.X)),
                    Value = point.X
                };
            }

            Invalidate();
        }

       // public override void UpdateFollowLabelPosition(MarkerViewModel marker) { }
  
        public override AxisInfo AxisInfo
        {
            get
            {
                var axisInfo = new AxisInfo()
                {
                    Labels = CreateLabels(),
                    Position = Position,
                    MinValue = MinClientValue,
                    MaxValue = MaxClientValue,
                    MinLabel = CreateMinMaxLabel(MinClientValue),
                    MaxLabel = CreateMinMaxLabel(MaxClientValue),               
                    DynamicLabel = (IsDynamicLabelEnable == true) ? _dynamicLabel : null,
                };

                return axisInfo;
            }
        }
    }
}
