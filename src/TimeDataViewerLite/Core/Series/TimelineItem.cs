namespace TimeDataViewerLite.Core;

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

    public int CategoryIndex { get; set; } = -1;

    public double Begin { get; set; }

    public double End { get; set; }
}
