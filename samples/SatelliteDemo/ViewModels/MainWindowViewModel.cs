using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SatelliteDemo.Models;
using System.Collections.ObjectModel;
using System.Linq;
using TimeDataViewer.ViewModels;
using System.ComponentModel;
using SatelliteDemo.SceneTimer;
using TimeDataViewer;
using Core = TimeDataViewer.Core;

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
        private IDictionary<Satellite, IEnumerable<object>> _dict;
        private readonly AcceleratedTimer _timer;
        private readonly System.Timers.Timer _timerThread;
        private string _currentTimeString;
        private double _currentTime;

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

            _dict = new Dictionary<Satellite, IEnumerable<object>>();

            foreach (var sat in Satellites)
            {
                _dict.Add(sat, NewInit(sat));
            }

            NewIntervals = new ObservableCollection<object>(_dict[Selected]);

            _timer = new AcceleratedTimer();

            _timerThread = new System.Timers.Timer(1000.0 / 60.0);

            _timerThread.Elapsed += TimerThreadElapsed;

            _timerThread.AutoReset = true;
            _timerThread.Enabled = true;
        }

        private void TimerThreadElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CurrentTime = _timer.CurrentTime;
            var dt = Epoch.AddSeconds(CurrentTime);
            CurrentTimeString = dt.ToLongDateString() + " " + dt.ToLongTimeString();
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Selected))
            {
                NewIntervals = new ObservableCollection<object>(_dict[Selected]);
            }
        }

        private IEnumerable<object> NewInit(Satellite satellite)
        {
            var ivals = new List<object>();

            var series1 = new Core.TimelineSeries();// { Name = "Rotations" };

            ivals.Add(series1);

            series1.ReplaceIntervals(satellite.Rotations.Select(s => new Core.TimelineItem(s.BeginTime, s.EndTime)));

            ivals.AddRange(series1.Items);

            var series2 = new Core.TimelineSeries();// { Name = "Observations" };

            ivals.Add(series2);

            series2.ReplaceIntervals(satellite.Observations.Select(s => new Core.TimelineItem(s.BeginTime, s.EndTime)));

            ivals.AddRange(series2.Items);

            var series3 = new Core.TimelineSeries();// { Name = "Transmissions" };

            ivals.Add(series3);

            series3.ReplaceIntervals(satellite.Transmissions.Select(s => new Core.TimelineItem(s.BeginTime, s.EndTime)));

            ivals.AddRange(series3.Items);

            return ivals;
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

        public string CurrentTimeString
        {
            get => _currentTimeString;
            set => RaiseAndSetIfChanged(ref _currentTimeString, value);
        }

        public double CurrentTime
        {
            get => _currentTime;
            set => RaiseAndSetIfChanged(ref _currentTime, value);
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

        public void OnReset()
        {
            _timer.Reset();
        }

        public void OnPlay()
        {
            _timer.Start();
        }

        public void OnPause()
        {
            _timer.Pause();
        }

        public void OnSlower()
        {
            _timer.Slower();
        }

        public void OnFaster()
        {
            _timer.Faster();
        }
    }
}
