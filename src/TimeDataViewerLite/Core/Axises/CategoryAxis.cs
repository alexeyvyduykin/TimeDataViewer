using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

/// <remarks>The category axis is using the index of the label collection items as coordinates.
/// If you have 5 categories in the Labels collection, the categories will be placed at coordinates 0 to 4.
/// The range of the axis will be from -0.5 to 4.5 (excluding padding).</remarks>
public sealed class CategoryAxis : Axis
{
    // The maximal width of all labels.
    private double _maxWidth;

    // Gets or sets the original offset of the bars (not used for stacked bar series).
    private double[]? _barOffset;

    // Gets or sets the stack index mapping. The mapping indicates to which rank a specific stack index belongs.
    private Dictionary<string, int> _stackIndexMapping = new();

    // Gets or sets the offset of the bars per StackIndex and Label (only used for stacked bar series).
    private double[,]? _stackedBarOffset;

    // Gets or sets sum of the widths of the single bars per label. This is used to find the bar width of BarSeries
    private double[]? _totalWidthPerCategory;

    public CategoryAxis()
    {
        Position = AxisPosition.Bottom;
        MajorStep = 1;
        GapWidth = 1;
    }

    public override void UpdateRenderInfo(PlotModel plot) => throw new Exception("Render info for CategoryAxis not enabled.");

    public override string ToLabel(double x)
    {
        var index = (int)x;

        if (index >= 0 && index < SourceLabels.Count)
        {
            return SourceLabels[index];
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets or sets the gap width.
    /// </summary>
    /// <remarks>The default value is 1.0 (100%). The gap width is given as a fraction of the total width/height of the items in a category.</remarks>
    public double GapWidth { get; set; }

    // Gets or sets a value indicating whether the ticks are centered. If this is <c>false</c>, ticks will be drawn between each category. If this is <c>true</c>, ticks will be drawn in the middle of each category.
    public bool IsTickCentered { get; set; }

    public List<string> SourceLabels { get; set; } = new();

    // Gets the maximum width of all category labels.
    public double MaxWidth => _maxWidth;

    public double GetCategoryValue(int categoryIndex, int stackIndex, double actualBarWidth)
    {
        if (_stackedBarOffset == null)
        {
            throw new Exception();
        }

        var offsetBegin = _stackedBarOffset[stackIndex, categoryIndex];
        var offsetEnd = _stackedBarOffset[stackIndex + 1, categoryIndex];
        return categoryIndex - 0.5 + ((offsetEnd + offsetBegin - actualBarWidth) * 0.5);
    }

    public int GetStackIndex(string stackGroup) => _stackIndexMapping[stackGroup];

    // Updates the actual maximum and minimum values. If the user has zoomed/panned the axis, the internal ViewMaximum/ViewMinimum values will be used. If Maximum or Minimum have been set, these values will be used. Otherwise the maximum and minimum values of the series will be used, including the 'padding'.
    internal override void UpdateActualMaxMin()
    {
        var count = SourceLabels.Count;

        // Update the DataMinimum/DataMaximum from the number of categories
        Include(-0.5);
        Include((count > 0) ? (count - 1) + 0.5 : 0.5);

        base.UpdateActualMaxMin();

        MinorStep = 1;
    }

    /// <summary>
    /// Updates the axis with information from the plot series.
    /// </summary>
    /// <param name="series">The series collection.</param>
    /// <remarks>This is used by the category axis that need to know the number of series using the axis.</remarks>
    internal override void UpdateFromSeries(Series[] series)
    {
        _stackIndexMapping.Clear();

        var len = SourceLabels.Count;

        if (len == 0)
        {
            _totalWidthPerCategory = null;
            _maxWidth = double.NaN;
            _barOffset = null;
            _stackedBarOffset = null;
            return;
        }

        _totalWidthPerCategory = new double[len];

        // Add width of stacked series
        var timelines = series.OfType<TimelineSeries>().ToList();
        var stackedSeries = timelines.OfType<TimelineSeries>().Where(s => s.IsStacked).ToList();
        var stackIndices = stackedSeries.Select(s => s.StackGroup).Distinct().ToList();
        var stackRankBarWidth = new Dictionary<int, double>();

        for (var j = 0; j < stackIndices.Count; j++)
        {
            var maxBarWidth = stackedSeries
                .Where(s => s.StackGroup == stackIndices[j])
                .Select(s => s.BarWidth)
                .Concat(new[] { 0.0 })
                .Max();
            for (var i = 0; i < len; i++)
            {
                int k = 0;
                if (stackedSeries
                    .SelectMany(s => s.Items)
                    .Any(item => item.GetCategoryIndex(k++) == i))
                {
                    _totalWidthPerCategory[i] += maxBarWidth;
                }
            }

            stackRankBarWidth[j] = maxBarWidth;
        }

        // Add width of unstacked series
        var unstackedBarSeries = timelines.Where(s => s is not IStackableSeries || s.IsStacked == false).ToList();
        foreach (var s in unstackedBarSeries)
        {
            for (var i = 0; i < len; i++)
            {
                int j = 0;
                var numberOfItems = s.Items.Count(item => item.GetCategoryIndex(j++) == i);
                _totalWidthPerCategory[i] += s.BarWidth * numberOfItems;
            }
        }

        _maxWidth = _totalWidthPerCategory.Max();

        // Calculate BarOffset and StackedBarOffset
        _barOffset = new double[len];
        _stackedBarOffset = new double[stackIndices.Count + 1, len];

        var factor = 0.5 / (1 + GapWidth) / _maxWidth;
        for (var i = 0; i < len; i++)
        {
            _barOffset[i] = 0.5 - (_totalWidthPerCategory[i] * factor);
        }

        for (var j = 0; j <= stackIndices.Count; j++)
        {
            for (var i = 0; i < len; i++)
            {
                int k = 0;
                if (stackedSeries
                    .SelectMany(s => s.Items)
                    .All(item => item.GetCategoryIndex(k++) != i))
                {
                    continue;
                }

                _stackedBarOffset[j, i] = _barOffset[i];
                if (j < stackIndices.Count)
                {
                    _barOffset[i] += stackRankBarWidth[j] / (1 + GapWidth) / _maxWidth;
                }
            }
        }

        _stackIndexMapping = stackIndices
            .OrderBy(s => s)
            .Select((s, i) => new { Item = s, Index = i })
            .ToDictionary(x => x.Item, x => x.Index);
    }

    public void UpdateAll(PlotModel plot)
    {
        UpdateTransform(plot.PlotArea);

        UpdateIntervals(plot.PlotArea);
    }

    // Updates the scale and offset properties of the transform from the specified boundary rectangle.
    private void UpdateTransform(OxyRect bounds)
    {
        var endPosition = 0;
        var startPosition = 1;

        double a0 = bounds.Bottom;
        double a1 = bounds.Top;

        double dx = a1 - a0;
        a1 = a0 + (endPosition * dx);
        a0 = a0 + (startPosition * dx);

        ScreenMin = new ScreenPoint(a0, a1);
        ScreenMax = new ScreenPoint(a1, a0);

        if (ActualMaximum - ActualMinimum < double.Epsilon)
        {
            ActualMaximum = ActualMinimum + 1;
        }

        double max = ActualMaximum;
        double min = ActualMinimum;

        double da = a0 - a1;
        double range = max - min;

        if (Math.Abs(da) > double.Epsilon)
        {
            _offset = (a0 / da * max) - (a1 / da * min);
        }
        else
        {
            _offset = 0;
        }

        if (Math.Abs(range) > double.Epsilon)
        {
            _scale = (a1 - a0) / range;
        }
        else
        {
            _scale = 1;
        }
    }

    // Updates the actual minor and major step intervals.
    private void UpdateIntervals(OxyRect plotArea)
    {
        var actualMajorStep = MajorStep ?? CalculateActualInterval(plotArea.Height, IntervalLength);

        var actualMinorStep = MinorStep ?? CalculateMinorInterval(actualMajorStep);

        ActualMinorStep = Math.Max(actualMinorStep, MinimumMinorStep);
        ActualMajorStep = Math.Max(actualMajorStep, MinimumMajorStep);

        ActualStringFormat = StringFormat;
    }
}
