#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace TimeDataViewer.ViewModels
{
    public class IntervalViewModel : MarkerViewModel
    {                      
        public IntervalViewModel(double left, double right) : base()
        {
            Left = left;
            Right = right;     
        }
        
        public Series? SeriesControl { get; set; }

        public double Left { get; init; }

        public double Right { get; init; }
    }
}
