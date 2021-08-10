using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;
using System.Collections.ObjectModel;
using Avalonia;
using TimeDataViewer.ViewModels;
using Core = TimeDataViewer.Core;
using Avalonia.Input;

namespace TimeDataViewer
{
    public static class Extensions
    {
        public static bool IsEmpty(this RectD rect)
        {
            return rect.Left == 0.0 && rect.Right == 0.0 && rect.Width == 0.0 && rect.Height == 0.0;
        }

        public static bool IsEmpty(this Point2D point)
        {
            return point.X == 0.0 && point.Y == 0.0;
        }

        public static Point2D GetCenter(this RectD rect)
        {
            return new(rect.X + rect.Width / 2, rect.Y - rect.Height / 2);
        }

        public static void AddRange<T>(this ObservableCollection<T> arr, IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                arr.Add(item);
            }
        }

        public static Rect ToAvaloniaRect(this RectD rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void ReplaceIntervals(this Core.TimelineSeries series, IEnumerable<Core.TimelineItem> ivals)
        {
            series.Items.Clear();
          
            foreach (var item in ivals)
            {             
                series.Items.Add(item);
            }
        }

        public static Core.OxyMouseWheelEventArgs ToMouseWheelEventArgs(this PointerWheelEventArgs e, IInputElement relativeTo)
        {
            return new Core.OxyMouseWheelEventArgs
            {
                Position = e.GetPosition(relativeTo).ToScreenPoint(),
                //ModifierKeys = Keyboard.Instance.GetModifierKeys(),
                Delta = (int)(e.Delta.Y + e.Delta.X) * 120
            };
        }

        public static Point2D ToScreenPoint(this Point pt)
        {
            return new Point2D(pt.X, pt.Y);
        }

        public static Core.OxyMouseDownEventArgs ToMouseDownEventArgs(this PointerPressedEventArgs e, IInputElement relativeTo)
        {
            var point = e.GetCurrentPoint(relativeTo);

            return new Core.OxyMouseDownEventArgs
            {
                ChangedButton = point.Properties.PointerUpdateKind.Convert(),
                Position = e.GetPosition(relativeTo).ToScreenPoint(),
                //ModifierKeys = e.KeyModifiers.ToModifierKeys()
            };
        }
        public static Core.OxyMouseEventArgs ToMouseReleasedEventArgs(this PointerReleasedEventArgs e, IInputElement relativeTo)
        {
            return new Core.OxyMouseEventArgs
            {
                Position = e.GetPosition(relativeTo).ToScreenPoint(),
                //ModifierKeys = e.KeyModifiers.ToModifierKeys()
            };
        }

        public static Core.OxyMouseEventArgs ToMouseEventArgs(this PointerEventArgs e, IInputElement relativeTo)
        {
            return new Core.OxyMouseEventArgs
            {
                Position = e.GetPosition(relativeTo).ToScreenPoint(),
                //ModifierKeys = e.KeyModifiers.ToModifierKeys()
            };
        }

        public static Core.OxyMouseButton Convert(this PointerUpdateKind pointerUpdateKind)
        {
            switch (pointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    return Core.OxyMouseButton.Left;
                case PointerUpdateKind.MiddleButtonPressed:
                    return Core.OxyMouseButton.Middle;
                case PointerUpdateKind.RightButtonPressed:
                    return Core.OxyMouseButton.Right;
                default:
                    return Core.OxyMouseButton.None;
            }
        }
    }
}
