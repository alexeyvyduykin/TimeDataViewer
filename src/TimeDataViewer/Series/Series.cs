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
    public record Interval(double Left, double Right);

    public abstract class Series : ItemsControl
    {          
        protected SeriesViewModel? _seriesViewModel;
        private BaseIntervalVisual _intervalTemplate;
       
        public event EventHandler? OnInvalidateData;

        public Series()
        {

        }

        public Core.Series InternalSeries { get; protected set; }

        public abstract Core.Series CreateModel();

        public SeriesViewModel? SeriesViewModel 
        {
            get => _seriesViewModel; 
            set => _seriesViewModel = value; 
        }

        public bool DirtyItems { get; set; } = false;

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

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            if (OnInvalidateData is not null)
            {
                foreach (var d in OnInvalidateData.GetInvocationList())
                {
                    OnInvalidateData -= (EventHandler)d;
                }
            }
        }

        protected static void DataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnDataChanged();
        }

        protected void OnDataChanged()
        {
            if (Parent is SchedulerControl pc)
            {
                pc.InvalidatePlot();
            }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);

            if (e.NewValue is not null && e.NewValue is IEnumerable items)
            {
                if (DirtyItems == false)
                {                                  
                    UpdateData(items);
                    DirtyItems = true;
                    OnInvalidateData?.Invoke(this, EventArgs.Empty);
                    //Debug.WriteLine($"Series -> OnInvalidateData -> Count = {OnInvalidateData?.GetInvocationList().Length}");
                }
            }
        }

        protected abstract void UpdateData(IEnumerable items);

        protected virtual void SynchronizeProperties(Core.Series s)
        {
         //   s.IsVisible = IsVisible;              
          
        }

        public SchedulerControl? Scheduler => (((ILogical)this).LogicalParent is SchedulerControl scheduler) ? scheduler : null;

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
