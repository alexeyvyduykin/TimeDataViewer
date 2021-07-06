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
using Timeline.Spatial;
using Timeline.ViewModels;
using Timeline.Models;
using Avalonia.LogicalTree;

namespace Timeline.Shapes
{
    public abstract class BaseVisual : Control, IShape
    {   
        private IMarker? _marker;
        private ITimeline? _scheduler;

        public BaseVisual()
        {
            DataContextProperty.Changed.AddClassHandler<BaseVisual>((d, e) => d.MarkerChanged(e));
        }

        public ITimeline? Scheduler => _scheduler;

        public IMarker? Marker => _marker;

        private void MarkerChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is IMarker marker)
            {
                _marker = marker;        
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            ILogical control = this;

            while((control.LogicalParent is ITimeline) == false)
            {
                control = control.LogicalParent;
            }

            _scheduler = (ITimeline)control.LogicalParent;

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
