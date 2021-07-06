using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.ViewModels;
using Timeline;
using Timeline.Models;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo.Views
{
    public class MySeries2 : Series
    {
        public override IntervalTooltipViewModel CreateTooltip(IInterval marker)
        {
            return new IntervalTooltipViewModel2(marker);
        }
    }
}
