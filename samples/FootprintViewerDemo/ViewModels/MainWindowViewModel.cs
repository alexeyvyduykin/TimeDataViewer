using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DynamicData;
using FootprintViewerDemo.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TimeDataViewer.Core;

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
    private readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

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
            .Transform(s => CreateInterval(s, Epoch))
            .Bind(out _items1)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite2"))
            .Transform(s => CreateInterval(s, Epoch))
            .Bind(out _items2)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite3"))
            .Transform(s => CreateInterval(s, Epoch))
            .Bind(out _items3)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite4"))
            .Transform(s => CreateInterval(s, Epoch))
            .Bind(out _items4)
            .DisposeMany()
            .Subscribe();

        _footprints
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(s => Equals(s.SatelliteName, "Satellite5"))
            .Transform(s => CreateInterval(s, Epoch))
            .Bind(out _items5)
            .DisposeMany()
            .Subscribe();

        Update = ReactiveCommand.CreateFromTask(UpdateImpl, outputScheduler: RxApp.MainThreadScheduler);

        Selected = Satellites.FirstOrDefault();

        Observable.StartAsync(UpdateImpl, RxApp.MainThreadScheduler)
            .Subscribe(s =>
            {
                Begin = ToTotalDays(Epoch, _timeOrigin);
                Duration = 1.0;

                PlotModel = CreatePlotModel();
            });
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

        BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
        EndScenario = BeginScenario + 3;

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

    private PlotModel CreatePlotModel()
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0
        };

        plotModel.Axises.AddRange(new[]
        {
            CreateAxisY(Labels),
            CreateAxisX(Epoch, BeginScenario, EndScenario)
        });

        plotModel.Series.AddRange(new[]
        {
            CreateSeries(Intervals1),
            CreateSeries(Intervals2),
            CreateSeries(Intervals3),
            CreateSeries(Intervals4),
            CreateSeries(Intervals5)
        });

        return plotModel;
    }

    private static Series CreateSeries(IEnumerable<Interval> intervals)
    {
        return new TimelineSeries()
        {
            BarWidth = 0.5,
            ItemsSource = intervals,
            CategoryField = "Category",
            BeginField = "BeginTime",
            EndField = "EndTime",
            IsVisible = true,
            TrackerKey = intervals.FirstOrDefault()?.Category ?? string.Empty,
        };
    }

    private static Axis CreateAxisY(IEnumerable<ItemViewModel> labels)
    {
        var axisY = new CategoryAxis()
        {
            Position = AxisPosition.Left,
            AbsoluteMinimum = -0.5,
            AbsoluteMaximum = 4.5,
            IsZoomEnabled = false,
            LabelField = "Label",
            IsTickCentered = false,
            GapWidth = 1.0,
            ItemsSource = labels
        };

        axisY.Labels.Clear();
        axisY.Labels.AddRange(labels.Select(s => s.Label)!);

        return axisY;
    }

    private static Axis CreateAxisX(DateTime epoch, double begin, double end)
    {
        return new DateTimeAxis()
        {
            Position = AxisPosition.Top,
            IntervalType = DateTimeIntervalType.Auto,
            AbsoluteMinimum = begin,
            AbsoluteMaximum = end,
            CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek,
            FirstDayOfWeek = DayOfWeek.Monday,
            MinorIntervalType = DateTimeIntervalType.Auto,
            Minimum = DateTimeAxis.ToDouble(epoch),
            AxisDistance = 0.0,
            AxisTickToLabelDistance = 4.0,
            ExtraGridlines = null,
            IntervalLength = 60.0,
            IsPanEnabled = true,
            IsAxisVisible = true,
            IsZoomEnabled = true,
            Key = null,
            MajorStep = double.NaN,
            MajorTickSize = 7.0,
            MinorStep = double.NaN,
            MinorTickSize = 4.0,
            Maximum = double.NaN,
            MinimumRange = 0.0,
            MaximumRange = double.PositiveInfinity,
            StringFormat = null
        };
    }

    private static Interval CreateInterval(Footprint footprint, DateTime epoch)
    {
        var secs = footprint.Begin.TimeOfDay.TotalSeconds;

        var date = epoch.Date;

        return new Interval()
        {
            Category = footprint.SatelliteName,
            BeginTime = date.AddSeconds(secs),
            EndTime = date.AddSeconds(secs + footprint.Duration)
        };
    }

    private static double ToTotalDays(DateTime value, DateTime timeOrigin)
    {
        return (value - timeOrigin).TotalDays + 1;
    }

    [Reactive]
    public PlotModel? PlotModel { get; set; }

    [Reactive]
    public ObservableCollection<ItemViewModel> Labels { get; set; }

    [Reactive]
    public DateTime Epoch { get; set; }

    [Reactive]
    public double BeginScenario { get; set; }

    [Reactive]
    public double EndScenario { get; set; }

    [Reactive]
    public double Begin { get; set; }

    [Reactive]
    public double Duration { get; set; }

    [Reactive]
    public string? Selected { get; set; }

    public ReadOnlyObservableCollection<string> Satellites => _items;

    public ReadOnlyObservableCollection<FootprintViewModel> Footprints => _footprintItems;

    public ReadOnlyObservableCollection<Interval> Intervals1 => _items1;

    public ReadOnlyObservableCollection<Interval> Intervals2 => _items2;

    public ReadOnlyObservableCollection<Interval> Intervals3 => _items3;

    public ReadOnlyObservableCollection<Interval> Intervals4 => _items4;

    public ReadOnlyObservableCollection<Interval> Intervals5 => _items5;
}
