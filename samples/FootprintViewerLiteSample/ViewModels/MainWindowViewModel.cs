﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using FootprintViewerLiteSample.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TimeDataViewerLite;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DataResult? _dataResult;
    private readonly SourceList<TaskModel> _tasks = new();
    private readonly ReadOnlyObservableCollection<string> _taskItems;
    private readonly SourceList<SeriesViewModel> _series = new();
    private readonly ReadOnlyObservableCollection<SeriesViewModel> _seriesItems;
    private readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    private readonly ObservableAsPropertyHelper<bool> _isLoading;

    public MainWindowViewModel()
    {
        Epoch = DateTime.Now.Date;

        BeginScenario = 0.0;
        EndScenario = 2 * 86400.0;

        var taskFilter = this.WhenAnyValue(s => s.ObservationTaskVisible, s => s.DownloadTaskVisible)
            .Select(TaskPredicate);

        var observable = _tasks
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(taskFilter)
            .Transform(s => s.Name)
            .Sort(SortExpressionComparer<string>.Ascending(s => int.Parse(s.Replace("Task_", ""))));

        observable
            .Bind(out _taskItems)
            .DisposeMany()
            .Subscribe();

        var observable2 = observable.Select(_ => this);

        _series
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _seriesItems)
            .DisposeMany()
            .Subscribe();

        var observable1 = _series
            .Connect()
            .WhenPropertyChanged(s => s.IsVisible)
            .Select(_ => this);

        var merged = Observable.Merge(observable1, observable2);

        Update = ReactiveCommand.CreateFromTask(UpdateImpl);

        _isLoading = Update.IsExecuting
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsLoading);

        merged
            .ObserveOn(RxApp.MainThreadScheduler)
            .Throttle(TimeSpan.FromSeconds(1.0))
            .Select(_ => Unit.Default)
            .InvokeCommand(Update);

        PanUp = ReactiveCommand.Create(() => PlotModel?.PanUp());
        PanDown = ReactiveCommand.Create(() => PlotModel?.PanDown());
        ZoomToCount = ReactiveCommand.Create<double>(s => PlotModel?.ZoomToCount((int)s));

        Observable.StartAsync(UpdateAsyncImpl, RxApp.MainThreadScheduler);
    }

    private async Task UpdateImpl()
    {
        var series = Series.Where(s => s.IsVisible == true);
        var tasks = Tasks.ToList();

        Epoch = _dataResult!.Epoch;

        BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
        EndScenario = BeginScenario + 3;

        Begin = ToTotalDays(Epoch, _timeOrigin);
        Duration = 1.0;

        var seriesInfos = _dataResult.Series
            .Where(s => series.Select(s => s.Name).Contains(s.StackGroup))
            .ToList();

        PlotModel = await Observable.Start(() => PlotModelBuilder.Build(Epoch, BeginScenario, EndScenario, tasks, seriesInfos),
            RxApp.TaskpoolScheduler);
    }

    public ReactiveCommand<Unit, Unit> Update { get; }

    public bool IsLoading => _isLoading.Value;

    private static Func<TaskModel, bool> TaskPredicate((bool observationTaskVisible, bool downloadTaskVisible) args)
    {
        return s =>
        (s.Type == TaskType.Observation && args.observationTaskVisible == true)
        || (s.Type == TaskType.Download && args.downloadTaskVisible == true);
    }

    private async Task UpdateAsyncImpl()
    {
        _dataResult = await DataSource.BuildSampleAsync();

        var series = _dataResult.Series
            .Select(s => s.StackGroup)
            .Distinct()
            .Select((s, index) => new SeriesViewModel()
            {
                Name = s,
                IsVisible = (index == 0)
            })
            .ToList();

        var tasks = _dataResult.Tasks;

        _tasks.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(tasks);
        });

        _series.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(series);
        });
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

    public ReactiveCommand<Unit, Unit> PanUp { get; }

    public ReactiveCommand<Unit, Unit> PanDown { get; }

    public ReactiveCommand<double, Unit> ZoomToCount { get; }

    public ReadOnlyObservableCollection<string> Tasks => _taskItems;

    public ReadOnlyObservableCollection<SeriesViewModel> Series => _seriesItems;

    [Reactive]
    public bool ObservationTaskVisible { get; set; } = true;

    [Reactive]
    public bool DownloadTaskVisible { get; set; } = true;
}
