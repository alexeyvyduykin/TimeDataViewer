using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using SatelliteDemo.SceneTimer;
using TimeDataViewer;
using Core = TimeDataViewer.Core;
using Newtonsoft.Json;

namespace SatelliteDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DateTime _epoch;
        private ObservableCollection<Satellite> _satellites; 
        private ObservableCollection<Item> _labels;
        private Satellite? _selected;
        private readonly AcceleratedTimer _timer;
        private readonly System.Timers.Timer _timerThread;
        private string _currentTimeString;
        private double _currentTime;

        public MainWindowViewModel()
        {
            Epoch = DateTime.Now;
            Satellites = new ObservableCollection<Satellite>();
            Labels = new ObservableCollection<Item>()
            {
                new() { Label = "Rotation" },
                new() { Label = "Observation" },
                new() { Label = "Transmission" }
            };

            var path1 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite1.json");
            var path2 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite2.json");
            var path3 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite3.json");
            var path4 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite4.json");

            var json1 = GetJson(path1);
            var json2 = GetJson(path2);
            var json3 = GetJson(path3);
            var json4 = GetJson(path4);

            var jsons = new List<string>() { json1, json2, json3, json4 };

            foreach (var item in jsons)
            {
                var sat = CreateSatelliteFromJson(item, Epoch);

                Satellites.Add(sat);
            }
          
            Selected = Satellites.FirstOrDefault();

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

        public ObservableCollection<Item> Labels
        {
            get => _labels;
            set => RaiseAndSetIfChanged(ref _labels, value);
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

        public Satellite? Selected
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

        private Satellite CreateSatelliteFromJson(string json, DateTime epoch)
        {
            var definition = new
            {
                Name = "",
                Rotations = new[] { new { BeginTime = 0.0, EndTime = 0.0 } },
                Observations = new[] { new { BeginTime = 0.0, EndTime = 0.0 } },
                Transmissions = new[] { new { BeginTime = 0.0, EndTime = 0.0 } }
            };

            var obj = JsonConvert.DeserializeAnonymousType(json, definition);


            var sat = new Satellite()
            {
                Name = obj.Name,
                Rotations = obj.Rotations.Select(s => new Rotation() { BeginTime = epoch.AddSeconds(s.BeginTime), EndTime = epoch.AddSeconds(s.EndTime) }).ToList(),
                Observations = obj.Observations.Select(s => new Observation() { BeginTime = epoch.AddSeconds(s.BeginTime), EndTime = epoch.AddSeconds(s.EndTime) }).ToList(),
                Transmissions = obj.Transmissions.Select(s => new Transmission() { BeginTime = epoch.AddSeconds(s.BeginTime), EndTime = epoch.AddSeconds(s.EndTime) }).ToList()
            };

            InvalidateIntervals(sat);

            return sat;
        }

        private void InvalidateIntervals(Satellite sat)
        {
            List<Rotation> rotations = new List<Rotation>();
            List<Observation> observations = new List<Observation>();
            List<Transmission> transmissions = new List<Transmission>();

            DateTime temp = DateTime.MinValue;

            foreach (var item in sat.Rotations.ToList().OrderBy(s => s.BeginTime))
            {
                if(temp <= item.BeginTime)
                {
                    rotations.Add(item);
                    temp = item.EndTime;
                }              
            }

            temp = DateTime.MinValue;

            foreach (var item in sat.Observations.ToList().OrderBy(s => s.BeginTime))
            {
                if (temp <= item.BeginTime)
                {
                    observations.Add(item);
                    temp = item.EndTime;
                }
            }

            temp = DateTime.MinValue;

            foreach (var item in sat.Transmissions.ToList().OrderBy(s => s.BeginTime))
            {
                if (temp <= item.BeginTime)
                {
                    transmissions.Add(item);
                    temp = item.EndTime;
                }
            }


            var min = rotations.Min(s => ToDouble(s.BeginTime, Epoch));
            min = Math.Min(observations.Min(s => ToDouble(s.BeginTime, Epoch)), min);
            min = Math.Min(transmissions.Min(s => ToDouble(s.BeginTime, Epoch)), min);

            var max = rotations.Max(s => ToDouble(s.EndTime, Epoch));
            max = Math.Max(observations.Max(s => ToDouble(s.EndTime, Epoch)), max);
            max = Math.Max(transmissions.Max(s => ToDouble(s.EndTime, Epoch)), max);

            sat.Rotations = new List<Rotation>(rotations);
            sat.Observations = new List<Observation>(observations);
            sat.Transmissions = new List<Transmission>(transmissions);

            sat.BeginScenario = (int)Math.Floor((Epoch.Date - new DateTime(1900, 1, 1)).TotalDays) + 2;
            sat.EndScenario = sat.BeginScenario + 2;
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

        public static double ToDouble(DateTime value, DateTime timeOrigin)
        {
            var span = value - timeOrigin;
            return span.TotalDays + 1;
        }
    }

    public class Item
    {
        public string Label { get; set; }
    }

    public class Satellite
    {
        public string Name { get; set; } = string.Empty;

        public List<Rotation> Rotations { get; set; } = new List<Rotation>();

        public List<Observation> Observations { get; set; } = new List<Observation>();

        public List<Transmission> Transmissions { get; set; } = new List<Transmission>();

        public int BeginScenario { get; set; }

        public int EndScenario { get; set; }
    }

    public class Rotation : Core.TimelineItem
    {
        public string Category => "Rotation";

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class Observation : Core.TimelineItem
    {
        public string Category => "Observation";

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class Transmission : Core.TimelineItem
    {
        public string Category => "Transmission";

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
