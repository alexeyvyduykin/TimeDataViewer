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
using Avalonia.Utilities;

namespace TimeDataViewer
{
    public record Interval(double Left, double Right);

    public abstract class Series : ItemsControl
    {          
        //private BaseIntervalShape _intervalTemplate;
        private readonly EventListener _eventListener;

        protected Series()
        {
            _eventListener = new EventListener(OnCollectionChanged);
        }

        public Core.Series InternalSeries { get; protected set; }

        public abstract Core.Series CreateModel();

        public bool DirtyItems { get; set; } = false;

        //public static readonly StyledProperty<BaseIntervalShape> IntervalTemplateProperty =
        //    AvaloniaProperty.Register<Series, BaseIntervalShape>(nameof(IntervalTemplate));

        //public BaseIntervalShape IntervalTemplate
        //{
        //    get { return _intervalTemplate; }
        //    set { SetAndRaise(IntervalTemplateProperty, ref _intervalTemplate, value); }
        //}

        public static readonly StyledProperty<Control> TooltipProperty =    
            AvaloniaProperty.Register<Series, Control>(nameof(Tooltip), new IntervalTooltip());

        public Control Tooltip
        {
            get { return GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        protected static void DataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnDataChanged();
        }

        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnVisualChanged();
        }

        protected void OnDataChanged()
        {
            if (Parent is Core.IPlotView pc)
            {
                pc.InvalidatePlot();
            }
        }

        protected void OnVisualChanged()
        {
            if (Parent is Core.IPlotView pc)
            {
                pc.InvalidatePlot(false);
            }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
            SubscribeToCollectionChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
            OnDataChanged();            
        }

        private void SubscribeToCollectionChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var collection = oldValue as INotifyCollectionChanged;
            if (collection != null)
            {
                WeakSubscriptionManager.Unsubscribe(collection, "CollectionChanged", _eventListener);
            }

            collection = newValue as INotifyCollectionChanged;
            if (collection != null)
            {
                WeakSubscriptionManager.Subscribe(collection, "CollectionChanged", _eventListener);
            }
        }
        
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnDataChanged();
        }

        protected virtual void SynchronizeProperties(Core.Series s)
        {
         //   s.IsVisible = IsVisible;                        
        }

        public Timeline? Scheduler => (((ILogical)this).LogicalParent is Timeline scheduler) ? scheduler : null;

        public virtual IntervalTooltipViewModel CreateTooltip(Core.TimelineItem marker)
        {
            return new IntervalTooltipViewModel(marker);
        }

        //public virtual BaseShape CreateIntervalShape()
        //{
        //    return IntervalTemplate.Clone();
        //}

        private class EventListener : IWeakSubscriber<NotifyCollectionChangedEventArgs>
        {
            /// <summary>
            /// The delegate to forward to
            /// </summary>
            private readonly EventHandler<NotifyCollectionChangedEventArgs> onCollectionChanged;

            /// <summary>
            /// Initializes a new instance of the <see cref="EventListener" /> class
            /// </summary>
            /// <param name="onCollectionChanged">The handler</param>
            public EventListener(EventHandler<NotifyCollectionChangedEventArgs> onCollectionChanged)
            {
                this.onCollectionChanged = onCollectionChanged;
            }

            public void OnEvent(object sender, NotifyCollectionChangedEventArgs e)
            {
                onCollectionChanged(sender, e);
            }
        }
    }
}
