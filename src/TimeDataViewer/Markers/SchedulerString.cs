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

namespace TimeDataViewer.Markers
{
    public class SchedulerString : SchedulerTargetMarker
    {
      //  private double _timeBegin = double.NaN;
      //  private double _timeEnd = double.NaN;
 
        public SchedulerString(string name) : base()
        {
            base.Name = name;
        }

        //public double TimeBegin
        //{
        //    get
        //    {
        //        if (double.IsNaN(_timeBegin) == true)
        //        {
        //            _timeBegin = MinTime();
        //        }

        //        return _timeBegin;
        //    }
        //    set
        //    {
        //        if (double.IsNaN(_timeEnd) == false)
        //        {
        //            if (_timeBegin <= _timeEnd)
        //            {
        //                _timeBegin = value;
        //            }
        //        }
        //        else
        //        {
        //            _timeBegin = value;
        //        }
        //    }
        //}

        //public double TimeEnd
        //{
        //    get
        //    {
        //        if (double.IsNaN(_timeEnd) == true)
        //        {
        //            _timeEnd = MaxTime();
        //        }

        //        return _timeEnd;
        //    }
        //    set
        //    {
        //        if (double.IsNaN(_timeBegin) == false)
        //        {
        //            if (_timeEnd >= _timeBegin)
        //            {
        //                _timeEnd = value;
        //            }
        //        }
        //        else
        //        {
        //            _timeEnd = value;
        //        }
        //    }
        //}

        public readonly ObservableCollection<SchedulerInterval> Intervals = new ObservableCollection<SchedulerInterval>();

        public override Point2D Offset
        {
            get
            {
                return base.Offset;
            }
            set
            {
                base.Offset = value;

                Intervals.ToList().ForEach(s => s.Offset = value);
            }
        }

        public double MinTime() => Intervals.Min(s => s.Left);

        public double MaxTime() => Intervals.Max(s => s.Right);        
    }


}
