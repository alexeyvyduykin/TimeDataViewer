using System;

namespace FootprintViewerDemo.Models;

public class Transmission
{
    public string Category => "Transmission";

    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }
}
