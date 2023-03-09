using TimeDataViewerLite.Core.Style;

namespace TimeDataViewerLite;

public class SeriesInfo
{
    public string Name { get; set; } = string.Empty;

    public Brush Brush { get; set; } = new Brush(Colors.Red);

    public List<IntervalInfo> Items { get; set; } = new();

    public string StackGroup { get; set; } = string.Empty;
}
