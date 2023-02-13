using TimeDataViewerLite.Core;
using TimeDataViewerLite.Core.Style;

namespace TimeDataViewerLite;

public class SeriesInfo
{
    public string Name { get; set; } = string.Empty;

    public Brush Brush { get; set; } = new Brush(Colors.Red);

    public Func<List<string>, List<TimelineItem>> Converter { get; set; } = s => new List<TimelineItem>();

    public string StackGroup { get; set; } = string.Empty;
}
