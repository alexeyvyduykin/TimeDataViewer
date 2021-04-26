using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SatelliteDemo.Models;
using System.Collections.ObjectModel;
using System.Linq;
using TimeDataViewer.ViewModels;
using System.ComponentModel;

namespace SatelliteDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DateTime _epoch;
        private ObservableCollection<Satellite> _satellites;
        private Satellite _selected;
        private ObservableCollection<object> _newIntervals;
        private int _absolutePositionX;
        private int _absolutePositionY;
        private int _zIndex;

        public MainWindowViewModel()
        {          
            var path1 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite1.json");
            var path2 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite2.json");
            var path3 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite3.json");
            var path4 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite4.json");

            var json1 = GetJson(path1);
            var json2 = GetJson(path2);
            var json3 = GetJson(path3);
            var json4 = GetJson(path4);

            Serializer serializer = new();

            var sat1 = serializer.Deserialize<Satellite>(json1);
            var sat2 = serializer.Deserialize<Satellite>(json2);
            var sat3 = serializer.Deserialize<Satellite>(json3);
            var sat4 = serializer.Deserialize<Satellite>(json4);

            InvalidateIntervals(sat1);
            InvalidateIntervals(sat2);
            InvalidateIntervals(sat3);
            InvalidateIntervals(sat4);

            Satellites = new ObservableCollection<Satellite>(new(){ sat1, sat2, sat3, sat4 });

            Selected = sat1;

            Epoch = DateTime.Now;

            PropertyChanged += MainWindowViewModel_PropertyChanged;
            
            NewInit();
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Selected))
            {
                NewInit();
            }
        }

        private void NewInit()
        {
            var ivals = new List<object>();

            var series1 = new SeriesViewModel("Rotations");

            ivals.Add(series1);

            foreach (var item in Selected.Rotations)
            {
                var ival = new IntervalViewModel(item.BeginTime, item.EndTime);
                series1.Intervals.Add(ival);
                ival.Series = series1;
                ivals.Add(ival);
            }

            var series2 = new SeriesViewModel("Observations");

            ivals.Add(series2);

            foreach (var item in Selected.Observations)
            {
                var ival = new IntervalViewModel(item.BeginTime, item.EndTime);
                series2.Intervals.Add(ival);
                ival.Series = series2;
                ivals.Add(ival);
            }

            var series3 = new SeriesViewModel("Transmissions");

            ivals.Add(series3);

            foreach (var item in Selected.Transmissions)
            {
                var ival = new IntervalViewModel(item.BeginTime, item.EndTime);
                series3.Intervals.Add(ival);
                ival.Series = series3;
                ivals.Add(ival);
            }

            NewIntervals = new ObservableCollection<object>(ivals);
        }


        public ObservableCollection<object> NewIntervals
        {
            get => _newIntervals;
            set => RaiseAndSetIfChanged(ref _newIntervals, value);
        }

        public DateTime Epoch
        {
            get => _epoch;
            set => RaiseAndSetIfChanged(ref _epoch, value);
        }

        public ObservableCollection<Satellite> Satellites
        {
            get => _satellites;
            set => RaiseAndSetIfChanged(ref _satellites, value);
        }

        public Satellite Selected
        {
            get => _selected;
            set => RaiseAndSetIfChanged(ref _selected, value);
        }

        private string GetJson(string path)
        {
            using var fs = File.OpenRead(path);
            using var sr = new StreamReader(fs, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        public virtual int AbsolutePositionX
        {
            get => _absolutePositionX;
            set => RaiseAndSetIfChanged(ref _absolutePositionX, value);
        }

        public virtual int AbsolutePositionY
        {
            get => _absolutePositionY;
            set => RaiseAndSetIfChanged(ref _absolutePositionY, value);
        }

        public int ZIndex
        {
            get => _zIndex;
            set => RaiseAndSetIfChanged(ref _zIndex, value);
        }

        private void InvalidateIntervals(Satellite sat)
        {
            List<Rotation> rotations = new List<Rotation>();
            List<Observation> observations = new List<Observation>();
            List<Transmission> transmissions = new List<Transmission>();

            double temp = 0.0;

            foreach (var item in sat.Rotations.ToList().OrderBy(s => s.BeginTime))
            {
                if(temp <= item.BeginTime)
                {
                    rotations.Add(item);
                    temp = item.EndTime;
                }              
            }

            temp = 0.0;

            foreach (var item in sat.Observations.ToList().OrderBy(s => s.BeginTime))
            {
                if (temp <= item.BeginTime)
                {
                    observations.Add(item);
                    temp = item.EndTime;
                }
            }

            temp = 0.0;

            foreach (var item in sat.Transmissions.ToList().OrderBy(s => s.BeginTime))
            {
                if (temp <= item.BeginTime)
                {
                    transmissions.Add(item);
                    temp = item.EndTime;
                }
            }

            sat.Rotations = new ObservableCollection<Rotation>(rotations);
            sat.Observations = new ObservableCollection<Observation>(observations);
            sat.Transmissions = new ObservableCollection<Transmission>(transmissions);
        }
    }
}
