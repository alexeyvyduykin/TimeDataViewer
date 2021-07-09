﻿using System;
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

    public class TimelineAxisXControl : ItemsControl
    {                
        private readonly ObservableCollection<Label> _labels;
        private Label? _dynamicLabel;       
        private double _width;     
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
                    //Timeline.AxisX.OnAxisChanged -= OnAxisChanged;
                    Timeline.AxisX.OnBoundChanged -= OnBoundChanged;
                    Timeline.PointerEnter -= OnMapEnter;
                    Timeline.PointerLeave -= OnMapLeave;
                }

                Timeline = timeline;

                //Timeline.AxisX.OnAxisChanged += OnAxisChanged;
                Timeline.AxisX.OnBoundChanged += OnBoundChanged;
                Timeline.PointerEnter += OnMapEnter;
                Timeline.PointerLeave += OnMapLeave;
            }
        }



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

        protected override void ArrangeCore(Rect finalRect)
        {
            base.ArrangeCore(finalRect);

            _width = finalRect.Width;          
        }

        private void OnBoundChanged(object? s, EventArgs e)
        {
            if (s is ITimeAxis axis)
            {
                _labels.Clear();

                double wth = axis.MaxValue - axis.MinValue;
                foreach (var item in axis.Labels)
                {
                    double x = _width * (item.Value - axis.MinValue) / wth;

                    _labels.Add(new Label(x, item.Label));
                }

                //if (axis.DynamicLabel != null && axis.DynamicLabel is AxisLabelPosition dynLab)
                //{                  
                //    double W = _width;
                //    double width = axis.MaxValue - axis.MinValue;

                //    double x = W * (dynLab.Value - axis.MinValue) / width;

                //    _dynamicLabel = new Label(x, dynLab.Label);
                //}

                LeftLabel = axis.MinLabel;

                RightLabel = axis.MaxLabel;

                Items = new ObservableCollection<Label>(_labels);
            }            
        }

        public override void Render(DrawingContext context)
        {
            foreach (var label in _labels)
            {
                //DrawTick(context, label, _tickSize);
                //DrawLabel(context, label);
            }

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
