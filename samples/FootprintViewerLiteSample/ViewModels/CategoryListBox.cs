using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

namespace FootprintViewerLiteSample.ViewModels;

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
        ActiveCountProperty.Changed.Subscribe(ItemsSourceChanged);
    }

    private void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (ItemsSource != null)
        {
            Items = ItemsSource.Skip(_startIndex).Take(ActiveCount).ToList();
        }
    }

    public static readonly StyledProperty<int> ActiveCountProperty =
        AvaloniaProperty.Register<CategoryListBox, int>(nameof(ActiveCount));

    public static readonly StyledProperty<IList<ItemViewModel>> ItemsSourceProperty =
        AvaloniaProperty.Register<CategoryListBox, IList<ItemViewModel>>(nameof(ItemsSource));

    public int ActiveCount
    {
        get => GetValue(ActiveCountProperty);
        set => SetValue(ActiveCountProperty, value);
    }

    public IList<ItemViewModel> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = e.Delta.Y;
        var start = _startIndex;
        var count = ItemsSource.Count;

        start += (delta > 0) ? -1 : 1;

        start = Math.Max(start, 0);
        start = Math.Min(start, count - ActiveCount);

        _startIndex = start;

        Items = ItemsSource
            .Skip(_startIndex)
            .Take(ActiveCount)
            .ToList();
    }
}
