using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.ViewModels;
using TimeDataViewer;
using TimeDataViewer.Shapes;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo.Views
{
    public class MySeries2 : Series
    {
        public override IntervalTooltipViewModel CreateTooltip(IntervalViewModel marker)
        {
            return new IntervalTooltipViewModel2(marker);
        }
    }
}
