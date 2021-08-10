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
        private Timeline? _scheduler;

        public BaseShape()
        {
            DataContextProperty.Changed.AddClassHandler<BaseShape>((d, e) => d.MarkerChanged(e));
        }

        public Timeline? Scheduler => _scheduler;

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

            while((control.LogicalParent is Timeline) == false)
            {
                control = control.LogicalParent;
            }

            _scheduler = (Timeline)control.LogicalParent;
        }
    }
}
