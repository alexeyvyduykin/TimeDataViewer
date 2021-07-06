using System;
using System.Collections;
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
using Avalonia.Threading;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Timeline.ViewModels;
using Timeline;
using Timeline.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Timeline.Models;
using Timeline.Shapes;
using Avalonia.Controls.Generators;
using System.Threading.Tasks;

namespace Timeline.Views
{
    public class CustomItemTemplate : IDataTemplate
    {
        public IControl Build(object param)
        {
            if(param is IInterval ival)
            {
                var shape = ival.SeriesControl.CreateIntervalShape(ival);

                if(shape is IControl)
                {
                    return (IControl)shape;
                }

                //return new IntervalVisual() { DataContext = param };
            }
            else if(param is ISeries)
            {
                return new SeriesVisual() { DataContext = param };
            }

            return new TextBlock() { Text = "Template not find" };
        }

        public bool Match(object data)
        {
            return (data is IInterval) || (data is ISeries);
        }
    }
}
