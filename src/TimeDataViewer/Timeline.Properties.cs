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
using System.Globalization;

namespace TimeDataViewer
{
    public partial class Timeline
    {
        private readonly ObservableCollection<Axis> _axises;
        private readonly ObservableCollection<Series> _series;

        static Timeline()
        {
            PaddingProperty.OverrideDefaultValue<Timeline>(new Thickness(8));
            PaddingProperty.Changed.AddClassHandler<Timeline>(AppearanceChanged);
            CultureProperty.Changed.AddClassHandler<Timeline>(AppearanceChanged);
            DefaultFontProperty.Changed.AddClassHandler<Timeline>(AppearanceChanged);
            DefaultFontSizeProperty.Changed.AddClassHandler<Timeline>(AppearanceChanged);
            //   DefaultColorsProperty.Changed.AddClassHandler<Plot>(AppearanceChanged);
            //  AxisTierDistanceProperty.Changed.AddClassHandler<Plot>(AppearanceChanged);
            // PlotMarginsProperty.Changed.AddClassHandler<Plot>(AppearanceChanged);              
            InvalidateFlagProperty.Changed.AddClassHandler<Timeline>((s, e) => s.InvalidateFlagChanged());
        }

        public Collection<Axis> Axises => _axises;

        [Content]
        public Collection<Series> Series => _series;

        public static readonly StyledProperty<CultureInfo> CultureProperty =
            AvaloniaProperty.Register<Timeline, CultureInfo>(nameof(Culture), null);

        public CultureInfo Culture
        {
            get
            {
                return GetValue(CultureProperty);
            }

            set
            {
                SetValue(CultureProperty, value);
            }
        }

        public static readonly StyledProperty<string> DefaultFontProperty =
            AvaloniaProperty.Register<Timeline, string>(nameof(DefaultFont), "Segoe UI");

        public string DefaultFont
        {
            get
            {
                return GetValue(DefaultFontProperty);
            }

            set
            {
                SetValue(DefaultFontProperty, value);
            }
        }

        public static readonly StyledProperty<double> DefaultFontSizeProperty =
            AvaloniaProperty.Register<Timeline, double>(nameof(DefaultFontSize), 12d);

        public double DefaultFontSize
        {
            get
            {
                return GetValue(DefaultFontSizeProperty);
            }

            set
            {
                SetValue(DefaultFontSizeProperty, value);
            }
        }

        //public static readonly StyledProperty<IList<Color>> DefaultColorsProperty = 
        //    AvaloniaProperty.Register<Plot, IList<Color>>(nameof(DefaultColors), new[]
        //    {
        //        Color.FromRgb(0x4E, 0x9A, 0x06),
        //            Color.FromRgb(0xC8, 0x8D, 0x00),
        //            Color.FromRgb(0xCC, 0x00, 0x00),
        //            Color.FromRgb(0x20, 0x4A, 0x87),
        //            Colors.Red,
        //            Colors.Orange,
        //            Colors.Yellow,
        //            Colors.Green,
        //            Colors.Blue,
        //            Colors.Indigo,
        //            Colors.Violet
        //    });

        //public IList<Color> DefaultColors
        //{
        //    get
        //    {
        //        return GetValue(DefaultColorsProperty);
        //    }

        //    set
        //    {
        //        SetValue(DefaultColorsProperty, value);
        //    }
        //}

        //public static readonly StyledProperty<double> AxisTierDistanceProperty = 
        //    AvaloniaProperty.Register<Plot, double>(nameof(AxisTierDistance), 4d);

        //public double AxisTierDistance
        //{
        //    get
        //    {
        //        return GetValue(AxisTierDistanceProperty);
        //    }

        //    set
        //    {
        //        SetValue(AxisTierDistanceProperty/*LegendTitleFontProperty*/, value);
        //    }
        //}

        //public static readonly StyledProperty<Thickness> PlotMarginsProperty = 
        //    AvaloniaProperty.Register<Plot, Thickness>(nameof(PlotMargins), new Thickness(double.NaN));

        //public Thickness PlotMargins
        //{
        //    get
        //    {
        //        return GetValue(PlotMarginsProperty);
        //    }

        //    set
        //    {
        //        SetValue(PlotMarginsProperty, value);
        //    }
        //}

        public static readonly StyledProperty<int> InvalidateFlagProperty =
            AvaloniaProperty.Register<Timeline, int>(nameof(InvalidateFlag), 0);

        // Gets or sets the refresh flag (an integer value). When the flag is changed, the Plot will be refreshed.
        public int InvalidateFlag
        {
            get
            {
                return GetValue(InvalidateFlagProperty);
            }

            set
            {
                SetValue(InvalidateFlagProperty, value);
            }
        }

        // Invalidates the Plot control/view when the <see cref="InvalidateFlag" /> property is changed.
        private void InvalidateFlagChanged()
        {
            InvalidatePlot();
        }
    }
}
