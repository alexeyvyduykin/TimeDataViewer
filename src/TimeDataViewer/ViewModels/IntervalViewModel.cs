#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Models;

namespace TimeDataViewer.ViewModels
{
    public class IntervalViewModel : MarkerViewModel
    {
        private SeriesViewModel? _series;
        private ISeriesControl? _seriesControl;
        private readonly double _left;
        private readonly double _right;

        public IntervalViewModel(double left, double right) : base()
        {
            _left = left;
            _right = right;     
        }
        
        public SeriesViewModel? Series
        {
            get
            {
                return _series;
            }
            set
            {
                _series = value;


                base.SetLocalPosition(Left + (Right - Left) / 2.0, _series.LocalPosition.Y);

                //   base.PositionX = Left + (Right - Left) / 2.0;
                //   base.PositionY = _string.PositionY;

                //  base.Position = new SCSchedulerPoint(Left + (Right - Left) / 2.0, _string.Position.Y);
            }
        }

        public ISeriesControl? SeriesControl
        {
            get => _seriesControl;
            set => _seriesControl = value;
        }

        public double Left => _left;

        public double Right => _right;

        public double Length => _right - _left;
    }
}
