using System;

namespace FootprintViewerDemo.Models;

public class Rotation
{
    public string Category => "Rotation";

    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }
}
