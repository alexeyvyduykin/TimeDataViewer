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
    public partial class Timeline : TimelineBase
    {
        private readonly PlotModel _internalModel;
        private readonly IPlotController _defaultController;

        public Timeline()
        {
            _series = new ObservableCollection<Series>();
            _axises = new ObservableCollection<Axis>();

            _series.CollectionChanged += OnSeriesChanged;
            _axises.CollectionChanged += OnAxesChanged;

            _defaultController = new PlotController();
            _internalModel = new PlotModel();
            ((IPlotModel)_internalModel).AttachPlotView(this);
        }

        protected override void MyRenderAxisX(Canvas canvasAxis, Canvas canvasPlot)
        {
            foreach (var item in Axises)
            {
                if (item.Position == Core.AxisPosition.Bottom)
                {
                    item.MyRender(canvasAxis, canvasPlot);
                }
            }
        }

        protected override void MyRenderAxisY(Canvas canvasAxis, Canvas canvasPlot)
        {
            foreach (var item in Axises)
            {
                if (item.Position == Core.AxisPosition.Left)
                {
                    item.MyRender(canvasAxis, canvasPlot);
                }
            }
        }

        protected override void MyRenderSeries(Canvas canvasPlot, DrawCanvas drawCanvas)
        {
            drawCanvas.RenderSeries(Series.Where(s => s.IsVisible).ToList());

            //foreach (var item in Series.Where(s => s.IsVisible))
            //{                
                //if (item is TimelineSeries series)
                //{
                //    var oxySeries = (Core.TimelineSeries)series.InternalSeries;

                //    drawCanvas.RenderIntervals(oxySeries.MyClippingRect,
                //        oxySeries.MyRectList, series.FillBrush, series.StrokeBrush);
                //}
                //else
                //{
                //    //rc.SetToolTip(item.ToolTip);
                //    item.MyRender(canvasPlot);
                //}
            //}

            //rc.SetToolTip(null);
        }

        public override PlotModel ActualModel => _internalModel;

        public override IPlotController ActualController => _defaultController;

        // Updates the model. If Model==<c>null</c>, an internal model will be created.
        // The ActualModel.Update will be called (updates all series data).
        protected override void UpdateModel(bool updateData = true)
        {
            SynchronizeProperties();
            SynchronizeSeries();
            SynchronizeAxes();

            base.UpdateModel(updateData);
        }

        // Called when the visual appearance is changed.     
        protected void OnAppearanceChanged()
        {
            InvalidatePlot(false);
        }

        // Called when the visual appearance is changed.
        private static void AppearanceChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((Timeline)d).OnAppearanceChanged();
        }

        private void OnAnnotationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncLogicalTree(e);
        }

        private void OnAxesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncLogicalTree(e);
        }

        private void OnSeriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncLogicalTree(e);
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

        // Synchronize properties in the internal Plot model
        private void SynchronizeProperties()
        {
            var m = _internalModel;

            //     m.PlotMargins = PlotMargins.ToOxyThickness();
            //     m.Padding = Padding.ToOxyThickness();

            //  m.DefaultColors = DefaultColors.Select(c => c.ToOxyColor()).ToArray();

            //   m.AxisTierDistance = AxisTierDistance;
        }

        // Synchronizes the axes in the internal model.      
        private void SynchronizeAxes()
        {
            _internalModel.Axises.Clear();
            foreach (var a in Axises)
            {
                _internalModel.Axises.Add(a.CreateModel());
            }
        }

        // Synchronizes the series in the internal model.       
        private void SynchronizeSeries()
        {
            _internalModel.Series.Clear();
            foreach (var s in Series)
            {
                _internalModel.Series.Add(s.CreateModel());
            }
        }
    }
}
