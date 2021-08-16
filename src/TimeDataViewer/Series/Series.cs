using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Utilities;

namespace TimeDataViewer
{
    public abstract class Series : ItemsControl
    {
        public static readonly StyledProperty<Color> ColorProperty =
            AvaloniaProperty.Register<Series, Color>(nameof(Color), Colors.Transparent);

        private readonly EventListener eventListener;

        static Series()
        {
            IsVisibleProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            BackgroundProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
            ColorProperty.Changed.AddClassHandler<Series>(AppearanceChanged);
        }

        public abstract void MyRender(Canvas canvasPlot);

        protected Series()
        {
            eventListener = new EventListener(OnCollectionChanged);
        }

        public Color Color
        {
            get
            {
                return GetValue(ColorProperty);
            }

            set
            {
                SetValue(ColorProperty, value);
            }
        }

        public Core.Series InternalSeries { get; protected set; }

        public abstract Core.Series CreateModel();

        protected static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnVisualChanged();
        }

        protected static void DataChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Series)d).OnDataChanged();
        }

        protected void OnDataChanged()
        {
            if (Parent is Core.IPlotView pc)
            {
                pc.InvalidatePlot();
            }
        }

        protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
            SubscribeToCollectionChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
            OnDataChanged();
        }

        protected void OnVisualChanged()
        {
            if (Parent is Core.IPlotView pc)
            {
                pc.InvalidatePlot(false);
            }
        }

        protected override void OnAttachedToLogicalTree(global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            //BeginInit();
            //EndInit();
        }

        protected virtual void SynchronizeProperties(Core.Series s)
        {
            s.IsVisible = IsVisible;
        }

        /// <summary>
        /// If the ItemsSource implements INotifyCollectionChanged update the visual when the collection changes.
        /// </summary>
        /// <param name="oldValue">The old ItemsSource</param>
        /// <param name="newValue">The new ItemsSource</param>
        private void SubscribeToCollectionChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var collection = oldValue as INotifyCollectionChanged;
            if (collection != null)
            {
                WeakSubscriptionManager.Unsubscribe(collection, "CollectionChanged", eventListener);
            }

            collection = newValue as INotifyCollectionChanged;
            if (collection != null)
            {
                WeakSubscriptionManager.Subscribe(collection, "CollectionChanged", eventListener);
            }
        }

        /// <summary>
        /// Invalidate the view when the collection changes
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="notifyCollectionChangedEventArgs">The collection changed args</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnDataChanged();
        }

        /// <summary>
        /// Listens to and forwards any collection changed events
        /// </summary>
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
