using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using TimeDataViewerLite;
using TimeDataViewerLite.Core;
using TimeDataViewerLite.Core.Style;

namespace FootprintViewerLiteSample.Models;

public static class DataSource
{
    private static DateTime _timeOrigin = new(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    private static Random _random = new();

    public static async Task<DataResult> BuildSampleAsync() => await Observable.Start(() => BuildSample(), RxApp.TaskpoolScheduler);

    public static DataResult BuildSample()
    {
        var begin = DateTime.Now;
        var begin0 = begin.Date;

        var beginScenario = (begin0 - _timeOrigin).TotalDays + 1 - 1;
        var endScenario = beginScenario + 3;

        var tasks = BuildTasks(2500);

        var seriesInfos = new List<SeriesInfo>();

        for (int i = 0; i < 5; i++)
        {
            var (windows, intervals) = Build(begin, 86400.0, tasks, 3);

            var arr = new[]
            {
                new SeriesInfo()
                {
                    Name = $"Satellite_{i + 1}_winds",
                    Items = windows.ToList(),
                    Brush = new Brush(Colors.Palette[i], 0.25),
                    StackGroup = $"Satellite_{i + 1}"
                },
                new SeriesInfo()
                {
                    Name = $"Satellite_{i + 1}_ivals",
                    Items = intervals.ToList(),
                    Brush = new Brush(Colors.Palette[i]),
                    StackGroup = $"Satellite_{i + 1}"
                }
            };

            seriesInfos.AddRange(arr);
        }

        return new DataResult()
        {
            Epoch = begin0,
            BeginScenario = beginScenario,
            EndScenario = endScenario,
            Tasks = tasks,
            Series = seriesInfos
        };
    }

    private static List<TaskModel> BuildTasks(int taskCount)
    {
        var list = new List<TaskModel>();

        for (int i = 0; i < taskCount; i++)
        {
            list.Add(new TaskModel()
            {
                Name = $"Task_{i + 1}",
                Type = _random.Next(0, 5) == 0 ? TaskType.Download : TaskType.Observation
            });
        }

        return list;
    }

    private static (IList<Interval> Windows, IList<Interval> Intervals) Build(DateTime begin, double duration, IList<TaskModel> tasks, int nMax = 3)
    {
        var dt = duration / nMax;

        var intervals = new List<Interval>();
        var windows = new List<Interval>();

        foreach (var task in tasks)
        {
            var taskType = task.Type;
            int categoryType = 0;

            for (int i = 0; i < nMax; i++)
            {
                if (taskType == TaskType.Download)
                {
                    categoryType = _random.Next(1, 3);
                }

                var b = i * dt;
                var e = (i + 1) * dt;

                var (minWindow, maxWindow) = BuildRange(b, e, 0.7, 0.9);

                var (minInterval, maxInterval) = BuildRange(minWindow, maxWindow, 0.3, 0.5);

                intervals.Add(new Interval()
                {
                    Category = task.Name,
                    Begin = begin.AddSeconds(minInterval),
                    End = begin.AddSeconds(maxInterval),
                    BrushMode = (BrushMode)categoryType
                });

                windows.Add(new Interval()
                {
                    Category = task.Name,
                    Begin = begin.AddSeconds(minWindow),
                    End = begin.AddSeconds(maxWindow),
                    BrushMode = BrushMode.Solid
                });
            }
        }

        return (windows, intervals);
    }

    private static (double min, double max) BuildRange(double begin, double end, double minCoef, double maxCoef)
    {
        var dt = end - begin;

        var windowDuration = _random.Next((int)(dt * minCoef), (int)(dt * maxCoef));

        var windowDurationHalf = windowDuration / 2.0;

        var center = _random.Next((int)(begin + windowDurationHalf), (int)(end - windowDurationHalf));

        return (center - windowDurationHalf, center + windowDurationHalf);
    }
}
