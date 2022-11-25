using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using FootprintViewerDemo.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace FootprintViewerDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SourceList<string> _satellites = new();
    private readonly ReadOnlyObservableCollection<string> _items;
    private readonly SourceList<Footprint> _footprints = new();
    private readonly ReadOnlyObservableCollection<FootprintViewModel> _footprintItems;
    private readonly ReadOnlyObservableCollection<Interval> _items1;
    private readonly ReadOnlyObservableCollection<Interval> _items2;
    private readonly ReadOnlyObservableCollection<Interval> _items3;
    private readonly ReadOnlyObservableCollection<Interval> _items4;
    private readonly ReadOnlyObservableCollection<Interval> _items5;

    private DateTime TimeOrigin { get; } = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public MainWindowViewModel()
    {
        Epoch = DateTime.Now.Date;

        Labels = new ObservableCollection<ItemViewModel>()
        {
            new() { Label = "Satellite1" },
            new() { Label = "Satellite2" },
            new() { Label = "Satellite3" },
            new() { Label = "Satellite4" },
            new() { Label = "Satellite5" }
        };

        BeginScenario = 0.0;
        EndScenario = 2 * 86400.0;

        Begin = 0.0;
        Duration = 1;

        _satellites
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Transform(s => new FootprintViewModel(s))
            .Bind(out _footprintItems)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite1"))
            .Transform(s => CreateInterval(s))
            .Bind(out _items1)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite2"))
            .Transform(s => CreateInterval(s))
            .Bind(out _items2)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite3"))
            .Transform(s => CreateInterval(s))
            .Bind(out _items3)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite4"))
            .Transform(s => CreateInterval(s))
            .Bind(out _items4)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite5"))
            .Transform(s => CreateInterval(s))
            .Bind(out _items5)
            .DisposeMany()
            .Subscribe();

        Update = ReactiveCommand.CreateFromTask(UpdateImpl, outputScheduler: RxApp.MainThreadScheduler);

        Selected = Satellites.FirstOrDefault();

        Observable.StartAsync(UpdateImpl, RxApp.MainThreadScheduler);
    }

    public ReactiveCommand<Unit, Unit> Update { get; }

    private async Task UpdateImpl()
    {
        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var path = Path.GetFullPath(Path.Combine(root, @"..\..\..\Assets", "Footprints.json"));

        var serializer = new FootprintSerializer(path);
        var res = await serializer.GetValuesAsync();
        var list = res.Cast<Footprint>().ToList();

        var min = list.Select(s => s.Begin).Min();
        var max = list.Select(s => s.Begin).Max();

        var satellites = list.Select(s => s.SatelliteName!).Distinct().ToList() ?? new List<string>();

        Epoch = min.Date;

        BeginScenario = ToTotalDays(Epoch.Date, TimeOrigin) - 1;
        EndScenario = BeginScenario + 3;

        Begin = ToTotalDays(Epoch, TimeOrigin);
        Duration = 1.0;

        _footprints.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(list);
        });

        _satellites.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(satellites);
        });
    }

    private Interval CreateInterval(Footprint footprint)
    {
        var secs = footprint.Begin.TimeOfDay.TotalSeconds;

        var date = Epoch.Date;

        return new Interval()
        {
            Category = footprint.SatelliteName,
            BeginTime = date.AddSeconds(secs),
            EndTime = date.AddSeconds(secs + footprint.Duration)
        };
    }

    [Reactive]
    public ObservableCollection<ItemViewModel> Labels { get; set; }

    [Reactive]
    public DateTime Epoch { get; set; }

    [Reactive]
    public double BeginScenario { get; set; }

    [Reactive]
    public double EndScenario { get; set; }

    [Reactive]
    public double Begin { get; set; } = 0.0;

    [Reactive]
    public double Duration { get; set; } = 86400.0;

    public ReadOnlyObservableCollection<string> Satellites => _items;

    public ReadOnlyObservableCollection<FootprintViewModel> Footprints => _footprintItems;

    public ReadOnlyObservableCollection<Interval> Intervals1 => _items1;

    public ReadOnlyObservableCollection<Interval> Intervals2 => _items2;

    public ReadOnlyObservableCollection<Interval> Intervals3 => _items3;

    public ReadOnlyObservableCollection<Interval> Intervals4 => _items4;

    public ReadOnlyObservableCollection<Interval> Intervals5 => _items5;

    [Reactive]
    public string? Selected { get; set; }

    private string GetJson(string path)
    {
        using var fs = File.OpenRead(path);
        using var sr = new StreamReader(fs, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    //private Satellite CreateSatelliteFromJson(string json, DateTime epoch)
    //{
    //    var definition = new
    //    {
    //        Name = "",
    //        Rotations = new[] { new { BeginTime = 0.0, EndTime = 0.0 } },
    //        Observations = new[] { new { BeginTime = 0.0, EndTime = 0.0 } },
    //        Transmissions = new[] { new { BeginTime = 0.0, EndTime = 0.0 } }
    //    };

    //    var obj = JsonConvert.DeserializeAnonymousType(json, definition);

    //    var sat = new Satellite()
    //    {
    //        Name = obj.Name,
    //        Rotations = obj.Rotations.Select(s => new Rotation() { BeginTime = epoch.AddSeconds(s.BeginTime), EndTime = epoch.AddSeconds(s.EndTime) }).ToList(),
    //        Observations = obj.Observations.Select(s => new Observation() { BeginTime = epoch.AddSeconds(s.BeginTime), EndTime = epoch.AddSeconds(s.EndTime) }).ToList(),
    //        Transmissions = obj.Transmissions.Select(s => new Transmission() { BeginTime = epoch.AddSeconds(s.BeginTime), EndTime = epoch.AddSeconds(s.EndTime) }).ToList()
    //    };

    //    InvalidateIntervals(sat);

    //    return sat;
    //}

    //private void InvalidateIntervals(Satellite sat)
    //{
    //    List<Rotation> rotations = new List<Rotation>();
    //    List<Observation> observations = new List<Observation>();
    //    List<Transmission> transmissions = new List<Transmission>();

    //    DateTime temp = DateTime.MinValue;

    //    foreach (var item in sat.Rotations.ToList().OrderBy(s => s.BeginTime))
    //    {
    //        if (temp <= item.BeginTime)
    //        {
    //            rotations.Add(item);
    //            temp = item.EndTime;
    //        }
    //    }

    //    temp = DateTime.MinValue;

    //    foreach (var item in sat.Observations.ToList().OrderBy(s => s.BeginTime))
    //    {
    //        if (temp <= item.BeginTime)
    //        {
    //            observations.Add(item);
    //            temp = item.EndTime;
    //        }
    //    }

    //    temp = DateTime.MinValue;

    //    foreach (var item in sat.Transmissions.ToList().OrderBy(s => s.BeginTime))
    //    {
    //        if (temp <= item.BeginTime)
    //        {
    //            transmissions.Add(item);
    //            temp = item.EndTime;
    //        }
    //    }

    //    var min = rotations.Min(s => ToTotalDays(s.BeginTime, Epoch));
    //    min = Math.Min(observations.Min(s => ToTotalDays(s.BeginTime, Epoch)), min);
    //    min = Math.Min(transmissions.Min(s => ToTotalDays(s.BeginTime, Epoch)), min);

    //    var max = rotations.Max(s => ToTotalDays(s.EndTime, Epoch));
    //    max = Math.Max(observations.Max(s => ToTotalDays(s.EndTime, Epoch)), max);
    //    max = Math.Max(transmissions.Max(s => ToTotalDays(s.EndTime, Epoch)), max);

    //    sat.Rotations = new List<Rotation>(rotations);
    //    sat.Observations = new List<Observation>(observations);
    //    sat.Transmissions = new List<Transmission>(transmissions);

    //    sat.BeginScenario = ToTotalDays(Epoch.Date, TimeOrigin);
    //    sat.EndScenario = sat.BeginScenario + 2;

    //    sat.Begin = ToTotalDays(Epoch, TimeOrigin);
    //    sat.Duration = 1.0;
    //}

    public static double ToTotalDays(DateTime value, DateTime timeOrigin)
    {
        return (value - timeOrigin).TotalDays + 1;
    }
}
