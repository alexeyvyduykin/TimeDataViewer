using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ReactiveUI;
using SatelliteDemo.SceneTimer;

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
        private DateTime _currentTime;

        private DateTime TimeOrigin { get; } = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

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
            var time = _timer.CurrentTime;
            CurrentTime = Epoch.AddSeconds(time);
            var dt = Epoch.AddSeconds(time);
            CurrentTimeString = dt.ToLongDateString() + " " + dt.ToLongTimeString();
        }

        public ObservableCollection<Item> Labels
        {
            get => _labels;
            set => this.RaiseAndSetIfChanged(ref _labels, value);
        }

        public DateTime Epoch
        {
            get => _epoch;
            set => this.RaiseAndSetIfChanged(ref _epoch, value);
        }

        public string CurrentTimeString
        {
            get => _currentTimeString;
            set => this.RaiseAndSetIfChanged(ref _currentTimeString, value);
        }

        public DateTime CurrentTime
        {
            get => _currentTime;
            set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        public ObservableCollection<Satellite> Satellites
        {
            get => _satellites;
            set => this.RaiseAndSetIfChanged(ref _satellites, value);
        }

        public Satellite? Selected
        {
            get => _selected;
            set => this.RaiseAndSetIfChanged(ref _selected, value);
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
                if (temp <= item.BeginTime)
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

            var min = rotations.Min(s => ToTotalDays(s.BeginTime, Epoch));
            min = Math.Min(observations.Min(s => ToTotalDays(s.BeginTime, Epoch)), min);
            min = Math.Min(transmissions.Min(s => ToTotalDays(s.BeginTime, Epoch)), min);

            var max = rotations.Max(s => ToTotalDays(s.EndTime, Epoch));
            max = Math.Max(observations.Max(s => ToTotalDays(s.EndTime, Epoch)), max);
            max = Math.Max(transmissions.Max(s => ToTotalDays(s.EndTime, Epoch)), max);

            sat.Rotations = new List<Rotation>(rotations);
            sat.Observations = new List<Observation>(observations);
            sat.Transmissions = new List<Transmission>(transmissions);

            sat.BeginScenario = ToTotalDays(Epoch.Date, TimeOrigin);
            sat.EndScenario = sat.BeginScenario + 2;

            sat.Begin = ToTotalDays(Epoch, TimeOrigin);
            sat.Duration = 1.0;
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

        public static double ToTotalDays(DateTime value, DateTime timeOrigin)
        {
            return (value - timeOrigin).TotalDays + 1;
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

        public double BeginScenario { get; set; }

        public double EndScenario { get; set; }

        public double Begin { get; set; }

        public double Duration { get; set; }
    }

    public class Rotation
    {
        public string Category => "Rotation";

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class Observation
    {
        public string Category => "Observation";

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class Transmission
    {
        public string Category => "Transmission";

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
