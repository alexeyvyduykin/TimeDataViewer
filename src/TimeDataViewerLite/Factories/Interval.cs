﻿using TimeDataViewerLite.Core;

namespace TimeDataViewerLite;

public class Interval
{
    public string Category { get; set; } = null!;

    public DateTime Begin { get; set; }

    public DateTime End { get; set; }

    public BrushMode BrushMode { get; set; }
}
