using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Controls;

public class CategoryListBox : ListBox
{
    private int _startIndex = 0;

    protected override Type StyleKeyOverride => typeof(CategoryListBox);

    static CategoryListBox()
    {
        AffectsMeasure<CategoryListBox>(ItemsSourceProperty);
    }

    public CategoryListBox()
    {
        ItemsSource2Property.Changed.Subscribe(ItemsSource2Changed);
        PlotModelProperty.Changed.Subscribe(PlotModelChanged);
    }

    public int StartIndex => _startIndex;

    public void InvalidateData()
    {
        if (ItemsSource2 != null)
        {
            var count = ItemsSource2.Count();

            if (_startIndex + ActiveCount > count)
            {
                _startIndex = count - (int)ActiveCount;
            }

            ItemsSource = ItemsSource2
                .Skip(_startIndex)
                .Take((int)ActiveCount)
                .ToList();
        }
    }

    private void ItemsSource2Changed(AvaloniaPropertyChangedEventArgs e)
    {
        InvalidateData();
    }

    private void PlotModelChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (PlotModel != null)
        {
            _startIndex = 0;

            ActiveCount = PlotModel.AxisY.ActiveCategoriesCount;

            ItemsSource2 = PlotModel.AxisY.SourceLabels
                .Select(s => new CategoryListBoxItem()
                {
                    Text = s
                })
                .ToList();
        }
    }

    public static readonly StyledProperty<IEnumerable<object>> ItemsSource2Property =
        AvaloniaProperty.Register<CategoryListBox, IEnumerable<object>>(nameof(ItemsSource2));

    public static readonly StyledProperty<PlotModel> PlotModelProperty =
        AvaloniaProperty.Register<CategoryListBox, PlotModel>(nameof(PlotModel));

    public static readonly StyledProperty<double> ActiveCountProperty =
        AvaloniaProperty.Register<CategoryListBox, double>(nameof(ActiveCount));

    public PlotModel PlotModel
    {
        get => GetValue(PlotModelProperty);
        set => SetValue(PlotModelProperty, value);
    }

    public IEnumerable<object> ItemsSource2
    {
        get => GetValue(ItemsSource2Property);
        set => SetValue(ItemsSource2Property, value);
    }

    public double ActiveCount
    {
        get => GetValue(ActiveCountProperty);
        set => SetValue(ActiveCountProperty, value);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = e.Delta.Y;
        var start = _startIndex;
        var count = ItemsSource2.Count();

        start += (delta > 0) ? -1 : 1;

        start = Math.Max(start, 0);
        start = Math.Min(start, count - (int)ActiveCount);

        _startIndex = start;

        ItemsSource = ItemsSource2
            .Skip(_startIndex)
            .Take((int)ActiveCount)
            .ToList();

        PlotModel?.PanToCategory(_startIndex);
    }
}
