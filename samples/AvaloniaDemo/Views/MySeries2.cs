using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.ViewModels;
using TimeDataViewer;
using TimeDataViewer.Markers;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo.Views
{
    public class MySeries2 : Series
    {
        public override IntervalTooltipViewModel CreateTooltip(SchedulerInterval marker)
        {
            return new IntervalTooltipViewModel2(marker);
        }
    }
}
