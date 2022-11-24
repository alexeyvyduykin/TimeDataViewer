using System;

namespace FootprintViewerDemo.Models;

public class Observation
{
    public string Category => "Observation";

    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }
}
