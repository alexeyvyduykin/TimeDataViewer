using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public sealed class TimelineSeries : Series, IStackableSeries
{
    public const string DefaultTrackerFormatString = "{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}";
    private OxyRect _clippingRect;
    private readonly List<(OxyRect, bool)> _rectangles = new();
    private int _selectedIndex = -1;

    // Gets or sets the maximum x-coordinate of the dataset.
    public double MaxX { get; private set; }

    // Gets or sets the maximum y-coordinate of the dataset.
    public double MaxY { get; private set; }

    // Gets or sets the minimum x-coordinate of the dataset.
    public double MinX { get; private set; }

    // Gets or sets the minimum y-coordinate of the dataset.
    public double MinY { get; private set; }

    public TimelineSeries(PlotModel plotModel) : base(plotModel)
    {
        Items = new List<TimelineItem>();
        TrackerFormatString = DefaultTrackerFormatString;
        BarWidth = 1;
    }

    // Transforms the specified coordinates to a screen point by the axes of this series.
    public ScreenPoint Transform(double x, double y)
    {
        return PlotModel.AxisX.Transform(x, y, PlotModel.AxisY);
    }

    public OxyRect GetClippingRect()
    {
        var axisX = PlotModel.AxisX;
        var axisY = PlotModel.AxisY;

        var minX = Math.Min(axisX.ScreenMin.X, axisX.ScreenMax.X);
        var minY = Math.Min(axisY.ScreenMin.Y, axisY.ScreenMax.Y);
        var maxX = Math.Max(axisX.ScreenMin.X, axisX.ScreenMax.X);
        var maxY = Math.Max(axisY.ScreenMin.Y, axisY.ScreenMax.Y);

        return new OxyRect(minX, minY, maxX - minX, maxY - minY);
    }

    public double BarWidth { get; set; }

    public bool IsStacked => true;

    public string StackGroup { get; set; } = string.Empty;

    public IList<TimelineItem> Items { get; set; }

    private IList<OxyRect>? ActualBarRectangles { get; set; }

    // Gets the point in the dataset that is nearest the specified point.
    public override TrackerHitResult? GetNearestPoint(ScreenPoint point, bool interpolate)
    {
        if (ActualBarRectangles == null)
        {
            return null;
        }

        for (int i = 0; i < ActualBarRectangles.Count; i++)
        {
            var r = ActualBarRectangles[i];
            if (r.Contains(point))
            {
                var item = Items[i];
                var categoryIndex = item.GetCategoryIndex(i);
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

    public override void Render()
    {
        ActualBarRectangles = new List<OxyRect>();

        if (Items == null || Items.Count == 0)
        {
            return;
        }

        var clippingRect = GetClippingRect();

        var actualBarWidth = GetActualBarWidth();
        var stackIndex = PlotModel.AxisY.GetStackIndex(StackGroup);

        _rectangles.Clear();
        _clippingRect = clippingRect;

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];

            var categoryIndex = item.GetCategoryIndex(i);

            double categoryValue = PlotModel.AxisY.GetCategoryValue(categoryIndex, stackIndex, actualBarWidth);

            var p0 = Transform(item.Begin, categoryValue);
            var p1 = Transform(item.End, categoryValue + actualBarWidth);

            var rectangle = OxyRect.Create(p0.X, p0.Y, p1.X, p1.Y);

            ActualBarRectangles.Add(rectangle);

            _rectangles.Add((rectangle, i == _selectedIndex));

            //rc.DrawClippedRectangleAsPolygon(
            //    clippingRect,
            //    rectangle,
            //    item.Color.GetActualColor(ActualFillColor),
            //    StrokeColor);
        }
    }

    public OxyRect MyClippingRect => _clippingRect;

    public List<(OxyRect, bool)> Rectangles => _rectangles;

    public void SelectIndex(int index) => _selectedIndex = index;

    public void ResetSelectIndex() => _selectedIndex = -1;

    // Gets or sets the width/height of the columns/bars (as a fraction of the available space).
    internal double GetBarWidth() => BarWidth;

    // Updates the axis maximum and minimum values.     
    protected internal override void UpdateAxisMaxMin()
    {
        //XAxis.Include(MinX);
        //XAxis.Include(MaxX);
        //YAxis.Include(MinY);
        //YAxis.Include(MaxY);

        PlotModel.AxisX.Include(MinX);
        PlotModel.AxisX.Include(MaxX);
    }

    // Updates the maximum and minimum values of the series.  
    protected internal override void UpdateMaxMin()
    {
        MinX = MinY = MaxX = MaxY = double.NaN;

        if (Items == null || Items.Count == 0)
        {
            return;
        }

        double minValue = double.MaxValue;
        double maxValue = double.MinValue;

        foreach (var item in Items)
        {
            minValue = Math.Min(minValue, item.Begin);
            minValue = Math.Min(minValue, item.End);
            maxValue = Math.Max(maxValue, item.Begin);
            maxValue = Math.Max(maxValue, item.End);
        }

        MinX = minValue;
        MaxX = maxValue;
    }

    // Gets the actual width/height of the items of this series.
    public double GetActualBarWidth() => BarWidth / (1 + PlotModel.AxisY.GapWidth) / PlotModel.AxisY.MaxWidth;
}
