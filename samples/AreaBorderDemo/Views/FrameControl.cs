using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace AreaBorderDemo.Views
{
    public delegate void SizeChangedEventHandler(double w, double h);

    public class FrameControl : UserControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(UserControl);

        public FrameControl()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

            LayoutUpdated += FrameControl_LayoutUpdated;
        }

        private void FrameControl_LayoutUpdated(object? sender, EventArgs e)
        {
            OnSizeChanged?.Invoke(Bounds.Width, Bounds.Height);
        }

        public event SizeChangedEventHandler OnSizeChanged;

        public override void Render(DrawingContext context)
        {

            //context.DrawRectangle(Background/*_windowBrush*/, _windowPen, Bounds);
        }
    }
}
