using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AvaloniaDemo.Models;

namespace AvaloniaDemo.ViewModels
{
    public class BaseModelView : INotifyPropertyChanged
    {
        DataPool data;

        RealDataPool realData;

        public enum EDataType { Old, New };

        EDataType DataType;

        public BaseModelView(EDataType type, RealDataPool.ETimePeriod timePeriod = RealDataPool.ETimePeriod.Day)
        {
            this.DataType = type;

            switch (type)
            {
                case EDataType.Old:
                    {
                        data = new DataPool();

                        DataPool.MaxLength = 5000;

                        StringCount = 3;

                        CreateBacks();
                    }
                    break;
                case EDataType.New:
                    {
                        realData = new RealDataPool() { NumSatellites = 4, TimePeriod = timePeriod };

                        StringCount = 4;
                    }
                    break;
                default:
                    break;
            }
        }
        ObservableCollection<IntervalCollection> _strings = new ObservableCollection<IntervalCollection>();
        public ObservableCollection<IntervalCollection> Strings
        {
            get
            {
                return _strings;
            }
            set
            {
                _strings = value;
                OnPropertyChanged("Strings");
            }
        }

        ObservableCollection<BaseInterval> Intervals
        {
            get
            {
                return new ObservableCollection<BaseInterval>(_strings.SelectMany(s => s.Intervals));
            }
        }

        ObservableCollection<BaseInterval> _backgroundIntervals = new ObservableCollection<BaseInterval>();
        public ObservableCollection<BaseInterval> BackgroundIntervals
        {
            get
            {
                return _backgroundIntervals;
            }
        }

        public DateTime Origin = new DateTime(2000, 1, 1, 0, 0, 0);

        public double MinValue
        {
            get
            {
                double min = double.MaxValue;
                foreach (var item in Strings)
                {
                    min = Math.Min(item.Intervals.Min(s => s.Left), min);
                }

                return min;
            }
        }

        public double MaxValue
        {
            get
            {
                double max = double.MinValue;
                foreach (var item in Strings)
                {
                    max = Math.Max(item.Intervals.Max(s => s.Left), max);
                }

                return max;
            }
        }

        void CreateBacks()
        {

            if (DataType == EDataType.Old)
            {
                data.GenerateBacks();

                foreach (var item in data.Backs)
                {
                    _backgroundIntervals.Add(new BaseInterval(item.Left, item.Right)
                    {
                        Name = string.Format("BackgroundInterval_{0}_{1}", item.Left, item.Right)
                    });
                }
            }
            else if (DataType == EDataType.New)
            {
                foreach (var item in realData.BackIntervals)
                {
                    _backgroundIntervals.Add(new BaseInterval(item.Left, item.Right)
                    {
                        Name = string.Format("BackgroundInterval_{0}_{1}", item.Left, item.Right)
                    });
                }
            }

        }

        private int _stringCount;
        public int StringCount
        {
            get
            {
                return _stringCount;
            }
            set
            {
                _stringCount = value;


                Strings.Clear();

                if (DataType == EDataType.Old)
                {

                    data.Generate(_stringCount);

                    //  double h = scheduler.SchedulerRect.SizeY;

                    //   double step = h / (_stringCount + 1);
                    int i = 1;
                    foreach (var key in data.Pool.Keys)
                    {
                        var group = new IntervalCollection("String_" + key.ToString());
                        foreach (var item in data.Pool[key].Select(s => new BaseInterval(s.Left, s.Right)))
                        {
                            //   str.Map = scheduler;
                            //  str.Position = new SCTimeScheduler.NET.SCSchedulerPoint(0.0, (i * step));
                            //var arr = data.Pool[key].Select(s => new Interval(s.Left, s.Right) /*{ String = str }*/);
                            group.AddInterval(item);

                        }
                        Strings.Add(group);

                        i++;
                    }
                }
                else if (DataType == EDataType.New)
                {
                    realData.NumSatellites = _stringCount;

                    realData.GenerateIntervals();

                    int i = 1;
                    foreach (var key in realData.Intervals.Keys)
                    {
                        var group = new IntervalCollection("String_" + key.ToString());
                        foreach (var item in realData.Intervals[key].Select(s => new BaseInterval(s.Left, s.Right)))
                        {
                            group.AddInterval(item);
                        }
                        Strings.Add(group);

                        i++;
                    }

                    CreateBacks();
                }

                OnPropertyChanged("StringCount");
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
