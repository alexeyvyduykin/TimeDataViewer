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
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AreaBorderDemo.Views
{
    public partial class ContentPresenter : ItemsControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ItemsControl);

        private Area _area;
        private TranslateTransform _transform;
        private FrameControl _rect; 
        private readonly Canvas _canvas;
        public ContentPresenter()
        {
            InitializeComponent();

            _transform = new TranslateTransform();

            _canvas = new Canvas()
            {
               // RenderTransform = _transform
            };

           // ItemsPanel = new FuncTemplate<IPanel>(() => _canvas);

           

            DataContextChanged += ContentPresenter_DataContextChanged;
            
            LayoutUpdated += ContentPresenter_LayoutUpdated;
        }

        private void ContentPresenter_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                _area = viewModel.Area;

                _rect = CreateWindow();
         
                Items = new ObservableCollection<object>() { _rect };
            }
        }

        private FrameControl CreateWindow()
        {
            var rect = new FrameControl();

            rect.BorderBrush = _windowPen.Brush;
            rect.BorderThickness = new Thickness(_windowPen.Thickness);
            rect.Background = _windowBrush;

            //((ISetLogicalParent)_rect).SetParent(this);

            //LogicalChildren.AddRange(new List<ILogical>() { _rect });
            //VisualChildren.AddRange(new List<IVisual>() { _rect });

            rect.PointerPressed += SchedulerControl_PointerPressed;
            rect.PointerReleased += SchedulerControl_PointerReleased;
            rect.PointerMoved += SchedulerControl_PointerMoved;

            return rect;
        }

        private Cursor? _cursorBefore;
        private int _onMouseUpTimestamp = 0;
        private Point2D _mousePosition = new();
        private bool _isDragging = false;

        public bool IgnoreMarkerOnMouseWheel { get; set; } = true;

        public Point2D MousePosition
        {
            get => _mousePosition;
            protected set
            {
                _mousePosition = value;
                //OnMousePositionChanged?.Invoke(_mousePosition);
            }
        }

        private void SchedulerControl_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                var p = e.GetPosition(this);

                _area.MouseDown = new Point2I((int)p.X, (int)p.Y);

                base.InvalidateVisual();
            }
        }

        private void SchedulerControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_area.IsDragging == true)
            {
                if (_isDragging == true)
                {
                    _onMouseUpTimestamp = (int)e.Timestamp & int.MaxValue;
                    _isDragging = false;

                    if (_cursorBefore == null)
                    {
                        _cursorBefore = new(StandardCursorType.Arrow);
                    }

                    base.Cursor = _cursorBefore;
                    e.Pointer.Capture(null);
                }
                _area.EndDrag();
            }
            else
            {
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
                {
                    _area.MouseDown = default;//Point2I.Empty;
                }
                    
                InvalidateVisual();                
            }
        }

        private void SchedulerControl_PointerMoved(object? sender, PointerEventArgs e)
        {
            var MouseScreenPosition = e.GetPosition(this);
                
            _area.ZoomScreenPosition = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            MousePosition = _area.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            // wpf generates to many events if mouse is over some visual and OnMouseUp       
            if (((int)e.Timestamp & int.MaxValue) - _onMouseUpTimestamp < 55)
            {
                return;
            }

            if (_area.IsDragging == false && _area.MouseDown.IsEmpty == false)
            {
                // cursor has moved beyond drag tolerance
                if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
                {
                    if (Math.Abs(MouseScreenPosition.X - _area.MouseDown.X) * 2 >= 2/*SystemParameters.MinimumHorizontalDragDistance*/ ||
                        Math.Abs(MouseScreenPosition.Y - _area.MouseDown.Y) * 2 >= 2/*SystemParameters.MinimumVerticalDragDistance*/)
                    {
                        _area.BeginDrag(_area.MouseDown);
                    }
                }

            }

            if (_area.IsDragging == true)
            {
                if (_isDragging == false)
                {
                    _isDragging = true;
                    _cursorBefore = base.Cursor;
                    Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    e.Pointer.Capture(this);                    
                }

                _area.MouseCurrent = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

                _area.Drag(_area.MouseCurrent);

                base.InvalidateVisual();
            }
        }

        private void ContentPresenter_LayoutUpdated(object? sender, EventArgs e)
        {
            _transform = new TranslateTransform(Bounds.Width / 2.0, Bounds.Height / 2.0);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void Render(DrawingContext context)
        {
            

            using (context.PushPreTransform(_transform.Value))
            {
                DrawFrame(context);

                DrawWindow(context);

                //base.Render(context);
            }
            
        }

        private Pen _blackPen = new Pen() { Brush = new SolidColorBrush() { Color = Colors.Black }, Thickness = 5.0 };
        private IBrush _windowBrush = new SolidColorBrush() { Color = Colors.Blue, Opacity = 0.1 };
        private Pen _windowPen = new Pen() { Brush = new SolidColorBrush() { Color = Colors.Blue }, Thickness = 1.0 };

        private void DrawFrame(DrawingContext context)
        {
            context.DrawLine(_blackPen, new Point(), new Point(50, 0));
            context.DrawLine(_blackPen, new Point(), new Point(0, 50));
        }

        private void DrawWindow(DrawingContext context)
        {       
            _rect.Width = _area.WindowZoom.Width;
            _rect.Height = _area.WindowZoom.Height;
            
            _rect.RenderTransform = _transform;
            //_rect.Margin = new Thickness(Bounds.Width / 2.0, Bounds.Height / 2.0);

            //_rect.Render(context);

            //context.DrawRectangle(_windowBrush, _windowPen, rect.ToRect());           
        }
    }

    internal static class Extensions 
    {    
        public static Rect ToRect(this RectI rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
