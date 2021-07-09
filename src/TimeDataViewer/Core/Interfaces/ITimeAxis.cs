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
        //event EventHandler OnBoundChanged;

        IDictionary<TimePeriod, string>? LabelFormatPool { get; }

        IDictionary<TimePeriod, double>? LabelDeltaPool { get; }

        TimePeriod TimePeriodMode { get; set; }

        void UpdateDynamicLabelPosition(DateTime begin, Point2D point); 

        void UpdateStaticLabels(DateTime begin);

        string MinLabel { get; }

        string MaxLabel { get; }

        IList<AxisLabelPosition> Labels { get; }
    }
}
