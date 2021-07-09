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
using Timeline.ViewModels;
using Timeline.Core;
using Timeline.Models;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using Timeline.Shapes;
using System.Globalization;


namespace Timeline
{
    public partial class TimelineAxisXControl
    {
        public static readonly StyledProperty<string> LeftLabelProperty =    
            AvaloniaProperty.Register<TimelineAxisXControl, string>(nameof(LeftLabel));

        public string LeftLabel
        {
            get => GetValue(LeftLabelProperty);
            set => SetValue(LeftLabelProperty, value);
        }

        public static readonly StyledProperty<string> RightLabelProperty =
            AvaloniaProperty.Register<TimelineAxisXControl, string>(nameof(RightLabel));

        public string RightLabel
        {
            get => GetValue(RightLabelProperty);
            set => SetValue(RightLabelProperty, value);
        }

        public static readonly StyledProperty<double> MinValueProperty =    
            AvaloniaProperty.Register<TimelineAxisXControl, double>(nameof(MinValue));

        public double MinValue
        {
            get { return GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly StyledProperty<double> MaxValueProperty =   
            AvaloniaProperty.Register<TimelineAxisXControl, double>(nameof(MaxValue));

        public double MaxValue
        {
            get { return GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly StyledProperty<double> MinClientValueProperty =    
            AvaloniaProperty.Register<TimelineAxisXControl, double>(nameof(MinClientValue));

        public double MinClientValue
        {
            get { return GetValue(MinClientValueProperty); }
            set { SetValue(MinClientValueProperty, value); }
        }

        public static readonly StyledProperty<double> MaxClientValueProperty =   
            AvaloniaProperty.Register<TimelineAxisXControl, double>(nameof(MaxClientValue));

        public double MaxClientValue
        {
            get { return GetValue(MaxClientValueProperty); }
            set { SetValue(MaxClientValueProperty, value); }
        }

        public static readonly StyledProperty<int> MinPixelProperty =    
            AvaloniaProperty.Register<TimelineAxisXControl, int>(nameof(MinPixel));

        public int MinPixel
        {
            get { return GetValue(MinPixelProperty); }
            set { SetValue(MinPixelProperty, value); }
        }

        public static readonly StyledProperty<int> MaxPixelProperty =    
            AvaloniaProperty.Register<TimelineAxisXControl, int>(nameof(MaxPixel));

        public int MaxPixel
        {
            get { return GetValue(MaxPixelProperty); }
            set { SetValue(MaxPixelProperty, value); }
        }

        private void UpdateProperties()
        {
            LeftLabel = _axisX.MinLabel;
            RightLabel = _axisX.MaxLabel;
            MinValue = _axisX.MinValue;
            MaxValue = _axisX.MaxValue;
            MinClientValue = _axisX.MinClientValue;
            MaxClientValue = _axisX.MaxClientValue;
            MinPixel = _axisX.MinPixel;
            MaxPixel = _axisX.MaxPixel;
        }
    }
}
