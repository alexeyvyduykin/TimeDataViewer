using System;
using System.Collections.Generic;
using TimeDataViewerLite;

namespace FootprintViewerLiteSample.Models;

public class DataResult
{
    public DateTime Epoch { get; init; }

    public double BeginScenario { get; init; }

    public double EndScenario { get; init; }

    public List<TaskModel> Tasks { get; init; } = new();

    public List<SeriesInfo> Series { get; init; } = new();
}
