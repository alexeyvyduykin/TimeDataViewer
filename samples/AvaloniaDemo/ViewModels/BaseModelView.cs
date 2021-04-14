﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AvaloniaDemo.Models;
using System.Collections.Immutable;

namespace AvaloniaDemo.ViewModels
{
    public class BaseModelView : ViewModelBase
    {     
        private ObservableCollection<TimeInterval> _interval1;
        private ObservableCollection<TimeInterval> _interval2;
        private ObservableCollection<TimeInterval> _interval3;        
        //private ObservableCollection<BaseInterval> _backgroundIntervals;

        public BaseModelView()
        {
            _interval1 = new();
            _interval2 = new();
            _interval3 = new();
           // _backgroundIntervals = new();
        }
                     
        //public ObservableCollection<BaseInterval> BackgroundIntervals => _backgroundIntervals;

        public DateTime Origin { get; } = DateTime.Now;

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