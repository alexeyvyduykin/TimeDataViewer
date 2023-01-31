using System.Linq;

namespace TimeDataViewerLite.Core;

/// <remarks>The category axis is using the index of the label collection items as coordinates.
/// If you have 5 categories in the Labels collection, the categories will be placed at coordinates 0 to 4.
/// The range of the axis will be from -0.5 to 4.5 (excluding padding).</remarks>
public class CategoryAxis : Axis
{
    /// <summary>
    /// The current max value per StackIndex and Label.
    /// </summary>
    /// <remarks>These values are modified during rendering.</remarks>
    private double[,]? _currentMaxValue;

    /// <summary>
    /// The current min value per StackIndex and Label.
    /// </summary>
    /// <remarks>These values are modified during rendering.</remarks>
    private double[,]? _currentMinValue;

    /// <summary>
    /// The base value per StackIndex and Label for positive values of stacked bar series.
    /// </summary>
    /// <remarks>These values are modified during rendering.</remarks>
    private double[,]? _currentPositiveBaseValues;

    /// <summary>
    /// The base value per StackIndex and Label for negative values of stacked bar series.
    /// </summary>
    /// <remarks>These values are modified during rendering.</remarks>
    private double[,]? _currentNegativeBaseValues;

    // The maximum stack index.
    private int _maxStackIndex;

    // The maximal width of all labels.
    private double _maxWidth;

    // Gets or sets the original offset of the bars (not used for stacked bar series).
    private double[] _barOffset;

    // Gets or sets the stack index mapping. The mapping indicates to which rank a specific stack index belongs.
    private Dictionary<string, int> _stackIndexMapping = new();

    // Gets or sets the offset of the bars per StackIndex and Label (only used for stacked bar series).
    private double[,] _stackedBarOffset;

    // Gets or sets sum of the widths of the single bars per label. This is used to find the bar width of BarSeries
    private double[] _totalWidthPerCategory;

    public CategoryAxis()
    {
        Position = AxisPosition.Bottom;
        MajorStep = 1;
        GapWidth = 1;
    }

    public override object GetValue(double x) => FormatValue(x);

    /// <summary>
    /// Gets or sets the gap width.
    /// </summary>
    /// <remarks>The default value is 1.0 (100%). The gap width is given as a fraction of the total width/height of the items in a category.</remarks>
    public double GapWidth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ticks are centered. If this is <c>false</c>, ticks will be drawn between each category. If this is <c>true</c>, ticks will be drawn in the middle of each category.
    /// </summary>
    public bool IsTickCentered { get; set; }

    public List<string> SourceLabels { get; set; } = new();

    /// <summary>
    /// Gets the maximum width of all category labels.
    /// </summary>
    /// <returns>The maximum width.</returns>
    public double MaxWidth => _maxWidth;

    /// <summary>
    /// Gets the category value.
    /// </summary>
    /// <param name="categoryIndex">Index of the category.</param>
    /// <param name="stackIndex">Index of the stack.</param>
    /// <param name="actualBarWidth">Actual width of the bar.</param>
    /// <returns>The get category value.</returns>
    public double GetCategoryValue(int categoryIndex, int stackIndex, double actualBarWidth)
    {
        var offsetBegin = _stackedBarOffset[stackIndex, categoryIndex];
        var offsetEnd = _stackedBarOffset[stackIndex + 1, categoryIndex];
        return categoryIndex - 0.5 + ((offsetEnd + offsetBegin - actualBarWidth) * 0.5);
    }

    /// <summary>
    /// Gets the coordinates used to draw ticks and tick labels (numbers or category names).
    /// </summary>
    /// <param name="majorLabelValues">The major label values.</param>
    /// <param name="majorTickValues">The major tick values.</param>
    /// <param name="minorTickValues">The minor tick values.</param>
    public override void GetTickValues(
        out IList<double> majorLabelValues, out IList<double> majorTickValues, out IList<double> minorTickValues)
    {
        base.GetTickValues(out majorLabelValues, out majorTickValues, out minorTickValues);
        minorTickValues.Clear();

        if (!IsTickCentered)
        {
            // Subtract 0.5 from the label values to get the tick values.
            // Add one extra tick at the end.
            var mv = new List<double>(majorLabelValues.Count);
            mv.AddRange(majorLabelValues.Select(v => v - 0.5));
            if (mv.Count > 0)
            {
                mv.Add(mv[^1] + 1);
            }

            majorTickValues = mv;
        }
    }

    public int GetStackIndex(string stackGroup) => _stackIndexMapping[stackGroup];
    
    /// <summary>
    /// Updates the actual maximum and minimum values. If the user has zoomed/panned the axis, the internal ViewMaximum/ViewMinimum values will be used. If Maximum or Minimum have been set, these values will be used. Otherwise the maximum and minimum values of the series will be used, including the 'padding'.
    /// </summary>
    internal override void UpdateActualMaxMin()
    {
        // Update the DataMinimum/DataMaximum from the number of categories
        Include(-0.5);

        if (SourceLabels.Count > 0)
        {
            Include((SourceLabels.Count - 1) + 0.5);
        }
        else
        {
            Include(0.5);
        }

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
        base.UpdateFromSeries(series);

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
        var categorizedSeries = series.OfType<TimelineSeries>().ToList();
        var stackedSeries = categorizedSeries.OfType<IStackableSeries>().Where(s => s.IsStacked).ToList();
        var stackIndices = stackedSeries.Select(s => s.StackGroup).Distinct().ToList();
        var stackRankBarWidth = new Dictionary<int, double>();
        for (var j = 0; j < stackIndices.Count; j++)
        {
            var maxBarWidth =
                stackedSeries.Where(s => s.StackGroup == stackIndices[j]).Select(
                    s => ((TimelineSeries)s).GetBarWidth()).Concat(new[] { 0.0 }).Max();
            for (var i = 0; i < len; i++)
            {
                int k = 0;
                if (
                    stackedSeries.SelectMany(s => ((TimelineSeries)s).GetItems()).Any(
                        item => item.GetCategoryIndex(k++) == i))
                {
                    _totalWidthPerCategory[i] += maxBarWidth;
                }
            }

            stackRankBarWidth[j] = maxBarWidth;
        }

        // Add width of unstacked series
        var unstackedBarSeries = categorizedSeries.Where(s => !(s is IStackableSeries) || !((IStackableSeries)s).IsStacked).ToList();
        foreach (var s in unstackedBarSeries)
        {
            for (var i = 0; i < len; i++)
            {
                int j = 0;
                var numberOfItems = s.GetItems().Count(item => item.GetCategoryIndex(j++) == i);
                _totalWidthPerCategory[i] += s.GetBarWidth() * numberOfItems;
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
                if (
                    stackedSeries.SelectMany(s => ((TimelineSeries)s).GetItems()).All(
                        item => item.GetCategoryIndex(k++) != i))
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

        _maxStackIndex = stackIndices.Count;
    }

    /// <summary>
    /// Resets the current values.
    /// </summary>
    /// <remarks>The current values may be modified during update of max/min and rendering.</remarks>
    protected internal override void ResetCurrentValues()
    {
        base.ResetCurrentValues();

        var len = SourceLabels.Count;

        if (_maxStackIndex > 0)
        {
            _currentPositiveBaseValues = new double[_maxStackIndex, len];
            _currentPositiveBaseValues.Fill2D(double.NaN);
            _currentNegativeBaseValues = new double[_maxStackIndex, len];
            _currentNegativeBaseValues.Fill2D(double.NaN);

            _currentMaxValue = new double[_maxStackIndex, len];
            _currentMaxValue.Fill2D(double.NaN);
            _currentMinValue = new double[_maxStackIndex, len];
            _currentMinValue.Fill2D(double.NaN);
        }
        else
        {
            _currentPositiveBaseValues = null;
            _currentNegativeBaseValues = null;
            _currentMaxValue = null;
            _currentMinValue = null;
        }
    }

    /// <summary>
    /// Formats the value to be used on the axis.
    /// </summary>
    /// <param name="x">The value to format.</param>
    /// <returns>The formatted value.</returns>
    protected override string FormatValueOverride(double x)
    {
        var index = (int)x;
        
        if (index >= 0 && index < SourceLabels.Count)
        {
            return SourceLabels[index];
        }

        return null;
    }
}
