using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Media;
using DynamicData;
using FootprintViewerLiteSample.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.ViewModels;

public class IntervalResult
{
    public string Category { get; set; } = string.Empty;

    public DateTime Begin { get; set; }

    public double Duration { get; set; }
}

public class MainWindowViewModel : ViewModelBase
{
    private readonly SourceList<IntervalResult> _results = new();
    private readonly ReadOnlyObservableCollection<Interval> _items;

    private readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public MainWindowViewModel()
    {
        Epoch = DateTime.Now.Date;

        Labels = new ObservableCollection<ItemViewModel>()
        {
            new() { Label = "Label1" },
            new() { Label = "Label2" },
            new() { Label = "Label3" },
            new() { Label = "Label4" },
            new() { Label = "Label5" }
        };

        BeginScenario = 0.0;
        EndScenario = 2 * 86400.0;

        _results
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Transform(s => CreateInterval(s, Epoch))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe();

        Observable.Start(UpdateImpl, RxApp.MainThreadScheduler)
            .Subscribe(s =>
            {
                Begin = ToTotalDays(Epoch, _timeOrigin);
                Duration = 1.0;

                SeriesBrushes = new[] { Brushes.LightCoral, Brushes.Green, Brushes.Blue, Brushes.Red, Brushes.Yellow };

                PlotModel = CreatePlotModel();
            });
    }

    private void UpdateImpl()
    {
        var date = DateTime.Now;
        var random = new Random();
        var dt = 0;
        var list = new List<IntervalResult>();

        for (int i = 0; i < 100; i++)
        {
            var index = random.Next(0, Labels.Count);
            list.Add(new IntervalResult()
            {
                Category = Labels[index].Label ?? string.Empty,
                Begin = date.AddSeconds(random.Next(dt, dt + 1000)),
                Duration = random.Next(30, 60)
            });
            dt += 1000;
        }

        var min = list.Select(s => s.Begin).Min();
        var max = list.Select(s => s.Begin).Max();

        Epoch = min.Date;

        BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
        EndScenario = BeginScenario + 3;

        _results.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(list);
        });
    }

    private PlotModel CreatePlotModel()
    {
        var plotModel = new PlotModel()
        {
            PlotMarginLeft = 0,
            PlotMarginTop = 30,
            PlotMarginRight = 0,
            PlotMarginBottom = 0,
        };

        plotModel.AddAxisX(TimeDataViewerLite.Factory.CreateAxisX(Epoch, BeginScenario, EndScenario));
        plotModel.AddAxisY(TimeDataViewerLite.Factory.CreateAxisY(Labels, s => ((ItemViewModel)s).Label ?? string.Empty));

        plotModel.Series.AddRange(new[]
        {
            CreateSeries(plotModel, Intervals.Where(s => Equals(s.Category, Labels[0].Label))),
            CreateSeries(plotModel, Intervals.Where(s => Equals(s.Category, Labels[1].Label))),
            CreateSeries(plotModel, Intervals.Where(s => Equals(s.Category, Labels[2].Label))),
            CreateSeries(plotModel, Intervals.Where(s => Equals(s.Category, Labels[3].Label))),
            CreateSeries(plotModel, Intervals.Where(s => Equals(s.Category, Labels[4].Label)))
        });

        return plotModel;
    }

    private static Series CreateSeries(PlotModel parent, IEnumerable<Interval> intervals)
    {
        return new TimelineSeries(parent)
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

    private static Interval CreateInterval(IntervalResult result, DateTime epoch)
    {
        var secs = result.Begin.TimeOfDay.TotalSeconds;

        var date = epoch.Date;

        return new Interval()
        {
            Category = result.Category,
            BeginTime = date.AddSeconds(secs),
            EndTime = date.AddSeconds(secs + result.Duration)
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
    public IList<IBrush>? SeriesBrushes { get; set; }

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

    public ReadOnlyObservableCollection<Interval> Intervals => _items;
}
