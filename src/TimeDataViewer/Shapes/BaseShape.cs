using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using System.Windows.Input;
using Avalonia.Media;
using System.Globalization;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.Specialized;
using Avalonia.Controls.Shapes;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;
using Avalonia.LogicalTree;

namespace TimeDataViewer.Shapes
{
    public abstract class BaseShape : Control
    {   
        private Core.TimelineItem? _marker;
        private SchedulerControl? _scheduler;

        public BaseShape()
        {
            DataContextProperty.Changed.AddClassHandler<BaseShape>((d, e) => d.MarkerChanged(e));
        }

        public SchedulerControl? Scheduler => _scheduler;

        public Core.TimelineItem? Marker => _marker;

        private void MarkerChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is Core.TimelineItem marker)
            {
                _marker = marker;        
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            ILogical control = this;

            while((control.LogicalParent is SchedulerControl) == false)
            {
                control = control.LogicalParent;
            }

            _scheduler = (SchedulerControl)control.LogicalParent;

            _scheduler.OnZoomChanged += HandleUpdateEvent;
            _scheduler.OnSizeChanged += HandleUpdateEvent;
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            if (_scheduler is not null)
            {
                _scheduler.OnZoomChanged -= HandleUpdateEvent;
                _scheduler.OnSizeChanged -= HandleUpdateEvent;
            }
        }

        private void HandleUpdateEvent(object? sender, EventArgs e)
        {
            Update();
        }

        protected abstract void Update();
    }
}
