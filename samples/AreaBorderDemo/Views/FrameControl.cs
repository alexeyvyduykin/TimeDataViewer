using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TimeDataViewer.ViewModels;
using TimeDataViewer;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using TimeDataViewer.Models;
using TimeDataViewer.Core;
using Avalonia.Controls.Generators;
using AreaBorderDemo.ViewModels;
using Avalonia.Visuals.Platform;

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
