#nullable enable
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
using System.ComponentModel;
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
using TimeDataViewer.Core;
using Avalonia.Controls.Generators;
using System.Threading.Tasks;
using TimeDataViewer.Views;
using Avalonia.Collections;
using Core = TimeDataViewer.Core;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace TimeDataViewer
{
    public delegate void SelectionChangeEventHandler(RectD Selection, bool ZoomToFit);

    public partial class Timeline : TimelineBase
    {    
        private readonly PlotModel _internalModel; 
        private readonly IPlotController _defaultController;
        //private readonly Canvas _canvas;
        //private ObservableCollection<Core.TimelineItem> _markers;
  
        private readonly TranslateTransform _schedulerTranslateTransform;
        private ObservableCollection<Series> _series;
        private IList<Core.TimelineSeries> _seriesViewModels;
        private DateTime _epoch;
        private readonly Popup _popup;
        // center 
        private readonly bool _showCenter = true;
        private readonly Pen _centerCrossPen = new(Brushes.Red, 1);
        // mouse center
        private readonly bool _showMouseCenter = true;
        private readonly Pen _mouseCrossPen = new(Brushes.Blue, 1);

        public Timeline()
        {
            _internalModel = new PlotModel();
            _defaultController = new PlotController();

            _internalModel.OnZoomChanged += ZoomChangedEvent;
            _internalModel.OnDragChanged += DragChangedEvent;

            _series = new ObservableCollection<Series>();
            _schedulerTranslateTransform = new TranslateTransform();

            _popup = new Popup()
            {
                //AllowsTransparency = true,
                //PlacementTarget = this,
                PlacementMode = PlacementMode.Pointer,
                IsOpen = false,
            };

            TopLevelForToolTips?.Children.Add(_popup);

            ClipToBounds = true;
            //       SnapsToDevicePixels = true;

            _series.CollectionChanged += OnSeriesChanged;

            EpochProperty.Changed.AddClassHandler<Timeline>((d, e) => d.EpochChanged(e));
            CurrentTimeProperty.Changed.AddClassHandler<Timeline>((d, e) => d.CurrentTimeChanged(e));
        }

        public override PlotModel ActualModel => _internalModel;

        public override IPlotController ActualController => _defaultController;

        public DateTime Epoch0 => Epoch.Date;

        public RectD ViewportArea => _internalModel.Viewport;

        public RectD ClientViewportArea => _internalModel.ClientViewport;

        public RectI AbsoluteWindow => _internalModel.Window;

        public RectD ScreenWindow => _internalModel.PlotArea;

        public Point2I WindowOffset => _internalModel.WindowOffset;

        public TimeAxis AxisX => _internalModel.AxisX;

        public CategoryAxis AxisY => _internalModel.AxisY;

        public Panel? TopLevelForToolTips
        {
            get
            {
                IVisual root = this.GetVisualRoot();

                while (root is not null)
                {
                    if (root is Panel panel)
                    {
                        return panel;
                    }

                    if (root.VisualChildren.Count != 0)
                    {
                        root = root.VisualChildren[0];
                    }
                    else
                    {
                        return null;
                    }
                }

                return null;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            var MouseScreenPosition = e.GetPosition(this);

            ActualModel.ZoomScreenPosition = new Point2I((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            var mousePosition = ActualModel.FromScreenToLocal((int)MouseScreenPosition.X, (int)MouseScreenPosition.Y);

            AxisX.UpdateDynamicLabelPosition(mousePosition);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            _internalModel.OnZoomChanged -= ZoomChangedEvent;      
        }

        private void OnSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SyncLogicalTree(e);
        }

        private void SynchronizeSeries()
        {
            _internalModel.Series.Clear();
            foreach (var s in Series)
            {
                var model = s.CreateModel();
                model.Parent = _internalModel;
                _internalModel.Series.Add(model);
            }
        }

        private void ZoomChangedEvent(object? sender, EventArgs e)
        {
            //if (_canvas != null)
            {
                _schedulerTranslateTransform.X = _internalModel.WindowOffset.X;
                _schedulerTranslateTransform.Y = _internalModel.WindowOffset.Y;
            }

            //OnZoomChanged?.Invoke(this, EventArgs.Empty);

            ForceUpdateOverlays();
        }

        private void DragChangedEvent(object? sender, EventArgs e)
        {
            //if (_canvas != null)
            {
                _schedulerTranslateTransform.X = _internalModel.WindowOffset.X;
                _schedulerTranslateTransform.Y = _internalModel.WindowOffset.Y;
            }
        }

        private void SyncLogicalTree(NotifyCollectionChangedEventArgs e)
        {
            // In order to get DataContext and binding to work with the series, axes and annotations
            // we add the items to the logical tree
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(this);
                }
                LogicalChildren.AddRange(e.NewItems.OfType<ILogical>());
                VisualChildren.AddRange(e.NewItems.OfType<IVisual>());
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<ISetLogicalParent>())
                {
                    item.SetParent(null);
                }
                foreach (var item in e.OldItems)
                {
                    LogicalChildren.Remove((ILogical)item);
                    VisualChildren.Remove((IVisual)item);
                }
            }
        }

        public void ShowTooltip(Control placementTarget, Control tooltip)
        {
            if (TopLevelForToolTips?.Children.Contains(_popup) == false)
            {
                TopLevelForToolTips?.Children.Add(_popup);
            }

            _popup.PlacementTarget = placementTarget;
            _popup.Child = tooltip;
            _popup.IsOpen = true;
        }

        public void HideTooltip()
        {
            _popup.IsOpen = false;
        }
        
        protected override void UpdateModel(bool updateData = true)
        {
            SynchronizeProperties();
            SynchronizeSeries();
            //SynchronizeAxes();

            base.UpdateModel(updateData);
        }

        private void UpdateVisuals(DrawingContext context)
        {
            //if (_canvas == null)
            //{
            //    return;
            //}

            _internalModel.UpdateSize((int)Bounds.Width, (int)Bounds.Height);

            DrawBackground(context);

            DrawEpoch(context);

            if (_showCenter == true)
            {
                context.DrawLine(_centerCrossPen, new Point((Bounds.Width / 2) - 5, Bounds.Height / 2), new Point((Bounds.Width / 2) + 5, Bounds.Height / 2));
                context.DrawLine(_centerCrossPen, new Point(Bounds.Width / 2, (Bounds.Height / 2) - 5), new Point(Bounds.Width / 2, (Bounds.Height / 2) + 5));
            }

            if (_showMouseCenter == true)
            {
                context.DrawLine(_mouseCrossPen,
                    new Point(_internalModel.ZoomScreenPosition.X - 5, _internalModel.ZoomScreenPosition.Y),
                    new Point(_internalModel.ZoomScreenPosition.X + 5, _internalModel.ZoomScreenPosition.Y));
                context.DrawLine(_mouseCrossPen,
                    new Point(_internalModel.ZoomScreenPosition.X, _internalModel.ZoomScreenPosition.Y - 5),
                    new Point(_internalModel.ZoomScreenPosition.X, _internalModel.ZoomScreenPosition.Y + 5));
            }
            DrawCurrentTime(context);
        }



        public void ForceUpdateOverlays()
        {
            SynchronizeProperties();

            _internalModel.UpdateViewport();

            InvalidateVisual();
        }

        private void EpochChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is DateTime)
            {              
                AxisX.Epoch0 = Epoch0;
            }
        }
        
        private void CurrentTimeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is double)
            {
                var xValue = (Epoch - Epoch0).TotalSeconds + CurrentTime;

                _internalModel.DragToTime(xValue);
            }
        }

        public int MaxZoom => _internalModel.MaxZoom;  

        public int MinZoom => _internalModel.MinZoom;   
  
        private void SynchronizeProperties()
        {
            var m = _internalModel;

            m.Epoch = _epoch;

            //     m.PlotMargins = PlotMargins.ToOxyThickness();
            //     m.Padding = Padding.ToOxyThickness();

            //  m.DefaultColors = DefaultColors.Select(c => c.ToOxyColor()).ToArray();

            //   m.AxisTierDistance = AxisTierDistance;
        }

        public Point2D FromScreenToLocal(int x, int y) => _internalModel.FromScreenToLocal(x, y);        

        public Point2I FromLocalToScreen(Point2D point) => _internalModel.FromLocalToScreen(point);        

        public Point2D FromAbsoluteToLocal(int x, int y) => _internalModel.FromAbsoluteToLocal(x, y);        

        public Point2I FromLocalToAbsolute(Point2D point) => _internalModel.FromLocalToAbsolute(point);
        
        public bool IsTestBrush { get; set; } = false;

        public override void Render(DrawingContext context)
        {
            //UpdateVisuals(context);

            base.Render(context);                     
        }

        protected override void MyRenderSeries(DrawCanvas drawCanvas)
        {
            foreach (var item in Series.Where(s => s.IsVisible))
            {
                if (item is TimelineSeries timelineSeries)
                {
                    var series = (Core.TimelineSeries)timelineSeries.InternalSeries;

                    drawCanvas.RenderTransform = _schedulerTranslateTransform;

                    drawCanvas.CreateRenderIntervals(series.MyClippingRect,
                        series.MyRectList, timelineSeries.FillBrush, timelineSeries.StrokeBrush);
                }
            }
        }

        private void DrawEpoch(DrawingContext context)
        {
            var d0 = (Epoch - Epoch0).TotalSeconds;
            var p = _internalModel.FromLocalToAbsolute(d0, 0.0);    
            Pen pen = new Pen(Brushes.Yellow, 2.0);
            context.DrawLine(pen, new Point(p.X + WindowOffset.X, 0.0), new Point(p.X + WindowOffset.X, _internalModel.Window.Height));
        }

        private void DrawCurrentTime(DrawingContext context)
        {            
            var d0 = (Epoch - Epoch0).TotalSeconds;
            var p = _internalModel.FromLocalToAbsolute(d0 + CurrentTime, 0.0);
            Pen pen = new Pen(Brushes.Red, 2.0);
            context.DrawLine(pen, new Point(p.X + WindowOffset.X, 0.0), new Point(p.X + WindowOffset.X, _internalModel.Window.Height));
        }       
    }
}
