using System;
using System.Collections.ObjectModel;
using AvaloniaDemo.Models;

namespace AvaloniaDemo.ViewModels
{
    public class BaseModelView : ViewModelBase
    {
        private ObservableCollection<TimeInterval> _interval1;
        private ObservableCollection<TimeInterval> _interval2;
        private ObservableCollection<TimeInterval> _interval3;
        private DateTime _epoch;
        //private ObservableCollection<BaseInterval> _backgroundIntervals;

        public BaseModelView()
        {
            _epoch = DateTime.Now;
            _interval1 = new();
            _interval2 = new();
            _interval3 = new();
            // _backgroundIntervals = new();
        }

        //public ObservableCollection<BaseInterval> BackgroundIntervals => _backgroundIntervals;

        public DateTime Epoch
        {
            get => _epoch;
            set => RaiseAndSetIfChanged(ref _epoch, value);
        }

        public ObservableCollection<TimeInterval> Interval1
        {
            get => _interval1;
            set => RaiseAndSetIfChanged(ref _interval1, value);
        }

        public ObservableCollection<TimeInterval> Interval2
        {
            get => _interval2;
            set => RaiseAndSetIfChanged(ref _interval2, value);
        }

        public ObservableCollection<TimeInterval> Interval3
        {
            get => _interval3;
            set => RaiseAndSetIfChanged(ref _interval3, value);
        }
    }
}
