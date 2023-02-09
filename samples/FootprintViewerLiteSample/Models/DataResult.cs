using System;
using System.Collections.Generic;
using System.Linq;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.Models;

public class DataResult
{
    private DateTime? _epoch;
    private List<string>? _categories;

    public List<TaskModel> Tasks { get; init; } = new();

    public List<SeriesResult> Series { get; init; } = new();

    public DateTime Epoch => _epoch ??= Series
        .SelectMany(s => s.Intervals)
        .Select(s => s.Begin)
        .Min()
        .Date;

    public List<string> Categories => _categories ??= Series
        .SelectMany(s => s.Intervals)
        .Select(s => s.Category!)
        .Distinct()
        .ToList();

    public List<Interval> GetIntervals(string seriesName)
    {
        return Series
            .Where(s => Equals(s.Name, seriesName))
            .Select(s => s.Intervals)
            .First();
    }

    public List<Interval> GetWindows(string seriesName)
    {
        return Series
            .Where(s => Equals(s.Name, seriesName))
            .Select(s => s.Windows)
            .First();
    }
}
