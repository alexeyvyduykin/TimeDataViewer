using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using DynamicData;
using FootprintViewerDemo.Models;
using Newtonsoft.Json;
using ReactiveUI;

namespace FootprintViewerDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DateTime _epoch;
    private ObservableCollection<ItemViewModel> _labels;
    private Satellite? _selected;
    private DateTime _currentTime;
    private readonly SourceList<Satellite> _satellites = new();
    private readonly ReadOnlyObservableCollection<Satellite> _items;
    private DateTime TimeOrigin { get; } = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public MainWindowViewModel()
    {
        Epoch = DateTime.Now;

        Labels = new ObservableCollection<ItemViewModel>()
        {
            new() { Label = "Rotation" },
            new() { Label = "Observation" },
            new() { Label = "Transmission" }
        };

        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var path1 = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "satellite1.json"));
        var path2 = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "satellite2.json"));
        var path3 = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "satellite3.json"));
        var path4 = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "satellite4.json"));

        var json1 = GetJson(path1);
        var json2 = GetJson(path2);
        var json3 = GetJson(path3);
        var json4 = GetJson(path4);

        var jsons = new List<string>() { json1, json2, json3, json4 };
        var satellites = jsons
            .Select(s => CreateSatelliteFromJson(s, Epoch))
            .ToList();

        _satellites
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe();

        _satellites.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(satellites);
        });

        Selected = Satellites.FirstOrDefault();
    }

    public ObservableCollection<ItemViewModel> Labels
    {
        get => _labels;
        set => this.RaiseAndSetIfChanged(ref _labels, value);
    }

    public DateTime Epoch
    {
        get => _epoch;
        set => this.RaiseAndSetIfChanged(ref _epoch, value);
    }

    public DateTime CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }

    public ReadOnlyObservableCollection<Satellite> Satellites => _items;

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

    public static double ToTotalDays(DateTime value, DateTime timeOrigin)
    {
        return (value - timeOrigin).TotalDays + 1;
    }
}
