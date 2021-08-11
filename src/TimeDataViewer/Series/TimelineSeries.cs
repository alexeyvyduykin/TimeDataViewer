using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.LogicalTree;
using TimeDataViewer.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
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
using TimeDataViewer.Views;
using System.Threading.Tasks;
using Core = TimeDataViewer.Core;

namespace TimeDataViewer
{
    public class TimelineSeries : Series
    {
        static TimelineSeries()
        {
          //  BarWidthProperty.Changed.AddClassHandler<TimelineSeries>(AppearanceChanged);
            FillBrushProperty.Changed.AddClassHandler<TimelineSeries>(AppearanceChanged);
            StrokeBrushProperty.Changed.AddClassHandler<TimelineSeries>(AppearanceChanged);
            CategoryProperty.Changed.AddClassHandler<TimelineSeries>(DataChanged);
            BeginFieldProperty.Changed.AddClassHandler<TimelineSeries>(DataChanged);
            EndFieldProperty.Changed.AddClassHandler<TimelineSeries>(DataChanged);
        }

        public TimelineSeries()
        {
            InternalSeries = new Core.TimelineSeries();
        }

        public static readonly StyledProperty<string> BeginFieldProperty = 
            AvaloniaProperty.Register<TimelineSeries, string>(nameof(BeginField), string.Empty);

        public string BeginField
        {
            get { return GetValue(BeginFieldProperty); }
            set { SetValue(BeginFieldProperty, value); }
        }

        public static readonly StyledProperty<string> EndFieldProperty = 
            AvaloniaProperty.Register<TimelineSeries, string>(nameof(EndField), string.Empty);

        public string EndField
        {
            get { return GetValue(EndFieldProperty); }
            set { SetValue(EndFieldProperty, value); }
        }

        public static readonly StyledProperty<string> CategoryProperty = 
            AvaloniaProperty.Register<TimelineSeries, string>(nameof(Category), string.Empty);

        public string Category
        {
            get { return GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        public static readonly StyledProperty<IBrush> FillBrushProperty =    
            AvaloniaProperty.Register<TimelineSeries, IBrush>(nameof(FillBrush), Brushes.Red);

        public IBrush FillBrush
        {
            get
            {
                return GetValue(FillBrushProperty);
            }

            set
            {
                SetValue(FillBrushProperty, value);
            }
        }
        
        public static readonly StyledProperty<IBrush> StrokeBrushProperty =
            AvaloniaProperty.Register<TimelineSeries, IBrush>(nameof(StrokeBrush), Brushes.Black);

        public IBrush StrokeBrush
        {
            get
            {
                return GetValue(StrokeBrushProperty);
            }

            set
            {
                SetValue(StrokeBrushProperty, value);
            }
        }

        public override Core.Series CreateModel()
        {
            SynchronizeProperties(InternalSeries);
            return InternalSeries;
        }

        protected override void SynchronizeProperties(Core.Series series)
        {
            base.SynchronizeProperties(series);
            var s = (Core.TimelineSeries)series;

            s.ItemsSource = Items;
            //  s.BarWidth = BarWidth;
            s.CategoryField = Category;
            s.BeginField = BeginField;
            s.EndField = EndField;

            s.SeriesControl = this;
        }
    }
}
