using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.Models;

public static class DataSource
{
    public static Random _random = new();

    public static async Task<DataResult> BuildSampleAsync() => await Observable.Start(() => BuildSample(), RxApp.TaskpoolScheduler);

    public static DataResult BuildSample()
    {
        var begin = DateTime.Now;

        var tasks = BuildTasks(2500);

        var (windows1, intervals1) = Build(begin, 86400.0, tasks, 3);
        var (windows2, intervals2) = Build(begin, 86400.0, tasks, 3);
        var (windows3, intervals3) = Build(begin, 86400.0, tasks, 3);
        var (windows4, intervals4) = Build(begin, 86400.0, tasks, 3);
        var (windows5, intervals5) = Build(begin, 86400.0, tasks, 3);

        return new DataResult()
        {
            Tasks = tasks,
            Series = new List<SeriesResult>()
            {
                new SeriesResult("Satellite_1", windows1, intervals1),
                new SeriesResult("Satellite_2", windows2, intervals2),
                new SeriesResult("Satellite_3", windows3, intervals3),
                new SeriesResult("Satellite_4", windows4, intervals4),
                new SeriesResult("Satellite_5", windows5, intervals5)
            }
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
            for (int i = 0; i < nMax; i++)
            {
                var b = i * dt;
                var e = (i + 1) * dt;

                var (minWindow, maxWindow) = BuildRange(b, e, 0.7, 0.9);

                var (minInterval, maxInterval) = BuildRange(minWindow, maxWindow, 0.3, 0.5);

                intervals.Add(new Interval()
                {
                    Category = task.Name,
                    Begin = begin.AddSeconds(minInterval),
                    End = begin.AddSeconds(maxInterval)
                });

                windows.Add(new Interval()
                {
                    Category = task.Name,
                    Begin = begin.AddSeconds(minWindow),
                    End = begin.AddSeconds(maxWindow)
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
