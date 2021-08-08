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
using TimeDataViewer.Models;
using Core = TimeDataViewer.Core;

namespace TimeDataViewer
{
    public class TimelineSeries : Series
    {
        static TimelineSeries()
        {
          //  BarWidthProperty.Changed.AddClassHandler<TimelineSeries>(AppearanceChanged);
          //  FillBrushProperty.Changed.AddClassHandler<TimelineSeries>(AppearanceChanged);
          //  StrokeColorProperty.Changed.AddClassHandler<TimelineSeries>(AppearanceChanged);
            CategoryProperty.Changed.AddClassHandler<TimelineSeries>(DataChanged);
            BeginFieldProperty.Changed.AddClassHandler<TimelineSeries>(DataChanged);
            EndFieldProperty.Changed.AddClassHandler<TimelineSeries>(DataChanged);
        }

        public TimelineSeries()
        {
            InternalSeries = new Core.TimelineSeries();
        }

        public static readonly StyledProperty<string> BeginFieldProperty = AvaloniaProperty.Register<Series, string>(nameof(BeginField), string.Empty);

        public string BeginField
        {
            get { return GetValue(BeginFieldProperty); }
            set { SetValue(BeginFieldProperty, value); }
        }

        public static readonly StyledProperty<string> EndFieldProperty = AvaloniaProperty.Register<Series, string>(nameof(EndField), string.Empty);

        public string EndField
        {
            get { return GetValue(EndFieldProperty); }
            set { SetValue(EndFieldProperty, value); }
        }

        public static readonly StyledProperty<string> CategoryProperty = AvaloniaProperty.Register<Series, string>(nameof(Category), string.Empty);

        public string Category
        {
            get { return GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
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
        }

        protected override void UpdateData(IEnumerable items)
        {
            IList<Interval> list;

            if (items is IEnumerable<Interval> ivals)
            {
                list = new List<Interval>(ivals);
            }
            else
            {
                list = UpdateItems(items);
            }

            _seriesViewModel =  new SeriesViewModel()
            {
                //Name = Category,
                //ZIndex = 30,
            };
       
            var intervals = list.Select(s => 
            new IntervalViewModel(s.Left, s.Right)
            {
                ZIndex = 100,
                SeriesControl = this,
            });

            _seriesViewModel.ReplaceIntervals(intervals);
        }

        private IList<Interval> UpdateItems(IEnumerable items)
        {
            if (string.IsNullOrWhiteSpace(BeginField) == false && string.IsNullOrWhiteSpace(EndField) == false)
            {
                var list = new List<Interval>();

                foreach (var item in items)
                {
                    var propertyInfoLeft = item.GetType().GetProperty(BeginField);
                    var propertyInfoRight = item.GetType().GetProperty(EndField);

                    var valueLeft = propertyInfoLeft?.GetValue(item, null);
                    var valueRight = propertyInfoRight?.GetValue(item, null);

                    if (valueLeft is not null && valueRight is not null && valueLeft is double left && valueRight is double right)
                    {
                        list.Add(new Interval(left, right));
                    }
                }
                return list;
            }

            return new List<Interval>();
        }
    }
}
