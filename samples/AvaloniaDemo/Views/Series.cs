﻿using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.LogicalTree;
using AvaloniaDemo.Markers;

namespace AvaloniaDemo.Views
{

    public class Series : ItemsControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);


        public SchedulerControl? Map { get; set; }

        public Series()
        {          
            PropertyChanged += Series_PropertyChanged;
        }

        public static readonly StyledProperty<BaseInterval> IntervalTemplateProperty =    
            AvaloniaProperty.Register<Series, BaseInterval>(nameof(IntervalTemplate), new IntervalVisual());

        public BaseInterval IntervalTemplate
        {
            get { return GetValue(IntervalTemplateProperty); }
            set { SetValue(IntervalTemplateProperty, value); }
        }

        private void Series_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {        
            if (e.Property.Name == nameof(Items))
            {
                
            }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
        
            if (e.NewValue is not null)
            {
                Map?.UpdateData();
            }
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);
                            
            Map?.UpdateData();            
        }
    }

}
