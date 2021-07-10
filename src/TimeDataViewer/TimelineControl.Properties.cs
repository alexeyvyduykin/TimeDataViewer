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
using Timeline.ViewModels;
using Timeline;
using Timeline.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace Timeline
{
    public enum DurationMode { Auto, Manual };

    public partial class TimelineControl
    {
        public static readonly DirectProperty<TimelineControl, ObservableCollection<Series>> SeriesProperty =    
            AvaloniaProperty.RegisterDirect<TimelineControl, ObservableCollection<Series>>(nameof(Series), o => o.Series, (o, v) => o.Series = v);

        [Content]
        public ObservableCollection<Series> Series
        {
            get { return _series; }
            set { SetAndRaise(SeriesProperty, ref _series, value); }
        }

        public static readonly StyledProperty<double> ZoomProperty =    
            AvaloniaProperty.Register<TimelineControl, double>(nameof(Zoom), defaultValue: 0.0, inherits: true, defaultBindingMode: BindingMode.TwoWay, coerce: OnCoerceZoom);

        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        private static double OnCoerceZoom(IAvaloniaObject obj, double value)
        {
            if (obj is TimelineControl scheduler)
            {             
                if (scheduler is not null)
                {
                    return Math.Clamp(value, scheduler.MinZoom, scheduler.MaxZoom);
                }
            }

            return value;
        }

        public static readonly StyledProperty<DateTime> BeginProperty =    
            AvaloniaProperty.Register<TimelineControl, DateTime>(nameof(Begin), new DateTime(2000, 6, 20, 12, 0, 0));

        public DateTime Begin
        {
            get => GetValue(BeginProperty);
            set => SetValue(BeginProperty, value);
        }

        public static readonly StyledProperty<double> DurationProperty =    
            AvaloniaProperty.Register<TimelineControl, double>(nameof(Duration), 86400.0);

        public double Duration
        {
            get => GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public static readonly StyledProperty<DurationMode> DurationModeProperty =    
            AvaloniaProperty.Register<TimelineControl, DurationMode>(nameof(Begin), DurationMode.Manual);

        public DurationMode DurationMode
        {
            get => GetValue(DurationModeProperty);
            set => SetValue(DurationModeProperty, value);
        }








        public static readonly StyledProperty<double> CurrentTimeProperty =    
            AvaloniaProperty.Register<TimelineControl, double>(nameof(CurrentTime), 0.0);

        public double CurrentTime
        {
            get => GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly StyledProperty<RectI> WindowProperty =    
            AvaloniaProperty.Register<TimelineControl, RectI>(nameof(Window));

        public RectI Window
        {
            get => GetValue(WindowProperty);
            set => SetValue(WindowProperty, value);
        }

        public static readonly StyledProperty<RectD> ViewportProperty =    
            AvaloniaProperty.Register<TimelineControl, RectD>(nameof(Viewport));

        public RectD Viewport
        {
            get => GetValue(ViewportProperty);
            set => SetValue(ViewportProperty, value);
        }

        public static readonly StyledProperty<RectD> ClientViewportProperty =    
            AvaloniaProperty.Register<TimelineControl, RectD>(nameof(ClientViewport));

        public RectD ClientViewport
        {
            get => GetValue(ClientViewportProperty);
            set => SetValue(ClientViewportProperty, value);
        }
    }
}
