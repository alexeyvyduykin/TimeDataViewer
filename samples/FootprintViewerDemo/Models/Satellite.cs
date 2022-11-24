using System.Collections.Generic;

namespace FootprintViewerDemo.Models;

public class Satellite
{
    public string Name { get; set; } = string.Empty;

    public List<Rotation> Rotations { get; set; } = new List<Rotation>();

    public List<Observation> Observations { get; set; } = new List<Observation>();

    public List<Transmission> Transmissions { get; set; } = new List<Transmission>();

    public double BeginScenario { get; set; }

    public double EndScenario { get; set; }

    public double Begin { get; set; }

    public double Duration { get; set; }
}
