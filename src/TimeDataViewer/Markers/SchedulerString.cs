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
        private double _timeBegin = double.NaN;
        private double _timeEnd = double.NaN;
        private Random _r = new Random();

        public SchedulerString(string name) : base()
        {
            base.Name = name;

            string[] descr = new string[]
            {
                "Satellites times vision",
                "Sunlight satellite subpoint",
                "Satellites angle rotation",
                "Satellite received",
                "Sensor daylight",
                "GroundStation work",
                "Satellite orbit correction"
            };

            var index = _r.Next(0, descr.Length - 1);

            Description = descr[index];
        }

        public double TimeBegin
        {
            get
            {
                if (double.IsNaN(_timeBegin) == true)
                {
                    _timeBegin = MinTime();
                }

                return _timeBegin;
            }
            set
            {
                if (double.IsNaN(_timeEnd) == false)
                {
                    if (_timeBegin <= _timeEnd)
                    {
                        _timeBegin = value;
                    }
                }
                else
                {
                    _timeBegin = value;
                }
            }
        }

        public double TimeEnd
        {
            get
            {
                if (double.IsNaN(_timeEnd) == true)
                {
                    _timeEnd = MaxTime();
                }

                return _timeEnd;
            }
            set
            {
                if (double.IsNaN(_timeBegin) == false)
                {
                    if (_timeEnd >= _timeBegin)
                    {
                        _timeEnd = value;
                    }
                }
                else
                {
                    _timeEnd = value;
                }
            }
        }

        //new void UpdateLocalPosition()
        //{
        //    if (Map != null)
        //    {
        //      //  SCPoint p = Map.FromSchedulerPointToLocal(new SCSchedulerPoint(Position__.X, Map.ActualHeight * Position__.Y));
        //        int x = Map.FromLocalValueToPixelX(base.PositionX);
        //        int y = Map.FromLocalValueToPixelY(base.Map.ActualHeight * (double)base.PositionY);
        //        var p = new SCPoint(x, y);

        //        p.Offset(-(int)Map.SchedulerTranslateTransform.X, -(int)Map.SchedulerTranslateTransform.Y);

        //        LocalPositionX = (int)(p.X + (int)(Offset.X));
        //        LocalPositionY = (int)(p.Y + (int)(Offset.Y));
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

        public string Description { get; set; }
    }


}
