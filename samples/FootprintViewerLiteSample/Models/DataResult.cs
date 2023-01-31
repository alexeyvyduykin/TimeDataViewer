using System.Collections.Generic;
using TimeDataViewerLite.Core;

namespace FootprintViewerLiteSample.Models;

public class DataResult
{
    public List<Interval> Intervals { get; set; } = new();

    public List<Interval> Windows { get; set; } = new();
}
