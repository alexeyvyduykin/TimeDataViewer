using System.Collections.Generic;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.Models;

public class SeriesResult
{
    public SeriesResult(string name, IList<Interval> windows, IList<Interval> intervals)
    {
        Name = name;
        Windows = new(windows);
        Intervals = new(intervals);
    }

    public string Name { get; set; } = string.Empty;

    public List<Interval> Intervals { get; set; } = new();

    public List<Interval> Windows { get; set; } = new();
}
