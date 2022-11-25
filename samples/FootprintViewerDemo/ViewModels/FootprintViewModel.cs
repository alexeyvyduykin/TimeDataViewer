using System;
using FootprintViewerDemo.Models;

namespace FootprintViewerDemo.ViewModels;

public class FootprintViewModel
{
    private readonly string? _name;
    private readonly string? _satelliteName;
    private readonly DateTime _begin;
    private readonly double _duration;

    public FootprintViewModel(Footprint footprint)
    {
        _name = footprint.Name;

        _satelliteName = footprint.SatelliteName;

        _begin = footprint.Begin;

        _duration = footprint.Duration;
    }

    public string? Name => _name;

    public string? SatelliteName => _satelliteName;

    public DateTime Begin => _begin;

    public double Duration => _duration;
}
