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
using TimeDataViewer.Models;


namespace TimeDataViewer.ViewModels
{
    public class SeriesViewModel : MarkerViewModel
    {
        private readonly List<IntervalViewModel> _intervals;
        private ISeriesControl? _series;

        public SeriesViewModel() : base()
        {         
            _intervals = new List<IntervalViewModel>();         
        }

        public ISeriesControl? SeriesControl
        {
            get => _series;
            set => _series = value;
        }

        public List<IntervalViewModel> Intervals => _intervals;

        public double MinTime() => (_intervals.Count == 0) ? 0.0 : _intervals.Min(s => s.Left);

        public double MaxTime() => (_intervals.Count == 0) ? 0.0 : _intervals.Max(s => s.Right);        
    }
}
