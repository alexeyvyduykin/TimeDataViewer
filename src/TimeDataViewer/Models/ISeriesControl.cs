﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.ViewModels;
using Avalonia.Controls;

namespace Timeline.Models
{
    public interface ISeriesControl
    {
        TimelineControl? Scheduler { get; }
       
        Control Tooltip { get; set; }

        IShape CreateIntervalShape(IInterval interval);

        IShape CreateSeriesShape(); 
          
        IntervalTooltipViewModel CreateTooltip(IInterval marker);
    }
}
