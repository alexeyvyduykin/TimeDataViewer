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

public class MainWindowViewModel : ViewModelBase
{
    private readonly SourceList<Interval> _intervals = new();
    private readonly ReadOnlyObservableCollection<Interval> _intervalItems;
    private readonly SourceList<Interval> _windows = new();
    private readonly ReadOnlyObservableCollection<Interval> _windowItems;
    private readonly SourceList<string> _labels = new();
    private readonly ReadOnlyObservableCollection<string> _labelItems;

    private readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

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

        Observable.Start(UpdateImpl, RxApp.MainThreadScheduler)
            .Subscribe(s =>
            {
                Epoch = Intervals.Select(s => s.Begin).Min().Date;

                BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
                EndScenario = BeginScenario + 3;

                Begin = ToTotalDays(Epoch, _timeOrigin);
                Duration = 1.0;

                SeriesBrushes = new[] { Brushes.LightCoral, Brushes.Green, Brushes.Blue, Brushes.Red, Brushes.Yellow };

                PlotModel = CreatePlotModel();
            });
    }

    private static Func<Interval, bool> CreateIntervalPredicate(IReadOnlyCollection<string> labels)
    {
        return s => labels.Contains(s.Category);
    }

    private void UpdateImpl()
    {
        var res = DataSource.Build(DateTime.Now, 86400.0);

        _intervals.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(res.Intervals);
        });

        _windows.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(res.Windows);
        });

        _labels.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(res.Intervals.Select(s => s.Category!).Distinct().ToList());
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
        plotModel.AddAxisY(TimeDataViewerLite.Factory.CreateAxisY(Labels));

        plotModel.Series.AddRange(new[]
        {
            CreateSeries(plotModel, Windows),
            CreateSeries(plotModel, Intervals),
            CreateSeries(plotModel, Windows),
            CreateSeries(plotModel, Intervals),
      //      CreateSeries(plotModel, ivals5)
        });

        ((TimelineSeries)plotModel.Series[0]).StackGroup = "Satellite_1";
        ((TimelineSeries)plotModel.Series[1]).StackGroup = "Satellite_1";
        ((TimelineSeries)plotModel.Series[2]).StackGroup = "Satellite_2";
        ((TimelineSeries)plotModel.Series[3]).StackGroup = "Satellite_2";
        //   ((TimelineSeries)plotModel.Series[4]).StackGroup = "Stack2";

        return plotModel;
    }

    private static Series CreateSeries(PlotModel parent, IEnumerable<Interval> intervals)
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
                CategoryIndex = parent.AxisY.SourceLabels.IndexOf(category)
            });
        }

        return new TimelineSeries(parent)
        {
            BarWidth = 0.5,
            Items = list,
            IsVisible = true,
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

    public ReadOnlyObservableCollection<Interval> Intervals => _intervalItems;

    public ReadOnlyObservableCollection<Interval> Windows => _windowItems;

    public ReadOnlyObservableCollection<string> Labels => _labelItems;
}
