#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing;
using System.ComponentModel;
using Avalonia.Media;
using System.Windows;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.ViewModels
{
    public class SeriesViewModel : MarkerViewModel
    {
        private readonly List<IntervalViewModel> _intervals;

        public SeriesViewModel(string name) : base()
        {
            Name = name;
            _intervals = new List<IntervalViewModel>();
        }

        public List<IntervalViewModel> Intervals => _intervals;

        public override Point2D Offset
        {
            get
            {
                return base.Offset;
            }
            set
            {
                base.Offset = value;

                _intervals.ForEach(s => s.Offset = value);
            }
        }

        public double MinTime() => _intervals.Min(s => s.Left);

        public double MaxTime() => _intervals.Max(s => s.Right);        
    }
}
