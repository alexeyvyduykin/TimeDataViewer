﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using TimeDataViewerLite.Core;

namespace TimeDataViewerLite.Controls;

public class CategoryListBox : ListBox, IStyleable
{
    private int _startIndex = 0;

    Type IStyleable.StyleKey => typeof(CategoryListBox);

    static CategoryListBox()
    {
        AffectsMeasure<CategoryListBox>(ItemsProperty);
    }

    public CategoryListBox()
    {
        ItemsSourceProperty.Changed.Subscribe(ItemsSourceChanged);
    }

    public int StartIndex => _startIndex;

    private void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (ItemsSource != null)
        {
            var count = ItemsSource.Count();

            if (_startIndex + ActiveCount > count)
            {
                _startIndex = count - (int)ActiveCount;
            }

            Items = ItemsSource
                .Skip(_startIndex)
                .Take((int)ActiveCount)
                .ToList();
        }
    }

    public static readonly StyledProperty<double> ActiveCountProperty =
        AvaloniaProperty.Register<CategoryListBox, double>(nameof(ActiveCount), 10);

    public static readonly StyledProperty<IEnumerable<object>> ItemsSourceProperty =
        AvaloniaProperty.Register<CategoryListBox, IEnumerable<object>>(nameof(ItemsSource));

    public double ActiveCount
    {
        get => GetValue(ActiveCountProperty);
        set => SetValue(ActiveCountProperty, value);
    }

    public IEnumerable<object> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public void Update(PlotModel plotModel)
    {
        _startIndex = 0;

        ActiveCount = 10;

        ItemsSource = plotModel.AxisY.SourceLabels
            .Select(s => new CategoryListBoxItem()
            {
                Text = s
            })
            .ToList();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = e.Delta.Y;
        var start = _startIndex;
        var count = ItemsSource.Count();

        start += (delta > 0) ? -1 : 1;

        start = Math.Max(start, 0);
        start = Math.Min(start, count - (int)ActiveCount);

        _startIndex = start;

        Items = ItemsSource
            .Skip(_startIndex)
            .Take((int)ActiveCount)
            .ToList();
    }
}
