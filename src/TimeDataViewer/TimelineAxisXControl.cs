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
    public record Label(double X, string Text);

    public partial class TimelineAxisXControl : ItemsControl
    {                
        private readonly ObservableCollection<Label> _labels;
        private Label? _dynamicLabel; 
        private ITimeAxis _axisX;
        //private double _width;     
        private bool _isDynamicLabel;       
        private Canvas _canvas;

        public TimelineAxisXControl()
        {
            _labels = new ObservableCollection<Label>();

            _isDynamicLabel = false;

            _canvas = new Canvas();

            ItemsPanel = new FuncTemplate<IPanel>(() => _canvas);
            
            TimelineProperty.Changed.AddClassHandler<TimelineAxisXControl>((x, e) => x.TimelineChanged(e));          
        }

        public static readonly StyledProperty<TimelineControl> TimelineProperty =   
            AvaloniaProperty.Register<TimelineAxisXControl, TimelineControl>(nameof(Timeline));

        public TimelineControl Timeline
        {
            get => GetValue(TimelineProperty);
            set => SetValue(TimelineProperty, value);
        }
        
        private void TimelineChanged(AvaloniaPropertyChangedEventArgs e)
        { 
            if(e.NewValue is TimelineControl timeline)
            {
                if(Timeline is not null)
                {
                    Timeline.OnSizeChanged -= OnBoundChanged;
                    Timeline.OnZoomChanged -= OnBoundChanged;
                    Timeline.OnDragChanged -= OnBoundChanged;

                    Timeline.PointerEnter -= OnMapEnter;
                    Timeline.PointerLeave -= OnMapLeave;
                }

                Timeline = timeline;
                          
                _axisX = timeline.AxisX;

                Timeline.OnSizeChanged += OnBoundChanged;
                Timeline.OnZoomChanged += OnBoundChanged;
                Timeline.OnDragChanged += OnBoundChanged;

                Timeline.PointerEnter += OnMapEnter;
                Timeline.PointerLeave += OnMapLeave;
            }
        }

        private void OnMapEnter(object? s, EventArgs e)
        {
            _isDynamicLabel = true;

            InvalidateVisual();
        }

        private void OnMapLeave(object? s, EventArgs e)
        {
            _isDynamicLabel = false;

            InvalidateVisual();
        }

        private void OnBoundChanged(object? s, EventArgs e)
        {
            var begin = Timeline.Begin0;
            _axisX.UpdateStaticLabels(begin);

            _labels.Clear();
           
            double wth = _axisX.MaxClientValue - _axisX.MinClientValue;
          //  double wth = _axisX.MaxValue - _axisX.MinValue;
            double width = Timeline.Screen/*Window*/.Width;
            foreach (var item in _axisX.Labels)
            {
                double x = width * (item.Value - _axisX.MinClientValue) / wth;

                _labels.Add(new Label(x, item.Label));
            }

            //if (axis.DynamicLabel != null && axis.DynamicLabel is AxisLabelPosition dynLab)
            //{                  
            //    double W = _width;
            //    double width = axis.MaxValue - axis.MinValue;

            //    double x = W * (dynLab.Value - axis.MinValue) / width;

            //    _dynamicLabel = new Label(x, dynLab.Label);
            //}

            UpdateProperties();

            Items = new ObservableCollection<Label>(_labels);
            
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (_isDynamicLabel == true && _dynamicLabel?.Text is not null)
            {
                //DrawDynamicLabel(context, _dynamicLabel);
            }
        }

        //private void DrawDynamicLabel(DrawingContext context, Label label)
        //{
        //    var px = label.X;
        //    var py = label.Y;
        //    var labelText = label.Text;

        //    var formattedText = new FormattedText()
        //    {
        //        Text = labelText,
        //        Typeface = _typeface,
        //        FontSize = _labelFontSize,
        //        TextAlignment = TextAlignment.Center,
        //        TextWrapping = TextWrapping.NoWrap,
        //        Constraint = Size.Infinity,
        //    };

        //    double w = formattedText.Bounds.Width;
        //    double h = _labelFontSize;

        //    var brush = new SolidColorBrush() { Color = Colors.LightYellow, Opacity = 0.8 };
        //    var offsetX = px - w / 2.0;
        //    var offsetY = 0.0 + _labelMargin;

        //    if (offsetX < 0)
        //    {
        //        offsetX = 0.0;
        //    }

        //    if (offsetX + w > _width)
        //    {
        //        offsetX = _width - w;
        //    }

        //    using (context.PushPreTransform(new TranslateTransform(offsetX, offsetY).Value))              
        //    {
        //        if (_labelRectangleVisible == true)
        //        {
        //            context.DrawRectangle(
        //                brush,
        //                new Pen(Brushes.Red, 2),
        //                new Rect(new Point(-_labelMargin, -_labelMargin),
        //                new Point(w + _labelMargin, h + _labelMargin)));
        //        }

        //        context.DrawText(_foregroundDynamicLabel, new Point(0, 0), formattedText);
        //    }  
        //}
    }
}
