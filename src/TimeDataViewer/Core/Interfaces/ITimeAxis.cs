using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Spatial;

namespace Timeline.Core
{
    public interface ITimeAxis : IAxis
    {
        IDictionary<TimePeriod, string>? LabelFormatPool { get; }
        IDictionary<TimePeriod, double>? LabelDeltaPool { get; }
        DateTime Epoch0 { get; set; }
        TimePeriod TimePeriodMode { get; set; }
        void UpdateDynamicLabelPosition(Point2D point); 
        string MinLabel { get; }

        string MaxLabel { get; }
        IList<AxisLabelPosition> Labels { get; }
    }
}
