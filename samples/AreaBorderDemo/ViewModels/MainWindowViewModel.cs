using System;
using System.Collections.Generic;
using System.Text;
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
using Timeline.Core;
using Avalonia.Controls.Generators;

namespace AreaBorderDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Area _area;

        public MainWindowViewModel()
        {

            CoreFactory factory = new CoreFactory();

            _area = factory.CreateArea();

            _area.UpdateSize(400, 160);
        }

        public Area Area => _area;

    }
}
