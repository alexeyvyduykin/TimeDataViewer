using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public sealed class TimelineSeries : Series, IStackableSeries
{
    public const string DefaultTrackerFormatString = "{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}";
    private OxyRect _clippingRect;
    private List<(OxyRect, bool)>? _rectangles;
    private int? _selectedIndex;

    public TimelineSeries(PlotModel plotModel) : base(plotModel)
    {
        Items = new List<TimelineItem>();
        TrackerFormatString = DefaultTrackerFormatString;
        BarWidth = 1;
    }

    public List<(OxyRect, bool)>? Rectangles => _rectangles;

    public double BarWidth { get; set; }

    public bool IsStacked => true;

    public string StackGroup { get; set; } = string.Empty;

    public OxyRect ClippingRect => _clippingRect;

    public IList<TimelineItem> Items { get; set; }

    // Transforms the specified coordinates to a screen point by the axes of this series.
    public ScreenPoint Transform(double x, double y)
    {
        return PlotModel.AxisX.Transform(x, y, PlotModel.AxisY);
    }

    private static OxyRect GetClippingRect(PlotModel plot)
    {
        var axisX = plot.AxisX;
        var axisY = plot.AxisY;

        var minX = Math.Min(axisX.ScreenMin.X, axisX.ScreenMax.X);
        var minY = Math.Min(axisY.ScreenMin.Y, axisY.ScreenMax.Y);
        var maxX = Math.Max(axisX.ScreenMin.X, axisX.ScreenMax.X);
        var maxY = Math.Max(axisY.ScreenMin.Y, axisY.ScreenMax.Y);

        return new OxyRect(minX, minY, maxX - minX, maxY - minY);
    }

    // Gets the point in the dataset that is nearest the specified point.
    public override TrackerHitResult? GetNearestPoint(ScreenPoint point, bool interpolate)
    {
        if (Rectangles == null)
        {
            return null;
        }

        foreach (var (r, i) in Rectangles.Select((s, index) => (s.Item1, index)))
        {
            if (r.Contains(point) == true)
            {
                var item = Items[i];
                var categoryIndex = item.CategoryIndex;
                double value = (item.Begin + item.End) / 2;
                var dp = new DataPoint(categoryIndex, value);

                return new TrackerHitResult
                {
                    Series = this,
                    DataPoint = dp,
                    Position = point,
                    Item = item,
                    Index = i,
                    Text = StringHelper.Format(
                        System.Globalization.CultureInfo.CurrentCulture,
                        TrackerFormatString,
                        item,
                        "Category",                          // {0}
                        item.Category!,                      // {1}
                        "Group",
                        StackGroup,
                        "Begin",                             // {4}                  
                        DateTimeAxis.ToDateTime(item.Begin), // {5}
                        "End",                               // {6}                                
                        DateTimeAxis.ToDateTime(item.End))   // {7}                        
                };
            }
        }

        return null;
    }

    public override void UpdateRenderInfo()
    {
        if (IsVisible == false || Items == null || Items.Count == 0)
        {
            return;
        }

        _clippingRect = GetClippingRect(PlotModel);

        _rectangles = CreateRectangles();
    }

    private List<(OxyRect, bool)> CreateRectangles()
    {
        var list = new List<(OxyRect, bool)>();

        var axisY = PlotModel.AxisY;

        var actualBarWidth = BarWidth / (1 + axisY.GapWidth) / axisY.MaxWidth;
        var stackIndex = axisY.GetStackIndex(StackGroup);

        foreach (var (item, i) in Items.Select((s, index) => (s, index)))
        {
            var categoryIndex = item.CategoryIndex;

            double categoryValue = axisY.GetCategoryValue(categoryIndex, stackIndex, actualBarWidth);

            var p0 = Transform(item.Begin, categoryValue);
            var p1 = Transform(item.End, categoryValue + actualBarWidth);

            var rectangle = OxyRect.Create(p0.X, p0.Y, p1.X, p1.Y);

            list.Add((rectangle, Equals(i, _selectedIndex)));
        }

        return list;
    }

    public void SelectIndex(int index) => _selectedIndex = index;

    public void ResetSelectIndex() => _selectedIndex = null;

    // Updates the axis maximum and minimum values.     
    protected internal override void UpdateAxisMaxMin()
    {
        //XAxis.Include(MinX);
        //XAxis.Include(MaxX);
        //YAxis.Include(MinY);
        //YAxis.Include(MaxY);

        var minX = Items.Select(s => Math.Min(s.Begin, s.End)).Min();
        var maxX = Items.Select(s => Math.Max(s.Begin, s.End)).Max();

        PlotModel.AxisX.Include(minX);
        PlotModel.AxisX.Include(maxX);
    }
}
