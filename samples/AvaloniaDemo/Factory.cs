using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AvaloniaDemo.Markers;
using AvaloniaDemo.Models;
using TimeDataViewer;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo
{
    public class Factory
    {
        public SchedulerString CreateString()
        {
            var marker = new SchedulerString("String");

            marker.Shape = new StringVisual(marker);

            return marker;
        }

        public SchedulerInterval CreateInterval(Interval ival, SchedulerString parent, BaseInterval template)
        {
            var marker = new SchedulerInterval(ival.Left, ival.Right);
            
            marker.String = parent;

            marker.Shape = template.Clone(marker);

            parent.Intervals.Add(marker);

            return marker;
        }
    }
}
