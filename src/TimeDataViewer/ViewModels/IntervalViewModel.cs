#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Models;

namespace TimeDataViewer.ViewModels
{
    public class IntervalViewModel : MarkerViewModel//, IInterval
    {
        private SeriesViewModel? _series;
        private Series? _seriesControl;
        private readonly double _left;
        private readonly double _right;

        public IntervalViewModel(double left, double right) : base()
        {
            _left = left;
            _right = right;     
        }
        
        public SeriesViewModel? Series
        {
            get => _series;            
            set => _series = value;            
        }

        public Series? SeriesControl
        {
            get => _seriesControl;
            set => _seriesControl = value;
        }

        public double Left => _left;

        public double Right => _right;

        public double Length => _right - _left;
    }
}
