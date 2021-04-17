using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TimeDataViewer.Markers;
using TimeDataViewer;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace TimeDataViewer
{
    public partial class SchedulerControl
    {
        public static readonly DirectProperty<SchedulerControl, ObservableCollection<Series>> SeriesProperty =    
            AvaloniaProperty.RegisterDirect<SchedulerControl, ObservableCollection<Series>>(nameof(Series), o => o.Series, (o, v) => o.Series = v);

        [Content]
        public ObservableCollection<Series> Series
        {
            get { return _series; }
            set { SetAndRaise(SeriesProperty, ref _series, value); }
        }

        public static readonly StyledProperty<double> ZoomProperty =    
            AvaloniaProperty.Register<SchedulerControl, double>(nameof(Zoom), defaultValue: 0.0, inherits: true, defaultBindingMode: BindingMode.TwoWay, coerce: OnCoerceZoom);

        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        private static double OnCoerceZoom(IAvaloniaObject o, double value)
        {
            SchedulerControl scheduler = o as SchedulerControl;
            if (scheduler != null)
            {
                if (value > scheduler.MaxZoom)
                {
                    value = scheduler.MaxZoom;
                }
                if (value < scheduler.MinZoom)
                {
                    value = scheduler.MinZoom;
                }

                return value;
            }
            else
            {
                return value;
            }
        }

        public static readonly StyledProperty<DateTime> EpochProperty =    
            AvaloniaProperty.Register<SchedulerControl, DateTime>(nameof(Epoch), DateTime.Now);

        public DateTime Epoch
        {
            get => GetValue(EpochProperty);
            set => SetValue(EpochProperty, value);
        }
    }
}
