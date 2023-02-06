using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using FootprintViewerLiteSample.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Core.Style;

namespace FootprintViewerLiteSample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SourceList<Interval> _intervals = new();
    private readonly ReadOnlyObservableCollection<Interval> _intervalItems;
    private readonly SourceList<Interval> _windows = new();
    private readonly ReadOnlyObservableCollection<Interval> _windowItems;
    private readonly SourceList<string> _labels = new();
    private readonly ReadOnlyObservableCollection<ItemViewModel> _labelItems;
    private readonly SourceList<SeriesViewModel> _series = new();
    private readonly ReadOnlyObservableCollection<SeriesViewModel> _seriesItems;

    private readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    private readonly Dictionary<string, Color> _colors =
        Colors.Palette
        .Select((s, i) => (color: s, index: i + 1))
        .Take(5)
        .ToDictionary(s => $"Satellite_{s.index}", s => s.color);

    private Dictionary<string, (List<Interval>, List<Interval>)> _dict = new();

    public MainWindowViewModel()
    {
        Epoch = DateTime.Now.Date;

        BeginScenario = 0.0;
        EndScenario = 2 * 86400.0;

        var observable = _labels
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Top(7);

        observable
            .Transform(s => new ItemViewModel() { Label = s })
            .Bind(out _labelItems)
            .DisposeMany()
            .Subscribe();

        var filter = observable
            .ToCollection()
            .Select(CreateIntervalPredicate);

        _intervals
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(filter)
            .Bind(out _intervalItems)
            .DisposeMany()
            .Subscribe();

        _windows
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(filter)
            .Bind(out _windowItems)
            .DisposeMany()
            .Subscribe();

        _series
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _seriesItems)
            .DisposeMany()
            .Subscribe();

        Observable.Start(UpdateImpl, RxApp.MainThreadScheduler)
            .Subscribe(s =>
            {
                Epoch = Intervals.Select(s => s.Begin).Min().Date;

                BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
                EndScenario = BeginScenario + 3;

                Begin = ToTotalDays(Epoch, _timeOrigin);
                Duration = 1.0;

                var labels = Labels.Select(s => s.Label!).ToList();

                var intervals = Series.Select(s => s.Name!).ToDictionary(s => s, s => _dict[s].Item1);
                var windows = Series.Select(s => s.Name!).ToDictionary(s => s, s => _dict[s].Item2);

                PlotModel = CreatePlotModel(Epoch, BeginScenario, EndScenario, labels, _colors, windows, intervals);
            });

        _series
            .Connect()
            .WhenPropertyChanged(s => s.IsVisible)
            .Select(_ => Series.Where(s => s.IsVisible == true))
            .Subscribe(s =>
            {
                var series = s.Select(s => s.Name!).ToList();

                Epoch = Intervals.Select(s => s.Begin).Min().Date;

                BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
                EndScenario = BeginScenario + 3;

                Begin = ToTotalDays(Epoch, _timeOrigin);
                Duration = 1.0;

                var labels = Labels.Select(s => s.Label!).ToList();

                var intervals = series.ToDictionary(s => s, s => _dict[s].Item1);
                var windows = series.ToDictionary(s => s, s => _dict[s].Item2);

                PlotModel = CreatePlotModel(Epoch, BeginScenario, EndScenario, labels, _colors, windows, intervals);
            });
    }

    private static Func<Interval, bool> CreateIntervalPredicate(IReadOnlyCollection<string> labels)
    {
        return s => labels.Contains(s.Category);
    }

    private void UpdateImpl()
    {
        var res1 = DataSource.Build(DateTime.Now, 86400.0, 100, 3);
        var res2 = DataSource.Build(DateTime.Now, 86400.0, 100, 3);
        var res3 = DataSource.Build(DateTime.Now, 86400.0, 100, 3);
        var res4 = DataSource.Build(DateTime.Now, 86400.0, 100, 3);
        var res5 = DataSource.Build(DateTime.Now, 86400.0, 100, 3);

        var series = new[]
        {
            new SeriesViewModel(){ Name = "Satellite_1", IsVisible = true },
            new SeriesViewModel(){ Name = "Satellite_2", IsVisible = true },
            new SeriesViewModel(){ Name = "Satellite_3", IsVisible = true },
            new SeriesViewModel(){ Name = "Satellite_4", IsVisible = true },
            new SeriesViewModel(){ Name = "Satellite_5", IsVisible = true },
        };

        _dict = new Dictionary<string, (List<Interval> Intervals, List<Interval> Windows)>()
        {
            { "Satellite_1", (res1.Intervals, res1.Windows) },
            { "Satellite_2", (res2.Intervals, res2.Windows) },
            { "Satellite_3", (res3.Intervals, res3.Windows) },
            { "Satellite_4", (res4.Intervals, res4.Windows) },
            { "Satellite_5", (res5.Intervals, res5.Windows) }
        };

        _intervals.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(res1.Intervals);
        });

        _windows.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(res1.Windows);
        });

        _labels.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(res1.Intervals.Select(s => s.Category!).Distinct().ToList());
        });

        _series.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(series);
        });
    }

    private static PlotModel CreatePlotModel(DateTime epoch, double begin, double end,
        IList<string> labels,
        IDictionary<string, Color> colors,
        Dictionary<string, List<Interval>> windows,
        Dictionary<string, List<Interval>> intervals)
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0,
        };

        var series = new List<Series>();

        foreach (var key in windows.Keys)
        {
            var list1 = new List<Interval>();
            var list2 = new List<Interval>();

            foreach (var label in labels)
            {
                list1.AddRange(windows[key].Where(s => Equals(s.Category, label)));
                list2.AddRange(intervals[key].Where(s => Equals(s.Category, label)));
            }

            series.Add(CreateSeries(plotModel, new Brush(colors[key], 0.35), list1, labels, key));
            series.Add(CreateSeries(plotModel, new Brush(colors[key]), list2, labels, key));
        }

        plotModel.AddAxisX(TimeDataViewerLite.Factory.CreateAxisX(epoch, begin, end));
        plotModel.AddAxisY(TimeDataViewerLite.Factory.CreateAxisY(labels));
        plotModel.AddSeries(series);

        return plotModel;
    }

    private static Series CreateSeries(PlotModel parent, Brush brush, IList<Interval> intervals, IList<string> labels, string stackGroup = "")
    {
        var list = new List<TimelineItem>();

        foreach (var item in intervals)
        {
            var category = item.Category ?? throw new Exception();

            list.Add(new TimelineItem()
            {
                Begin = DateTimeAxis.ToDouble(item.Begin),
                End = DateTimeAxis.ToDouble(item.End),
                Category = item.Category,
                CategoryIndex = labels.IndexOf(category)
            });
        }

        return new TimelineSeries(parent)
        {
            BarWidth = 0.5,
            Brush = brush,
            Items = list,
            IsVisible = true,
            StackGroup = stackGroup,
            TrackerKey = intervals.FirstOrDefault()?.Category ?? string.Empty,
        };
    }

    private static double ToTotalDays(DateTime value, DateTime timeOrigin)
    {
        return (value - timeOrigin).TotalDays + 1;
    }

    [Reactive]
    public PlotModel? PlotModel { get; set; }

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

    public ReadOnlyObservableCollection<Interval> Intervals => _intervalItems;

    public ReadOnlyObservableCollection<Interval> Windows => _windowItems;

    public ReadOnlyObservableCollection<ItemViewModel> Labels => _labelItems;

    public ReadOnlyObservableCollection<SeriesViewModel> Series => _seriesItems;
}
