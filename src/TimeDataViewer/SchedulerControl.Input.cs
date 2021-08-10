using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Visuals;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TimeDataViewer.ViewModels;
using TimeDataViewer.Core;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Input.TextInput;

namespace TimeDataViewer
{
    public partial class SchedulerControl
    {
        private Point2D _mouseDownPoint;

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseWheel(this, e.ToMouseWheelEventArgs(this));
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Handled)
            {
                return;
            }

            Focus();
            e.Pointer.Capture(this);

            // store the mouse down point, check it when mouse button is released to determine if the context menu should be shown
            _mouseDownPoint = e.GetPosition(this).ToScreenPoint();

            e.Handled = ActualController.HandleMouseDown(this, e.ToMouseDownEventArgs(this));
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e.Handled)
            {
                return;
            }

            e.Handled = ActualController.HandleMouseMove(this, e.ToMouseEventArgs(this));

            
            var MouseScreenPosition = e.GetPosition(this);

            ActualModel.ZoomScreenPosition = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            var mousePosition = ActualModel.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            AxisX.UpdateDynamicLabelPosition(mousePosition);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.Handled)
            {
                return;
            }

            var releasedArgs = (PointerReleasedEventArgs)e;

            e.Pointer.Capture(null);

            e.Handled = ActualController.HandleMouseUp(this, releasedArgs.ToMouseReleasedEventArgs(this));

            // Open the context menu
            var p = e.GetPosition(this).ToScreenPoint();
            var d = p.DistanceTo(_mouseDownPoint);

            if (ContextMenu != null)
            {
                if (Math.Abs(d) < 1e-8 && releasedArgs.InitialPressMouseButton == MouseButton.Right)
                {
                    ContextMenu.DataContext = DataContext;
                    ContextMenu.IsVisible = true;
                }
                else
                {
                    ContextMenu.IsVisible = false;
                }
            }
        }

        //protected override void OnPointerEnter(PointerEventArgs e)
        //{
        //    base.OnPointerEnter(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseEnter(this, e.ToMouseEventArgs(this));
        //}

        //protected override void OnPointerLeave(PointerEventArgs e)
        //{
        //    base.OnPointerLeave(e);
        //    if (e.Handled)
        //    {
        //        return;
        //    }

        //    e.Handled = ActualController.HandleMouseLeave(this, e.ToMouseEventArgs(this));
        //}
    }
}
