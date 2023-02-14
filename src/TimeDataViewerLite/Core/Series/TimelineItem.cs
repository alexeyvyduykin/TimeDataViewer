namespace TimeDataViewerLite.Core;

public enum BrushMode
{
    Solid,
    UpLine,
    DownLine
}

public class TimelineItem
{
    public int GetCategoryIndex(int defaultIndex)
    {
        if (CategoryIndex < 0)
        {
            return defaultIndex;
        }

        return CategoryIndex;
    }

    public string? Category { get; set; }

    public int CategoryIndex { get; set; } = -1;

    public double Begin { get; set; }

    public double End { get; set; }

    public BrushMode BrushMode { get; set; } = BrushMode.Solid;
}
