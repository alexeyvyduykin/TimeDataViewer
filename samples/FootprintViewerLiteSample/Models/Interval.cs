using System;

namespace FootprintViewerLiteSample.Models;

public class Interval
{
    public string? Category { get; set; }

    public DateTime Begin { get; set; }

    public DateTime End { get; set; }

    public int Type { get; set; }
}
