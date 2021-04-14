using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.LogicalTree;
using TimeDataViewer.Markers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using TimeDataViewer.Shapes;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;

namespace TimeDataViewer
{
    public record Interval(double Left, double Right);

    public class Series : ItemsControl, IStyleable // TemplatedControl
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private ObservableCollection<object> _itemsSource;

        public Series()
        {
            _itemsSource = new ObservableCollection<object>();

            //ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;

            //PropertyChanged += Series_PropertyChanged;

            //Initialized += (s, e) => UpdateItems();

            //ItemsSourceProperty.Changed.AddClassHandler<Series>((d, e) => d.UpdateItems());
            //LeftBindingPathProperty.Changed.AddClassHandler<Series>((d, e) => d.UpdateItems());
            //RightBindingPathProperty.Changed.AddClassHandler<Series>((d, e) => d.UpdateItems());
        }

        private void Series_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(ItemsSource))
            {
                if (e.NewValue is not null)
                {
                    int dfdf = 0;
                }
            }
        }

        public static readonly StyledProperty<BaseIntervalVisual> IntervalTemplateProperty =    
            AvaloniaProperty.Register<Series, BaseIntervalVisual>(nameof(IntervalTemplate), new IntervalVisual());

        public BaseIntervalVisual IntervalTemplate
        {
            get { return GetValue(IntervalTemplateProperty); }
            set { SetValue(IntervalTemplateProperty, value); }
        }

        public static readonly DirectProperty<Series, ObservableCollection<object>> ItemsSourceProperty =            
            AvaloniaProperty.RegisterDirect<Series, ObservableCollection<object>>(nameof(ItemsSource), o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        [Content]
        public ObservableCollection<object> ItemsSource
        {
            get { return _itemsSource; }
            set { SetAndRaise(ItemsSourceProperty, ref _itemsSource, value); }
        }

        public static readonly StyledProperty<string> LeftBindingPathProperty =    
            AvaloniaProperty.Register<Series, string>(nameof(LeftBindingPath), string.Empty);

        public string LeftBindingPath
        {
            get { return GetValue(LeftBindingPathProperty); }
            set { SetValue(LeftBindingPathProperty, value); }
        }

        public static readonly StyledProperty<string> RightBindingPathProperty =    
            AvaloniaProperty.Register<Series, string>(nameof(RightBindingPath), string.Empty);

        public string RightBindingPath
        {
            get { return GetValue(RightBindingPathProperty); }
            set { SetValue(RightBindingPathProperty, value); }
        }

        private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            PassingLogicalTree(e);
        }

        private void PassingLogicalTree(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(this);
                }
                LogicalChildren.AddRange(e.NewItems.OfType<ILogical>());
                VisualChildren.AddRange(e.NewItems.OfType<IVisual>());
            }

            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(null);
                }
                foreach (var item in e.OldItems)
                {
                    LogicalChildren.Remove((ILogical)item);
                    VisualChildren.Remove((IVisual)item);
                }
            }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);

            if (e.NewValue is not null && e.NewValue is IEnumerable items)
            {
                if (items is IEnumerable<Interval>)
                {
                    Map?.UpdateData();
                }
                else
                {
                    UpdateItems(items);
                }
            }
        }

        private void UpdateItems(IEnumerable items)
        {
            if (string.IsNullOrWhiteSpace(LeftBindingPath) == false && string.IsNullOrWhiteSpace(RightBindingPath) == false)
            {
                var list = new List<Interval>();

                foreach (var item in items)
                {
                    var propertyInfoLeft = item.GetType().GetProperty(LeftBindingPath);
                    var propertyInfoRight = item.GetType().GetProperty(RightBindingPath);

                    var valueLeft = propertyInfoLeft?.GetValue(item, null);
                    var valueRight = propertyInfoRight?.GetValue(item, null);

                    if (valueLeft is not null && valueRight is not null && valueLeft is double left && valueRight is double right)
                    {
                        list.Add(new Interval(left, right));
                    }
                }

                Items = new ObservableCollection<Interval>(list);
            }
        }

        public SchedulerControl? Map => (((ILogical)this).LogicalParent is SchedulerControl map) ? map : null;
    }

}
