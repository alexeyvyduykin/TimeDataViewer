using System;
using System.Collections.Generic;
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
    private readonly ReadOnlyObservableCollection<TaskModel> _taskItems;
    private readonly SourceList<SeriesViewModel> _series = new();
    private readonly ReadOnlyObservableCollection<SeriesViewModel> _seriesItems;

    private readonly DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public MainWindowViewModel()
    {
        Epoch = DateTime.Now.Date;

        BeginScenario = 0.0;
        EndScenario = 2 * 86400.0;

        var taskFilter = this.WhenAnyValue(s => s.ObservationTaskVisible, s => s.DownloadTaskVisible)
            .Select(TaskPredicate);

        _tasks
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(taskFilter)
            .Bind(out _taskItems)
            .DisposeMany()
            .Subscribe();

        _series
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _seriesItems)
            .DisposeMany()
            .Subscribe();

        Observable.StartAsync(UpdateAsyncImpl, RxApp.MainThreadScheduler);

        _series
            .Connect()
            .WhenPropertyChanged(s => s.IsVisible)
            .Throttle(TimeSpan.FromSeconds(1.0))
            .Select(_ => Series.Where(s => s.IsVisible == true))
            .Subscribe(async s => PlotModel = await UpdatePlotModelAsycn(s));

        PanUp = ReactiveCommand.Create(() => PlotModel?.PanUp());
        PanDown = ReactiveCommand.Create(() => PlotModel?.PanDown());
        ZoomToCount = ReactiveCommand.Create<double>(s => PlotModel?.ZoomToCount((int)s));
    }

    private async Task<PlotModel> UpdatePlotModelAsycn(IEnumerable<SeriesViewModel> series)
    {
        Epoch = _dataResult!.Epoch;

        BeginScenario = ToTotalDays(Epoch.Date, _timeOrigin) - 1;
        EndScenario = BeginScenario + 3;

        Begin = ToTotalDays(Epoch, _timeOrigin);
        Duration = 1.0;

        var labels = Tasks.Select(s => s.Name).ToList();

        var seriesInfos = _dataResult.Series
            .Where(s => series.Select(s => s.Name).Contains(s.StackGroup))
            .ToList();

        return await Observable.Start(() => PlotModelBuilder.Build(Epoch, BeginScenario, EndScenario, labels, seriesInfos),
            RxApp.TaskpoolScheduler);
    }

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

    public ReadOnlyObservableCollection<TaskModel> Tasks => _taskItems;

    public ReadOnlyObservableCollection<SeriesViewModel> Series => _seriesItems;

    [Reactive]
    public bool ObservationTaskVisible { get; set; } = true;

    [Reactive]
    public bool DownloadTaskVisible { get; set; } = true;
}
