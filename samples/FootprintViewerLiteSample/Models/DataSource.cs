using System;
using System.Collections.Generic;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.Models;

public static class DataSource
{
    public static Random _random = new();

    public static DataResult Build(DateTime begin, double duration, int taskCount = 100, int nMax = 3)
    {
        var dt = duration / nMax;

        var intervals = new List<Interval>();
        var windows = new List<Interval>();

        for (int j = 0; j < taskCount; j++)
        {
            var task = $"Task_{j + 1}";

            for (int i = 0; i < nMax; i++)
            {
                var b = i * dt;
                var e = (i + 1) * dt;

                var (minWindow, maxWindow) = BuildRange(b, e, 0.7, 0.9);

                var (minInterval, maxInterval) = BuildRange(minWindow, maxWindow, 0.3, 0.5);

                intervals.Add(new Interval()
                {
                    Category = task,
                    Begin = begin.AddSeconds(minInterval),
                    End = begin.AddSeconds(maxInterval)
                });

                windows.Add(new Interval()
                {
                    Category = task,
                    Begin = begin.AddSeconds(minWindow),
                    End = begin.AddSeconds(maxWindow)
                });
            }
        }

        return new DataResult()
        {
            Intervals = intervals,
            Windows = windows
        };
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
