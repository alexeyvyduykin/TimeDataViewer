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
using TimeDataViewer.ViewModels;
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

        private static double OnCoerceZoom(IAvaloniaObject obj, double value)
        {
            if (obj is SchedulerControl scheduler)
            {             
                if (scheduler is not null)
                {
                    return Math.Clamp(value, scheduler.MinZoom, scheduler.MaxZoom);
                }
            }

            return value;
        }

        public static readonly StyledProperty<DateTime> EpochProperty =    
            AvaloniaProperty.Register<SchedulerControl, DateTime>(nameof(Epoch), DateTime.Now);

        public DateTime Epoch
        {
            get => GetValue(EpochProperty);
            set => SetValue(EpochProperty, value);
        }

        public static readonly StyledProperty<double> CurrentTimeProperty =    
            AvaloniaProperty.Register<SchedulerControl, double>(nameof(CurrentTime), 0.0);

        public double CurrentTime
        {
            get => GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly StyledProperty<ControlTemplate> ZoomRectangleTemplateProperty =   
            AvaloniaProperty.Register<SchedulerControl, ControlTemplate>(nameof(ZoomRectangleTemplate));

        public ControlTemplate ZoomRectangleTemplate
        {
            get
            {
                return GetValue(ZoomRectangleTemplateProperty);
            }

            set
            {
                SetValue(ZoomRectangleTemplateProperty, value);
            }
        }

        public static readonly StyledProperty<Cursor> PanCursorProperty =   
            AvaloniaProperty.Register<SchedulerControl, Cursor>(nameof(PanCursor), new Cursor(StandardCursorType.Hand));

        public Cursor PanCursor
        {
            get
            {
                return GetValue(PanCursorProperty);
            }

            set
            {
                SetValue(PanCursorProperty, value);
            }
        }

        public static readonly StyledProperty<Cursor> ZoomHorizontalCursorProperty =
            AvaloniaProperty.Register<SchedulerControl, Cursor>(nameof(ZoomHorizontalCursor), new Cursor(StandardCursorType.SizeWestEast));

        public Cursor ZoomHorizontalCursor
        {
            get
            {
                return GetValue(ZoomHorizontalCursorProperty);
            }

            set
            {
                SetValue(ZoomHorizontalCursorProperty, value);
            }
        }

        public static readonly StyledProperty<Cursor> ZoomRectangleCursorProperty =
            AvaloniaProperty.Register<SchedulerControl, Cursor>(nameof(ZoomRectangleCursor), new Cursor(StandardCursorType.SizeAll));

        public Cursor ZoomRectangleCursor
        {
            get
            {
                return GetValue(ZoomRectangleCursorProperty);
            }

            set
            {
                SetValue(ZoomRectangleCursorProperty, value);
            }
        }
        
        public static readonly StyledProperty<Cursor> ZoomVerticalCursorProperty =
            AvaloniaProperty.Register<SchedulerControl, Cursor>(nameof(ZoomVerticalCursor), new Cursor(StandardCursorType.SizeNorthSouth));

        public Cursor ZoomVerticalCursor
        {
            get
            {
                return GetValue(ZoomVerticalCursorProperty);
            }

            set
            {
                SetValue(ZoomVerticalCursorProperty, value);
            }
        }
    }
}
