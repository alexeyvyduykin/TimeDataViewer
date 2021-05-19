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

namespace TimeDataViewer
{
    public record Interval(double Left, double Right);

    public class Series : ItemsControl, IStyleable, ISeriesControl
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private readonly Factory _factory;
        private SeriesViewModel? _seriesViewModel;
        private string _leftBindingPath;
        private string _rightBindingPath;
        private string _category;
        private BaseIntervalVisual _intervalTemplate;

        public event EventHandler? OnInvalidateData;

        public Series()
        {
            _factory = new Factory();
        //    IntervalTemplate = new IntervalVisual() { /*Series = this*/ };

        //    IntervalTemplateProperty.Changed.AddClassHandler<Series>((d, e) => d.IntervalTemplateChanged(e));
        }

        public SeriesViewModel? SeriesViewModel 
        {
            get => _seriesViewModel; 
            set => _seriesViewModel = value; 
        }

        public bool DirtyItems { get; set; } = false;

        private void IntervalTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if(e.NewValue is not null && e.NewValue is BaseIntervalVisual ival)
            {
                //ival.Series = this;
            }
        }

        public static readonly StyledProperty<BaseIntervalVisual> IntervalTemplateProperty =
            AvaloniaProperty.Register<Series, BaseIntervalVisual>(nameof(IntervalTemplate));

        public BaseIntervalVisual IntervalTemplate
        {
            get { return _intervalTemplate; }
            set { SetAndRaise(IntervalTemplateProperty, ref _intervalTemplate, value); }
        }

        public static readonly StyledProperty<Control> TooltipProperty =    
            AvaloniaProperty.Register<Series, Control>(nameof(Tooltip), new IntervalTooltip());

        public Control Tooltip
        {
            get { return GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public static readonly StyledProperty<string> LeftBindingPathProperty =    
            AvaloniaProperty.Register<Series, string>(nameof(LeftBindingPath), string.Empty);

        public string LeftBindingPath
        {
            //get { return GetValue(LeftBindingPathProperty); }
            //set { SetValue(LeftBindingPathProperty, value); }
            get { return _leftBindingPath; }
            set { SetAndRaise(LeftBindingPathProperty, ref _leftBindingPath, value); }
        }

        public static readonly StyledProperty<string> RightBindingPathProperty =    
            AvaloniaProperty.Register<Series, string>(nameof(RightBindingPath), string.Empty);

        public string RightBindingPath
        {
            //get { return GetValue(RightBindingPathProperty); }
            //set { SetValue(RightBindingPathProperty, value); }
            get { return _rightBindingPath; }
            set { SetAndRaise(RightBindingPathProperty, ref _rightBindingPath, value); }
        }

        public static readonly StyledProperty<string> CategoryProperty =    
            AvaloniaProperty.Register<Series, string>(nameof(Category), string.Empty);

        public string Category
        {
            //get { return GetValue(CategoryProperty); }
            //set { SetValue(CategoryProperty, value); }
            get { return _category; }
            set { SetAndRaise(CategoryProperty, ref _category, value); }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);

            if (e.NewValue is not null && e.NewValue is IEnumerable items)
            {
                if (DirtyItems == false)
                {                   
                    //Task.Run(() => update(items));
                    update(items);

                    DirtyItems = true;
                    OnInvalidateData?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void update(IEnumerable items)
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

            _seriesViewModel = _factory.CreateSeries(_category, this);
            
            //var intervals = list.Select(s => _factory.CreateInterval(s, _intervalTemplate));
            var intervals = list.Select(s => _factory.CreateInterval(s.Left, s.Right, this));

            _seriesViewModel.AddIntervals(intervals);

           // Ivals = list.Select(s => _factory.CreateInterval(s, String, IntervalTemplate));
        }

        private IList<Interval> UpdateItems(IEnumerable items)
        {
            if (string.IsNullOrWhiteSpace(_leftBindingPath) == false && string.IsNullOrWhiteSpace(_rightBindingPath) == false)
            {
                var list = new List<Interval>();

                foreach (var item in items)
                {
                    var propertyInfoLeft = item.GetType().GetProperty(_leftBindingPath);
                    var propertyInfoRight = item.GetType().GetProperty(_rightBindingPath);

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

        //public SchedulerControl? Scheduler => (((ILogical)this).LogicalParent is SchedulerControl scheduler) ? scheduler : null;

        public virtual IntervalTooltipViewModel CreateTooltip(IntervalViewModel marker)
        {
            return new IntervalTooltipViewModel(marker);
        }

        public virtual IShape CreateIntervalShape(IntervalViewModel interval)
        {
            return IntervalTemplate.Clone(interval);
        }

        public IShape CreateSeriesShape()
        {                
            return new SeriesVisual() { DataContext = this };            
        }
    }
}
